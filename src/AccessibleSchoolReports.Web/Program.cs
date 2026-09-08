using AccessibleSchoolReports.Application.Knowledge;
using AccessibleSchoolReports.Application.Reporting;
using AccessibleSchoolReports.Infrastructure.Embeddings;
using AccessibleSchoolReports.Infrastructure.Import;
using AccessibleSchoolReports.Infrastructure.Knowledge;
using AccessibleSchoolReports.Infrastructure.LanguageModels;
using AccessibleSchoolReports.Infrastructure.Persistence;
using AccessibleSchoolReports.Infrastructure.Security;
using AccessibleSchoolReports.Web.Api;
using AccessibleSchoolReports.Web.Downloads;
using AccessibleSchoolReports.Web.Security;
using Microsoft.AspNetCore.Http.Features;

var builder = WebApplication.CreateBuilder(args);

var connectionString = builder.Configuration.GetConnectionString("SchoolReports")
    ?? throw new InvalidOperationException("Connection string 'SchoolReports' is not configured.");
connectionString = SqliteConnectionString.ResolveWorkingDatabase(
    connectionString,
    builder.Environment.ContentRootPath);
builder.Services.AddSchoolReportsPersistence(
    connectionString,
    options =>
    {
        builder.Configuration.GetSection(ReportGenerationOptions.SectionName).Bind(options);
        if (string.IsNullOrWhiteSpace(options.OutputRoot))
        {
            options.OutputRoot = "output";
        }

        if (!Path.IsPathRooted(options.OutputRoot))
        {
            options.OutputRoot = Path.GetFullPath(
                options.OutputRoot,
                builder.Environment.ContentRootPath);
        }
    });

builder.Services.AddSchoolReportsEmbeddings(options =>
    builder.Configuration.GetSection(EmbeddingOptions.SectionName).Bind(options));
builder.Services.AddSchoolReportsLanguageModel(options =>
    builder.Configuration.GetSection(LanguageModelOptions.SectionName).Bind(options));
builder.Logging.AddFilter(
    "System.Net.Http.HttpClient.OpenAiCompatibleEmbeddingService",
    LogLevel.Warning);
builder.Logging.AddFilter(
    "System.Net.Http.HttpClient.OpenAiCompatibleLanguageModelService",
    LogLevel.Warning);

builder.Services.AddSchoolReportsIdentity();
builder.Services.AddAntiforgery();
builder.Services.Configure<FormOptions>(options =>
    options.MultipartBodyLengthLimit = 12 * 1024 * 1024);

var app = builder.Build();

await using (var scope = app.Services.CreateAsyncScope())
{
    var db = scope.ServiceProvider.GetRequiredService<SchoolReportsDbContext>();
    await db.MigrateAsync(app.Lifetime.ApplicationStopping);
    if (!app.Environment.IsEnvironment("Testing"))
    {
        var root = SqliteConnectionString.FindRepositoryRoot(app.Environment.ContentRootPath);
        await GraduateClassYearBackfill.ApplyFromSampleWorkbooksAsync(
            db,
            root,
            app.Lifetime.ApplicationStopping);
        await GraduateDuplicateCleanup.ApplyAsync(db, app.Lifetime.ApplicationStopping);
    }

    await IdentityRoleSeed.EnsureRolesAsync(scope.ServiceProvider, app.Lifetime.ApplicationStopping);
    if (app.Environment.IsDevelopment())
    {
        await IdentityDevelopmentSeed.TryApplyAsync(scope.ServiceProvider, app.Lifetime.ApplicationStopping);
    }
}

await KnowledgeStartup.PrepareAsync(
    app.Services,
    app.Environment.ContentRootPath,
    app.Environment.EnvironmentName,
    app.Lifetime.ApplicationStopping);

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler(errorApp => errorApp.Run(async context =>
    {
        context.Response.StatusCode = StatusCodes.Status500InternalServerError;
        await context.Response.WriteAsync("An error occurred.");
    }));
    app.UseHsts();
}

if (!app.Environment.IsEnvironment("Testing"))
{
    app.UseHttpsRedirection();
}

app.UseStaticFiles();
app.UseAuthentication();
app.UseAuthorization();
app.UseAntiforgery();

app.MapAccountApi();
app.MapIdentityAuth();
app.MapMeApi();
app.MapDashboardApi();
app.MapImportApi();
app.MapSchoolsApi();
app.MapReportsApi();
app.MapRunsApi();
app.MapAssistantApi();
app.MapReportDownloads();
app.MapSpaPages();

app.Run();

public partial class Program;
