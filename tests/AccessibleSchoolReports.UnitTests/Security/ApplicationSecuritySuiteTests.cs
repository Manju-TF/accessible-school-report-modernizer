using System.Diagnostics;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using AccessibleSchoolReports.Application.Knowledge;
using AccessibleSchoolReports.Application.Security;
using AccessibleSchoolReports.Domain.Knowledge;
using AccessibleSchoolReports.Infrastructure.Knowledge;
using AccessibleSchoolReports.Infrastructure.LanguageModels;
using AccessibleSchoolReports.Infrastructure.Security;
using AccessibleSchoolReports.UnitTests.Knowledge;
using AccessibleSchoolReports.Web.Security;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace AccessibleSchoolReports.UnitTests.Security;

[Collection(SecurityCollection.Name)]
public sealed class ApplicationSecuritySuiteTests : IClassFixture<ReportDownloadWebApplicationFactory>
{
    private readonly SecurityWebApplicationFactory _auth;
    private readonly ReportDownloadWebApplicationFactory _reports;

    public ApplicationSecuritySuiteTests(
        SecurityWebApplicationFactory auth,
        ReportDownloadWebApplicationFactory reports)
    {
        _auth = auth;
        _reports = reports;
    }

    [Fact]
    public async Task Case01_AnonymousUserCannotAccessProtectedPages()
    {
        var client = AuthTestHttp.CreateClient(_auth);
        foreach (var path in new[]
        {
            "/", "/import", "/generate", "/generate-all", "/runs",
            "/knowledge-assistant", "/reports/1", "/downloads/reports/1",
        })
        {
            var response = await client.GetAsync(path);
            Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
            Assert.Contains("/signin", response.Headers.Location?.ToString(), StringComparison.OrdinalIgnoreCase);
        }
    }

    [Fact]
    public async Task Case02_ValidUserCanAuthenticate()
    {
        var client = AuthTestHttp.CreateClient(_auth);
        var signedIn = await AuthTestHttp.SignInAsync(
            client,
            SecurityWebApplicationFactory.TestUserName,
            SecurityWebApplicationFactory.TestPassword);

        Assert.Equal(HttpStatusCode.Redirect, signedIn.StatusCode);
        Assert.Contains(
            IdentityAuthenticationExtensions.AuthCookieName,
            signedIn.Headers.GetValues("Set-Cookie").First());
        Assert.Equal(HttpStatusCode.OK, (await client.GetAsync("/")).StatusCode);
    }

    [Fact]
    public async Task Case03_InvalidCredentialsAreRejected()
    {
        var client = AuthTestHttp.CreateClient(_auth);
        var response = await AuthTestHttp.SignInAsync(
            client,
            SecurityWebApplicationFactory.TestUserName,
            "Wrong-Password-1!");

        Assert.Contains("/signin?error=1", response.Headers.Location?.ToString(), StringComparison.OrdinalIgnoreCase);
        Assert.Equal(HttpStatusCode.Redirect, (await client.GetAsync("/")).StatusCode);
    }

    [Fact]
    public async Task Case04_LogoutInvalidatesAuthenticatedAccess()
    {
        var client = AuthTestHttp.CreateClient(_auth);
        await AuthTestHttp.SignInAsync(
            client,
            SecurityWebApplicationFactory.TestUserName,
            SecurityWebApplicationFactory.TestPassword);
        Assert.Equal(HttpStatusCode.OK, (await client.GetAsync("/")).StatusCode);

        await AuthTestHttp.SignOutAsync(client);
        var after = await client.GetAsync("/");
        Assert.Equal(HttpStatusCode.Redirect, after.StatusCode);
        Assert.Contains("/signin", after.Headers.Location?.ToString(), StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task Case05_ProtectedEndpointsRequireAuthentication()
    {
        var client = AuthTestHttp.CreateClient(_reports);
        var download = await client.GetAsync($"/downloads/reports/{_reports.ReportAId}");
        Assert.Equal(HttpStatusCode.Redirect, download.StatusCode);
        Assert.Contains("/signin", download.Headers.Location?.ToString(), StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task Case06_ViewerCannotAccessAdminFunctions()
    {
        var client = await SignInAsync(_auth, SecurityWebApplicationFactory.ViewerUserName);
        AssertDeniedToDeniedPage(await client.GetAsync("/import"));
        AssertDeniedToDeniedPage(await client.GetAsync("/generate-all"));
    }

    [Fact]
    public async Task Case07_ReportUserCannotAccessAdminFunctions()
    {
        var client = await SignInAsync(_auth, SecurityWebApplicationFactory.ReportUserName);
        AssertDeniedToDeniedPage(await client.GetAsync("/import"));
        AssertDeniedToDeniedPage(await client.GetAsync("/generate-all"));
    }

    [Fact]
    public async Task Case08_AdminCanAccessAdminFunctions()
    {
        var client = await SignInAsync(_auth, SecurityWebApplicationFactory.AdminUserName);
        Assert.Equal(HttpStatusCode.OK, (await client.GetAsync("/import")).StatusCode);
        Assert.Equal(HttpStatusCode.OK, (await client.GetAsync("/generate-all")).StatusCode);
    }

    [Fact]
    public async Task Case09_UserCannotAccessAnUnauthorizedSchool()
    {
        var client = await SignInAsync(_reports, ReportDownloadWebApplicationFactory.ViewerUserName);
        var other = await client.GetAsync($"/reports/{_reports.ReportBId}");
        var html = await other.Content.ReadAsStringAsync();
        Assert.Contains("That report is not available.", html, StringComparison.Ordinal);
        Assert.DoesNotContain(ReportDownloadWebApplicationFactory.SchoolBName, html, StringComparison.Ordinal);
        Assert.DoesNotContain("23306", html, StringComparison.Ordinal);
    }

    [Fact]
    public async Task Case10_UserCannotAccessAnUnauthorizedReport()
    {
        var client = await SignInAsync(_reports, ReportDownloadWebApplicationFactory.ReportUserName);
        var other = await client.GetAsync($"/downloads/reports/{_reports.ReportBId}");
        var body = await other.Content.ReadAsStringAsync();
        Assert.Equal(HttpStatusCode.NotFound, other.StatusCode);
        Assert.DoesNotContain(ReportDownloadWebApplicationFactory.SchoolBName, body, StringComparison.Ordinal);
        Assert.DoesNotContain(_reports.OutputRoot, body, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task Case11_UnauthorizedPdfDownloadIsDenied()
    {
        var client = await SignInAsync(_reports, ReportDownloadWebApplicationFactory.ViewerUserName);
        var response = await client.GetAsync($"/downloads/reports/{_reports.ReportBId}");
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        Assert.NotEqual("application/pdf", response.Content.Headers.ContentType?.MediaType);
    }

    [Fact]
    public async Task Case12_PathTraversalIsDenied()
    {
        var client = await SignInAsync(_reports, ReportDownloadWebApplicationFactory.AdminUserName);
        foreach (var url in new[]
        {
            "/downloads/reports/..%2F..%2Fsecret.pdf",
            "/downloads/reports/%2e%2e/%2e%2e/secret.pdf",
            $"/downloads/reports/{_reports.TraversalStoredReportId}",
        })
        {
            var response = await client.GetAsync(url);
            var body = await response.Content.ReadAsStringAsync();
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
            Assert.DoesNotContain(_reports.OutputRoot, body, StringComparison.OrdinalIgnoreCase);
        }
    }

    [Fact]
    public async Task Case13_PhysicalOutputDirectoryIsNotPubliclyBrowsable()
    {
        var client = await SignInAsync(_reports, ReportDownloadWebApplicationFactory.AdminUserName);
        var response = await client.GetAsync("/output/2025/10701/summary-report.pdf");
        var body = await response.Content.ReadAsByteArrayAsync();
        Assert.NotEqual("application/pdf", response.Content.Headers.ContentType?.MediaType);
        Assert.False(body.Length >= 4 && Encoding.ASCII.GetString(body, 0, 4) == "%PDF");
    }

    [Fact]
    public async Task Case14_InvalidReportIdsDoNotRevealFilesystemInformation()
    {
        var client = await SignInAsync(_reports, ReportDownloadWebApplicationFactory.AdminUserName);
        var response = await client.GetAsync("/downloads/reports/99999");
        var body = await response.Content.ReadAsStringAsync();
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        Assert.DoesNotContain(_reports.OutputRoot, body, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("summary-report.pdf", body, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain(ReportDownloadWebApplicationFactory.SchoolAName, body, StringComparison.Ordinal);
    }

    [Fact]
    public async Task Case15_UnauthorizedKnowledgeChunksAreNeverRetrieved()
    {
        await using var fixture = await KnowledgeRetrievalTestFixture.CreateAsync();
        var (service, _) = fixture.CreateSut();
        var result = await service.RetrieveAsync(
            KnowledgeRetrievalTestFixture.Principal("user-a", AppRoles.ReportUser),
            "best matching secret",
            OpenOptions());

        Assert.DoesNotContain(result.Hits, hit => hit.ChunkId == fixture.AdminChunkId);
        Assert.DoesNotContain(result.Hits, hit => hit.Content.Contains(KnowledgeRetrievalTestFixture.AdminSecret));
    }

    [Fact]
    public async Task Case16_UnauthorizedPdfChunksAreNeverRetrieved()
    {
        await using var fixture = await KnowledgeRetrievalTestFixture.CreateAsync();
        var (service, _) = fixture.CreateSut();
        var result = await service.RetrieveAsync(
            KnowledgeRetrievalTestFixture.Principal("user-a", AppRoles.ReportUser),
            "employment",
            OpenOptions());

        Assert.DoesNotContain(result.Hits, hit => hit.ChunkId == fixture.SchoolBChunkId);
        Assert.DoesNotContain(result.Hits, hit => hit.Content.Contains(KnowledgeRetrievalTestFixture.SchoolBSecret));
    }

    [Fact]
    public async Task Case17_UnauthorizedContentNeverReachesTheLlm()
    {
        await using var fixture = await KnowledgeRetrievalTestFixture.CreateAsync();
        var (assistant, languageModel, _) = fixture.CreateAssistant();
        await assistant.AskAsync(
            KnowledgeRetrievalTestFixture.Principal("user-a", AppRoles.ReportUser),
            "best matching secret",
            OpenOptions());

        Assert.DoesNotContain(
            languageModel.Requests.SelectMany(request => request.ContextDocuments),
            document => document.Content.Contains(KnowledgeRetrievalTestFixture.SchoolBSecret)
                || document.Content.Contains(KnowledgeRetrievalTestFixture.AdminSecret));
    }

    [Fact]
    public async Task Case18_UserACannotQueryUserBAuthorizedReports()
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
        Assert.Equal(0, embeddings.EmbedCalls);
        Assert.DoesNotContain(
            languageModel.Requests.SelectMany(request => request.ContextDocuments),
            document => document.Content.Contains(KnowledgeRetrievalTestFixture.SchoolBSecret));
    }

    [Fact]
    public async Task Case19_SchoolAUserCannotRetrieveSchoolBContent()
    {
        await using var fixture = await KnowledgeRetrievalTestFixture.CreateAsync();
        var (service, _) = fixture.CreateSut();
        var result = await service.RetrieveAsync(
            KnowledgeRetrievalTestFixture.Principal("user-a", AppRoles.ReportUser),
            "salary suppression",
            OpenOptions());

        Assert.DoesNotContain(result.Hits, hit => hit.SchoolId == fixture.SchoolBId);
        Assert.DoesNotContain(result.Hits, hit => hit.Content.Contains(KnowledgeRetrievalTestFixture.SchoolBSecret));
    }

    [Fact]
    public async Task Case20_ReportSpecificRagCannotBeEscapedByChangingReportId()
    {
        await using var fixture = await KnowledgeRetrievalTestFixture.CreateAsync();
        var session = new KnowledgeAssistantSession(
            fixture.Db,
            new ReportAuthorizationService(fixture.Db));
        var user = KnowledgeRetrievalTestFixture.Principal("user-a", AppRoles.ReportUser);
        Assert.True(await session.TrySelectReportAsync(user, fixture.SchoolAReportId));
        Assert.False(await session.TrySelectReportAsync(user, fixture.SchoolBReportId));
        Assert.Null(session.Context);

        var (service, embeddings) = fixture.CreateSut();
        var tampered = await service.RetrieveAsync(
            user,
            "employment",
            new KnowledgeRetrievalOptions { ReportId = fixture.SchoolBReportId, TopK = 10, MinimumSimilarity = 0.1f });
        Assert.Empty(tampered.Hits);
        Assert.Equal(0, embeddings.EmbedCalls);
    }

    [Theory]
    [InlineData("sas")]
    [InlineData("markdown")]
    [InlineData("pdf")]
    public async Task Case21To23_MaliciousRetrievedTextCannotOverrideSystemInstructions(string kind)
    {
        var injection = kind switch
        {
            "sas" => KnowledgeGroundedPromptTests.SasInjection,
            "markdown" => KnowledgeGroundedPromptTests.MarkdownInjection,
            _ => KnowledgeGroundedPromptTests.PdfInjection,
        };
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
                        SourceLocation = "source",
                        SourceIdentifier = "doc",
                        FileName = "doc",
                        DocumentType = KnowledgeDocumentType.Legacy,
                        AuthorizationScope = KnowledgeAuthorizationScope.Authenticated,
                        Similarity = 0.9f,
                    },
                ],
                AuthorizedCandidateCount = 1,
                Duration = TimeSpan.Zero,
            },
        };
        var languageModel = new FakeLanguageModelService();
        var assistant = new KnowledgeAssistantService(retrieval, languageModel);

        await assistant.AskAsync(
            KnowledgeRetrievalTestFixture.Principal("admin", AppRoles.Admin),
            "What does this document say?");

        var request = Assert.Single(languageModel.Requests);
        Assert.Equal(KnowledgeGroundedPrompt.SystemInstructions, request.SystemInstructions);
        Assert.False(FakeLanguageModelService.ContainsInjection(request.SystemInstructions));
        Assert.Contains(injection, Assert.Single(request.ContextDocuments).Content, StringComparison.Ordinal);
    }

    [Fact]
    public void Case24_ApiKeysAreNotInSourceCode()
    {
        var webRoot = Path.Combine(FindRepositoryRoot(), "src", "AccessibleSchoolReports.Web");
        var appsettings = Path.Combine(webRoot, "appsettings.json");
        var development = Path.Combine(webRoot, "appsettings.Development.json");
        Assert.True(File.Exists(appsettings));
        using var json = JsonDocument.Parse(File.ReadAllText(appsettings));
        Assert.Equal(string.Empty, json.RootElement.GetProperty("Embeddings").GetProperty("ApiKey").GetString());
        Assert.Equal(string.Empty, json.RootElement.GetProperty("LanguageModel").GetProperty("ApiKey").GetString());
        if (File.Exists(development))
        {
            Assert.DoesNotContain("ApiKey", File.ReadAllText(development), StringComparison.Ordinal);
        }

        foreach (var file in Directory.EnumerateFiles(Path.Combine(webRoot, "wwwroot"), "*", SearchOption.AllDirectories))
        {
            var text = File.ReadAllText(file);
            Assert.DoesNotContain("ApiKey", text, StringComparison.Ordinal);
            Assert.DoesNotContain("sk-", text, StringComparison.Ordinal);
        }
    }

    [Fact]
    public async Task Case25_ApiKeysAreNotLogged()
    {
        var embedding = new EmbeddingOptions { ApiKey = "super-secret-key", Model = "test" };
        var language = new LanguageModelOptions { ApiKey = "super-secret-key", Model = "test" };
        Assert.DoesNotContain("super-secret-key", embedding.ToString(), StringComparison.Ordinal);
        Assert.DoesNotContain("super-secret-key", language.ToString(), StringComparison.Ordinal);

        var handler = new ScriptedHandler();
        handler.EnqueueSuccess("ok");
        var logger = new ListLogger();
        var service = new OpenAiCompatibleLanguageModelService(
            new HttpClient(handler) { Timeout = TimeSpan.FromSeconds(10) },
            Options.Create(SuiteLanguageModel()),
            logger);
        await service.CompleteAsync(KnowledgeGroundedPrompt.Create("log check", []));
        Assert.NotEmpty(logger.Messages);
        Assert.All(
            logger.Messages,
            message =>
            {
                Assert.DoesNotContain("super-secret-key", message, StringComparison.Ordinal);
                Assert.DoesNotContain("Bearer ", message, StringComparison.Ordinal);
            });
    }

    [Fact]
    public async Task Case26_ApiKeysAreNotExposedToBrowserClientCode()
    {
        var client = await SignInAsync(_auth, SecurityWebApplicationFactory.ViewerUserName);
        foreach (var path in new[] { "/", "/knowledge-assistant", "/signin", "/app.css" })
        {
            var html = await (await client.GetAsync(path)).Content.ReadAsStringAsync();
            Assert.DoesNotContain("LanguageModel:ApiKey", html, StringComparison.Ordinal);
            Assert.DoesNotContain("Embeddings:ApiKey", html, StringComparison.Ordinal);
            Assert.DoesNotContain("Bearer ", html, StringComparison.Ordinal);
        }
    }

    [Fact]
    public void Case27_SecretConfigurationIsExcludedFromGit()
    {
        var repoRoot = FindRepositoryRoot();
        var gitignore = File.ReadAllText(Path.Combine(repoRoot, ".gitignore"));
        Assert.Contains(".env", gitignore, StringComparison.Ordinal);
        Assert.Contains("secrets.json", gitignore, StringComparison.Ordinal);
        Assert.Contains("appsettings.Local.json", gitignore, StringComparison.Ordinal);
        Assert.False(File.Exists(Path.Combine(repoRoot, ".env")));
        Assert.False(File.Exists(Path.Combine(repoRoot, "secrets.json")));
        Assert.False(File.Exists(Path.Combine(repoRoot, "src", "AccessibleSchoolReports.Web", "secrets.json")));

        foreach (var path in new[] { ".env", "secrets.json", "appsettings.Local.json" })
        {
            Assert.True(
                IsGitIgnored(repoRoot, path),
                $"Expected Git to ignore '{path}'.");
        }
    }

    [Fact]
    public async Task Case28_EmptyQuestionIsRejected()
    {
        await using var fixture = await KnowledgeRetrievalTestFixture.CreateAsync();
        var (assistant, languageModel, embeddings) = fixture.CreateAssistant();
        var answer = await assistant.AskAsync(
            KnowledgeRetrievalTestFixture.Principal("admin", AppRoles.Admin),
            "   ");

        Assert.False(answer.LanguageModelInvoked);
        Assert.Equal(0, languageModel.CompleteCalls);
        Assert.Equal(0, embeddings.EmbedCalls);
        Assert.Empty(answer.Sources);
    }

    [Fact]
    public async Task Case29_ExcessivelyLongQuestionIsHandledSafely()
    {
        await using var fixture = await KnowledgeRetrievalTestFixture.CreateAsync();
        var (assistant, languageModel, embeddings) = fixture.CreateAssistant();
        var answer = await assistant.AskAsync(
            KnowledgeRetrievalTestFixture.Principal("admin", AppRoles.Admin),
            new string('q', KnowledgeRetrievalOptions.MaxQuestionLength + 1));

        Assert.False(answer.LanguageModelInvoked);
        Assert.Equal(0, languageModel.CompleteCalls);
        Assert.Equal(0, embeddings.EmbedCalls);
        Assert.Empty(answer.Sources);
    }

    [Fact]
    public async Task Case30_CancellationIsHandled()
    {
        await using var fixture = await KnowledgeRetrievalTestFixture.CreateAsync();
        var (assistant, languageModel, embeddings) = fixture.CreateAssistant();
        using var cts = new CancellationTokenSource();
        await cts.CancelAsync();

        await Assert.ThrowsAnyAsync<OperationCanceledException>(() =>
            assistant.AskAsync(
                KnowledgeRetrievalTestFixture.Principal("admin", AppRoles.Admin),
                "How is salary suppression handled?",
                OpenOptions(),
                cts.Token));
        Assert.Equal(0, languageModel.CompleteCalls);
        Assert.Equal(0, embeddings.EmbedCalls);
    }

    [Fact]
    public async Task Case31_ExternalApiTimeoutIsHandled()
    {
        var options = SuiteLanguageModel();
        options.TimeoutSeconds = 1;
        var handler = new ScriptedHandler { Delay = TimeSpan.FromSeconds(5) };
        handler.EnqueueSuccess("late");
        var service = new OpenAiCompatibleLanguageModelService(
            new HttpClient(handler) { Timeout = TimeSpan.FromSeconds(10) },
            Options.Create(options),
            new ListLogger());

        await Assert.ThrowsAsync<LanguageModelTimeoutException>(
            () => service.CompleteAsync(KnowledgeGroundedPrompt.Create("timeout", [])));
    }

    [Fact]
    public async Task Case32_ExternalApiFailureIsHandled()
    {
        var handler = new ScriptedHandler();
        handler.Enqueue(HttpStatusCode.BadRequest, """{"error":"bad"}""");
        var service = new OpenAiCompatibleLanguageModelService(
            new HttpClient(handler) { Timeout = TimeSpan.FromSeconds(10) },
            Options.Create(SuiteLanguageModel()),
            new ListLogger());

        var exception = await Assert.ThrowsAsync<LanguageModelProviderException>(
            () => service.CompleteAsync(KnowledgeGroundedPrompt.Create("fail", [])));
        Assert.Equal(400, exception.StatusCode);
        Assert.Single(handler.Requests);
    }

    private static KnowledgeRetrievalOptions OpenOptions() =>
        new() { TopK = 10, MinimumSimilarity = 0.1f };

    private static LanguageModelOptions SuiteLanguageModel() =>
        new()
        {
            Provider = "OpenAICompatible",
            Endpoint = "https://llm.test/v1/chat/completions",
            Model = "gpt-4o-mini",
            TimeoutSeconds = 5,
            MaxRetries = 0,
            ApiKey = "super-secret-key",
        };

    private static string FindRepositoryRoot()
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);
        while (directory is not null)
        {
            if (File.Exists(Path.Combine(directory.FullName, ".gitignore"))
                && Directory.Exists(Path.Combine(directory.FullName, "src")))
            {
                return directory.FullName;
            }

            directory = directory.Parent;
        }

        throw new InvalidOperationException("Could not locate the repository root from the test output directory.");
    }

    private static bool IsGitIgnored(string repoRoot, string relativePath)
    {
        var start = new ProcessStartInfo("git", $"check-ignore -q -- {relativePath}")
        {
            WorkingDirectory = repoRoot,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true,
        };
        using var process = Process.Start(start);
        Assert.NotNull(process);
        process.WaitForExit(10_000);
        return process.ExitCode == 0;
    }

    private static async Task<HttpClient> SignInAsync(WebApplicationFactory<Program> factory, string userName)
    {
        var client = AuthTestHttp.CreateClient(factory);
        await AuthTestHttp.SignInAsync(client, userName, SecurityWebApplicationFactory.TestPassword);
        return client;
    }

    private static void AssertDeniedToDeniedPage(HttpResponseMessage response)
    {
        Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
        Assert.Contains("/denied", response.Headers.Location?.ToString(), StringComparison.OrdinalIgnoreCase);
    }

    private sealed class ScriptedHandler : HttpMessageHandler
    {
        private readonly Queue<Func<HttpRequestMessage, HttpResponseMessage>> _responses = new();

        public List<HttpRequestMessage> Requests { get; } = [];

        public TimeSpan Delay { get; set; }

        public void EnqueueSuccess(string text)
        {
            var escaped = text.Replace("\"", "\\\"", StringComparison.Ordinal);
            Enqueue(
                HttpStatusCode.OK,
                $"{{\"choices\":[{{\"message\":{{\"role\":\"assistant\",\"content\":\"{escaped}\"}}}}]}}");
        }

        public void Enqueue(HttpStatusCode status, string body)
        {
            _responses.Enqueue(_ => new HttpResponseMessage(status)
            {
                Content = new StringContent(body, Encoding.UTF8, "application/json"),
            });
        }

        protected override async Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            if (Delay > TimeSpan.Zero)
            {
                await Task.Delay(Delay, cancellationToken);
            }

            Requests.Add(request);
            return _responses.Dequeue()(request);
        }
    }

    private sealed class ListLogger : ILogger<OpenAiCompatibleLanguageModelService>
    {
        public List<string> Messages { get; } = [];

        public IDisposable BeginScope<TState>(TState state)
            where TState : notnull => NullScope.Instance;

        public bool IsEnabled(LogLevel logLevel) => true;

        public void Log<TState>(
            LogLevel logLevel,
            EventId eventId,
            TState state,
            Exception? exception,
            Func<TState, Exception?, string> formatter)
        {
            Messages.Add(formatter(state, exception));
            if (exception is not null)
            {
                Messages.Add(exception.ToString());
            }
        }

        private sealed class NullScope : IDisposable
        {
            public static readonly NullScope Instance = new();

            public void Dispose()
            {
            }
        }
    }
}
