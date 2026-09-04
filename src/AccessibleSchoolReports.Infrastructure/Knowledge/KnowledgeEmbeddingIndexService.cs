using System.Security.Claims;
using AccessibleSchoolReports.Application.Knowledge;
using AccessibleSchoolReports.Application.Security;
using AccessibleSchoolReports.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace AccessibleSchoolReports.Infrastructure.Knowledge;

public sealed class KnowledgeEmbeddingIndexService : IKnowledgeEmbeddingIndexService
{
    private readonly IDbContextFactory<SchoolReportsDbContext> _dbFactory;
    private readonly IEmbeddingService _embeddings;
    private readonly IReportAuthorizationService _authorization;

    public KnowledgeEmbeddingIndexService(
        IDbContextFactory<SchoolReportsDbContext> dbFactory,
        IEmbeddingService embeddings,
        IReportAuthorizationService authorization)
    {
        _dbFactory = dbFactory;
        _embeddings = embeddings;
        _authorization = authorization;
    }

    public async Task<KnowledgeIndexResult> IndexPendingEmbeddingsAsync(
        ClaimsPrincipal user,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(user);
        var started = DateTimeOffset.UtcNow;
        var modelKey = _embeddings.Model.Key;

        await using var db = await _dbFactory.CreateDbContextAsync(cancellationToken);
        var chunks = await db.KnowledgeChunks
            .AsNoTracking()
            .Include(chunk => chunk.KnowledgeDocument)
            .OrderBy(chunk => chunk.Id)
            .ToListAsync(cancellationToken);
        var schools = await _authorization.GetAccessibleSchoolIdsAsync(user, cancellationToken);

        var indexedDocuments = new HashSet<int>();
        var chunksIndexed = 0;
        var chunksSkipped = 0;
        var failures = new List<KnowledgeIndexFailure>();

        foreach (var chunk in chunks)
        {
            cancellationToken.ThrowIfCancellationRequested();

            if (chunk.KnowledgeDocument is null
                || !EmbeddingAccess.CanSendToExternalProvider(chunk.KnowledgeDocument, user, schools))
            {
                chunksSkipped++;
                continue;
            }

            if (KnowledgeEmbeddingState.HasCurrentEmbedding(chunk, modelKey))
            {
                chunksSkipped++;
                continue;
            }

            try
            {
                var embedded = await _embeddings.EmbedPermittedChunksAsync(
                    user,
                    [chunk.Id],
                    cancellationToken);
                if (embedded.Embedded.Any(item => item.ChunkId == chunk.Id))
                {
                    chunksIndexed++;
                    indexedDocuments.Add(chunk.KnowledgeDocumentId);
                }
                else
                {
                    chunksSkipped++;
                }
            }
            catch (OperationCanceledException)
            {
                throw;
            }
            catch (Exception exception)
            {
                failures.Add(new KnowledgeIndexFailure
                {
                    ChunkId = chunk.Id,
                    DocumentId = chunk.KnowledgeDocumentId,
                    Message = Truncate(exception.Message),
                });
            }
        }

        return new KnowledgeIndexResult
        {
            DocumentsIndexed = indexedDocuments.Count,
            ChunksIndexed = chunksIndexed,
            ChunksSkipped = chunksSkipped,
            Failures = failures.Count,
            Duration = DateTimeOffset.UtcNow - started,
            FailureDetails = failures,
        };
    }

    private static string Truncate(string message) =>
        message.Length <= 200 ? message : message[..200];
}
