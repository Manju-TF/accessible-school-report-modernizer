using System.Net.Http;
using System.Text.RegularExpressions;
using AccessibleSchoolReports.Web;
using Microsoft.AspNetCore.Mvc.Testing;

namespace AccessibleSchoolReports.UnitTests.Security;

internal static class AuthTestHttp
{
    private static readonly Regex AntiforgeryToken = new(
        @"name=""__RequestVerificationToken""[^>]*value=""([^""]+)""|value=""([^""]+)""[^>]*name=""__RequestVerificationToken""",
        RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);

    public static HttpClient CreateClient(WebApplicationFactory<Program> factory) =>
        factory.CreateClient(new() { AllowAutoRedirect = false, HandleCookies = true });

    public static async Task<HttpResponseMessage> SignInAsync(
        HttpClient client,
        string userName,
        string password)
    {
        var signInPage = await client.GetAsync("/signin");
        var token = ReadAntiforgeryToken(await signInPage.Content.ReadAsStringAsync());
        return await client.PostAsync(
            "/account/signin",
            new FormUrlEncodedContent(new Dictionary<string, string>
            {
                ["username"] = userName,
                ["password"] = password,
                ["returnUrl"] = "/",
                ["__RequestVerificationToken"] = token,
            }));
    }

    public static async Task<HttpResponseMessage> SignOutAsync(HttpClient client)
    {
        var dashboard = await client.GetAsync("/");
        var token = ReadAntiforgeryToken(await dashboard.Content.ReadAsStringAsync());
        return await client.PostAsync(
            "/account/signout",
            new FormUrlEncodedContent(new Dictionary<string, string>
            {
                ["__RequestVerificationToken"] = token,
            }));
    }

    public static string ReadAntiforgeryToken(string html)
    {
        var match = AntiforgeryToken.Match(html);
        if (!match.Success)
        {
            throw new InvalidOperationException("The page did not include an antiforgery token.");
        }

        return match.Groups[1].Success ? match.Groups[1].Value : match.Groups[2].Value;
    }
}
