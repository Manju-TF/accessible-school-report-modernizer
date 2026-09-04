using AccessibleSchoolReports.Application.Knowledge;
using AccessibleSchoolReports.Application.Security;
using AccessibleSchoolReports.Infrastructure.Knowledge;
using AccessibleSchoolReports.Infrastructure.Security;
using AccessibleSchoolReports.UnitTests.Embeddings;
using Microsoft.EntityFrameworkCore;

namespace AccessibleSchoolReports.UnitTests.Knowledge;

public sealed class KnowledgeEmbeddingIndexServiceTests
{
    [Fact]
    public async Task IndexesPendingChunks_AndReportsStatus()
    {
        await using var fixture = await EmbeddingTestFixture.CreateAsync();
        var fake = fixture.CreateFake();
        var indexer = CreateIndexer(fixture, fake);
        var admin = EmbeddingTestFixture.Principal("admin", AppRoles.Admin);

        var result = await indexer.IndexPendingEmbeddingsAsync(admin);

        Assert.Equal(3, result.ChunksIndexed);
        Assert.Equal(0, result.ChunksSkipped);
        Assert.Equal(0, result.Failures);
        Assert.Equal(3, result.DocumentsIndexed);
        Assert.True(result.Duration >= TimeSpan.Zero);
        Assert.Equal(3, fake.EmbedCalls);
        Assert.False(fake.UsedNetwork);

        fixture.Db.ChangeTracker.Clear();
        var chunks = await fixture.Db.KnowledgeChunks.ToListAsync();
        Assert.All(chunks, chunk =>
        {
            Assert.NotNull(chunk.Embedding);
            Assert.Equal(fake.Model.Key, chunk.EmbeddingModel);
        });
    }

    [Fact]
    public async Task DoesNotEmbedUnchangedChunks()
    {
        await using var fixture = await EmbeddingTestFixture.CreateAsync();
        var fake = fixture.CreateFake();
        var indexer = CreateIndexer(fixture, fake);
        var admin = EmbeddingTestFixture.Principal("admin", AppRoles.Admin);

        var first = await indexer.IndexPendingEmbeddingsAsync(admin);
        var second = await indexer.IndexPendingEmbeddingsAsync(admin);

        Assert.Equal(3, first.ChunksIndexed);
        Assert.Equal(0, second.ChunksIndexed);
        Assert.Equal(3, second.ChunksSkipped);
        Assert.Equal(0, second.DocumentsIndexed);
        Assert.Equal(3, fake.EmbedCalls);
    }

    [Fact]
    public async Task ReembedsWhenModelChanges()
    {
        await using var fixture = await EmbeddingTestFixture.CreateAsync();
        var fake = fixture.CreateFake();
        var indexer = CreateIndexer(fixture, fake);
        var admin = EmbeddingTestFixture.Principal("admin", AppRoles.Admin);
        await indexer.IndexPendingEmbeddingsAsync(admin);

        fixture.Options.Model = "test-embed-v2";
        var result = await indexer.IndexPendingEmbeddingsAsync(admin);

        Assert.Equal(3, result.ChunksIndexed);
        Assert.Equal(0, result.ChunksSkipped);
        Assert.Equal(6, fake.EmbedCalls);
        fixture.Db.ChangeTracker.Clear();
        Assert.All(
            await fixture.Db.KnowledgeChunks.ToListAsync(),
            chunk => Assert.Equal("Fake/test-embed-v2", chunk.EmbeddingModel));
    }

    [Fact]
    public async Task DoesNotSendUnauthorizedReportContent()
    {
        await using var fixture = await EmbeddingTestFixture.CreateAsync();
        var fake = fixture.CreateFake();
        var indexer = CreateIndexer(fixture, fake);
        var user = EmbeddingTestFixture.Principal("user-a", AppRoles.ReportUser);

        var result = await indexer.IndexPendingEmbeddingsAsync(user);

        Assert.Equal(2, result.ChunksIndexed);
        Assert.Equal(1, result.ChunksSkipped);
        Assert.DoesNotContain(fixture.SchoolBChunkId, fake.RequestedChunkIds);
        fixture.Db.ChangeTracker.Clear();
        var schoolB = await fixture.Db.KnowledgeChunks.SingleAsync(chunk => chunk.Id == fixture.SchoolBChunkId);
        Assert.Null(schoolB.Embedding);
    }

    [Fact]
    public async Task IndividualFailure_DoesNotStopOtherChunks()
    {
        await using var fixture = await EmbeddingTestFixture.CreateAsync();
        var fake = fixture.CreateFake();
        fake.FailChunkIds.Add(fixture.SchoolAChunkId);
        var indexer = CreateIndexer(fixture, fake);
        var admin = EmbeddingTestFixture.Principal("admin", AppRoles.Admin);

        var result = await indexer.IndexPendingEmbeddingsAsync(admin);

        Assert.Equal(2, result.ChunksIndexed);
        Assert.Equal(1, result.Failures);
        Assert.Equal(fixture.SchoolAChunkId, Assert.Single(result.FailureDetails).ChunkId);
        fixture.Db.ChangeTracker.Clear();
        var failed = await fixture.Db.KnowledgeChunks.SingleAsync(chunk => chunk.Id == fixture.SchoolAChunkId);
        var legacy = await fixture.Db.KnowledgeChunks.SingleAsync(chunk => chunk.Id == fixture.LegacyChunkId);
        Assert.Null(failed.Embedding);
        Assert.NotNull(legacy.Embedding);
    }

    [Fact]
    public async Task Cancellation_StopsFurtherEmbedding()
    {
        await using var fixture = await EmbeddingTestFixture.CreateAsync();
        var fake = fixture.CreateFake();
        var indexer = CreateIndexer(fixture, fake);
        using var cts = new CancellationTokenSource();
        await cts.CancelAsync();

        await Assert.ThrowsAnyAsync<OperationCanceledException>(
            () => indexer.IndexPendingEmbeddingsAsync(
                EmbeddingTestFixture.Principal("admin", AppRoles.Admin),
                cts.Token));
        Assert.Equal(0, fake.EmbedCalls);
    }

    private static KnowledgeEmbeddingIndexService CreateIndexer(
        EmbeddingTestFixture fixture,
        FakeEmbeddingService fake) =>
        new(
            new EmbeddingTestFixture.Factory(fixture.DbOptions),
            fake,
            new ReportAuthorizationService(fixture.Db));
}
