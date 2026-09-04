using System.Security.Claims;
using AccessibleSchoolReports.Application.Knowledge;
using AccessibleSchoolReports.Application.Security;
using AccessibleSchoolReports.Domain.Entities;
using AccessibleSchoolReports.Domain.Knowledge;
using AccessibleSchoolReports.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace AccessibleSchoolReports.Infrastructure.Knowledge;

public sealed class KnowledgeRetrievalService : IKnowledgeRetrievalService
{
    private const int MaxTopK = 50;

    private readonly IDbContextFactory<SchoolReportsDbContext> _dbFactory;
    private readonly IEmbeddingService _embeddings;
    private readonly IReportAuthorizationService _authorization;

    public KnowledgeRetrievalService(
        IDbContextFactory<SchoolReportsDbContext> dbFactory,
        IEmbeddingService embeddings,
        IReportAuthorizationService authorization)
    {
        _dbFactory = dbFactory;
        _embeddings = embeddings;
        _authorization = authorization;
    }

    public async Task<KnowledgeRetrievalResult> RetrieveAsync(
        ClaimsPrincipal user,
        string question,
        KnowledgeRetrievalOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(user);
        var started = DateTimeOffset.UtcNow;
        var settings = Normalize(options);

        if (!CanRetrieve(user)
            || string.IsNullOrWhiteSpace(question)
            || question.Trim().Length > KnowledgeRetrievalOptions.MaxQuestionLength)
        {
            return Empty(started);
        }

        cancellationToken.ThrowIfCancellationRequested();
        var authenticated = user.Identity?.IsAuthenticated == true;
        var isAdmin = authenticated && user.IsInRole(AppRoles.Admin);
        int? scopedReportId = null;
        if (settings.ReportId is int requestedReportId)
        {
            if (requestedReportId <= 0
                || !await _authorization.CanViewReportAsync(user, requestedReportId, cancellationToken))
            {
                return Empty(started);
            }

            scopedReportId = requestedReportId;
        }

        var schools = await _authorization.GetAccessibleSchoolIdsAsync(user, cancellationToken);

        await using var db = await _dbFactory.CreateDbContextAsync(cancellationToken);
        var documentIds = await AuthorizedDocumentIdsAsync(
            db,
            user,
            authenticated,
            isAdmin,
            schools,
            scopedReportId,
            cancellationToken);
        if (documentIds.Count == 0)
        {
            return Empty(started);
        }

        var modelKey = _embeddings.Model.Key;
        var candidates = await db.KnowledgeChunks
            .AsNoTracking()
            .Include(chunk => chunk.KnowledgeDocument)
            .Where(chunk => documentIds.Contains(chunk.KnowledgeDocumentId)
                && chunk.Embedding != null
                && chunk.Embedding.Length > 0
                && chunk.EmbeddingModel == modelKey)
            .ToListAsync(cancellationToken);

        if (candidates.Count == 0)
        {
            return new KnowledgeRetrievalResult
            {
                Hits = [],
                AuthorizedCandidateCount = 0,
                Duration = DateTimeOffset.UtcNow - started,
            };
        }

        var query = await _embeddings.EmbedQueryAsync(question, cancellationToken);
        var hits = candidates
            .Select(chunk => Score(chunk, query.Values))
            .Where(hit => hit.Similarity >= settings.MinimumSimilarity)
            .OrderByDescending(hit => hit.Similarity)
            .ThenBy(hit => hit.ChunkId)
            .Take(settings.TopK)
            .ToList();

        return new KnowledgeRetrievalResult
        {
            Hits = hits,
            AuthorizedCandidateCount = candidates.Count,
            Duration = DateTimeOffset.UtcNow - started,
        };
    }

    private async Task<List<int>> AuthorizedDocumentIdsAsync(
        SchoolReportsDbContext db,
        ClaimsPrincipal user,
        bool authenticated,
        bool isAdmin,
        IReadOnlySet<int> schools,
        int? scopedReportId,
        CancellationToken cancellationToken)
    {
        var documents = db.KnowledgeDocuments
            .AsNoTracking()
            .WhereAccessible(authenticated, isAdmin, schools);
        if (scopedReportId is int requiredReportId)
        {
            documents = documents.Where(document => document.ReportId == requiredReportId);
        }

        var rows = await documents
            .Select(document => new
            {
                document.Id,
                document.AuthorizationScope,
                document.ReportId,
            })
            .ToListAsync(cancellationToken);

        var ids = new List<int>(rows.Count);
        foreach (var document in rows)
        {
            cancellationToken.ThrowIfCancellationRequested();
            if (document.AuthorizationScope == KnowledgeAuthorizationScope.Report
                && document.ReportId is int reportId
                && !await _authorization.CanViewReportAsync(user, reportId, cancellationToken))
            {
                continue;
            }

            ids.Add(document.Id);
        }

        return ids;
    }

    private static KnowledgeRetrievalHit Score(KnowledgeChunk chunk, float[] query)
    {
        var document = chunk.KnowledgeDocument;
        var values = chunk.Embedding is { Length: > 0 }
            ? EmbeddingVectorConvert.ToFloats(chunk.Embedding)
            : [];
        return new KnowledgeRetrievalHit
        {
            ChunkId = chunk.Id,
            DocumentId = chunk.KnowledgeDocumentId,
            Content = chunk.Content,
            RuleId = chunk.RuleId,
            SchoolId = document.SchoolId,
            SchoolCode = document.SchoolCode,
            ReportId = document.ReportId,
            ReportYear = document.ReportYear,
            SourceLocation = chunk.SourceLocation,
            SourceIdentifier = document.SourceIdentifier,
            FileName = document.FileName,
            DocumentType = document.DocumentType,
            AuthorizationScope = document.AuthorizationScope,
            Similarity = EmbeddingSimilarity.Cosine(query, values),
        };
    }

    private static bool CanRetrieve(ClaimsPrincipal user)
    {
        var authenticated = user.Identity?.IsAuthenticated == true;
        return KnowledgeAccess.HasRetrievalAccess(
            authenticated,
            authenticated && user.IsInRole(AppRoles.Admin),
            authenticated && user.IsInRole(AppRoles.ReportUser),
            authenticated && user.IsInRole(AppRoles.Viewer));
    }

    private static KnowledgeRetrievalOptions Normalize(KnowledgeRetrievalOptions? options)
    {
        var source = options ?? new KnowledgeRetrievalOptions();
        var topK = Math.Clamp(source.TopK, 1, MaxTopK);
        var threshold = float.IsFinite(source.MinimumSimilarity)
            ? Math.Clamp(source.MinimumSimilarity, 0f, 1f)
            : KnowledgeRetrievalOptions.DefaultMinimumSimilarity;
        return new KnowledgeRetrievalOptions
        {
            TopK = topK,
            MinimumSimilarity = threshold,
            ReportId = source.ReportId is > 0 ? source.ReportId : null,
        };
    }

    private static KnowledgeRetrievalResult Empty(DateTimeOffset started) =>
        new()
        {
            Hits = [],
            AuthorizedCandidateCount = 0,
            Duration = DateTimeOffset.UtcNow - started,
        };
}
