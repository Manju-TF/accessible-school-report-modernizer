using AccessibleSchoolReports.Application.Knowledge;
using AccessibleSchoolReports.Application.Reporting;
using AccessibleSchoolReports.Domain.Entities;
using AccessibleSchoolReports.Domain.Knowledge;
using AccessibleSchoolReports.Domain.Persistence;
using AccessibleSchoolReports.Infrastructure.Embeddings;
using AccessibleSchoolReports.Infrastructure.Knowledge;
using AccessibleSchoolReports.Infrastructure.Pdf;
using AccessibleSchoolReports.Infrastructure.Persistence;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace AccessibleSchoolReports.UnitTests.Knowledge;

public sealed class KnowledgeStartupTests
{
    [Fact]
    public async Task Prepare_IngestsCatalogAndWritesLexicalEmbeddings()
    {
        await using var fixture = await StartupFixture.CreateAsync();
        var prepare = await KnowledgeStartup.PrepareAsync(fixture.Services, fixture.Root, "Development");
        Assert.Null(prepare.Error);

        await using var db = await fixture.CreateDbAsync();
        var documents = await db.KnowledgeDocuments.CountAsync();
        var embedded = await db.KnowledgeChunks.CountAsync(chunk =>
            chunk.Embedding != null && chunk.Embedding.Length > 0);
        Assert.True(documents >= 3, $"documents={documents}");
        Assert.True(embedded >= 3, $"embedded={embedded} documents={documents}");
        Assert.Contains(await db.KnowledgeChunks.Select(chunk => chunk.RuleId).ToListAsync(), id => id == "CF-S-00");
    }

    [Fact]
    public async Task Prepare_SkipsTestingEnvironment()
    {
        await using var fixture = await StartupFixture.CreateAsync();
        await KnowledgeStartup.PrepareAsync(fixture.Services, fixture.Root, "Testing");

        await using var db = await fixture.CreateDbAsync();
        Assert.Equal(0, await db.KnowledgeDocuments.CountAsync());
    }

    [Fact]
    public async Task Prepare_BackfillsCompletedGeneratedReports()
    {
        await using var fixture = await StartupFixture.CreateAsync(withGeneratedPdf: true);
        var prepare = await KnowledgeStartup.PrepareAsync(fixture.Services, fixture.Root, "Development");
        Assert.Null(prepare.Error);

        await using var db = await fixture.CreateDbAsync();
        var report = await db.KnowledgeDocuments
            .Include(document => document.Chunks)
            .SingleAsync(document => document.DocumentType == KnowledgeDocumentType.GeneratedReport);
        Assert.Equal("10701", report.SchoolCode);
        Assert.Equal(2025, report.ReportYear);
        Assert.Contains(
            report.Chunks,
            chunk => chunk.Content.StartsWith("[School 10701", StringComparison.Ordinal)
                && chunk.Embedding is { Length: > 0 });
    }

    private sealed class StartupFixture : IAsyncDisposable
    {
        private readonly string _directory;
        private readonly ServiceProvider _provider;

        public string Root { get; }
        public IServiceProvider Services => _provider;

        public Task<SchoolReportsDbContext> CreateDbAsync() =>
            Task.FromResult(_provider.GetRequiredService<IDbContextFactory<SchoolReportsDbContext>>().CreateDbContext());

        private StartupFixture(string directory, string root, ServiceProvider provider)
        {
            _directory = directory;
            Root = root;
            _provider = provider;
        }

        public static async Task<StartupFixture> CreateAsync(bool withGeneratedPdf = false)
        {
            var directory = Path.Combine(Path.GetTempPath(), "asr-knowledge-startup", Guid.NewGuid().ToString("N"));
            var root = Path.Combine(directory, "repo");
            var outputRoot = Path.Combine(directory, "output");
            Directory.CreateDirectory(Path.Combine(root, "legacy", "sas"));
            Directory.CreateDirectory(Path.Combine(root, "docs", "capstone"));
            File.WriteAllText(Path.Combine(root, "AccessibleSchoolReports.sln"), string.Empty);
            File.WriteAllText(
                Path.Combine(root, "legacy", "sas", "sample.sas"),
                "* header\r\nproc format;\r\nvalue $time 'BGRAD' = 'Before Graduation';\r\nrun;\r\n");
            File.WriteAllText(
                Path.Combine(root, "docs", "capstone", "business-rules.md"),
                """
                # Rules

                | Rule ID | Notes |
                |---|---|
                | CF-S-00 | n ge 5 |
                """);
            File.WriteAllText(Path.Combine(root, "README.md"), "# Sample\n\nCapstone notes.\n");

            var connectionString = new SqliteConnectionStringBuilder
            {
                DataSource = Path.Combine(directory, "schoolreports.db"),
                Mode = SqliteOpenMode.ReadWriteCreate,
                Pooling = false,
            }.ToString();
            var services = new ServiceCollection();
            services.AddLogging();
            services.AddSchoolReportsPersistence(connectionString, options =>
            {
                options.OutputRoot = outputRoot;
                options.ClassYear = "2025";
            });
            services.AddSchoolReportsEmbeddings(options =>
            {
                options.Provider = "Lexical";
                options.Model = "hashed-bow";
                options.Dimensions = HashedLexicalVector.DefaultDimensions;
            });
            var provider = services.BuildServiceProvider();
            await using (var db = provider.GetRequiredService<IDbContextFactory<SchoolReportsDbContext>>().CreateDbContext())
            {
                await db.MigrateAsync();
                if (withGeneratedPdf)
                {
                    var pdfPath = Path.Combine(outputRoot, "2025", "10701", "summary-report.pdf");
                    Directory.CreateDirectory(Path.GetDirectoryName(pdfPath)!);
                    var report = new SchoolReport
                    {
                        SchoolCode = "10701",
                        SchoolName = "School A",
                        Rows = [new() { Analvar = "A", Newvar = "A", Count = 5, Percent = 100m }],
                        Sections = [new() { Analvar = "A", Details = [], SubtotalCount = 5, SubtotalPercent = 100m }],
                    };
                    await using (var stream = File.Create(pdfPath))
                    {
                        new QuestPdfAccessiblePdfGenerator().Generate(report, stream);
                    }

                    var school = new School { Code = "10701", Name = "School A" };
                    db.Schools.Add(school);
                    await db.SaveChangesAsync();
                    var run = new ReportRun
                    {
                        Mode = ReportGenerationMode.Single,
                        Status = RunStatus.Completed,
                        StartedUtc = DateTimeOffset.UtcNow,
                        OutputDirectory = Path.Combine(outputRoot, "2025"),
                    };
                    db.ReportRuns.Add(run);
                    await db.SaveChangesAsync();
                    db.ReportRunItems.Add(new ReportRunItem
                    {
                        ReportRunId = run.Id,
                        SchoolId = school.Id,
                        Status = RunStatus.Completed,
                        OutputPath = pdfPath,
                    });
                    await db.SaveChangesAsync();
                }
            }

            return new StartupFixture(directory, root, provider);
        }

        public async ValueTask DisposeAsync()
        {
            await _provider.DisposeAsync();
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
