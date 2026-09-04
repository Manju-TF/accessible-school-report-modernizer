using AccessibleSchoolReports.Application.Knowledge;
using AccessibleSchoolReports.Domain.Knowledge;
using AccessibleSchoolReports.Infrastructure.Knowledge;
using AccessibleSchoolReports.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace AccessibleSchoolReports.IntegrationTests;

public sealed class KnowledgeIngestionTests
{
    [Fact]
    public async Task Ingest_FromRepository_IndexesListedSources_WithoutTouchingLegacyFiles()
    {
        await using var db = await SqliteTestDatabase.CreateAsync();
        var root = SqliteConnectionString.FindRepositoryRoot(Directory.GetCurrentDirectory());
        var sasPath = Path.Combine(root, "legacy", "sas", "createschrptfiles2025.sas");
        var hasLocalLegacy = File.Exists(sasPath);
        byte[]? beforeBytes = null;
        DateTime? beforeWrite = null;
        if (hasLocalLegacy)
        {
            beforeBytes = File.ReadAllBytes(sasPath);
            beforeWrite = File.GetLastWriteTimeUtc(sasPath);
        }

        var result = await new KnowledgeIngestionService(db.Context)
            .IngestLegacyAndProjectDocumentsAsync(root);

        if (hasLocalLegacy)
        {
            Assert.Contains("legacy/sas/createschrptfiles2025.sas", result.Indexed);
            Assert.Contains("legacy/sas/schreptsummary_2025.sas", result.Indexed);
            Assert.Equal(beforeBytes, File.ReadAllBytes(sasPath));
            Assert.Equal(beforeWrite, File.GetLastWriteTimeUtc(sasPath));
        }

        Assert.Contains("docs/capstone/business-rules.md", result.Indexed);
        Assert.Contains("docs/capstone/createschrptfiles-analysis.md", result.Indexed);
        Assert.Contains("docs/capstone/schreptsummary-analysis.md", result.Indexed);
        Assert.Contains("docs/capstone/report-map.md", result.Indexed);
        Assert.Contains("docs/accessibility/pdf-accessibility-strategy.md", result.Indexed);
        Assert.Contains("docs/architecture/corrected-plan.md", result.Indexed);
        Assert.Contains("README.md", result.Indexed);
        Assert.Empty(result.Reindexed);
        Assert.DoesNotContain(result.Indexed, path => path.StartsWith("data/", StringComparison.OrdinalIgnoreCase));

        var documents = await db.Context.KnowledgeDocuments.Include(row => row.Chunks).ToListAsync();
        Assert.Contains(documents, row => row.SourceIdentifier == "docs/capstone/business-rules.md"
            && row.Chunks.Any(chunk => chunk.RuleId == "CF-S-00"));
        Assert.All(documents, document =>
        {
            Assert.Equal(KnowledgeAuthorizationScope.Authenticated, document.AuthorizationScope);
            Assert.Null(document.SchoolId);
            Assert.Null(document.ReportId);
            Assert.All(document.Chunks, chunk => Assert.Null(chunk.Embedding));
        });

        var second = await new KnowledgeIngestionService(db.Context)
            .IngestLegacyAndProjectDocumentsAsync(root);
        Assert.Empty(second.Indexed);
        Assert.Empty(second.Reindexed);
        Assert.Equal(result.Indexed, second.SkippedUnchanged);
    }
}
