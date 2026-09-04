using AccessibleSchoolReports.Application.Reporting;
using AccessibleSchoolReports.Infrastructure.Pdf;
using AccessibleSchoolReports.UnitTests.Reporting;
using UglyToad.PdfPig;

namespace AccessibleSchoolReports.UnitTests.Pdf;

public sealed class QuestPdfAccessiblePdfGeneratorTests
{
    private readonly IAccessiblePdfGenerator _generator = new QuestPdfAccessiblePdfGenerator();

    [Fact]
    public void Generate_WritesPdfWithTitleLanguageAndSevenPages()
    {
        var bytes = _generator.Generate(FixtureReport());

        Assert.True(bytes.Length > 100);
        Assert.Equal("%PDF", System.Text.Encoding.ASCII.GetString(bytes, 0, 4));

        using var document = PdfDocument.Open(new MemoryStream(bytes));
        Assert.Equal(7, document.NumberOfPages);
        Assert.Equal("Test University School of Law — Class of 2025 Summary Report", document.Information.Title);
    }

    [Fact]
    public void Generate_ContainsBusinessContent_InReadingOrder()
    {
        var text = ExtractText(_generator.Generate(FixtureReport()));

        Assert.Contains("Test University School of Law", text);
        Assert.Contains("Class of 2025 Summary Report", text);
        Assert.Contains("Total Reported = 100", text);
        Assert.Contains("Gender Reported", text);
        Assert.Contains("Women", text);
        Assert.Contains("46.0", text);
        Assert.Contains("Men of Color", text);
        Assert.Contains("Full-time Long-term Salaries", text);
        Assert.Contains("Number of Jobs Reported as:", text);
        Assert.Contains("Duration of Jobs by Employer Type", text);
        Assert.Contains("Long-term", text);
        Assert.Contains("(1+ years)", text);
        Assert.Contains("At least five salaries are required", text);
        Assert.Contains(SchoolReportPresentation.PreparedLine, text);
        Assert.Contains(SchoolReportPresentation.FooterUrl, text);
        Assert.DoesNotContain("NALP", text, StringComparison.Ordinal);
        Assert.DoesNotContain("ABA", text, StringComparison.Ordinal);
        Assert.DoesNotContain("nalp.org", text, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("25th", text);
        Assert.Contains("Percentile", text);
    }

    [Fact]
    public void Generate_PageOne_ReadsSchoolNameThenHeadersThenTotalThenTablesThenNoteThenLink()
    {
        var pageOne = ExtractPageText(_generator.Generate(FixtureReport()), pageNumber: 1);

        Assert.True(
            IndexOf(pageOne, "Test University School of Law") < IndexOf(pageOne, "Full-time Long-term Salaries"),
            "School name should be announced before the table headers.");
        Assert.True(
            IndexOf(pageOne, "Full-time Long-term Salaries") < IndexOf(pageOne, "Total Reported = 100"),
            "SAS places Total Reported under the column headers.");
        Assert.True(
            IndexOf(pageOne, "Total Reported = 100") < IndexOf(pageOne, "Gender Reported"),
            "Total Reported should be announced before the first data section.");
        Assert.True(
            IndexOf(pageOne, "Gender Reported") < IndexOf(pageOne, "At least five salaries are required"),
            "Table sections should be announced before the page note.");
        Assert.True(
            IndexOf(pageOne, "At least five salaries are required") < IndexOf(pageOne, SchoolReportPresentation.FooterUrl),
            "The page note should be announced before the test-client footer.");
    }

    [Fact]
    public void Generate_PrintsNalpFooterOncePerPage_NotDuplicated()
    {
        var text = ExtractText(_generator.Generate(FixtureReport()));
        var prepared = CountOccurrences(text, SchoolReportPresentation.PreparedLine);

        Assert.Equal(7, prepared);
        Assert.DoesNotContain("#084C9E", text);
        Assert.DoesNotContain("shown in red", text, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void Generate_WritesTaggedPdfMarkers_WithoutCertifyingAccessibility()
    {
        var pdf = System.Text.Encoding.ASCII.GetString(_generator.Generate(FixtureReport()));

        Assert.Contains("/StructTreeRoot", pdf);
        Assert.Contains("/Lang", pdf);
        Assert.Contains("en-US", pdf);
        Assert.Contains("pdfuaid", pdf);
    }

    [Fact]
    public void Generate_OmitsAbsentEducationJobsHeading()
    {
        var text = ExtractText(_generator.Generate(FixtureReport()));

        Assert.DoesNotContain("Education Jobs", text);
        Assert.Contains("Business Jobs", text);
    }

    [Fact]
    public void Generate_UsesMeaningfulMissingText_NotColorAlone()
    {
        var text = ExtractText(_generator.Generate(FixtureReport()));

        Assert.Contains("Men of Color", text);
        Assert.Contains(".", text);
        Assert.DoesNotContain("shown in red", text, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("shown in green", text, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void Generate_RendersCalculatorOutput_WithoutChangingCounts()
    {
        var calculator = new SchoolReportCalculator();
        var report = calculator.Calculate(
            "10701",
            [
                GraduateFactory.Create(sex3: "F", salFtPerm: 80000),
                GraduateFactory.Create(sex3: "M", salFtPerm: 90000),
                GraduateFactory.Create(sex3: "F", salFtPerm: 85000),
                GraduateFactory.Create(sex3: "M", salFtPerm: 95000),
                GraduateFactory.Create(sex3: "F", salFtPerm: 70000),
            ]);
        report = new SchoolReport
        {
            SchoolCode = report.SchoolCode,
            SchoolName = "Sample Law School",
            Rows = report.Rows,
            Sections = report.Sections,
        };

        var text = ExtractText(_generator.Generate(report));

        Assert.Contains("Sample Law School", text);
        Assert.Contains("Total Reported = 5", text);
        Assert.Contains("Women", text);
        Assert.Contains("Men", text);
        Assert.DoesNotContain("Education Jobs", text);
    }

    [Fact]
    public void Generate_WritesToStream()
    {
        using var stream = new MemoryStream();
        _generator.Generate(FixtureReport(), stream);

        Assert.True(stream.Length > 100);
        Assert.Equal((byte)'%', stream.ToArray()[0]);
    }

    private static string ExtractText(byte[] pdf)
    {
        using var document = PdfDocument.Open(new MemoryStream(pdf));
        return string.Join(
            "\n",
            document.GetPages().Select(page => string.Join(" ", page.GetWords().Select(word => word.Text))));
    }

    private static string ExtractPageText(byte[] pdf, int pageNumber)
    {
        using var document = PdfDocument.Open(new MemoryStream(pdf));
        var page = document.GetPage(pageNumber);
        return string.Join(" ", page.GetWords().Select(word => word.Text));
    }

    private static int CountOccurrences(string text, string value)
    {
        var count = 0;
        var start = 0;
        while (true)
        {
            var index = text.IndexOf(value, start, StringComparison.Ordinal);
            if (index < 0)
            {
                return count;
            }

            count++;
            start = index + value.Length;
        }
    }

    private static int IndexOf(string text, string value)
    {
        var index = text.IndexOf(value, StringComparison.Ordinal);
        Assert.True(index >= 0, $"Expected to find '{value}' in extracted page text.");
        return index;
    }

    private static SchoolReport FixtureReport()
    {
        return new SchoolReport
        {
            SchoolCode = "99999",
            SchoolName = "Test University School of Law",
            Rows =
            [
                new() { Analvar = "A", Newvar = "A", Count = 100, Percent = 100m },
                new() { Analvar = "B", Newvar = "F", Count = 46, Percent = 46m, SalaryN = 40, Pct25 = 70000, Median = 85500, Pct75 = 110000, Mean = 85802 },
                new() { Analvar = "B", Newvar = "M", Count = 54, Percent = 54m },
                new() { Analvar = "C1", Newvar = "MINORM", Count = 4, Percent = 6.3m },
                new() { Analvar = "D1", Newvar = "EMPL", Count = 93, Percent = 93m },
                new() { Analvar = "E3", Newvar = "1-LJD", Count = 2, Percent = 14.3m },
                new() { Analvar = "DURATION", Newvar = "FIRM", DurationCounts = new Dictionary<string, int> { ["PERM"] = 50 } },
            ],
            Sections =
            [
                new() { Analvar = "A", Details = [], SubtotalCount = 100, SubtotalPercent = 100m },
                new() { Analvar = "B", Details = [], SubtotalCount = 100, SubtotalPercent = 100m },
                new() { Analvar = "C1", Details = [], SubtotalCount = 4, SubtotalPercent = 6.3m },
                new() { Analvar = "D1", Details = [], SubtotalCount = 93, SubtotalPercent = 93m },
                new() { Analvar = "E3", Details = [], SubtotalCount = 2, SubtotalPercent = 14.3m },
                new() { Analvar = "DURATION", Details = [], SubtotalCount = 50, SubtotalPercent = 0m },
            ],
        };
    }
}
