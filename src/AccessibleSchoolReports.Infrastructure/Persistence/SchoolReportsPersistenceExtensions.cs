using AccessibleSchoolReports.Application.Imports;
using AccessibleSchoolReports.Application.Reporting;
using AccessibleSchoolReports.Infrastructure.Import;
using AccessibleSchoolReports.Infrastructure.Pdf;
using AccessibleSchoolReports.Infrastructure.Reporting;
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
        var generation = services.AddOptions<ReportGenerationOptions>();
        if (configureGeneration is not null)
        {
            generation.Configure(configureGeneration);
        }

        services.AddScoped<IReportGenerationService, ReportGenerationService>();

        return services;
    }
}
