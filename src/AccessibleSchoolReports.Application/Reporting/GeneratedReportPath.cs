namespace AccessibleSchoolReports.Application.Reporting;

public static class GeneratedReportPath
{
    public static bool TryParseClassYear(string? outputPath, out int year)
    {
        year = 0;
        if (string.IsNullOrWhiteSpace(outputPath))
        {
            return false;
        }

        var parts = outputPath.Split(
            [Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar],
            StringSplitOptions.RemoveEmptyEntries);
        if (parts.Length < 3)
        {
            return false;
        }

        var fileName = parts[^1];
        var yearPart = parts[^3];
        if (!fileName.EndsWith(".pdf", StringComparison.OrdinalIgnoreCase)
            || yearPart.Length != 4
            || !yearPart.All(char.IsAsciiDigit))
        {
            return false;
        }

        year = int.Parse(yearPart);
        return year is >= 1900 and <= 2100;
    }
}
