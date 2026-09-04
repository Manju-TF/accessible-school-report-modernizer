using System.Security.Claims;
using AccessibleSchoolReports.Application.Knowledge;
using AccessibleSchoolReports.Application.Security;
using AccessibleSchoolReports.Infrastructure.Persistence;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace AccessibleSchoolReports.Infrastructure.Knowledge;

public static class KnowledgeStartup
{
    public static async Task<KnowledgePrepareResult> PrepareAsync(
        IServiceProvider services,
        string contentRoot,
        string environmentName,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentException.ThrowIfNullOrWhiteSpace(contentRoot);
        if (string.Equals(environmentName, "Testing", StringComparison.OrdinalIgnoreCase))
        {
            return new KnowledgePrepareResult(0, 0, 0, Error: null);
        }

        ILogger? logger = null;
        try
        {
            logger = services.GetService<ILoggerFactory>()?.CreateLogger("KnowledgeStartup");
            await using var scope = services.CreateAsyncScope();
            var root = SqliteConnectionString.FindRepositoryRoot(contentRoot);
            var ingestion = scope.ServiceProvider.GetRequiredService<IKnowledgeIngestionService>();
            var ingested = await ingestion.IngestLegacyAndProjectDocumentsAsync(root, cancellationToken);
            logger?.LogInformation(
                "Knowledge ingest finished. Indexed={Indexed} Reindexed={Reindexed} Skipped={Skipped} Missing={Missing}",
                ingested.Indexed.Count,
                ingested.Reindexed.Count,
                ingested.SkippedUnchanged.Count,
                ingested.Missing.Count);

            var index = scope.ServiceProvider.GetRequiredService<IKnowledgeEmbeddingIndexService>();
            var result = await index.IndexPendingEmbeddingsAsync(StartupAdmin(), cancellationToken);
            logger?.LogInformation(
                "Knowledge embedding index finished. ChunksIndexed={Indexed} Skipped={Skipped} Failures={Failures}",
                result.ChunksIndexed,
                result.ChunksSkipped,
                result.Failures);
            return new KnowledgePrepareResult(
                ingested.Indexed.Count + ingested.Reindexed.Count + ingested.SkippedUnchanged.Count,
                result.ChunksIndexed,
                result.Failures,
                Error: null);
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception exception)
        {
            logger?.LogWarning(
                exception,
                "Knowledge prepare failed. The assistant may show insufficient evidence until ingest and index succeed.");
            return new KnowledgePrepareResult(0, 0, 0, exception.Message);
        }
    }

    public sealed record KnowledgePrepareResult(int DocumentsTouched, int ChunksIndexed, int Failures, string? Error);

    private static ClaimsPrincipal StartupAdmin() =>
        new(new ClaimsIdentity(
            [
                new Claim(ClaimTypes.NameIdentifier, "startup"),
                new Claim(ClaimTypes.Name, "startup"),
                new Claim(ClaimTypes.Role, AppRoles.Admin),
            ],
            "startup"));
}
