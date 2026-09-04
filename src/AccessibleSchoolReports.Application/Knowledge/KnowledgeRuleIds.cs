using System.Text.RegularExpressions;

namespace AccessibleSchoolReports.Application.Knowledge;

public static partial class KnowledgeRuleIds
{
    public static string? FirstIn(string? text)
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            return null;
        }

        var match = RuleIdPattern().Match(text);
        return match.Success ? match.Value : null;
    }

    [GeneratedRegex(@"\b(?:CF|SS)-[A-Z0-9]+-\d+\b", RegexOptions.CultureInvariant)]
    private static partial Regex RuleIdPattern();
}
