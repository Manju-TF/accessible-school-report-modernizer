using AccessibleSchoolReports.Application.Knowledge;
using AccessibleSchoolReports.Domain.Entities;
using AccessibleSchoolReports.Domain.Knowledge;

namespace AccessibleSchoolReports.UnitTests.Knowledge;

public sealed class KnowledgeAccessTests
{
    [Fact]
    public void HasRetrievalAccess_MatchesRagPolicyRoles()
    {
        Assert.True(KnowledgeAccess.HasRetrievalAccess(true, isAdmin: true, isReportUser: false, isViewer: false));
        Assert.True(KnowledgeAccess.HasRetrievalAccess(true, isAdmin: false, isReportUser: true, isViewer: false));
        Assert.True(KnowledgeAccess.HasRetrievalAccess(true, isAdmin: false, isReportUser: false, isViewer: true));
        Assert.False(KnowledgeAccess.HasRetrievalAccess(true, isAdmin: false, isReportUser: false, isViewer: false));
        Assert.False(KnowledgeAccess.HasRetrievalAccess(false, isAdmin: true, isReportUser: true, isViewer: true));
    }

    [Fact]
    public void AuthenticatedScope_IsVisibleToAnyAuthenticatedUser()
    {
        var document = Document(KnowledgeAuthorizationScope.Authenticated, schoolId: null);

        Assert.True(KnowledgeAccess.IsAccessible(document, isAuthenticated: true, isAdmin: false, new HashSet<int>()));
        Assert.False(KnowledgeAccess.IsAccessible(document, isAuthenticated: false, isAdmin: false, new HashSet<int>()));
    }

    [Fact]
    public void SchoolAndReportScope_RequireAssignedSchool()
    {
        var schoolDoc = Document(KnowledgeAuthorizationScope.School, schoolId: 10);
        var reportDoc = Document(KnowledgeAuthorizationScope.Report, schoolId: 10);
        var assigned = new HashSet<int> { 10 };
        var other = new HashSet<int> { 11 };

        Assert.True(KnowledgeAccess.IsAccessible(schoolDoc, true, false, assigned));
        Assert.True(KnowledgeAccess.IsAccessible(reportDoc, true, false, assigned));
        Assert.False(KnowledgeAccess.IsAccessible(schoolDoc, true, false, other));
        Assert.False(KnowledgeAccess.IsAccessible(reportDoc, true, false, other));
        Assert.False(KnowledgeAccess.IsAccessible(
            Document(KnowledgeAuthorizationScope.School, schoolId: null),
            true,
            false,
            assigned));
    }

    [Fact]
    public void Admin_CanAccessAllScopes()
    {
        var assigned = new HashSet<int>();

        Assert.True(KnowledgeAccess.IsAccessible(
            Document(KnowledgeAuthorizationScope.Authenticated, schoolId: null),
            true,
            true,
            assigned));
        Assert.True(KnowledgeAccess.IsAccessible(
            Document(KnowledgeAuthorizationScope.Report, schoolId: 99),
            true,
            true,
            assigned));
        Assert.True(KnowledgeAccess.IsAccessible(
            Document(KnowledgeAuthorizationScope.Admin, schoolId: null),
            true,
            true,
            assigned));
    }

    [Fact]
    public void AdminScope_IsHiddenFromNonAdmin()
    {
        var adminDoc = Document(KnowledgeAuthorizationScope.Admin, schoolId: null);
        var assigned = new HashSet<int> { 10 };

        Assert.False(KnowledgeAccess.IsAccessible(adminDoc, true, false, assigned));
        Assert.False(KnowledgeAccess.IsAccessible(
            Document(KnowledgeAuthorizationScope.Admin, schoolId: 10),
            true,
            false,
            assigned));
    }

    [Fact]
    public void WhereAccessible_DoesNotLeakAdminOrUnassignedSchoolDocuments()
    {
        var documents = new[]
        {
            Document(KnowledgeAuthorizationScope.Authenticated, schoolId: null),
            Document(KnowledgeAuthorizationScope.Admin, schoolId: null),
            Document(KnowledgeAuthorizationScope.Report, schoolId: 10),
            Document(KnowledgeAuthorizationScope.Report, schoolId: 11),
        }.AsQueryable();

        var visible = documents
            .WhereAccessible(isAuthenticated: true, isAdmin: false, new HashSet<int> { 10 })
            .ToList();

        Assert.Equal(2, visible.Count);
        Assert.DoesNotContain(visible, document => document.AuthorizationScope == KnowledgeAuthorizationScope.Admin);
        Assert.DoesNotContain(visible, document => document.SchoolId == 11);
    }

    private static KnowledgeDocument Document(KnowledgeAuthorizationScope scope, int? schoolId) =>
        new()
        {
            FileName = "doc.pdf",
            DocumentType = schoolId is null ? KnowledgeDocumentType.Legacy : KnowledgeDocumentType.GeneratedReport,
            ContentHash = new string('0', 64),
            SourceIdentifier = "source",
            AuthorizationScope = scope,
            SchoolId = schoolId,
        };
}
