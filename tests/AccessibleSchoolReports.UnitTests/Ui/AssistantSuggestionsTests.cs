using AccessibleSchoolReports.Web.Ui;

namespace AccessibleSchoolReports.UnitTests.Ui;

public sealed class AssistantSuggestionsTests
{
    [Fact]
    public void GlobalScope_HasGeneralRulesAndAllReports()
    {
        var groups = AssistantSuggestions.ForScope(reportScoped: false);
        Assert.Equal(
            new[] { "General", "Business rules", "All generated reports" },
            groups.Select(group => group.Title).ToArray());
        Assert.Contains(
            groups,
            group => group.Questions.Contains("How is salary suppression handled?"));
        Assert.DoesNotContain(groups, group => group.Title == "This report");
        Assert.All(groups, group => Assert.True(group.Questions.Count >= 5));
    }

    [Fact]
    public void ReportScope_HasThisReportOnly()
    {
        var groups = AssistantSuggestions.ForScope(reportScoped: true);
        Assert.Equal(new[] { "This report" }, groups.Select(group => group.Title).ToArray());
        Assert.Contains(
            groups,
            group => group.Questions.Contains("What gender counts are printed in this report?"));
        Assert.All(groups, group => Assert.True(group.Questions.Count >= 5));
    }

    [Fact]
    public void EveryDefinedGroup_HasAtLeastFiveQuestions()
    {
        Assert.True(AssistantSuggestions.General.Questions.Count >= 5);
        Assert.True(AssistantSuggestions.BusinessRules.Questions.Count >= 5);
        Assert.True(AssistantSuggestions.ThisReport.Questions.Count >= 5);
        Assert.True(AssistantSuggestions.AllReports.Questions.Count >= 5);
    }
}
