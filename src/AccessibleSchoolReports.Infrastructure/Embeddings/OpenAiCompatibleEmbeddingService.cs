using System.Net;
using System.Net.Http.Headers;
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

public sealed class OpenAiCompatibleEmbeddingService : IEmbeddingService
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
    private readonly ILogger<OpenAiCompatibleEmbeddingService> _logger;

    public OpenAiCompatibleEmbeddingService(
        HttpClient http,
        IOptions<EmbeddingOptions> options,
        IDbContextFactory<SchoolReportsDbContext> dbFactory,
        IReportAuthorizationService authorization,
        ILogger<OpenAiCompatibleEmbeddingService> logger)
    {
        _http = http;
        _options = options.Value;
        _dbFactory = dbFactory;
        _authorization = authorization;
        _logger = logger;
    }

    public EmbeddingModelInfo Model => new()
    {
        Provider = _options.Provider,
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
            _logger.LogInformation(
                "Embedding request sent no chunk text. Provider={Provider} Model={Model} Requested={Requested} Skipped={Skipped}",
                _options.Provider,
                _options.Model,
                uniqueIds.Length,
                skipped.Length);
            return new EmbeddingBatchResult
            {
                Provider = _options.Provider,
                Model = _options.Model,
                Dimensions = _options.Dimensions,
                Embedded = [],
                SkippedUnauthorizedChunkIds = skipped,
            };
        }

        var embedded = new List<EmbeddedChunk>();
        string? responseModel = null;
        foreach (var batch in permitted.Chunk(Math.Max(1, _options.MaxBatchSize)))
        {
            var vectors = await EmbedTextsAsync(batch.Select(chunk => chunk.Content).ToArray(), cancellationToken);
            responseModel = vectors.Model;
            for (var index = 0; index < batch.Length; index++)
            {
                var chunk = batch[index];
                var vector = vectors.Vectors[index];
                chunk.Embedding = EmbeddingVectorConvert.ToBytes(vector);
                chunk.EmbeddingModel = Model.Key;
                embedded.Add(new EmbeddedChunk { ChunkId = chunk.Id, Values = vector });
            }
        }

        await db.SaveChangesAsync(cancellationToken);
        return new EmbeddingBatchResult
        {
            Provider = _options.Provider,
            Model = $"{_options.Provider}/{responseModel ?? _options.Model}",
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
        var batch = await EmbedTextsAsync([text], cancellationToken);
        return new EmbeddingVector
        {
            Values = batch.Vectors[0],
            Provider = batch.Provider,
            Model = batch.Model,
            Dimensions = batch.Dimensions,
        };
    }

    private async Task<ProviderBatch> EmbedTextsAsync(
        IReadOnlyList<string> texts,
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
                using var request = CreateRequest(texts);
                using var response = await _http.SendAsync(request, timeout.Token);
                if (response.IsSuccessStatusCode)
                {
                    var parsed = await ReadVectorsAsync(response, texts.Count, timeout.Token);
                    return new ProviderBatch(
                        _options.Provider,
                        parsed.Model,
                        _options.Dimensions,
                        parsed.Vectors);
                }

                var status = (int)response.StatusCode;
                if (!IsTransient(response.StatusCode) || attempt == maxAttempts - 1)
                {
                    _logger.LogWarning(
                        "Embedding provider returned {Status} for {Count} text(s). Provider={Provider} Model={Model}",
                        status,
                        texts.Count,
                        _options.Provider,
                        _options.Model);
                    throw new EmbeddingProviderException(
                        $"Embedding provider returned {status}.",
                        status);
                }

                var delay = RetryDelay(response, attempt);
                _logger.LogWarning(
                    "Transient embedding provider status {Status}. Retry {Attempt} in {Delay}. Provider={Provider}",
                    status,
                    attempt + 1,
                    delay,
                    _options.Provider);
                await Task.Delay(delay, timeout.Token);
            }
            catch (OperationCanceledException exception) when (!cancellationToken.IsCancellationRequested)
            {
                throw new EmbeddingTimeoutException("The embedding provider timed out.", exception);
            }
            catch (EmbeddingProviderException)
            {
                throw;
            }
            catch (HttpRequestException exception) when (attempt < maxAttempts - 1)
            {
                last = exception;
                _logger.LogWarning(exception, "Transient embedding HTTP error. Provider={Provider}", _options.Provider);
                await Task.Delay(RetryDelay(null, attempt), timeout.Token);
            }
        }

        throw new EmbeddingProviderException(
            Redact(last?.Message) ?? "The embedding provider failed.",
            statusCode: null);
    }

    private HttpRequestMessage CreateRequest(IReadOnlyList<string> texts)
    {
        var payload = new EmbeddingApiRequest
        {
            Model = _options.Model,
            Input = texts.ToArray(),
            Dimensions = _options.Dimensions > 0 ? _options.Dimensions : null,
        };
        var request = new HttpRequestMessage(HttpMethod.Post, ResolveEndpoint())
        {
            Content = new StringContent(JsonSerializer.Serialize(payload, JsonOptions), Encoding.UTF8, "application/json"),
        };
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", (_options.ApiKey ?? string.Empty).Trim());
        return request;
    }

    private async Task<(string Model, IReadOnlyList<float[]> Vectors)> ReadVectorsAsync(
        HttpResponseMessage response,
        int expectedCount,
        CancellationToken cancellationToken)
    {
        await using var stream = await response.Content.ReadAsStreamAsync(cancellationToken);
        var parsed = await JsonSerializer.DeserializeAsync<EmbeddingApiResponse>(stream, JsonOptions, cancellationToken)
            ?? throw new EmbeddingProviderException("The embedding provider returned an empty body.");

        var ordered = (parsed.Data ?? [])
            .OrderBy(item => item.Index)
            .Select(item => item.Embedding ?? [])
            .ToArray();
        if (ordered.Length != expectedCount)
        {
            throw new EmbeddingProviderException(
                $"The embedding provider returned {ordered.Length} vector(s); expected {expectedCount}.");
        }

        foreach (var vector in ordered)
        {
            if (vector.Length != _options.Dimensions)
            {
                throw new EmbeddingDimensionException(
                    $"Expected {_options.Dimensions} dimensions; received {vector.Length}.");
            }
        }

        var model = string.IsNullOrWhiteSpace(parsed.Model) ? _options.Model : parsed.Model;
        return (model, ordered);
    }

    private sealed record ProviderBatch(
        string Provider,
        string Model,
        int Dimensions,
        IReadOnlyList<float[]> Vectors);

    private void EnsureConfigured()
    {
        if (string.IsNullOrWhiteSpace(_options.ApiKey))
        {
            throw new EmbeddingConfigurationException("Embeddings:ApiKey is not configured.");
        }

        if (string.IsNullOrWhiteSpace(_options.Endpoint)
            || !Uri.TryCreate(_options.Endpoint, UriKind.Absolute, out _))
        {
            throw new EmbeddingConfigurationException("Embeddings:Endpoint is not a valid absolute URI.");
        }

        if (_options.Dimensions <= 0)
        {
            throw new EmbeddingConfigurationException("Embeddings:Dimensions must be a positive integer.");
        }
    }

    private Uri ResolveEndpoint() => new(_options.Endpoint, UriKind.Absolute);

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

    private sealed class EmbeddingApiRequest
    {
        public required string Model { get; init; }

        public required string[] Input { get; init; }

        public int? Dimensions { get; init; }
    }

    private sealed class EmbeddingApiResponse
    {
        public string? Model { get; init; }

        public IReadOnlyList<EmbeddingApiItem>? Data { get; init; }
    }

    private sealed class EmbeddingApiItem
    {
        public int Index { get; init; }

        public float[]? Embedding { get; init; }
    }
}
