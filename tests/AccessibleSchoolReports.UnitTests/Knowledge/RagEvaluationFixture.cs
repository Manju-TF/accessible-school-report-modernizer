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

internal sealed class RagEvaluationFixture : IAsyncDisposable
{
    public const string SchoolBSecret = "SCHOOL-B-SECRET-TEXT";
    public const string SchoolAMarker = "SCHOOL-A-TOTAL-REPORTED-42";
    public const string SchoolAEmployment = "SCHOOL-A-EMPLOYED-BAR-REQUIRED-30";
    public const string UserAId = "user-a";

    public const string SchoolAReportText =
        """
        School A Class of 2025 Summary Report.
        School code 10701.
        Total Reported 42.
        SCHOOL-A-TOTAL-REPORTED-42
        Employment status: 30 graduates employed in bar-required positions.
        SCHOOL-A-EMPLOYED-BAR-REQUIRED-30
        Number Reported on page 1 is 42.
        Full-time long-term salaries follow CF-S-00 suppression on this generated report.
        """;

    public const string SchoolBReportText =
        """
        School B Class of 2025 Summary Report.
        School code 23306.
        Total Reported 99.
        SCHOOL-B-SECRET-TEXT
        Unique School B employment figure 77.
        This generated report text must never be sent to the language model for a School A-only user.
        """;

    private readonly string _directory;

    public string RepositoryRoot { get; }
    public EmbeddingOptions Options { get; }
    public SchoolReportsDbContext Db { get; }
    public DbContextOptions<SchoolReportsDbContext> DbOptions { get; }
    public KnowledgeIngestionResult Ingestion { get; }
    public KnowledgeIndexResult Index { get; }
    public int SchoolAId { get; }
    public int SchoolBId { get; }
    public int SchoolAReportId { get; }
    public int SchoolBReportId { get; }
    public int DocumentCount { get; }
    public int ChunkCount { get; }
    public int EmbeddedChunkCount { get; }

    private RagEvaluationFixture(
        string directory,
        string repositoryRoot,
        EmbeddingOptions options,
        SchoolReportsDbContext db,
        DbContextOptions<SchoolReportsDbContext> dbOptions,
        KnowledgeIngestionResult ingestion,
        KnowledgeIndexResult index,
        int schoolAId,
        int schoolBId,
        int schoolAReportId,
        int schoolBReportId,
        int documentCount,
        int chunkCount,
        int embeddedChunkCount)
    {
        _directory = directory;
        RepositoryRoot = repositoryRoot;
        Options = options;
        Db = db;
        DbOptions = dbOptions;
        Ingestion = ingestion;
        Index = index;
        SchoolAId = schoolAId;
        SchoolBId = schoolBId;
        SchoolAReportId = schoolAReportId;
        SchoolBReportId = schoolBReportId;
        DocumentCount = documentCount;
        ChunkCount = chunkCount;
        EmbeddedChunkCount = embeddedChunkCount;
    }

    public static ClaimsPrincipal Principal(string userId, string role) =>
        new(new ClaimsIdentity(
            [
                new Claim(ClaimTypes.NameIdentifier, userId),
                new Claim(ClaimTypes.Name, userId),
                new Claim(ClaimTypes.Role, role),
            ],
            "test"));

    public static async Task<RagEvaluationFixture> CreateAsync()
    {
        var repositoryRoot = SqliteConnectionString.FindRepositoryRoot(AppContext.BaseDirectory);
        if (!File.Exists(Path.Combine(repositoryRoot, "AccessibleSchoolReports.sln")))
        {
            throw new InvalidOperationException("Could not locate AccessibleSchoolReports.sln for RAG evaluation.");
        }

        var directory = Path.Combine(Path.GetTempPath(), "asr-rag-eval", Guid.NewGuid().ToString("N"));
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
            OutputPath = "output/2025/10701/summary-report.pdf",
        };
        var itemB = new ReportRunItem
        {
            ReportRunId = run.Id,
            SchoolId = schoolB.Id,
            Status = RunStatus.Completed,
            OutputPath = "output/2025/23306/summary-report.pdf",
        };
        db.ReportRunItems.AddRange(itemA, itemB);
        db.UserSchoolAccess.Add(new UserSchoolAccess
        {
            UserId = UserAId,
            SchoolId = schoolA.Id,
            AccessLevel = SchoolAccessLevel.Generate,
            CreatedAt = DateTimeOffset.UtcNow,
        });
        await db.SaveChangesAsync();

        var ingestion = await new KnowledgeIngestionService(db)
            .IngestLegacyAndProjectDocumentsAsync(repositoryRoot);
        AddGeneratedReport(
            db,
            "10701-summary-report.pdf",
            itemA.OutputPath!,
            schoolA.Id,
            itemA.Id,
            "10701",
            SchoolAReportText);
        AddGeneratedReport(
            db,
            "23306-summary-report.pdf",
            itemB.OutputPath!,
            schoolB.Id,
            itemB.Id,
            "23306",
            SchoolBReportText);
        await db.SaveChangesAsync();

        var options = new EmbeddingOptions
        {
            Provider = "Lexical",
            Endpoint = "https://embeddings.test/v1/embeddings",
            Model = "hashed-bow",
            Dimensions = LexicalEmbeddingService.DefaultDimensions,
            TimeoutSeconds = 5,
            MaxRetries = 0,
            MaxBatchSize = 32,
            ApiKey = "unused",
        };
        var factory = new EmbeddingTestFixture.Factory(dbOptions);
        var authorization = new ReportAuthorizationService(db);
        var embeddings = new LexicalEmbeddingService(factory, authorization, options);
        var indexer = new KnowledgeEmbeddingIndexService(factory, embeddings, authorization);
        var index = await indexer.IndexPendingEmbeddingsAsync(Principal("admin", AppRoles.Admin));

        db.ChangeTracker.Clear();
        var documentCount = await db.KnowledgeDocuments.CountAsync();
        var chunkCount = await db.KnowledgeChunks.CountAsync();
        var embeddedChunkCount = await db.KnowledgeChunks.CountAsync(chunk =>
            chunk.Embedding != null && chunk.Embedding.Length > 0);

        return new RagEvaluationFixture(
            directory,
            repositoryRoot,
            options,
            db,
            dbOptions,
            ingestion,
            index,
            schoolA.Id,
            schoolB.Id,
            itemA.Id,
            itemB.Id,
            documentCount,
            chunkCount,
            embeddedChunkCount);
    }

    public (KnowledgeAssistantService Assistant, FakeLanguageModelService LanguageModel, LexicalEmbeddingService Embeddings)
        CreateAssistant()
    {
        var factory = new EmbeddingTestFixture.Factory(DbOptions);
        var authorization = new ReportAuthorizationService(Db);
        var embeddings = new LexicalEmbeddingService(factory, authorization, Options);
        var retrieval = new KnowledgeRetrievalService(factory, embeddings, authorization);
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

    private static void AddGeneratedReport(
        SchoolReportsDbContext db,
        string fileName,
        string sourceIdentifier,
        int schoolId,
        int reportId,
        string schoolCode,
        string content)
    {
        var now = DateTimeOffset.UtcNow;
        var document = new KnowledgeDocument
        {
            FileName = fileName,
            DocumentType = KnowledgeDocumentType.GeneratedReport,
            ContentHash = KnowledgeContentHash.Sha256Hex(System.Text.Encoding.UTF8.GetBytes(content)),
            SourceIdentifier = sourceIdentifier,
            IndexedAt = now,
            SchoolId = schoolId,
            SchoolCode = schoolCode,
            ReportId = reportId,
            ReportYear = 2025,
            ReportType = "Summary",
            AuthorizationScope = KnowledgeAuthorizationScope.Report,
            CreatedAt = now,
        };
        document.Chunks.Add(new KnowledgeChunk
        {
            ChunkNumber = 1,
            Content = content,
            RuleId = null,
            Category = "report",
            SourceLocation = "page 1",
            CreatedAt = now,
        });
        db.KnowledgeDocuments.Add(document);
    }
}
