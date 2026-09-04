using System.Security.Claims;
using AccessibleSchoolReports.Domain.Entities;

namespace AccessibleSchoolReports.Application.Knowledge;

public interface IKnowledgeEmbeddingIndexService
{
    Task<KnowledgeIndexResult> IndexPendingEmbeddingsAsync(
        ClaimsPrincipal user,
        CancellationToken cancellationToken = default);
}

public sealed class KnowledgeIndexResult
{
    public int DocumentsIndexed { get; init; }

    public int ChunksIndexed { get; init; }

    public int ChunksSkipped { get; init; }

    public int Failures { get; init; }

    public TimeSpan Duration { get; init; }

    public IReadOnlyList<KnowledgeIndexFailure> FailureDetails { get; init; } = [];
}

public sealed class KnowledgeIndexFailure
{
    public required int ChunkId { get; init; }

    public int DocumentId { get; init; }

    public string? Message { get; init; }
}

public static class KnowledgeEmbeddingState
{
    public static bool HasCurrentEmbedding(KnowledgeChunk chunk, string modelKey) =>
        chunk.Embedding is { Length: > 0 }
        && string.Equals(chunk.EmbeddingModel, modelKey, StringComparison.Ordinal);

    public static bool NeedsEmbedding(KnowledgeChunk chunk, string modelKey) =>
        !HasCurrentEmbedding(chunk, modelKey);
}
