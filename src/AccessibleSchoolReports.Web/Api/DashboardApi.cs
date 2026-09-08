using AccessibleSchoolReports.Application.Reporting;
using AccessibleSchoolReports.Application.Security;
using AccessibleSchoolReports.Infrastructure.Persistence;
using AccessibleSchoolReports.Web.Ui;
using Microsoft.EntityFrameworkCore;

namespace AccessibleSchoolReports.Web.Api;

public static class DashboardApi
{
    private const int TopSchoolLimit = 10;

    public static void MapDashboardApi(this WebApplication app)
    {
        app.MapGet("/api/dashboard", async (
                HttpContext http,
                SchoolReportsDbContext db,
                IReportAuthorizationService authorization,
                CancellationToken cancellationToken) =>
            {
                try
                {
                    var canConnect = await db.Database.CanConnectAsync(cancellationToken);
                    if (!canConnect)
                    {
                        return Results.Json(
                            new
                            {
                                databaseStatus = "Unavailable",
                                databaseDetail = "The application could not open the SQLite database.",
                                generatedAtUtc = UiFormat.Utc(DateTimeOffset.UtcNow),
                                schoolCount = 0,
                                graduateCount = 0,
                                years = Array.Empty<object>(),
                                schools = Array.Empty<object>(),
                                topSchools = Array.Empty<object>(),
                                lastImport = (object?)null,
                                lastRun = (object?)null,
                            },
                            ApiConventions.Json);
                    }

                    var allowed = await authorization.GetAccessibleSchoolIdsAsync(http.User, cancellationToken);
                    var counts = allowed.Count == 0
                        ? []
                        : await db.GraduateRecords
                            .AsNoTracking()
                            .Where(row => allowed.Contains(row.SchoolId))
                            .GroupBy(row => new { row.SchoolId, row.ClassYear })
                            .Select(group => new
                            {
                                group.Key.SchoolId,
                                group.Key.ClassYear,
                                Count = group.Count(),
                            })
                            .ToListAsync(cancellationToken);

                    var schools = allowed.Count == 0
                        ? []
                        : await db.Schools
                            .AsNoTracking()
                            .Where(school => allowed.Contains(school.Id))
                            .OrderBy(school => school.Code)
                            .Select(school => new { school.Id, school.Code, school.Name })
                            .ToListAsync(cancellationToken);

                    var yearHeadcounts = counts
                        .GroupBy(row => row.ClassYear)
                        .Select(group => new YearHeadcount(
                            group.Key,
                            group.Select(row => row.SchoolId).Distinct().Count(),
                            group.Sum(row => row.Count)))
                        .ToArray();
                    var graduateCount = counts.Sum(row => row.Count);
                    var years = DashboardAnalytics.BuildYearMetrics(yearHeadcounts, graduateCount);
                    var yearKeys = years.Select(year => year.Key).ToArray();

                    var schoolRows = schools.Select(school =>
                    {
                        var byYear = counts
                            .Where(row => row.SchoolId == school.Id)
                            .ToDictionary(row => row.ClassYear?.ToString() ?? "none", row => row.Count);
                        var total = byYear.Values.Sum();
                        return new
                        {
                            id = school.Id,
                            code = school.Code,
                            name = school.Name,
                            label = UiFormat.SchoolLabel(school.Code, school.Name),
                            graduateCount = total,
                            yearCounts = yearKeys.ToDictionary(
                                key => key,
                                key => byYear.GetValueOrDefault(key)),
                        };
                    }).ToArray();

                    var lastImport = await db.ImportRuns
                        .AsNoTracking()
                        .Where(run =>
                            run.ImportedRowCount > 0
                            && (run.Status == Domain.Persistence.RunStatus.Completed
                                || run.Status == Domain.Persistence.RunStatus.CompletedWithErrors))
                        .OrderByDescending(run => run.Id)
                        .FirstOrDefaultAsync(cancellationToken);
                    var lastRun = await db.ReportRuns
                        .AsNoTracking()
                        .OrderByDescending(run => run.Id)
                        .FirstOrDefaultAsync(cancellationToken);

                    return Results.Json(
                        new
                        {
                            databaseStatus = "Connected",
                            databaseDetail = "SQLite is reachable. Counts are live stored graduate records, not printed report totals.",
                            generatedAtUtc = UiFormat.Utc(DateTimeOffset.UtcNow),
                            schoolCount = schools.Count,
                            graduateCount,
                            years,
                            schools = schoolRows,
                            topSchools = schoolRows
                                .OrderByDescending(school => school.graduateCount)
                                .ThenBy(school => school.code, StringComparer.Ordinal)
                                .Take(TopSchoolLimit)
                                .Select(school => new
                                {
                                    school.code,
                                    school.label,
                                    school.graduateCount,
                                }),
                            lastImport = lastImport is null
                                ? null
                                : new
                                {
                                    fileName = lastImport.FileName,
                                    status = UiFormat.Status(lastImport.Status),
                                    statusTone = UiFormat.StatusTone(lastImport.Status),
                                    startedUtc = UiFormat.Utc(lastImport.StartedUtc),
                                    importedRowCount = lastImport.ImportedRowCount,
                                    invalidRowCount = lastImport.InvalidRowCount,
                                    blankRowCount = lastImport.BlankRowCount,
                                },
                            lastRun = lastRun is null
                                ? null
                                : new
                                {
                                    id = lastRun.Id,
                                    status = UiFormat.Status(lastRun.Status),
                                    statusTone = UiFormat.StatusTone(lastRun.Status),
                                    mode = UiFormat.Mode(lastRun.Mode),
                                    startedUtc = UiFormat.Utc(lastRun.StartedUtc),
                                    totalCount = lastRun.TotalCount,
                                    successfulCount = lastRun.SuccessfulCount,
                                    failedCount = lastRun.FailedCount,
                                    duration = UiFormat.Duration(lastRun.DurationMilliseconds),
                                },
                        },
                        ApiConventions.Json);
                }
                catch (Exception)
                {
                    return Results.Json(
                        new { error = "The dashboard could not read the database." },
                        ApiConventions.Json,
                        statusCode: StatusCodes.Status500InternalServerError);
                }
            })
            .RequireAuthorization(AppPolicies.RequireReportAccess);
    }
}
