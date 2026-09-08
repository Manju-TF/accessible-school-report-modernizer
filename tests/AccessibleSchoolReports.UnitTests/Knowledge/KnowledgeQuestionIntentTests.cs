using AccessibleSchoolReports.Application.Knowledge;

namespace AccessibleSchoolReports.UnitTests.Knowledge;

public sealed class KnowledgeQuestionIntentTests
{
    [Theory]
    [InlineData("Compare Total Reported across generated reports I can view.")]
    [InlineData("What is Total Reported on the School A Class of 2025 summary report?")]
    [InlineData("Compare year-over-year printed values for the same school.")]
    [InlineData("How many employed graduates are in this report?")]
    [InlineData("What gender counts are printed?")]
    [InlineData("What is the median salary?")]
    [InlineData("List every printed count in the PDF.")]
    [InlineData("What is the sum of Total Reported across all schools?")]
    [InlineData("What is the difference from last year?")]
    public void PrintedReportQuestions_PreferGeneratedReports(string question)
    {
        Assert.True(KnowledgeQuestionIntent.PrefersPrintedReportEvidence(question));
    }

    [Theory]
    [InlineData("What does rule CF-S-00 in createschrptfiles2025.sas mean when salary rows are kept only if n ge 5?")]
    [InlineData("How is employment status counted when jobcat1 is UNKN for analvar D?")]
    [InlineData("When are salary statistics omitted because n ge 5 on salftperm?")]
    public void RuleQuestions_DoNotPreferGeneratedReports(string question)
    {
        Assert.False(KnowledgeQuestionIntent.PrefersPrintedReportEvidence(question));
    }
}
