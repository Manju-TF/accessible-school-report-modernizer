using System.Net;
using System.Net.Http;
using AccessibleSchoolReports.Infrastructure.Persistence;
using AccessibleSchoolReports.Web.Security;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;

namespace AccessibleSchoolReports.UnitTests.Security;

[Collection(SecurityCollection.Name)]
public sealed class AuthenticationFoundationTests
{
    private readonly SecurityWebApplicationFactory _factory;

    public AuthenticationFoundationTests(SecurityWebApplicationFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task AnonymousRequest_IsRedirectedToSignIn()
    {
        var client = AuthTestHttp.CreateClient(_factory);

        var response = await client.GetAsync("/");

        Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
        Assert.Contains("/signin", response.Headers.Location?.ToString(), StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task AnonymousKnowledgeAssistant_IsRedirectedToSignIn()
    {
        var client = AuthTestHttp.CreateClient(_factory);

        var response = await client.GetAsync("/knowledge-assistant");

        Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
        Assert.Contains("/signin", response.Headers.Location?.ToString(), StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task AnonymousDownload_IsRedirectedToSignIn()
    {
        var client = AuthTestHttp.CreateClient(_factory);

        var response = await client.GetAsync("/downloads/reports/1");

        Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
        Assert.Contains("/signin", response.Headers.Location?.ToString(), StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task SignInPage_IsAnonymous()
    {
        var client = AuthTestHttp.CreateClient(_factory);

        var response = await client.GetAsync("/signin");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task Password_IsHashedByIdentity_NotStoredInPlainText()
    {
        using var scope = _factory.Services.CreateScope();
        var users = scope.ServiceProvider.GetRequiredService<UserManager<IdentityUser>>();
        var db = scope.ServiceProvider.GetRequiredService<SchoolReportsDbContext>();

        var user = await users.FindByNameAsync(SecurityWebApplicationFactory.TestUserName);
        Assert.NotNull(user);
        Assert.False(string.IsNullOrWhiteSpace(user.PasswordHash));
        Assert.DoesNotContain(
            SecurityWebApplicationFactory.TestPassword,
            user.PasswordHash,
            StringComparison.Ordinal);
        Assert.True(await users.CheckPasswordAsync(user, SecurityWebApplicationFactory.TestPassword));
        Assert.DoesNotContain(
            SecurityWebApplicationFactory.TestPassword,
            db.Users.Select(row => row.PasswordHash));
    }

    [Fact]
    public async Task ValidCredentials_SetAuthCookie_AndOpenDashboard()
    {
        var client = AuthTestHttp.CreateClient(_factory);

        var signedIn = await AuthTestHttp.SignInAsync(
            client,
            SecurityWebApplicationFactory.TestUserName,
            SecurityWebApplicationFactory.TestPassword);
        Assert.Equal(HttpStatusCode.Redirect, signedIn.StatusCode);
        Assert.Equal("/", signedIn.Headers.Location?.ToString());
        Assert.Contains(
            IdentityAuthenticationExtensions.AuthCookieName,
            signedIn.Headers.GetValues("Set-Cookie").First());

        var dashboard = await client.GetAsync("/");
        Assert.Equal(HttpStatusCode.OK, dashboard.StatusCode);
    }

    [Fact]
    public async Task InvalidCredentials_DoNotAuthenticate()
    {
        var client = AuthTestHttp.CreateClient(_factory);
        await client.GetAsync("/signin");

        var response = await AuthTestHttp.SignInAsync(
            client,
            SecurityWebApplicationFactory.TestUserName,
            "Wrong-Password-1!");

        Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
        Assert.Contains("/signin?error=1", response.Headers.Location?.ToString(), StringComparison.OrdinalIgnoreCase);
        if (response.Headers.Contains("Set-Cookie"))
        {
            Assert.DoesNotContain(
                IdentityAuthenticationExtensions.AuthCookieName,
                string.Join('\n', response.Headers.GetValues("Set-Cookie")));
        }

        var dashboard = await client.GetAsync("/");
        Assert.Equal(HttpStatusCode.Redirect, dashboard.StatusCode);
    }

    [Fact]
    public async Task SignOut_ClearsSession()
    {
        var client = AuthTestHttp.CreateClient(_factory);
        await AuthTestHttp.SignInAsync(
            client,
            SecurityWebApplicationFactory.TestUserName,
            SecurityWebApplicationFactory.TestPassword);

        var signOut = await AuthTestHttp.SignOutAsync(client);
        Assert.Equal(HttpStatusCode.Redirect, signOut.StatusCode);
        Assert.Contains("/signin", signOut.Headers.Location?.ToString(), StringComparison.OrdinalIgnoreCase);

        var dashboard = await client.GetAsync("/");
        Assert.Equal(HttpStatusCode.Redirect, dashboard.StatusCode);
    }

    [Fact]
    public async Task SignIn_RejectsExternalReturnUrl()
    {
        var client = AuthTestHttp.CreateClient(_factory);
        var signInPage = await client.GetAsync("/signin");
        var token = AuthTestHttp.ReadAntiforgeryToken(await signInPage.Content.ReadAsStringAsync());

        var response = await client.PostAsync(
            "/account/signin",
            new FormUrlEncodedContent(new Dictionary<string, string>
            {
                ["username"] = SecurityWebApplicationFactory.TestUserName,
                ["password"] = SecurityWebApplicationFactory.TestPassword,
                ["returnUrl"] = "https://example.com/phish",
                ["__RequestVerificationToken"] = token,
            }));

        Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
        Assert.Equal("/", response.Headers.Location?.ToString());
    }
}
