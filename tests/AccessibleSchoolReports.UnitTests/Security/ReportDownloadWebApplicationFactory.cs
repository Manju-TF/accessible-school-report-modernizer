using AccessibleSchoolReports.Application.Security;
using AccessibleSchoolReports.Domain.Entities;
using AccessibleSchoolReports.Domain.Persistence;
using AccessibleSchoolReports.Domain.Security;
using AccessibleSchoolReports.Infrastructure.Persistence;
using AccessibleSchoolReports.Infrastructure.Security;
using AccessibleSchoolReports.Web;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace AccessibleSchoolReports.UnitTests.Security;

public sealed class ReportDownloadWebApplicationFactory : WebApplicationFactory<Program>
{
    public const string TestPassword = SecurityWebApplicationFactory.TestPassword;
    public const string AdminUserName = SecurityWebApplicationFactory.AdminUserName;
    public const string ReportUserName = SecurityWebApplicationFactory.ReportUserName;
    public const string ViewerUserName = SecurityWebApplicationFactory.ViewerUserName;
    public const string SchoolAName = "School A";
    public const string SchoolBName = "School B";

    public static readonly byte[] SchoolAPdfBytes = "%PDF-1.4\nA-authorized-report\n%%EOF\n"u8.ToArray();
    public static readonly byte[] SchoolBPdfBytes = "%PDF-1.4\nB-authorized-report\n%%EOF\n"u8.ToArray();

    private static readonly object HostGate = new();
    private readonly string _root = Path.Combine(
        Path.GetTempPath(),
        "asr-download-http",
        Guid.NewGuid().ToString("N"));

    public string OutputRoot { get; }
    public int SchoolAId { get; private set; }
    public int SchoolBId { get; private set; }
    public int ReportAId { get; private set; }
    public int ReportBId { get; private set; }
    public int MissingPdfReportId { get; private set; }
    public int DeletedPdfReportId { get; private set; }
    public int TraversalStoredReportId { get; private set; }

    public ReportDownloadWebApplicationFactory()
    {
        Directory.CreateDirectory(_root);
        OutputRoot = Path.Combine(_root, "output");
        Directory.CreateDirectory(OutputRoot);
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        var database = Path.Combine(_root, "schoolreports.db");
        builder.UseEnvironment("Testing");
        builder.UseSetting("ConnectionStrings:SchoolReports", $"Data Source={database}");
        builder.ConfigureAppConfiguration((_, config) =>
        {
            config.AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["ConnectionStrings:SchoolReports"] = $"Data Source={database}",
                ["ReportGeneration:OutputRoot"] = OutputRoot,
                ["Identity:SeedUserName"] = "",
                ["Identity:SeedPassword"] = "",
                ["Identity:SeedRole"] = "",
            });
        });
    }

    protected override IHost CreateHost(IHostBuilder builder)
    {
        lock (HostGate)
        {
            var host = base.CreateHost(builder);
            using var scope = host.Services.CreateScope();
            IdentityRoleSeed.EnsureRolesAsync(scope.ServiceProvider).GetAwaiter().GetResult();
            var users = scope.ServiceProvider.GetRequiredService<UserManager<IdentityUser>>();
            var admin = EnsureUser(users, AdminUserName, AppRoles.Admin);
            var reportUser = EnsureUser(users, ReportUserName, AppRoles.ReportUser);
            var viewer = EnsureUser(users, ViewerUserName, AppRoles.Viewer);
            SeedReports(scope.ServiceProvider, viewer.Id, reportUser.Id, admin.Id);
            return host;
        }
    }

    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);
        try
        {
            if (Directory.Exists(_root))
            {
                Directory.Delete(_root, recursive: true);
            }
        }
        catch (IOException)
        {
        }
    }

    private void SeedReports(IServiceProvider services, string viewerId, string reportUserId, string adminId)
    {
        var db = services.GetRequiredService<SchoolReportsDbContext>();
        var schoolA = new School { Code = "10701", Name = SchoolAName };
        var schoolB = new School { Code = "23306", Name = SchoolBName };
        db.Schools.AddRange(schoolA, schoolB);
        db.SaveChanges();
        SchoolAId = schoolA.Id;
        SchoolBId = schoolB.Id;

        var pdfA = WritePdf(Path.Combine(OutputRoot, "2025", "10701", "summary-report.pdf"), SchoolAPdfBytes);
        var pdfB = WritePdf(Path.Combine(OutputRoot, "2025", "23306", "summary-report.pdf"), SchoolBPdfBytes);
        var deletedPath = WritePdf(Path.Combine(OutputRoot, "2025", "10701", "deleted-report.pdf"), SchoolAPdfBytes);
        var missingPath = Path.Combine(OutputRoot, "2025", "10701", "missing-report.pdf");
        var traversalPath = Path.Combine(OutputRoot, "..", "secret.pdf");

        var run = new ReportRun
        {
            Mode = ReportGenerationMode.Single,
            Status = RunStatus.Completed,
            StartedUtc = DateTimeOffset.UtcNow,
            OutputDirectory = Path.Combine(OutputRoot, "2025"),
        };
        db.ReportRuns.Add(run);
        db.SaveChanges();

        var reportA = Item(run.Id, schoolA.Id, pdfA);
        var reportB = Item(run.Id, schoolB.Id, pdfB);
        var missing = Item(run.Id, schoolA.Id, missingPath);
        var deleted = Item(run.Id, schoolA.Id, deletedPath);
        var traversal = Item(run.Id, schoolA.Id, traversalPath);
        db.ReportRunItems.AddRange(reportA, reportB, missing, deleted, traversal);
        db.UserSchoolAccess.AddRange(
            new UserSchoolAccess
            {
                UserId = viewerId,
                SchoolId = schoolA.Id,
                AccessLevel = SchoolAccessLevel.View,
                CreatedAt = DateTimeOffset.UtcNow,
            },
            new UserSchoolAccess
            {
                UserId = reportUserId,
                SchoolId = schoolA.Id,
                AccessLevel = SchoolAccessLevel.Generate,
                CreatedAt = DateTimeOffset.UtcNow,
            });
        db.SaveChanges();

        File.Delete(deletedPath);

        ReportAId = reportA.Id;
        ReportBId = reportB.Id;
        MissingPdfReportId = missing.Id;
        DeletedPdfReportId = deleted.Id;
        TraversalStoredReportId = traversal.Id;
        _ = adminId;
    }

    private static ReportRunItem Item(int runId, int schoolId, string outputPath) =>
        new()
        {
            ReportRunId = runId,
            SchoolId = schoolId,
            Status = RunStatus.Completed,
            OutputPath = outputPath,
        };

    private static string WritePdf(string path, byte[] bytes)
    {
        Directory.CreateDirectory(Path.GetDirectoryName(path)!);
        File.WriteAllBytes(path, bytes);
        return path;
    }

    private static IdentityUser EnsureUser(UserManager<IdentityUser> users, string userName, string role)
    {
        var user = users.FindByNameAsync(userName).GetAwaiter().GetResult()
            ?? CreateUser(users, userName);

        if (!users.IsInRoleAsync(user, role).GetAwaiter().GetResult())
        {
            var assigned = users.AddToRoleAsync(user, role).GetAwaiter().GetResult();
            if (!assigned.Succeeded && !users.IsInRoleAsync(user, role).GetAwaiter().GetResult())
            {
                throw new InvalidOperationException($"Could not assign role '{role}'.");
            }
        }

        return user;
    }

    private static IdentityUser CreateUser(UserManager<IdentityUser> users, string userName)
    {
        var user = new IdentityUser { UserName = userName };
        var created = users.CreateAsync(user, TestPassword).GetAwaiter().GetResult();
        if (created.Succeeded)
        {
            return user;
        }

        var existing = users.FindByNameAsync(userName).GetAwaiter().GetResult();
        if (existing is not null)
        {
            return existing;
        }

        throw new InvalidOperationException($"Could not create test user '{userName}'.");
    }
}
