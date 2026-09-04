using AccessibleSchoolReports.Domain.Persistence;

namespace AccessibleSchoolReports.Domain.Entities;

/// <summary>
/// One school’s result inside a report run. Stores output path only — not PDF bytes.
/// </summary>
public sealed class ReportRunItem
{
    public int Id { get; set; }

    public int ReportRunId { get; set; }

    public ReportRun ReportRun { get; set; } = null!;

    public int SchoolId { get; set; }

    public School School { get; set; } = null!;

    public RunStatus Status { get; set; }

    public string? OutputPath { get; set; }

    public string? Message { get; set; }

    public DateTimeOffset? StartedUtc { get; set; }

    public DateTimeOffset? CompletedUtc { get; set; }
}
