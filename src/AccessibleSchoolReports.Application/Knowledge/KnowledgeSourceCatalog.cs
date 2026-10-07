namespace AccessibleSchoolReports.Application.Knowledge;

public sealed record KnowledgeProjectSource(string RelativePath, string? FallbackRelativePath = null);

public static class KnowledgeSourceCatalog
{
    public const string LegacySasDirectory = "legacy/sas";

    private static readonly string[] ExcludedRootSegments =
    [
        "data",
        "evidence",
        ".cursor",
        ".git",
        "bin",
        "obj",
        "node_modules",
        ".vs",
    ];

    private static readonly string[] BusinessRuleCodeDirectories =
    [
        "src/AccessibleSchoolReports.Application",
        "src/AccessibleSchoolReports.Domain",
    ];

    public static readonly IReadOnlyList<KnowledgeProjectSource> ProjectDocuments =
    [
        new("docs/capstone/business-rules.md"),
        new("docs/capstone/createschrptfiles-analysis.md"),
        new("docs/capstone/schreptsummary-analysis.md"),
        new("docs/capstone/report-map.md"),
        new("docs/capstone/pdf-accessibility-strategy.md", "docs/accessibility/pdf-accessibility-strategy.md"),
        new("docs/capstone/corrected-plan.md", "docs/architecture/corrected-plan.md"),
        new("README.md"),
    ];

    public static IReadOnlyList<string> DiscoverProjectMarkdownFiles(string root)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(root);
        var sourceRoot = Path.GetFullPath(root);
        var discovered = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        foreach (var file in Directory.EnumerateFiles(sourceRoot, "*.md", SearchOption.AllDirectories))
        {
            var relative = Normalize(Path.GetRelativePath(sourceRoot, file));
            if (IsAllowedRelativePath(relative))
            {
                discovered.Add(relative);
            }
        }

        foreach (var source in ProjectDocuments)
        {
            var normalized = Normalize(source.RelativePath);
            if (!string.IsNullOrWhiteSpace(normalized)
                && File.Exists(Path.Combine(sourceRoot, normalized.Replace('/', Path.DirectorySeparatorChar)))
                && IsAllowedRelativePath(normalized))
            {
                discovered.Add(normalized);
            }

            if (source.FallbackRelativePath is not null)
            {
                var fallback = Normalize(source.FallbackRelativePath);
                if (!string.IsNullOrWhiteSpace(fallback)
                    && File.Exists(Path.Combine(sourceRoot, fallback.Replace('/', Path.DirectorySeparatorChar)))
                    && IsAllowedRelativePath(fallback))
                {
                    discovered.Add(fallback);
                }
            }
        }

        return discovered
            .OrderBy(path => path, StringComparer.OrdinalIgnoreCase)
            .ToList();
    }

    public static IReadOnlyList<string> DiscoverBusinessRuleCodeFiles(string root)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(root);
        var sourceRoot = Path.GetFullPath(root);
        var discovered = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        foreach (var relativeDirectory in BusinessRuleCodeDirectories)
        {
            var directory = Path.Combine(sourceRoot, relativeDirectory.Replace('/', Path.DirectorySeparatorChar));
            if (!Directory.Exists(directory))
            {
                continue;
            }

            foreach (var file in Directory.EnumerateFiles(directory, "*.cs", SearchOption.AllDirectories))
            {
                var relative = Normalize(Path.GetRelativePath(sourceRoot, file));
                if (IsAllowedRelativePath(relative))
                {
                    discovered.Add(relative);
                }
            }
        }

        return discovered
            .OrderBy(path => path, StringComparer.OrdinalIgnoreCase)
            .ToList();
    }

    public static bool IsAllowedRelativePath(string relativePath)
    {
        var normalized = Normalize(relativePath);
        if (string.IsNullOrWhiteSpace(normalized)
            || normalized.Contains("..", StringComparison.Ordinal)
            || normalized.EndsWith(".xlsx", StringComparison.OrdinalIgnoreCase)
            || normalized.EndsWith(".xls", StringComparison.OrdinalIgnoreCase)
            || normalized.EndsWith(".csv", StringComparison.OrdinalIgnoreCase)
            || normalized.EndsWith(".png", StringComparison.OrdinalIgnoreCase)
            || normalized.EndsWith(".jpg", StringComparison.OrdinalIgnoreCase)
            || normalized.EndsWith(".jpeg", StringComparison.OrdinalIgnoreCase)
            || normalized.EndsWith(".pdf", StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }

        if (normalized.StartsWith("legacy/sas/", StringComparison.OrdinalIgnoreCase)
            && normalized.EndsWith(".sas", StringComparison.OrdinalIgnoreCase))
        {
            return true;
        }

        var segments = normalized.Split('/', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        if (segments.Any(segment => ExcludedRootSegments.Contains(segment, StringComparer.OrdinalIgnoreCase)))
        {
            return false;
        }

        if (normalized.EndsWith(".cs", StringComparison.OrdinalIgnoreCase))
        {
            return BusinessRuleCodeDirectories.Any(directory =>
                normalized.StartsWith($"{directory}/", StringComparison.OrdinalIgnoreCase));
        }

        if (normalized.StartsWith("docs/", StringComparison.OrdinalIgnoreCase)
            && normalized.EndsWith(".md", StringComparison.OrdinalIgnoreCase))
        {
            return true;
        }

        if (normalized == "README.md" || normalized.EndsWith("/README.md", StringComparison.OrdinalIgnoreCase))
        {
            return true;
        }

        return ProjectDocuments.Any(source =>
            Normalize(source.RelativePath) == normalized
            || (source.FallbackRelativePath is not null
                && Normalize(source.FallbackRelativePath) == normalized));
    }

    public static string Normalize(string relativePath) =>
        relativePath.Replace('\\', '/').TrimStart('/');
}
