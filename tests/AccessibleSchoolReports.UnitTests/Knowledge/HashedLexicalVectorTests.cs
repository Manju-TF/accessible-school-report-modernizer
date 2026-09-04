using AccessibleSchoolReports.Application.Knowledge;

namespace AccessibleSchoolReports.UnitTests.Knowledge;

public sealed class HashedLexicalVectorTests
{
    [Fact]
    public void RelatedText_ScoresHigherThanUnrelated()
    {
        var query = HashedLexicalVector.Embed("salary suppression n ge 5 CF-S-00", 256);
        var salary = HashedLexicalVector.Embed("Salary row kept only if n ge 5. RuleId CF-S-00.", 256);
        var lunch = HashedLexicalVector.Embed("cafeteria lunch menu for next Tuesday", 256);

        Assert.True(EmbeddingSimilarity.Cosine(query, salary) > EmbeddingSimilarity.Cosine(query, lunch));
    }
}
