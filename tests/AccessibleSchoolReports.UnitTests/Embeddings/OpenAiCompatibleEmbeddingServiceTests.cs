using System.Net;
using System.Net.Http;
using System.Text;
using AccessibleSchoolReports.Application.Knowledge;
using AccessibleSchoolReports.Application.Security;
using AccessibleSchoolReports.Infrastructure.Embeddings;
using AccessibleSchoolReports.Infrastructure.Security;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace AccessibleSchoolReports.UnitTests.Embeddings;

public sealed class OpenAiCompatibleEmbeddingServiceTests
{
    [Fact]
    public async Task EmbedPermittedChunks_SendsOnlyAuthorizedText_AndRecordsModel()
    {
        await using var fixture = await EmbeddingTestFixture.CreateAsync();
        var handler = ScriptedHandler.Success(dimension: 4, count: 1);
        var logger = new ListLogger();
        var service = CreateService(fixture, handler, logger);
        var user = EmbeddingTestFixture.Principal("user-a", AppRoles.ReportUser);

        var result = await service.EmbedPermittedChunksAsync(
            user,
            [fixture.SchoolAChunkId, fixture.SchoolBChunkId]);

        var body = Assert.Single(handler.Bodies);
        Assert.Contains(fixture.SchoolAText, body, StringComparison.Ordinal);
        Assert.DoesNotContain(EmbeddingTestFixture.SchoolBSecret, body, StringComparison.Ordinal);
        Assert.Equal([fixture.SchoolBChunkId], result.SkippedUnauthorizedChunkIds);
        Assert.Equal(fixture.SchoolAChunkId, Assert.Single(result.Embedded).ChunkId);
        Assert.Equal(4, result.Dimensions);
        Assert.Contains("test-embed", result.Model, StringComparison.Ordinal);
        Assert.All(logger.Messages, message => Assert.DoesNotContain("super-secret-key", message, StringComparison.Ordinal));
        Assert.Equal("Bearer", handler.Requests[0].Headers.Authorization?.Scheme);
    }

    [Fact]
    public async Task EmbedPermittedChunks_RetriesOnRateLimit()
    {
        await using var fixture = await EmbeddingTestFixture.CreateAsync();
        var handler = new ScriptedHandler();
        handler.Enqueue(HttpStatusCode.TooManyRequests, "{}", retryAfterSeconds: 0);
        handler.EnqueueSuccess(dimension: 4, count: 1);
        var service = CreateService(fixture, handler);

        var result = await service.EmbedPermittedChunksAsync(
            EmbeddingTestFixture.Principal("admin", AppRoles.Admin),
            [fixture.SchoolAChunkId]);

        Assert.Equal(2, handler.Requests.Count);
        Assert.Single(result.Embedded);
    }

    [Fact]
    public async Task EmbedPermittedChunks_RetriesTransientError()
    {
        await using var fixture = await EmbeddingTestFixture.CreateAsync();
        var handler = new ScriptedHandler();
        handler.Enqueue(HttpStatusCode.ServiceUnavailable, "{}");
        handler.EnqueueSuccess(dimension: 4, count: 1);
        var service = CreateService(fixture, handler);

        var result = await service.EmbedPermittedChunksAsync(
            EmbeddingTestFixture.Principal("admin", AppRoles.Admin),
            [fixture.LegacyChunkId]);

        Assert.Equal(2, handler.Requests.Count);
        Assert.Single(result.Embedded);
    }

    [Fact]
    public async Task EmbedQuery_TimesOut()
    {
        await using var fixture = await EmbeddingTestFixture.CreateAsync();
        fixture.Options.TimeoutSeconds = 1;
        var handler = new ScriptedHandler { Delay = TimeSpan.FromSeconds(5) };
        handler.EnqueueSuccess(dimension: 4, count: 1);
        var service = CreateService(fixture, handler);

        await Assert.ThrowsAsync<EmbeddingTimeoutException>(
            () => service.EmbedQueryAsync("timeout please"));
    }

    [Fact]
    public async Task EmbedQuery_HonorsCancellation()
    {
        await using var fixture = await EmbeddingTestFixture.CreateAsync();
        var handler = new ScriptedHandler { Delay = TimeSpan.FromSeconds(5) };
        handler.EnqueueSuccess(dimension: 4, count: 1);
        var service = CreateService(fixture, handler);
        using var cts = new CancellationTokenSource();
        await cts.CancelAsync();

        await Assert.ThrowsAnyAsync<OperationCanceledException>(
            () => service.EmbedQueryAsync("cancel please", cts.Token));
    }

    [Fact]
    public async Task MissingApiKey_DoesNotCallProvider()
    {
        await using var fixture = await EmbeddingTestFixture.CreateAsync();
        fixture.Options.ApiKey = "";
        var handler = new ScriptedHandler();
        var service = CreateService(fixture, handler);

        await Assert.ThrowsAsync<EmbeddingConfigurationException>(
            () => service.EmbedQueryAsync("no key"));
        Assert.Empty(handler.Requests);
    }

    [Fact]
    public async Task WrongDimension_IsRejected()
    {
        await using var fixture = await EmbeddingTestFixture.CreateAsync();
        var handler = ScriptedHandler.Success(dimension: 2, count: 1);
        var service = CreateService(fixture, handler);

        await Assert.ThrowsAsync<EmbeddingDimensionException>(
            () => service.EmbedQueryAsync("dimension mismatch"));
    }

    private static OpenAiCompatibleEmbeddingService CreateService(
        EmbeddingTestFixture fixture,
        ScriptedHandler handler,
        ILogger<OpenAiCompatibleEmbeddingService>? logger = null)
    {
        var client = new HttpClient(handler) { Timeout = TimeSpan.FromSeconds(10) };
        return new OpenAiCompatibleEmbeddingService(
            client,
            Options.Create(fixture.Options),
            new EmbeddingTestFixture.Factory(fixture.DbOptions),
            new ReportAuthorizationService(fixture.Db),
            logger ?? new ListLogger());
    }

    private sealed class ScriptedHandler : HttpMessageHandler
    {
        private readonly Queue<Func<HttpRequestMessage, HttpResponseMessage>> _responses = new();

        public List<HttpRequestMessage> Requests { get; } = [];

        public List<string> Bodies { get; } = [];

        public TimeSpan Delay { get; set; }

        public static ScriptedHandler Success(int dimension, int count)
        {
            var handler = new ScriptedHandler();
            handler.EnqueueSuccess(dimension, count);
            return handler;
        }

        public void EnqueueSuccess(int dimension, int count)
        {
            var items = string.Join(
                ",",
                Enumerable.Range(0, count).Select(index =>
                {
                    var values = string.Join(",", Enumerable.Range(0, dimension).Select(i => (i + 1) * 0.01));
                    return $"{{\"index\":{index},\"embedding\":[{values}]}}";
                }));
            Enqueue(HttpStatusCode.OK, $"{{\"model\":\"test-embed\",\"data\":[{items}]}}");
        }

        public void Enqueue(HttpStatusCode status, string body, int? retryAfterSeconds = null)
        {
            _responses.Enqueue(_ =>
            {
                var response = new HttpResponseMessage(status)
                {
                    Content = new StringContent(body, Encoding.UTF8, "application/json"),
                };
                if (retryAfterSeconds is int seconds)
                {
                    response.Headers.RetryAfter = new System.Net.Http.Headers.RetryConditionHeaderValue(
                        TimeSpan.FromSeconds(seconds));
                }

                return response;
            });
        }

        protected override async Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            if (Delay > TimeSpan.Zero)
            {
                await Task.Delay(Delay, cancellationToken);
            }

            Requests.Add(request);
            Bodies.Add(request.Content is null ? string.Empty : await request.Content.ReadAsStringAsync(cancellationToken));
            return _responses.Dequeue()(request);
        }
    }

    private sealed class ListLogger : ILogger<OpenAiCompatibleEmbeddingService>
    {
        public List<string> Messages { get; } = [];

        public IDisposable BeginScope<TState>(TState state)
            where TState : notnull => NullScope.Instance;

        public bool IsEnabled(LogLevel logLevel) => true;

        public void Log<TState>(
            LogLevel logLevel,
            EventId eventId,
            TState state,
            Exception? exception,
            Func<TState, Exception?, string> formatter)
        {
            Messages.Add(formatter(state, exception));
            if (exception is not null)
            {
                Messages.Add(exception.ToString());
            }
        }

        private sealed class NullScope : IDisposable
        {
            public static readonly NullScope Instance = new();

            public void Dispose()
            {
            }
        }
    }
}
