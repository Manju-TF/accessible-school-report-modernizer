namespace AccessibleSchoolReports.Application.Reporting;

public enum PrintableTableKind
{
    Salary,
    CountPercent,
    Duration,
}

public sealed class PrintableReport
{
    public required string SchoolCode { get; init; }

    public required string SchoolName { get; init; }

    public required string DocumentTitle { get; init; }

    public required string Language { get; init; }

    public int? TotalReported { get; init; }

    public required IReadOnlyList<PrintablePage> Pages { get; init; }
}

public sealed class PrintablePage
{
    public required int Number { get; init; }

    public required string Heading { get; init; }

    public required string Note { get; init; }

    public required PrintableTableKind TableKind { get; init; }

    public required IReadOnlyList<PrintableSection> Sections { get; init; }
}

public sealed class PrintableSection
{
    public required string Analvar { get; init; }

    public required string Heading { get; init; }

    public required string SubtotalLabel { get; init; }

    public required IReadOnlyList<PrintableRow> Rows { get; init; }

    public required PrintableRow Subtotal { get; init; }
}

public sealed class PrintableRow
{
    public required string Label { get; init; }

    public string Count { get; init; } = SchoolReportPresentation.NotDisplayed;

    public string Percent { get; init; } = SchoolReportPresentation.NotDisplayed;

    public string SalaryN { get; init; } = SchoolReportPresentation.NotDisplayed;

    public string Pct25 { get; init; } = SchoolReportPresentation.NotDisplayed;

    public string Median { get; init; } = SchoolReportPresentation.NotDisplayed;

    public string Pct75 { get; init; } = SchoolReportPresentation.NotDisplayed;

    public string Mean { get; init; } = SchoolReportPresentation.NotDisplayed;

    public string LongTerm { get; init; } = SchoolReportPresentation.NotDisplayed;

    public string ShortTerm { get; init; } = SchoolReportPresentation.NotDisplayed;
}

/// <summary>
/// Maps a calculated <see cref="SchoolReport"/> onto SAS page slices and labels.
/// Does not recompute business values.
/// </summary>
public static class SchoolReportLayout
{
    public static PrintableReport Compose(SchoolReport report)
    {
        ArgumentNullException.ThrowIfNull(report);

        var schoolName = string.IsNullOrWhiteSpace(report.SchoolName)
            ? report.SchoolCode
            : report.SchoolName.Trim();
        var total = report.Rows.FirstOrDefault(row => row.Analvar == "A" && row.Newvar == "A")?.Count;

        return new PrintableReport
        {
            SchoolCode = report.SchoolCode,
            SchoolName = schoolName,
            DocumentTitle = $"{schoolName} — {SchoolReportPresentation.ClassYearTitle}",
            Language = SchoolReportPresentation.Language,
            TotalReported = total,
            Pages =
            [
                Page(1, PrintableTableKind.Salary, SchoolReportPresentation.Page1Analvars, report),
                Page(2, PrintableTableKind.Salary, Page2Analvars(report), report),
                Page(3, PrintableTableKind.Salary, SchoolReportPresentation.Page3Analvars, report),
                Page(4, PrintableTableKind.Salary, SchoolReportPresentation.Page4Analvars, report),
                Page(5, PrintableTableKind.Salary, SchoolReportPresentation.Page5Analvars, report),
                Page(6, PrintableTableKind.CountPercent, SchoolReportPresentation.Page6Analvars, report),
                Page(7, PrintableTableKind.Duration, SchoolReportPresentation.Page7Analvars, report),
            ],
        };
    }

    private static IReadOnlyList<string> Page2Analvars(SchoolReport report) =>
        report.Sections
            .Select(section => section.Analvar)
            .Where(SchoolReportPresentation.IsPage2Analvar)
            .Distinct(StringComparer.Ordinal)
            .OrderBy(analvar => analvar, StringComparer.Ordinal)
            .ToArray();

    private static PrintablePage Page(
        int number,
        PrintableTableKind kind,
        IReadOnlyList<string> analvars,
        SchoolReport report)
    {
        var sections = analvars
            .Select(analvar => Section(analvar, kind, report))
            .Where(section => section is not null)
            .Cast<PrintableSection>()
            .ToArray();

        return new PrintablePage
        {
            Number = number,
            Heading = SchoolReportPresentation.PageHeading(number),
            Note = SchoolReportPresentation.PageNote(number),
            TableKind = kind,
            Sections = sections,
        };
    }

    private static PrintableSection? Section(string analvar, PrintableTableKind kind, SchoolReport report)
    {
        var rows = report.Rows
            .Where(row => row.Analvar == analvar)
            .Where(row => analvar != "DURATION" || !string.IsNullOrEmpty(row.Newvar))
            .OrderBy(row => row.Newvar, StringComparer.Ordinal)
            .Select(row => MapRow(row, kind))
            .ToArray();
        if (rows.Length == 0)
        {
            return null;
        }

        return new PrintableSection
        {
            Analvar = analvar,
            Heading = SchoolReportPresentation.SectionHeading(analvar),
            SubtotalLabel = SchoolReportPresentation.SubtotalLabel(analvar),
            Rows = rows,
            Subtotal = Subtotal(analvar, kind, rows, report),
        };
    }

    private static PrintableRow MapRow(SchoolReportRow row, PrintableTableKind kind)
    {
        var label = SchoolReportPresentation.RowLabel(row.Analvar, row.Newvar);
        if (row.Analvar == "LAW SCHOOL FUNDED")
        {
            return new PrintableRow
            {
                Label = label,
                LongTerm = SchoolReportPresentation.FormatCount(row.Count),
                ShortTerm = SchoolReportPresentation.NotDisplayed,
            };
        }

        if (kind == PrintableTableKind.Duration)
        {
            return new PrintableRow
            {
                Label = label,
                LongTerm = DurationValue(row, "perm"),
                ShortTerm = DurationValue(row, "temp"),
            };
        }

        return new PrintableRow
        {
            Label = label,
            Count = SchoolReportPresentation.FormatCount(row.Count),
            Percent = SchoolReportPresentation.FormatPercent(row.Percent),
            SalaryN = SchoolReportPresentation.FormatCount(row.SalaryN),
            Pct25 = SchoolReportPresentation.FormatMoney(row.Pct25),
            Median = SchoolReportPresentation.FormatMoney(row.Median),
            Pct75 = SchoolReportPresentation.FormatMoney(row.Pct75),
            Mean = SchoolReportPresentation.FormatMoney(row.Mean),
        };
    }

    private static PrintableRow Subtotal(
        string analvar,
        PrintableTableKind kind,
        IReadOnlyList<PrintableRow> rows,
        SchoolReport report)
    {
        if (kind == PrintableTableKind.Duration || analvar == "LAW SCHOOL FUNDED")
        {
            var overall = report.Rows.FirstOrDefault(row => row.Analvar == analvar && string.IsNullOrEmpty(row.Newvar));
            return new PrintableRow
            {
                Label = SchoolReportPresentation.SubtotalLabel(analvar),
                LongTerm = overall is not null
                    ? DurationValue(overall, "perm")
                    : SumDisplayed(rows.Select(row => row.LongTerm)),
                ShortTerm = overall is not null
                    ? DurationValue(overall, "temp")
                    : SumDisplayed(rows.Select(row => row.ShortTerm)),
            };
        }

        var section = report.Sections.FirstOrDefault(item => item.Analvar == analvar);
        return new PrintableRow
        {
            Label = SchoolReportPresentation.SubtotalLabel(analvar),
            Count = SchoolReportPresentation.FormatCount(section?.SubtotalCount),
            Percent = analvar == "JOBREG3"
                ? SchoolReportPresentation.NotDisplayed
                : SchoolReportPresentation.FormatPercent(section?.SubtotalPercent),
        };
    }

    private static string DurationValue(SchoolReportRow row, string code)
    {
        if (row.DurationCounts is null)
        {
            return SchoolReportPresentation.NotDisplayed;
        }

        var match = row.DurationCounts.FirstOrDefault(item =>
            string.Equals(item.Key, code, StringComparison.OrdinalIgnoreCase));
        return string.IsNullOrEmpty(match.Key)
            ? SchoolReportPresentation.NotDisplayed
            : SchoolReportPresentation.FormatCount(match.Value);
    }

    private static string SumDisplayed(IEnumerable<string> values)
    {
        var total = 0;
        var any = false;
        foreach (var value in values)
        {
            if (value == SchoolReportPresentation.NotDisplayed)
            {
                continue;
            }

            if (int.TryParse(value.Replace(",", string.Empty, StringComparison.Ordinal), out var number))
            {
                total += number;
                any = true;
            }
        }

        return any ? SchoolReportPresentation.FormatCount(total) : SchoolReportPresentation.NotDisplayed;
    }
}
