using System.Net;
using AccessibleSchoolReports.Application.Security;

namespace AccessibleSchoolReports.UnitTests.Security;

[Collection(SecurityCollection.Name)]
public sealed class KnowledgeAssistantPageTests
{
    private readonly SecurityWebApplicationFactory _factory;

    public KnowledgeAssistantPageTests(SecurityWebApplicationFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task Viewer_SeesAssistantChrome_WithoutSecretsOrInternalIds()
    {
        var client = AuthTestHttp.CreateClient(_factory);
        await AuthTestHttp.SignInAsync(
            client,
            SecurityWebApplicationFactory.ViewerUserName,
            SecurityWebApplicationFactory.TestPassword);

        var response = await client.GetAsync("/knowledge-assistant");
        var html = await response.Content.ReadAsStringAsync();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Contains("Knowledge Assistant", html, StringComparison.Ordinal);
        Assert.Contains("Question", html, StringComparison.Ordinal);
        Assert.Contains("Ask", html, StringComparison.Ordinal);
        Assert.Contains("How is salary suppression handled?", html, StringComparison.Ordinal);
        Assert.Contains("for=\"assistant-question\"", html, StringComparison.Ordinal);
        Assert.Contains("aria-live", html, StringComparison.Ordinal);
        Assert.DoesNotContain("ApiKey", html, StringComparison.Ordinal);
        Assert.DoesNotContain("Bearer ", html, StringComparison.Ordinal);
        Assert.DoesNotContain("ChunkId", html, StringComparison.Ordinal);
        Assert.DoesNotContain("DocumentId", html, StringComparison.Ordinal);
        Assert.DoesNotContain(AppPolicies.RequireAdmin, html, StringComparison.Ordinal);
    }
}
