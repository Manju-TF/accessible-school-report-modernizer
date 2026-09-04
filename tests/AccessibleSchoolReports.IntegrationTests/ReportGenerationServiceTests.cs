using System.Security.Claims;
using AccessibleSchoolReports.Application.Knowledge;
using AccessibleSchoolReports.Application.Reporting;
using AccessibleSchoolReports.Application.Security;
using AccessibleSchoolReports.Domain.Entities;
using AccessibleSchoolReports.Domain.Knowledge;
using AccessibleSchoolReports.Domain.Persistence;
using AccessibleSchoolReports.Infrastructure.Knowledge;
using AccessibleSchoolReports.Infrastructure.Pdf;
using AccessibleSchoolReports.Infrastructure.Reporting;
using AccessibleSchoolReports.Infrastructure.Security;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace AccessibleSchoolReports.IntegrationTests;

public sealed class ReportGenerationServiceTests
{
    [Fact]
    public async Task GenerateSchoolReport_WritesPdfAndPersistsRunMetadata()
    {
        await using var db = await SqliteTestDatabase.CreateAsync();
        await using var output = OutputFolder.Create();
        var school = await SeedSchoolAsync(db, "10701", "Sample Law School", graduates: 3);
        var service = CreateService(db, output.Path);

        var result = await service.GenerateSchoolReportAsync(school.Id);

        var expectedPath = Path.Combine(output.Path, "2025", "10701", "summary-report.pdf");
        Assert.Equal(RunStatus.Completed, result.Status);
        Assert.Equal("10701", result.SchoolCode);
        Assert.Equal("Sample Law School", result.SchoolName);
        Assert.Equal(3, result.GraduateCount);
        Assert.Equal(expectedPath, result.OutputPath);
        Assert.True(result.DurationMilliseconds >= 0);
        Assert.True(result.CompletedUtc >= result.StartedUtc);
        Assert.True(File.Exists(expectedPath));
        Assert.Equal("%PDF", System.Text.Encoding.ASCII.GetString(await File.ReadAllBytesAsync(expectedPath), 0, 4));

        var run = await db.Context.ReportRuns.Include(item => item.Items).SingleAsync();
        Assert.Equal(ReportGenerationMode.Single, run.Mode);
        Assert.Equal(RunStatus.Completed, run.Status);
        Assert.Equal(Path.Combine(output.Path, "2025"), run.OutputDirectory);
        Assert.Contains("Duration", run.Message, StringComparison.Ordinal);
        var item = Assert.Single(run.Items);
        Assert.Equal(school.Id, item.SchoolId);
        Assert.Equal(RunStatus.Completed, item.Status);
        Assert.Equal(expectedPath, item.OutputPath);
        Assert.NotNull(item.StartedUtc);
        Assert.NotNull(item.CompletedUtc);

        var knowledge = await db.Context.KnowledgeDocuments.Include(row => row.Chunks).SingleAsync();
        Assert.Equal(item.Id, knowledge.ReportId);
        Assert.Equal(school.Id, knowledge.SchoolId);
        Assert.Equal("10701", knowledge.SchoolCode);
        Assert.Equal(KnowledgeAuthorizationScope.Report, knowledge.AuthorizationScope);
        Assert.NotEmpty(knowledge.Chunks);
    }

    [Fact]
    public async Task GenerateSchoolReport_UnknownSchool_DoesNotThrow_AndPersistsFailedRun()
    {
        await using var db = await SqliteTestDatabase.CreateAsync();
        await using var output = OutputFolder.Create();
        var service = CreateService(db, output.Path);

        var result = await service.GenerateSchoolReportAsync(4242);

        Assert.Equal(RunStatus.Failed, result.Status);
        Assert.Null(result.ReportRunItemId);
        Assert.Contains("not found", result.Message, StringComparison.OrdinalIgnoreCase);
        var run = await db.Context.ReportRuns.Include(item => item.Items).SingleAsync();
        Assert.Equal(RunStatus.Failed, run.Status);
        Assert.Empty(run.Items);
        Assert.False(Directory.Exists(Path.Combine(output.Path, "2025", "4242")));
    }

    [Fact]
    public async Task GenerateSchoolReport_NoGraduates_DoesNotThrow_AndPersistsFailedItem()
    {
        await using var db = await SqliteTestDatabase.CreateAsync();
        await using var output = OutputFolder.Create();
        var school = await SeedSchoolAsync(db, "50904", graduates: 0);
        var service = CreateService(db, output.Path);

        var result = await service.GenerateSchoolReportAsync(school.Id);

        Assert.Equal(RunStatus.Failed, result.Status);
        Assert.Contains("No graduate records", result.Message, StringComparison.Ordinal);
        Assert.False(File.Exists(Path.Combine(output.Path, "2025", "50904", "summary-report.pdf")));
        var item = await db.Context.ReportRunItems.SingleAsync();
        Assert.Equal(RunStatus.Failed, item.Status);
        Assert.Null(item.OutputPath);
    }

    [Fact]
    public async Task GenerateSchoolReport_PdfFailure_DoesNotThrow_AndLeavesNoPartialFile()
    {
        await using var db = await SqliteTestDatabase.CreateAsync();
        await using var output = OutputFolder.Create();
        var school = await SeedSchoolAsync(db, "23306", graduates: 1);
        var service = CreateService(db, output.Path, new FailingPdfGenerator());

        var result = await service.GenerateSchoolReportAsync(school.Id);

        Assert.Equal(RunStatus.Failed, result.Status);
        Assert.Contains("boom", result.Message, StringComparison.OrdinalIgnoreCase);
        Assert.False(File.Exists(Path.Combine(output.Path, "2025", "23306", "summary-report.pdf")));
        var run = await db.Context.ReportRuns.Include(item => item.Items).SingleAsync();
        Assert.Equal(RunStatus.Failed, run.Status);
        Assert.Equal(RunStatus.Failed, Assert.Single(run.Items).Status);
    }

    [Fact]
    public async Task GenerateSchoolReport_Cancelled_DoesNotThrow_AndPersistsCancelled()
    {
        await using var db = await SqliteTestDatabase.CreateAsync();
        await using var output = OutputFolder.Create();
        var school = await SeedSchoolAsync(db, "12203", graduates: 1);
        var service = CreateService(db, output.Path);
        using var cts = new CancellationTokenSource();
        await cts.CancelAsync();

        var result = await service.GenerateSchoolReportAsync(school.Id, cts.Token);

        Assert.Equal(RunStatus.Cancelled, result.Status);
        Assert.Contains("Cancelled", result.Message, StringComparison.Ordinal);
        var run = await db.Context.ReportRuns.SingleAsync();
        Assert.Equal(RunStatus.Cancelled, run.Status);
        Assert.False(File.Exists(Path.Combine(output.Path, "2025", "12203", "summary-report.pdf")));
    }

    [Fact]
    public async Task GenerateAllSequential_WritesEachEligibleSchool_AndPersistsTotals()
    {
        await using var db = await SqliteTestDatabase.CreateAsync();
        await using var output = OutputFolder.Create();
        await SeedSchoolAsync(db, "50904", "Later School", graduates: 1);
        await SeedSchoolAsync(db, "10701", "First School", graduates: 2);
        await SeedSchoolAsync(db, "12203", graduates: 0);
        var service = CreateService(db, output.Path);

        var result = await service.GenerateAllSequentialAsync();

        Assert.Equal(RunStatus.Completed, result.Status);
        Assert.Equal(2, result.Total);
        Assert.Equal(2, result.Successful);
        Assert.Equal(0, result.Failed);
        Assert.True(result.DurationMilliseconds >= 0);
        Assert.Equal(new[] { "10701", "50904" }, result.Items.Select(item => item.SchoolCode));
        Assert.True(File.Exists(Path.Combine(output.Path, "2025", "10701", "summary-report.pdf")));
        Assert.True(File.Exists(Path.Combine(output.Path, "2025", "50904", "summary-report.pdf")));
        Assert.False(Directory.Exists(Path.Combine(output.Path, "2025", "12203")));

        var run = await db.Context.ReportRuns.Include(item => item.Items).SingleAsync();
        Assert.Equal(ReportGenerationMode.Sequential, run.Mode);
        Assert.Equal(1, run.MaxParallelism);
        Assert.Equal(2, run.TotalCount);
        Assert.Equal(2, run.SuccessfulCount);
        Assert.Equal(0, run.FailedCount);
        Assert.True(run.DurationMilliseconds >= 0);
        Assert.Equal(2, run.Items.Count);
        Assert.All(run.Items, item => Assert.Equal(RunStatus.Completed, item.Status));
        Assert.All(run.Items, item => Assert.NotNull(item.CompletedUtc));
    }

    [Fact]
    public async Task GenerateAllSequential_ContinuesAfterIndividualFailure()
    {
        await using var db = await SqliteTestDatabase.CreateAsync();
        await using var output = OutputFolder.Create();
        await SeedSchoolAsync(db, "10701", graduates: 1);
        await SeedSchoolAsync(db, "23306", graduates: 1);
        await SeedSchoolAsync(db, "50904", graduates: 1);
        var service = CreateService(db, output.Path, new SelectiveFailingPdfGenerator("23306"));

        var result = await service.GenerateAllSequentialAsync();

        Assert.Equal(RunStatus.CompletedWithErrors, result.Status);
        Assert.Equal(3, result.Total);
        Assert.Equal(2, result.Successful);
        Assert.Equal(1, result.Failed);
        Assert.Equal(new[] { "10701", "23306", "50904" }, result.Items.Select(item => item.SchoolCode));
        Assert.Equal(RunStatus.Failed, result.Items.Single(item => item.SchoolCode == "23306").Status);
        Assert.True(File.Exists(Path.Combine(output.Path, "2025", "10701", "summary-report.pdf")));
        Assert.True(File.Exists(Path.Combine(output.Path, "2025", "50904", "summary-report.pdf")));
        Assert.False(File.Exists(Path.Combine(output.Path, "2025", "23306", "summary-report.pdf")));

        var run = await db.Context.ReportRuns.Include(item => item.Items).SingleAsync();
        Assert.Equal(ReportGenerationMode.Sequential, run.Mode);
        Assert.Equal(3, run.TotalCount);
        Assert.Equal(2, run.SuccessfulCount);
        Assert.Equal(1, run.FailedCount);
        Assert.True(run.DurationMilliseconds >= 0);
        Assert.Equal(RunStatus.CompletedWithErrors, run.Status);
    }

    [Fact]
    public async Task GenerateAllSequential_NoEligibleSchools_PersistsZeroTotals()
    {
        await using var db = await SqliteTestDatabase.CreateAsync();
        await using var output = OutputFolder.Create();
        await SeedSchoolAsync(db, "12203", graduates: 0);
        var service = CreateService(db, output.Path);

        var result = await service.GenerateAllSequentialAsync();

        Assert.Equal(RunStatus.Completed, result.Status);
        Assert.Equal(0, result.Total);
        Assert.Equal(0, result.Successful);
        Assert.Equal(0, result.Failed);
        Assert.Empty(result.Items);
        var run = await db.Context.ReportRuns.Include(item => item.Items).SingleAsync();
        Assert.Equal(0, run.TotalCount);
        Assert.Empty(run.Items);
    }

    [Fact]
    public async Task GenerateAllSequential_ReportsProgressAfterEachSchool()
    {
        await using var db = await SqliteTestDatabase.CreateAsync();
        await using var output = OutputFolder.Create();
        await SeedSchoolAsync(db, "10701", graduates: 1);
        await SeedSchoolAsync(db, "50904", graduates: 1);
        var service = CreateService(db, output.Path);
        var reports = new List<SchoolGenerationProgress>();

        var result = await service.GenerateAllSequentialAsync(
            progress: new Progress<SchoolGenerationProgress>(reports.Add));

        Assert.Equal(RunStatus.Completed, result.Status);
        Assert.Equal(2, result.Successful);
        Assert.Contains(reports, item => item.Total == 2 && item.Completed == 0);
        Assert.Contains(reports, item => item.Total == 2 && item.Completed == 1 && item.Successful == 1);
        Assert.Contains(reports, item => item.Total == 2 && item.Completed == 2 && item.Successful == 2);
    }

    [Fact]
    public async Task GenerateAllSequential_CancelledAfterFirstSchool_StopsAndPersistsCounts()
    {
        await using var db = await SqliteTestDatabase.CreateAsync();
        await using var output = OutputFolder.Create();
        await SeedSchoolAsync(db, "10701", graduates: 1);
        await SeedSchoolAsync(db, "50904", graduates: 1);
        using var cts = new CancellationTokenSource();
        var service = CreateService(db, output.Path, new CancelAfterFirstPdf(cts));

        var result = await service.GenerateAllSequentialAsync(cts.Token);

        Assert.Equal(RunStatus.Cancelled, result.Status);
        Assert.Equal(2, result.Total);
        Assert.Equal(1, result.Successful);
        Assert.Equal(0, result.Failed);
        Assert.Single(result.Items);
        Assert.True(File.Exists(Path.Combine(output.Path, "2025", "10701", "summary-report.pdf")));
        Assert.False(File.Exists(Path.Combine(output.Path, "2025", "50904", "summary-report.pdf")));
        var run = await db.Context.ReportRuns.Include(item => item.Items).SingleAsync();
        Assert.Equal(RunStatus.Cancelled, run.Status);
        Assert.Equal(2, run.TotalCount);
        Assert.Equal(1, run.SuccessfulCount);
        Assert.Equal(0, run.FailedCount);
        Assert.Single(run.Items);
    }

    [Fact]
    public async Task GenerateAllParallel_WritesEachEligibleSchool_AndPersistsTotals()
    {
        await using var db = await SqliteTestDatabase.CreateAsync();
        await using var output = OutputFolder.Create();
        await SeedSchoolAsync(db, "50904", "Later School", graduates: 1);
        await SeedSchoolAsync(db, "10701", "First School", graduates: 2);
        await SeedSchoolAsync(db, "12203", graduates: 0);
        var service = CreateService(db, output.Path);

        var result = await service.GenerateAllParallelAsync();

        Assert.Equal(RunStatus.Completed, result.Status);
        Assert.Equal(2, result.Total);
        Assert.Equal(2, result.Successful);
        Assert.Equal(0, result.Failed);
        Assert.True(result.DurationMilliseconds >= 0);
        Assert.Equal(new[] { "10701", "50904" }, result.Items.Select(item => item.SchoolCode));
        Assert.True(File.Exists(Path.Combine(output.Path, "2025", "10701", "summary-report.pdf")));
        Assert.True(File.Exists(Path.Combine(output.Path, "2025", "50904", "summary-report.pdf")));
        Assert.False(Directory.Exists(Path.Combine(output.Path, "2025", "12203")));

        var run = await ReloadRunAsync(db, result.ReportRunId);
        Assert.Equal(ReportGenerationMode.BoundedParallel, run.Mode);
        Assert.Equal(ReportGenerationOptions.DefaultMaxParallelism, run.MaxParallelism);
        Assert.Equal(2, run.TotalCount);
        Assert.Equal(2, run.SuccessfulCount);
        Assert.Equal(0, run.FailedCount);
        Assert.True(run.DurationMilliseconds >= 0);
        Assert.Equal(2, run.Items.Count);
        Assert.All(run.Items, item => Assert.Equal(RunStatus.Completed, item.Status));
        Assert.All(run.Items, item => Assert.NotNull(item.CompletedUtc));
    }

    [Fact]
    public async Task GenerateAllParallel_ContinuesAfterIndividualFailure()
    {
        await using var db = await SqliteTestDatabase.CreateAsync();
        await using var output = OutputFolder.Create();
        await SeedSchoolAsync(db, "10701", graduates: 1);
        await SeedSchoolAsync(db, "23306", graduates: 1);
        await SeedSchoolAsync(db, "50904", graduates: 1);
        var service = CreateService(db, output.Path, new SelectiveFailingPdfGenerator("23306"));

        var result = await service.GenerateAllParallelAsync();

        Assert.Equal(RunStatus.CompletedWithErrors, result.Status);
        Assert.Equal(3, result.Total);
        Assert.Equal(2, result.Successful);
        Assert.Equal(1, result.Failed);
        Assert.Equal(new[] { "10701", "23306", "50904" }, result.Items.Select(item => item.SchoolCode));
        Assert.Equal(RunStatus.Failed, result.Items.Single(item => item.SchoolCode == "23306").Status);
        Assert.True(File.Exists(Path.Combine(output.Path, "2025", "10701", "summary-report.pdf")));
        Assert.True(File.Exists(Path.Combine(output.Path, "2025", "50904", "summary-report.pdf")));
        Assert.False(File.Exists(Path.Combine(output.Path, "2025", "23306", "summary-report.pdf")));

        var run = await ReloadRunAsync(db, result.ReportRunId);
        Assert.Equal(ReportGenerationMode.BoundedParallel, run.Mode);
        Assert.Equal(3, run.TotalCount);
        Assert.Equal(2, run.SuccessfulCount);
        Assert.Equal(1, run.FailedCount);
        Assert.True(run.DurationMilliseconds >= 0);
        Assert.Equal(RunStatus.CompletedWithErrors, run.Status);
    }

    [Fact]
    public async Task GenerateAllParallel_ClampsMaxDegreeOfParallelism()
    {
        await using var db = await SqliteTestDatabase.CreateAsync();
        await using var output = OutputFolder.Create();
        await SeedSchoolAsync(db, "10701", graduates: 1);
        var service = CreateService(db, output.Path);

        var low = await service.GenerateAllParallelAsync(0);
        var high = await service.GenerateAllParallelAsync(99);

        Assert.Equal(RunStatus.Completed, low.Status);
        Assert.Equal(RunStatus.Completed, high.Status);
        var lowRun = await ReloadRunAsync(db, low.ReportRunId);
        var highRun = await ReloadRunAsync(db, high.ReportRunId);
        Assert.Equal(ReportGenerationOptions.MinMaxParallelism, lowRun.MaxParallelism);
        Assert.Equal(ReportGenerationOptions.MaxMaxParallelism, highRun.MaxParallelism);
        Assert.Equal(ReportGenerationMode.BoundedParallel, lowRun.Mode);
        Assert.Equal(ReportGenerationMode.BoundedParallel, highRun.Mode);
    }

    [Fact]
    public async Task GenerateAllParallel_Cancelled_DoesNotThrow_AndPersistsCancelled()
    {
        await using var db = await SqliteTestDatabase.CreateAsync();
        await using var output = OutputFolder.Create();
        await SeedSchoolAsync(db, "10701", graduates: 1);
        await SeedSchoolAsync(db, "50904", graduates: 1);
        var service = CreateService(db, output.Path);
        using var cts = new CancellationTokenSource();
        await cts.CancelAsync();

        var result = await service.GenerateAllParallelAsync(cancellationToken: cts.Token);

        Assert.Equal(RunStatus.Cancelled, result.Status);
        Assert.Contains("Cancelled", result.Message, StringComparison.Ordinal);
        Assert.Equal(0, result.Failed);
        var run = await ReloadRunAsync(db, result.ReportRunId);
        Assert.Equal(RunStatus.Cancelled, run.Status);
        Assert.Equal(ReportGenerationMode.BoundedParallel, run.Mode);
        Assert.Equal(0, run.FailedCount);
    }

    [Fact]
    public async Task GenerateAllSequential_AndParallel_ProduceEquivalentResults()
    {
        await using var db = await SqliteTestDatabase.CreateAsync();
        await using var sequentialOutput = OutputFolder.Create();
        await using var parallelOutput = OutputFolder.Create();
        await SeedSchoolAsync(db, "50904", "Later School", graduates: 3);
        await SeedSchoolAsync(db, "10701", "First School", graduates: 2);
        await SeedSchoolAsync(db, "23306", "Failing School", graduates: 1);
        await SeedSchoolAsync(db, "12203", graduates: 0);
        var pdf = new SelectiveFailingPdfGenerator("23306");
        var sequential = CreateService(db, sequentialOutput.Path, pdf);
        var parallel = CreateService(db, parallelOutput.Path, pdf);

        var sequentialResult = await sequential.GenerateAllSequentialAsync();
        var parallelResult = await parallel.GenerateAllParallelAsync(4);

        Assert.Equal(sequentialResult.Status, parallelResult.Status);
        Assert.Equal(sequentialResult.Total, parallelResult.Total);
        Assert.Equal(sequentialResult.Successful, parallelResult.Successful);
        Assert.Equal(sequentialResult.Failed, parallelResult.Failed);
        Assert.Equal(
            sequentialResult.Items.Select(item => item.SchoolCode),
            parallelResult.Items.Select(item => item.SchoolCode));

        foreach (var (expected, actual) in sequentialResult.Items.Zip(parallelResult.Items))
        {
            Assert.Equal(expected.SchoolCode, actual.SchoolCode);
            Assert.Equal(expected.SchoolName, actual.SchoolName);
            Assert.Equal(expected.SchoolId, actual.SchoolId);
            Assert.Equal(expected.Status, actual.Status);
            Assert.Equal(expected.GraduateCount, actual.GraduateCount);
            Assert.Equal(RelativeReportPath(expected.SchoolCode!), RelativeReportPath(actual.SchoolCode!));
            if (expected.Status == RunStatus.Completed)
            {
                Assert.Equal(ExtractPdfText(expected.OutputPath!), ExtractPdfText(actual.OutputPath!));
            }
            else
            {
                Assert.Null(expected.OutputPath);
                Assert.Null(actual.OutputPath);
            }
        }

        var sequentialRun = await ReloadRunAsync(db, sequentialResult.ReportRunId);
        var parallelRun = await ReloadRunAsync(db, parallelResult.ReportRunId);
        Assert.Equal(ReportGenerationMode.Sequential, sequentialRun.Mode);
        Assert.Equal(ReportGenerationMode.BoundedParallel, parallelRun.Mode);
        Assert.Equal(sequentialRun.TotalCount, parallelRun.TotalCount);
        Assert.Equal(sequentialRun.SuccessfulCount, parallelRun.SuccessfulCount);
        Assert.Equal(sequentialRun.FailedCount, parallelRun.FailedCount);
        Assert.Equal(
            sequentialRun.Items.OrderBy(item => item.SchoolId).Select(item => item.Status),
            parallelRun.Items.OrderBy(item => item.SchoolId).Select(item => item.Status));
    }

    private static IReportGenerationService CreateService(
        SqliteTestDatabase db,
        string outputRoot,
        IAccessiblePdfGenerator? pdf = null)
    {
        var options = Options.Create(new ReportGenerationOptions { OutputRoot = outputRoot });
        return new ReportGenerationService(
            db.Context,
            db.CreateFactory(),
            new SchoolReportCalculator(),
            pdf ?? new QuestPdfAccessiblePdfGenerator(),
            new ReportAuthorizationService(db.Context),
            new PdfKnowledgeIngestionService(
                db.CreateFactory(),
                new PdfPigTextExtractor(),
                options),
            new NoopKnowledgeEmbeddingIndex(),
            new StaticCurrentUserAccessor(AdminPrincipal()),
            options);
    }

    private static ClaimsPrincipal AdminPrincipal() =>
        new(new ClaimsIdentity(
            [
                new Claim(ClaimTypes.NameIdentifier, "integration-admin"),
                new Claim(ClaimTypes.Name, "integration-admin"),
                new Claim(ClaimTypes.Role, AppRoles.Admin),
            ],
            "test"));

    private sealed class StaticCurrentUserAccessor : ICurrentUserAccessor
    {
        public StaticCurrentUserAccessor(ClaimsPrincipal user) => User = user;

        public ClaimsPrincipal User { get; }
    }

    private sealed class NoopKnowledgeEmbeddingIndex : IKnowledgeEmbeddingIndexService
    {
        public Task<KnowledgeIndexResult> IndexPendingEmbeddingsAsync(
            ClaimsPrincipal user,
            CancellationToken cancellationToken = default) =>
            Task.FromResult(new KnowledgeIndexResult());
    }

    private static async Task<School> SeedSchoolAsync(
        SqliteTestDatabase db,
        string code,
        string? name = null,
        int graduates = 1)
    {
        var import = new ImportRun
        {
            FileName = "seed.xlsx",
            StartedUtc = DateTimeOffset.UtcNow,
            Status = RunStatus.Completed,
            ImportedRowCount = graduates,
        };
        var school = new School { Code = code, Name = name };
        db.Context.ImportRuns.Add(import);
        db.Context.Schools.Add(school);
        await db.Context.SaveChangesAsync();

        for (var index = 0; index < graduates; index++)
        {
            db.Context.GraduateRecords.Add(new GraduateRecord
            {
                ImportRunId = import.Id,
                SchoolId = school.Id,
                Sex3 = index % 2 == 0 ? "F" : "M",
                Minstat = "NONMIN",
                Jobcat1 = "LJD",
                JobFtPt = "FULL",
                Empgen = "FIRM",
                Firm1 = "1",
                Lfjob = "ATTY",
                Jobreg = "1",
                LocationFlag = "INSTATE",
                Jobst = "107",
                Source = "JOBPST",
                Time1 = "BGRAD",
                Status = "SET",
                Duration = "PERM",
                SchoolFund = "NO",
                SalFtPerm = 80000 + (index * 1000),
            });
        }

        await db.Context.SaveChangesAsync();
        return school;
    }

    private static async Task<ReportRun> ReloadRunAsync(SqliteTestDatabase db, int reportRunId)
    {
        db.Context.ChangeTracker.Clear();
        return await db.Context.ReportRuns
            .AsNoTracking()
            .Include(run => run.Items)
            .SingleAsync(run => run.Id == reportRunId);
    }

    private static string RelativeReportPath(string schoolCode) =>
        Path.Combine("2025", schoolCode, "summary-report.pdf");

    private static string ExtractPdfText(string path)
    {
        using var document = UglyToad.PdfPig.PdfDocument.Open(path);
        return string.Join('\n', document.GetPages().Select(page => page.Text));
    }

    private sealed class FailingPdfGenerator : IAccessiblePdfGenerator
    {
        public void Generate(SchoolReport report, Stream output) => throw new InvalidOperationException("boom");

        public byte[] Generate(SchoolReport report) => throw new InvalidOperationException("boom");
    }

    private sealed class SelectiveFailingPdfGenerator : IAccessiblePdfGenerator
    {
        private readonly string _failingSchoolCode;
        private readonly IAccessiblePdfGenerator _inner = new QuestPdfAccessiblePdfGenerator();

        public SelectiveFailingPdfGenerator(string failingSchoolCode) => _failingSchoolCode = failingSchoolCode;

        public void Generate(SchoolReport report, Stream output)
        {
            if (report.SchoolCode == _failingSchoolCode)
            {
                throw new InvalidOperationException("boom");
            }

            _inner.Generate(report, output);
        }

        public byte[] Generate(SchoolReport report)
        {
            using var stream = new MemoryStream();
            Generate(report, stream);
            return stream.ToArray();
        }
    }

    private sealed class CancelAfterFirstPdf : IAccessiblePdfGenerator
    {
        private readonly CancellationTokenSource _cts;
        private readonly IAccessiblePdfGenerator _inner = new QuestPdfAccessiblePdfGenerator();

        public CancelAfterFirstPdf(CancellationTokenSource cts) => _cts = cts;

        public void Generate(SchoolReport report, Stream output)
        {
            _inner.Generate(report, output);
            _cts.Cancel();
        }

        public byte[] Generate(SchoolReport report)
        {
            using var stream = new MemoryStream();
            Generate(report, stream);
            return stream.ToArray();
        }
    }

    private sealed class OutputFolder : IAsyncDisposable
    {
        public string Path { get; }

        private OutputFolder(string path) => Path = path;

        public static OutputFolder Create()
        {
            var path = System.IO.Path.Combine(System.IO.Path.GetTempPath(), "asr-report-output", Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(path);
            return new OutputFolder(path);
        }

        public ValueTask DisposeAsync()
        {
            if (Directory.Exists(Path))
            {
                Directory.Delete(Path, recursive: true);
            }

            return ValueTask.CompletedTask;
        }
    }
}
