using System.Security.Claims;

namespace AccessibleSchoolReports.Application.Knowledge;

public interface IKnowledgeAssistantService
{
    Task<KnowledgeAssistantAnswer> AskAsync(
        ClaimsPrincipal user,
        string question,
        KnowledgeRetrievalOptions? options = null,
        CancellationToken cancellationToken = default);
}

public sealed class KnowledgeAssistantAnswer
{
    public required string Answer { get; init; }

    public required IReadOnlyList<KnowledgeRetrievalHit> Sources { get; init; }

    public required KnowledgeRetrievalResult Retrieval { get; init; }

    public bool LanguageModelInvoked { get; init; }

    public string? Provider { get; init; }

    public string? Model { get; init; }
}
