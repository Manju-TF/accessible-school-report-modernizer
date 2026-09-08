using AccessibleSchoolReports.Application.Security;
using AccessibleSchoolReports.Domain.Persistence;
using AccessibleSchoolReports.Infrastructure.Persistence;
using AccessibleSchoolReports.Web.Ui;
using Microsoft.EntityFrameworkCore;

namespace AccessibleSchoolReports.Web.Api;

public static class RunsApi
{
    public static void MapRunsApi(this WebApplication app)
    {
        app.MapGet("/api/runs", ListAsync)
            .RequireAuthorization(AppPolicies.RequireReportAccess);

        app.MapGet("/api/runs/{id:int}", GetAsync)
            .RequireAuthorization(AppPolicies.RequireReportAccess);
    }

    private static async Task<IResult> ListAsync(
        HttpContext http,
        SchoolReportsDbContext db,
        IReportAuthorizationService authorization,
        CancellationToken cancellationToken)
    {
        var allowed = await authorization.GetAccessibleSchoolIdsAsync(http.User, cancellationToken);
        var runs = await db.ReportRuns
            .AsNoTracking()
            .Include(run => run.Items)
            .ThenInclude(item => item.School)
            .OrderByDescending(run => run.Id)
            .Take(50)
            .ToListAsync(cancellationToken);

        var payload = runs
            .Select(run =>
            {
                var items = run.Items.Where(item => allowed.Contains(item.SchoolId)).OrderBy(item => item.School.Code).ToList();
                return items.Count == 0 ? null : ToRun(run, items);
            })
            .Where(run => run is not null);

        return Results.Json(new { runs = payload }, ApiConventions.Json);
    }

    private static async Task<IResult> GetAsync(
        int id,
        HttpContext http,
        SchoolReportsDbContext db,
        IReportAuthorizationService authorization,
        CancellationToken cancellationToken)
    {
        var allowed = await authorization.GetAccessibleSchoolIdsAsync(http.User, cancellationToken);
        var run = await db.ReportRuns
            .AsNoTracking()
            .Include(row => row.Items)
            .ThenInclude(item => item.School)
            .FirstOrDefaultAsync(row => row.Id == id, cancellationToken);
        if (run is null)
        {
            return Results.Json(new { error = "That report is not available." }, ApiConventions.Json, statusCode: StatusCodes.Status404NotFound);
        }

        var items = run.Items.Where(item => allowed.Contains(item.SchoolId)).OrderBy(item => item.School.Code).ToList();
        if (items.Count == 0)
        {
            return Results.Json(new { error = "That report is not available." }, ApiConventions.Json, statusCode: StatusCodes.Status404NotFound);
        }

        return Results.Json(ToRun(run, items), ApiConventions.Json);
    }

    private static object ToRun(Domain.Entities.ReportRun run, IReadOnlyList<Domain.Entities.ReportRunItem> items) =>
        new
        {
            id = run.Id,
            status = UiFormat.Status(run.Status),
            statusTone = UiFormat.StatusTone(run.Status),
            mode = UiFormat.Mode(run.Mode),
            startedUtc = UiFormat.Utc(run.StartedUtc),
            totalCount = run.TotalCount,
            successfulCount = run.SuccessfulCount,
            failedCount = run.FailedCount,
            duration = UiFormat.Duration(run.DurationMilliseconds),
            items = items.Select(item =>
            {
                var completed = item.Status == RunStatus.Completed && !string.IsNullOrWhiteSpace(item.OutputPath);
                return new
                {
                    id = item.Id,
                    schoolLabel = UiFormat.SchoolLabel(item.School.Code, item.School.Name),
                    schoolCode = item.School.Code,
                    status = UiFormat.Status(item.Status),
                    statusTone = UiFormat.StatusTone(item.Status),
                    message = item.Message,
                    downloadUrl = completed ? $"/downloads/reports/{item.Id}/{item.School.Code}-summary-report.pdf" : null,
                    downloadName = completed ? $"{item.School.Code}-summary-report.pdf" : null,
                };
            }),
        };
}
