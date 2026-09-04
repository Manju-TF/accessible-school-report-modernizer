using System.Security.Claims;
using AccessibleSchoolReports.Application.Knowledge;
using AccessibleSchoolReports.Application.Security;
using AccessibleSchoolReports.Domain.Entities;
using AccessibleSchoolReports.Domain.Knowledge;
using AccessibleSchoolReports.Domain.Persistence;
using AccessibleSchoolReports.Domain.Security;
using AccessibleSchoolReports.Infrastructure.Persistence;
using AccessibleSchoolReports.Infrastructure.Security;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace AccessibleSchoolReports.UnitTests.Embeddings;

internal sealed class EmbeddingTestFixture : IAsyncDisposable
{
    public const string SchoolBSecret = "SCHOOL-B-SECRET-TEXT";

    private readonly string _directory;

    public EmbeddingOptions Options { get; }
    public SchoolReportsDbContext Db { get; }
    public DbContextOptions<SchoolReportsDbContext> DbOptions { get; }
    public int SchoolAChunkId { get; }
    public int SchoolBChunkId { get; }
    public int LegacyChunkId { get; }
    public string SchoolAText { get; } = "School A permitted report text CF-S-00";

    private EmbeddingTestFixture(
        string directory,
        EmbeddingOptions options,
        SchoolReportsDbContext db,
        DbContextOptions<SchoolReportsDbContext> dbOptions,
        int schoolAChunkId,
        int schoolBChunkId,
        int legacyChunkId)
    {
        _directory = directory;
        Options = options;
        Db = db;
        DbOptions = dbOptions;
        SchoolAChunkId = schoolAChunkId;
        SchoolBChunkId = schoolBChunkId;
        LegacyChunkId = legacyChunkId;
    }

    public static ClaimsPrincipal Principal(string userId, string role) =>
        new(new ClaimsIdentity(
            [
                new Claim(ClaimTypes.NameIdentifier, userId),
                new Claim(ClaimTypes.Name, userId),
                new Claim(ClaimTypes.Role, role),
            ],
            "test"));

    public FakeEmbeddingService CreateFake() =>
        new(new Factory(DbOptions), new ReportAuthorizationService(Db), Options);

    public static async Task<EmbeddingTestFixture> CreateAsync()
    {
        var directory = Path.Combine(Path.GetTempPath(), "asr-embeddings", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(directory);
        var connectionString = new SqliteConnectionStringBuilder
        {
            DataSource = Path.Combine(directory, "schoolreports.db"),
            Mode = SqliteOpenMode.ReadWriteCreate,
            Pooling = false,
        }.ToString();
        var dbOptions = new DbContextOptionsBuilder<SchoolReportsDbContext>().UseSqlite(connectionString).Options;
        var db = new SchoolReportsDbContext(dbOptions);
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
        var itemA = new ReportRunItem
        {
            ReportRunId = run.Id,
            SchoolId = schoolA.Id,
            Status = RunStatus.Completed,
            OutputPath = "output/a.pdf",
        };
        var itemB = new ReportRunItem
        {
            ReportRunId = run.Id,
            SchoolId = schoolB.Id,
            Status = RunStatus.Completed,
            OutputPath = "output/b.pdf",
        };
        db.ReportRunItems.AddRange(itemA, itemB);
        db.UserSchoolAccess.Add(new UserSchoolAccess
        {
            UserId = "user-a",
            SchoolId = schoolA.Id,
            AccessLevel = SchoolAccessLevel.Generate,
            CreatedAt = DateTimeOffset.UtcNow,
        });
        await db.SaveChangesAsync();

        var now = DateTimeOffset.UtcNow;
        var reportA = Document("a.pdf", KnowledgeDocumentType.GeneratedReport, KnowledgeAuthorizationScope.Report, schoolA.Id, itemA.Id, "10701");
        var reportB = Document("b.pdf", KnowledgeDocumentType.GeneratedReport, KnowledgeAuthorizationScope.Report, schoolB.Id, itemB.Id, "23306");
        var legacy = Document("legacy.md", KnowledgeDocumentType.Legacy, KnowledgeAuthorizationScope.Authenticated, null, null, null);
        reportA.Chunks.Add(Chunk("School A permitted report text CF-S-00", now));
        reportB.Chunks.Add(Chunk(SchoolBSecret, now));
        legacy.Chunks.Add(Chunk("Global salary suppression note", now));
        db.KnowledgeDocuments.AddRange(reportA, reportB, legacy);
        await db.SaveChangesAsync();

        return new EmbeddingTestFixture(
            directory,
            new EmbeddingOptions
            {
                Provider = "OpenAICompatible",
                Endpoint = "https://embeddings.test/v1/embeddings",
                Model = "test-embed",
                Dimensions = 4,
                TimeoutSeconds = 5,
                MaxRetries = 2,
                MaxBatchSize = 8,
                ApiKey = "super-secret-key",
            },
            db,
            dbOptions,
            reportA.Chunks.Single().Id,
            reportB.Chunks.Single().Id,
            legacy.Chunks.Single().Id);
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

    private static KnowledgeDocument Document(
        string fileName,
        KnowledgeDocumentType type,
        KnowledgeAuthorizationScope scope,
        int? schoolId,
        int? reportId,
        string? schoolCode) =>
        new()
        {
            FileName = fileName,
            DocumentType = type,
            ContentHash = new string(fileName[0], 64),
            SourceIdentifier = fileName,
            IndexedAt = DateTimeOffset.UtcNow,
            SchoolId = schoolId,
            SchoolCode = schoolCode,
            ReportId = reportId,
            ReportYear = 2025,
            ReportType = type == KnowledgeDocumentType.GeneratedReport ? "Summary" : null,
            AuthorizationScope = scope,
            CreatedAt = DateTimeOffset.UtcNow,
        };

    private static KnowledgeChunk Chunk(string content, DateTimeOffset created) =>
        new()
        {
            ChunkNumber = 1,
            Content = content,
            Category = "report",
            SourceLocation = "page 1",
            CreatedAt = created,
        };

    internal sealed class Factory : IDbContextFactory<SchoolReportsDbContext>
    {
        private readonly DbContextOptions<SchoolReportsDbContext> _options;

        public Factory(DbContextOptions<SchoolReportsDbContext> options) => _options = options;

        public SchoolReportsDbContext CreateDbContext() => new(_options);

        public Task<SchoolReportsDbContext> CreateDbContextAsync(CancellationToken cancellationToken = default) =>
            Task.FromResult(CreateDbContext());
    }
}
