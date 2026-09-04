using System.Net;
using System.Security.Claims;
using AccessibleSchoolReports.Application.Security;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;

namespace AccessibleSchoolReports.UnitTests.Security;

[Collection(SecurityCollection.Name)]
public sealed class RoleAuthorizationTests
{
    private readonly SecurityWebApplicationFactory _factory;

    public RoleAuthorizationTests(SecurityWebApplicationFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task Startup_CreatesTheThreeApplicationRoles()
    {
        using var scope = _factory.Services.CreateScope();
        var roles = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();

        Assert.True(await roles.RoleExistsAsync(AppRoles.Admin));
        Assert.True(await roles.RoleExistsAsync(AppRoles.ReportUser));
        Assert.True(await roles.RoleExistsAsync(AppRoles.Viewer));
    }

    [Theory]
    [InlineData("/", SecurityWebApplicationFactory.ViewerUserName, HttpStatusCode.OK)]
    [InlineData("/runs", SecurityWebApplicationFactory.ViewerUserName, HttpStatusCode.OK)]
    [InlineData("/knowledge-assistant", SecurityWebApplicationFactory.ViewerUserName, HttpStatusCode.OK)]
    [InlineData("/import", SecurityWebApplicationFactory.ViewerUserName, HttpStatusCode.Redirect)]
    [InlineData("/generate", SecurityWebApplicationFactory.ViewerUserName, HttpStatusCode.Redirect)]
    [InlineData("/generate-all", SecurityWebApplicationFactory.ViewerUserName, HttpStatusCode.Redirect)]
    [InlineData("/", SecurityWebApplicationFactory.ReportUserName, HttpStatusCode.OK)]
    [InlineData("/runs", SecurityWebApplicationFactory.ReportUserName, HttpStatusCode.OK)]
    [InlineData("/knowledge-assistant", SecurityWebApplicationFactory.ReportUserName, HttpStatusCode.OK)]
    [InlineData("/generate", SecurityWebApplicationFactory.ReportUserName, HttpStatusCode.OK)]
    [InlineData("/import", SecurityWebApplicationFactory.ReportUserName, HttpStatusCode.Redirect)]
    [InlineData("/generate-all", SecurityWebApplicationFactory.ReportUserName, HttpStatusCode.Redirect)]
    [InlineData("/", SecurityWebApplicationFactory.AdminUserName, HttpStatusCode.OK)]
    [InlineData("/knowledge-assistant", SecurityWebApplicationFactory.AdminUserName, HttpStatusCode.OK)]
    [InlineData("/import", SecurityWebApplicationFactory.AdminUserName, HttpStatusCode.OK)]
    [InlineData("/generate", SecurityWebApplicationFactory.AdminUserName, HttpStatusCode.OK)]
    [InlineData("/generate-all", SecurityWebApplicationFactory.AdminUserName, HttpStatusCode.OK)]
    [InlineData("/runs", SecurityWebApplicationFactory.AdminUserName, HttpStatusCode.OK)]
    public async Task Role_CanAccessOnlyPermittedPages(string path, string userName, HttpStatusCode expected)
    {
        var client = AuthTestHttp.CreateClient(_factory);
        await AuthTestHttp.SignInAsync(client, userName, SecurityWebApplicationFactory.TestPassword);

        var response = await client.GetAsync(path);

        Assert.Equal(expected, response.StatusCode);
        if (expected == HttpStatusCode.Redirect)
        {
            Assert.Contains("/denied", response.Headers.Location?.ToString(), StringComparison.OrdinalIgnoreCase);
        }
    }

    [Fact]
    public async Task ViewerDownload_IsAuthorizedButDoesNotBypassMissingFile()
    {
        var client = AuthTestHttp.CreateClient(_factory);
        await AuthTestHttp.SignInAsync(
            client,
            SecurityWebApplicationFactory.ViewerUserName,
            SecurityWebApplicationFactory.TestPassword);

        var response = await client.GetAsync("/downloads/reports/1");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Theory]
    [InlineData(AppRoles.Viewer, AppPolicies.RequireReportAccess, true)]
    [InlineData(AppRoles.Viewer, AppPolicies.RequireRagAccess, true)]
    [InlineData(AppRoles.Viewer, AppPolicies.RequireReportGeneration, false)]
    [InlineData(AppRoles.Viewer, AppPolicies.RequireAdmin, false)]
    [InlineData(AppRoles.ReportUser, AppPolicies.RequireReportAccess, true)]
    [InlineData(AppRoles.ReportUser, AppPolicies.RequireReportGeneration, true)]
    [InlineData(AppRoles.ReportUser, AppPolicies.RequireRagAccess, true)]
    [InlineData(AppRoles.ReportUser, AppPolicies.RequireAdmin, false)]
    [InlineData(AppRoles.Admin, AppPolicies.RequireReportAccess, true)]
    [InlineData(AppRoles.Admin, AppPolicies.RequireReportGeneration, true)]
    [InlineData(AppRoles.Admin, AppPolicies.RequireRagAccess, true)]
    [InlineData(AppRoles.Admin, AppPolicies.RequireAdmin, true)]
    public async Task Policy_MatchesRoleMatrix(string role, string policy, bool allowed)
    {
        using var scope = _factory.Services.CreateScope();
        var authorization = scope.ServiceProvider.GetRequiredService<IAuthorizationService>();
        var user = new ClaimsPrincipal(new ClaimsIdentity(
            [new Claim(ClaimTypes.Name, role), new Claim(ClaimTypes.Role, role)],
            IdentityConstants.ApplicationScheme));

        var result = await authorization.AuthorizeAsync(user, policy);

        Assert.Equal(allowed, result.Succeeded);
    }
}
