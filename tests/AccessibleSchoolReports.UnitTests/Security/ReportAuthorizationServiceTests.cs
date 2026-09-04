using System.Security.Claims;
using AccessibleSchoolReports.Application.Security;
using AccessibleSchoolReports.Domain.Entities;
using AccessibleSchoolReports.Domain.Persistence;
using AccessibleSchoolReports.Domain.Security;
using AccessibleSchoolReports.Infrastructure.Persistence;
using AccessibleSchoolReports.Infrastructure.Security;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace AccessibleSchoolReports.UnitTests.Security;

public sealed class ReportAuthorizationServiceTests
{
    [Fact]
    public async Task Admin_CanAccessSchoolA_AndSchoolB()
    {
        await using var fixture = await Fixture.CreateAsync();
        var admin = Principal("admin", AppRoles.Admin);

        Assert.True(await fixture.Auth.CanAccessSchoolAsync(admin, fixture.SchoolA.Id));
        Assert.True(await fixture.Auth.CanAccessSchoolAsync(admin, fixture.SchoolB.Id));
        Assert.True(await fixture.Auth.CanViewReportAsync(admin, fixture.ReportA));
        Assert.True(await fixture.Auth.CanViewReportAsync(admin, fixture.ReportB));
        Assert.True(await fixture.Auth.CanGenerateReportAsync(admin, fixture.SchoolA.Id));
        Assert.True(await fixture.Auth.CanGenerateReportAsync(admin, fixture.SchoolB.Id));
    }

    [Fact]
    public async Task UserA_CanAccessSchoolA_AndIsDeniedSchoolB()
    {
        await using var fixture = await Fixture.CreateAsync();
        var userA = Principal("user-a", AppRoles.ReportUser);

        Assert.True(await fixture.Auth.CanAccessSchoolAsync(userA, fixture.SchoolA.Id));
        Assert.True(await fixture.Auth.CanViewReportAsync(userA, fixture.ReportA));
        Assert.True(await fixture.Auth.CanGenerateReportAsync(userA, fixture.SchoolA.Id));
        Assert.False(await fixture.Auth.CanAccessSchoolAsync(userA, fixture.SchoolB.Id));
        Assert.False(await fixture.Auth.CanViewReportAsync(userA, fixture.ReportB));
        Assert.False(await fixture.Auth.CanGenerateReportAsync(userA, fixture.SchoolB.Id));
    }

    [Fact]
    public async Task UserB_CanAccessSchoolB_AndIsDeniedSchoolA()
    {
        await using var fixture = await Fixture.CreateAsync();
        var userB = Principal("user-b", AppRoles.Viewer);

        Assert.True(await fixture.Auth.CanAccessSchoolAsync(userB, fixture.SchoolB.Id));
        Assert.True(await fixture.Auth.CanViewReportAsync(userB, fixture.ReportB));
        Assert.False(await fixture.Auth.CanGenerateReportAsync(userB, fixture.SchoolB.Id));
        Assert.False(await fixture.Auth.CanAccessSchoolAsync(userB, fixture.SchoolA.Id));
        Assert.False(await fixture.Auth.CanViewReportAsync(userB, fixture.ReportA));
        Assert.False(await fixture.Auth.CanGenerateReportAsync(userB, fixture.SchoolA.Id));
    }

    [Fact]
    public async Task UnknownReportId_CannotBypassAuthorization()
    {
        await using var fixture = await Fixture.CreateAsync();
        var admin = Principal("admin", AppRoles.Admin);
        var userA = Principal("user-a", AppRoles.ReportUser);

        Assert.False(await fixture.Auth.CanViewReportAsync(admin, reportRunItemId: 99_999));
        Assert.False(await fixture.Auth.CanViewReportAsync(userA, reportRunItemId: 99_999));
        Assert.False(await fixture.Auth.CanViewReportAsync(userA, fixture.ReportB.Id));
        Assert.False(await fixture.Auth.CanAccessSchoolAsync(userA, schoolId: 99_999));
    }

    [Fact]
    public async Task UnauthenticatedUser_IsDenied()
    {
        await using var fixture = await Fixture.CreateAsync();
        var anonymous = new ClaimsPrincipal(new ClaimsIdentity());

        Assert.False(await fixture.Auth.CanAccessSchoolAsync(anonymous, fixture.SchoolA.Id));
        Assert.False(await fixture.Auth.CanViewReportAsync(anonymous, fixture.ReportA));
        Assert.False(await fixture.Auth.CanGenerateReportAsync(anonymous, fixture.SchoolA.Id));
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

        public ReportAuthorizationService Auth { get; }
        public School SchoolA { get; }
        public School SchoolB { get; }
        public ReportRunItem ReportA { get; }
        public ReportRunItem ReportB { get; }

        private Fixture(
            string directory,
            SchoolReportsDbContext db,
            School schoolA,
            School schoolB,
            ReportRunItem reportA,
            ReportRunItem reportB)
        {
            _directory = directory;
            _db = db;
            Auth = new ReportAuthorizationService(db);
            SchoolA = schoolA;
            SchoolB = schoolB;
            ReportA = reportA;
            ReportB = reportB;
        }

        public static async Task<Fixture> CreateAsync()
        {
            var directory = Path.Combine(Path.GetTempPath(), "asr-resource-auth", Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(directory);
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

            var run = new ReportRun
            {
                Mode = ReportGenerationMode.Single,
                Status = RunStatus.Completed,
                StartedUtc = DateTimeOffset.UtcNow,
            };
            db.ReportRuns.Add(run);
            await db.SaveChangesAsync();

            var reportA = new ReportRunItem
            {
                ReportRunId = run.Id,
                SchoolId = schoolA.Id,
                Status = RunStatus.Completed,
                OutputPath = "output/a.pdf",
            };
            var reportB = new ReportRunItem
            {
                ReportRunId = run.Id,
                SchoolId = schoolB.Id,
                Status = RunStatus.Completed,
                OutputPath = "output/b.pdf",
            };
            db.ReportRunItems.AddRange(reportA, reportB);
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
            return new Fixture(directory, db, schoolA, schoolB, reportA, reportB);
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
    }
}
