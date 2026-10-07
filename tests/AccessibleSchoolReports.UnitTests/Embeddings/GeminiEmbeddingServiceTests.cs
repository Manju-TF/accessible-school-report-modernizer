using System.Net;
using System.Text;
using System.Text.Json;
using AccessibleSchoolReports.Application.Knowledge;
using AccessibleSchoolReports.Application.Security;
using AccessibleSchoolReports.Infrastructure.Embeddings;
using AccessibleSchoolReports.Infrastructure.Security;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;

namespace AccessibleSchoolReports.UnitTests.Embeddings;

public sealed class GeminiEmbeddingServiceTests
{
    [Fact]
    public async Task EmbedPermittedChunks_UsesGeminiBatchApiAndSendsOnlyAuthorizedText()
    {
        await using var fixture = await EmbeddingTestFixture.CreateAsync();
        ConfigureGemini(fixture.Options);
        var handler = new ScriptedHandler();
        handler.EnqueueVectors(4, 1);
        var service = CreateService(fixture, handler);

        var result = await service.EmbedPermittedChunksAsync(
            EmbeddingTestFixture.Principal("user-a", AppRoles.ReportUser),
            [fixture.SchoolAChunkId, fixture.SchoolBChunkId]);

        var request = Assert.Single(handler.Requests);
        Assert.Equal(
            "https://generativelanguage.googleapis.com/v1beta/models/gemini-embedding-001:batchEmbedContents",
            request.Request.RequestUri!.ToString());
        Assert.True(request.Request.Headers.Contains("x-goog-api-key"));
        Assert.False(request.Request.Headers.Contains("Authorization"));
        var body = JsonDocument.Parse(request.Body).RootElement;
        var embedRequest = Assert.Single(body.GetProperty("requests").EnumerateArray());
        Assert.Equal("models/gemini-embedding-001", embedRequest.GetProperty("model").GetString());
        Assert.Equal("RETRIEVAL_DOCUMENT", embedRequest.GetProperty("taskType").GetString());
        Assert.Equal(4, embedRequest.GetProperty("outputDimensionality").GetInt32());
        var sentText = embedRequest.GetProperty("content")
            .GetProperty("parts")[0]
            .GetProperty("text")
            .GetString();
        Assert.Equal(fixture.SchoolAText, sentText);
        Assert.DoesNotContain(EmbeddingTestFixture.SchoolBSecret, request.Body, StringComparison.Ordinal);
        Assert.Equal([fixture.SchoolBChunkId], result.SkippedUnauthorizedChunkIds);
        Assert.Equal(fixture.SchoolAChunkId, Assert.Single(result.Embedded).ChunkId);
        Assert.Equal("Gemini", result.Provider);
    }

    [Fact]
    public async Task EmbedQuery_UsesRetrievalQueryTaskType()
    {
        await using var fixture = await EmbeddingTestFixture.CreateAsync();
        ConfigureGemini(fixture.Options);
        var handler = new ScriptedHandler();
        handler.EnqueueVectors(4, 1);
        var service = CreateService(fixture, handler);

        var result = await service.EmbedQueryAsync("How is employment status counted?");

        var body = JsonDocument.Parse(Assert.Single(handler.Requests).Body).RootElement;
        var embedRequest = Assert.Single(body.GetProperty("requests").EnumerateArray());
        Assert.Equal("RETRIEVAL_QUERY", embedRequest.GetProperty("taskType").GetString());
        Assert.Equal(4, result.Dimensions);
        Assert.Equal(4, result.Values.Length);
    }

    [Fact]
    public async Task EmbedQuery_RejectsWrongVectorSize()
    {
        await using var fixture = await EmbeddingTestFixture.CreateAsync();
        ConfigureGemini(fixture.Options);
        var handler = new ScriptedHandler();
        handler.EnqueueVectors(3, 1);
        var service = CreateService(fixture, handler);

        await Assert.ThrowsAsync<EmbeddingDimensionException>(
            () => service.EmbedQueryAsync("dimension mismatch"));
    }

    private static void ConfigureGemini(EmbeddingOptions options)
    {
        options.Provider = "Gemini";
        options.Endpoint = "https://generativelanguage.googleapis.com/v1beta";
        options.Model = "gemini-embedding-001";
        options.Dimensions = 4;
    }

    private static GeminiEmbeddingService CreateService(
        EmbeddingTestFixture fixture,
        ScriptedHandler handler) =>
        new(
            new HttpClient(handler),
            Options.Create(fixture.Options),
            new EmbeddingTestFixture.Factory(fixture.DbOptions),
            new ReportAuthorizationService(fixture.Db),
            NullLogger<GeminiEmbeddingService>.Instance);

    private sealed class ScriptedHandler : HttpMessageHandler
    {
        public List<CapturedRequest> Requests { get; } = [];

        public void EnqueueVectors(int dimensions, int count) =>
            ResponseBodies.Enqueue(BuildResponse(dimensions, count));

        private readonly Queue<string> ResponseBodies = new();

        protected override async Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            var body = request.Content is null
                ? string.Empty
                : await request.Content.ReadAsStringAsync(cancellationToken);
            Requests.Add(new CapturedRequest(request, body));
            return new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(ResponseBodies.Dequeue(), Encoding.UTF8, "application/json"),
            };
        }

        private static string BuildResponse(int dimensions, int count)
        {
            var vectors = string.Join(
                ",",
                Enumerable.Range(0, count).Select(index =>
                {
                    var values = string.Join(",", Enumerable.Range(0, dimensions).Select(value => value + 0.01 * (index + 1)));
                    return $"{{\"values\":[{values}]}}";
                }));
            return $"{{\"embeddings\":[{vectors}]}}";
        }
    }

    private sealed record CapturedRequest(HttpRequestMessage Request, string Body);
}
