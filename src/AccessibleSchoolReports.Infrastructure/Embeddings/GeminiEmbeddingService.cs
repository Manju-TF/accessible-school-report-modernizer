using System.Net;
using System.Security.Claims;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using AccessibleSchoolReports.Application.Knowledge;
using AccessibleSchoolReports.Application.Security;
using AccessibleSchoolReports.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace AccessibleSchoolReports.Infrastructure.Embeddings;

public sealed class GeminiEmbeddingService : IEmbeddingService
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
    };

    private readonly HttpClient _http;
    private readonly EmbeddingOptions _options;
    private readonly IDbContextFactory<SchoolReportsDbContext> _dbFactory;
    private readonly IReportAuthorizationService _authorization;
    private readonly ILogger<GeminiEmbeddingService> _logger;

    public GeminiEmbeddingService(
        HttpClient http,
        IOptions<EmbeddingOptions> options,
        IDbContextFactory<SchoolReportsDbContext> dbFactory,
        IReportAuthorizationService authorization,
        ILogger<GeminiEmbeddingService> logger)
    {
        _http = http;
        _options = options.Value;
        _dbFactory = dbFactory;
        _authorization = authorization;
        _logger = logger;
    }

    public EmbeddingModelInfo Model => new()
    {
        Provider = "Gemini",
        Model = _options.Model,
        Dimensions = _options.Dimensions,
    };

    public async Task<EmbeddingBatchResult> EmbedPermittedChunksAsync(
        ClaimsPrincipal user,
        IReadOnlyList<int> chunkIds,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(user);
        ArgumentNullException.ThrowIfNull(chunkIds);

        await using var db = await _dbFactory.CreateDbContextAsync(cancellationToken);
        var uniqueIds = chunkIds.Distinct().ToArray();
        var chunks = await db.KnowledgeChunks
            .Include(chunk => chunk.KnowledgeDocument)
            .Where(chunk => uniqueIds.Contains(chunk.Id))
            .ToListAsync(cancellationToken);
        var schools = await _authorization.GetAccessibleSchoolIdsAsync(user, cancellationToken);
        var permitted = EmbeddingAccess.FilterPermitted(chunks, user, schools);
        var skipped = chunks
            .Select(chunk => chunk.Id)
            .Except(permitted.Select(chunk => chunk.Id))
            .Concat(uniqueIds.Except(chunks.Select(chunk => chunk.Id)))
            .Distinct()
            .ToArray();

        if (permitted.Count == 0)
        {
            return new EmbeddingBatchResult
            {
                Provider = "Gemini",
                Model = Model.Key,
                Dimensions = _options.Dimensions,
                Embedded = [],
                SkippedUnauthorizedChunkIds = skipped,
            };
        }

        var embedded = new List<EmbeddedChunk>();
        foreach (var batch in permitted.Chunk(Math.Clamp(_options.MaxBatchSize, 1, 100)))
        {
            var vectors = await EmbedTextsAsync(
                batch.Select(chunk => chunk.Content).ToArray(),
                "RETRIEVAL_DOCUMENT",
                cancellationToken);
            for (var index = 0; index < batch.Length; index++)
            {
                var chunk = batch[index];
                var vector = vectors[index];
                chunk.Embedding = EmbeddingVectorConvert.ToBytes(vector);
                chunk.EmbeddingModel = Model.Key;
                embedded.Add(new EmbeddedChunk { ChunkId = chunk.Id, Values = vector });
            }
        }

        await db.SaveChangesAsync(cancellationToken);
        return new EmbeddingBatchResult
        {
            Provider = "Gemini",
            Model = Model.Key,
            Dimensions = _options.Dimensions,
            Embedded = embedded,
            SkippedUnauthorizedChunkIds = skipped,
        };
    }

    public async Task<EmbeddingVector> EmbedQueryAsync(
        string text,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(text);
        var vectors = await EmbedTextsAsync([text], "RETRIEVAL_QUERY", cancellationToken);
        return new EmbeddingVector
        {
            Values = vectors[0],
            Provider = "Gemini",
            Model = _options.Model,
            Dimensions = _options.Dimensions,
        };
    }

    private async Task<IReadOnlyList<float[]>> EmbedTextsAsync(
        IReadOnlyList<string> texts,
        string taskType,
        CancellationToken cancellationToken)
    {
        EnsureConfigured();
        var maxAttempts = Math.Max(0, _options.MaxRetries) + 1;
        using var timeout = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        timeout.CancelAfter(TimeSpan.FromSeconds(Math.Clamp(_options.TimeoutSeconds, 1, 120)));

        Exception? last = null;
        for (var attempt = 0; attempt < maxAttempts; attempt++)
        {
            timeout.Token.ThrowIfCancellationRequested();
            try
            {
                using var request = CreateRequest(texts, taskType);
                using var response = await _http.SendAsync(request, timeout.Token);
                if (response.IsSuccessStatusCode)
                {
                    return await ReadVectorsAsync(response, texts.Count, timeout.Token);
                }

                var status = (int)response.StatusCode;
                if (!IsTransient(response.StatusCode) || attempt == maxAttempts - 1)
                {
                    _logger.LogWarning(
                        "Gemini embedding provider returned {Status} for {Count} text(s). Model={Model}",
                        status,
                        texts.Count,
                        _options.Model);
                    throw new EmbeddingProviderException(
                        $"Gemini embedding provider returned {status}.",
                        status);
                }

                var delay = RetryDelay(response, attempt);
                _logger.LogWarning(
                    "Transient Gemini embedding status {Status}. Retry {Attempt} in {Delay}.",
                    status,
                    attempt + 1,
                    delay);
                await Task.Delay(delay, timeout.Token);
            }
            catch (OperationCanceledException exception) when (!cancellationToken.IsCancellationRequested)
            {
                throw new EmbeddingTimeoutException("The Gemini embedding provider timed out.", exception);
            }
            catch (EmbeddingProviderException)
            {
                throw;
            }
            catch (HttpRequestException exception) when (attempt < maxAttempts - 1)
            {
                last = exception;
                _logger.LogWarning(exception, "Transient Gemini embedding HTTP error.");
                await Task.Delay(RetryDelay(null, attempt), timeout.Token);
            }
        }

        throw new EmbeddingProviderException(
            Redact(last?.Message) ?? "The Gemini embedding provider failed.",
            statusCode: null);
    }

    private HttpRequestMessage CreateRequest(IReadOnlyList<string> texts, string taskType)
    {
        var model = NormalizeModel(_options.Model);
        var payload = new BatchEmbedRequest
        {
            Requests = texts.Select(text => new EmbedRequest
            {
                Model = $"models/{model}",
                Content = new EmbedContent
                {
                    Parts = [new EmbedPart { Text = text }],
                },
                TaskType = taskType,
                OutputDimensionality = _options.Dimensions,
            }).ToArray(),
        };
        var endpoint = $"{_options.Endpoint.TrimEnd('/')}/models/{Uri.EscapeDataString(model)}:batchEmbedContents";
        var request = new HttpRequestMessage(HttpMethod.Post, new Uri(endpoint, UriKind.Absolute))
        {
            Content = new StringContent(
                JsonSerializer.Serialize(payload, JsonOptions),
                Encoding.UTF8,
                "application/json"),
        };
        request.Headers.TryAddWithoutValidation("x-goog-api-key", _options.ApiKey.Trim());
        return request;
    }

    private async Task<IReadOnlyList<float[]>> ReadVectorsAsync(
        HttpResponseMessage response,
        int expectedCount,
        CancellationToken cancellationToken)
    {
        await using var stream = await response.Content.ReadAsStreamAsync(cancellationToken);
        var parsed = await JsonSerializer.DeserializeAsync<BatchEmbedResponse>(
            stream,
            JsonOptions,
            cancellationToken)
            ?? throw new EmbeddingProviderException("The Gemini embedding provider returned an empty body.");
        var vectors = (parsed.Embeddings ?? [])
            .Select(item => item.Values ?? [])
            .ToArray();
        if (vectors.Length != expectedCount)
        {
            throw new EmbeddingProviderException(
                $"The Gemini embedding provider returned {vectors.Length} vector(s); expected {expectedCount}.");
        }

        foreach (var vector in vectors)
        {
            if (vector.Length != _options.Dimensions)
            {
                throw new EmbeddingDimensionException(
                    $"Expected {_options.Dimensions} dimensions; received {vector.Length}.");
            }
        }

        return vectors;
    }

    private void EnsureConfigured()
    {
        if (string.IsNullOrWhiteSpace(_options.ApiKey))
        {
            throw new EmbeddingConfigurationException("Embeddings:ApiKey is not configured.");
        }

        if (string.IsNullOrWhiteSpace(_options.Endpoint)
            || !Uri.TryCreate(_options.Endpoint, UriKind.Absolute, out var endpoint)
            || endpoint.Scheme != Uri.UriSchemeHttps)
        {
            throw new EmbeddingConfigurationException("Embeddings:Endpoint must be a valid HTTPS URI.");
        }

        if (string.IsNullOrWhiteSpace(_options.Model)
            || _options.Model.Contains('/')
            || _options.Model.Contains(':'))
        {
            throw new EmbeddingConfigurationException("Embeddings:Model must be a Gemini model name.");
        }

        if (_options.Dimensions <= 0)
        {
            throw new EmbeddingConfigurationException("Embeddings:Dimensions must be a positive integer.");
        }
    }

    private static string NormalizeModel(string model) =>
        model.StartsWith("models/", StringComparison.Ordinal) ? model["models/".Length..] : model;

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

    private static string? Redact(string? value) =>
        string.IsNullOrWhiteSpace(value)
            ? value
            : value.Contains("x-goog-api-key", StringComparison.OrdinalIgnoreCase)
                ? "[redacted]"
                : value;

    private sealed class BatchEmbedRequest
    {
        public required EmbedRequest[] Requests { get; init; }
    }

    private sealed class EmbedRequest
    {
        public required string Model { get; init; }

        public required EmbedContent Content { get; init; }

        public required string TaskType { get; init; }

        public int OutputDimensionality { get; init; }
    }

    private sealed class EmbedContent
    {
        public required EmbedPart[] Parts { get; init; }
    }

    private sealed class EmbedPart
    {
        public required string Text { get; init; }
    }

    private sealed class BatchEmbedResponse
    {
        public IReadOnlyList<EmbedResponse>? Embeddings { get; init; }
    }

    private sealed class EmbedResponse
    {
        public float[]? Values { get; init; }
    }
}
