using AccessibleSchoolReports.Application.Knowledge;

namespace AccessibleSchoolReports.UnitTests.Knowledge;

public sealed class KnowledgeTextChunkerTests
{
    [Fact]
    public void Chunk_IsDeterministic()
    {
        const string text = """
            ## First
            Alpha.

            ## Second
            Bravo.
            """;

        var first = KnowledgeTextChunker.Chunk(text, KnowledgeSourceKind.Markdown);
        var second = KnowledgeTextChunker.Chunk(text, KnowledgeSourceKind.Markdown);

        Assert.Equal(first, second);
        Assert.Equal(2, first.Count);
        Assert.Equal("lines 1-3", first[0].SourceLocation);
        Assert.Equal("lines 4-5", first[1].SourceLocation);
    }

    [Fact]
    public void MarkdownRuleTable_ExtractsRuleIdAndSourceLocation()
    {
        const string text = """
            | Rule ID | Notes |
            |---|---|
            | CF-S-00 | n ge 5 |
            | SS-HDR-01 | school name |
            """;

        var chunks = KnowledgeTextChunker.Chunk(text, KnowledgeSourceKind.Markdown);

        Assert.Equal(2, chunks.Count);
        Assert.Equal("CF-S-00", chunks[0].RuleId);
        Assert.Equal("rule", chunks[0].Category);
        Assert.Equal("lines 3-3", chunks[0].SourceLocation);
        Assert.Contains("n ge 5", chunks[0].Content, StringComparison.Ordinal);
        Assert.Equal("SS-HDR-01", chunks[1].RuleId);
    }

    [Fact]
    public void MarkdownHeading_ExtractsRuleId()
    {
        const string text = """
            ### CF-FMT-01 — Offer-timing labels

            Value labels for BGRAD and AFTGRD.
            """;

        var chunks = KnowledgeTextChunker.Chunk(text, KnowledgeSourceKind.Markdown);

        Assert.Equal("CF-FMT-01", Assert.Single(chunks).RuleId);
        Assert.Equal("lines 1-3", chunks[0].SourceLocation);
    }

    [Fact]
    public void Sas_SplitsOnProcAndPreservesLocation()
    {
        const string text = """
            * header
            options pagesize=63;
            proc format;
            value $time 'BGRAD' = 'Before Graduation';
            run;
            data work.x;
            set work.y;
            run;
            """;

        var chunks = KnowledgeTextChunker.Chunk(text, KnowledgeSourceKind.Sas);

        Assert.True(chunks.Count >= 2);
        Assert.All(chunks, chunk => Assert.Equal("sas", chunk.Category));
        Assert.StartsWith("lines ", chunks[0].SourceLocation);
        Assert.Contains("proc format", string.Join('\n', chunks.Select(chunk => chunk.Content)), StringComparison.OrdinalIgnoreCase);
        Assert.Null(chunks[0].RuleId);
    }
}
