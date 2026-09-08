namespace AccessibleSchoolReports.Application.Reporting;

/// <summary>
/// Inventory headcounts from stored graduate rows.
/// Does not apply calculator, suppression, or printed-report rules.
/// </summary>
public static class DashboardAnalytics
{
    public static IReadOnlyList<YearMetric> BuildYearMetrics(
        IReadOnlyList<YearHeadcount> years,
        int totalGraduates)
    {
        ArgumentNullException.ThrowIfNull(years);

        var ordered = years
            .OrderByDescending(year => year.Year.HasValue)
            .ThenByDescending(year => year.Year)
            .ToArray();

        var previousByYear = ordered
            .Where(year => year.Year is not null)
            .OrderBy(year => year.Year)
            .ToArray();

        var metrics = new List<YearMetric>(ordered.Length);
        foreach (var year in ordered)
        {
            int? change = null;
            if (year.Year is int current)
            {
                var prior = previousByYear.LastOrDefault(candidate => candidate.Year < current);
                if (prior is not null)
                {
                    change = year.GraduateCount - prior.GraduateCount;
                }
            }

            var share = totalGraduates <= 0
                ? 0
                : (int)Math.Round(year.GraduateCount * 100d / totalGraduates, MidpointRounding.AwayFromZero);
            metrics.Add(new YearMetric(
                year.Year,
                year.Year?.ToString() ?? "none",
                year.Year is int value ? $"Class of {value}" : "Year not recorded",
                year.SchoolCount,
                year.GraduateCount,
                share,
                change));
        }

        return metrics;
    }

    public static int AveragePerSchool(int graduates, int schoolsWithGraduates) =>
        schoolsWithGraduates <= 0
            ? 0
            : (int)Math.Round(graduates / (double)schoolsWithGraduates, MidpointRounding.AwayFromZero);
}

public sealed record YearHeadcount(int? Year, int SchoolCount, int GraduateCount);

public sealed record YearMetric(
    int? Year,
    string Key,
    string Label,
    int SchoolCount,
    int GraduateCount,
    int SharePercent,
    int? GraduateChange);
