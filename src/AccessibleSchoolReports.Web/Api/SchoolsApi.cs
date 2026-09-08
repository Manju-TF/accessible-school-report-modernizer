using AccessibleSchoolReports.Application.Reporting;
using AccessibleSchoolReports.Application.Security;
using AccessibleSchoolReports.Infrastructure.Persistence;
using AccessibleSchoolReports.Web.Ui;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace AccessibleSchoolReports.Web.Api;

public static class SchoolsApi
{
    public static void MapSchoolsApi(this WebApplication app)
    {
        app.MapGet("/api/schools", async (
                HttpContext http,
                SchoolReportsDbContext db,
                IReportAuthorizationService authorization,
                IOptions<ReportGenerationOptions> options,
                CancellationToken cancellationToken) =>
            {
                var user = http.User;
                var allowed = await authorization.GetAccessibleSchoolIdsAsync(user, cancellationToken);
                var rows = await db.Schools
                    .AsNoTracking()
                    .Where(school => allowed.Contains(school.Id)
                        && db.GraduateRecords.Any(row => row.SchoolId == school.Id))
                    .OrderBy(school => school.Code)
                    .Select(school => new
                    {
                        school.Id,
                        school.Code,
                        school.Name,
                        GraduateCount = db.GraduateRecords.Count(row => row.SchoolId == school.Id),
                    })
                    .ToListAsync(cancellationToken);

                var schools = new List<object>();
                var visibleIds = new List<int>();
                foreach (var row in rows)
                {
                    if (await authorization.CanGenerateReportAsync(user, row.Id, cancellationToken))
                    {
                        visibleIds.Add(row.Id);
                    }
                }

                var years = await SchoolClassYears.LoadAsync(
                    db,
                    visibleIds,
                    options.Value.ClassYear,
                    cancellationToken);

                foreach (var row in rows.Where(row => visibleIds.Contains(row.Id)))
                {
                    years.BySchool.TryGetValue(row.Id, out var schoolYears);
                    schools.Add(new
                    {
                        id = row.Id,
                        code = row.Code,
                        name = row.Name,
                        graduateCount = row.GraduateCount,
                        label = UiFormat.SchoolLabel(row.Code, row.Name, row.GraduateCount),
                        classYears = schoolYears ?? years.Years,
                    });
                }

                return Results.Json(
                    new
                    {
                        classYear = years.DefaultYear,
                        classYears = years.Years,
                        schools,
                    },
                    ApiConventions.Json);
            })
            .RequireAuthorization(AppPolicies.RequireReportGeneration);
    }
}
