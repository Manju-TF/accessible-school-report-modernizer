using System.Net;
using System.Text.Json;
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

        var page = await client.GetAsync("/knowledge-assistant");
        Assert.Equal(HttpStatusCode.OK, page.StatusCode);

        var response = await client.GetAsync("/api/assistant/suggestions");
        var json = await response.Content.ReadAsStringAsync();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Contains("General", json, StringComparison.Ordinal);
        Assert.Contains("All generated reports", json, StringComparison.Ordinal);
        Assert.Contains("Compare reports", json, StringComparison.Ordinal);
        Assert.Contains("What is the sum of Total Reported across generated reports I can view?", json, StringComparison.Ordinal);
        Assert.Contains("What is the difference in Total Reported between Class of 2025 and last year in generated reports I can view?", json, StringComparison.Ordinal);
        Assert.Contains("What does a printed period mean on the report?", json, StringComparison.Ordinal);
        Assert.Contains("What do generated reports say about employment?", json, StringComparison.Ordinal);
        Assert.Contains("What employer types appear in generated reports I can view?", json, StringComparison.Ordinal);
        Assert.DoesNotContain("Business rules", json, StringComparison.Ordinal);
        Assert.DoesNotContain("How is salary suppression handled?", json, StringComparison.Ordinal);
        Assert.DoesNotContain("Which rules appear to lack tests?", json, StringComparison.Ordinal);
        Assert.DoesNotContain("What gender counts are printed in this report?", json, StringComparison.Ordinal);
        Assert.DoesNotContain("ApiKey", json, StringComparison.Ordinal);
        Assert.DoesNotContain("Bearer ", json, StringComparison.Ordinal);
        Assert.DoesNotContain("ChunkId", json, StringComparison.Ordinal);
        Assert.DoesNotContain("DocumentId", json, StringComparison.Ordinal);
        Assert.DoesNotContain(AppPolicies.RequireAdmin, json, StringComparison.Ordinal);

        using var document = JsonDocument.Parse(json);
        foreach (var group in document.RootElement.GetProperty("groups").EnumerateArray())
        {
            Assert.True(group.GetProperty("questions").GetArrayLength() >= 5);
        }
    }

    [Fact]
    public async Task Viewer_ReportScopedSuggestions_DoNotIncludeGlobalGroups()
    {
        var client = AuthTestHttp.CreateClient(_factory);
        await AuthTestHttp.SignInAsync(
            client,
            SecurityWebApplicationFactory.ViewerUserName,
            SecurityWebApplicationFactory.TestPassword);

        var response = await client.GetAsync("/api/assistant/suggestions?reportScoped=true");
        var json = await response.Content.ReadAsStringAsync();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Contains("This report", json, StringComparison.Ordinal);
        Assert.Contains("What gender counts are printed in this report?", json, StringComparison.Ordinal);
        Assert.DoesNotContain("General", json, StringComparison.Ordinal);
        Assert.DoesNotContain("Business rules", json, StringComparison.Ordinal);
        Assert.DoesNotContain("Which SAS program generates the report?", json, StringComparison.Ordinal);
        Assert.DoesNotContain("ApiKey", json, StringComparison.Ordinal);
        Assert.DoesNotContain("ChunkId", json, StringComparison.Ordinal);

        using var document = JsonDocument.Parse(json);
        var groups = document.RootElement.GetProperty("groups");
        Assert.Equal(1, groups.GetArrayLength());
        Assert.True(groups[0].GetProperty("questions").GetArrayLength() >= 5);
    }
}
