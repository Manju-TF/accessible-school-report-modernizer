using System.Security.Claims;
using AccessibleSchoolReports.Application.Knowledge;
using AccessibleSchoolReports.Application.Security;
using AccessibleSchoolReports.Domain.Entities;
using AccessibleSchoolReports.Domain.Knowledge;
using AccessibleSchoolReports.Domain.Persistence;
using AccessibleSchoolReports.Domain.Security;
using AccessibleSchoolReports.Infrastructure.Knowledge;
using AccessibleSchoolReports.Infrastructure.Persistence;
using AccessibleSchoolReports.Infrastructure.Security;
using AccessibleSchoolReports.UnitTests.Embeddings;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace AccessibleSchoolReports.UnitTests.Knowledge;

internal sealed class KnowledgeRetrievalTestFixture : IAsyncDisposable
{
    public const string SchoolBSecret = "SCHOOL-B-SECRET-TEXT";
    public const string AdminSecret = "ADMIN-ONLY-SECRET-TEXT";
    public const string SchoolAText = "School A permitted report text CF-S-00";
    public const string LegacyText = "Global salary suppression note";

    public static readonly float[] QueryVector = [1f, 0f, 0f, 0f];

    private readonly string _directory;

    public EmbeddingOptions Options { get; }
    public SchoolReportsDbContext Db { get; }
    public DbContextOptions<SchoolReportsDbContext> DbOptions { get; }
    public int SchoolAId { get; }
    public int SchoolBId { get; }
    public int SchoolAReportId { get; }
    public int SchoolBReportId { get; }
    public int SchoolAChunkId { get; }
    public int SchoolBChunkId { get; }
    public int LegacyChunkId { get; }
    public int AdminChunkId { get; }

    private KnowledgeRetrievalTestFixture(
        string directory,
        EmbeddingOptions options,
        SchoolReportsDbContext db,
        DbContextOptions<SchoolReportsDbContext> dbOptions,
        int schoolAId,
        int schoolBId,
        int schoolAReportId,
        int schoolBReportId,
        int schoolAChunkId,
        int schoolBChunkId,
        int legacyChunkId,
        int adminChunkId)
    {
        _directory = directory;
        Options = options;
        Db = db;
        DbOptions = dbOptions;
        SchoolAId = schoolAId;
        SchoolBId = schoolBId;
        SchoolAReportId = schoolAReportId;
        SchoolBReportId = schoolBReportId;
        SchoolAChunkId = schoolAChunkId;
        SchoolBChunkId = schoolBChunkId;
        LegacyChunkId = legacyChunkId;
        AdminChunkId = adminChunkId;
    }

    public static ClaimsPrincipal Principal(string userId, string role) =>
        new(new ClaimsIdentity(
            [
                new Claim(ClaimTypes.NameIdentifier, userId),
                new Claim(ClaimTypes.Name, userId),
                new Claim(ClaimTypes.Role, role),
            ],
            "test"));

    public static async Task<KnowledgeRetrievalTestFixture> CreateAsync()
    {
        var directory = Path.Combine(Path.GetTempPath(), "asr-retrieval", Guid.NewGuid().ToString("N"));
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
                UserId = "viewer-a",
                SchoolId = schoolA.Id,
                AccessLevel = SchoolAccessLevel.View,
                CreatedAt = DateTimeOffset.UtcNow,
            });
        await db.SaveChangesAsync();

        var now = DateTimeOffset.UtcNow;
        var modelKey = "Fake/test-embed";
        var reportA = Document(
            "a.pdf",
            KnowledgeDocumentType.GeneratedReport,
            KnowledgeAuthorizationScope.Report,
            schoolA.Id,
            itemA.Id,
            "10701");
        var reportB = Document(
            "b.pdf",
            KnowledgeDocumentType.GeneratedReport,
            KnowledgeAuthorizationScope.Report,
            schoolB.Id,
            itemB.Id,
            "23306");
        var legacy = Document(
            "legacy.md",
            KnowledgeDocumentType.Legacy,
            KnowledgeAuthorizationScope.Authenticated,
            null,
            null,
            null);
        var admin = Document(
            "admin-policy.md",
            KnowledgeDocumentType.Project,
            KnowledgeAuthorizationScope.Admin,
            null,
            null,
            null);
        reportA.Chunks.Add(Chunk(SchoolAText, "CF-S-00", "page 1", [0.6f, 0.8f, 0f, 0f], modelKey, now));
        reportB.Chunks.Add(Chunk(SchoolBSecret, null, "page 2", QueryVector, modelKey, now));
        legacy.Chunks.Add(Chunk(LegacyText, "SS-00", "legacy/sas/notes.md:1", [0.5f, 0.866f, 0f, 0f], modelKey, now));
        admin.Chunks.Add(Chunk(AdminSecret, null, "docs/admin.md:1", [0.99f, 0.01f, 0f, 0f], modelKey, now));
        db.KnowledgeDocuments.AddRange(reportA, reportB, legacy, admin);
        await db.SaveChangesAsync();

        return new KnowledgeRetrievalTestFixture(
            directory,
            new EmbeddingOptions
            {
                Provider = "Fake",
                Endpoint = "https://embeddings.test/v1/embeddings",
                Model = "test-embed",
                Dimensions = 4,
                TimeoutSeconds = 5,
                MaxRetries = 0,
                MaxBatchSize = 8,
                ApiKey = "unused",
            },
            db,
            dbOptions,
            schoolA.Id,
            schoolB.Id,
            itemA.Id,
            itemB.Id,
            reportA.Chunks.Single().Id,
            reportB.Chunks.Single().Id,
            legacy.Chunks.Single().Id,
            admin.Chunks.Single().Id);
    }

    public (KnowledgeRetrievalService Service, FakeEmbeddingService Embeddings) CreateSut()
    {
        var factory = new EmbeddingTestFixture.Factory(DbOptions);
        var authorization = new ReportAuthorizationService(Db);
        var embeddings = new FakeEmbeddingService(factory, authorization, Options)
        {
            NextQueryVector = QueryVector,
        };
        var service = new KnowledgeRetrievalService(factory, embeddings, authorization);
        return (service, embeddings);
    }

    public (KnowledgeAssistantService Assistant, FakeLanguageModelService LanguageModel, FakeEmbeddingService Embeddings) CreateAssistant()
    {
        var (retrieval, embeddings) = CreateSut();
        var languageModel = new FakeLanguageModelService();
        var assistant = new KnowledgeAssistantService(retrieval, languageModel);
        return (assistant, languageModel, embeddings);
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
            ReportYear = type == KnowledgeDocumentType.GeneratedReport ? 2025 : null,
            ReportType = type == KnowledgeDocumentType.GeneratedReport ? "Summary" : null,
            AuthorizationScope = scope,
            CreatedAt = DateTimeOffset.UtcNow,
        };

    private static KnowledgeChunk Chunk(
        string content,
        string? ruleId,
        string location,
        float[] vector,
        string modelKey,
        DateTimeOffset created) =>
        new()
        {
            ChunkNumber = 1,
            Content = content,
            RuleId = ruleId,
            Category = "report",
            SourceLocation = location,
            Embedding = EmbeddingVectorConvert.ToBytes(vector),
            EmbeddingModel = modelKey,
            CreatedAt = created,
        };
}
