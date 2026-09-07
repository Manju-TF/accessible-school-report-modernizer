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
        Assert.Contains("Suggested questions", html, StringComparison.Ordinal);
        Assert.Contains("<details", html, StringComparison.Ordinal);
        Assert.Contains("<summary", html, StringComparison.Ordinal);
        Assert.Contains("General", html, StringComparison.Ordinal);
        Assert.Contains("Business rules", html, StringComparison.Ordinal);
        Assert.Contains("All generated reports", html, StringComparison.Ordinal);
        Assert.Contains("Which SAS program generates the report?", html, StringComparison.Ordinal);
        Assert.Contains("What does a printed period mean on the report?", html, StringComparison.Ordinal);
        Assert.Contains("How is salary suppression handled?", html, StringComparison.Ordinal);
        Assert.Contains("Which rules appear to lack tests?", html, StringComparison.Ordinal);
        Assert.Contains("What do generated reports say about employment?", html, StringComparison.Ordinal);
        Assert.Contains("What employer types appear in generated reports I can view?", html, StringComparison.Ordinal);
        Assert.DoesNotContain("What gender counts are printed in this report?", html, StringComparison.Ordinal);
        Assert.Contains("for=\"assistant-question\"", html, StringComparison.Ordinal);
        Assert.Contains("aria-live", html, StringComparison.Ordinal);
        Assert.DoesNotContain("ApiKey", html, StringComparison.Ordinal);
        Assert.DoesNotContain("Bearer ", html, StringComparison.Ordinal);
        Assert.DoesNotContain("ChunkId", html, StringComparison.Ordinal);
        Assert.DoesNotContain("DocumentId", html, StringComparison.Ordinal);
        Assert.DoesNotContain(AppPolicies.RequireAdmin, html, StringComparison.Ordinal);
    }
}
