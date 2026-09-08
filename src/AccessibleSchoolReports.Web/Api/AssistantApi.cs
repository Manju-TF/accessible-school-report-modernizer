using AccessibleSchoolReports.Application.Knowledge;
using AccessibleSchoolReports.Application.Security;
using AccessibleSchoolReports.Infrastructure.Embeddings;
using AccessibleSchoolReports.Infrastructure.LanguageModels;
using AccessibleSchoolReports.Web.Ui;

namespace AccessibleSchoolReports.Web.Api;

public static class AssistantApi
{
    public static void MapAssistantApi(this WebApplication app)
    {
        app.MapGet("/api/assistant/suggestions", (
                bool reportScoped = false) =>
            {
                var groups = AssistantSuggestions.ForScope(reportScoped)
                    .Select(group => new
                    {
                        title = group.Title,
                        hint = group.Hint,
                        questions = group.Questions,
                    });
                return Results.Json(new { groups }, ApiConventions.Json);
            })
            .RequireAuthorization(AppPolicies.RequireRagAccess);

        app.MapGet("/api/assistant/context", GetContextAsync)
            .RequireAuthorization(AppPolicies.RequireRagAccess);

        app.MapPost("/api/assistant/ask", AskAsync)
            .RequireAuthorization(AppPolicies.RequireRagAccess);
    }

    private static async Task<IResult> GetContextAsync(
        int? report,
        HttpContext http,
        IKnowledgeAssistantSession session,
        CancellationToken cancellationToken)
    {
        if (report is null)
        {
            session.Clear();
            return Results.Json(new { report = (object?)null }, ApiConventions.Json);
        }

        if (!await session.TrySelectReportAsync(http.User, report.Value, cancellationToken))
        {
            return Results.Json(
                new { error = "That report is not available." },
                ApiConventions.Json,
                statusCode: StatusCodes.Status404NotFound);
        }

        var context = session.Context!;
        return Results.Json(
            new
            {
                report = new
                {
                    reportId = context.ReportId,
                    schoolCode = context.SchoolCode,
                    schoolName = context.SchoolName,
                    schoolLabel = UiFormat.SchoolLabel(context.SchoolCode, context.SchoolName),
                    reportYear = context.ReportYear,
                },
            },
            ApiConventions.Json);
    }

    private static async Task<IResult> AskAsync(
        AskRequest body,
        HttpContext http,
        IKnowledgeAssistantService assistant,
        IKnowledgeAssistantSession session,
        CancellationToken cancellationToken)
    {
        var question = body.Question?.Trim() ?? string.Empty;
        if (string.IsNullOrWhiteSpace(question))
        {
            return Results.Json(new { error = "Enter a question before asking." }, ApiConventions.Json, statusCode: StatusCodes.Status400BadRequest);
        }

        if (question.Length > KnowledgeRetrievalOptions.MaxQuestionLength)
        {
            return Results.Json(
                new { error = $"Question must be {KnowledgeRetrievalOptions.MaxQuestionLength} characters or fewer." },
                ApiConventions.Json,
                statusCode: StatusCodes.Status400BadRequest);
        }

        int? reportId = null;
        if (body.ReportId is int requested)
        {
            if (!await session.TrySelectReportAsync(http.User, requested, cancellationToken))
            {
                return Results.Json(
                    new { error = "That report is not available." },
                    ApiConventions.Json,
                    statusCode: StatusCodes.Status404NotFound);
            }

            reportId = session.Context?.ReportId;
        }

        try
        {
            var result = await assistant.AskAsync(
                http.User,
                question,
                new KnowledgeRetrievalOptions { ReportId = reportId },
                cancellationToken);

            if (result.Sources.Count == 0)
            {
                return Results.Json(
                    new
                    {
                        insufficient = true,
                        answer = (string?)null,
                        sources = Array.Empty<object>(),
                        message = "Insufficient evidence in authorized project context.",
                    },
                    ApiConventions.Json);
            }

            return Results.Json(
                new
                {
                    insufficient = false,
                    answer = string.IsNullOrWhiteSpace(result.Answer)
                        ? "The assistant returned no answer text."
                        : result.Answer,
                    message = $"Answer ready. {result.Sources.Count} {(result.Sources.Count == 1 ? "source" : "sources")}.",
                    sources = result.Sources.Select(source => new
                    {
                        documentName = KnowledgeSourceDisplay.DocumentName(source.FileName),
                        documentKind = KnowledgeSourceDisplay.DocumentKind(source.DocumentType),
                        ruleId = KnowledgeSourceDisplay.RuleId(source.RuleId),
                        sourceLocation = KnowledgeSourceDisplay.Location(source.SourceLocation),
                        schoolCode = source.SchoolCode,
                        reportYear = source.ReportYear,
                    }),
                },
                ApiConventions.Json);
        }
        catch (OperationCanceledException)
        {
            return Results.Json(new { error = "The question was cancelled." }, ApiConventions.Json, statusCode: StatusCodes.Status400BadRequest);
        }
        catch (Exception exception)
        {
            return Results.Json(new { error = UserFacingError(exception) }, ApiConventions.Json, statusCode: StatusCodes.Status502BadGateway);
        }
    }

    private static string UserFacingError(Exception exception) =>
        exception switch
        {
            LanguageModelConfigurationException => "The language model is not configured.",
            LanguageModelTimeoutException => "The language model timed out. Try again.",
            LanguageModelProviderException { StatusCode: 429 } =>
                "The language model is rate-limited. Wait a minute and try again.",
            LanguageModelProviderException { StatusCode: 404 } =>
                "The language model was not found. Check LanguageModel:Model.",
            LanguageModelProviderException => "The language model could not complete the request.",
            EmbeddingConfigurationException => "Embeddings are not configured.",
            EmbeddingTimeoutException => "The embedding service timed out. Try again.",
            EmbeddingProviderException { StatusCode: 429 } =>
                "The embedding service is rate-limited. Wait a minute and try again.",
            EmbeddingProviderException => "The embedding service could not complete the request.",
            _ => "The assistant could not answer. Try again.",
        };

    public sealed record AskRequest(string? Question, int? ReportId);
}
