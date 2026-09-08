using System.Net;

namespace AccessibleSchoolReports.UnitTests.Security;

public sealed class ReportDetailsPageTests : IClassFixture<ReportDownloadWebApplicationFactory>
{
    private readonly ReportDownloadWebApplicationFactory _factory;

    public ReportDetailsPageTests(ReportDownloadWebApplicationFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task AuthorizedUser_CanOpenSchoolAReportDetails()
    {
        var client = await SignInAsync(ReportDownloadWebApplicationFactory.ViewerUserName);

        var response = await client.GetAsync($"/api/reports/{_factory.ReportAId}");
        var json = await response.Content.ReadAsStringAsync();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Contains("10701", json, StringComparison.Ordinal);
        Assert.DoesNotContain(ReportDownloadWebApplicationFactory.SchoolBName, json, StringComparison.Ordinal);
        Assert.DoesNotContain("23306", json, StringComparison.Ordinal);
    }

    [Fact]
    public async Task UnauthorizedUser_CannotOpenSchoolBReportDetails()
    {
        var client = await SignInAsync(ReportDownloadWebApplicationFactory.ViewerUserName);

        var response = await client.GetAsync($"/api/reports/{_factory.ReportBId}");
        var json = await response.Content.ReadAsStringAsync();

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        Assert.Contains("That report is not available.", json, StringComparison.Ordinal);
        Assert.DoesNotContain(ReportDownloadWebApplicationFactory.SchoolBName, json, StringComparison.Ordinal);
        Assert.DoesNotContain("23306", json, StringComparison.Ordinal);
        Assert.DoesNotContain("Not authorized", json, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task TamperedReportId_DoesNotRevealUnauthorizedReport()
    {
        var client = await SignInAsync(ReportDownloadWebApplicationFactory.ViewerUserName);
        var authorized = await client.GetAsync($"/api/reports/{_factory.ReportAId}");
        Assert.Equal(HttpStatusCode.OK, authorized.StatusCode);

        var tampered = await client.GetAsync($"/api/reports/{_factory.ReportBId}");
        var json = await tampered.Content.ReadAsStringAsync();
        Assert.Contains("That report is not available.", json, StringComparison.Ordinal);
        Assert.DoesNotContain(ReportDownloadWebApplicationFactory.SchoolBName, json, StringComparison.Ordinal);

        var assistant = await client.GetAsync($"/api/assistant/context?report={_factory.ReportBId}");
        var assistantJson = await assistant.Content.ReadAsStringAsync();
        Assert.Equal(HttpStatusCode.NotFound, assistant.StatusCode);
        Assert.DoesNotContain(ReportDownloadWebApplicationFactory.SchoolBName, assistantJson, StringComparison.Ordinal);
        Assert.DoesNotContain("23306", assistantJson, StringComparison.Ordinal);
        Assert.DoesNotContain("ApiKey", assistantJson, StringComparison.Ordinal);
    }

    [Fact]
    public async Task Anonymous_IsRedirectedToSignIn()
    {
        var client = AuthTestHttp.CreateClient(_factory);

        var page = await client.GetAsync($"/reports/{_factory.ReportAId}");
        Assert.Equal(HttpStatusCode.Redirect, page.StatusCode);
        Assert.Contains("/signin", page.Headers.Location?.ToString(), StringComparison.OrdinalIgnoreCase);

        var api = await client.GetAsync($"/api/reports/{_factory.ReportAId}");
        Assert.Equal(HttpStatusCode.Unauthorized, api.StatusCode);
    }

    private async Task<HttpClient> SignInAsync(string userName)
    {
        var client = AuthTestHttp.CreateClient(_factory);
        await AuthTestHttp.SignInAsync(client, userName, ReportDownloadWebApplicationFactory.TestPassword);
        return client;
    }
}
