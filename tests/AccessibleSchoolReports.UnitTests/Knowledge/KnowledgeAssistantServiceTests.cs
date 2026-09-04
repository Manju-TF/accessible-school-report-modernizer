using AccessibleSchoolReports.Application.Knowledge;
using AccessibleSchoolReports.Application.Security;
using AccessibleSchoolReports.Domain.Knowledge;
using AccessibleSchoolReports.Infrastructure.Knowledge;

namespace AccessibleSchoolReports.UnitTests.Knowledge;

public sealed class KnowledgeAssistantServiceTests
{
    private static readonly KnowledgeRetrievalOptions OpenOptions = new()
    {
        TopK = 10,
        MinimumSimilarity = 0.1f,
    };

    [Fact]
    public async Task PromptInjection_InSasComment_StaysInUntrustedContext()
    {
        await AssertInjectionIsUntrustedDataAsync(
            KnowledgeGroundedPromptTests.SasInjection,
            "cf200.sas",
            "legacy/sas/cf200.sas:8");
    }

    [Fact]
    public async Task PromptInjection_InMarkdown_StaysInUntrustedContext()
    {
        await AssertInjectionIsUntrustedDataAsync(
            KnowledgeGroundedPromptTests.MarkdownInjection,
            "notes.md",
            "docs/notes.md:3");
    }

    [Fact]
    public async Task PromptInjection_InPdfText_StaysInUntrustedContext()
    {
        await AssertInjectionIsUntrustedDataAsync(
            KnowledgeGroundedPromptTests.PdfInjection,
            "summary-report.pdf",
            "page 2");
    }

    [Fact]
    public async Task UserA_CannotSendSchoolBPdfContentToLanguageModel()
    {
        await using var fixture = await KnowledgeRetrievalTestFixture.CreateAsync();
        var (assistant, languageModel, _) = fixture.CreateAssistant();

        var answer = await assistant.AskAsync(
            KnowledgeRetrievalTestFixture.Principal("user-a", AppRoles.ReportUser),
            "school B employment outcomes",
            OpenOptions);

        Assert.True(answer.LanguageModelInvoked);
        Assert.Equal("grounded-answer", answer.Answer);
        Assert.DoesNotContain(answer.Sources, hit => hit.FileName == "b.pdf");
        var request = Assert.Single(languageModel.Requests);
        Assert.DoesNotContain(
            request.ContextDocuments,
            document => document.Content.Contains(KnowledgeRetrievalTestFixture.SchoolBSecret));
        Assert.False(languageModel.UsedNetwork);
    }

    [Fact]
    public async Task Viewer_CannotSendAdminOnlyKnowledgeToLanguageModel()
    {
        await using var fixture = await KnowledgeRetrievalTestFixture.CreateAsync();
        var (assistant, languageModel, _) = fixture.CreateAssistant();

        var answer = await assistant.AskAsync(
            KnowledgeRetrievalTestFixture.Principal("viewer-a", AppRoles.Viewer),
            "admin policy",
            OpenOptions);

        Assert.DoesNotContain(answer.Sources, hit => hit.AuthorizationScope == KnowledgeAuthorizationScope.Admin);
        var request = Assert.Single(languageModel.Requests);
        Assert.DoesNotContain(
            request.ContextDocuments,
            document => document.Content.Contains(KnowledgeRetrievalTestFixture.AdminSecret));
    }

    [Fact]
    public async Task UnauthorizedChunks_NeverReachTheLanguageModel()
    {
        await using var fixture = await KnowledgeRetrievalTestFixture.CreateAsync();
        var (assistant, languageModel, _) = fixture.CreateAssistant();

        var answer = await assistant.AskAsync(
            KnowledgeRetrievalTestFixture.Principal("user-a", AppRoles.ReportUser),
            "best matching secret",
            OpenOptions);

        Assert.True(answer.LanguageModelInvoked);
        Assert.Equal(1, languageModel.CompleteCalls);
        var sent = Assert.Single(languageModel.Requests).ContextDocuments;
        Assert.Equal(answer.Sources.Select(hit => hit.Content), sent.Select(document => document.Content));
        Assert.DoesNotContain(sent, document => document.Content.Contains(KnowledgeRetrievalTestFixture.SchoolBSecret));
        Assert.DoesNotContain(sent, document => document.Content.Contains(KnowledgeRetrievalTestFixture.AdminSecret));
    }

    [Fact]
    public async Task UserA_CanAskAboutAuthorizedSchoolAReport()
    {
        await using var fixture = await KnowledgeRetrievalTestFixture.CreateAsync();
        var (assistant, languageModel, _) = fixture.CreateAssistant();

        var answer = await assistant.AskAsync(
            KnowledgeRetrievalTestFixture.Principal("user-a", AppRoles.ReportUser),
            "employment",
            new KnowledgeRetrievalOptions
            {
                TopK = 10,
                MinimumSimilarity = 0.1f,
                ReportId = fixture.SchoolAReportId,
            });

        Assert.All(answer.Sources, hit => Assert.Equal(fixture.SchoolAReportId, hit.ReportId));
        Assert.DoesNotContain(
            languageModel.Requests[0].ContextDocuments,
            document => document.Content.Contains(KnowledgeRetrievalTestFixture.SchoolBSecret));
    }

    [Fact]
    public async Task UserA_CannotAskAboutSchoolBReport()
    {
        await using var fixture = await KnowledgeRetrievalTestFixture.CreateAsync();
        var (assistant, languageModel, embeddings) = fixture.CreateAssistant();

        var answer = await assistant.AskAsync(
            KnowledgeRetrievalTestFixture.Principal("user-a", AppRoles.ReportUser),
            "employment",
            new KnowledgeRetrievalOptions
            {
                TopK = 10,
                MinimumSimilarity = 0.1f,
                ReportId = fixture.SchoolBReportId,
            });

        Assert.Empty(answer.Sources);
        Assert.DoesNotContain(
            languageModel.Requests.SelectMany(request => request.ContextDocuments),
            document => document.Content.Contains(KnowledgeRetrievalTestFixture.SchoolBSecret));
        Assert.Equal(0, embeddings.EmbedCalls);
        Assert.DoesNotContain(fixture.SchoolBReportId.ToString(), answer.Answer, StringComparison.Ordinal);
        Assert.DoesNotContain("23306", answer.Answer, StringComparison.Ordinal);
    }

    [Fact]
    public async Task AnonymousUser_DoesNotCallLanguageModel()
    {
        await using var fixture = await KnowledgeRetrievalTestFixture.CreateAsync();
        var (assistant, languageModel, embeddings) = fixture.CreateAssistant();

        var answer = await assistant.AskAsync(
            new(new System.Security.Claims.ClaimsIdentity()),
            "anything",
            OpenOptions);

        Assert.False(answer.LanguageModelInvoked);
        Assert.Empty(answer.Sources);
        Assert.Equal(0, languageModel.CompleteCalls);
        Assert.Equal(0, embeddings.EmbedCalls);
    }

    [Fact]
    public async Task Admin_CanAskAboutAllAuthorizedReports()
    {
        await using var fixture = await KnowledgeRetrievalTestFixture.CreateAsync();
        var (assistant, languageModel, _) = fixture.CreateAssistant();

        var answer = await assistant.AskAsync(
            KnowledgeRetrievalTestFixture.Principal("admin", AppRoles.Admin),
            "all school reports",
            OpenOptions);

        Assert.Contains(answer.Sources, hit => hit.ChunkId == fixture.SchoolAChunkId);
        Assert.Contains(answer.Sources, hit => hit.ChunkId == fixture.SchoolBChunkId);
        Assert.Contains(
            languageModel.Requests[0].ContextDocuments,
            document => document.Content.Contains(KnowledgeRetrievalTestFixture.SchoolBSecret));
        Assert.Equal("Fake", answer.Provider);
        Assert.Equal("test-chat", answer.Model);
    }

    [Fact]
    public async Task ReturnsAnswerAndSources_WithoutLiveProvider()
    {
        var retrieval = new StubKnowledgeRetrievalService
        {
            Next = new KnowledgeRetrievalResult
            {
                Hits =
                [
                    new KnowledgeRetrievalHit
                    {
                        ChunkId = 9,
                        DocumentId = 3,
                        Content = "Salary suppression uses n ge 5. RuleId CF-S-00.",
                        RuleId = "CF-S-00",
                        SourceLocation = "legacy/sas/cf200.sas:20",
                        SourceIdentifier = "cf200.sas",
                        FileName = "cf200.sas",
                        DocumentType = KnowledgeDocumentType.Legacy,
                        AuthorizationScope = KnowledgeAuthorizationScope.Authenticated,
                        Similarity = 0.88f,
                    },
                ],
                AuthorizedCandidateCount = 1,
                Duration = TimeSpan.FromMilliseconds(4),
            },
        };
        var languageModel = new FakeLanguageModelService { Answer = "Suppression is n ge 5 (CF-S-00). Source: cf200.sas." };
        var assistant = new KnowledgeAssistantService(retrieval, languageModel);

        var answer = await assistant.AskAsync(
            KnowledgeRetrievalTestFixture.Principal("admin", AppRoles.Admin),
            "What is the salary suppression rule?");

        Assert.Equal(languageModel.Answer, answer.Answer);
        var source = Assert.Single(answer.Sources);
        Assert.Equal("CF-S-00", source.RuleId);
        Assert.Equal("cf200.sas", source.FileName);
        Assert.False(languageModel.UsedNetwork);
        Assert.Equal(KnowledgeGroundedPrompt.SystemInstructions, languageModel.Requests[0].SystemInstructions);
    }

    private static async Task AssertInjectionIsUntrustedDataAsync(
        string injection,
        string fileName,
        string location)
    {
        var retrieval = new StubKnowledgeRetrievalService
        {
            Next = new KnowledgeRetrievalResult
            {
                Hits =
                [
                    new KnowledgeRetrievalHit
                    {
                        ChunkId = 1,
                        DocumentId = 1,
                        Content = injection,
                        RuleId = "CF-S-00",
                        SourceLocation = location,
                        SourceIdentifier = fileName,
                        FileName = fileName,
                        DocumentType = KnowledgeDocumentType.Legacy,
                        AuthorizationScope = KnowledgeAuthorizationScope.Authenticated,
                        Similarity = 0.95f,
                    },
                ],
                AuthorizedCandidateCount = 1,
                Duration = TimeSpan.Zero,
            },
        };
        var languageModel = new FakeLanguageModelService();
        var assistant = new KnowledgeAssistantService(retrieval, languageModel);

        var answer = await assistant.AskAsync(
            KnowledgeRetrievalTestFixture.Principal("admin", AppRoles.Admin),
            "What does this document say?");

        Assert.True(answer.LanguageModelInvoked);
        Assert.False(languageModel.UsedNetwork);
        var request = Assert.Single(languageModel.Requests);
        Assert.Equal(KnowledgeGroundedPrompt.SystemInstructions, request.SystemInstructions);
        Assert.False(FakeLanguageModelService.ContainsInjection(request.SystemInstructions));
        Assert.Contains(injection, Assert.Single(request.ContextDocuments).Content, StringComparison.Ordinal);
        var user = KnowledgeGroundedPrompt.FormatUserMessage(request);
        Assert.Contains(KnowledgeGroundedPrompt.UntrustedBegin, user, StringComparison.Ordinal);
        Assert.True(user.IndexOf(injection, StringComparison.Ordinal)
            > user.IndexOf(KnowledgeGroundedPrompt.UntrustedBegin, StringComparison.Ordinal));
        Assert.Equal(fileName, Assert.Single(answer.Sources).FileName);
    }
}
