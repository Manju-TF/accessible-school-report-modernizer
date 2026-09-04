using AccessibleSchoolReports.Application.Reporting;
using AccessibleSchoolReports.Infrastructure.Pdf;
using AccessibleSchoolReports.UnitTests.Reporting;
using UglyToad.PdfPig;
using UglyToad.PdfPig.Content;

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
        Assert.Contains("Not displayed", text);
        Assert.Contains("Full-time Long-term Salaries", text);
        Assert.Contains("Duration of Jobs by Employer Type", text);
        Assert.Contains("Long-term (1+ years)", text);
        Assert.Contains("At least five salaries are required", text);
        Assert.Contains("Table prepared by NALP, July 2026", text);
        Assert.Contains("www.nalp.org/erssinfo", text);
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

        Assert.Contains("Not displayed", text);
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
