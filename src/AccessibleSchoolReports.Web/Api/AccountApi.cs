using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.Identity;

namespace AccessibleSchoolReports.Web.Api;

public static class AccountApi
{
    public static void MapAccountApi(this WebApplication app)
    {
        app.MapGet("/account/antiforgery", (IAntiforgery antiforgery, HttpContext http) =>
            {
                var tokens = antiforgery.GetAndStoreTokens(http);
                return Results.Json(
                    new { requestToken = tokens.RequestToken, headerName = tokens.HeaderName ?? "RequestVerificationToken" },
                    ApiConventions.Json);
            })
            .AllowAnonymous();
    }
}
