namespace AccessibleSchoolReports.Application.Knowledge;

public enum KnowledgeSourceKind
{
    Sas = 0,
    Markdown = 1,
    Code = 2,
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
    public const int PdfOverlapLines = 2;

    public static IReadOnlyList<KnowledgeTextChunk> ChunkGeneratedReportPages(
        IReadOnlyList<PdfExtractedPage> pages,
        string? schoolCode,
        int? reportYear,
        string? schoolName = null)
    {
        ArgumentNullException.ThrowIfNull(pages);
        var label = FormatReportLabel(schoolCode, reportYear, schoolName);
        var chunks = new List<KnowledgeTextChunk>();
        foreach (var page in pages.OrderBy(item => item.PageNumber))
        {
            if (string.IsNullOrWhiteSpace(page.Text))
            {
                continue;
            }

            var lines = NormalizeLines(page.Text.Trim());
            if (lines.Length == 0)
            {
                continue;
            }

            foreach (var piece in SplitPdfPage(lines))
            {
                var body = Join(lines, piece.Start, piece.End);
                if (string.IsNullOrWhiteSpace(body))
                {
                    continue;
                }

                chunks.Add(new KnowledgeTextChunk(
                    chunks.Count + 1,
                    $"{label}, page {page.PageNumber}{Environment.NewLine}{body}",
                    null,
                    "report",
                    piece.Start == 0 && piece.End == lines.Length - 1
                        ? $"page {page.PageNumber}"
                        : $"page {page.PageNumber}, lines {piece.Start + 1}-{piece.End + 1}"));
            }
        }

        return chunks;
    }

    public static string FormatReportLabel(string? schoolCode, int? reportYear, string? schoolName = null)
    {
        var school = string.IsNullOrWhiteSpace(schoolCode) ? "unknown" : schoolCode.Trim();
        var year = reportYear is > 0 ? reportYear.Value.ToString() : "unknown";
        if (string.IsNullOrWhiteSpace(schoolName))
        {
            return $"[School {school}, Class of {year}]";
        }

        return $"[School {school} {schoolName.Trim()}, Class of {year}]";
    }

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
        var ranges = kind switch
        {
            KnowledgeSourceKind.Markdown => SplitMarkdown(lines),
            KnowledgeSourceKind.Code => SplitCode(lines)
                .Select(range => (range.Start, range.End, ContextStart: range.Start))
                .ToList(),
            _ => SplitSas(lines)
                .Select(range => (range.Start, range.End, ContextStart: range.Start))
                .ToList(),
        };

        var chunks = new List<KnowledgeTextChunk>();
        foreach (var range in ranges)
        {
            foreach (var piece in SplitOversized(lines, (range.Start, range.End)))
            {
                var content = range.ContextStart < piece.Start
                    ? $"{Join(lines, range.ContextStart, range.ContextStart)}\n{Join(lines, piece.Start, piece.End)}"
                    : Join(lines, piece.Start, piece.End);
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

        return kind switch
        {
            KnowledgeSourceKind.Sas => "sas",
            KnowledgeSourceKind.Code => "code",
            _ => "section",
        };
    }

    private static List<(int Start, int End, int ContextStart)> SplitMarkdown(string[] lines)
    {
        var ranges = new List<(int Start, int End, int ContextStart)>();
        var index = 0;
        while (index < lines.Length)
        {
            if (IsMarkdownTableHeader(lines, index))
            {
                var contextStart = index;
                index = SkipTableHeader(lines, index);
                while (index < lines.Length && IsTableRow(lines[index]))
                {
                    ranges.Add((index, index, contextStart));
                    index++;
                }

                continue;
            }

            var start = index;
            index++;
            while (index < lines.Length
                && !IsHeading(lines[index])
                && !IsMarkdownTableHeader(lines, index))
            {
                index++;
            }

            ranges.Add((start, index - 1, start));
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

    private static List<(int Start, int End)> SplitCode(string[] lines)
    {
        var ranges = new List<(int Start, int End)>();
        var start = 0;
        while (start < lines.Length)
        {
            var end = Math.Min(lines.Length - 1, start + MaxChunkLines - 1);
            while (end > start && Join(lines, start, end).Length > MaxChunkCharacters)
            {
                end--;
            }

            while (end < lines.Length - 1
                && end - start + 1 < MaxChunkLines
                && Join(lines, start, end + 1).Length <= MaxChunkCharacters)
            {
                end++;
            }

            ranges.Add((start, end));
            if (end == lines.Length - 1)
            {
                break;
            }

            start = Math.Max(start + 1, end - PdfOverlapLines + 1);
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

    private static IEnumerable<(int Start, int End)> SplitPdfPage(string[] lines)
    {
        var start = 0;
        while (start < lines.Length)
        {
            var end = Math.Min(lines.Length - 1, start + MaxChunkLines - 1);
            while (end > start && Join(lines, start, end).Length > MaxChunkCharacters)
            {
                end--;
            }

            while (end < lines.Length - 1
                && end - start + 1 < MaxChunkLines
                && Join(lines, start, end + 1).Length <= MaxChunkCharacters)
            {
                end++;
            }

            if (end < lines.Length - 1
                && IsPdfTableRow(lines[end])
                && IsPdfTableRow(lines[end + 1]))
            {
                var tableStart = end;
                while (tableStart > start && IsPdfTableRow(lines[tableStart - 1]))
                {
                    tableStart--;
                }

                if (tableStart > start)
                {
                    end = tableStart - 1;
                }
            }

            yield return (start, end);
            if (end == lines.Length - 1)
            {
                yield break;
            }

            start = Math.Max(start + 1, end - PdfOverlapLines + 1);
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

    private static bool IsMarkdownTableHeader(string[] lines, int index) =>
        IsTableRow(lines[index])
        && index + 1 < lines.Length
        && IsTableSeparator(lines[index + 1]);

    private static bool IsPdfTableRow(string line)
    {
        if (line.Contains('|'))
        {
            return true;
        }

        var tokens = line.Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries);
        return tokens.Length >= 4 && tokens.Count(token => token.Any(char.IsDigit)) >= 2;
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
