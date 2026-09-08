using AccessibleSchoolReports.Application.Knowledge;
using AccessibleSchoolReports.Domain.Knowledge;

namespace AccessibleSchoolReports.UnitTests.Knowledge;

public sealed class PrintedReportArithmeticTests
{
    [Fact]
    public void Parser_ReadsTotalReportedEquals_AndIgnoresPage7WithoutEquals()
    {
        Assert.True(PrintedMetricParser.TryRead("[School 10701, Class of 2025]\nTotal Reported = 20", PrintedMetricKind.TotalReported, out var total));
        Assert.Equal(20, total);
        Assert.False(PrintedMetricParser.TryRead("Total Reported 18 .", PrintedMetricKind.TotalReported, out _));
        Assert.True(PrintedMetricParser.TryRead("Employed 18 90.0\nSubtotal 18 90.0", PrintedMetricKind.Employed, out var employed));
        Assert.Equal(18, employed);
        Assert.False(PrintedMetricParser.TryRead("Not Employed-Not Seeking 1 7.7", PrintedMetricKind.Employed, out _));
    }

    [Fact]
    public void SumAndYearDifference_UsePrintedValuesOnly()
    {
        var formatted = PrintedReportArithmetic.TryFormat(
            "What is the sum of Total Reported across all schools and the difference from last year?",
            [
                Hit("10701", 2025, "[School 10701, Class of 2025]\nTotal Reported = 20"),
                Hit("10701", 2024, "[School 10701, Class of 2024]\nTotal Reported = 17"),
                Hit("23306", 2025, "[School 23306, Class of 2025]\nTotal Reported = 10"),
            ]);

        Assert.NotNull(formatted);
        Assert.Contains("Class of 2025: 30 (2 schools)", formatted, StringComparison.Ordinal);
        Assert.Contains("Class of 2024: 17 (1 schools)", formatted, StringComparison.Ordinal);
        Assert.Contains("Class of 2025 minus Class of 2024: +13", formatted, StringComparison.Ordinal);
        Assert.Contains("10701 Class of 2025 minus Class of 2024: +3", formatted, StringComparison.Ordinal);
        Assert.Contains("Grand sum of extracted printed values: 47", formatted, StringComparison.Ordinal);
        Assert.Contains("not SAS calculator output", formatted, StringComparison.Ordinal);
        Assert.DoesNotContain("Total Reported 18 .", formatted, StringComparison.Ordinal);
    }

    [Fact]
    public void CompareQuestion_WithoutPrintedValues_SaysNoneExtracted()
    {
        var formatted = PrintedReportArithmetic.TryFormat(
            "Compare Total Reported across generated reports I can view.",
            [Hit("10701", 2025, "note only")]);

        Assert.Contains("No printable Total Reported values were extracted", formatted, StringComparison.Ordinal);
    }

    [Fact]
    public void RuleQuestion_DoesNotBuildArithmetic()
    {
        Assert.Null(PrintedReportArithmetic.TryFormat(
            "How is median salary calculated?",
            [Hit("10701", 2025, "Total Reported = 20")]));
    }

    [Fact]
    public void GroundedPrompt_PlacesArithmeticOutsideUntrustedFence()
    {
        var arithmetic = PrintedReportArithmetic.TryFormat(
            "Compare Total Reported across all schools.",
            [Hit("10701", 2025, "Total Reported = 20")]);
        var user = KnowledgeGroundedPrompt.FormatUserMessage(
            KnowledgeGroundedPrompt.Create("Compare Total Reported across all schools.", [Hit("10701", 2025, "secret")], arithmetic));

        Assert.Contains("Application-computed from printed PDF values", user, StringComparison.Ordinal);
        Assert.Contains("Class of 2025: 20", user, StringComparison.Ordinal);
        Assert.True(user.IndexOf("Application-computed", StringComparison.Ordinal) < user.IndexOf(KnowledgeGroundedPrompt.UntrustedBegin, StringComparison.Ordinal));
    }

    private static KnowledgeRetrievalHit Hit(string school, int year, string content) =>
        new()
        {
            ChunkId = school.GetHashCode(StringComparison.Ordinal) + year,
            DocumentId = year,
            Content = content,
            SchoolCode = school,
            ReportYear = year,
            SourceLocation = "page 1",
            SourceIdentifier = $"{school}.pdf",
            FileName = $"{school}-summary-report.pdf",
            DocumentType = KnowledgeDocumentType.GeneratedReport,
            AuthorizationScope = KnowledgeAuthorizationScope.Report,
            Similarity = 0.9f,
        };
}
