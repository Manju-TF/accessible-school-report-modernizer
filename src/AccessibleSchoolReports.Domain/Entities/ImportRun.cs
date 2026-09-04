using AccessibleSchoolReports.Domain.Persistence;

namespace AccessibleSchoolReports.Domain.Entities;

public sealed class ImportRun
{
    public int Id { get; set; }

    public string? FileName { get; set; }

    public string? ContentSha256 { get; set; }

    public DateTimeOffset StartedUtc { get; set; }

    public DateTimeOffset? CompletedUtc { get; set; }

    public RunStatus Status { get; set; }

    public int ImportedRowCount { get; set; }

    public int InvalidRowCount { get; set; }

    public int BlankRowCount { get; set; }

    public string? Message { get; set; }

    public ICollection<GraduateRecord> Graduates { get; } = new List<GraduateRecord>();

    public ICollection<ImportRowIssue> Issues { get; } = new List<ImportRowIssue>();
}
