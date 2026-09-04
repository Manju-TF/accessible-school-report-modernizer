using System.Security.Claims;
using AccessibleSchoolReports.Application.Knowledge;
using AccessibleSchoolReports.Application.Security;

namespace AccessibleSchoolReports.Infrastructure.Knowledge;

public sealed class KnowledgeAssistantService : IKnowledgeAssistantService
{
    private readonly IKnowledgeRetrievalService _retrieval;
    private readonly ILanguageModelService _languageModel;

    public KnowledgeAssistantService(
        IKnowledgeRetrievalService retrieval,
        ILanguageModelService languageModel)
    {
        _retrieval = retrieval;
        _languageModel = languageModel;
    }

    public async Task<KnowledgeAssistantAnswer> AskAsync(
        ClaimsPrincipal user,
        string question,
        KnowledgeRetrievalOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(user);
        if (!CanAsk(user)
            || string.IsNullOrWhiteSpace(question)
            || question.Trim().Length > KnowledgeRetrievalOptions.MaxQuestionLength)
        {
            return new KnowledgeAssistantAnswer
            {
                Answer = string.Empty,
                Sources = [],
                Retrieval = await _retrieval.RetrieveAsync(user, question, options, cancellationToken),
                LanguageModelInvoked = false,
            };
        }

        var retrieval = await _retrieval.RetrieveAsync(user, question, options, cancellationToken);
        var request = KnowledgeGroundedPrompt.Create(question, retrieval.Hits);
        var completion = await _languageModel.CompleteAsync(request, cancellationToken);
        return new KnowledgeAssistantAnswer
        {
            Answer = completion.Text,
            Sources = retrieval.Hits,
            Retrieval = retrieval,
            LanguageModelInvoked = true,
            Provider = completion.Provider,
            Model = completion.Model,
        };
    }

    private static bool CanAsk(ClaimsPrincipal user)
    {
        var authenticated = user.Identity?.IsAuthenticated == true;
        return KnowledgeAccess.HasRetrievalAccess(
            authenticated,
            authenticated && user.IsInRole(AppRoles.Admin),
            authenticated && user.IsInRole(AppRoles.ReportUser),
            authenticated && user.IsInRole(AppRoles.Viewer));
    }
}
