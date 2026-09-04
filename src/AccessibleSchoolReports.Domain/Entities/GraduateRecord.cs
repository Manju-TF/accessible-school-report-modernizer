namespace AccessibleSchoolReports.Domain.Entities;

/// <summary>
/// One imported graduate row. Columns are SAS builder inputs only.
/// Recodes and calculations are not applied here.
/// </summary>
public sealed class GraduateRecord
{
    public int Id { get; set; }

    public int ImportRunId { get; set; }

    public ImportRun ImportRun { get; set; } = null!;

    public int SchoolId { get; set; }

    public School School { get; set; } = null!;

    public string? Sex3 { get; set; }

    public string? Minstat { get; set; }

    public string? Jobcat1 { get; set; }

    public string? JobFtPt { get; set; }

    public string? Empgen { get; set; }

    public string? Firm1 { get; set; }

    public string? Lfjob { get; set; }

    public string? Jobreg { get; set; }

    public string? LocationFlag { get; set; }

    public string? Jobst { get; set; }

    public string? Source { get; set; }

    public string? Time1 { get; set; }

    public string? Status { get; set; }

    public string? Duration { get; set; }

    public string? SchoolFund { get; set; }

    public decimal? SalFtPerm { get; set; }

    public string? Emptype1 { get; set; }
}
