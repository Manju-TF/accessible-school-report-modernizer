using AccessibleSchoolReports.Domain.Persistence;
using AccessibleSchoolReports.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace AccessibleSchoolReports.Infrastructure.Import;

/// <summary>
/// Fills missing graduate class years from the original workbook.
/// Used when a file was imported before classyear was persisted.
/// Does not change calculator inputs.
/// </summary>
public static class GraduateClassYearBackfill
{
    public static async Task<int> ApplyFromSampleWorkbooksAsync(
        SchoolReportsDbContext db,
        string repositoryRoot,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(db);
        ArgumentException.ThrowIfNullOrWhiteSpace(repositoryRoot);

        var folder = Path.Combine(repositoryRoot, "data", "sample-workbooks");
        if (!Directory.Exists(folder))
        {
            return 0;
        }

        var runs = await db.ImportRuns
            .AsNoTracking()
            .Where(run =>
                run.ImportedRowCount > 0
                && (run.Status == RunStatus.Completed || run.Status == RunStatus.CompletedWithErrors)
                && db.GraduateRecords.Any(row => row.ImportRunId == run.Id && row.ClassYear == null))
            .ToListAsync(cancellationToken);

        var updated = 0;
        foreach (var run in runs)
        {
            if (string.IsNullOrWhiteSpace(run.FileName))
            {
                continue;
            }

            var path = Path.Combine(folder, Path.GetFileName(run.FileName));
            if (!File.Exists(path))
            {
                continue;
            }

            await using var stream = File.OpenRead(path);
            updated += await ApplyAsync(db, run.Id, stream, cancellationToken);
        }

        return updated;
    }

    public static async Task<int> ApplyAsync(
        SchoolReportsDbContext db,
        int importRunId,
        Stream workbook,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(db);
        ArgumentNullException.ThrowIfNull(workbook);

        var parsed = ExcelGraduateWorkbookParser.Parse(workbook);
        if (parsed.ValidRows.Count == 0)
        {
            return 0;
        }

        var graduates = await db.GraduateRecords
            .Include(row => row.School)
            .Where(row => row.ImportRunId == importRunId)
            .OrderBy(row => row.Id)
            .ToListAsync(cancellationToken);
        if (graduates.Count != parsed.ValidRows.Count)
        {
            return 0;
        }

        var updated = 0;
        for (var index = 0; index < graduates.Count; index++)
        {
            var graduate = graduates[index];
            var row = parsed.ValidRows[index];
            if (!string.Equals(graduate.School.Code, row.SchoolCode, StringComparison.Ordinal))
            {
                return 0;
            }

            if (graduate.ClassYear is null && row.ClassYear is int year)
            {
                graduate.ClassYear = year;
                updated++;
            }
        }

        if (updated > 0)
        {
            await db.SaveChangesAsync(cancellationToken);
        }

        return updated;
    }
}
