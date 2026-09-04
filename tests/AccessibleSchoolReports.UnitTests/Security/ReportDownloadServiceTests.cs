using System.Security.Claims;
using AccessibleSchoolReports.Application.Reporting;
using AccessibleSchoolReports.Application.Security;
using AccessibleSchoolReports.Domain.Entities;
using AccessibleSchoolReports.Domain.Persistence;
using AccessibleSchoolReports.Domain.Security;
using AccessibleSchoolReports.Infrastructure.Persistence;
using AccessibleSchoolReports.Infrastructure.Reporting;
using AccessibleSchoolReports.Infrastructure.Security;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace AccessibleSchoolReports.UnitTests.Security;

public sealed class ReportDownloadServiceTests
{
    [Fact]
    public async Task AuthorizedUser_ReceivesPdfBytes()
    {
        await using var fixture = await Fixture.CreateAsync();
        var user = Principal("user-a", AppRoles.ReportUser);

        var result = await fixture.Downloads.TryDownloadAsync(user, fixture.ReportA.Id);

        Assert.True(result.Succeeded);
        Assert.NotNull(result.Content);
        await using (result.Content)
        {
            using var buffer = new MemoryStream();
            await result.Content.CopyToAsync(buffer);
            Assert.Equal(fixture.PdfABytes, buffer.ToArray());
        }

        Assert.Equal("10701-summary-report.pdf", result.FileName);
    }

    [Fact]
    public async Task UnauthorizedUser_IsDenied_WithoutOpeningOtherSchoolPdf()
    {
        await using var fixture = await Fixture.CreateAsync();
        var user = Principal("user-a", AppRoles.ReportUser);

        var result = await fixture.Downloads.TryDownloadAsync(user, fixture.ReportB.Id);

        Assert.False(result.Succeeded);
        Assert.Null(result.Content);
    }

    [Fact]
    public async Task InvalidReportId_IsDenied()
    {
        await using var fixture = await Fixture.CreateAsync();
        var admin = Principal("admin", AppRoles.Admin);

        var result = await fixture.Downloads.TryDownloadAsync(admin, 99_999);

        Assert.False(result.Succeeded);
        Assert.Null(result.Content);
    }

    [Fact]
    public async Task StoredTraversalPath_IsDenied_EvenForAdmin()
    {
        await using var fixture = await Fixture.CreateAsync();
        var admin = Principal("admin", AppRoles.Admin);

        var result = await fixture.Downloads.TryDownloadAsync(admin, fixture.TraversalReport.Id);

        Assert.False(result.Succeeded);
        Assert.Null(result.Content);
    }

    [Fact]
    public async Task MissingPdf_IsDenied()
    {
        await using var fixture = await Fixture.CreateAsync();
        var admin = Principal("admin", AppRoles.Admin);

        var result = await fixture.Downloads.TryDownloadAsync(admin, fixture.MissingReport.Id);

        Assert.False(result.Succeeded);
        Assert.Null(result.Content);
    }

    [Fact]
    public async Task DeletedPdf_IsDenied()
    {
        await using var fixture = await Fixture.CreateAsync();
        var admin = Principal("admin", AppRoles.Admin);

        var result = await fixture.Downloads.TryDownloadAsync(admin, fixture.DeletedReport.Id);

        Assert.False(result.Succeeded);
        Assert.Null(result.Content);
    }

    [Fact]
    public async Task Admin_CanDownloadBothSchools()
    {
        await using var fixture = await Fixture.CreateAsync();
        var admin = Principal("admin", AppRoles.Admin);

        var schoolA = await fixture.Downloads.TryDownloadAsync(admin, fixture.ReportA.Id);
        var schoolB = await fixture.Downloads.TryDownloadAsync(admin, fixture.ReportB.Id);

        Assert.True(schoolA.Succeeded);
        Assert.True(schoolB.Succeeded);
        schoolA.Content?.Dispose();
        schoolB.Content?.Dispose();
    }

    [Fact]
    public async Task Viewer_CanDownloadAssignedSchool_Only()
    {
        await using var fixture = await Fixture.CreateAsync();
        var viewer = Principal("user-b", AppRoles.Viewer);

        var assigned = await fixture.Downloads.TryDownloadAsync(viewer, fixture.ReportB.Id);
        var other = await fixture.Downloads.TryDownloadAsync(viewer, fixture.ReportA.Id);

        Assert.True(assigned.Succeeded);
        Assert.False(other.Succeeded);
        assigned.Content?.Dispose();
    }

    [Fact]
    public async Task ReportUser_CanDownloadAssignedSchool_Only()
    {
        await using var fixture = await Fixture.CreateAsync();
        var reportUser = Principal("user-a", AppRoles.ReportUser);

        var assigned = await fixture.Downloads.TryDownloadAsync(reportUser, fixture.ReportA.Id);
        var other = await fixture.Downloads.TryDownloadAsync(reportUser, fixture.ReportB.Id);

        Assert.True(assigned.Succeeded);
        Assert.False(other.Succeeded);
        assigned.Content?.Dispose();
    }

    [Fact]
    public async Task UnauthenticatedUser_IsDenied()
    {
        await using var fixture = await Fixture.CreateAsync();
        var anonymous = new ClaimsPrincipal(new ClaimsIdentity());

        var result = await fixture.Downloads.TryDownloadAsync(anonymous, fixture.ReportA.Id);

        Assert.False(result.Succeeded);
        Assert.Null(result.Content);
    }

    private static ClaimsPrincipal Principal(string userId, string role) =>
        new(new ClaimsIdentity(
            [
                new Claim(ClaimTypes.NameIdentifier, userId),
                new Claim(ClaimTypes.Name, userId),
                new Claim(ClaimTypes.Role, role),
            ],
            "test"));

    private sealed class Fixture : IAsyncDisposable
    {
        private readonly string _directory;
        private readonly SchoolReportsDbContext _db;

        public ReportDownloadService Downloads { get; }
        public ReportRunItem ReportA { get; }
        public ReportRunItem ReportB { get; }
        public ReportRunItem MissingReport { get; }
        public ReportRunItem DeletedReport { get; }
        public ReportRunItem TraversalReport { get; }
        public byte[] PdfABytes { get; } = "%PDF-1.4\nservice-a\n%%EOF\n"u8.ToArray();

        private Fixture(
            string directory,
            SchoolReportsDbContext db,
            ReportDownloadService downloads,
            ReportRunItem reportA,
            ReportRunItem reportB,
            ReportRunItem missingReport,
            ReportRunItem deletedReport,
            ReportRunItem traversalReport)
        {
            _directory = directory;
            _db = db;
            Downloads = downloads;
            ReportA = reportA;
            ReportB = reportB;
            MissingReport = missingReport;
            DeletedReport = deletedReport;
            TraversalReport = traversalReport;
        }

        public static async Task<Fixture> CreateAsync()
        {
            var directory = Path.Combine(Path.GetTempPath(), "asr-download-service", Guid.NewGuid().ToString("N"));
            var outputRoot = Path.Combine(directory, "output");
            Directory.CreateDirectory(outputRoot);

            var connectionString = new SqliteConnectionStringBuilder
            {
                DataSource = Path.Combine(directory, "schoolreports.db"),
                Mode = SqliteOpenMode.ReadWriteCreate,
                Pooling = false,
            }.ToString();
            var db = new SchoolReportsDbContext(
                new DbContextOptionsBuilder<SchoolReportsDbContext>().UseSqlite(connectionString).Options);
            await db.MigrateAsync();

            var schoolA = new School { Code = "10701", Name = "School A" };
            var schoolB = new School { Code = "23306", Name = "School B" };
            db.Schools.AddRange(schoolA, schoolB);
            await db.SaveChangesAsync();

            var pdfA = Path.Combine(outputRoot, "2025", "10701", "summary-report.pdf");
            var pdfB = Path.Combine(outputRoot, "2025", "23306", "summary-report.pdf");
            var deletedPath = Path.Combine(outputRoot, "2025", "10701", "deleted-report.pdf");
            var missingPath = Path.Combine(outputRoot, "2025", "10701", "missing-report.pdf");
            WritePdf(pdfA, "%PDF-1.4\nservice-a\n%%EOF\n"u8.ToArray());
            WritePdf(pdfB, "%PDF-1.4\nservice-b\n%%EOF\n"u8.ToArray());
            WritePdf(deletedPath, "%PDF-1.4\ndeleted\n%%EOF\n"u8.ToArray());

            var run = new ReportRun
            {
                Mode = ReportGenerationMode.Single,
                Status = RunStatus.Completed,
                StartedUtc = DateTimeOffset.UtcNow,
            };
            db.ReportRuns.Add(run);
            await db.SaveChangesAsync();

            var reportA = Item(run.Id, schoolA.Id, pdfA);
            var reportB = Item(run.Id, schoolB.Id, pdfB);
            var missing = Item(run.Id, schoolA.Id, missingPath);
            var deleted = Item(run.Id, schoolA.Id, deletedPath);
            var traversal = Item(run.Id, schoolA.Id, Path.Combine(outputRoot, "..", "secret.pdf"));
            db.ReportRunItems.AddRange(reportA, reportB, missing, deleted, traversal);
            db.UserSchoolAccess.AddRange(
                new UserSchoolAccess
                {
                    UserId = "user-a",
                    SchoolId = schoolA.Id,
                    AccessLevel = SchoolAccessLevel.Generate,
                    CreatedAt = DateTimeOffset.UtcNow,
                },
                new UserSchoolAccess
                {
                    UserId = "user-b",
                    SchoolId = schoolB.Id,
                    AccessLevel = SchoolAccessLevel.View,
                    CreatedAt = DateTimeOffset.UtcNow,
                });
            await db.SaveChangesAsync();
            File.Delete(deletedPath);

            var downloads = new ReportDownloadService(
                db,
                new ReportAuthorizationService(db),
                Options.Create(new ReportGenerationOptions { OutputRoot = outputRoot }));
            return new Fixture(directory, db, downloads, reportA, reportB, missing, deleted, traversal);
        }

        public async ValueTask DisposeAsync()
        {
            await _db.DisposeAsync();
            try
            {
                if (Directory.Exists(_directory))
                {
                    Directory.Delete(_directory, recursive: true);
                }
            }
            catch (IOException)
            {
            }
        }

        private static ReportRunItem Item(int runId, int schoolId, string outputPath) =>
            new()
            {
                ReportRunId = runId,
                SchoolId = schoolId,
                Status = RunStatus.Completed,
                OutputPath = outputPath,
            };

        private static void WritePdf(string path, byte[] bytes)
        {
            Directory.CreateDirectory(Path.GetDirectoryName(path)!);
            File.WriteAllBytes(path, bytes);
        }
    }
}
