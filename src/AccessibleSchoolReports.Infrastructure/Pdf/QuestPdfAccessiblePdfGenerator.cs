using AccessibleSchoolReports.Application.Reporting;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace AccessibleSchoolReports.Infrastructure.Pdf;

public sealed class QuestPdfAccessiblePdfGenerator : IAccessiblePdfGenerator
{
    private static readonly Color TextColor = Colors.Grey.Darken4;
    private static readonly Color HeaderFill = Colors.Grey.Lighten3;
    private static readonly Color BorderColor = Colors.Grey.Darken1;

    static QuestPdfAccessiblePdfGenerator()
    {
        QuestPDF.Settings.License = LicenseType.Community;
    }

    public byte[] Generate(SchoolReport report)
    {
        using var stream = new MemoryStream();
        Generate(report, stream);
        return stream.ToArray();
    }

    public void Generate(SchoolReport report, Stream output)
    {
        ArgumentNullException.ThrowIfNull(report);
        ArgumentNullException.ThrowIfNull(output);

        var printable = SchoolReportLayout.Compose(report);
        Document
            .Create(container => Compose(container, printable))
            .WithMetadata(new DocumentMetadata
            {
                Title = printable.DocumentTitle,
                Author = "Accessible School Report Modernizer",
                Subject = "NALP ERSS school employment summary",
                Language = printable.Language,
                Creator = "AccessibleSchoolReports",
            })
            .WithSettings(new DocumentSettings
            {
                PDFA_Conformance = PDFA_Conformance.PDFA_3A,
                PDFUA_Conformance = PDFUA_Conformance.PDFUA_1,
            })
            .GeneratePdf(output);
    }

    private static void Compose(IDocumentContainer document, PrintableReport report)
    {
        foreach (var page in report.Pages)
        {
            document.Page(pdfPage =>
            {
                pdfPage.Size(PageSizes.Letter);
                pdfPage.Margin(36);
                pdfPage.DefaultTextStyle(text => text.FontSize(9).FontColor(TextColor).FontFamily(Fonts.Calibri));

                pdfPage.Header().Element(header => ComposeHeader(header, report, page));
                pdfPage.Content().Element(content => ComposeContent(content, page));
                pdfPage.Footer().Element(ComposeFooter);
            });
        }
    }

    private static void ComposeHeader(IContainer container, PrintableReport report, PrintablePage page)
    {
        container.SemanticSection().Column(column =>
        {
            column.Item()
                .SemanticHeader1()
                .Text(report.SchoolName)
                .FontSize(14)
                .Bold();
            column.Item()
                .PaddingTop(2)
                .SemanticHeader2()
                .Text(page.Heading)
                .FontSize(12)
                .Bold();
            if (page.Number == 1 && report.TotalReported is not null)
            {
                column.Item()
                    .PaddingTop(8)
                    .SemanticParagraph()
                    .Text($"Total Reported = {SchoolReportPresentation.FormatCount(report.TotalReported)}")
                    .FontSize(11)
                    .Bold();
            }
        });
    }

    private static void ComposeContent(IContainer container, PrintablePage page)
    {
        container.PaddingTop(10).Column(column =>
        {
            column.Spacing(10);
            if (page.Sections.Count == 0)
            {
                column.Item()
                    .SemanticParagraph()
                    .Text("No rows were stored for the sections that appear on this page.");
            }

            foreach (var section in page.Sections)
            {
                column.Item().Element(item => ComposeSection(item, page, section));
            }

            column.Item()
                .PaddingTop(6)
                .SemanticParagraph()
                .Text(page.Note);
        });
    }

    private static void ComposeSection(IContainer container, PrintablePage page, PrintableSection section)
    {
        container.SemanticSection().Column(column =>
        {
            column.Item()
                .SemanticHeader3()
                .Text(section.Heading)
                .FontSize(11)
                .Bold();

            if (page.TableKind == PrintableTableKind.Salary)
            {
                column.Item()
                    .PaddingTop(2)
                    .SemanticCaption()
                    .Text(SchoolReportPresentation.SalaryGroupCaption)
                    .Italic();
            }

            column.Item().PaddingTop(4).Element(table => ComposeTable(table, page.TableKind, section));
        });
    }

    private static void ComposeTable(IContainer container, PrintableTableKind kind, PrintableSection section)
    {
        var headers = Headers(kind);
        container.SemanticTable().Table(table =>
        {
            table.ColumnsDefinition(columns =>
            {
                columns.RelativeColumn(2.4f);
                for (var i = 1; i < headers.Length; i++)
                {
                    columns.RelativeColumn();
                }
            });

            table.Header(header =>
            {
                foreach (var title in headers)
                {
                    header.Cell().Element(HeaderCell).Text(title).Bold();
                }
            });

            foreach (var row in section.Rows)
            {
                WriteRow(table, kind, row, headerRow: false);
            }

            WriteRow(table, kind, section.Subtotal, headerRow: true);
        });
    }

    private static void WriteRow(TableDescriptor table, PrintableTableKind kind, PrintableRow row, bool headerRow)
    {
        table.Cell().AsSemanticHorizontalHeader().Element(cell => StyledCell(cell, headerRow).Text(row.Label).Bold());
        foreach (var value in Values(kind, row))
        {
            table.Cell().Element(cell => StyledCell(cell, headerRow).AlignRight().Text(value));
        }
    }

    private static IContainer StyledCell(IContainer container, bool headerRow) =>
        headerRow ? HeaderCell(container) : BodyCell(container);

    private static string[] Headers(PrintableTableKind kind) => kind switch
    {
        PrintableTableKind.CountPercent =>
        [
            "Category",
            "Number Reported",
            "% of Reported",
        ],
        PrintableTableKind.Duration =>
        [
            "Category",
            "Long-term (1+ years)",
            "Short-term (Less than 1 year)",
        ],
        _ =>
        [
            "Category",
            "Number Reported",
            "% of Reported",
            "# with Salary",
            "25th Percentile",
            "Median",
            "75th Percentile",
            "Mean",
        ],
    };

    private static IEnumerable<string> Values(PrintableTableKind kind, PrintableRow row) => kind switch
    {
        PrintableTableKind.CountPercent => [row.Count, row.Percent],
        PrintableTableKind.Duration => [row.LongTerm, row.ShortTerm],
        _ => [row.Count, row.Percent, row.SalaryN, row.Pct25, row.Median, row.Pct75, row.Mean],
    };

    private static void ComposeFooter(IContainer container)
    {
        container.SemanticSection().Column(column =>
        {
            column.Item().SemanticParagraph().Text(SchoolReportPresentation.PreparedLine);
            column.Item().PaddingTop(2).SemanticParagraph().Text(SchoolReportPresentation.Disclaimer);
            column.Item()
                .PaddingTop(2)
                .SemanticLink("NALP ERSS information")
                .Hyperlink("https://www.nalp.org/erssinfo")
                .Text("www.nalp.org/erssinfo.");
        });
    }

    private static IContainer HeaderCell(IContainer container) =>
        container
            .Border(0.5f)
            .BorderColor(BorderColor)
            .Background(HeaderFill)
            .Padding(3);

    private static IContainer BodyCell(IContainer container) =>
        container
            .Border(0.5f)
            .BorderColor(BorderColor)
            .Padding(3);
}
