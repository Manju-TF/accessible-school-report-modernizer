using System.Globalization;

namespace AccessibleSchoolReports.Application.Reporting;

/// <summary>
/// Distinct four-digit years for generate-page choices.
/// Does not change calculator or printed PDF titles.
/// </summary>
public static class ReportYearCatalog
{
    public static IReadOnlyList<string> Distinct(string? configuredClassYear, IEnumerable<int?> storedYears)
    {
        var years = new SortedSet<int>();
        if (TryParse(configuredClassYear, out var configured))
        {
            years.Add(configured);
        }

        foreach (var year in storedYears)
        {
            if (year is >= 1900 and <= 2100)
            {
                years.Add(year.Value);
            }
        }

        if (years.Count == 0)
        {
            years.Add(2025);
        }

        return years
            .Reverse()
            .Select(year => year.ToString(CultureInfo.InvariantCulture))
            .ToArray();
    }

    public static bool TryParse(string? value, out int year)
    {
        year = 0;
        if (string.IsNullOrWhiteSpace(value))
        {
            return false;
        }

        var trimmed = value.Trim();
        if (trimmed.Length != 4 || !trimmed.All(char.IsAsciiDigit))
        {
            return false;
        }

        year = int.Parse(trimmed, CultureInfo.InvariantCulture);
        return year is >= 1900 and <= 2100;
    }
}
