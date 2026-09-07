using System.Text.RegularExpressions;
using Markdig;

namespace AccessibleSchoolReports.Web.Ui;

/// <summary>
/// Turns assistant Markdown into HTML. Raw HTML in the model text is disabled.
/// </summary>
public static class AssistantAnswerHtml
{
    private static readonly MarkdownPipeline Pipeline = new MarkdownPipelineBuilder()
        .UsePipeTables()
        .DisableHtml()
        .Build();

    private static readonly Regex TableOpen = new("<table>", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);
    private static readonly Regex TableClose = new("</table>", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);

    public static string ToSafeHtml(string? markdown)
    {
        if (string.IsNullOrWhiteSpace(markdown))
        {
            return string.Empty;
        }

        var html = Markdown.ToHtml(markdown.Replace("\u202f", " ", StringComparison.Ordinal), Pipeline);
        html = TableOpen.Replace(html, "<div class=\"table-wrap\"><table>");
        html = TableClose.Replace(html, "</table></div>");
        return html;
    }
}
