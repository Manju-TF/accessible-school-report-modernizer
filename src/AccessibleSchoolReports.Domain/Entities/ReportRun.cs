using AccessibleSchoolReports.Domain.Persistence;

namespace AccessibleSchoolReports.Domain.Entities;

public sealed class ReportRun
{
    public int Id { get; set; }

    public ReportGenerationMode Mode { get; set; }

    public RunStatus Status { get; set; }

    public DateTimeOffset StartedUtc { get; set; }

    public DateTimeOffset? CompletedUtc { get; set; }

    public int MaxParallelism { get; set; } = 1;

    public int TotalCount { get; set; }

    public int SuccessfulCount { get; set; }

    public int FailedCount { get; set; }

    public long DurationMilliseconds { get; set; }

    public string? OutputDirectory { get; set; }

    public string? Message { get; set; }

    public ICollection<ReportRunItem> Items { get; } = new List<ReportRunItem>();

    public ICollection<KnowledgeDocument> KnowledgeDocuments { get; } = new List<KnowledgeDocument>();
}
