namespace AccessibleSchoolReports.Application.Reporting;

public static class ReportFileAccess
{
    public static bool TryResolveDownloadPath(string? storedPath, string? outputRoot, out string fullPath)
    {
        fullPath = string.Empty;
        if (string.IsNullOrWhiteSpace(storedPath) || string.IsNullOrWhiteSpace(outputRoot))
        {
            return false;
        }

        var root = Path.GetFullPath(outputRoot);
        var candidate = Path.GetFullPath(storedPath);
        if (!candidate.EndsWith(".pdf", StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }

        var rootPrefix = root.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar)
            + Path.DirectorySeparatorChar;
        if (!candidate.StartsWith(rootPrefix, StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }

        if (!File.Exists(candidate))
        {
            return false;
        }

        fullPath = candidate;
        return true;
    }
}
