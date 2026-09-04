using System.Security.Claims;
using AccessibleSchoolReports.Application.Knowledge;
using AccessibleSchoolReports.Application.Security;

namespace AccessibleSchoolReports.UnitTests.Embeddings;

public sealed class FakeEmbeddingServiceTests
{
    [Fact]
    public async Task Fake_DoesNotUseNetwork_AndTracksModelDimensions()
    {
        await using var fixture = await EmbeddingTestFixture.CreateAsync();
        var fake = fixture.CreateFake();

        var result = await fake.EmbedPermittedChunksAsync(
            EmbeddingTestFixture.Principal("admin", AppRoles.Admin),
            [fixture.SchoolAChunkId]);

        Assert.False(fake.UsedNetwork);
        Assert.Equal(1, fake.EmbedCalls);
        Assert.Equal("Fake", result.Provider);
        Assert.Equal(4, result.Dimensions);
        Assert.Equal(4, Assert.Single(result.Embedded).Values.Length);
        Assert.Equal($"Fake/{fixture.Options.Model}", result.Model);
    }

    [Fact]
    public async Task Fake_DoesNotEmbedUnauthorizedReportContent()
    {
        await using var fixture = await EmbeddingTestFixture.CreateAsync();
        var fake = fixture.CreateFake();
        var user = EmbeddingTestFixture.Principal("user-a", AppRoles.ReportUser);

        var result = await fake.EmbedPermittedChunksAsync(
            user,
            [fixture.SchoolAChunkId, fixture.SchoolBChunkId]);

        Assert.Equal([fixture.SchoolAChunkId], result.Embedded.Select(item => item.ChunkId));
        Assert.Equal([fixture.SchoolBChunkId], result.SkippedUnauthorizedChunkIds);
        Assert.False(fake.UsedNetwork);
    }

    [Fact]
    public async Task Fake_QueryEmbedding_IsDeterministic()
    {
        await using var fixture = await EmbeddingTestFixture.CreateAsync();
        var fake = fixture.CreateFake();

        var first = await fake.EmbedQueryAsync("how is CF-S-00 applied?");
        var second = await fake.EmbedQueryAsync("how is CF-S-00 applied?");

        Assert.Equal(first.Values, second.Values);
        Assert.Equal(4, first.Dimensions);
    }
}
