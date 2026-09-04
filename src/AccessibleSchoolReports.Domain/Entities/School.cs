namespace AccessibleSchoolReports.Domain.Entities;

public sealed class School
{
    public int Id { get; set; }

    public required string Code { get; set; }

    public string? Name { get; set; }

    public ICollection<GraduateRecord> Graduates { get; } = new List<GraduateRecord>();

    public ICollection<ReportRunItem> ReportRunItems { get; } = new List<ReportRunItem>();
}
