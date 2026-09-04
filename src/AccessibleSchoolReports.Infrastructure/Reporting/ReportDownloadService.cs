using System.Security.Claims;
using AccessibleSchoolReports.Application.Reporting;
using AccessibleSchoolReports.Application.Security;
using AccessibleSchoolReports.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace AccessibleSchoolReports.Infrastructure.Reporting;

public sealed class ReportDownloadService : IReportDownloadService
{
    private readonly SchoolReportsDbContext _db;
    private readonly IReportAuthorizationService _authorization;
    private readonly ReportGenerationOptions _options;

    public ReportDownloadService(
        SchoolReportsDbContext db,
        IReportAuthorizationService authorization,
        IOptions<ReportGenerationOptions> options)
    {
        _db = db;
        _authorization = authorization;
        _options = options.Value;
    }

    public async Task<ReportDownloadResult> TryDownloadAsync(
        ClaimsPrincipal user,
        int reportRunItemId,
        CancellationToken cancellationToken = default)
    {
        if (user.Identity?.IsAuthenticated != true)
        {
            return ReportDownloadResult.Denied;
        }

        var item = await _db.ReportRunItems
            .AsNoTracking()
            .Include(row => row.School)
            .FirstOrDefaultAsync(row => row.Id == reportRunItemId, cancellationToken);
        if (item?.School is null)
        {
            return ReportDownloadResult.Denied;
        }

        if (!await _authorization.CanViewReportAsync(user, item, cancellationToken))
        {
            return ReportDownloadResult.Denied;
        }

        if (!ReportFileAccess.TryResolveDownloadPath(item.OutputPath, _options.OutputRoot, out var path))
        {
            return ReportDownloadResult.Denied;
        }

        var stream = new FileStream(
            path,
            FileMode.Open,
            FileAccess.Read,
            FileShare.Read);
        return ReportDownloadResult.Ok(stream, DownloadFileName(item.School.Code));
    }

    private static string DownloadFileName(string? schoolCode)
    {
        if (string.IsNullOrWhiteSpace(schoolCode))
        {
            return "school-summary-report.pdf";
        }

        var invalid = Path.GetInvalidFileNameChars();
        var chars = schoolCode.Select(ch => invalid.Contains(ch) ? '-' : ch).ToArray();
        return $"{new string(chars)}-summary-report.pdf";
    }
}
