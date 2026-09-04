using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using AccessibleSchoolReports.Application.Knowledge;
using AccessibleSchoolReports.Application.Security;
using AccessibleSchoolReports.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace AccessibleSchoolReports.UnitTests.Knowledge;

/// <summary>
/// Deterministic hashed bag-of-words embeddings for RAG evaluation.
/// Does not call a network provider. Authorization still uses <see cref="EmbeddingAccess"/>.
/// </summary>
internal sealed class LexicalEmbeddingService : IEmbeddingService
{
    public const int DefaultDimensions = 256;

    private static readonly Regex TokenPattern = new(
        @"[A-Za-z0-9]+(?:-[A-Za-z0-9]+)*",
        RegexOptions.CultureInvariant | RegexOptions.Compiled);

    private readonly IDbContextFactory<SchoolReportsDbContext> _dbFactory;
    private readonly IReportAuthorizationService _authorization;
    private readonly EmbeddingOptions _options;

    public LexicalEmbeddingService(
        IDbContextFactory<SchoolReportsDbContext> dbFactory,
        IReportAuthorizationService authorization,
        EmbeddingOptions options)
    {
        _dbFactory = dbFactory;
        _authorization = authorization;
        _options = options;
    }

    public int EmbedCalls { get; private set; }

    public bool UsedNetwork => false;

    public EmbeddingModelInfo Model => new()
    {
        Provider = "Lexical",
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
            var vector = EmbedText(chunk.Content, _options.Dimensions);
            chunk.Embedding = EmbeddingVectorConvert.ToBytes(vector);
            chunk.EmbeddingModel = Model.Key;
            embedded.Add(new EmbeddedChunk { ChunkId = chunk.Id, Values = vector });
        }

        await db.SaveChangesAsync(cancellationToken);
        return new EmbeddingBatchResult
        {
            Provider = Model.Provider,
            Model = Model.Key,
            Dimensions = _options.Dimensions,
            Embedded = embedded,
            SkippedUnauthorizedChunkIds = skipped,
        };
    }

    public Task<EmbeddingVector> EmbedQueryAsync(string text, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        EmbedCalls++;
        return Task.FromResult(new EmbeddingVector
        {
            Values = EmbedText(text, _options.Dimensions),
            Provider = Model.Provider,
            Model = _options.Model,
            Dimensions = _options.Dimensions,
        });
    }

    public static float[] EmbedText(string? text, int dimensions)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(dimensions);
        var vector = new float[dimensions];
        var tokens = Tokenize(text);
        if (tokens.Count == 0)
        {
            return vector;
        }

        for (var index = 0; index < tokens.Count; index++)
        {
            Add(vector, tokens[index], 1f);
            if (index + 1 < tokens.Count)
            {
                Add(vector, tokens[index] + " " + tokens[index + 1], 1.25f);
            }
        }

        var norm = Math.Sqrt(vector.Sum(value => value * value));
        if (norm == 0)
        {
            return vector;
        }

        for (var index = 0; index < vector.Length; index++)
        {
            vector[index] = (float)(vector[index] / norm);
        }

        return vector;
    }

    private static void Add(float[] vector, string token, float weight)
    {
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(token));
        var bucket = (int)(BitConverter.ToUInt32(bytes, 0) % (uint)vector.Length);
        vector[bucket] += weight;
    }

    private static List<string> Tokenize(string? text)
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            return [];
        }

        return TokenPattern.Matches(text.ToLowerInvariant())
            .Select(match => match.Value)
            .Where(token => token.Length >= 2)
            .ToList();
    }
}
