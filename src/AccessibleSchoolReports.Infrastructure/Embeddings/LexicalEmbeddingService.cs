using System.Security.Claims;
using AccessibleSchoolReports.Application.Knowledge;
using AccessibleSchoolReports.Application.Security;
using AccessibleSchoolReports.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace AccessibleSchoolReports.Infrastructure.Embeddings;

public sealed class LexicalEmbeddingService : IEmbeddingService
{
    private readonly IDbContextFactory<SchoolReportsDbContext> _dbFactory;
    private readonly IReportAuthorizationService _authorization;
    private readonly EmbeddingOptions _options;

    public LexicalEmbeddingService(
        IDbContextFactory<SchoolReportsDbContext> dbFactory,
        IReportAuthorizationService authorization,
        IOptions<EmbeddingOptions> options)
    {
        _dbFactory = dbFactory;
        _authorization = authorization;
        _options = options.Value;
    }

    public EmbeddingModelInfo Model => new()
    {
        Provider = string.IsNullOrWhiteSpace(_options.Provider) ? "Lexical" : _options.Provider,
        Model = string.IsNullOrWhiteSpace(_options.Model) ? "hashed-bow" : _options.Model,
        Dimensions = _options.Dimensions > 0 ? _options.Dimensions : HashedLexicalVector.DefaultDimensions,
    };

    public async Task<EmbeddingBatchResult> EmbedPermittedChunksAsync(
        ClaimsPrincipal user,
        IReadOnlyList<int> chunkIds,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(user);
        ArgumentNullException.ThrowIfNull(chunkIds);
        cancellationToken.ThrowIfCancellationRequested();

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
            var vector = HashedLexicalVector.Embed(chunk.Content, Model.Dimensions);
            chunk.Embedding = EmbeddingVectorConvert.ToBytes(vector);
            chunk.EmbeddingModel = Model.Key;
            embedded.Add(new EmbeddedChunk { ChunkId = chunk.Id, Values = vector });
        }

        await db.SaveChangesAsync(cancellationToken);
        return new EmbeddingBatchResult
        {
            Provider = Model.Provider,
            Model = Model.Key,
            Dimensions = Model.Dimensions,
            Embedded = embedded,
            SkippedUnauthorizedChunkIds = skipped,
        };
    }

    public Task<EmbeddingVector> EmbedQueryAsync(string text, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        return Task.FromResult(new EmbeddingVector
        {
            Values = HashedLexicalVector.Embed(text, Model.Dimensions),
            Provider = Model.Provider,
            Model = Model.Model,
            Dimensions = Model.Dimensions,
        });
    }
}
