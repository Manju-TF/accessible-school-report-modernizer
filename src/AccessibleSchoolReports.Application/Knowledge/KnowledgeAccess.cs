using AccessibleSchoolReports.Domain.Entities;
using AccessibleSchoolReports.Domain.Knowledge;

namespace AccessibleSchoolReports.Application.Knowledge;

/// <summary>
/// Authorization-aware knowledge filters. Retrieval applies this before scoring or calling an LLM.
/// </summary>
public static class KnowledgeAccess
{
    public static bool HasRetrievalAccess(bool isAuthenticated, bool isAdmin, bool isReportUser, bool isViewer) =>
        isAuthenticated && (isAdmin || isReportUser || isViewer);

    public static bool IsAccessible(
        KnowledgeDocument document,
        bool isAuthenticated,
        bool isAdmin,
        IReadOnlySet<int> accessibleSchoolIds)
    {
        ArgumentNullException.ThrowIfNull(document);
        ArgumentNullException.ThrowIfNull(accessibleSchoolIds);

        if (!isAuthenticated)
        {
            return false;
        }

        if (isAdmin)
        {
            return true;
        }

        return document.AuthorizationScope switch
        {
            KnowledgeAuthorizationScope.Authenticated => true,
            KnowledgeAuthorizationScope.School or KnowledgeAuthorizationScope.Report =>
                document.SchoolId is int schoolId && accessibleSchoolIds.Contains(schoolId),
            KnowledgeAuthorizationScope.Admin => false,
            _ => false,
        };
    }

    public static IQueryable<KnowledgeDocument> WhereAccessible(
        this IQueryable<KnowledgeDocument> documents,
        bool isAuthenticated,
        bool isAdmin,
        IReadOnlySet<int> accessibleSchoolIds)
    {
        ArgumentNullException.ThrowIfNull(documents);
        ArgumentNullException.ThrowIfNull(accessibleSchoolIds);

        if (!isAuthenticated)
        {
            return documents.Where(_ => false);
        }

        if (isAdmin)
        {
            return documents;
        }

        var schoolIds = accessibleSchoolIds.ToArray();
        return documents.Where(document =>
            document.AuthorizationScope == KnowledgeAuthorizationScope.Authenticated
            || ((document.AuthorizationScope == KnowledgeAuthorizationScope.School
                    || document.AuthorizationScope == KnowledgeAuthorizationScope.Report)
                && document.SchoolId != null
                && schoolIds.Contains(document.SchoolId.Value)));
    }
}
