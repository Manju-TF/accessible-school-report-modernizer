using AccessibleSchoolReports.Domain.Entities;
using AccessibleSchoolReports.Domain.Persistence;
using AccessibleSchoolReports.Infrastructure.Persistence;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace AccessibleSchoolReports.IntegrationTests;

public sealed class SchoolReportsDbContextTests
{
    [Fact]
    public async Task Migrate_CreatesDatabaseFile()
    {
        await using var db = await SqliteTestDatabase.CreateAsync();

        var dataSource = new SqliteConnectionStringBuilder(db.ConnectionString).DataSource;
        Assert.True(File.Exists(dataSource));
        Assert.True(await db.Context.Database.CanConnectAsync());
    }

    [Fact]
    public async Task ConnectionString_IsReadFromConfiguration()
    {
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["ConnectionStrings:SchoolReports"] = "Data Source=data/schoolreports.db",
            })
            .Build();

        var configured = configuration.GetConnectionString("SchoolReports");
        Assert.Equal("Data Source=data/schoolreports.db", configured);

        var resolved = SqliteConnectionString.Resolve(configured!, Path.GetTempPath());
        Assert.Contains("schoolreports.db", resolved, StringComparison.OrdinalIgnoreCase);
        Assert.True(Path.IsPathRooted(new SqliteConnectionStringBuilder(resolved).DataSource));
    }

    [Fact]
    public async Task InsertsSchoolImportAndGraduate_WithForeignKeys()
    {
        await using var db = await SqliteTestDatabase.CreateAsync();
        var context = db.Context;

        var import = new ImportRun
        {
            FileName = "sample.xlsx",
            StartedUtc = DateTimeOffset.UtcNow,
            Status = RunStatus.Completed,
            ImportedRowCount = 1,
        };
        var school = new School { Code = "99999", Name = "Test University School of Law" };
        context.ImportRuns.Add(import);
        context.Schools.Add(school);
        await context.SaveChangesAsync();

        context.GraduateRecords.Add(new GraduateRecord
        {
            ImportRunId = import.Id,
            SchoolId = school.Id,
            Sex3 = "F",
            Jobcat1 = "LJD",
            SalFtPerm = 85000m,
        });
        await context.SaveChangesAsync();

        var stored = await context.GraduateRecords
            .Include(g => g.School)
            .Include(g => g.ImportRun)
            .SingleAsync();

        Assert.Equal("99999", stored.School.Code);
        Assert.Equal("sample.xlsx", stored.ImportRun.FileName);
        Assert.Equal(85000m, stored.SalFtPerm);
    }

    [Fact]
    public async Task Graduate_WithUnknownSchool_FailsForeignKey()
    {
        await using var db = await SqliteTestDatabase.CreateAsync();
        var context = db.Context;

        var import = new ImportRun
        {
            StartedUtc = DateTimeOffset.UtcNow,
            Status = RunStatus.Running,
        };
        context.ImportRuns.Add(import);
        await context.SaveChangesAsync();

        context.GraduateRecords.Add(new GraduateRecord
        {
            ImportRunId = import.Id,
            SchoolId = 4242,
        });

        await Assert.ThrowsAnyAsync<DbUpdateException>(() => context.SaveChangesAsync());
    }

    [Fact]
    public async Task SchoolCode_IsUnique()
    {
        await using var db = await SqliteTestDatabase.CreateAsync();
        var context = db.Context;

        context.Schools.Add(new School { Code = "10701" });
        await context.SaveChangesAsync();

        context.Schools.Add(new School { Code = "10701" });
        await Assert.ThrowsAnyAsync<DbUpdateException>(() => context.SaveChangesAsync());
    }

    [Fact]
    public async Task ReportRunItem_StoresOutputPath_NotPdfBytes()
    {
        await using var db = await SqliteTestDatabase.CreateAsync();
        var context = db.Context;

        var school = new School { Code = "10701" };
        var run = new ReportRun
        {
            Mode = ReportGenerationMode.Single,
            Status = RunStatus.Completed,
            StartedUtc = DateTimeOffset.UtcNow,
            CompletedUtc = DateTimeOffset.UtcNow,
            OutputDirectory = "data/reports",
            MaxParallelism = 1,
        };
        context.Schools.Add(school);
        context.ReportRuns.Add(run);
        await context.SaveChangesAsync();

        context.ReportRunItems.Add(new ReportRunItem
        {
            ReportRunId = run.Id,
            SchoolId = school.Id,
            Status = RunStatus.Completed,
            OutputPath = "data/reports/10701_summary2025.pdf",
        });
        await context.SaveChangesAsync();

        var item = await context.ReportRunItems.SingleAsync();
        Assert.Equal("data/reports/10701_summary2025.pdf", item.OutputPath);

        var itemType = context.Model.FindEntityType(typeof(ReportRunItem));
        Assert.NotNull(itemType);
        Assert.DoesNotContain(itemType.GetProperties(), property => property.ClrType == typeof(byte[]));
        Assert.Null(itemType.FindProperty("Pdf"));
        Assert.Null(itemType.FindProperty("Content"));
        Assert.NotNull(itemType.FindProperty(nameof(ReportRunItem.OutputPath)));
    }

    [Fact]
    public async Task SaveChangesAsync_HonorsCancellation()
    {
        await using var db = await SqliteTestDatabase.CreateAsync();
        var context = db.Context;
        context.Schools.Add(new School { Code = "12001" });

        using var cts = new CancellationTokenSource();
        await cts.CancelAsync();

        await Assert.ThrowsAnyAsync<OperationCanceledException>(
            () => context.SaveChangesAsync(cts.Token));
    }

    [Fact]
    public async Task MigrateAsync_HonorsCancellation()
    {
        var directory = Path.Combine(Path.GetTempPath(), "asr-sqlite-tests", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(directory);
        try
        {
            var options = new DbContextOptionsBuilder<SchoolReportsDbContext>()
                .UseSqlite($"Data Source={Path.Combine(directory, "schoolreports.db")}")
                .Options;

            await using var context = new SchoolReportsDbContext(options);
            using var cts = new CancellationTokenSource();
            await cts.CancelAsync();

            await Assert.ThrowsAnyAsync<OperationCanceledException>(
                () => context.MigrateAsync(cts.Token));
        }
        finally
        {
            SqliteConnection.ClearAllPools();
            if (Directory.Exists(directory))
            {
                Directory.Delete(directory, recursive: true);
            }
        }
    }
}
