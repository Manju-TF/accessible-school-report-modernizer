using System.Text;
using AccessibleSchoolReports.Application.Knowledge;
using AccessibleSchoolReports.Application.Security;
using AccessibleSchoolReports.Domain.Knowledge;
using Microsoft.EntityFrameworkCore;

namespace AccessibleSchoolReports.UnitTests.Knowledge;

public sealed class RagEvaluationTests
{
    private static readonly KnowledgeRetrievalOptions DefaultOptions = new()
    {
        TopK = KnowledgeRetrievalOptions.DefaultTopK,
        MinimumSimilarity = KnowledgeRetrievalOptions.DefaultMinimumSimilarity,
    };

    [Fact]
    public async Task RunEvaluation_WritesObservedResults()
    {
        await using var fixture = await RagEvaluationFixture.CreateAsync();
        Assert.True(fixture.EmbeddedChunkCount > 0, "Evaluation corpus must have embeddings.");
        Assert.Contains("legacy/sas/createschrptfiles2025.sas", fixture.Ingestion.Indexed);
        Assert.True(
            await SchoolBIsIndexedAsync(fixture),
            "School B must be indexed so the leak check is meaningful.");

        var started = DateTimeOffset.UtcNow;
        var cases = CreateCases(fixture);
        var records = new List<RagEvaluationRecord>(cases.Count);
        foreach (var definition in cases)
        {
            records.Add(await EvaluateAsync(fixture, definition));
        }

        var elapsed = DateTimeOffset.UtcNow - started;
        var markdown = RenderMarkdown(fixture, records, elapsed);
        var outputPath = Path.Combine(fixture.RepositoryRoot, "evidence", "test-results", "rag-evaluation.md");
        Directory.CreateDirectory(Path.GetDirectoryName(outputPath)!);
        await File.WriteAllTextAsync(outputPath, markdown);

        Assert.Equal(cases.Count, records.Count);
        Assert.True(records.Count >= 10);
        Assert.All(records, record => Assert.True(record.Passed, FailMessage(record)));
    }

    private static IReadOnlyList<RagCase> CreateCases(RagEvaluationFixture fixture) =>
    [
        new()
        {
            Number = 1,
            Category = "Legacy SAS logic",
            Question = "What does rule CF-S-00 in createschrptfiles2025.sas mean when salary rows are kept only if n ge 5?",
            ExpectedSource = "legacy/sas/createschrptfiles2025.sas, docs/capstone/createschrptfiles-analysis.md, or docs/capstone/business-rules.md",
            ExpectedRuleId = "CF-S-00",
            ExpectedScope = KnowledgeAuthorizationScope.Authenticated.ToString(),
            User = RagEvaluationFixture.Principal(RagEvaluationFixture.UserAId, AppRoles.ReportUser),
            Options = DefaultOptions,
            Judge = record =>
                HasSource(record, "createschrptfiles2025.sas", "createschrptfiles-analysis.md", "business-rules.md")
                && HasRule(record, "CF-S-00")
                && AllScopes(record, KnowledgeAuthorizationScope.Authenticated)
                && NoSchoolBLeak(record),
        },
        new()
        {
            Number = 2,
            Category = "Business rules",
            Question = "What does rule CF-C-08 say about mapping empgen ACAD GOVT CLERK PUBINT to PUBLIC and BUS FIRM to PRIVATE?",
            ExpectedSource = "docs/capstone/business-rules.md",
            ExpectedRuleId = "CF-C-08",
            ExpectedScope = KnowledgeAuthorizationScope.Authenticated.ToString(),
            User = RagEvaluationFixture.Principal(RagEvaluationFixture.UserAId, AppRoles.ReportUser),
            Options = DefaultOptions,
            Judge = record =>
                HasSource(record, "business-rules.md")
                && HasRule(record, "CF-C-08")
                && AllScopes(record, KnowledgeAuthorizationScope.Authenticated)
                && NoSchoolBLeak(record),
        },
        new()
        {
            Number = 3,
            Category = "Salary rules",
            Question = "When are salary statistics omitted because n ge 5 on salftperm?",
            ExpectedSource = "docs/capstone/business-rules.md or docs/capstone/createschrptfiles-analysis.md",
            ExpectedRuleId = "CF-S-00",
            ExpectedScope = KnowledgeAuthorizationScope.Authenticated.ToString(),
            User = RagEvaluationFixture.Principal(RagEvaluationFixture.UserAId, AppRoles.ReportUser),
            Options = DefaultOptions,
            Judge = record =>
                HasSource(record, "business-rules.md", "createschrptfiles-analysis.md", "createschrptfiles2025.sas")
                && HasRule(record, "CF-S-00")
                && AllScopes(record, KnowledgeAuthorizationScope.Authenticated)
                && NoSchoolBLeak(record),
        },
        new()
        {
            Number = 4,
            Category = "Employment rules",
            Question = "How is employment status counted when jobcat1 is UNKN for analvar D?",
            ExpectedSource = "docs/capstone/business-rules.md",
            ExpectedRuleId = "CF-C-05",
            ExpectedScope = KnowledgeAuthorizationScope.Authenticated.ToString(),
            User = RagEvaluationFixture.Principal(RagEvaluationFixture.UserAId, AppRoles.ReportUser),
            Options = DefaultOptions,
            Judge = record =>
                HasSource(record, "business-rules.md", "createschrptfiles-analysis.md")
                && HasRule(record, "CF-C-05")
                && AllScopes(record, KnowledgeAuthorizationScope.Authenticated)
                && NoSchoolBLeak(record),
        },
        new()
        {
            Number = 5,
            Category = "Accessibility requirements",
            Question = "What does the PDF accessibility strategy say about veraPDF, PAC, SemanticArticle, PDFUA_1, and the rule Do not add a green test named PDF is accessible?",
            ExpectedSource = "docs/accessibility/pdf-accessibility-strategy.md",
            ExpectedRuleId = "(none)",
            ExpectedScope = KnowledgeAuthorizationScope.Authenticated.ToString(),
            User = RagEvaluationFixture.Principal(RagEvaluationFixture.UserAId, AppRoles.ReportUser),
            Options = DefaultOptions,
            Judge = record =>
                HasSource(record, "pdf-accessibility-strategy.md")
                && AllScopes(record, KnowledgeAuthorizationScope.Authenticated)
                && NoSchoolBLeak(record),
        },
        new()
        {
            Number = 6,
            Category = "Modern implementation traceability",
            Question = "Where does the modern SchoolReportCalculator apply characterized SAS salary suppression CF-S-00?",
            ExpectedSource = "README.md",
            ExpectedRuleId = "CF-S-00",
            ExpectedScope = KnowledgeAuthorizationScope.Authenticated.ToString(),
            User = RagEvaluationFixture.Principal(RagEvaluationFixture.UserAId, AppRoles.ReportUser),
            Options = DefaultOptions,
            Judge = record =>
                HasSource(record, "README.md", "corrected-plan.md", "business-rules.md")
                && ContainsAny(record, "SchoolReportCalculator", "CF-S-00")
                && AllScopes(record, KnowledgeAuthorizationScope.Authenticated)
                && NoSchoolBLeak(record),
        },
        new()
        {
            Number = 7,
            Category = "Generated PDF content",
            Question = "What is Total Reported on the School A Class of 2025 summary report for school 10701?",
            ExpectedSource = "10701-summary-report.pdf (generated report, page 1)",
            ExpectedRuleId = "(none)",
            ExpectedScope = KnowledgeAuthorizationScope.Report.ToString(),
            User = RagEvaluationFixture.Principal(RagEvaluationFixture.UserAId, AppRoles.ReportUser),
            Options = DefaultOptions,
            Judge = record =>
                HasSource(record, "10701-summary-report.pdf")
                && record.Hits.Any(hit =>
                    hit.AuthorizationScope == KnowledgeAuthorizationScope.Report
                    && hit.Content.Contains(RagEvaluationFixture.SchoolAMarker, StringComparison.Ordinal))
                && NoSchoolBLeak(record),
        },
        new()
        {
            Number = 8,
            Category = "Report-specific questions",
            Question = "What employment figures appear on this report?",
            ExpectedSource = "10701-summary-report.pdf only (report-scoped)",
            ExpectedRuleId = "(none)",
            ExpectedScope = KnowledgeAuthorizationScope.Report.ToString(),
            User = RagEvaluationFixture.Principal(RagEvaluationFixture.UserAId, AppRoles.ReportUser),
            Options = new KnowledgeRetrievalOptions
            {
                TopK = KnowledgeRetrievalOptions.DefaultTopK,
                MinimumSimilarity = KnowledgeRetrievalOptions.DefaultMinimumSimilarity,
                ReportId = fixture.SchoolAReportId,
            },
            Judge = record =>
                record.Hits.Count > 0
                && record.Hits.All(hit =>
                    hit.ReportId == fixture.SchoolAReportId
                    && hit.AuthorizationScope == KnowledgeAuthorizationScope.Report
                    && hit.FileName == "10701-summary-report.pdf")
                && NoSchoolBLeak(record),
        },
        new()
        {
            Number = 9,
            Category = "Insufficient evidence",
            Question = "What is the cafeteria lunch menu for next Tuesday at the student union?",
            ExpectedSource = "(none that answer the question)",
            ExpectedRuleId = "(none)",
            ExpectedScope = "(none that answer the question)",
            User = RagEvaluationFixture.Principal(RagEvaluationFixture.UserAId, AppRoles.ReportUser),
            Options = DefaultOptions,
            Judge = record =>
                record.LanguageModelInvoked
                && !ContainsAny(record, "cafeteria", "lunch menu", "student union")
                && NoSchoolBLeak(record),
        },
        new()
        {
            Number = 10,
            Category = "Unauthorized report access",
            Question = "What does the School B report say about employment outcomes for school 23306?",
            ExpectedSource = "No School B generated report. Authenticated project docs may appear. School B secret must not reach the LLM.",
            ExpectedRuleId = "(none)",
            ExpectedScope = "Authenticated only if any hit; never Report/23306",
            User = RagEvaluationFixture.Principal(RagEvaluationFixture.UserAId, AppRoles.ReportUser),
            Options = DefaultOptions,
            Judge = NoSchoolBLeak,
        },
        new()
        {
            Number = 11,
            Category = "Unauthorized report access",
            Question = "What employment figures appear on this report?",
            ExpectedSource = "(none) — unauthorized School B reportId is ignored as empty",
            ExpectedRuleId = "(none)",
            ExpectedScope = "(none)",
            User = RagEvaluationFixture.Principal(RagEvaluationFixture.UserAId, AppRoles.ReportUser),
            Options = new KnowledgeRetrievalOptions
            {
                TopK = KnowledgeRetrievalOptions.DefaultTopK,
                MinimumSimilarity = KnowledgeRetrievalOptions.DefaultMinimumSimilarity,
                ReportId = fixture.SchoolBReportId,
            },
            Judge = record =>
                record.Hits.Count == 0
                && record.AuthorizedCandidateCount == 0
                && record.EmbedCalls == 0
                && record.LanguageModelInvoked
                && record.LlmContextDocuments.Count == 0
                && NoSchoolBLeak(record),
        },
    ];

    private static async Task<RagEvaluationRecord> EvaluateAsync(RagEvaluationFixture fixture, RagCase definition)
    {
        var (assistant, languageModel, embeddings) = fixture.CreateAssistant();
        var answer = await assistant.AskAsync(definition.User, definition.Question, definition.Options);
        var request = languageModel.Requests.SingleOrDefault();
        var formatted = request is null ? string.Empty : KnowledgeGroundedPrompt.FormatUserMessage(request);
        var record = new RagEvaluationRecord
        {
            Number = definition.Number,
            Category = definition.Category,
            Question = definition.Question,
            ExpectedSource = definition.ExpectedSource,
            ExpectedRuleId = definition.ExpectedRuleId,
            ExpectedScope = definition.ExpectedScope,
            Hits = answer.Sources,
            AuthorizedCandidateCount = answer.Retrieval.AuthorizedCandidateCount,
            LanguageModelInvoked = answer.LanguageModelInvoked,
            EmbedCalls = embeddings.EmbedCalls,
            LlmContextDocuments = request?.ContextDocuments.ToList() ?? [],
            FormattedLlmUserMessage = formatted,
            AnswerStub = answer.Answer,
        };
        record.Passed = definition.Judge(record);
        record.Result = DescribeResult(record);
        record.RetrievedSource = DescribeRetrieved(record);
        return record;
    }

    private static bool HasSource(RagEvaluationRecord record, params string[] names) =>
        record.Hits.Any(hit =>
            names.Any(name =>
                hit.FileName.Contains(name, StringComparison.OrdinalIgnoreCase)
                || hit.SourceIdentifier.Contains(name, StringComparison.OrdinalIgnoreCase)));

    private static bool HasRule(RagEvaluationRecord record, string ruleId) =>
        record.Hits.Any(hit =>
            string.Equals(hit.RuleId, ruleId, StringComparison.Ordinal)
            || hit.Content.Contains(ruleId, StringComparison.Ordinal));

    private static bool AllScopes(RagEvaluationRecord record, KnowledgeAuthorizationScope scope) =>
        record.Hits.Count > 0 && record.Hits.All(hit => hit.AuthorizationScope == scope);

    private static bool ContainsAny(RagEvaluationRecord record, params string[] terms) =>
        record.Hits.Any(hit =>
            terms.Any(term => hit.Content.Contains(term, StringComparison.Ordinal)));

    private static bool NoSchoolBLeak(RagEvaluationRecord record)
    {
        if (record.Hits.Any(hit =>
            hit.SchoolCode == "23306"
            || hit.FileName.Contains("23306", StringComparison.Ordinal)
            || hit.Content.Contains(RagEvaluationFixture.SchoolBSecret, StringComparison.Ordinal)))
        {
            return false;
        }

        if (record.LlmContextDocuments.Any(document =>
            document.Content.Contains(RagEvaluationFixture.SchoolBSecret, StringComparison.Ordinal)
            || document.FileName.Contains("23306", StringComparison.Ordinal)
            || document.SourceIdentifier.Contains("23306", StringComparison.Ordinal)
            || document.Content.Contains("Unique School B employment figure 77", StringComparison.Ordinal)))
        {
            return false;
        }

        return !record.FormattedLlmUserMessage.Contains(RagEvaluationFixture.SchoolBSecret, StringComparison.Ordinal)
            && !record.FormattedLlmUserMessage.Contains("Unique School B employment figure 77", StringComparison.Ordinal);
    }

    private static string DescribeRetrieved(RagEvaluationRecord record)
    {
        if (record.Hits.Count == 0)
        {
            return "(none)";
        }

        return string.Join(
            "; ",
            record.Hits.Select(hit =>
                $"{hit.FileName} ({hit.SourceLocation}, RuleId {hit.RuleId ?? "(none)"}, {hit.AuthorizationScope}, sim {hit.Similarity:0.000})"));
    }

    private static string DescribeResult(RagEvaluationRecord record)
    {
        var builder = new StringBuilder();
        builder.Append($"hits={record.Hits.Count}");
        builder.Append($"; candidates={record.AuthorizedCandidateCount}");
        builder.Append($"; LLM invoked={record.LanguageModelInvoked}");
        builder.Append($"; LLM context docs={record.LlmContextDocuments.Count}");
        builder.Append($"; School B in LLM context={SchoolBInLlm(record)}");
        if (record.Hits.Count == 0 && record.LanguageModelInvoked)
        {
            builder.Append("; LLM received empty authorized context");
        }

        return builder.ToString();
    }

    private static bool SchoolBInLlm(RagEvaluationRecord record) =>
        record.LlmContextDocuments.Any(document =>
            document.Content.Contains(RagEvaluationFixture.SchoolBSecret, StringComparison.Ordinal)
            || document.FileName.Contains("23306", StringComparison.Ordinal));

    private static async Task<bool> SchoolBIsIndexedAsync(RagEvaluationFixture fixture)
    {
        var chunks = await fixture.Db.KnowledgeChunks
            .AsNoTracking()
            .Include(chunk => chunk.KnowledgeDocument)
            .Where(chunk => chunk.KnowledgeDocument.SchoolCode == "23306")
            .ToListAsync();
        return chunks.Count > 0
            && chunks.All(chunk =>
                chunk.Embedding is { Length: > 0 }
                && chunk.Content.Contains(RagEvaluationFixture.SchoolBSecret, StringComparison.Ordinal));
    }

    private static string FailMessage(RagEvaluationRecord record) =>
        $"Case {record.Number} ({record.Category}) failed. Retrieved: {record.RetrievedSource}. Result: {record.Result}";

    private static string RenderMarkdown(
        RagEvaluationFixture fixture,
        IReadOnlyList<RagEvaluationRecord> records,
        TimeSpan elapsed)
    {
        var passed = records.Count(record => record.Passed);
        var builder = new StringBuilder();
        builder.AppendLine("# RAG evaluation");
        builder.AppendLine();
        builder.AppendLine("This document records an **observed** retrieval evaluation. It does not claim that generated PDFs are accessible. Report calculations stay in deterministic C#. Live OpenAI chat was not used (the provider previously returned HTTP 429).");
        builder.AppendLine();
        builder.AppendLine("## Method");
        builder.AppendLine();
        builder.AppendLine("The runner is `tests/AccessibleSchoolReports.UnitTests/Knowledge/RagEvaluationTests.cs`. This file is written to `evidence/test-results/rag-evaluation.md`.");
        builder.AppendLine();
        builder.AppendLine("| Piece | What ran |");
        builder.AppendLine("|---|---|");
        builder.AppendLine("| Corpus | Real `KnowledgeSourceCatalog` files ingested by `KnowledgeIngestionService` from this repository |");
        builder.AppendLine("| Generated reports | Two `GeneratedReport` documents (`10701` School A, `23306` School B) with distinctive page-1 text |");
        builder.AppendLine("| Embeddings | Deterministic lexical hashed bag-of-words (`LexicalEmbeddingService`). No network. |");
        builder.AppendLine("| Retrieval / authz | Production `KnowledgeRetrievalService` + `KnowledgeAccess` + `IReportAuthorizationService` |");
        builder.AppendLine("| Assistant | Production `KnowledgeAssistantService` + `KnowledgeGroundedPrompt` |");
        builder.AppendLine("| Language model | `FakeLanguageModelService` records the exact request. Completion text is a stub, not a live answer. |");
        builder.AppendLine("| Scoring | Top-K = 5, minimum similarity = 0.2 |");
        builder.AppendLine("| Pass rule | An expected source or RuleId appears **somewhere in top-K**, not only as rank 1. Security cases also require School B text absent from hits and from the formatted LLM user message. |");
        builder.AppendLine();
        builder.AppendLine("School B chunks were embedded **before** the School A user asked questions, so a leak would have been possible if authorization failed.");
        builder.AppendLine();
        builder.AppendLine("## Command");
        builder.AppendLine();
        builder.AppendLine("```text");
        builder.AppendLine("dotnet test tests/AccessibleSchoolReports.UnitTests/AccessibleSchoolReports.UnitTests.csproj --filter \"FullyQualifiedName~RagEvaluation\"");
        builder.AppendLine("```");
        builder.AppendLine();
        builder.AppendLine("## Run");
        builder.AppendLine();
        builder.AppendLine("| Field | Value |");
        builder.AppendLine("|---|---|");
        builder.AppendLine($"| Date | {DateTime.Now:yyyy-MM-dd} |");
        builder.AppendLine("| Host | Windows 10 (win32 10.0.26100) |");
        builder.AppendLine("| Project | `AccessibleSchoolReports.UnitTests` (`net8.0`) |");
        builder.AppendLine("| Filter | `FullyQualifiedName~RagEvaluation` |");
        builder.AppendLine($"| Cases | {records.Count} |");
        builder.AppendLine($"| Passed | {passed} |");
        builder.AppendLine($"| Failed | {records.Count - passed} |");
        builder.AppendLine($"| Evaluation duration | {elapsed.TotalSeconds:0.000} s |");
        builder.AppendLine($"| Documents indexed | {fixture.DocumentCount} |");
        builder.AppendLine($"| Chunks | {fixture.ChunkCount} |");
        builder.AppendLine($"| Chunks with embeddings | {fixture.EmbeddedChunkCount} |");
        builder.AppendLine($"| Ingestion indexed | {fixture.Ingestion.Indexed.Count} |");
        builder.AppendLine($"| Ingestion missing | {fixture.Ingestion.Missing.Count} |");
        builder.AppendLine($"| Embedding index chunks | {fixture.Index.ChunksIndexed} |");
        builder.AppendLine($"| Embedding index failures | {fixture.Index.Failures} |");
        builder.AppendLine();
        builder.AppendLine("## Results");
        builder.AppendLine();
        builder.AppendLine("| # | Category | Question | Expected source | Expected RuleId | Expected authorization scope | Retrieved source | Result | Pass/Fail |");
        builder.AppendLine("|---|---|---|---|---|---|---|---|---|");
        foreach (var record in records)
        {
            builder.Append("| ");
            builder.Append(record.Number);
            builder.Append(" | ");
            builder.Append(Escape(record.Category));
            builder.Append(" | ");
            builder.Append(Escape(record.Question));
            builder.Append(" | ");
            builder.Append(Escape(record.ExpectedSource));
            builder.Append(" | ");
            builder.Append(Escape(record.ExpectedRuleId));
            builder.Append(" | ");
            builder.Append(Escape(record.ExpectedScope));
            builder.Append(" | ");
            builder.Append(Escape(record.RetrievedSource));
            builder.Append(" | ");
            builder.Append(Escape(record.Result));
            builder.Append(" | ");
            builder.Append(record.Passed ? "**PASS**" : "**FAIL**");
            builder.AppendLine(" |");
        }

        builder.AppendLine();
        builder.AppendLine("## Security proof: School B is not passed to the LLM");
        builder.AppendLine();
        builder.AppendLine("Caller: ReportUser `user-a`, grant on School A (`10701`) only.");
        builder.AppendLine();
        builder.AppendLine($"School B marker that must never appear in retrieval hits or LLM context: `{RagEvaluationFixture.SchoolBSecret}`.");
        builder.AppendLine();
        var unauthorized = records.Where(record => record.Number is 10 or 11).ToList();
        builder.AppendLine("| Case | Hits contain School B | LLM context documents | School B secret in formatted LLM user message | Pass |");
        builder.AppendLine("|---|---|---|---|---|");
        foreach (var record in unauthorized)
        {
            var hitLeak = record.Hits.Any(hit =>
                hit.SchoolCode == "23306"
                || hit.Content.Contains(RagEvaluationFixture.SchoolBSecret, StringComparison.Ordinal));
            builder.Append("| ");
            builder.Append(record.Number);
            builder.Append(" | ");
            builder.Append(hitLeak ? "YES" : "no");
            builder.Append(" | ");
            builder.Append(record.LlmContextDocuments.Count);
            builder.Append(" (");
            builder.Append(SchoolBInLlm(record) ? "LEAK" : "no School B");
            builder.Append(") | ");
            builder.Append(record.FormattedLlmUserMessage.Contains(RagEvaluationFixture.SchoolBSecret, StringComparison.Ordinal) ? "YES" : "no");
            builder.Append(" | ");
            builder.Append(record.Passed ? "**PASS**" : "**FAIL**");
            builder.AppendLine(" |");
        }

        builder.AppendLine();
        builder.AppendLine("Case 10 asks about School B without a report id. Authenticated catalog chunks may be retrieved. Generated School B text must not be.");
        builder.AppendLine();
        builder.AppendLine("Case 11 sends School B `reportId`. `CanViewReportAsync` fails, retrieval returns empty **before** query embedding, and the grounded prompt contains `(no authorized context documents)`.");
        builder.AppendLine();
        builder.AppendLine("## Case notes");
        builder.AppendLine();
        foreach (var record in records)
        {
            builder.AppendLine($"### {record.Number}. {record.Category}");
            builder.AppendLine();
            builder.AppendLine($"- Question: {record.Question}");
            builder.AppendLine($"- Expected source: {record.ExpectedSource}");
            builder.AppendLine($"- Expected RuleId: {record.ExpectedRuleId}");
            builder.AppendLine($"- Expected scope: {record.ExpectedScope}");
            builder.AppendLine($"- Retrieved: {record.RetrievedSource}");
            builder.AppendLine($"- Result: {record.Result}");
            builder.AppendLine($"- Pass/Fail: {(record.Passed ? "PASS" : "FAIL")}");
            builder.AppendLine($"- LLM invoked: {record.LanguageModelInvoked}; completion stub: `{record.AnswerStub}`");
            builder.AppendLine();
        }

        builder.AppendLine("## Limitations");
        builder.AppendLine();
        builder.AppendLine("- Lexical embeddings are not `text-embedding-3-small`. Rankings can differ from a live embedding provider.");
        builder.AppendLine("- The language-model **answer text** is a test stub. This evaluation scores retrieval, authorization, and the grounded prompt payload.");
        builder.AppendLine("- Case 9 (insufficient evidence) uses the production 0.2 similarity floor. Hashed bag-of-words can still return weakly related catalog chunks. The case passes only when none of those chunks contain cafeteria / lunch-menu evidence.");
        builder.AppendLine("- Generated-report chunks are evaluation fixtures with the same `GeneratedReport` / `Report` shape as production PDF ingestion. They are not a live OpenAI-indexed working-database snapshot.");
        builder.AppendLine("- Do not treat this file as PDF/UA validation.");
        builder.AppendLine();
        return builder.ToString();
    }

    private static string Escape(string? value)
    {
        if (string.IsNullOrEmpty(value))
        {
            return string.Empty;
        }

        return value.Replace("|", "\\|", StringComparison.Ordinal).Replace("\r\n", " ", StringComparison.Ordinal).Replace('\n', ' ');
    }

    private sealed class RagCase
    {
        public required int Number { get; init; }
        public required string Category { get; init; }
        public required string Question { get; init; }
        public required string ExpectedSource { get; init; }
        public required string ExpectedRuleId { get; init; }
        public required string ExpectedScope { get; init; }
        public required System.Security.Claims.ClaimsPrincipal User { get; init; }
        public required KnowledgeRetrievalOptions Options { get; init; }
        public required Func<RagEvaluationRecord, bool> Judge { get; init; }
    }

    private sealed class RagEvaluationRecord
    {
        public required int Number { get; init; }
        public required string Category { get; init; }
        public required string Question { get; init; }
        public required string ExpectedSource { get; init; }
        public required string ExpectedRuleId { get; init; }
        public required string ExpectedScope { get; init; }
        public required IReadOnlyList<KnowledgeRetrievalHit> Hits { get; init; }
        public required int AuthorizedCandidateCount { get; init; }
        public required bool LanguageModelInvoked { get; init; }
        public required int EmbedCalls { get; init; }
        public required IReadOnlyList<LanguageModelContextDocument> LlmContextDocuments { get; init; }
        public required string FormattedLlmUserMessage { get; init; }
        public required string AnswerStub { get; init; }
        public string RetrievedSource { get; set; } = "(none)";
        public string Result { get; set; } = string.Empty;
        public bool Passed { get; set; }
    }
}
