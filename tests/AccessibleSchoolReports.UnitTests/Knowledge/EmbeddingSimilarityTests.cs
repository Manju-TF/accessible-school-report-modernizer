using AccessibleSchoolReports.Application.Knowledge;

namespace AccessibleSchoolReports.UnitTests.Knowledge;

public sealed class EmbeddingSimilarityTests
{
    [Fact]
    public void Cosine_IdenticalVectors_IsOne()
    {
        float[] vector = [1f, 0f, 0f, 0f];
        Assert.Equal(1f, EmbeddingSimilarity.Cosine(vector, vector), 5);
    }

    [Fact]
    public void Cosine_OrthogonalVectors_IsZero()
    {
        float[] left = [1f, 0f];
        float[] right = [0f, 1f];
        Assert.Equal(0f, EmbeddingSimilarity.Cosine(left, right), 5);
    }

    [Fact]
    public void Cosine_MismatchedOrZero_IsZero()
    {
        Assert.Equal(0f, EmbeddingSimilarity.Cosine([1f, 0f], [1f]));
        Assert.Equal(0f, EmbeddingSimilarity.Cosine([0f, 0f], [1f, 0f]));
        Assert.Equal(0f, EmbeddingSimilarity.Cosine([], [1f]));
    }
}
