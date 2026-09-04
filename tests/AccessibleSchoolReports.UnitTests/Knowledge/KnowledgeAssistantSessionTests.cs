using AccessibleSchoolReports.Application.Security;
using AccessibleSchoolReports.Infrastructure.Knowledge;
using AccessibleSchoolReports.Infrastructure.Security;

namespace AccessibleSchoolReports.UnitTests.Knowledge;

public sealed class KnowledgeAssistantSessionTests
{
    [Fact]
    public async Task UserA_CanSelectSchoolAReport()
    {
        await using var fixture = await KnowledgeRetrievalTestFixture.CreateAsync();
        var session = new KnowledgeAssistantSession(fixture.Db, new ReportAuthorizationService(fixture.Db));
        var user = KnowledgeRetrievalTestFixture.Principal("user-a", AppRoles.ReportUser);

        Assert.True(await session.TrySelectReportAsync(user, fixture.SchoolAReportId));
        Assert.NotNull(session.Context);
        Assert.Equal(fixture.SchoolAReportId, session.Context.ReportId);
        Assert.Equal("10701", session.Context.SchoolCode);
    }

    [Fact]
    public async Task UserA_CannotSelectSchoolBReport_AndSessionIsCleared()
    {
        await using var fixture = await KnowledgeRetrievalTestFixture.CreateAsync();
        var session = new KnowledgeAssistantSession(fixture.Db, new ReportAuthorizationService(fixture.Db));
        var user = KnowledgeRetrievalTestFixture.Principal("user-a", AppRoles.ReportUser);
        await session.TrySelectReportAsync(user, fixture.SchoolAReportId);

        Assert.False(await session.TrySelectReportAsync(user, fixture.SchoolBReportId));
        Assert.Null(session.Context);
    }

    [Fact]
    public async Task TamperedReportId_DoesNotLeaveUnauthorizedContext()
    {
        await using var fixture = await KnowledgeRetrievalTestFixture.CreateAsync();
        var session = new KnowledgeAssistantSession(fixture.Db, new ReportAuthorizationService(fixture.Db));
        var user = KnowledgeRetrievalTestFixture.Principal("user-a", AppRoles.ReportUser);

        Assert.False(await session.TrySelectReportAsync(user, fixture.SchoolBReportId));
        Assert.Null(session.Context);
        Assert.False(await session.TrySelectReportAsync(user, 99999));
        Assert.Null(session.Context);
    }
}
