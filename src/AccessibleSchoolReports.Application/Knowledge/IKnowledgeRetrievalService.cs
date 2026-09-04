using System.Security.Claims;
using AccessibleSchoolReports.Domain.Knowledge;

namespace AccessibleSchoolReports.Application.Knowledge;

public interface IKnowledgeRetrievalService
{
    /// <summary>
    /// Selects authorized candidate chunks, then ranks by similarity. Does not call an LLM.
    /// </summary>
    Task<KnowledgeRetrievalResult> RetrieveAsync(
        ClaimsPrincipal user,
        string question,
        KnowledgeRetrievalOptions? options = null,
        CancellationToken cancellationToken = default);
}

public sealed class KnowledgeRetrievalOptions
{
    public const int DefaultTopK = 5;

    public const float DefaultMinimumSimilarity = 0.2f;

    public const int MaxQuestionLength = 4000;

    public int TopK { get; init; } = DefaultTopK;

    public float MinimumSimilarity { get; init; } = DefaultMinimumSimilarity;

    /// <summary>
    /// When set, retrieval is limited to that report after <c>CanViewReportAsync</c>.
    /// Unauthorized or unknown values are ignored as empty — they are not trusted.
    /// </summary>
    public int? ReportId { get; init; }
}

public sealed class KnowledgeRetrievalResult
{
    public required IReadOnlyList<KnowledgeRetrievalHit> Hits { get; init; }

    public int AuthorizedCandidateCount { get; init; }

    public TimeSpan Duration { get; init; }
}

public sealed class KnowledgeRetrievalHit
{
    public required int ChunkId { get; init; }

    public required int DocumentId { get; init; }

    public required string Content { get; init; }

    public string? RuleId { get; init; }

    public int? SchoolId { get; init; }

    public string? SchoolCode { get; init; }

    public int? ReportId { get; init; }

    public int? ReportYear { get; init; }

    public required string SourceLocation { get; init; }

    public required string SourceIdentifier { get; init; }

    public required string FileName { get; init; }

    public KnowledgeDocumentType DocumentType { get; init; }

    public KnowledgeAuthorizationScope AuthorizationScope { get; init; }

    public required float Similarity { get; init; }
}
