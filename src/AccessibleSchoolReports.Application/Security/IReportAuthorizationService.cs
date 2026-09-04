using System.Security.Claims;
using AccessibleSchoolReports.Domain.Entities;

namespace AccessibleSchoolReports.Application.Security;

public interface IReportAuthorizationService
{
    Task<bool> CanViewReportAsync(
        ClaimsPrincipal user,
        ReportRunItem report,
        CancellationToken cancellationToken = default);

    Task<bool> CanViewReportAsync(
        ClaimsPrincipal user,
        int reportRunItemId,
        CancellationToken cancellationToken = default);

    Task<bool> CanGenerateReportAsync(
        ClaimsPrincipal user,
        int schoolId,
        CancellationToken cancellationToken = default);

    Task<bool> CanAccessSchoolAsync(
        ClaimsPrincipal user,
        int schoolId,
        CancellationToken cancellationToken = default);

    Task<IReadOnlySet<int>> GetAccessibleSchoolIdsAsync(
        ClaimsPrincipal user,
        CancellationToken cancellationToken = default);
}
