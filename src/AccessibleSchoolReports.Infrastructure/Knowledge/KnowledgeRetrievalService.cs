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
        var reportScoped = settings.ReportId is int;
        var prefersPrintedReports = KnowledgeQuestionIntent.PrefersPrintedReportEvidence(question);
        var minimumSimilarity = reportScoped && prefersPrintedReports
            ? 0f
            : settings.MinimumSimilarity;
        var ranked = candidates
            .Select(chunk => Score(chunk, query.Values))
            .Where(hit => hit.Similarity >= minimumSimilarity)
            .OrderByDescending(hit => !reportScoped && prefersPrintedReports && hit.DocumentType == KnowledgeDocumentType.GeneratedReport)
            .ThenByDescending(hit => hit.Similarity)
            .ThenBy(hit => hit.ChunkId)
            .ToList();
        var topK = settings.TopK;
        if (settings.TopK == KnowledgeRetrievalOptions.DefaultTopK)
        {
            if (reportScoped)
            {
                topK = KnowledgeRetrievalOptions.DefaultReportTopK;
            }
            else if (prefersPrintedReports)
            {
                topK = KnowledgeRetrievalOptions.DefaultGlobalTopK;
            }
        }

        var hits = !reportScoped && prefersPrintedReports
            ? Diversify(ranked, topK, KnowledgeRetrievalOptions.MaxChunksPerReport)
            : ranked.Take(topK).ToList();
        var printedMetricHits = KnowledgeQuestionIntent.AsksForPrintedArithmetic(question)
            ? candidates
                .Where(chunk => chunk.KnowledgeDocument.DocumentType == KnowledgeDocumentType.GeneratedReport)
                .Select(chunk => Score(chunk, query.Values))
                .ToList()
            : [];

        return new KnowledgeRetrievalResult
        {
            Hits = hits,
            PrintedMetricHits = printedMetricHits,
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

    internal static IReadOnlyList<KnowledgeRetrievalHit> Diversify(
        IReadOnlyList<KnowledgeRetrievalHit> ranked,
        int topK,
        int maxPerDocument = 3)
    {
        if (ranked.Count == 0 || topK <= 0)
        {
            return [];
        }

        var perDocumentLimit = Math.Max(1, maxPerDocument);
        var selected = new List<KnowledgeRetrievalHit>(Math.Min(topK, ranked.Count));
        var seenDocuments = new HashSet<int>();
        foreach (var hit in ranked)
        {
            if (selected.Count >= topK)
            {
                break;
            }

            if (seenDocuments.Add(hit.DocumentId))
            {
                selected.Add(hit);
            }
        }

        var perDocument = selected
            .GroupBy(hit => hit.DocumentId)
            .ToDictionary(group => group.Key, group => group.Count());
        foreach (var hit in ranked)
        {
            if (selected.Count >= topK)
            {
                break;
            }

            if (selected.Contains(hit))
            {
                continue;
            }

            perDocument.TryGetValue(hit.DocumentId, out var count);
            if (count >= perDocumentLimit)
            {
                continue;
            }

            selected.Add(hit);
            perDocument[hit.DocumentId] = count + 1;
        }

        return selected
            .OrderByDescending(hit => hit.Similarity)
            .ThenBy(hit => hit.ChunkId)
            .ToList();
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
