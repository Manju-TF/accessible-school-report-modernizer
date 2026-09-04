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
        var userName = http.Request.Form["username"].ToString().Trim();
        var password = http.Request.Form["password"].ToString();
        var returnUrl = http.Request.Form["returnUrl"].ToString();

        var result = await signInManager.PasswordSignInAsync(
            userName,
            password,
            isPersistent: false,
            lockoutOnFailure: true);

        if (!result.Succeeded)
        {
            return Results.Redirect("/signin?error=1");
        }

        return Results.Redirect(SafeLocalUrl(returnUrl));
    }

    private static async Task<IResult> SignOutAsync(SignInManager<IdentityUser> signInManager)
    {
        await signInManager.SignOutAsync();
        return Results.Redirect("/signin");
    }

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
}
