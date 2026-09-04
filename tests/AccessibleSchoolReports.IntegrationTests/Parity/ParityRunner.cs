using AccessibleSchoolReports.Application.Reporting;
using AccessibleSchoolReports.Domain.Entities;
using AccessibleSchoolReports.Infrastructure.Import;
using Microsoft.EntityFrameworkCore;

namespace AccessibleSchoolReports.IntegrationTests.Parity;

internal sealed record ParityRun(
    string? ModernSchoolCode,
    int ModernGraduateCount,
    int SampleSchoolCount,
    int LargestSampleGraduateCount,
    int MatchCount,
    int MismatchCount,
    int UnresolvedCount,
    string SelectionReason,
    IReadOnlyList<ParityObservation> Observations);

internal static class ParityRunner
{
    private static readonly IReadOnlyList<LegacyExpectedMetric> Catalog = LegacyExpectedSchoolReport.All();

    public static async Task<ParityRun> RunAsync()
    {
        await using var db = await SqliteTestDatabase.CreateAsync();
        var importer = new ExcelGraduateImportService(db.Context);
        await using (var stream = File.OpenRead(FindSampleExport()))
        {
            await importer.ImportAsync(stream, "sample-export.xlsx");
        }

        var calculator = new SchoolReportCalculator();
        var bySchool = await db.Context.GraduateRecords
            .AsNoTracking()
            .Include(record => record.School)
            .ToListAsync();
        var reports = bySchool
            .GroupBy(record => record.School.Code)
            .Select(group => calculator.Calculate(group.Key, group.ToList()))
            .ToArray();

        var (chosen, reason) = ChooseSchool(reports);
        var observations = Catalog
            .Select(metric => Observe(metric, chosen))
            .ToArray();
        var graduateCounts = reports
            .Select(report => Value(report, "A", "A", ParityField.Count) ?? 0)
            .ToArray();

        return new ParityRun(
            chosen?.SchoolCode,
            chosen?.Rows.FirstOrDefault(row => row.Analvar == "A")?.Count ?? 0,
            reports.Length,
            graduateCounts.Length == 0 ? 0 : (int)graduateCounts.Max(),
            observations.Count(item => item.Status == ParityStatus.Match),
            observations.Count(item => item.Status == ParityStatus.Mismatch),
            observations.Count(item => item.Status == ParityStatus.Unresolved),
            reason,
            observations);
    }

    public static string RenderMarkdown(ParityRun run)
    {
        var matchedRules = run.Observations.Where(item => item.Status == ParityStatus.Match).Select(item => item.Metric.RuleId).Distinct().OrderBy(x => x);
        var mismatchedRules = run.Observations.Where(item => item.Status == ParityStatus.Mismatch).Select(item => item.Metric.RuleId).Distinct().OrderBy(x => x);
        var unresolvedRules = run.Observations.Where(item => item.Status == ParityStatus.Unresolved).Select(item => item.Metric.RuleId).Distinct().OrderBy(x => x);

        var writer = new StringWriter();
        writer.WriteLine("# Legacy vs modern parity results");
        writer.WriteLine();
        writer.WriteLine("Sources (read-only):");
        writer.WriteLine();
        writer.WriteLine("- `legacy/baseline/test-school-report.pdf` — expected business values from `docs/capstone/report-map.md`");
        writer.WriteLine("- `legacy/samples/sample-export.xlsx` — modern calculator input");
        writer.WriteLine();
        writer.WriteLine("This is value-level parity. No pixel comparison was done.");
        writer.WriteLine();
        writer.WriteLine($"**Result: FAIL.** {run.MismatchCount} numerical mismatches. `{nameof(LegacyModernParityTests)}` asserts zero mismatches.");
        writer.WriteLine();
        writer.WriteLine("## Subject");
        writer.WriteLine();
        writer.WriteLine($"- Legacy school name: `{LegacyExpectedSchoolReport.SchoolName}`");
        writer.WriteLine($"- Legacy class year on PDF: `{LegacyExpectedSchoolReport.PdfClassYear}`");
        writer.WriteLine("- Legacy school CODE: **unknown** (not on the PDF; not in the 2025 `%SCHRPTS` list)");
        writer.WriteLine($"- Modern school CODE used for comparison: `{run.ModernSchoolCode ?? "(none)"}`");
        writer.WriteLine($"- Modern graduate count (CF-C-01): {run.ModernGraduateCount}");
        writer.WriteLine($"- Sample schools imported: {run.SampleSchoolCount}");
        writer.WriteLine($"- Largest sample school graduate count: {run.LargestSampleGraduateCount}");
        writer.WriteLine($"- Selection: {run.SelectionReason}");
        writer.WriteLine();
        writer.WriteLine($"Compared {run.Observations.Count} characterized metrics. Match {run.MatchCount}. Mismatch {run.MismatchCount}. Unresolved {run.UnresolvedCount}.");
        writer.WriteLine();
        writer.WriteLine("## Matched rules");
        writer.WriteLine();
        WriteRuleList(writer, matchedRules, run.Observations, ParityStatus.Match);
        writer.WriteLine("## Mismatched rules");
        writer.WriteLine();
        WriteRuleList(writer, mismatchedRules, run.Observations, ParityStatus.Mismatch);
        writer.WriteLine("## Unresolved rules");
        writer.WriteLine();
        WriteRuleList(writer, unresolvedRules, run.Observations, ParityStatus.Unresolved);
        writer.WriteLine("## Metric table");
        writer.WriteLine();
        writer.WriteLine("| Status | Id | Rule | Analvar | Newvar | Field | Legacy | Modern | Explanation |");
        writer.WriteLine("|---|---|---|---|---|---|---|---|---|");
        foreach (var item in run.Observations)
        {
            writer.WriteLine($"| {item.Status} | `{item.Metric.Id}` | {item.Metric.RuleId} | {item.Metric.Analvar} | {item.Metric.Newvar} | {item.Metric.Field} | {Fmt(Normalize(item.Metric.Field, item.Metric.Expected))} | {Fmt(Normalize(item.Metric.Field, item.Modern))} | {Escape(item.Explanation)} |");
        }

        writer.WriteLine();
        writer.WriteLine("## Explanations");
        writer.WriteLine();
        writer.WriteLine($"1. The baseline PDF is a Test University Class of **2024** artifact. The sample export is a multi-school 2025-style file. No sample school has 100 graduates (largest observed is {run.LargestSampleGraduateCount}). School identity is unresolved, so numerical mismatches are expected until a matching graduate file exists.");
        writer.WriteLine("2. Men and LJD 25th-percentile PDF text-layer values `850000` and `765000` are recorded as observed, not corrected to 85,000 / 76,500.");
        writer.WriteLine("3. D3 printed subtotal 93 vs detail sum 94, LJD 79 vs 78, and JD Advantage salary n 12 vs count 10 stay documented PDF/SAS tensions. The calculator is not changed to hide them.");
        writer.WriteLine("4. Clerkship printed labels State/Local are compared as `$newvar` `JCSTGV`/`JCTLOG` (`SS-FMT-04`). The sample export has no `emptype1` column, so modern E55 rows are expected to be missing.");
        writer.WriteLine("5. Duration long-term/short-term is compared to `DurationCounts` keys `perm`/`PERM` and `temp`/`TEMP`. CF-P2-01 treats duration codes as data-driven column IDs; that mapping is a conservative reading of the report `perm`/`temp` columns, not a proven ERSS codebook.");
        writer.WriteLine("6. Percents are compared at one decimal (SAS `6.1`). Salary money is compared at zero decimals (SAS `COMMA7.0`).");
        writer.WriteLine($"7. Of the {run.MatchCount} matches, most are missing salary cells or absent categories (both sides `.`). Those are not evidence that school `{run.ModernSchoolCode}` is Test University.");
        writer.WriteLine("8. Mismatches are listed in full above. None were dropped to make the test look green.");
        return writer.ToString();
    }

    private static (SchoolReport? Report, string Reason) ChooseSchool(IReadOnlyList<SchoolReport> reports)
    {
        SchoolReport? Exact()
        {
            return reports.FirstOrDefault(report =>
                Value(report, "A", "A", ParityField.Count) == 100
                && Value(report, "B", "F", ParityField.Count) == 46
                && Value(report, "B", "M", ParityField.Count) == 54);
        }

        var exact = Exact();
        if (exact is not null)
        {
            return (exact, "Exact match on Total Reported 100, Women 46, Men 54.");
        }

        if (reports.Count == 0)
        {
            return (null, "Sample export produced no school reports.");
        }

        var hundred = reports.Where(report => Value(report, "A", "A", ParityField.Count) == 100).ToArray();
        if (hundred.Length == 1)
        {
            return (hundred[0], "Only sample school with Total Reported 100. Gender did not match the PDF.");
        }

        var scored = reports
            .Select(report => (report, score: CountMatches(report)))
            .OrderByDescending(item => item.score)
            .ThenByDescending(item => Value(item.report, "A", "A", ParityField.Count) ?? 0)
            .First();
        return (scored.report, $"No PDF-identity match. Used sample school {scored.report.SchoolCode} with the most matching characterized counts ({scored.score}).");
    }

    private static int CountMatches(SchoolReport report) =>
        Catalog.Count(metric => metric.Field == ParityField.Count && !metric.Unresolved && Observe(metric, report).Status == ParityStatus.Match);

    private static ParityObservation Observe(LegacyExpectedMetric metric, SchoolReport? report)
    {
        if (metric.Unresolved)
        {
            return new ParityObservation(metric, null, report?.SchoolCode, ParityStatus.Unresolved, metric.Note ?? "Not enough characterized mapping to compare.");
        }

        if (report is null)
        {
            return new ParityObservation(metric, null, null, ParityStatus.Mismatch, "No modern school report was produced.");
        }

        var modern = Value(report, metric.Analvar, metric.Newvar, metric.Field, metric.Id);
        var comparableModern = Normalize(metric.Field, modern);
        var comparableLegacy = Normalize(metric.Field, metric.Expected);
        var comparison = $"Legacy {Fmt(comparableLegacy)} vs modern {Fmt(comparableModern)}";
        if (!string.IsNullOrWhiteSpace(metric.Note))
        {
            comparison = $"{comparison}; {metric.Note}";
        }

        if (Equals(comparableLegacy, comparableModern))
        {
            return new ParityObservation(metric, modern, report.SchoolCode, ParityStatus.Match, comparison);
        }

        return new ParityObservation(metric, modern, report.SchoolCode, ParityStatus.Mismatch, comparison);
    }

    private static decimal? Value(SchoolReport report, string analvar, string? newvar, ParityField field, string? id = null)
    {
        if (field is ParityField.SubtotalCount or ParityField.SubtotalPercent)
        {
            var section = report.Sections.FirstOrDefault(item => item.Analvar == analvar);
            if (section is null)
            {
                return null;
            }

            return field == ParityField.SubtotalCount ? section.SubtotalCount : section.SubtotalPercent;
        }

        if (newvar is null && field == ParityField.Count)
        {
            var any = report.Rows.Any(row => row.Analvar == analvar);
            return any ? 1 : null;
        }

        var row = report.Rows.FirstOrDefault(item => item.Analvar == analvar && item.Newvar == newvar);
        if (row is null)
        {
            return null;
        }

        if (analvar == "DURATION")
        {
            var wanted = id?.EndsWith(".temp", StringComparison.Ordinal) == true ? "temp" : "perm";
            if (row.DurationCounts is not null)
            {
                var match = row.DurationCounts
                    .FirstOrDefault(item => string.Equals(item.Key, wanted, StringComparison.OrdinalIgnoreCase));
                if (!string.IsNullOrEmpty(match.Key))
                {
                    return match.Value;
                }
            }

            return null;
        }

        return field switch
        {
            ParityField.Count => row.Count,
            ParityField.Percent => row.Percent,
            ParityField.SalaryN => row.SalaryN,
            ParityField.Pct25 => row.Pct25,
            ParityField.Median => row.Median,
            ParityField.Pct75 => row.Pct75,
            ParityField.Mean => row.Mean,
            _ => null,
        };
    }

    private static decimal? Normalize(ParityField field, decimal? value)
    {
        if (value is null)
        {
            return null;
        }

        return field switch
        {
            ParityField.Percent or ParityField.SubtotalPercent => decimal.Round(value.Value, 1, MidpointRounding.AwayFromZero),
            ParityField.Pct25 or ParityField.Median or ParityField.Pct75 or ParityField.Mean => decimal.Round(value.Value, 0, MidpointRounding.AwayFromZero),
            _ => value,
        };
    }

    private static void WriteRuleList(
        TextWriter writer,
        IEnumerable<string> rules,
        IReadOnlyList<ParityObservation> observations,
        ParityStatus status)
    {
        var list = rules.ToArray();
        if (list.Length == 0)
        {
            writer.WriteLine("None.");
            writer.WriteLine();
            return;
        }

        foreach (var rule in list)
        {
            var hits = observations.Where(item => item.Metric.RuleId == rule && item.Status == status).ToArray();
            writer.WriteLine($"- **{rule}** — {hits.Length} metric(s). {hits[0].Explanation}");
        }

        writer.WriteLine();
    }

    private static string Fmt(decimal? value) => value is null ? "." : value.Value.ToString("G");

    private static string Escape(string value) => value.Replace("|", "/", StringComparison.Ordinal);

    internal static string FindSampleExport()
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);
        while (directory is not null)
        {
            var candidate = Path.Combine(directory.FullName, "legacy", "samples", "sample-export.xlsx");
            if (File.Exists(candidate))
            {
                return candidate;
            }

            directory = directory.Parent;
        }

        throw new FileNotFoundException("legacy/samples/sample-export.xlsx");
    }

    internal static string FindRepoRoot()
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);
        while (directory is not null)
        {
            if (File.Exists(Path.Combine(directory.FullName, "AccessibleSchoolReports.sln"))
                && Directory.Exists(Path.Combine(directory.FullName, "docs", "capstone")))
            {
                return directory.FullName;
            }

            directory = directory.Parent;
        }

        throw new DirectoryNotFoundException("Could not find the repository root.");
    }

    internal static void WriteResultsDocument(ParityRun run)
    {
        var path = Path.Combine(FindRepoRoot(), "evidence", "test-results", "parity-results.md");
        File.WriteAllText(path, RenderMarkdown(run));
    }
}
