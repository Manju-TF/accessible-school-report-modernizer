namespace AccessibleSchoolReports.Infrastructure.Import;

internal sealed class ParsedGraduateRow
{
    public required int ExcelRowNumber { get; init; }

    public required string SchoolCode { get; init; }

    public string? Sex3 { get; init; }

    public string? Minstat { get; init; }

    public string? Jobcat1 { get; init; }

    public string? JobFtPt { get; init; }

    public string? Empgen { get; init; }

    public string? Firm1 { get; init; }

    public string? Lfjob { get; init; }

    public string? Jobreg { get; init; }

    public string? LocationFlag { get; init; }

    public string? Jobst { get; init; }

    public string? Source { get; init; }

    public string? Time1 { get; init; }

    public string? Status { get; init; }

    public string? Duration { get; init; }

    public string? SchoolFund { get; init; }

    public decimal? SalFtPerm { get; init; }

    public string? Emptype1 { get; init; }
}
