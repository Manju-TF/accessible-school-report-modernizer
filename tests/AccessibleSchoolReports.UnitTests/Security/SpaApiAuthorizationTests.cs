using System.Net;
using System.Text.Json;

namespace AccessibleSchoolReports.UnitTests.Security;

[Collection(SecurityCollection.Name)]
public sealed class SpaApiAuthorizationTests
{
    private readonly SecurityWebApplicationFactory _factory;

    public SpaApiAuthorizationTests(SecurityWebApplicationFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task Anonymous_ApiCalls_ReturnUnauthorized()
    {
        var client = AuthTestHttp.CreateClient(_factory);
        Assert.Equal(HttpStatusCode.Unauthorized, (await client.GetAsync("/api/me")).StatusCode);
        Assert.Equal(HttpStatusCode.Unauthorized, (await client.GetAsync("/api/dashboard")).StatusCode);
        Assert.Equal(HttpStatusCode.Unauthorized, (await client.GetAsync("/api/imports")).StatusCode);
        Assert.Equal(HttpStatusCode.Unauthorized, (await client.GetAsync("/api/schools")).StatusCode);
        Assert.Equal(HttpStatusCode.Unauthorized, (await client.GetAsync("/api/runs")).StatusCode);
        Assert.Equal(HttpStatusCode.Unauthorized, (await client.GetAsync("/api/assistant/suggestions")).StatusCode);
        Assert.Equal(HttpStatusCode.Unauthorized, (await client.PostAsync("/api/reports", null)).StatusCode);
        Assert.Equal(HttpStatusCode.Unauthorized, (await client.PostAsync("/api/assistant/ask", null)).StatusCode);
    }

    [Fact]
    public async Task Viewer_CannotCallAdminApis()
    {
        var client = AuthTestHttp.CreateClient(_factory);
        await AuthTestHttp.SignInAsync(
            client,
            SecurityWebApplicationFactory.ViewerUserName,
            SecurityWebApplicationFactory.TestPassword);

        Assert.Equal(HttpStatusCode.Forbidden, (await client.GetAsync("/api/imports")).StatusCode);
        Assert.Equal(HttpStatusCode.Forbidden, (await client.GetAsync("/api/reports/all")).StatusCode);
        Assert.Equal(HttpStatusCode.Forbidden, (await client.GetAsync("/api/schools")).StatusCode);
        var dashboard = await client.GetAsync("/api/dashboard");
        Assert.Equal(HttpStatusCode.OK, dashboard.StatusCode);
        using var payload = JsonDocument.Parse(await dashboard.Content.ReadAsStringAsync());
        Assert.True(payload.RootElement.TryGetProperty("generatedAtUtc", out _));
        Assert.True(payload.RootElement.TryGetProperty("years", out var years));
        Assert.Equal(JsonValueKind.Array, years.ValueKind);
        Assert.True(payload.RootElement.TryGetProperty("schools", out var schools));
        Assert.Equal(JsonValueKind.Array, schools.ValueKind);
        Assert.Equal(HttpStatusCode.OK, (await client.GetAsync("/api/me")).StatusCode);
    }

    [Fact]
    public async Task Admin_CanCallAdminApis()
    {
        var client = AuthTestHttp.CreateClient(_factory);
        await AuthTestHttp.SignInAsync(
            client,
            SecurityWebApplicationFactory.AdminUserName,
            SecurityWebApplicationFactory.TestPassword);

        Assert.Equal(HttpStatusCode.OK, (await client.GetAsync("/api/imports")).StatusCode);
        Assert.Equal(HttpStatusCode.OK, (await client.GetAsync("/api/reports/all")).StatusCode);
        Assert.Equal(HttpStatusCode.OK, (await client.GetAsync("/api/schools")).StatusCode);
    }
}
