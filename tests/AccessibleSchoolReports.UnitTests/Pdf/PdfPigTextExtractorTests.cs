using AccessibleSchoolReports.Application.Knowledge;
using AccessibleSchoolReports.Application.Reporting;
using AccessibleSchoolReports.Infrastructure.Pdf;

namespace AccessibleSchoolReports.UnitTests.Pdf;

public sealed class PdfPigTextExtractorTests
{
    [Fact]
    public void Extract_KeepsPrintedRowsOnSeparateLines()
    {
        var report = new SchoolReport
        {
            SchoolCode = "10701",
            SchoolName = "School A",
            Rows = [new() { Analvar = "A", Newvar = "A", Count = 5, Percent = 100m }],
            Sections = [new() { Analvar = "A", Details = [], SubtotalCount = 5, SubtotalPercent = 100m }],
        };
        var bytes = new QuestPdfAccessiblePdfGenerator().Generate(report);
        using var stream = new MemoryStream(bytes);

        var extracted = new PdfPigTextExtractor().Extract(stream);

        Assert.Equal(PdfTextExtractionStatus.Succeeded, extracted.Status);
        Assert.True(extracted.Pages.Count >= 7, $"pages={extracted.Pages.Count}");
        var text = string.Join('\n', extracted.Pages.Select(page => page.Text));
        Assert.Contains("Class of 2025", text, StringComparison.Ordinal);
        Assert.Contains("Total Reported", text, StringComparison.Ordinal);
        Assert.Contains('\n', text);
        Assert.DoesNotContain('\0', text);
    }
}
