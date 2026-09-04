using AccessibleSchoolReports.Application.Reporting;

namespace AccessibleSchoolReports.UnitTests.Reporting;

public sealed class SchoolReportLayoutTests
{
    [Fact]
    public void Compose_BuildsSevenPages_AndOmitsEmptyEducationSection()
    {
        var printable = SchoolReportLayout.Compose(FixtureReport());

        Assert.Equal(7, printable.Pages.Count);
        Assert.Equal(new[] { 1, 2, 3, 4, 5, 6, 7 }, printable.Pages.Select(page => page.Number));
        Assert.DoesNotContain(printable.Pages[2].Sections, section => section.Analvar == "E2");
        Assert.Contains(printable.Pages[0].Sections, section => section.Analvar == "B");
        Assert.Contains(printable.Pages[1].Sections, section => section.Analvar == "D1");
        Assert.Contains(printable.Pages[6].Sections, section => section.Analvar == "DURATION");
    }

    [Fact]
    public void Compose_UsesStoredValues_AndDoesNotRecalculate()
    {
        var printable = SchoolReportLayout.Compose(FixtureReport());
        var women = printable.Pages[0].Sections.Single(section => section.Analvar == "B").Rows.Single(row => row.Label == "Women");

        Assert.Equal("46", women.Count);
        Assert.Equal("46.0", women.Percent);
        Assert.Equal("40", women.SalaryN);
        Assert.Equal("70,000", women.Pct25);
    }

    [Fact]
    public void Compose_UsesNotDisplayed_ForSuppressedSalaries()
    {
        var printable = SchoolReportLayout.Compose(FixtureReport());
        var menOfColor = printable.Pages[0].Sections
            .Single(section => section.Analvar == "C1")
            .Rows.Single(row => row.Label == "Men of Color");

        Assert.Equal("4", menOfColor.Count);
        Assert.Equal(SchoolReportPresentation.NotDisplayed, menOfColor.SalaryN);
        Assert.Equal(SchoolReportPresentation.NotDisplayed, menOfColor.Median);
    }

    [Fact]
    public void Compose_DurationUsesLongAndShortTerm_WithoutPercents()
    {
        var printable = SchoolReportLayout.Compose(FixtureReport());
        var duration = printable.Pages[6].Sections.Single(section => section.Analvar == "DURATION");
        var firm = duration.Rows.Single(row => row.Label == "Private Practice");

        Assert.Equal(PrintableTableKind.Duration, printable.Pages[6].TableKind);
        Assert.Equal("50", firm.LongTerm);
        Assert.Equal(SchoolReportPresentation.NotDisplayed, firm.ShortTerm);
        Assert.Equal(SchoolReportPresentation.NotDisplayed, firm.Percent);
        Assert.Equal("Total Reported", duration.SubtotalLabel);
    }

    [Fact]
    public void Compose_Page2IncludesD1ThroughE1_ByCharacterSlice()
    {
        Assert.True(SchoolReportPresentation.IsPage2Analvar("D1"));
        Assert.True(SchoolReportPresentation.IsPage2Analvar("E1"));
        Assert.False(SchoolReportPresentation.IsPage2Analvar("E2"));
        Assert.False(SchoolReportPresentation.IsPage2Analvar("B"));
    }

    [Fact]
    public void Compose_Jobreg3SubtotalIsTotalHash_AndPercentNotDisplayed()
    {
        var printable = SchoolReportLayout.Compose(FixtureReport());
        var states = printable.Pages[4].Sections.Single(section => section.Analvar == "JOBREG3");

        Assert.Equal("Total #", states.SubtotalLabel);
        Assert.Equal("14", states.Rows[0].Count);
        Assert.Equal(SchoolReportPresentation.NotDisplayed, states.Rows[0].Percent);
        Assert.Equal(SchoolReportPresentation.NotDisplayed, states.Subtotal.Percent);
    }

    [Fact]
    public void Compose_UsesSchoolNameInDocumentTitle()
    {
        var printable = SchoolReportLayout.Compose(FixtureReport());

        Assert.Equal("Test University School of Law", printable.SchoolName);
        Assert.Equal("Test University School of Law — Class of 2025 Summary Report", printable.DocumentTitle);
        Assert.Equal("en-US", printable.Language);
        Assert.Equal(100, printable.TotalReported);
    }

    private static SchoolReport FixtureReport()
    {
        return new SchoolReport
        {
            SchoolCode = "99999",
            SchoolName = "Test University School of Law",
            Rows =
            [
                Row("A", "A", 100, 100m),
                Row("B", "F", 46, 46m, 40, 70000, 85500, 110000, 85802),
                Row("B", "M", 54, 54m, 30, 85000, 90500, 120000, 103401),
                Row("C", "MINOR", 24, 34.3m),
                Row("C", "NONMIN", 46, 65.7m),
                Row("C1", "MINORF", 10, 15.6m, 6, 65000, 88500, 90000, 84673),
                Row("C1", "MINORM", 4, 6.3m),
                Row("D", "1-LJD", 78, 78m),
                Row("D1", "EMPL", 93, 93m),
                Row("D2", "PRIVATE", 78, 83.9m),
                Row("D3", "1-LJDFULL", 79, 84.9m),
                Row("E1", "FIRM", 50, 53.8m),
                Row("E3", "1-LJD", 2, 14.3m),
                Row("E4", "1-LJD", 46, 92m),
                Row("E5", "1-LJD", 13, 81.3m),
                Row("E55", "JCSTGV", 4, 80m),
                Row("E6", "1-LJD", 6, 75m),
                Row("FIRM", "LF1", 30, 60m),
                Row("FIRM2", "ATTY", 42, 84m),
                Row("JOBREG1", "1", 69, 74.2m),
                Row("JOBREG2", "INSTATE", 63, 67.8m),
                Row("JOBREG3", "JOBREG3", 14, null),
                Row("SOURCE", "AOCI", 4, 4.4m),
                Row("TIME", "BGRAD", 49, 61.3m),
                Row("ZSTATUS", "SET", 86, 95.6m),
                new SchoolReportRow
                {
                    Analvar = "DURATION",
                    Newvar = string.Empty,
                    DurationCounts = new Dictionary<string, int>(StringComparer.Ordinal)
                    {
                        ["PERM"] = 85,
                        ["TEMP"] = 2,
                    },
                },
                new SchoolReportRow
                {
                    Analvar = "DURATION",
                    Newvar = "FIRM",
                    DurationCounts = new Dictionary<string, int>(StringComparer.Ordinal)
                    {
                        ["PERM"] = 50,
                    },
                },
            ],
            Sections =
            [
                Section("A", 100, 100m),
                Section("B", 100, 100m),
                Section("C", 70, 100m),
                Section("C1", 64, 100m),
                Section("D", 100, 100m),
                Section("D1", 93, 93m),
                Section("D2", 93, 100m),
                Section("D3", 93, 100m),
                Section("E1", 93, 100m),
                Section("E3", 14, 100m),
                Section("E4", 50, 100m),
                Section("E5", 16, 100m),
                Section("E55", 5, 100m),
                Section("E6", 8, 100m),
                Section("FIRM", 50, 100m),
                Section("FIRM2", 50, 100m),
                Section("JOBREG1", 93, 100m),
                Section("JOBREG2", 93, 100m),
                Section("JOBREG3", 14, 100m),
                Section("SOURCE", 90, 100m),
                Section("TIME", 80, 100.1m),
                Section("ZSTATUS", 90, 100m),
                Section("DURATION", 85, 0m),
            ],
        };
    }

    private static SchoolReportRow Row(
        string analvar,
        string newvar,
        int count,
        decimal? percent,
        int? salaryN = null,
        int? pct25 = null,
        int? median = null,
        int? pct75 = null,
        int? mean = null) =>
        new()
        {
            Analvar = analvar,
            Newvar = newvar,
            Count = count,
            Percent = percent,
            SalaryN = salaryN,
            Pct25 = pct25,
            Median = median,
            Pct75 = pct75,
            Mean = mean,
        };

    private static SchoolReportSection Section(string analvar, int count, decimal percent) =>
        new()
        {
            Analvar = analvar,
            Details = [],
            SubtotalCount = count,
            SubtotalPercent = percent,
        };
}
