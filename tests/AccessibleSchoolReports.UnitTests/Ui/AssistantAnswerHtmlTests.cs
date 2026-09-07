using AccessibleSchoolReports.Web.Ui;

namespace AccessibleSchoolReports.UnitTests.Ui;

public sealed class AssistantAnswerHtmlTests
{
    [Fact]
    public void RendersHeadingBoldAndTable()
    {
        const string markdown =
            """
            **Key metrics**

            | Metric | Source |
            |---|---|
            | Employment by gender | Document 4, page 1 |
            """;

        var html = AssistantAnswerHtml.ToSafeHtml(markdown);

        Assert.Contains("<strong>Key metrics</strong>", html, StringComparison.Ordinal);
        Assert.Contains("<table>", html, StringComparison.Ordinal);
        Assert.Contains("<th>", html, StringComparison.Ordinal);
        Assert.Contains("Employment by gender", html, StringComparison.Ordinal);
        Assert.Contains("class=\"table-wrap\"", html, StringComparison.Ordinal);
    }

    [Fact]
    public void DoesNotRenderRawHtmlFromTheModel()
    {
        var html = AssistantAnswerHtml.ToSafeHtml("<script>alert(1)</script> **safe**");

        Assert.DoesNotContain("<script>", html, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("<strong>safe</strong>", html, StringComparison.Ordinal);
    }

    [Fact]
    public void EmptyMarkdown_IsEmptyHtml()
    {
        Assert.Equal(string.Empty, AssistantAnswerHtml.ToSafeHtml("  "));
        Assert.Equal(string.Empty, AssistantAnswerHtml.ToSafeHtml(null));
    }
}
