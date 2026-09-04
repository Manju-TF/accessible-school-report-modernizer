using System.Security.Claims;
using AccessibleSchoolReports.Application.Knowledge;

namespace AccessibleSchoolReports.UnitTests.Knowledge;

internal sealed class StubKnowledgeRetrievalService : IKnowledgeRetrievalService
{
    public KnowledgeRetrievalResult Next { get; set; } = new()
    {
        Hits = [],
        AuthorizedCandidateCount = 0,
        Duration = TimeSpan.Zero,
    };

    public Task<KnowledgeRetrievalResult> RetrieveAsync(
        ClaimsPrincipal user,
        string question,
        KnowledgeRetrievalOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        return Task.FromResult(Next);
    }
}
