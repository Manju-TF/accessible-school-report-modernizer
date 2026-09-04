using AccessibleSchoolReports.Application.Imports;
using AccessibleSchoolReports.Application.Knowledge;
using AccessibleSchoolReports.Application.Reporting;
using AccessibleSchoolReports.Application.Security;
using AccessibleSchoolReports.Infrastructure.Import;
using AccessibleSchoolReports.Infrastructure.Knowledge;
using AccessibleSchoolReports.Infrastructure.Pdf;
using AccessibleSchoolReports.Infrastructure.Reporting;
using AccessibleSchoolReports.Infrastructure.Security;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace AccessibleSchoolReports.Infrastructure.Persistence;

public static class SchoolReportsPersistenceExtensions
{
    public static IServiceCollection AddSchoolReportsPersistence(
        this IServiceCollection services,
        string connectionString,
        Action<ReportGenerationOptions>? configureGeneration = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(connectionString);

        services.AddSingleton(new WorkingDatabase(SqliteConnectionString.GetDataSource(connectionString)));
        services.AddDbContext<SchoolReportsDbContext>(options =>
            options.UseSqlite(connectionString));
        services.AddDbContextFactory<SchoolReportsDbContext>(
            options => options.UseSqlite(connectionString),
            lifetime: ServiceLifetime.Scoped);
        services.AddScoped<IGraduateImportService, ExcelGraduateImportService>();
        services.AddSingleton<ISchoolReportCalculator, SchoolReportCalculator>();
        services.AddSingleton<IAccessiblePdfGenerator, QuestPdfAccessiblePdfGenerator>();
        services.AddSingleton<IPdfTextExtractor, PdfPigTextExtractor>();
        var generation = services.AddOptions<ReportGenerationOptions>();
        if (configureGeneration is not null)
        {
            generation.Configure(configureGeneration);
        }

        services.AddScoped<IReportAuthorizationService, ReportAuthorizationService>();
        services.AddScoped<IReportDownloadService, ReportDownloadService>();
        services.AddScoped<IKnowledgeIngestionService, KnowledgeIngestionService>();
        services.AddScoped<IPdfKnowledgeIngestionService, PdfKnowledgeIngestionService>();
        services.AddScoped<IKnowledgeEmbeddingIndexService, KnowledgeEmbeddingIndexService>();
        services.AddScoped<IKnowledgeRetrievalService, KnowledgeRetrievalService>();
        services.AddScoped<IKnowledgeAssistantSession, KnowledgeAssistantSession>();
        services.AddScoped<IReportGenerationService, ReportGenerationService>();

        return services;
    }
}
