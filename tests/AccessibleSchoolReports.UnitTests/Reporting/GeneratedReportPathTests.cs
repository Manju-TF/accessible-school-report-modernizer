using AccessibleSchoolReports.Application.Reporting;

namespace AccessibleSchoolReports.UnitTests.Reporting;

public sealed class GeneratedReportPathTests
{
    [Theory]
    [InlineData("output/2025/10701/summary-report.pdf", 2025)]
    [InlineData(@"C:\data\output\2024\23306\summary-report.pdf", 2024)]
    public void TryParseClassYear_ReadsYearFolder(string path, int year)
    {
        Assert.True(GeneratedReportPath.TryParseClassYear(path, out var parsed));
        Assert.Equal(year, parsed);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("output/summary-report.pdf")]
    [InlineData("output/notes.txt")]
    public void TryParseClassYear_RejectsUnknownPaths(string? path)
    {
        Assert.False(GeneratedReportPath.TryParseClassYear(path, out var year));
        Assert.Equal(0, year);
    }
}
