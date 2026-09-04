using AccessibleSchoolReports.Application.Reporting;
using AccessibleSchoolReports.Infrastructure.Persistence;
using AccessibleSchoolReports.Web.Components;
using AccessibleSchoolReports.Web.Downloads;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.SignalR;

var builder = WebApplication.CreateBuilder(args);

var connectionString = builder.Configuration.GetConnectionString("SchoolReports")
    ?? throw new InvalidOperationException("Connection string 'SchoolReports' is not configured.");
connectionString = SqliteConnectionString.Resolve(connectionString, builder.Environment.ContentRootPath);
builder.Services.AddSchoolReportsPersistence(
    connectionString,
    options =>
    {
        builder.Configuration.GetSection(ReportGenerationOptions.SectionName).Bind(options);
        options.OutputRoot = Path.Combine(builder.Environment.ContentRootPath, "output");
    });

builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();
builder.Services.Configure<HubOptions>(options =>
    options.MaximumReceiveMessageSize = 12 * 1024 * 1024);
builder.Services.Configure<FormOptions>(options =>
    options.MultipartBodyLengthLimit = 12 * 1024 * 1024);

var app = builder.Build();

await using (var scope = app.Services.CreateAsyncScope())
{
    var db = scope.ServiceProvider.GetRequiredService<SchoolReportsDbContext>();
    await db.MigrateAsync(app.Lifetime.ApplicationStopping);
}

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseStaticFiles();
app.UseAntiforgery();

app.MapReportDownloads();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();
