using System.Security.Cryptography;
using AccessibleSchoolReports.Application.Knowledge;
using AccessibleSchoolReports.Domain.Entities;
using AccessibleSchoolReports.Domain.Knowledge;
using AccessibleSchoolReports.Infrastructure.Knowledge;
using AccessibleSchoolReports.Infrastructure.Persistence;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace AccessibleSchoolReports.UnitTests.Knowledge;

public sealed class KnowledgeIngestionServiceTests
{
    [Fact]
    public async Task Ingest_IndexesSasAndProjectDocs_WithTypeAndScope()
    {
        await using var fixture = await Fixture.CreateAsync();
        var result = await fixture.IngestAsync();

        Assert.Contains("legacy/sas/sample.sas", result.Indexed);
        Assert.Contains("docs/capstone/business-rules.md", result.Indexed);
        Assert.Contains("README.md", result.Indexed);
        Assert.Empty(result.SkippedUnchanged);
        Assert.DoesNotContain(result.Indexed, path => path.EndsWith(".xlsx", StringComparison.OrdinalIgnoreCase));

        var documents = await fixture.Db.KnowledgeDocuments.Include(row => row.Chunks).ToListAsync();
        var sas = documents.Single(row => row.SourceIdentifier == "legacy/sas/sample.sas");
        var rules = documents.Single(row => row.SourceIdentifier == "docs/capstone/business-rules.md");

        Assert.Equal(KnowledgeDocumentType.Legacy, sas.DocumentType);
        Assert.Equal(KnowledgeDocumentType.Project, rules.DocumentType);
        Assert.All(documents, document =>
        {
            Assert.Equal(KnowledgeAuthorizationScope.Authenticated, document.AuthorizationScope);
            Assert.Null(document.SchoolId);
            Assert.Null(document.ReportId);
            Assert.All(document.Chunks, chunk =>
            {
                Assert.Null(chunk.Embedding);
                Assert.Null(chunk.EmbeddingModel);
                Assert.False(string.IsNullOrWhiteSpace(chunk.SourceLocation));
            });
        });
        Assert.Contains(rules.Chunks, chunk => chunk.RuleId == "CF-S-00");
    }

    [Fact]
    public async Task Ingest_SkipsUnchangedFiles()
    {
        await using var fixture = await Fixture.CreateAsync();
        var first = await fixture.IngestAsync();
        var second = await fixture.IngestAsync();

        Assert.NotEmpty(first.Indexed);
        Assert.Empty(first.SkippedUnchanged);
        Assert.Empty(second.Indexed);
        Assert.Empty(second.Reindexed);
        Assert.Equal(first.Indexed, second.SkippedUnchanged);
        Assert.Equal(first.Indexed.Count, await fixture.Db.KnowledgeDocuments.CountAsync());
    }

    [Fact]
    public async Task Ingest_ReindexesChangedFiles()
    {
        await using var fixture = await Fixture.CreateAsync();
        await fixture.IngestAsync();
        var originalId = (await fixture.Db.KnowledgeDocuments
            .SingleAsync(row => row.SourceIdentifier == "README.md")).Id;

        File.WriteAllText(Path.Combine(fixture.Root, "README.md"), "# Changed\n\nNew CF-AMB-01 note.\n");
        var result = await fixture.IngestAsync();

        Assert.Equal(["README.md"], result.Reindexed);
        Assert.DoesNotContain("README.md", result.SkippedUnchanged);

        var readme = await fixture.Db.KnowledgeDocuments
            .Include(row => row.Chunks)
            .SingleAsync(row => row.SourceIdentifier == "README.md");
        Assert.Equal(originalId, readme.Id);
        Assert.Contains(readme.Chunks, chunk => chunk.RuleId == "CF-AMB-01");
        Assert.Contains(
            readme.Chunks,
            chunk => chunk.Content.Contains("Changed", StringComparison.Ordinal));
    }

    [Fact]
    public async Task Ingest_HashesSha256_AndDoesNotModifySources()
    {
        await using var fixture = await Fixture.CreateAsync();
        var sasPath = Path.Combine(fixture.Root, "legacy", "sas", "sample.sas");
        var beforeBytes = File.ReadAllBytes(sasPath);
        var beforeHash = Convert.ToHexString(SHA256.HashData(beforeBytes)).ToLowerInvariant();
        var beforeWrite = File.GetLastWriteTimeUtc(sasPath);

        await fixture.IngestAsync();

        var afterBytes = File.ReadAllBytes(sasPath);
        Assert.Equal(beforeBytes, afterBytes);
        Assert.Equal(beforeWrite, File.GetLastWriteTimeUtc(sasPath));

        var stored = await fixture.Db.KnowledgeDocuments
            .SingleAsync(row => row.SourceIdentifier == "legacy/sas/sample.sas");
        Assert.Equal(beforeHash, stored.ContentHash);
        Assert.Equal(KnowledgeContentHash.Sha256Hex(beforeBytes), stored.ContentHash);
    }

    [Fact]
    public async Task Ingest_DoesNotIngestGraduateOrStudentRecords()
    {
        await using var fixture = await Fixture.CreateAsync();
        fixture.Db.Schools.Add(new School { Code = "10701", Name = "School A" });
        await fixture.Db.SaveChangesAsync();
        var graduateCount = await fixture.Db.GraduateRecords.CountAsync();

        var result = await fixture.IngestAsync();

        Assert.Equal(graduateCount, await fixture.Db.GraduateRecords.CountAsync());
        Assert.DoesNotContain(
            await fixture.Db.KnowledgeDocuments.Select(row => row.SourceIdentifier).ToListAsync(),
            path => path.Contains("graduates", StringComparison.OrdinalIgnoreCase)
                || path.EndsWith(".xlsx", StringComparison.OrdinalIgnoreCase)
                || path.StartsWith("data/", StringComparison.OrdinalIgnoreCase));
        Assert.DoesNotContain("data/graduates.xlsx", result.Indexed);
        Assert.False(KnowledgeSourceCatalog.IsAllowedRelativePath("data/graduates.xlsx"));
        Assert.False(KnowledgeSourceCatalog.IsAllowedRelativePath("../legacy/sas/secret.sas"));
    }

    [Fact]
    public async Task Ingest_IsDeterministicForTheSameFiles()
    {
        await using var fixture = await Fixture.CreateAsync();
        await fixture.IngestAsync();
        var first = await SnapshotAsync(fixture.Db);

        await using var other = await Fixture.CreateAsync();
        await other.IngestAsync();
        var second = await SnapshotAsync(other.Db);

        Assert.Equal(first, second);
    }

    private static async Task<string> SnapshotAsync(SchoolReportsDbContext db)
    {
        var documents = await db.KnowledgeDocuments
            .AsNoTracking()
            .Include(row => row.Chunks)
            .OrderBy(row => row.SourceIdentifier)
            .ToListAsync();
        return string.Join('\n', documents.Select(document =>
            $"{document.SourceIdentifier}|{document.ContentHash}|{document.DocumentType}|{document.AuthorizationScope}|"
            + string.Join(';', document.Chunks.OrderBy(chunk => chunk.ChunkNumber)
                .Select(chunk => $"{chunk.ChunkNumber}:{chunk.RuleId}:{chunk.SourceLocation}:{chunk.Content}"))));
    }

    private sealed class Fixture : IAsyncDisposable
    {
        private readonly string _directory;

        public string Root { get; }
        public SchoolReportsDbContext Db { get; }
        public KnowledgeIngestionService Ingestion { get; }

        private Fixture(string directory, string root, SchoolReportsDbContext db)
        {
            _directory = directory;
            Root = root;
            Db = db;
            Ingestion = new KnowledgeIngestionService(db);
        }

        public Task<KnowledgeIngestionResult> IngestAsync() =>
            Ingestion.IngestLegacyAndProjectDocumentsAsync(Root);

        public static async Task<Fixture> CreateAsync()
        {
            var directory = Path.Combine(Path.GetTempPath(), "asr-knowledge-ingest", Guid.NewGuid().ToString("N"));
            var root = Path.Combine(directory, "repo");
            Directory.CreateDirectory(Path.Combine(root, "legacy", "sas"));
            Directory.CreateDirectory(Path.Combine(root, "docs", "capstone"));
            Directory.CreateDirectory(Path.Combine(root, "data"));
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
            File.WriteAllBytes(Path.Combine(root, "data", "graduates.xlsx"), "not-a-real-workbook"u8.ToArray());

            var connectionString = new SqliteConnectionStringBuilder
            {
                DataSource = Path.Combine(directory, "schoolreports.db"),
                Mode = SqliteOpenMode.ReadWriteCreate,
                Pooling = false,
            }.ToString();
            var db = new SchoolReportsDbContext(
                new DbContextOptionsBuilder<SchoolReportsDbContext>().UseSqlite(connectionString).Options);
            await db.MigrateAsync();
            return new Fixture(directory, root, db);
        }

        public async ValueTask DisposeAsync()
        {
            await Db.DisposeAsync();
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
