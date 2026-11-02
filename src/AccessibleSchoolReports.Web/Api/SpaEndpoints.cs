using AccessibleSchoolReports.Application.Security;

namespace AccessibleSchoolReports.Web.Api;

public static class SpaEndpoints
{
    public static void MapSpaPages(this WebApplication app)
    {
        app.MapGet("/signin", ServeIndex).AllowAnonymous();
        app.MapGet("/denied", ServeIndex).AllowAnonymous();
        app.MapGet("/", ServeIndex).RequireAuthorization(AppPolicies.RequireReportAccess);
        app.MapGet("/import", ServeIndex).RequireAuthorization(AppPolicies.RequireAdmin);
        app.MapGet("/portfolio", ServeIndex).RequireAuthorization(AppPolicies.RequireAdmin);
        app.MapGet("/generate", ServeIndex).RequireAuthorization(AppPolicies.RequireReportGeneration);
        app.MapGet("/generate-all", ServeIndex).RequireAuthorization(AppPolicies.RequireAdmin);
        app.MapGet("/runs", ServeIndex).RequireAuthorization(AppPolicies.RequireReportAccess);
        app.MapGet("/knowledge-assistant", ServeIndex).RequireAuthorization(AppPolicies.RequireRagAccess);
        app.MapGet("/reports/{id:int}", ServeIndex).RequireAuthorization(AppPolicies.RequireReportAccess);
        app.MapFallback(ServeIndex).RequireAuthorization();
    }

    private static IResult ServeIndex(HttpContext context, IWebHostEnvironment environment)
    {
        var path = Path.Combine(environment.WebRootPath ?? string.Empty, "index.html");
        if (!File.Exists(path))
        {
            return Results.Text(
                "The React UI is not built. Run npm run build in src/web.",
                "text/plain",
                statusCode: StatusCodes.Status503ServiceUnavailable);
        }

        context.Response.Headers.CacheControl = "no-cache, no-store, must-revalidate";
        context.Response.Headers.Pragma = "no-cache";
        context.Response.Headers.Expires = "0";
        return Results.File(path, "text/html; charset=utf-8");
    }
}
