using AccessibleSchoolReports.Application.Reporting;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace AccessibleSchoolReports.Infrastructure.Pdf;

/// <summary>
/// Renders the SAS %SCHRPTS / GrayscalePrinter page layout as a tagged PDF/UA-1 target.
/// Visual design follows legacy/baseline/test-school-report.pdf. Tags do not restyle the page.
/// Generation is not a validation result and does not certify accessibility.
/// </summary>
public sealed class QuestPdfAccessiblePdfGenerator : IAccessiblePdfGenerator
{
    private static readonly Color TextColor = Colors.Black;
    private static readonly Color HeaderFill = Color.FromHex("#BBBBBB");
    private static readonly Color RuleColor = Colors.Black;
    private static readonly string[] FontStack = ["Times New Roman", "Times", "Liberation Serif", "Thorndale AMT", "Lato"];
    private const float LabelWidth = 126f;
    private const float DurationLabelWidth = 144f;
    private const float MeasureWidth = 54f;
    private const string FooterUrl = SchoolReportPresentation.FooterUrl + ".";

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
        var created = DateTime.UtcNow;
        Document
            .Create(container => Compose(container, printable))
            .WithMetadata(new DocumentMetadata
            {
                Title = printable.DocumentTitle,
                Author = "Accessible School Report Modernizer",
                Subject = "Meridian Test Client school employment summary",
                Keywords = "school report, employment, test client, Class of 2025",
                Language = printable.Language,
                Creator = "AccessibleSchoolReports",
                CreationDate = created,
                ModifiedDate = created,
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
                pdfPage.MarginHorizontal(51);
                pdfPage.MarginTop(10);
                pdfPage.MarginBottom(24);
                pdfPage.DefaultTextStyle(text => text
                    .FontSize(8.2f)
                    .LineHeight(1.05f)
                    .FontColor(TextColor)
                    .FontFamily(FontStack));

                pdfPage.Content()
                    .SemanticLanguage(report.Language)
                    .SemanticArticle()
                    .Element(content => ComposeContent(content, report, page));
            });
        }
    }

    private static void ComposeContent(IContainer container, PrintableReport report, PrintablePage page)
    {
        container.Column(column =>
        {
            column.Spacing(0);

            if (page.Number == 1)
            {
                column.Item()
                    .SemanticHeader1()
                    .AlignCenter()
                    .DefaultTextStyle(TitleStyle)
                    .Text(report.SchoolName);
            }
            else
            {
                column.Item()
                    .SemanticIgnore()
                    .AlignCenter()
                    .DefaultTextStyle(TitleStyle)
                    .Text(report.SchoolName);
            }

            column.Item()
                .SemanticHeader2()
                .AlignCenter()
                .DefaultTextStyle(TitleStyle)
                .Text(page.Heading);

            column.Item().PaddingTop(16).Element(table => ComposePageTable(table, report, page));

            column.Item()
                .PaddingTop(8)
                .SemanticParagraph()
                .DefaultTextStyle(text => text.FontSize(9.2f).FontFamily(FontStack).FontColor(TextColor))
                .Text(NoteText(page.Note));

            column.Item().PaddingTop(14).Element(footer => ComposeNalpFooter(footer, tagged: page.Number == 1));
        });
    }

    private static void ComposePageTable(IContainer container, PrintableReport report, PrintablePage page)
    {
        var columnCount = ColumnCount(page.TableKind);
        container.SemanticTable().Table(table =>
        {
            table.ColumnsDefinition(columns =>
            {
                if (page.TableKind == PrintableTableKind.Duration)
                {
                    columns.ConstantColumn(DurationLabelWidth);
                    columns.ConstantColumn(MeasureWidth);
                    columns.ConstantColumn(MeasureWidth);
                    return;
                }

                columns.ConstantColumn(LabelWidth);
                for (var i = 1; i < columnCount; i++)
                {
                    columns.ConstantColumn(MeasureWidth);
                }
            });

            table.Header(header => WriteHeader(header, page.TableKind));

            if (page.Number == 1 && report.TotalReported is not null)
            {
                table.Cell()
                    .ColumnSpan((uint)columnCount)
                    .Element(BannerCell)
                    .SemanticParagraph()
                    .Text($"Total Reported = {SchoolReportPresentation.FormatCount(report.TotalReported)}")
                    .Bold();
            }

            foreach (var section in page.Sections)
            {
                table.Cell()
                    .ColumnSpan((uint)columnCount)
                    .Element(SectionBanner)
                    .SemanticHeader3()
                    .Text(section.Heading)
                    .Bold();

                foreach (var row in section.Rows)
                {
                    WriteRow(table, page.TableKind, row, headerRow: false);
                }

                WriteRow(table, page.TableKind, section.Subtotal, headerRow: true);
            }
        });
    }

    private static void WriteHeader(TableCellDescriptor header, PrintableTableKind kind)
    {
        if (kind == PrintableTableKind.Salary)
        {
            header.Cell().ColumnSpan(3).Element(HeaderCell).Text(" ");
            header.Cell().ColumnSpan(5).Element(HeaderCell).AlignCenter().Text("Full-time Long-term Salaries").Bold();
            header.Cell().Element(HeaderCell).Text(" ");
            header.Cell().Element(HeaderCell).AlignCenter().Text("Number\nReported").Bold();
            header.Cell().Element(HeaderCell).AlignCenter().Text("% of\nReported").Bold();
            header.Cell().Element(HeaderCell).AlignCenter().Text("# with\nSalary").Bold();
            header.Cell().Element(HeaderCell).AlignCenter().Text("25th\nPercentile").Bold();
            header.Cell().Element(HeaderCell).AlignCenter().Text("Median").Bold();
            header.Cell().Element(HeaderCell).AlignCenter().Text("75th\nPercentile").Bold();
            header.Cell().Element(HeaderCell).AlignCenter().Text("Mean").Bold();
            return;
        }

        if (kind == PrintableTableKind.Duration)
        {
            header.Cell().Element(HeaderCell).Text(" ");
            header.Cell().ColumnSpan(2).Element(HeaderCell).AlignCenter().Text("Number of Jobs Reported as:").Bold();
            header.Cell().Element(HeaderCell).Text(" ");
            header.Cell().Element(HeaderCell).AlignCenter().Text("Long-term\n(1+ years)").Bold();
            header.Cell().Element(HeaderCell).AlignCenter().Text("Short-term\n(Less than 1 year)").Bold();
            return;
        }

        header.Cell().Element(HeaderCell).Text(" ");
        header.Cell().Element(HeaderCell).AlignCenter().Text("Number\nReported").Bold();
        header.Cell().Element(HeaderCell).AlignCenter().Text("% of\nReported").Bold();
    }

    private static void WriteRow(TableDescriptor table, PrintableTableKind kind, PrintableRow row, bool headerRow)
    {
        table.Cell()
            .AsSemanticHorizontalHeader()
            .Element(cell =>
            {
                var body = StyledCell(cell, headerRow);
                if (headerRow)
                {
                    body = body.PaddingLeft(20);
                }

                var text = body.Text(row.Label);
                if (headerRow)
                {
                    text.Bold();
                }
            });

        foreach (var value in Values(kind, row))
        {
            table.Cell().Element(cell => WriteValue(StyledCell(cell, headerRow).AlignCenter(), value, headerRow));
        }
    }

    private static void WriteValue(IContainer container, string value, bool headerRow)
    {
        if (value == SchoolReportPresentation.NotDisplayed)
        {
            container.SemanticSpan(SchoolReportPresentation.NotDisplayedAccessibleName).Text(value);
            return;
        }

        var text = container.Text(value);
        if (headerRow)
        {
            text.Bold();
        }
    }

    private static IEnumerable<string> Values(PrintableTableKind kind, PrintableRow row) => kind switch
    {
        PrintableTableKind.CountPercent => [row.Count, row.Percent],
        PrintableTableKind.Duration => [row.LongTerm, row.ShortTerm],
        _ => [row.Count, row.Percent, row.SalaryN, row.Pct25, row.Median, row.Pct75, row.Mean],
    };

    private static int ColumnCount(PrintableTableKind kind) => kind switch
    {
        PrintableTableKind.CountPercent => 3,
        PrintableTableKind.Duration => 3,
        _ => 8,
    };

    private static string NoteText(string note)
    {
        if (string.IsNullOrWhiteSpace(note))
        {
            return string.Empty;
        }

        return note.StartsWith("Note:", StringComparison.Ordinal) ? note : $"Note: {note}";
    }

    private static void ComposeNalpFooter(IContainer container, bool tagged)
    {
        var block = tagged ? container : container.SemanticIgnore();
        block
            .DefaultTextStyle(text => text.FontSize(10.1f).FontColor(TextColor).FontFamily(FontStack))
            .Column(column =>
            {
                column.Item().AlignCenter().Element(item =>
                {
                    if (tagged)
                    {
                        item.SemanticParagraph().Text(SchoolReportPresentation.PreparedLine);
                    }
                    else
                    {
                        item.Text(SchoolReportPresentation.PreparedLine);
                    }
                });
                column.Item().AlignCenter().Element(item =>
                {
                    if (tagged)
                    {
                        item.SemanticParagraph().Text(SchoolReportPresentation.Disclaimer);
                    }
                    else
                    {
                        item.Text(SchoolReportPresentation.Disclaimer);
                    }
                });
                column.Item().AlignCenter().Element(item =>
                {
                    if (tagged)
                    {
                        item.SemanticLink(SchoolReportPresentation.FooterLinkName)
                            .Hyperlink(SchoolReportPresentation.FooterUrlHref)
                            .Text(FooterUrl);
                    }
                    else
                    {
                        item.Text(FooterUrl);
                    }
                });
            });
    }

    private static TextStyle TitleStyle(TextStyle text) =>
        text.FontSize(13).Bold().Italic().FontColor(TextColor).FontFamily(FontStack);

    private static IContainer HeaderCell(IContainer container) =>
        container
            .Border(0.5f)
            .BorderColor(RuleColor)
            .Background(HeaderFill)
            .PaddingVertical(3)
            .PaddingHorizontal(2)
            .DefaultTextStyle(text => text.FontSize(9.2f).FontColor(TextColor).FontFamily(FontStack).Bold());

    private static IContainer BannerCell(IContainer container) =>
        container
            .Border(0.5f)
            .BorderColor(RuleColor)
            .PaddingVertical(6)
            .PaddingHorizontal(4)
            .DefaultTextStyle(text => text.FontSize(8.2f).FontColor(TextColor).FontFamily(FontStack).Bold());

    private static IContainer SectionBanner(IContainer container) =>
        container
            .Border(0.5f)
            .BorderColor(RuleColor)
            .PaddingVertical(4)
            .PaddingHorizontal(4)
            .DefaultTextStyle(text => text.FontSize(8.2f).FontColor(TextColor).FontFamily(FontStack).Bold());

    private static IContainer StyledCell(IContainer container, bool headerRow) =>
        container
            .Border(0.5f)
            .BorderColor(RuleColor)
            .PaddingVertical(2)
            .PaddingHorizontal(2)
            .DefaultTextStyle(text => text
                .FontSize(8.2f)
                .FontColor(TextColor)
                .FontFamily(FontStack)
                .Weight(headerRow ? FontWeight.Bold : FontWeight.Normal));
}
