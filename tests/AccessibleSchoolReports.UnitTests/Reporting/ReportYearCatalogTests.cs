using AccessibleSchoolReports.Application.Reporting;

namespace AccessibleSchoolReports.UnitTests.Reporting;

public sealed class ReportYearCatalogTests
{
    [Fact]
    public void Distinct_UsesConfiguredAndStoredYearsNewestFirst()
    {
        var years = ReportYearCatalog.Distinct("2025", [2023, 2025, 2024, null, 1800]);

        Assert.Equal(["2025", "2024", "2023"], years);
    }

    [Fact]
    public void Distinct_FallsBackTo2025WhenEmpty()
    {
        Assert.Equal(["2025"], ReportYearCatalog.Distinct(" ", []));
    }

    [Theory]
    [InlineData("2025", 2025)]
    [InlineData(" 1999 ", 1999)]
    public void TryParse_AcceptsFourDigitYears(string value, int expected)
    {
        Assert.True(ReportYearCatalog.TryParse(value, out var year));
        Assert.Equal(expected, year);
    }

    [Theory]
    [InlineData("25")]
    [InlineData("year")]
    [InlineData("1899")]
    public void TryParse_RejectsInvalidYears(string value)
    {
        Assert.False(ReportYearCatalog.TryParse(value, out _));
    }
}
