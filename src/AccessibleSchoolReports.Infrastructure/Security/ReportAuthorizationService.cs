using System.Security.Claims;
using AccessibleSchoolReports.Application.Security;
using AccessibleSchoolReports.Domain.Entities;
using AccessibleSchoolReports.Domain.Security;
using AccessibleSchoolReports.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace AccessibleSchoolReports.Infrastructure.Security;

public sealed class ReportAuthorizationService : IReportAuthorizationService
{
    private readonly SchoolReportsDbContext _db;

    public ReportAuthorizationService(SchoolReportsDbContext db)
    {
        _db = db;
    }

    public Task<bool> CanViewReportAsync(
        ClaimsPrincipal user,
        ReportRunItem report,
        CancellationToken cancellationToken = default) =>
        CanAccessSchoolAsync(user, report.SchoolId, cancellationToken);

    public async Task<bool> CanViewReportAsync(
        ClaimsPrincipal user,
        int reportRunItemId,
        CancellationToken cancellationToken = default)
    {
        var schoolId = await _db.ReportRunItems
            .AsNoTracking()
            .Where(item => item.Id == reportRunItemId)
            .Select(item => (int?)item.SchoolId)
            .FirstOrDefaultAsync(cancellationToken);
        if (schoolId is null)
        {
            return false;
        }

        return await CanAccessSchoolAsync(user, schoolId.Value, cancellationToken);
    }

    public async Task<bool> CanGenerateReportAsync(
        ClaimsPrincipal user,
        int schoolId,
        CancellationToken cancellationToken = default)
    {
        if (IsAdmin(user))
        {
            return true;
        }

        if (!user.IsInRole(AppRoles.ReportUser))
        {
            return false;
        }

        return await HasGrantAsync(user, schoolId, SchoolAccessLevel.Generate, cancellationToken);
    }

    public async Task<bool> CanAccessSchoolAsync(
        ClaimsPrincipal user,
        int schoolId,
        CancellationToken cancellationToken = default)
    {
        if (IsAdmin(user))
        {
            return true;
        }

        return await HasGrantAsync(user, schoolId, SchoolAccessLevel.View, cancellationToken);
    }

    public async Task<IReadOnlySet<int>> GetAccessibleSchoolIdsAsync(
        ClaimsPrincipal user,
        CancellationToken cancellationToken = default)
    {
        if (IsAdmin(user))
        {
            var all = await _db.Schools
                .AsNoTracking()
                .Select(school => school.Id)
                .ToListAsync(cancellationToken);
            return all.ToHashSet();
        }

        var userId = GetUserId(user);
        if (userId is null)
        {
            return new HashSet<int>();
        }

        var assigned = await _db.UserSchoolAccess
            .AsNoTracking()
            .Where(row => row.UserId == userId)
            .Select(row => row.SchoolId)
            .ToListAsync(cancellationToken);
        return assigned.ToHashSet();
    }

    private async Task<bool> HasGrantAsync(
        ClaimsPrincipal user,
        int schoolId,
        SchoolAccessLevel minimum,
        CancellationToken cancellationToken)
    {
        var userId = GetUserId(user);
        if (userId is null)
        {
            return false;
        }

        return await _db.UserSchoolAccess
            .AsNoTracking()
            .AnyAsync(
                row => row.UserId == userId
                    && row.SchoolId == schoolId
                    && row.AccessLevel >= minimum,
                cancellationToken);
    }

    private static bool IsAdmin(ClaimsPrincipal user) =>
        user.Identity?.IsAuthenticated == true && user.IsInRole(AppRoles.Admin);

    private static string? GetUserId(ClaimsPrincipal user)
    {
        if (user.Identity?.IsAuthenticated != true)
        {
            return null;
        }

        return user.FindFirstValue(ClaimTypes.NameIdentifier);
    }
}
