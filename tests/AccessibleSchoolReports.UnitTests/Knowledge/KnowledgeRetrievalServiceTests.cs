using AccessibleSchoolReports.Application.Knowledge;
using AccessibleSchoolReports.Application.Security;
using AccessibleSchoolReports.Domain.Knowledge;

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
}
