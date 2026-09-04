using System.Security.Claims;

namespace AccessibleSchoolReports.Application.Knowledge;

public interface IEmbeddingService
{
    EmbeddingModelInfo Model { get; }

    Task<EmbeddingBatchResult> EmbedPermittedChunksAsync(
        ClaimsPrincipal user,
        IReadOnlyList<int> chunkIds,
        CancellationToken cancellationToken = default);

    Task<EmbeddingVector> EmbedQueryAsync(
        string text,
        CancellationToken cancellationToken = default);
}

public sealed class EmbeddingModelInfo
{
    public required string Provider { get; init; }

    public required string Model { get; init; }

    public required int Dimensions { get; init; }

    public string Key => $"{Provider}/{Model}";
}

public sealed class EmbeddingVector
{
    public required float[] Values { get; init; }

    public required string Provider { get; init; }

    public required string Model { get; init; }

    public required int Dimensions { get; init; }
}

public sealed class EmbeddedChunk
{
    public required int ChunkId { get; init; }

    public required float[] Values { get; init; }
}

public sealed class EmbeddingBatchResult
{
    public required string Provider { get; init; }

    public required string Model { get; init; }

    public required int Dimensions { get; init; }

    public required IReadOnlyList<EmbeddedChunk> Embedded { get; init; }

    public required IReadOnlyList<int> SkippedUnauthorizedChunkIds { get; init; }
}
