namespace AccessibleSchoolReports.Application.Knowledge;

public enum KnowledgeSourceKind
{
    Sas = 0,
    Markdown = 1,
}

public sealed record KnowledgeTextChunk(
    int ChunkNumber,
    string Content,
    string? RuleId,
    string Category,
    string SourceLocation);

public static class KnowledgeTextChunker
{
    public const int MaxChunkLines = 50;
    public const int MaxChunkCharacters = 2000;

    public static IReadOnlyList<KnowledgeTextChunk> ChunkPages(IReadOnlyList<PdfExtractedPage> pages)
    {
        ArgumentNullException.ThrowIfNull(pages);
        var chunks = new List<KnowledgeTextChunk>();
        foreach (var page in pages.OrderBy(page => page.PageNumber))
        {
            if (string.IsNullOrWhiteSpace(page.Text))
            {
                continue;
            }

            var pageChunks = Chunk(page.Text, KnowledgeSourceKind.Markdown);
            if (pageChunks.Count == 0)
            {
                continue;
            }

            if (pageChunks.Count == 1)
            {
                var single = pageChunks[0];
                chunks.Add(single with
                {
                    ChunkNumber = chunks.Count + 1,
                    Category = single.RuleId is null ? "report" : "rule",
                    SourceLocation = $"page {page.PageNumber}",
                });
                continue;
            }

            foreach (var piece in pageChunks)
            {
                chunks.Add(piece with
                {
                    ChunkNumber = chunks.Count + 1,
                    Category = piece.RuleId is null ? "report" : "rule",
                    SourceLocation = $"page {page.PageNumber}, {piece.SourceLocation}",
                });
            }
        }

        return chunks;
    }

    public static IReadOnlyList<KnowledgeTextChunk> Chunk(string text, KnowledgeSourceKind kind)
    {
        var lines = NormalizeLines(text);
        var ranges = kind == KnowledgeSourceKind.Markdown
            ? SplitMarkdown(lines)
            : SplitSas(lines);

        var chunks = new List<KnowledgeTextChunk>();
        foreach (var range in ranges)
        {
            foreach (var piece in SplitOversized(lines, range))
            {
                var content = Join(lines, piece.Start, piece.End);
                if (string.IsNullOrWhiteSpace(content))
                {
                    continue;
                }

                var ruleId = KnowledgeRuleIds.FirstIn(content);
                chunks.Add(new KnowledgeTextChunk(
                    chunks.Count + 1,
                    content,
                    ruleId,
                    CategoryFor(kind, ruleId),
                    $"lines {piece.Start + 1}-{piece.End + 1}"));
            }
        }

        return chunks;
    }

    private static string CategoryFor(KnowledgeSourceKind kind, string? ruleId)
    {
        if (ruleId is not null)
        {
            return "rule";
        }

        return kind == KnowledgeSourceKind.Sas ? "sas" : "section";
    }

    private static List<(int Start, int End)> SplitMarkdown(string[] lines)
    {
        var ranges = new List<(int Start, int End)>();
        var index = 0;
        while (index < lines.Length)
        {
            if (IsRuleTableHeader(lines[index]))
            {
                index = SkipTableHeader(lines, index);
                while (index < lines.Length && IsTableRow(lines[index]))
                {
                    ranges.Add((index, index));
                    index++;
                }

                continue;
            }

            var start = index;
            index++;
            while (index < lines.Length
                && !IsHeading(lines[index])
                && !IsRuleTableHeader(lines[index]))
            {
                index++;
            }

            ranges.Add((start, index - 1));
        }

        return ranges;
    }

    private static List<(int Start, int End)> SplitSas(string[] lines)
    {
        var ranges = new List<(int Start, int End)>();
        var index = 0;
        while (index < lines.Length)
        {
            var start = index;
            index++;
            while (index < lines.Length
                && !IsSasBoundary(lines[index])
                && index - start < MaxChunkLines)
            {
                index++;
            }

            ranges.Add((start, index - 1));
        }

        return ranges;
    }

    private static IEnumerable<(int Start, int End)> SplitOversized(
        string[] lines,
        (int Start, int End) range)
    {
        var content = Join(lines, range.Start, range.End);
        var lineCount = range.End - range.Start + 1;
        if (content.Length <= MaxChunkCharacters && lineCount <= MaxChunkLines)
        {
            yield return range;
            yield break;
        }

        var start = range.Start;
        while (start <= range.End)
        {
            var end = Math.Min(range.End, start + MaxChunkLines - 1);
            while (end < range.End && Join(lines, start, end).Length < MaxChunkCharacters)
            {
                var next = Join(lines, start, end + 1);
                if (next.Length > MaxChunkCharacters)
                {
                    break;
                }

                end++;
            }

            yield return (start, end);
            start = end + 1;
        }
    }

    private static int SkipTableHeader(string[] lines, int headerIndex)
    {
        var index = headerIndex + 1;
        if (index < lines.Length && IsTableSeparator(lines[index]))
        {
            index++;
        }

        return index;
    }

    private static bool IsHeading(string line)
    {
        var trimmed = line.TrimStart();
        if (!trimmed.StartsWith('#'))
        {
            return false;
        }

        var hashes = trimmed.TakeWhile(ch => ch == '#').Count();
        return hashes is >= 1 and <= 6
            && trimmed.Length > hashes
            && char.IsWhiteSpace(trimmed[hashes]);
    }

    private static bool IsRuleTableHeader(string line) =>
        IsTableRow(line)
        && line.Contains("Rule ID", StringComparison.OrdinalIgnoreCase);

    private static bool IsTableSeparator(string line)
    {
        if (!IsTableRow(line))
        {
            return false;
        }

        var body = line.Replace("|", string.Empty).Replace(":", string.Empty).Replace("-", string.Empty);
        return string.IsNullOrWhiteSpace(body);
    }

    private static bool IsTableRow(string line) =>
        line.TrimStart().StartsWith('|');

    private static bool IsSasBoundary(string line)
    {
        var trimmed = line.TrimStart();
        if (trimmed.StartsWith("*---", StringComparison.Ordinal)
            || trimmed.StartsWith("***", StringComparison.Ordinal))
        {
            return true;
        }

        return StartsWithWord(trimmed, "proc")
            || StartsWithWord(trimmed, "data")
            || StartsWithWord(trimmed, "libname")
            || StartsWithWord(trimmed, "%macro")
            || StartsWithWord(trimmed, "%mend");
    }

    private static bool StartsWithWord(string line, string word) =>
        line.StartsWith(word, StringComparison.OrdinalIgnoreCase)
        && (line.Length == word.Length || !char.IsLetterOrDigit(line[word.Length]));

    private static string[] NormalizeLines(string text)
    {
        var normalized = text.Replace("\r\n", "\n", StringComparison.Ordinal)
            .Replace('\r', '\n');
        return normalized.Split('\n');
    }

    private static string Join(string[] lines, int start, int end)
    {
        if (start > end)
        {
            return string.Empty;
        }

        return string.Join('\n', lines.Skip(start).Take(end - start + 1)).Trim();
    }
}
