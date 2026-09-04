using AccessibleSchoolReports.Application.Knowledge;
using AccessibleSchoolReports.Infrastructure.Embeddings;
using AccessibleSchoolReports.Infrastructure.Knowledge;
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

        public static async Task<StartupFixture> CreateAsync()
        {
            var directory = Path.Combine(Path.GetTempPath(), "asr-knowledge-startup", Guid.NewGuid().ToString("N"));
            var root = Path.Combine(directory, "repo");
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
            services.AddSchoolReportsPersistence(connectionString);
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
