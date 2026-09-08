using AccessibleSchoolReports.Web.Ui;

namespace AccessibleSchoolReports.UnitTests.Ui;

public sealed class AssistantSuggestionsTests
{
    [Fact]
    public void GlobalScope_HasPrintedTotalsAndCompare_NotBusinessRules()
    {
        var groups = AssistantSuggestions.ForScope(reportScoped: false);
        Assert.Equal(
            new[] { "All generated reports", "Compare reports", "General" },
            groups.Select(group => group.Title).ToArray());
        Assert.Contains(
            groups,
            group => group.Questions.Contains("What is the sum of Total Reported across generated reports I can view?"));
        Assert.Contains(
            groups,
            group => group.Questions.Contains("What is the difference in Total Reported between Class of 2025 and last year in generated reports I can view?"));
        Assert.DoesNotContain(groups, group => group.Title == "Business rules");
        Assert.DoesNotContain(groups, group => group.Title == "This report");
        Assert.DoesNotContain(
            groups.SelectMany(group => group.Questions),
            question => question.Contains("salary suppression", StringComparison.OrdinalIgnoreCase)
                || question.Contains("CF-S-00", StringComparison.OrdinalIgnoreCase)
                || question.Contains("lack tests", StringComparison.OrdinalIgnoreCase));
        Assert.All(groups, group => Assert.True(group.Questions.Count >= 5));
    }

    [Fact]
    public void ReportScope_HasThisReportOnly()
    {
        var groups = AssistantSuggestions.ForScope(reportScoped: true);
        Assert.Equal(new[] { "This report" }, groups.Select(group => group.Title).ToArray());
        Assert.Contains(
            groups,
            group => group.Questions.Contains("What is Total Reported in this report?"));
        Assert.All(groups, group => Assert.True(group.Questions.Count >= 5));
    }

    [Fact]
    public void EveryDefinedGroup_HasAtLeastFiveQuestions()
    {
        Assert.True(AssistantSuggestions.General.Questions.Count >= 5);
        Assert.True(AssistantSuggestions.ThisReport.Questions.Count >= 5);
        Assert.True(AssistantSuggestions.AllReports.Questions.Count >= 5);
        Assert.True(AssistantSuggestions.CompareReports.Questions.Count >= 5);
    }
}
