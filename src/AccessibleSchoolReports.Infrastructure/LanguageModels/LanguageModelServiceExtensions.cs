using AccessibleSchoolReports.Application.Knowledge;
using AccessibleSchoolReports.Infrastructure.Knowledge;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace AccessibleSchoolReports.Infrastructure.LanguageModels;

public static class LanguageModelServiceExtensions
{
    public static IServiceCollection AddSchoolReportsLanguageModel(
        this IServiceCollection services,
        Action<LanguageModelOptions>? configure = null)
    {
        var options = services.AddOptions<LanguageModelOptions>();
        if (configure is not null)
        {
            options.Configure(configure);
        }

        services.PostConfigure<LanguageModelOptions>(value =>
            value.ApiKey = (value.ApiKey ?? string.Empty).Trim());

        services.AddHttpClient(nameof(OpenAiCompatibleLanguageModelService), (provider, client) =>
        {
            var value = provider.GetRequiredService<IOptions<LanguageModelOptions>>().Value;
            client.Timeout = TimeSpan.FromSeconds(Math.Clamp(value.TimeoutSeconds, 1, 180));
            client.DefaultRequestHeaders.Accept.ParseAdd("application/json");
        });

        services.AddScoped<ILanguageModelService>(provider =>
        {
            var client = provider.GetRequiredService<IHttpClientFactory>()
                .CreateClient(nameof(OpenAiCompatibleLanguageModelService));
            return new OpenAiCompatibleLanguageModelService(
                client,
                provider.GetRequiredService<IOptions<LanguageModelOptions>>(),
                provider.GetRequiredService<ILogger<OpenAiCompatibleLanguageModelService>>());
        });
        services.AddScoped<IKnowledgeAssistantService, KnowledgeAssistantService>();

        return services;
    }
}
