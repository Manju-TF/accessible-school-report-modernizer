using AccessibleSchoolReports.Application.Knowledge;
using AccessibleSchoolReports.Domain.Entities;
using AccessibleSchoolReports.Domain.Knowledge;
using AccessibleSchoolReports.Domain.Persistence;
using AccessibleSchoolReports.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace AccessibleSchoolReports.IntegrationTests;

public sealed class KnowledgePersistenceTests
{
    [Fact]
    public async Task CanPersistLegacyDocument_AndChunks_WithoutPdfBytes()
    {
        await using var db = await SqliteTestDatabase.CreateAsync();
        var context = db.Context;
        var created = DateTimeOffset.UtcNow;

        var document = new KnowledgeDocument
        {
            FileName = "legacy-baseline-notes.md",
            DocumentType = KnowledgeDocumentType.Legacy,
            ContentHash = new string('a', 64),
            SourceIdentifier = "legacy/baseline/test-school-report.pdf",
            IndexedAt = created,
            AuthorizationScope = KnowledgeAuthorizationScope.Authenticated,
            CreatedAt = created,
        };
        document.Chunks.Add(new KnowledgeChunk
        {
            ChunkNumber = 1,
            Content = "Salary cells are suppressed when n is below the characterized threshold.",
            RuleId = "CF-S-00",
            Category = "suppression",
            SourceLocation = "page 1, notes",
            CreatedAt = created,
        });
        context.KnowledgeDocuments.Add(document);
        await context.SaveChangesAsync();

        var stored = await context.KnowledgeDocuments
            .Include(row => row.Chunks)
            .SingleAsync();

        Assert.Equal(KnowledgeDocumentType.Legacy, stored.DocumentType);
        Assert.Equal(KnowledgeAuthorizationScope.Authenticated, stored.AuthorizationScope);
        Assert.Null(stored.SchoolId);
        Assert.Null(stored.ReportId);
        Assert.Equal("legacy/baseline/test-school-report.pdf", stored.SourceIdentifier);
        Assert.Single(stored.Chunks);
        Assert.Equal("CF-S-00", stored.Chunks.Single().RuleId);
        Assert.Null(stored.Chunks.Single().Embedding);
        Assert.Null(stored.Chunks.Single().EmbeddingModel);

        var documentType = context.Model.FindEntityType(typeof(KnowledgeDocument));
        Assert.NotNull(documentType);
        Assert.DoesNotContain(documentType.GetProperties(), property => property.ClrType == typeof(byte[]));
        Assert.Null(documentType.FindProperty("Pdf"));
        Assert.Null(documentType.FindProperty("Content"));
        Assert.NotNull(documentType.FindProperty(nameof(KnowledgeDocument.SourceIdentifier)));
    }

    [Fact]
    public async Task GeneratedReportDocument_StoresSchoolAndReportReference_NotPdfBytes()
    {
        await using var db = await SqliteTestDatabase.CreateAsync();
        var context = db.Context;
        var (school, item) = await SeedReportAsync(context);
        var created = DateTimeOffset.UtcNow;

        context.KnowledgeDocuments.Add(new KnowledgeDocument
        {
            FileName = "10701-summary-report.pdf",
            DocumentType = KnowledgeDocumentType.GeneratedReport,
            ContentHash = new string('b', 64),
            SourceIdentifier = item.OutputPath!,
            IndexedAt = created,
            SchoolId = school.Id,
            ReportId = item.Id,
            ReportYear = 2025,
            ReportType = "Summary",
            AuthorizationScope = KnowledgeAuthorizationScope.Report,
            CreatedAt = created,
        });
        await context.SaveChangesAsync();

        var stored = await context.KnowledgeDocuments
            .Include(row => row.School)
            .Include(row => row.Report)
            .SingleAsync();

        Assert.Equal(school.Id, stored.SchoolId);
        Assert.Equal(item.Id, stored.ReportId);
        Assert.Equal(2025, stored.ReportYear);
        Assert.Equal("Summary", stored.ReportType);
        Assert.Equal(KnowledgeAuthorizationScope.Report, stored.AuthorizationScope);
        Assert.Equal("data/reports/10701_summary2025.pdf", stored.SourceIdentifier);
        Assert.Equal("10701", stored.School!.Code);
        Assert.Equal(item.Id, stored.Report!.Id);
    }

    [Fact]
    public async Task UnknownSchool_FailsForeignKey()
    {
        await using var db = await SqliteTestDatabase.CreateAsync();
        var context = db.Context;

        context.KnowledgeDocuments.Add(new KnowledgeDocument
        {
            FileName = "missing-school.pdf",
            DocumentType = KnowledgeDocumentType.GeneratedReport,
            ContentHash = new string('c', 64),
            SourceIdentifier = "output/2025/99999/summary-report.pdf",
            IndexedAt = DateTimeOffset.UtcNow,
            SchoolId = 4242,
            AuthorizationScope = KnowledgeAuthorizationScope.School,
            CreatedAt = DateTimeOffset.UtcNow,
        });

        await Assert.ThrowsAnyAsync<DbUpdateException>(() => context.SaveChangesAsync());
    }

    [Fact]
    public async Task ChunkNumber_IsUniquePerDocument()
    {
        await using var db = await SqliteTestDatabase.CreateAsync();
        var context = db.Context;
        var created = DateTimeOffset.UtcNow;
        var document = new KnowledgeDocument
        {
            FileName = "notes.md",
            DocumentType = KnowledgeDocumentType.Legacy,
            ContentHash = new string('d', 64),
            SourceIdentifier = "docs/notes.md",
            IndexedAt = created,
            AuthorizationScope = KnowledgeAuthorizationScope.Authenticated,
            CreatedAt = created,
        };
        document.Chunks.Add(Chunk(1, created));
        document.Chunks.Add(Chunk(1, created));
        context.KnowledgeDocuments.Add(document);

        await Assert.ThrowsAnyAsync<DbUpdateException>(() => context.SaveChangesAsync());
    }

    [Fact]
    public async Task AuthorizationAwareQuery_ReturnsGlobalAndAssignedSchoolDocumentsOnly()
    {
        await using var db = await SqliteTestDatabase.CreateAsync();
        var context = db.Context;
        var schoolA = new School { Code = "10701", Name = "School A" };
        var schoolB = new School { Code = "23306", Name = "School B" };
        context.Schools.AddRange(schoolA, schoolB);
        await context.SaveChangesAsync();

        var created = DateTimeOffset.UtcNow;
        context.KnowledgeDocuments.AddRange(
            new KnowledgeDocument
            {
                FileName = "legacy.pdf",
                DocumentType = KnowledgeDocumentType.Legacy,
                ContentHash = new string('e', 64),
                SourceIdentifier = "legacy/baseline/test-school-report.pdf",
                IndexedAt = created,
                AuthorizationScope = KnowledgeAuthorizationScope.Authenticated,
                CreatedAt = created,
            },
            new KnowledgeDocument
            {
                FileName = "a.pdf",
                DocumentType = KnowledgeDocumentType.GeneratedReport,
                ContentHash = new string('f', 64),
                SourceIdentifier = "output/2025/10701/summary-report.pdf",
                IndexedAt = created,
                SchoolId = schoolA.Id,
                ReportYear = 2025,
                ReportType = "Summary",
                AuthorizationScope = KnowledgeAuthorizationScope.School,
                CreatedAt = created,
            },
            new KnowledgeDocument
            {
                FileName = "b.pdf",
                DocumentType = KnowledgeDocumentType.GeneratedReport,
                ContentHash = new string('1', 64),
                SourceIdentifier = "output/2025/23306/summary-report.pdf",
                IndexedAt = created,
                SchoolId = schoolB.Id,
                ReportYear = 2025,
                ReportType = "Summary",
                AuthorizationScope = KnowledgeAuthorizationScope.Report,
                CreatedAt = created,
            });
        await context.SaveChangesAsync();

        var assignedA = await context.KnowledgeDocuments
            .WhereAccessible(isAuthenticated: true, isAdmin: false, accessibleSchoolIds: new HashSet<int> { schoolA.Id })
            .Select(document => document.FileName)
            .OrderBy(name => name)
            .ToListAsync();
        var admin = await context.KnowledgeDocuments
            .WhereAccessible(isAuthenticated: true, isAdmin: true, accessibleSchoolIds: new HashSet<int>())
            .Select(document => document.FileName)
            .OrderBy(name => name)
            .ToListAsync();
        var anonymous = await context.KnowledgeDocuments
            .WhereAccessible(isAuthenticated: false, isAdmin: false, accessibleSchoolIds: new HashSet<int>())
            .ToListAsync();

        Assert.Equal(["a.pdf", "legacy.pdf"], assignedA);
        Assert.Equal(["a.pdf", "b.pdf", "legacy.pdf"], admin);
        Assert.Empty(anonymous);
    }

    [Fact]
    public async Task DeletingDocument_CascadesChunks()
    {
        await using var db = await SqliteTestDatabase.CreateAsync();
        var context = db.Context;
        var created = DateTimeOffset.UtcNow;
        var document = new KnowledgeDocument
        {
            FileName = "legacy.pdf",
            DocumentType = KnowledgeDocumentType.Legacy,
            ContentHash = new string('2', 64),
            SourceIdentifier = "legacy/notes.pdf",
            IndexedAt = created,
            AuthorizationScope = KnowledgeAuthorizationScope.Authenticated,
            CreatedAt = created,
        };
        document.Chunks.Add(Chunk(1, created));
        context.KnowledgeDocuments.Add(document);
        await context.SaveChangesAsync();

        context.KnowledgeDocuments.Remove(document);
        await context.SaveChangesAsync();

        Assert.Empty(await context.KnowledgeChunks.ToListAsync());
    }

    private static async Task<(School School, ReportRunItem Item)> SeedReportAsync(
        SchoolReportsDbContext context)
    {
        var school = new School { Code = "10701" };
        var run = new ReportRun
        {
            Mode = ReportGenerationMode.Single,
            Status = RunStatus.Completed,
            StartedUtc = DateTimeOffset.UtcNow,
            OutputDirectory = "data/reports",
        };
        context.Schools.Add(school);
        context.ReportRuns.Add(run);
        await context.SaveChangesAsync();

        var item = new ReportRunItem
        {
            ReportRunId = run.Id,
            SchoolId = school.Id,
            Status = RunStatus.Completed,
            OutputPath = "data/reports/10701_summary2025.pdf",
        };
        context.ReportRunItems.Add(item);
        await context.SaveChangesAsync();
        return (school, item);
    }

    private static KnowledgeChunk Chunk(int number, DateTimeOffset created) =>
        new()
        {
            ChunkNumber = number,
            Content = "chunk",
            Category = "note",
            SourceLocation = "page 1",
            CreatedAt = created,
        };
}
