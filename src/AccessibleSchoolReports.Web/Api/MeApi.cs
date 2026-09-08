using AccessibleSchoolReports.Application.Security;

namespace AccessibleSchoolReports.Web.Api;

public static class MeApi
{
    public static void MapMeApi(this WebApplication app)
    {
        app.MapGet("/api/me", (HttpContext http) =>
            {
                var user = http.User;
                var name = user.Identity?.Name ?? string.Empty;
                var isAdmin = user.IsInRole(AppRoles.Admin);
                var isReportUser = user.IsInRole(AppRoles.ReportUser);
                var isViewer = user.IsInRole(AppRoles.Viewer);
                var roles = new List<string>();
                if (isAdmin)
                {
                    roles.Add(AppRoles.Admin);
                }

                if (isReportUser)
                {
                    roles.Add(AppRoles.ReportUser);
                }

                if (isViewer)
                {
                    roles.Add(AppRoles.Viewer);
                }

                return Results.Json(
                    new
                    {
                        userName = name,
                        roles,
                        capabilities = new
                        {
                            canViewReports = isAdmin || isReportUser || isViewer,
                            canGenerate = isAdmin || isReportUser,
                            canImport = isAdmin,
                            canGenerateAll = isAdmin,
                            canAsk = isAdmin || isReportUser || isViewer,
                        },
                    },
                    ApiConventions.Json);
            })
            .RequireAuthorization();
    }
}
