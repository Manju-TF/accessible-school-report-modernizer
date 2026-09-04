using System.Security.Claims;
using AccessibleSchoolReports.Application.Knowledge;
using AccessibleSchoolReports.Application.Security;
using AccessibleSchoolReports.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace AccessibleSchoolReports.UnitTests.Embeddings;

internal sealed class FakeEmbeddingService : IEmbeddingService
{
    private readonly IDbContextFactory<SchoolReportsDbContext> _dbFactory;
    private readonly IReportAuthorizationService _authorization;
    private readonly EmbeddingOptions _options;

    public FakeEmbeddingService(
        IDbContextFactory<SchoolReportsDbContext> dbFactory,
        IReportAuthorizationService authorization,
        EmbeddingOptions options)
    {
        _dbFactory = dbFactory;
        _authorization = authorization;
        _options = options;
    }

    public int EmbedCalls { get; private set; }

    public bool UsedNetwork { get; private set; }

    public List<int> RequestedChunkIds { get; } = [];

    public HashSet<int> FailChunkIds { get; } = [];

    public float[]? NextQueryVector { get; set; }

    public EmbeddingModelInfo Model => new()
    {
        Provider = "Fake",
        Model = _options.Model,
        Dimensions = _options.Dimensions,
    };

    public async Task<EmbeddingBatchResult> EmbedPermittedChunksAsync(
        ClaimsPrincipal user,
        IReadOnlyList<int> chunkIds,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        EmbedCalls++;
        RequestedChunkIds.AddRange(chunkIds);
        await using var db = await _dbFactory.CreateDbContextAsync(cancellationToken);
        var uniqueIds = chunkIds.Distinct().ToArray();
        var chunks = await db.KnowledgeChunks
            .Include(chunk => chunk.KnowledgeDocument)
            .Where(chunk => uniqueIds.Contains(chunk.Id))
            .ToListAsync(cancellationToken);
        var schools = await _authorization.GetAccessibleSchoolIdsAsync(user, cancellationToken);
        var permitted = EmbeddingAccess.FilterPermitted(chunks, user, schools);
        var skipped = uniqueIds.Except(permitted.Select(chunk => chunk.Id)).ToArray();

        var embedded = new List<EmbeddedChunk>();
        foreach (var chunk in permitted)
        {
            if (FailChunkIds.Contains(chunk.Id))
            {
                throw new InvalidOperationException($"Fake embedding failed for chunk {chunk.Id}.");
            }

            var vector = VectorFor(chunk.Id);
            chunk.Embedding = EmbeddingVectorConvert.ToBytes(vector);
            chunk.EmbeddingModel = $"Fake/{_options.Model}";
            embedded.Add(new EmbeddedChunk { ChunkId = chunk.Id, Values = vector });
        }

        await db.SaveChangesAsync(cancellationToken);
        return new EmbeddingBatchResult
        {
            Provider = "Fake",
            Model = $"Fake/{_options.Model}",
            Dimensions = _options.Dimensions,
            Embedded = embedded,
            SkippedUnauthorizedChunkIds = skipped,
        };
    }

    public Task<EmbeddingVector> EmbedQueryAsync(string text, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        EmbedCalls++;
        var values = NextQueryVector is { Length: > 0 }
            ? NextQueryVector
            : VectorFor(text.Length);
        return Task.FromResult(new EmbeddingVector
        {
            Values = values,
            Provider = "Fake",
            Model = _options.Model,
            Dimensions = _options.Dimensions,
        });
    }

    private float[] VectorFor(int seed) =>
        Enumerable.Range(0, _options.Dimensions)
            .Select(index => (seed + index) * 0.001f)
            .ToArray();
}
