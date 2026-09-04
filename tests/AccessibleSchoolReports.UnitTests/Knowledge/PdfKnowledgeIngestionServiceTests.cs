using AccessibleSchoolReports.Application.Knowledge;
using AccessibleSchoolReports.Application.Reporting;
using AccessibleSchoolReports.Domain.Entities;
using AccessibleSchoolReports.Domain.Knowledge;
using AccessibleSchoolReports.Domain.Persistence;
using AccessibleSchoolReports.Infrastructure.Knowledge;
using AccessibleSchoolReports.Infrastructure.Pdf;
using AccessibleSchoolReports.Infrastructure.Persistence;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace AccessibleSchoolReports.UnitTests.Knowledge;

public sealed class PdfKnowledgeIngestionServiceTests
{
    [Fact]
    public async Task IndexesGeneratedPdf_WithSchoolAndReportMetadata()
    {
        await using var fixture = await Fixture.CreateAsync();
        var beforeWrite = File.GetLastWriteTimeUtc(fixture.PdfPath);
        var beforeBytes = File.ReadAllBytes(fixture.PdfPath);

        var result = await fixture.IngestAsync();

        Assert.Equal(PdfKnowledgeIngestionStatus.Indexed, result.Status);
        Assert.NotNull(result.KnowledgeDocumentId);
        Assert.Equal(beforeBytes, File.ReadAllBytes(fixture.PdfPath));
        Assert.Equal(beforeWrite, File.GetLastWriteTimeUtc(fixture.PdfPath));

        var document = await fixture.Db.KnowledgeDocuments
            .Include(row => row.Chunks)
            .SingleAsync();
        Assert.Equal(KnowledgeDocumentType.GeneratedReport, document.DocumentType);
        Assert.Equal(KnowledgeAuthorizationScope.Report, document.AuthorizationScope);
        Assert.Equal(fixture.School.Id, document.SchoolId);
        Assert.Equal("10701", document.SchoolCode);
        Assert.Equal(fixture.Item.Id, document.ReportId);
        Assert.Equal(fixture.Run.Id, document.ReportRunId);
        Assert.Equal(2025, document.ReportYear);
        Assert.Equal("Summary", document.ReportType);
        Assert.Equal(fixture.Item.OutputPath, document.SourceIdentifier);
        Assert.NotEmpty(document.Chunks);
        Assert.Contains(document.Chunks, chunk => chunk.SourceLocation.StartsWith("page ", StringComparison.Ordinal));
        Assert.Contains(document.Chunks, chunk => chunk.Content.Contains("Class of 2025", StringComparison.Ordinal));
        Assert.All(document.Chunks, chunk => Assert.Null(chunk.Embedding));

        var documentType = fixture.Db.Model.FindEntityType(typeof(KnowledgeDocument));
        Assert.DoesNotContain(documentType!.GetProperties(), property => property.ClrType == typeof(byte[]));
    }

    [Fact]
    public async Task DuplicateIndexing_SkipsUnchangedHash()
    {
        await using var fixture = await Fixture.CreateAsync();
        var first = await fixture.IngestAsync();
        var second = await fixture.IngestAsync();

        Assert.Equal(PdfKnowledgeIngestionStatus.Indexed, first.Status);
        Assert.Equal(PdfKnowledgeIngestionStatus.SkippedDuplicate, second.Status);
        Assert.Equal(first.KnowledgeDocumentId, second.KnowledgeDocumentId);
        Assert.Equal(1, await fixture.Db.KnowledgeDocuments.CountAsync());
    }

    [Fact]
    public async Task ChangedPdf_ReindexesChunks()
    {
        await using var fixture = await Fixture.CreateAsync();
        var first = await fixture.IngestAsync();
        WritePdf(fixture.PdfPath, schoolName: "Changed School Name");

        var result = await fixture.IngestAsync();

        Assert.Equal(PdfKnowledgeIngestionStatus.Reindexed, result.Status);
        Assert.Equal(first.KnowledgeDocumentId, result.KnowledgeDocumentId);
        var document = await fixture.Db.KnowledgeDocuments.Include(row => row.Chunks).SingleAsync();
        Assert.Contains(document.Chunks, chunk => chunk.Content.Contains("Changed School Name", StringComparison.Ordinal));
        Assert.Equal(1, await fixture.Db.KnowledgeDocuments.CountAsync());
    }

    [Fact]
    public async Task MissingPdf_DoesNotCreateDocument()
    {
        await using var fixture = await Fixture.CreateAsync();
        File.Delete(fixture.PdfPath);

        var result = await fixture.IngestAsync();

        Assert.Equal(PdfKnowledgeIngestionStatus.MissingPdf, result.Status);
        Assert.Empty(await fixture.Db.KnowledgeDocuments.ToListAsync());
    }

    [Fact]
    public async Task InvalidPdf_DoesNotCreateDocument_OrModifyFile()
    {
        await using var fixture = await Fixture.CreateAsync();
        File.WriteAllText(fixture.PdfPath, "this is not a pdf");
        var before = File.ReadAllBytes(fixture.PdfPath);

        var result = await fixture.IngestAsync();

        Assert.Equal(PdfKnowledgeIngestionStatus.InvalidPdf, result.Status);
        Assert.Equal(before, File.ReadAllBytes(fixture.PdfPath));
        Assert.Empty(await fixture.Db.KnowledgeDocuments.ToListAsync());
    }

    [Fact]
    public async Task ExtractionFailure_DoesNotCreateDocument()
    {
        await using var fixture = await Fixture.CreateAsync(new FailingExtractor());

        var result = await fixture.IngestAsync();

        Assert.Equal(PdfKnowledgeIngestionStatus.ExtractionFailed, result.Status);
        Assert.Empty(await fixture.Db.KnowledgeDocuments.ToListAsync());
        Assert.True(File.Exists(fixture.PdfPath));
    }

    [Fact]
    public async Task AuthorizationMetadata_KeepsSchoolBHiddenFromSchoolA()
    {
        await using var fixture = await Fixture.CreateAsync();
        await fixture.IngestAsync();
        var schoolB = await fixture.AddSchoolReportAsync("23306", "School B Report");
        await fixture.IngestAsync(schoolB.Item.Id, schoolB.School.Id, "23306");

        var documents = fixture.Db.KnowledgeDocuments.AsQueryable();
        var schoolA = await documents
            .WhereAccessible(true, false, new HashSet<int> { fixture.School.Id })
            .Select(row => row.SchoolCode)
            .ToListAsync();
        var schoolBOnly = await documents
            .WhereAccessible(true, false, new HashSet<int> { schoolB.School.Id })
            .Select(row => row.SchoolCode)
            .ToListAsync();

        Assert.Equal(["10701"], schoolA);
        Assert.Equal(["23306"], schoolBOnly);
        Assert.False(KnowledgeAccess.IsAccessible(
            await documents.SingleAsync(row => row.SchoolCode == "23306"),
            true,
            false,
            new HashSet<int> { fixture.School.Id }));
    }

    [Fact]
    public async Task SchoolAndReportAssociation_MatchStoredRunItem()
    {
        await using var fixture = await Fixture.CreateAsync();
        await fixture.IngestAsync();

        var document = await fixture.Db.KnowledgeDocuments
            .Include(row => row.School)
            .Include(row => row.Report)
            .Include(row => row.ReportRun)
            .SingleAsync();

        Assert.Equal(fixture.School.Id, document.School!.Id);
        Assert.Equal(fixture.Item.Id, document.Report!.Id);
        Assert.Equal(fixture.Run.Id, document.ReportRun!.Id);
        Assert.Equal(fixture.Item.SchoolId, document.Report.SchoolId);
    }

    private static void WritePdf(string path, string schoolName)
    {
        Directory.CreateDirectory(Path.GetDirectoryName(path)!);
        var report = new SchoolReport
        {
            SchoolCode = "10701",
            SchoolName = schoolName,
            Rows = [new() { Analvar = "A", Newvar = "A", Count = 5, Percent = 100m }],
            Sections = [new() { Analvar = "A", Details = [], SubtotalCount = 5, SubtotalPercent = 100m }],
        };
        using var stream = File.Create(path);
        new QuestPdfAccessiblePdfGenerator().Generate(report, stream);
    }

    private sealed class FailingExtractor : IPdfTextExtractor
    {
        public PdfTextExtractionResult Extract(Stream pdf) =>
            PdfTextExtractionResult.Failed("extractor exploded");
    }

    private sealed class Fixture : IAsyncDisposable
    {
        private readonly string _directory;
        private readonly IPdfTextExtractor _extractor;
        private readonly DbContextOptions<SchoolReportsDbContext> _dbOptions;

        public string OutputRoot { get; }
        public string PdfPath { get; }
        public School School { get; }
        public ReportRun Run { get; }
        public ReportRunItem Item { get; }
        public SchoolReportsDbContext Db { get; }

        private Fixture(
            string directory,
            string outputRoot,
            string pdfPath,
            School school,
            ReportRun run,
            ReportRunItem item,
            SchoolReportsDbContext db,
            DbContextOptions<SchoolReportsDbContext> dbOptions,
            IPdfTextExtractor extractor)
        {
            _directory = directory;
            OutputRoot = outputRoot;
            PdfPath = pdfPath;
            School = school;
            Run = run;
            Item = item;
            Db = db;
            _dbOptions = dbOptions;
            _extractor = extractor;
        }

        public Task<PdfKnowledgeIngestionResult> IngestAsync() =>
            IngestAsync(Item.Id, School.Id, School.Code);

        public async Task<PdfKnowledgeIngestionResult> IngestAsync(int itemId, int schoolId, string schoolCode)
        {
            var options = Options.Create(new ReportGenerationOptions { OutputRoot = OutputRoot, ClassYear = "2025" });
            var ingestion = new PdfKnowledgeIngestionService(new Factory(_dbOptions), _extractor, options);
            var result = await ingestion.IndexGeneratedReportAsync(new GeneratedPdfKnowledgeRequest
            {
                ReportRunItemId = itemId,
                ReportRunId = Run.Id,
                SchoolId = schoolId,
                SchoolCode = schoolCode,
                OutputPath = PdfPath,
                ReportYear = 2025,
                ReportType = "Summary",
            });
            Db.ChangeTracker.Clear();
            return result;
        }

        public async Task<(School School, ReportRunItem Item)> AddSchoolReportAsync(string code, string schoolName)
        {
            var school = new School { Code = code, Name = schoolName };
            Db.Schools.Add(school);
            await Db.SaveChangesAsync();
            var path = Path.Combine(OutputRoot, "2025", code, "summary-report.pdf");
            WritePdf(path, schoolName);
            var item = new ReportRunItem
            {
                ReportRunId = Run.Id,
                SchoolId = school.Id,
                Status = RunStatus.Completed,
                OutputPath = path,
            };
            Db.ReportRunItems.Add(item);
            await Db.SaveChangesAsync();
            return (school, item);
        }

        public static async Task<Fixture> CreateAsync(IPdfTextExtractor? extractor = null)
        {
            var directory = Path.Combine(Path.GetTempPath(), "asr-pdf-knowledge", Guid.NewGuid().ToString("N"));
            var outputRoot = Path.Combine(directory, "output");
            var pdfPath = Path.Combine(outputRoot, "2025", "10701", "summary-report.pdf");
            Directory.CreateDirectory(directory);
            WritePdf(pdfPath, "School A Report");

            var connectionString = new SqliteConnectionStringBuilder
            {
                DataSource = Path.Combine(directory, "schoolreports.db"),
                Mode = SqliteOpenMode.ReadWriteCreate,
                Pooling = false,
            }.ToString();
            var options = new DbContextOptionsBuilder<SchoolReportsDbContext>().UseSqlite(connectionString).Options;
            var db = new SchoolReportsDbContext(options);
            await db.MigrateAsync();

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
            var item = new ReportRunItem
            {
                ReportRunId = run.Id,
                SchoolId = school.Id,
                Status = RunStatus.Completed,
                OutputPath = pdfPath,
            };
            db.ReportRunItems.Add(item);
            await db.SaveChangesAsync();

            return new Fixture(
                directory,
                outputRoot,
                pdfPath,
                school,
                run,
                item,
                db,
                options,
                extractor ?? new PdfPigTextExtractor());
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

        private sealed class Factory : IDbContextFactory<SchoolReportsDbContext>
        {
            private readonly DbContextOptions<SchoolReportsDbContext> _options;

            public Factory(DbContextOptions<SchoolReportsDbContext> options) => _options = options;

            public SchoolReportsDbContext CreateDbContext() => new(_options);

            public Task<SchoolReportsDbContext> CreateDbContextAsync(CancellationToken cancellationToken = default) =>
                Task.FromResult(CreateDbContext());
        }
    }
}
