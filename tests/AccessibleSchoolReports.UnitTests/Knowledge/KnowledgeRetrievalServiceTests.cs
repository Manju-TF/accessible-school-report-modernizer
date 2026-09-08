using AccessibleSchoolReports.Application.Knowledge;
using AccessibleSchoolReports.Application.Security;
using AccessibleSchoolReports.Domain.Entities;
using AccessibleSchoolReports.Domain.Knowledge;
using AccessibleSchoolReports.Domain.Persistence;
using Microsoft.EntityFrameworkCore;

namespace AccessibleSchoolReports.UnitTests.Knowledge;

public sealed class KnowledgeRetrievalServiceTests
{
    private static readonly KnowledgeRetrievalOptions OpenOptions = new()
    {
        TopK = 10,
        MinimumSimilarity = 0.1f,
    };

    [Fact]
    public async Task UserA_CannotRetrieveSchoolBChunks()
    {
        await using var fixture = await KnowledgeRetrievalTestFixture.CreateAsync();
        var (service, _) = fixture.CreateSut();

        var result = await service.RetrieveAsync(
            KnowledgeRetrievalTestFixture.Principal("user-a", AppRoles.ReportUser),
            "salary suppression",
            OpenOptions);

        Assert.DoesNotContain(result.Hits, hit => hit.ChunkId == fixture.SchoolBChunkId);
        Assert.DoesNotContain(result.Hits, hit => hit.SchoolId == fixture.SchoolBId);
        Assert.DoesNotContain(result.Hits, hit => hit.Content.Contains(KnowledgeRetrievalTestFixture.SchoolBSecret));
    }

    [Fact]
    public async Task UnauthorizedChunks_AreNeverReturned()
    {
        await using var fixture = await KnowledgeRetrievalTestFixture.CreateAsync();
        var (service, embeddings) = fixture.CreateSut();

        var result = await service.RetrieveAsync(
            KnowledgeRetrievalTestFixture.Principal("user-a", AppRoles.ReportUser),
            "best matching secret",
            OpenOptions);

        Assert.Equal(2, result.AuthorizedCandidateCount);
        Assert.DoesNotContain(result.Hits, hit => hit.ChunkId == fixture.SchoolBChunkId || hit.ChunkId == fixture.AdminChunkId);
        Assert.True(embeddings.EmbedCalls >= 1);
    }

    [Fact]
    public async Task Admin_CanRetrieveAuthorizedGlobalKnowledge()
    {
        await using var fixture = await KnowledgeRetrievalTestFixture.CreateAsync();
        var (service, _) = fixture.CreateSut();

        var result = await service.RetrieveAsync(
            KnowledgeRetrievalTestFixture.Principal("admin", AppRoles.Admin),
            "salary suppression",
            OpenOptions);

        var legacy = Assert.Single(result.Hits, hit => hit.ChunkId == fixture.LegacyChunkId);
        Assert.Equal(KnowledgeAuthorizationScope.Authenticated, legacy.AuthorizationScope);
        Assert.Equal(KnowledgeRetrievalTestFixture.LegacyText, legacy.Content);
        Assert.Equal("SS-00", legacy.RuleId);
        Assert.Equal("legacy/sas/notes.md:1", legacy.SourceLocation);
        Assert.Equal("legacy.md", legacy.SourceIdentifier);
    }

    [Fact]
    public async Task Hits_IncludeSourceMetadata()
    {
        await using var fixture = await KnowledgeRetrievalTestFixture.CreateAsync();
        var (service, _) = fixture.CreateSut();

        var result = await service.RetrieveAsync(
            KnowledgeRetrievalTestFixture.Principal("user-a", AppRoles.ReportUser),
            "school A report",
            OpenOptions);

        var hit = Assert.Single(result.Hits, item => item.ChunkId == fixture.SchoolAChunkId);
        Assert.Equal("CF-S-00", hit.RuleId);
        Assert.Equal(fixture.SchoolAId, hit.SchoolId);
        Assert.Equal(fixture.SchoolAReportId, hit.ReportId);
        Assert.Equal(2025, hit.ReportYear);
        Assert.Equal("page 1", hit.SourceLocation);
        Assert.Equal("a.pdf", hit.SourceIdentifier);
        Assert.Equal("10701", hit.SchoolCode);
        Assert.True(hit.Similarity >= 0.1f);
    }

    [Fact]
    public async Task UserA_CanRetrieveScopedSchoolAReportOnly()
    {
        await using var fixture = await KnowledgeRetrievalTestFixture.CreateAsync();
        var (service, _) = fixture.CreateSut();

        var result = await service.RetrieveAsync(
            KnowledgeRetrievalTestFixture.Principal("user-a", AppRoles.ReportUser),
            "employment",
            new KnowledgeRetrievalOptions
            {
                TopK = 10,
                MinimumSimilarity = 0.1f,
                ReportId = fixture.SchoolAReportId,
            });

        Assert.Equal(fixture.SchoolAChunkId, Assert.Single(result.Hits).ChunkId);
        Assert.DoesNotContain(result.Hits, hit => hit.ChunkId == fixture.LegacyChunkId);
        Assert.DoesNotContain(result.Hits, hit => hit.ChunkId == fixture.SchoolBChunkId);
    }

    [Fact]
    public async Task UserA_CannotRetrieveScopedSchoolBReport()
    {
        await using var fixture = await KnowledgeRetrievalTestFixture.CreateAsync();
        var (service, embeddings) = fixture.CreateSut();

        var result = await service.RetrieveAsync(
            KnowledgeRetrievalTestFixture.Principal("user-a", AppRoles.ReportUser),
            "employment",
            new KnowledgeRetrievalOptions
            {
                TopK = 10,
                MinimumSimilarity = 0.1f,
                ReportId = fixture.SchoolBReportId,
            });

        Assert.Empty(result.Hits);
        Assert.Equal(0, result.AuthorizedCandidateCount);
        Assert.Equal(0, embeddings.EmbedCalls);
        Assert.DoesNotContain(KnowledgeRetrievalTestFixture.SchoolBSecret, result.Hits.Select(hit => hit.Content));
    }

    [Fact]
    public async Task TamperedReportId_DoesNotBypassAuthorization()
    {
        await using var fixture = await KnowledgeRetrievalTestFixture.CreateAsync();
        var (service, embeddings) = fixture.CreateSut();
        var user = KnowledgeRetrievalTestFixture.Principal("user-a", AppRoles.ReportUser);
        await service.RetrieveAsync(
            user,
            "employment",
            new KnowledgeRetrievalOptions { ReportId = fixture.SchoolAReportId, MinimumSimilarity = 0.1f, TopK = 10 });

        var tampered = await service.RetrieveAsync(
            user,
            "employment",
            new KnowledgeRetrievalOptions { ReportId = fixture.SchoolBReportId, MinimumSimilarity = 0.1f, TopK = 10 });

        Assert.Empty(tampered.Hits);
        Assert.Equal(0, tampered.AuthorizedCandidateCount);
        Assert.Equal(1, embeddings.EmbedCalls);
    }

    [Fact]
    public async Task AppliesMinimumSimilarityAndTopK()
    {
        await using var fixture = await KnowledgeRetrievalTestFixture.CreateAsync();
        var (service, _) = fixture.CreateSut();

        var result = await service.RetrieveAsync(
            KnowledgeRetrievalTestFixture.Principal("admin", AppRoles.Admin),
            "query",
            new KnowledgeRetrievalOptions { TopK = 1, MinimumSimilarity = 0.9f });

        var hit = Assert.Single(result.Hits);
        Assert.Equal(fixture.SchoolBChunkId, hit.ChunkId);
        Assert.True(hit.Similarity >= 0.9f);
        Assert.Equal(4, result.AuthorizedCandidateCount);
    }

    [Fact]
    public async Task ReportScopedNumberQuestion_ReturnsEveryPageOfThatReport()
    {
        await using var fixture = await KnowledgeRetrievalTestFixture.CreateAsync();
        var document = await fixture.Db.KnowledgeDocuments.SingleAsync(row => row.ReportId == fixture.SchoolAReportId);
        fixture.Db.KnowledgeChunks.AddRange(
            new KnowledgeChunk
            {
                KnowledgeDocumentId = document.Id,
                ChunkNumber = 2,
                Content = "[School 10701, Class of 2025], page 2 Employed 18 90.0",
                Category = "report",
                SourceLocation = "page 2",
                Embedding = EmbeddingVectorConvert.ToBytes([0.1f, 0.1f, 0.1f, 0.1f]),
                EmbeddingModel = "Fake/test-embed",
                CreatedAt = DateTimeOffset.UtcNow,
            },
            new KnowledgeChunk
            {
                KnowledgeDocumentId = document.Id,
                ChunkNumber = 7,
                Content = "[School 10701, Class of 2025], page 7 Total Reported 18",
                Category = "report",
                SourceLocation = "page 7",
                Embedding = EmbeddingVectorConvert.ToBytes([0.05f, 0.05f, 0.05f, 0.05f]),
                EmbeddingModel = "Fake/test-embed",
                CreatedAt = DateTimeOffset.UtcNow,
            });
        await fixture.Db.SaveChangesAsync();
        fixture.Db.ChangeTracker.Clear();
        var (service, _) = fixture.CreateSut();

        var result = await service.RetrieveAsync(
            KnowledgeRetrievalTestFixture.Principal("user-a", AppRoles.ReportUser),
            "How many employed graduates are printed in this report?",
            new KnowledgeRetrievalOptions { ReportId = fixture.SchoolAReportId });

        Assert.Equal(3, result.Hits.Count);
        Assert.Contains(result.Hits, hit => hit.SourceLocation == "page 1");
        Assert.Contains(result.Hits, hit => hit.SourceLocation == "page 2");
        Assert.Contains(result.Hits, hit => hit.SourceLocation == "page 7");
        Assert.All(result.Hits, hit => Assert.Equal(fixture.SchoolAReportId, hit.ReportId));
        Assert.DoesNotContain(result.Hits, hit => hit.SchoolId == fixture.SchoolBId);
    }

    [Fact]
    public async Task SumQuestion_UserA_DoesNotIncludeSchoolBPrintedMetrics()
    {
        await using var fixture = await KnowledgeRetrievalTestFixture.CreateAsync();
        var schoolA = await fixture.Db.KnowledgeDocuments.SingleAsync(row => row.ReportId == fixture.SchoolAReportId);
        var schoolB = await fixture.Db.KnowledgeDocuments.SingleAsync(row => row.ReportId == fixture.SchoolBReportId);
        fixture.Db.KnowledgeChunks.AddRange(
            new KnowledgeChunk
            {
                KnowledgeDocumentId = schoolA.Id,
                ChunkNumber = 2,
                Content = "[School 10701, Class of 2025], page 1\nTotal Reported = 20",
                Category = "report",
                SourceLocation = "page 1",
                Embedding = EmbeddingVectorConvert.ToBytes([0.2f, 0.2f, 0.2f, 0.2f]),
                EmbeddingModel = "Fake/test-embed",
                CreatedAt = DateTimeOffset.UtcNow,
            },
            new KnowledgeChunk
            {
                KnowledgeDocumentId = schoolB.Id,
                ChunkNumber = 2,
                Content = $"[School 23306, Class of 2025], page 1\nTotal Reported = 99 {KnowledgeRetrievalTestFixture.SchoolBSecret}",
                Category = "report",
                SourceLocation = "page 1",
                Embedding = EmbeddingVectorConvert.ToBytes([0.2f, 0.2f, 0.2f, 0.2f]),
                EmbeddingModel = "Fake/test-embed",
                CreatedAt = DateTimeOffset.UtcNow,
            });
        await fixture.Db.SaveChangesAsync();
        fixture.Db.ChangeTracker.Clear();
        var (service, _) = fixture.CreateSut();

        var result = await service.RetrieveAsync(
            KnowledgeRetrievalTestFixture.Principal("user-a", AppRoles.ReportUser),
            "What is the sum of Total Reported across all schools?");

        Assert.Contains(result.PrintedMetricHits, hit => hit.Content.Contains("Total Reported = 20", StringComparison.Ordinal));
        Assert.DoesNotContain(result.PrintedMetricHits, hit => hit.SchoolId == fixture.SchoolBId);
        Assert.DoesNotContain(result.PrintedMetricHits, hit => hit.Content.Contains(KnowledgeRetrievalTestFixture.SchoolBSecret));
    }

    [Fact]
    public async Task ComparisonQuestion_DiversifiesAcrossAuthorizedSchoolsAndYears()
    {
        await using var fixture = await KnowledgeRetrievalTestFixture.CreateAsync();
        await AddSchoolAPriorYearAsync(fixture);
        var (service, _) = fixture.CreateSut();

        var result = await service.RetrieveAsync(
            KnowledgeRetrievalTestFixture.Principal("admin", AppRoles.Admin),
            "Compare Total Reported across generated reports I can view.");

        Assert.Contains(result.Hits, hit => hit.SchoolCode == "10701" && hit.ReportYear == 2025);
        Assert.Contains(result.Hits, hit => hit.SchoolCode == "10701" && hit.ReportYear == 2024);
        Assert.Contains(result.Hits, hit => hit.SchoolCode == "23306");
        Assert.True(result.Hits.Select(hit => hit.DocumentId).Distinct().Count() >= 3);
    }

    [Fact]
    public async Task ComparisonQuestion_UserA_DoesNotIncludeSchoolB()
    {
        await using var fixture = await KnowledgeRetrievalTestFixture.CreateAsync();
        await AddSchoolAPriorYearAsync(fixture);
        var (service, _) = fixture.CreateSut();

        var result = await service.RetrieveAsync(
            KnowledgeRetrievalTestFixture.Principal("user-a", AppRoles.ReportUser),
            "Compare year-over-year printed values for the same school in generated reports I can view.");

        Assert.Contains(result.Hits, hit => hit.SchoolCode == "10701" && hit.ReportYear == 2025);
        Assert.Contains(result.Hits, hit => hit.SchoolCode == "10701" && hit.ReportYear == 2024);
        Assert.DoesNotContain(result.Hits, hit => hit.SchoolCode == "23306");
        Assert.DoesNotContain(result.Hits, hit => hit.Content.Contains(KnowledgeRetrievalTestFixture.SchoolBSecret));
    }

    private static async Task AddSchoolAPriorYearAsync(KnowledgeRetrievalTestFixture fixture)
    {
        var prior = new ReportRunItem
        {
            ReportRunId = fixture.Db.ReportRunItems.Select(item => item.ReportRunId).First(),
            SchoolId = fixture.SchoolAId,
            Status = RunStatus.Completed,
            OutputPath = "output/2024/10701/summary-report.pdf",
        };
        fixture.Db.ReportRunItems.Add(prior);
        await fixture.Db.SaveChangesAsync();

        var document = new KnowledgeDocument
        {
            FileName = "10701-summary-report.pdf",
            DocumentType = KnowledgeDocumentType.GeneratedReport,
            ContentHash = new string('y', 64),
            SourceIdentifier = prior.OutputPath,
            IndexedAt = DateTimeOffset.UtcNow,
            SchoolId = fixture.SchoolAId,
            SchoolCode = "10701",
            ReportId = prior.Id,
            ReportYear = 2024,
            ReportType = "Summary",
            AuthorizationScope = KnowledgeAuthorizationScope.Report,
            CreatedAt = DateTimeOffset.UtcNow,
        };
        document.Chunks.Add(new KnowledgeChunk
        {
            ChunkNumber = 1,
            Content = "[School 10701, Class of 2024], page 1 Total Reported 41",
            Category = "report",
            SourceLocation = "page 1",
            Embedding = EmbeddingVectorConvert.ToBytes([0.8f, 0.6f, 0f, 0f]),
            EmbeddingModel = "Fake/test-embed",
            CreatedAt = DateTimeOffset.UtcNow,
        });
        fixture.Db.KnowledgeDocuments.Add(document);
        await fixture.Db.SaveChangesAsync();
        fixture.Db.ChangeTracker.Clear();
    }
}
