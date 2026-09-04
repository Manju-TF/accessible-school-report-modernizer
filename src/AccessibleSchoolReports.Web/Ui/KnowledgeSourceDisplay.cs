using AccessibleSchoolReports.Domain.Knowledge;

namespace AccessibleSchoolReports.Web.Ui;

public static class KnowledgeSourceDisplay
{
    public static string DocumentName(string? fileName)
    {
        if (string.IsNullOrWhiteSpace(fileName))
        {
            return "Untitled document";
        }

        var name = Path.GetFileName(fileName.Replace('\\', '/').Trim());
        return string.IsNullOrWhiteSpace(name) ? "Untitled document" : name;
    }

    public static string Location(string? sourceLocation)
    {
        if (string.IsNullOrWhiteSpace(sourceLocation))
        {
            return "Not specified";
        }

        var value = sourceLocation.Replace('\\', '/').Trim();
        if (IsPhysicalOrUnsafe(value))
        {
            return TryPageLabel(value) ?? "Document location";
        }

        return value;
    }

    public static string RuleId(string? ruleId) =>
        string.IsNullOrWhiteSpace(ruleId) ? "Not recorded" : ruleId.Trim();

    public static string DocumentKind(KnowledgeDocumentType type) =>
        type switch
        {
            KnowledgeDocumentType.GeneratedReport => "Generated report",
            KnowledgeDocumentType.Project => "Project documentation",
            _ => "Legacy documentation",
        };

    private static bool IsPhysicalOrUnsafe(string value)
    {
        if (value.Contains("..", StringComparison.Ordinal))
        {
            return true;
        }

        if (value.Length >= 2 && char.IsAsciiLetter(value[0]) && value[1] == ':')
        {
            return true;
        }

        return value.StartsWith("//", StringComparison.Ordinal)
            || value.Contains("/Users/", StringComparison.OrdinalIgnoreCase)
            || value.Contains("/home/", StringComparison.OrdinalIgnoreCase)
            || value.Contains("AppData", StringComparison.OrdinalIgnoreCase)
            || value.Contains("/tmp/", StringComparison.OrdinalIgnoreCase)
            || value.Contains("output/", StringComparison.OrdinalIgnoreCase);
    }

    private static string? TryPageLabel(string value)
    {
        var marker = value.LastIndexOf("page ", StringComparison.OrdinalIgnoreCase);
        if (marker < 0)
        {
            return null;
        }

        var page = value[marker..].Trim();
        return page.Length > 32 ? page[..32] : page;
    }
}
