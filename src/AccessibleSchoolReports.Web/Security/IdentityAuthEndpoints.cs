using System.Text.Json;
using AccessibleSchoolReports.Web.Api;
using Microsoft.AspNetCore.Identity;

namespace AccessibleSchoolReports.Web.Security;

public static class IdentityAuthEndpoints
{
    public static void MapIdentityAuth(this WebApplication app)
    {
        app.MapPost("/account/signin", SignInAsync).AllowAnonymous();
        app.MapPost("/account/signout", SignOutAsync).AllowAnonymous();
    }

    private static async Task<IResult> SignInAsync(
        HttpContext http,
        SignInManager<IdentityUser> signInManager)
    {
        string userName;
        string password;
        string returnUrl;
        var wantsJson = WantsJson(http.Request);

        if (http.Request.HasJsonContentType())
        {
            var body = await http.Request.ReadFromJsonAsync<SignInJson>(ApiConventions.Json);
            userName = body?.UserName?.Trim() ?? string.Empty;
            password = body?.Password ?? string.Empty;
            returnUrl = body?.ReturnUrl ?? string.Empty;
        }
        else
        {
            userName = http.Request.Form["username"].ToString().Trim();
            password = http.Request.Form["password"].ToString();
            returnUrl = http.Request.Form["returnUrl"].ToString();
        }

        var result = await signInManager.PasswordSignInAsync(
            userName,
            password,
            isPersistent: false,
            lockoutOnFailure: true);

        if (!result.Succeeded)
        {
            return wantsJson
                ? Results.Json(new { error = "The user name or password is not correct." }, ApiConventions.Json, statusCode: StatusCodes.Status401Unauthorized)
                : Results.Redirect("/signin?error=1");
        }

        var safe = SafeLocalUrl(returnUrl);
        return wantsJson
            ? Results.Json(new { ok = true, returnUrl = safe }, ApiConventions.Json)
            : Results.Redirect(safe);
    }

    private static async Task<IResult> SignOutAsync(HttpContext http, SignInManager<IdentityUser> signInManager)
    {
        await signInManager.SignOutAsync();
        return WantsJson(http.Request)
            ? Results.Json(new { ok = true }, ApiConventions.Json)
            : Results.Redirect("/signin");
    }

    private static bool WantsJson(HttpRequest request) =>
        request.HasJsonContentType()
        || request.Headers.Accept.ToString().Contains("application/json", StringComparison.OrdinalIgnoreCase);

    private static string SafeLocalUrl(string? returnUrl)
    {
        if (string.IsNullOrWhiteSpace(returnUrl)
            || !returnUrl.StartsWith('/')
            || returnUrl.StartsWith("//", StringComparison.Ordinal)
            || returnUrl.StartsWith("/\\", StringComparison.Ordinal))
        {
            return "/";
        }

        return returnUrl;
    }

    private sealed record SignInJson(string? UserName, string? Password, string? ReturnUrl);
}
