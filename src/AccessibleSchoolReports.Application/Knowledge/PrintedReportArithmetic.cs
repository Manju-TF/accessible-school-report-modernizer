using System.Globalization;
using System.Text;

namespace AccessibleSchoolReports.Application.Knowledge;

public static class PrintedReportArithmetic
{
    private const int MaxListedRows = 25;

    public static string? TryFormat(string? question, IReadOnlyList<KnowledgeRetrievalHit> hits)
    {
        if (!KnowledgeQuestionIntent.AsksForPrintedArithmetic(question) || hits is null || hits.Count == 0)
        {
            return null;
        }

        var kind = PrintedMetricParser.Resolve(question);
        var rows = PrintedMetricParser.Extract(hits, kind);
        if (rows.Count == 0)
        {
            return $"""
                Metric: {PrintedMetricParser.Label(kind)}
                No printable {PrintedMetricParser.Label(kind)} values were extracted from authorized generated reports.
                Printed periods are not included. SAS calculator rules were not rerun.
                """;
        }

        var byYear = rows
            .GroupBy(row => row.ReportYear)
            .OrderByDescending(group => group.Key)
            .Select(group => (
                Year: group.Key,
                Sum: group.Sum(row => row.Value),
                Schools: group.Select(row => row.SchoolCode).Distinct(StringComparer.Ordinal).Count(),
                Rows: group.OrderBy(row => row.SchoolCode, StringComparer.Ordinal).ToArray()))
            .ToArray();
        var builder = new StringBuilder();
        builder.AppendLine($"Metric: {PrintedMetricParser.Label(kind)}");
        builder.AppendLine("These figures are sums and differences of printed PDF values from authorized reports.");
        builder.AppendLine("They are not SAS calculator output and do not invent suppressed salaries.");
        builder.AppendLine($"Authorized printed values used: {rows.Count}.");
        builder.AppendLine();
        builder.AppendLine("Sum by Class year:");
        foreach (var year in byYear)
        {
            var label = year.Year is int value ? $"Class of {value}" : "Year not recorded";
            builder.AppendLine(CultureInfo.InvariantCulture, $"- {label}: {year.Sum} ({year.Schools} schools)");
        }

        if (byYear.Length >= 2)
        {
            builder.AppendLine();
            builder.AppendLine("Year-to-year difference of those year sums:");
            for (var index = 0; index < byYear.Length - 1; index++)
            {
                var newer = byYear[index];
                var older = byYear[index + 1];
                if (newer.Year is null || older.Year is null)
                {
                    continue;
                }

                var change = newer.Sum - older.Sum;
                builder.AppendLine(
                    CultureInfo.InvariantCulture,
                    $"- Class of {newer.Year} minus Class of {older.Year}: {FormatSigned(change)}");
            }

            AppendSchoolYearDifferences(builder, rows);
        }

        builder.AppendLine();
        builder.AppendLine(CultureInfo.InvariantCulture, $"Grand sum of extracted printed values: {rows.Sum(row => row.Value)}");
        builder.AppendLine();
        if (rows.Count <= MaxListedRows)
        {
            builder.AppendLine("Printed addends:");
            foreach (var row in rows)
            {
                var year = row.ReportYear is int value ? value.ToString(CultureInfo.InvariantCulture) : "none";
                builder.AppendLine(
                    CultureInfo.InvariantCulture,
                    $"- {row.SchoolCode ?? row.FileName} Class of {year}: {row.Value} ({row.FileName}, {row.SourceLocation})");
            }
        }
        else
        {
            builder.AppendLine("School-level addends are omitted from this block because more than 25 reports were used.");
            builder.AppendLine("The year sums above include every extracted printed value.");
        }

        return builder.ToString().TrimEnd();
    }

    private static void AppendSchoolYearDifferences(StringBuilder builder, IReadOnlyList<PrintedMetricValue> rows)
    {
        var pairs = rows
            .Where(row => row.ReportYear is not null && !string.IsNullOrWhiteSpace(row.SchoolCode))
            .GroupBy(row => row.SchoolCode!, StringComparer.Ordinal)
            .Select(group => group.OrderBy(row => row.ReportYear).ToArray())
            .Where(group => group.Length >= 2)
            .Take(MaxListedRows)
            .ToArray();
        if (pairs.Length == 0)
        {
            return;
        }

        builder.AppendLine();
        builder.AppendLine("Same-school year difference (newer minus older printed value):");
        foreach (var group in pairs)
        {
            for (var index = 1; index < group.Length; index++)
            {
                var older = group[index - 1];
                var newer = group[index];
                builder.AppendLine(
                    CultureInfo.InvariantCulture,
                    $"- {newer.SchoolCode} Class of {newer.ReportYear} minus Class of {older.ReportYear}: {FormatSigned(newer.Value - older.Value)}");
            }
        }
    }

    private static string FormatSigned(int value) =>
        value > 0 ? $"+{value}" : value.ToString(CultureInfo.InvariantCulture);
}
