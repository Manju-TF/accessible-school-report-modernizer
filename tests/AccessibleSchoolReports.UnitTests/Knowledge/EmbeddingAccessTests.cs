using AccessibleSchoolReports.Application.Knowledge;
using AccessibleSchoolReports.Application.Security;
using AccessibleSchoolReports.Domain.Entities;
using AccessibleSchoolReports.Domain.Knowledge;
using AccessibleSchoolReports.UnitTests.Embeddings;

namespace AccessibleSchoolReports.UnitTests.Knowledge;

public sealed class EmbeddingAccessTests
{
    [Fact]
    public void ReportScopedChunk_IsNotSentForOtherSchool()
    {
        var document = new KnowledgeDocument
        {
            FileName = "b.pdf",
            DocumentType = KnowledgeDocumentType.GeneratedReport,
            ContentHash = new string('b', 64),
            SourceIdentifier = "b.pdf",
            SchoolId = 2,
            AuthorizationScope = KnowledgeAuthorizationScope.Report,
        };

        Assert.False(EmbeddingAccess.CanSendToExternalProvider(
            document,
            EmbeddingTestFixture.Principal("user-a", AppRoles.ReportUser),
            new HashSet<int> { 1 }));
        Assert.True(EmbeddingAccess.CanSendToExternalProvider(
            document,
            EmbeddingTestFixture.Principal("admin", AppRoles.Admin),
            new HashSet<int>()));
    }

    [Fact]
    public void AuthenticatedLegacy_MayBeSentForAnySignedInUser()
    {
        var document = new KnowledgeDocument
        {
            FileName = "legacy.md",
            DocumentType = KnowledgeDocumentType.Legacy,
            ContentHash = new string('a', 64),
            SourceIdentifier = "legacy.md",
            AuthorizationScope = KnowledgeAuthorizationScope.Authenticated,
        };

        Assert.True(EmbeddingAccess.CanSendToExternalProvider(
            document,
            EmbeddingTestFixture.Principal("user-a", AppRoles.Viewer),
            new HashSet<int>()));
        Assert.False(EmbeddingAccess.CanSendToExternalProvider(
            document,
            new System.Security.Claims.ClaimsPrincipal(new System.Security.Claims.ClaimsIdentity()),
            new HashSet<int>()));
    }
}
