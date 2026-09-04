namespace AccessibleSchoolReports.Application.Reporting;

public sealed class SchoolReport
{
    public required string SchoolCode { get; init; }

    /// <summary>Display name for the PDF title. Not used by the calculator.</summary>
    public string? SchoolName { get; init; }

    public required IReadOnlyList<SchoolReportRow> Rows { get; init; }

    public required IReadOnlyList<SchoolReportSection> Sections { get; init; }
}

public sealed class SchoolReportSection
{
    public required string Analvar { get; init; }

    public required IReadOnlyList<SchoolReportRow> Details { get; init; }

    public int SubtotalCount { get; init; }

    public decimal SubtotalPercent { get; init; }
}

public sealed class SchoolReportRow
{
    public required string Analvar { get; init; }

    public required string Newvar { get; init; }

    public int? Count { get; init; }

    public decimal? Percent { get; init; }

    public int? SalaryN { get; init; }

    public decimal? Pct25 { get; init; }

    public decimal? Median { get; init; }

    public decimal? Pct75 { get; init; }

    public decimal? Mean { get; init; }

    public IReadOnlyDictionary<string, int>? DurationCounts { get; init; }
}
