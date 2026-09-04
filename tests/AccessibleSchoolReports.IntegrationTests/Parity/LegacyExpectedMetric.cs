namespace AccessibleSchoolReports.IntegrationTests.Parity;

public enum ParityField
{
    Count,
    Percent,
    SalaryN,
    Pct25,
    Median,
    Pct75,
    Mean,
    SubtotalCount,
    SubtotalPercent,
}

public enum ParityStatus
{
    Match,
    Mismatch,
    Unresolved,
}

public sealed record LegacyExpectedMetric(
    string Id,
    string RuleId,
    string Analvar,
    string? Newvar,
    ParityField Field,
    decimal? Expected,
    string Label,
    string? Note = null,
    bool Unresolved = false);

public sealed record ParityObservation(
    LegacyExpectedMetric Metric,
    decimal? Modern,
    string? ModernSchoolCode,
    ParityStatus Status,
    string Explanation);
