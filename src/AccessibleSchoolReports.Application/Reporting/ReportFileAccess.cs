namespace AccessibleSchoolReports.Application.Reporting;

public static class ReportFileAccess
{
    public static bool TryResolveDownloadPath(string? storedPath, string? outputRoot, out string fullPath)
    {
        if (!TryResolveStoredPdfPath(storedPath, outputRoot, out fullPath))
        {
            return false;
        }

        if (!File.Exists(fullPath))
        {
            fullPath = string.Empty;
            return false;
        }

        return true;
    }

    /// <summary>
    /// Resolves a stored report path against the configured output root.
    /// Does not use request input. Rejects traversal and paths outside the root.
    /// Does not require the file to exist.
    /// </summary>
    public static bool TryResolveStoredPdfPath(string? storedPath, string? outputRoot, out string fullPath)
    {
        fullPath = string.Empty;
        if (string.IsNullOrWhiteSpace(storedPath) || string.IsNullOrWhiteSpace(outputRoot))
        {
            return false;
        }

        if (ContainsTraversalSegment(storedPath))
        {
            return false;
        }

        if (!HasPdfExtension(storedPath))
        {
            return false;
        }

        var root = Path.GetFullPath(outputRoot);
        var candidate = Path.IsPathRooted(storedPath)
            ? Path.GetFullPath(storedPath)
            : Path.GetFullPath(Path.Combine(root, storedPath));

        if (!HasPdfExtension(candidate))
        {
            return false;
        }

        if (!IsUnderRoot(candidate, root))
        {
            return false;
        }

        fullPath = candidate;
        return true;
    }

    private static bool HasPdfExtension(string path) =>
        path.EndsWith(".pdf", StringComparison.OrdinalIgnoreCase);

    private static bool ContainsTraversalSegment(string path)
    {
        var parts = path.Split(
            [Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar],
            StringSplitOptions.RemoveEmptyEntries);
        return parts.Any(part => part == ".." || part == "...");
    }

    private static bool IsUnderRoot(string candidate, string root)
    {
        var rootPrefix = root.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar)
            + Path.DirectorySeparatorChar;
        return candidate.StartsWith(rootPrefix, StringComparison.OrdinalIgnoreCase);
    }
}
