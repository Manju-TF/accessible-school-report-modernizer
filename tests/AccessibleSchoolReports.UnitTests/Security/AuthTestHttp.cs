using System.Net.Http;
using System.Text.Json;
using AccessibleSchoolReports.Web;
using Microsoft.AspNetCore.Mvc.Testing;

namespace AccessibleSchoolReports.UnitTests.Security;

internal static class AuthTestHttp
{
    public static HttpClient CreateClient(WebApplicationFactory<Program> factory) =>
        factory.CreateClient(new() { AllowAutoRedirect = false, HandleCookies = true });

    public static async Task<string> GetAntiforgeryTokenAsync(HttpClient client)
    {
        var response = await client.GetAsync("/account/antiforgery");
        response.EnsureSuccessStatusCode();
        using var document = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        return document.RootElement.GetProperty("requestToken").GetString()
            ?? throw new InvalidOperationException("The antiforgery response did not include a request token.");
    }

    public static async Task<HttpResponseMessage> SignInAsync(
        HttpClient client,
        string userName,
        string password)
    {
        var token = await GetAntiforgeryTokenAsync(client);
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
        var token = await GetAntiforgeryTokenAsync(client);
        return await client.PostAsync(
            "/account/signout",
            new FormUrlEncodedContent(new Dictionary<string, string>
            {
                ["__RequestVerificationToken"] = token,
            }));
    }
}
