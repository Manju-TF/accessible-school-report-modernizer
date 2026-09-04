using System.Security.Claims;
using AccessibleSchoolReports.Application.Knowledge;
using AccessibleSchoolReports.Application.Security;
using AccessibleSchoolReports.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace AccessibleSchoolReports.Infrastructure.Knowledge;

public sealed class KnowledgeAssistantSession : IKnowledgeAssistantSession
{
    private readonly SchoolReportsDbContext _db;
    private readonly IReportAuthorizationService _authorization;

    public KnowledgeAssistantSession(
        SchoolReportsDbContext db,
        IReportAuthorizationService authorization)
    {
        _db = db;
        _authorization = authorization;
    }

    public KnowledgeReportContext? Context { get; private set; }

    public async Task<bool> TrySelectReportAsync(
        ClaimsPrincipal user,
        int reportId,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(user);
        if (reportId <= 0 || !await _authorization.CanViewReportAsync(user, reportId, cancellationToken))
        {
            Clear();
            return false;
        }

        var row = await _db.ReportRunItems
            .AsNoTracking()
            .Where(item => item.Id == reportId)
            .Select(item => new
            {
                item.School.Code,
                item.School.Name,
            })
            .FirstOrDefaultAsync(cancellationToken);
        if (row is null)
        {
            Clear();
            return false;
        }

        var year = await _db.KnowledgeDocuments
            .AsNoTracking()
            .Where(document => document.ReportId == reportId)
            .Select(document => document.ReportYear)
            .FirstOrDefaultAsync(cancellationToken);

        Context = new KnowledgeReportContext
        {
            ReportId = reportId,
            SchoolCode = row.Code,
            SchoolName = row.Name,
            ReportYear = year,
        };
        return true;
    }

    public void Clear() => Context = null;
}
