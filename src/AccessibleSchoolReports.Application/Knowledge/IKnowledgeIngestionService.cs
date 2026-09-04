namespace AccessibleSchoolReports.Application.Knowledge;

public interface IKnowledgeIngestionService
{
    Task<KnowledgeIngestionResult> IngestLegacyAndProjectDocumentsAsync(
        string repositoryRoot,
        CancellationToken cancellationToken = default);
}

public sealed class KnowledgeIngestionResult
{
    public required IReadOnlyList<string> Indexed { get; init; }

    public required IReadOnlyList<string> Reindexed { get; init; }

    public required IReadOnlyList<string> SkippedUnchanged { get; init; }

    public required IReadOnlyList<string> Missing { get; init; }

    public int Considered =>
        Indexed.Count + Reindexed.Count + SkippedUnchanged.Count + Missing.Count;
}
