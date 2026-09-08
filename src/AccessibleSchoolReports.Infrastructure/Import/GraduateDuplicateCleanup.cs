using AccessibleSchoolReports.Application.Imports;
using AccessibleSchoolReports.Domain.Entities;
using AccessibleSchoolReports.Domain.Persistence;
using AccessibleSchoolReports.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace AccessibleSchoolReports.Infrastructure.Import;

/// <summary>
/// Removes extra import runs that stored the same workbook or the same graduate row set.
/// Does not delete look-alike rows that arrived together in one source file.
/// </summary>
public static class GraduateDuplicateCleanup
{
    public static async Task<int> ApplyAsync(
        SchoolReportsDbContext db,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(db);

        var runs = await db.ImportRuns
            .Where(run =>
                run.ImportedRowCount > 0
                && (run.Status == RunStatus.Completed || run.Status == RunStatus.CompletedWithErrors))
            .OrderBy(run => run.Id)
            .ToListAsync(cancellationToken);

        foreach (var run in runs.Where(run => string.IsNullOrWhiteSpace(run.RowSetSha256)))
        {
            var graduates = await db.GraduateRecords
                .AsNoTracking()
                .Include(row => row.School)
                .Where(row => row.ImportRunId == run.Id)
                .ToListAsync(cancellationToken);
            if (graduates.Count == 0)
            {
                continue;
            }

            run.RowSetSha256 = GraduateRowFingerprint.ComputeSet(graduates.Select(Fingerprint));
        }

        if (db.ChangeTracker.HasChanges())
        {
            await db.SaveChangesAsync(cancellationToken);
        }

        var removed = 0;
        removed += await RemoveNewerDuplicatesAsync(db, runs, run => run.ContentSha256, cancellationToken);
        var remaining = await db.ImportRuns
            .Where(run =>
                run.ImportedRowCount > 0
                && (run.Status == RunStatus.Completed || run.Status == RunStatus.CompletedWithErrors))
            .OrderBy(run => run.Id)
            .ToListAsync(cancellationToken);
        removed += await RemoveNewerDuplicatesAsync(db, remaining, run => run.RowSetSha256, cancellationToken);
        return removed;
    }

    private static async Task<int> RemoveNewerDuplicatesAsync(
        SchoolReportsDbContext db,
        IReadOnlyCollection<ImportRun> runs,
        Func<ImportRun, string?> keySelector,
        CancellationToken cancellationToken)
    {
        var removed = 0;
        foreach (var group in runs
            .Where(run => !string.IsNullOrWhiteSpace(keySelector(run)))
            .GroupBy(keySelector, StringComparer.OrdinalIgnoreCase)
            .Where(group => group.Count() > 1))
        {
            foreach (var extra in group.Skip(1))
            {
                var tracked = await db.ImportRuns.FirstOrDefaultAsync(run => run.Id == extra.Id, cancellationToken);
                if (tracked is null)
                {
                    continue;
                }

                db.ImportRuns.Remove(tracked);
                removed++;
            }
        }

        if (removed > 0)
        {
            await db.SaveChangesAsync(cancellationToken);
        }

        return removed;
    }

    private static string Fingerprint(GraduateRecord row) =>
        GraduateRowFingerprint.Compute(
            row.School.Code,
            row.ClassYear,
            row.Sex3,
            row.Minstat,
            row.Jobcat1,
            row.JobFtPt,
            row.Empgen,
            row.Firm1,
            row.Lfjob,
            row.Jobreg,
            row.LocationFlag,
            row.Jobst,
            row.Source,
            row.Time1,
            row.Status,
            row.Duration,
            row.SchoolFund,
            row.SalFtPerm,
            row.Emptype1);
}
