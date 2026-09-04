using System.Net;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using AccessibleSchoolReports.Application.Knowledge;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace AccessibleSchoolReports.Infrastructure.LanguageModels;

public sealed class OpenAiCompatibleLanguageModelService : ILanguageModelService
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
    };

    private readonly HttpClient _http;
    private readonly LanguageModelOptions _options;
    private readonly ILogger<OpenAiCompatibleLanguageModelService> _logger;

    public OpenAiCompatibleLanguageModelService(
        HttpClient http,
        IOptions<LanguageModelOptions> options,
        ILogger<OpenAiCompatibleLanguageModelService> logger)
    {
        _http = http;
        _options = options.Value;
        _logger = logger;
    }

    public LanguageModelInfo Model => new()
    {
        Provider = _options.Provider,
        Model = _options.Model,
    };

    public async Task<LanguageModelCompletion> CompleteAsync(
        LanguageModelRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        ArgumentException.ThrowIfNullOrWhiteSpace(request.SystemInstructions);
        EnsureConfigured();

        var maxAttempts = Math.Max(0, _options.MaxRetries) + 1;
        using var timeout = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        timeout.CancelAfter(TimeSpan.FromSeconds(Math.Clamp(_options.TimeoutSeconds, 1, 180)));

        Exception? last = null;
        for (var attempt = 0; attempt < maxAttempts; attempt++)
        {
            timeout.Token.ThrowIfCancellationRequested();
            try
            {
                using var httpRequest = CreateRequest(request);
                using var response = await _http.SendAsync(httpRequest, timeout.Token);
                if (response.IsSuccessStatusCode)
                {
                    var text = await ReadTextAsync(response, timeout.Token);
                    _logger.LogInformation(
                        "Language model completed. Provider={Provider} Model={Model} ContextDocuments={Count}",
                        _options.Provider,
                        _options.Model,
                        request.ContextDocuments.Count);
                    return new LanguageModelCompletion
                    {
                        Text = text,
                        Provider = _options.Provider,
                        Model = _options.Model,
                    };
                }

                var status = (int)response.StatusCode;
                if (!IsTransient(response.StatusCode) || attempt == maxAttempts - 1)
                {
                    _logger.LogWarning(
                        "Language model returned {Status}. Provider={Provider} Model={Model}",
                        status,
                        _options.Provider,
                        _options.Model);
                    throw new LanguageModelProviderException(
                        $"Language model provider returned {status}.",
                        status);
                }

                var delay = RetryDelay(response, attempt);
                _logger.LogWarning(
                    "Transient language model status {Status}. Retry {Attempt} in {Delay}. Provider={Provider}",
                    status,
                    attempt + 1,
                    delay,
                    _options.Provider);
                await Task.Delay(delay, timeout.Token);
            }
            catch (OperationCanceledException exception) when (!cancellationToken.IsCancellationRequested)
            {
                throw new LanguageModelTimeoutException("The language model provider timed out.", exception);
            }
            catch (LanguageModelProviderException)
            {
                throw;
            }
            catch (HttpRequestException exception) when (attempt < maxAttempts - 1)
            {
                last = exception;
                _logger.LogWarning(exception, "Transient language model HTTP error. Provider={Provider}", _options.Provider);
                await Task.Delay(RetryDelay(null, attempt), timeout.Token);
            }
        }

        throw new LanguageModelProviderException(
            Redact(last?.Message) ?? "The language model provider failed.",
            statusCode: null);
    }

    private HttpRequestMessage CreateRequest(LanguageModelRequest request)
    {
        var payload = new ChatRequest
        {
            Model = _options.Model,
            Temperature = _options.Temperature,
            MaxTokens = _options.MaxTokens > 0 ? _options.MaxTokens : null,
            Messages =
            [
                new ChatMessage { Role = "system", Content = request.SystemInstructions },
                new ChatMessage { Role = "user", Content = KnowledgeGroundedPrompt.FormatUserMessage(request) },
            ],
        };
        var httpRequest = new HttpRequestMessage(HttpMethod.Post, new Uri(_options.Endpoint, UriKind.Absolute))
        {
            Content = new StringContent(JsonSerializer.Serialize(payload, JsonOptions), Encoding.UTF8, "application/json"),
        };
        httpRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", (_options.ApiKey ?? string.Empty).Trim());
        return httpRequest;
    }

    private static async Task<string> ReadTextAsync(HttpResponseMessage response, CancellationToken cancellationToken)
    {
        await using var stream = await response.Content.ReadAsStreamAsync(cancellationToken);
        var parsed = await JsonSerializer.DeserializeAsync<ChatResponse>(stream, JsonOptions, cancellationToken)
            ?? throw new LanguageModelProviderException("The language model provider returned an empty body.");
        var text = parsed.Choices?.FirstOrDefault()?.Message?.Content;
        if (string.IsNullOrWhiteSpace(text))
        {
            throw new LanguageModelProviderException("The language model provider returned no message text.");
        }

        return text.Trim();
    }

    private void EnsureConfigured()
    {
        if (string.IsNullOrWhiteSpace(_options.ApiKey))
        {
            throw new LanguageModelConfigurationException("LanguageModel:ApiKey is not configured.");
        }

        if (string.IsNullOrWhiteSpace(_options.Endpoint)
            || !Uri.TryCreate(_options.Endpoint, UriKind.Absolute, out _))
        {
            throw new LanguageModelConfigurationException("LanguageModel:Endpoint is not a valid absolute URI.");
        }
    }

    private static bool IsTransient(HttpStatusCode status) =>
        status is HttpStatusCode.RequestTimeout
            or HttpStatusCode.TooManyRequests
            or HttpStatusCode.InternalServerError
            or HttpStatusCode.BadGateway
            or HttpStatusCode.ServiceUnavailable
            or HttpStatusCode.GatewayTimeout;

    private static TimeSpan RetryDelay(HttpResponseMessage? response, int attempt)
    {
        if (response?.Headers.RetryAfter?.Delta is TimeSpan delta && delta > TimeSpan.Zero)
        {
            return delta > TimeSpan.FromSeconds(30) ? TimeSpan.FromSeconds(30) : delta;
        }

        if (response?.Headers.RetryAfter?.Date is DateTimeOffset date)
        {
            var until = date - DateTimeOffset.UtcNow;
            if (until > TimeSpan.Zero)
            {
                return until > TimeSpan.FromSeconds(30) ? TimeSpan.FromSeconds(30) : until;
            }
        }

        return TimeSpan.FromMilliseconds(200 * Math.Pow(2, attempt));
    }

    private static string? Redact(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return value;
        }

        return value.Contains("Bearer ", StringComparison.OrdinalIgnoreCase)
            ? "[redacted]"
            : value;
    }

    private sealed class ChatRequest
    {
        public required string Model { get; init; }

        public double Temperature { get; init; }

        [JsonPropertyName("max_tokens")]
        public int? MaxTokens { get; init; }

        public required ChatMessage[] Messages { get; init; }
    }

    private sealed class ChatMessage
    {
        public required string Role { get; init; }

        public required string Content { get; init; }
    }

    private sealed class ChatResponse
    {
        public IReadOnlyList<ChatChoice>? Choices { get; init; }
    }

    private sealed class ChatChoice
    {
        public ChatMessage? Message { get; init; }
    }
}
