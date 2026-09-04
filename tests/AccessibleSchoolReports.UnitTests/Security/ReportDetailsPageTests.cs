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

        var response = await client.GetAsync($"/reports/{_factory.ReportAId}");
        var html = await response.Content.ReadAsStringAsync();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Contains("Ask about this report", html, StringComparison.Ordinal);
        Assert.Contains("10701", html, StringComparison.Ordinal);
        Assert.DoesNotContain(ReportDownloadWebApplicationFactory.SchoolBName, html, StringComparison.Ordinal);
        Assert.DoesNotContain("23306", html, StringComparison.Ordinal);
    }

    [Fact]
    public async Task UnauthorizedUser_CannotOpenSchoolBReportDetails()
    {
        var client = await SignInAsync(ReportDownloadWebApplicationFactory.ViewerUserName);

        var response = await client.GetAsync($"/reports/{_factory.ReportBId}");
        var html = await response.Content.ReadAsStringAsync();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Contains("Report not found", html, StringComparison.Ordinal);
        Assert.Contains("That report is not available.", html, StringComparison.Ordinal);
        Assert.DoesNotContain("Ask about this report", html, StringComparison.Ordinal);
        Assert.DoesNotContain(ReportDownloadWebApplicationFactory.SchoolBName, html, StringComparison.Ordinal);
        Assert.DoesNotContain("23306", html, StringComparison.Ordinal);
        Assert.DoesNotContain("Not authorized", html, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task TamperedReportId_DoesNotRevealUnauthorizedReport()
    {
        var client = await SignInAsync(ReportDownloadWebApplicationFactory.ViewerUserName);
        var authorized = await client.GetAsync($"/reports/{_factory.ReportAId}");
        Assert.Equal(HttpStatusCode.OK, authorized.StatusCode);

        var tampered = await client.GetAsync($"/reports/{_factory.ReportBId}");
        var html = await tampered.Content.ReadAsStringAsync();
        Assert.Contains("That report is not available.", html, StringComparison.Ordinal);
        Assert.DoesNotContain(ReportDownloadWebApplicationFactory.SchoolBName, html, StringComparison.Ordinal);

        var assistant = await client.GetAsync($"/knowledge-assistant?report={_factory.ReportBId}");
        var assistantHtml = await assistant.Content.ReadAsStringAsync();
        Assert.Equal(HttpStatusCode.OK, assistant.StatusCode);
        Assert.DoesNotContain(ReportDownloadWebApplicationFactory.SchoolBName, assistantHtml, StringComparison.Ordinal);
        Assert.DoesNotContain("23306", assistantHtml, StringComparison.Ordinal);
        Assert.DoesNotContain("ApiKey", assistantHtml, StringComparison.Ordinal);
    }

    [Fact]
    public async Task Anonymous_IsRedirectedToSignIn()
    {
        var client = AuthTestHttp.CreateClient(_factory);

        var response = await client.GetAsync($"/reports/{_factory.ReportAId}");

        Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
        Assert.Contains("/signin", response.Headers.Location?.ToString(), StringComparison.OrdinalIgnoreCase);
    }

    private async Task<HttpClient> SignInAsync(string userName)
    {
        var client = AuthTestHttp.CreateClient(_factory);
        await AuthTestHttp.SignInAsync(client, userName, ReportDownloadWebApplicationFactory.TestPassword);
        return client;
    }
}
