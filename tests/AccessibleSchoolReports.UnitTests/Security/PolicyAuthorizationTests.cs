using System.Net;
using System.Security.Claims;
using AccessibleSchoolReports.Application.Security;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;

namespace AccessibleSchoolReports.UnitTests.Security;

[Collection(SecurityCollection.Name)]
public sealed class PolicyAuthorizationTests
{
    private readonly SecurityWebApplicationFactory _factory;

    public PolicyAuthorizationTests(SecurityWebApplicationFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task Anonymous_IsDenied()
    {
        var client = AuthTestHttp.CreateClient(_factory);

        var dashboard = await client.GetAsync("/");
        var import = await client.GetAsync("/import");
        var assistant = await client.GetAsync("/knowledge-assistant");
        var report = await client.GetAsync("/reports/1");
        var download = await client.GetAsync("/downloads/reports/1");

        AssertDeniedToSignIn(dashboard);
        AssertDeniedToSignIn(import);
        AssertDeniedToSignIn(assistant);
        AssertDeniedToSignIn(report);
        AssertDeniedToSignIn(download);
        Assert.False(await AuthorizeAnonymousAsync(AppPolicies.RequireAdmin));
        Assert.False(await AuthorizeAnonymousAsync(AppPolicies.RequireReportAccess));
        Assert.False(await AuthorizeAnonymousAsync(AppPolicies.RequireRagAccess));
        Assert.False(await AuthorizeAnonymousAsync(AppPolicies.RequireReportGeneration));
    }

    [Fact]
    public async Task Viewer_IsDeniedAdmin()
    {
        Assert.False(await AuthorizeRoleAsync(AppRoles.Viewer, AppPolicies.RequireAdmin));

        var client = AuthTestHttp.CreateClient(_factory);
        await AuthTestHttp.SignInAsync(
            client,
            SecurityWebApplicationFactory.ViewerUserName,
            SecurityWebApplicationFactory.TestPassword);

        var import = await client.GetAsync("/import");
        var generateAll = await client.GetAsync("/generate-all");
        AssertDeniedToAccessDenied(import);
        AssertDeniedToAccessDenied(generateAll);
    }

    [Fact]
    public async Task ReportUser_IsDeniedAdmin()
    {
        Assert.False(await AuthorizeRoleAsync(AppRoles.ReportUser, AppPolicies.RequireAdmin));

        var client = AuthTestHttp.CreateClient(_factory);
        await AuthTestHttp.SignInAsync(
            client,
            SecurityWebApplicationFactory.ReportUserName,
            SecurityWebApplicationFactory.TestPassword);

        var import = await client.GetAsync("/import");
        var generateAll = await client.GetAsync("/generate-all");
        AssertDeniedToAccessDenied(import);
        AssertDeniedToAccessDenied(generateAll);
    }

    [Fact]
    public async Task Admin_IsAllowedAdmin()
    {
        Assert.True(await AuthorizeRoleAsync(AppRoles.Admin, AppPolicies.RequireAdmin));

        var client = AuthTestHttp.CreateClient(_factory);
        await AuthTestHttp.SignInAsync(
            client,
            SecurityWebApplicationFactory.AdminUserName,
            SecurityWebApplicationFactory.TestPassword);

        var import = await client.GetAsync("/import");
        var generateAll = await client.GetAsync("/generate-all");
        Assert.Equal(HttpStatusCode.OK, import.StatusCode);
        Assert.Equal(HttpStatusCode.OK, generateAll.StatusCode);
    }

    [Fact]
    public async Task Viewer_IsAllowedPermittedReportAccess()
    {
        Assert.True(await AuthorizeRoleAsync(AppRoles.Viewer, AppPolicies.RequireReportAccess));
        Assert.True(await AuthorizeRoleAsync(AppRoles.Viewer, AppPolicies.RequireRagAccess));
        Assert.False(await AuthorizeRoleAsync(AppRoles.Viewer, AppPolicies.RequireReportGeneration));

        var client = AuthTestHttp.CreateClient(_factory);
        await AuthTestHttp.SignInAsync(
            client,
            SecurityWebApplicationFactory.ViewerUserName,
            SecurityWebApplicationFactory.TestPassword);

        var dashboard = await client.GetAsync("/");
        var history = await client.GetAsync("/runs");
        var assistant = await client.GetAsync("/knowledge-assistant");
        var download = await client.GetAsync("/downloads/reports/1");
        Assert.Equal(HttpStatusCode.OK, dashboard.StatusCode);
        Assert.Equal(HttpStatusCode.OK, history.StatusCode);
        Assert.Equal(HttpStatusCode.OK, assistant.StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, download.StatusCode);
    }

    [Fact]
    public async Task ReportUser_IsAllowedPermittedReportAccess()
    {
        Assert.True(await AuthorizeRoleAsync(AppRoles.ReportUser, AppPolicies.RequireReportAccess));
        Assert.True(await AuthorizeRoleAsync(AppRoles.ReportUser, AppPolicies.RequireReportGeneration));
        Assert.True(await AuthorizeRoleAsync(AppRoles.ReportUser, AppPolicies.RequireRagAccess));

        var client = AuthTestHttp.CreateClient(_factory);
        await AuthTestHttp.SignInAsync(
            client,
            SecurityWebApplicationFactory.ReportUserName,
            SecurityWebApplicationFactory.TestPassword);

        var dashboard = await client.GetAsync("/");
        var generate = await client.GetAsync("/generate");
        var history = await client.GetAsync("/runs");
        var assistant = await client.GetAsync("/knowledge-assistant");
        Assert.Equal(HttpStatusCode.OK, dashboard.StatusCode);
        Assert.Equal(HttpStatusCode.OK, generate.StatusCode);
        Assert.Equal(HttpStatusCode.OK, history.StatusCode);
        Assert.Equal(HttpStatusCode.OK, assistant.StatusCode);
    }

    private async Task<bool> AuthorizeRoleAsync(string role, string policy)
    {
        using var scope = _factory.Services.CreateScope();
        var authorization = scope.ServiceProvider.GetRequiredService<IAuthorizationService>();
        var user = new ClaimsPrincipal(new ClaimsIdentity(
            [new Claim(ClaimTypes.Name, role), new Claim(ClaimTypes.Role, role)],
            IdentityConstants.ApplicationScheme));
        var result = await authorization.AuthorizeAsync(user, policy);
        return result.Succeeded;
    }

    private async Task<bool> AuthorizeAnonymousAsync(string policy)
    {
        using var scope = _factory.Services.CreateScope();
        var authorization = scope.ServiceProvider.GetRequiredService<IAuthorizationService>();
        var result = await authorization.AuthorizeAsync(new ClaimsPrincipal(new ClaimsIdentity()), policy);
        return result.Succeeded;
    }

    private static void AssertDeniedToSignIn(HttpResponseMessage response)
    {
        Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
        Assert.Contains("/signin", response.Headers.Location?.ToString(), StringComparison.OrdinalIgnoreCase);
    }

    private static void AssertDeniedToAccessDenied(HttpResponseMessage response)
    {
        Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
        Assert.Contains("/denied", response.Headers.Location?.ToString(), StringComparison.OrdinalIgnoreCase);
    }
}
