using System.Globalization;
using System.Text.RegularExpressions;
using AccessibleSchoolReports.Domain.Knowledge;

namespace AccessibleSchoolReports.Application.Knowledge;

public enum PrintedMetricKind
{
    TotalReported = 0,
    Employed = 1,
}

public sealed record PrintedMetricValue(
    string? SchoolCode,
    int? ReportYear,
    int Value,
    string FileName,
    string SourceLocation);

public static partial class PrintedMetricParser
{
    public static PrintedMetricKind Resolve(string? question)
    {
        if (!string.IsNullOrWhiteSpace(question)
            && (Contains(question, "employed") || Contains(question, "employment")))
        {
            return PrintedMetricKind.Employed;
        }

        return PrintedMetricKind.TotalReported;
    }

    public static string Label(PrintedMetricKind kind) =>
        kind == PrintedMetricKind.Employed ? "Employed" : "Total Reported";

    public static IReadOnlyList<PrintedMetricValue> Extract(
        IEnumerable<KnowledgeRetrievalHit> hits,
        PrintedMetricKind kind)
    {
        ArgumentNullException.ThrowIfNull(hits);
        var found = new Dictionary<string, PrintedMetricValue>(StringComparer.Ordinal);
        foreach (var hit in hits)
        {
            if (hit.DocumentType != KnowledgeDocumentType.GeneratedReport)
            {
                continue;
            }

            if (!TryRead(hit.Content, kind, out var value))
            {
                continue;
            }

            var key = $"{hit.SchoolCode ?? hit.FileName}|{hit.ReportYear?.ToString(CultureInfo.InvariantCulture) ?? "none"}";
            if (found.ContainsKey(key))
            {
                continue;
            }

            found[key] = new PrintedMetricValue(
                hit.SchoolCode,
                hit.ReportYear,
                value,
                hit.FileName,
                hit.SourceLocation);
        }

        return found.Values
            .OrderByDescending(row => row.ReportYear)
            .ThenBy(row => row.SchoolCode, StringComparer.Ordinal)
            .ToArray();
    }

    public static bool TryRead(string? content, PrintedMetricKind kind, out int value)
    {
        value = 0;
        if (string.IsNullOrWhiteSpace(content))
        {
            return false;
        }

        var match = kind == PrintedMetricKind.Employed
            ? EmployedLine().Match(content)
            : TotalReportedEquals().Match(content);
        return match.Success
            && int.TryParse(match.Groups[1].Value, NumberStyles.None, CultureInfo.InvariantCulture, out value);
    }

    private static bool Contains(string question, string phrase) =>
        question.Contains(phrase, StringComparison.OrdinalIgnoreCase);

    [GeneratedRegex(@"Total Reported\s*=\s*(\d+)", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant)]
    private static partial Regex TotalReportedEquals();

    [GeneratedRegex(@"(?m)^Employed\s+(\d+)\b", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant)]
    private static partial Regex EmployedLine();
}
