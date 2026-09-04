using System.Net;
using System.Net.Http;
using System.Text;
using AccessibleSchoolReports.Application.Knowledge;
using AccessibleSchoolReports.Infrastructure.LanguageModels;
using AccessibleSchoolReports.UnitTests.Knowledge;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace AccessibleSchoolReports.UnitTests.LanguageModels;

public sealed class OpenAiCompatibleLanguageModelServiceTests
{
    [Fact]
    public async Task Complete_SendsSystemAndUntrustedUserRoles_WithoutLiveCall()
    {
        var handler = ScriptedHandler.Success("cited answer");
        var logger = new ListLogger();
        var service = CreateService(handler, logger);
        var request = KnowledgeGroundedPrompt.Create(
            "What is CF-S-00?",
            [
                new KnowledgeRetrievalHit
                {
                    ChunkId = 1,
                    DocumentId = 1,
                    Content = KnowledgeGroundedPromptTests.SasInjection,
                    RuleId = "CF-S-00",
                    SourceLocation = "legacy/sas/cf200.sas:1",
                    SourceIdentifier = "cf200.sas",
                    FileName = "cf200.sas",
                    DocumentType = Domain.Knowledge.KnowledgeDocumentType.Legacy,
                    AuthorizationScope = Domain.Knowledge.KnowledgeAuthorizationScope.Authenticated,
                    Similarity = 0.9f,
                },
            ]);

        var completion = await service.CompleteAsync(request);

        Assert.Equal("cited answer", completion.Text);
        Assert.Equal("OpenAICompatible", completion.Provider);
        var body = Assert.Single(handler.Bodies);
        Assert.Contains("\"role\":\"system\"", body, StringComparison.Ordinal);
        Assert.Contains("\"role\":\"user\"", body, StringComparison.Ordinal);
        Assert.DoesNotContain(KnowledgeGroundedPromptTests.SasInjection, SystemContent(body), StringComparison.Ordinal);
        Assert.Contains(KnowledgeGroundedPromptTests.SasInjection, body, StringComparison.Ordinal);
        Assert.Contains("UNTRUSTED PROJECT DATA", body, StringComparison.Ordinal);
        Assert.Contains("Do not invent business rules", body, StringComparison.Ordinal);
        Assert.Equal("Bearer", handler.Requests[0].Headers.Authorization?.Scheme);
        Assert.All(logger.Messages, message => Assert.DoesNotContain("super-secret-key", message, StringComparison.Ordinal));
    }

    [Fact]
    public async Task Complete_RetriesOnRateLimit()
    {
        var handler = new ScriptedHandler();
        handler.Enqueue(HttpStatusCode.TooManyRequests, "{}", retryAfterSeconds: 0);
        handler.EnqueueSuccess("ok");
        var service = CreateService(handler);

        var completion = await service.CompleteAsync(EmptyRequest());

        Assert.Equal(2, handler.Requests.Count);
        Assert.Equal("ok", completion.Text);
    }

    [Fact]
    public async Task Complete_TimesOut()
    {
        var options = DefaultOptions();
        options.TimeoutSeconds = 1;
        var handler = new ScriptedHandler { Delay = TimeSpan.FromSeconds(5) };
        handler.EnqueueSuccess("late");
        var service = CreateService(handler, options: options);

        await Assert.ThrowsAsync<LanguageModelTimeoutException>(
            () => service.CompleteAsync(EmptyRequest()));
    }

    [Fact]
    public async Task Complete_HonorsCancellation()
    {
        var handler = new ScriptedHandler { Delay = TimeSpan.FromSeconds(5) };
        handler.EnqueueSuccess("late");
        var service = CreateService(handler);
        using var cts = new CancellationTokenSource();
        await cts.CancelAsync();

        await Assert.ThrowsAnyAsync<OperationCanceledException>(
            () => service.CompleteAsync(EmptyRequest(), cts.Token));
    }

    [Fact]
    public async Task MissingApiKey_DoesNotCallProvider()
    {
        var options = DefaultOptions();
        options.ApiKey = "";
        var handler = new ScriptedHandler();
        var service = CreateService(handler, options: options);

        await Assert.ThrowsAsync<LanguageModelConfigurationException>(
            () => service.CompleteAsync(EmptyRequest()));
        Assert.Empty(handler.Requests);
    }

    private static OpenAiCompatibleLanguageModelService CreateService(
        ScriptedHandler handler,
        ILogger<OpenAiCompatibleLanguageModelService>? logger = null,
        LanguageModelOptions? options = null)
    {
        var client = new HttpClient(handler) { Timeout = TimeSpan.FromSeconds(10) };
        return new OpenAiCompatibleLanguageModelService(
            client,
            Options.Create(options ?? DefaultOptions()),
            logger ?? new ListLogger());
    }

    private static LanguageModelOptions DefaultOptions() =>
        new()
        {
            Provider = "OpenAICompatible",
            Endpoint = "https://llm.test/v1/chat/completions",
            Model = "gpt-4o-mini",
            TimeoutSeconds = 5,
            MaxRetries = 2,
            Temperature = 0,
            MaxTokens = 200,
            ApiKey = "super-secret-key",
        };

    private static LanguageModelRequest EmptyRequest() =>
        KnowledgeGroundedPrompt.Create("question", []);

    private static string SystemContent(string body)
    {
        var start = body.IndexOf("\"role\":\"system\"", StringComparison.Ordinal);
        var end = body.IndexOf("\"role\":\"user\"", StringComparison.Ordinal);
        return start >= 0 && end > start ? body[start..end] : string.Empty;
    }

    private sealed class ScriptedHandler : HttpMessageHandler
    {
        private readonly Queue<Func<HttpRequestMessage, HttpResponseMessage>> _responses = new();

        public List<HttpRequestMessage> Requests { get; } = [];

        public List<string> Bodies { get; } = [];

        public TimeSpan Delay { get; set; }

        public static ScriptedHandler Success(string text)
        {
            var handler = new ScriptedHandler();
            handler.EnqueueSuccess(text);
            return handler;
        }

        public void EnqueueSuccess(string text)
        {
            var escaped = text.Replace("\"", "\\\"", StringComparison.Ordinal);
            Enqueue(
                HttpStatusCode.OK,
                $"{{\"choices\":[{{\"message\":{{\"role\":\"assistant\",\"content\":\"{escaped}\"}}}}]}}");
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

    private sealed class ListLogger : ILogger<OpenAiCompatibleLanguageModelService>
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
