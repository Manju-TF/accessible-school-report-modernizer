using AccessibleSchoolReports.Application.Reporting;

namespace AccessibleSchoolReports.UnitTests.Reporting;

public sealed class DashboardAnalyticsTests
{
    [Fact]
    public void BuildYearMetrics_OrdersYearsAndComputesShareAndChange()
    {
        var metrics = DashboardAnalytics.BuildYearMetrics(
            [
                new YearHeadcount(2023, 10, 100),
                new YearHeadcount(null, 8, 50),
                new YearHeadcount(2025, 12, 150),
                new YearHeadcount(2024, 11, 120),
            ],
            420);

        Assert.Equal(["2025", "2024", "2023", "none"], metrics.Select(year => year.Key));
        Assert.Equal("Class of 2025", metrics[0].Label);
        Assert.Equal("Year not recorded", metrics[3].Label);
        Assert.Equal(36, metrics[0].SharePercent);
        Assert.Equal(30, metrics[0].GraduateChange);
        Assert.Equal(20, metrics[1].GraduateChange);
        Assert.Null(metrics[2].GraduateChange);
    }

    [Fact]
    public void AveragePerSchool_RoundsAwayFromZero()
    {
        Assert.Equal(0, DashboardAnalytics.AveragePerSchool(10, 0));
        Assert.Equal(17, DashboardAnalytics.AveragePerSchool(50, 3));
    }
}
