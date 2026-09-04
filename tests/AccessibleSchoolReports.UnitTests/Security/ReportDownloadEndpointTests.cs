using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;

namespace AccessibleSchoolReports.UnitTests.Security;

public sealed class ReportDownloadEndpointTests : IClassFixture<ReportDownloadWebApplicationFactory>
{
    private readonly ReportDownloadWebApplicationFactory _factory;

    public ReportDownloadEndpointTests(ReportDownloadWebApplicationFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task AuthorizedPdfDownload_ReturnsPdfBytes()
    {
        var client = await SignInAsync(ReportDownloadWebApplicationFactory.AdminUserName);

        var response = await client.GetAsync($"/downloads/reports/{_factory.ReportAId}");

        await AssertPdfAsync(response, ReportDownloadWebApplicationFactory.SchoolAPdfBytes, "10701-summary-report.pdf");
    }

    [Fact]
    public async Task UnauthorizedPdfDownload_ReturnsNotFound_WithoutMetadata()
    {
        var client = await SignInAsync(ReportDownloadWebApplicationFactory.ViewerUserName);

        var response = await client.GetAsync($"/downloads/reports/{_factory.ReportBId}");

        await AssertHiddenAsync(response);
    }

    [Fact]
    public async Task InvalidReportId_ReturnsNotFound()
    {
        var client = await SignInAsync(ReportDownloadWebApplicationFactory.AdminUserName);

        var response = await client.GetAsync("/downloads/reports/99999");

        await AssertHiddenAsync(response);
    }

    [Fact]
    public async Task PathTraversalAttempt_DoesNotReadArbitraryFiles()
    {
        var client = await SignInAsync(ReportDownloadWebApplicationFactory.AdminUserName);

        var traversalUrls = new[]
        {
            "/downloads/reports/..%2F..%2Fsecret.pdf",
            "/downloads/reports/%2e%2e/%2e%2e/secret.pdf",
            $"/downloads/reports/{_factory.TraversalStoredReportId}",
            $"/downloads/reports/{_factory.ReportAId}?path=..%2F..%2Fsecret.pdf",
        };

        foreach (var url in traversalUrls)
        {
            var response = await client.GetAsync(url);
            if (url.Contains($"/{_factory.ReportAId}", StringComparison.Ordinal))
            {
                await AssertPdfAsync(
                    response,
                    ReportDownloadWebApplicationFactory.SchoolAPdfBytes,
                    "10701-summary-report.pdf");
                continue;
            }

            await AssertHiddenAsync(response);
        }

        var decorativeTraversal = await client.GetAsync(
            $"/downloads/reports/{_factory.ReportAId}/%2e%2e%2f%2e%2e%2fsecret.pdf");
        await AssertPdfAsync(
            decorativeTraversal,
            ReportDownloadWebApplicationFactory.SchoolAPdfBytes,
            "10701-summary-report.pdf");
    }

    [Fact]
    public async Task MissingPdf_ReturnsNotFound()
    {
        var client = await SignInAsync(ReportDownloadWebApplicationFactory.AdminUserName);

        var response = await client.GetAsync($"/downloads/reports/{_factory.MissingPdfReportId}");

        await AssertHiddenAsync(response);
    }

    [Fact]
    public async Task DeletedPdf_ReturnsNotFound()
    {
        var client = await SignInAsync(ReportDownloadWebApplicationFactory.AdminUserName);

        var response = await client.GetAsync($"/downloads/reports/{_factory.DeletedPdfReportId}");

        await AssertHiddenAsync(response);
    }

    [Fact]
    public async Task AdminAccess_CanDownloadBothSchools()
    {
        var client = await SignInAsync(ReportDownloadWebApplicationFactory.AdminUserName);

        var schoolA = await client.GetAsync($"/downloads/reports/{_factory.ReportAId}");
        var schoolB = await client.GetAsync($"/downloads/reports/{_factory.ReportBId}");

        await AssertPdfAsync(schoolA, ReportDownloadWebApplicationFactory.SchoolAPdfBytes, "10701-summary-report.pdf");
        await AssertPdfAsync(schoolB, ReportDownloadWebApplicationFactory.SchoolBPdfBytes, "23306-summary-report.pdf");
    }

    [Fact]
    public async Task ViewerAccess_CanDownloadAssignedSchoolOnly()
    {
        var client = await SignInAsync(ReportDownloadWebApplicationFactory.ViewerUserName);

        var assigned = await client.GetAsync($"/downloads/reports/{_factory.ReportAId}");
        var other = await client.GetAsync($"/downloads/reports/{_factory.ReportBId}");

        await AssertPdfAsync(assigned, ReportDownloadWebApplicationFactory.SchoolAPdfBytes, "10701-summary-report.pdf");
        await AssertHiddenAsync(other);
    }

    [Fact]
    public async Task ReportUserAccess_CanDownloadAssignedSchoolOnly()
    {
        var client = await SignInAsync(ReportDownloadWebApplicationFactory.ReportUserName);

        var assigned = await client.GetAsync($"/downloads/reports/{_factory.ReportAId}");
        var other = await client.GetAsync($"/downloads/reports/{_factory.ReportBId}");

        await AssertPdfAsync(assigned, ReportDownloadWebApplicationFactory.SchoolAPdfBytes, "10701-summary-report.pdf");
        await AssertHiddenAsync(other);
    }

    [Fact]
    public async Task OutputFolder_IsNotAPublicStaticDirectory()
    {
        var client = await SignInAsync(ReportDownloadWebApplicationFactory.AdminUserName);

        var response = await client.GetAsync("/output/2025/10701/summary-report.pdf");
        var body = await response.Content.ReadAsByteArrayAsync();

        Assert.NotEqual("application/pdf", response.Content.Headers.ContentType?.MediaType);
        Assert.False(StartsWithPdf(body));
    }

    [Fact]
    public async Task AnonymousDownload_IsRedirectedToSignIn()
    {
        var client = AuthTestHttp.CreateClient(_factory);

        var response = await client.GetAsync($"/downloads/reports/{_factory.ReportAId}");

        Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
        Assert.Contains("/signin", response.Headers.Location?.ToString(), StringComparison.OrdinalIgnoreCase);
    }

    private async Task<HttpClient> SignInAsync(string userName)
    {
        var client = AuthTestHttp.CreateClient(_factory);
        var signedIn = await AuthTestHttp.SignInAsync(
            client,
            userName,
            ReportDownloadWebApplicationFactory.TestPassword);
        Assert.Equal(HttpStatusCode.Redirect, signedIn.StatusCode);
        return client;
    }

    private async Task AssertHiddenAsync(HttpResponseMessage response)
    {
        var body = await response.Content.ReadAsStringAsync();
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        Assert.NotEqual("application/pdf", response.Content.Headers.ContentType?.MediaType);
        Assert.DoesNotContain(ReportDownloadWebApplicationFactory.SchoolAName, body, StringComparison.Ordinal);
        Assert.DoesNotContain(ReportDownloadWebApplicationFactory.SchoolBName, body, StringComparison.Ordinal);
        Assert.DoesNotContain(_factory.OutputRoot, body, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("summary-report.pdf", body, StringComparison.OrdinalIgnoreCase);
    }

    private static async Task AssertPdfAsync(HttpResponseMessage response, byte[] expected, string fileName)
    {
        var body = await response.Content.ReadAsByteArrayAsync();
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal("application/pdf", response.Content.Headers.ContentType?.MediaType);
        Assert.Equal(expected, body);
        Assert.True(StartsWithPdf(body));
        Assert.Contains(fileName, response.Content.Headers.ContentDisposition?.FileName?.Trim('"'));
        Assert.DoesNotContain("output", ContentDispositionDirectory(response.Content.Headers.ContentDisposition));
    }

    private static bool StartsWithPdf(byte[] body) =>
        body.Length >= 4 && Encoding.ASCII.GetString(body, 0, 4) == "%PDF";

    private static string ContentDispositionDirectory(ContentDispositionHeaderValue? disposition) =>
        disposition?.FileName ?? string.Empty;
}
