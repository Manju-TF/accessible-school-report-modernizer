using System.Security.Claims;

namespace AccessibleSchoolReports.Application.Knowledge;

public interface IKnowledgeAssistantSession
{
    KnowledgeReportContext? Context { get; }

    /// <summary>
    /// Authorizes <paramref name="reportId"/> and stores it server-side.
    /// Unknown or unauthorized ids clear the session and return false without report details.
    /// </summary>
    Task<bool> TrySelectReportAsync(
        ClaimsPrincipal user,
        int reportId,
        CancellationToken cancellationToken = default);

    void Clear();
}

public sealed class KnowledgeReportContext
{
    public required int ReportId { get; init; }

    public required string SchoolCode { get; init; }

    public string? SchoolName { get; init; }

    public int? ReportYear { get; init; }
}
