namespace AccessibleSchoolReports.Application.Knowledge;

public sealed record KnowledgeProjectSource(string RelativePath, string? FallbackRelativePath = null);

public static class KnowledgeSourceCatalog
{
    public const string LegacySasDirectory = "legacy/sas";

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

    public static bool IsAllowedRelativePath(string relativePath)
    {
        var normalized = Normalize(relativePath);
        if (normalized.Contains("..", StringComparison.Ordinal)
            || normalized.StartsWith("data/", StringComparison.OrdinalIgnoreCase)
            || normalized.EndsWith(".xlsx", StringComparison.OrdinalIgnoreCase)
            || normalized.EndsWith(".xls", StringComparison.OrdinalIgnoreCase)
            || normalized.EndsWith(".csv", StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }

        if (normalized.StartsWith("legacy/sas/", StringComparison.OrdinalIgnoreCase)
            && normalized.EndsWith(".sas", StringComparison.OrdinalIgnoreCase))
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
