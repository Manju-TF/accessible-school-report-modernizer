using AccessibleSchoolReports.Application.Knowledge;
using AccessibleSchoolReports.Application.Security;
using AccessibleSchoolReports.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace AccessibleSchoolReports.Infrastructure.Embeddings;

public static class EmbeddingServiceExtensions
{
    public static IServiceCollection AddSchoolReportsEmbeddings(
        this IServiceCollection services,
        Action<EmbeddingOptions>? configure = null)
    {
        var options = services.AddOptions<EmbeddingOptions>();
        if (configure is not null)
        {
            options.Configure(configure);
        }

        services.PostConfigure<EmbeddingOptions>(value =>
        {
            value.ApiKey = (value.ApiKey ?? string.Empty).Trim();
            if (value.UsesLocalLexical)
            {
                if (string.IsNullOrWhiteSpace(value.Model))
                {
                    value.Model = "hashed-bow";
                }

                if (value.Dimensions <= 0)
                {
                    value.Dimensions = HashedLexicalVector.DefaultDimensions;
                }
            }
        });

        services.AddHttpClient(nameof(OpenAiCompatibleEmbeddingService), (provider, client) =>
        {
            var value = provider.GetRequiredService<IOptions<EmbeddingOptions>>().Value;
            client.Timeout = TimeSpan.FromSeconds(Math.Clamp(value.TimeoutSeconds, 1, 120));
            client.DefaultRequestHeaders.Accept.ParseAdd("application/json");
        });

        services.AddScoped<IEmbeddingService>(provider =>
        {
            var configured = provider.GetRequiredService<IOptions<EmbeddingOptions>>();
            if (configured.Value.UsesLocalLexical)
            {
                return new LexicalEmbeddingService(
                    provider.GetRequiredService<IDbContextFactory<SchoolReportsDbContext>>(),
                    provider.GetRequiredService<IReportAuthorizationService>(),
                    configured);
            }

            var client = provider.GetRequiredService<IHttpClientFactory>()
                .CreateClient(nameof(OpenAiCompatibleEmbeddingService));
            return new OpenAiCompatibleEmbeddingService(
                client,
                configured,
                provider.GetRequiredService<IDbContextFactory<SchoolReportsDbContext>>(),
                provider.GetRequiredService<IReportAuthorizationService>(),
                provider.GetRequiredService<ILogger<OpenAiCompatibleEmbeddingService>>());
        });

        return services;
    }
}
