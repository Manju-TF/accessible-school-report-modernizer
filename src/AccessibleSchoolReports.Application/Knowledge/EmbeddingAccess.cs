using System.Security.Claims;
using AccessibleSchoolReports.Application.Security;
using AccessibleSchoolReports.Domain.Entities;

namespace AccessibleSchoolReports.Application.Knowledge;

public static class EmbeddingAccess
{
    public static bool CanSendToExternalProvider(
        KnowledgeDocument document,
        ClaimsPrincipal user,
        IReadOnlySet<int> accessibleSchoolIds)
    {
        ArgumentNullException.ThrowIfNull(document);
        ArgumentNullException.ThrowIfNull(user);
        ArgumentNullException.ThrowIfNull(accessibleSchoolIds);

        var authenticated = user.Identity?.IsAuthenticated == true;
        var isAdmin = authenticated && user.IsInRole(AppRoles.Admin);
        return KnowledgeAccess.IsAccessible(document, authenticated, isAdmin, accessibleSchoolIds);
    }

    public static IReadOnlyList<KnowledgeChunk> FilterPermitted(
        IEnumerable<KnowledgeChunk> chunks,
        ClaimsPrincipal user,
        IReadOnlySet<int> accessibleSchoolIds)
    {
        ArgumentNullException.ThrowIfNull(chunks);
        return chunks
            .Where(chunk => chunk.KnowledgeDocument is not null
                && CanSendToExternalProvider(chunk.KnowledgeDocument, user, accessibleSchoolIds))
            .ToList();
    }
}
