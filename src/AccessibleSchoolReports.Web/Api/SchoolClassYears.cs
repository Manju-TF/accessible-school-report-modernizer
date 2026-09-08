using AccessibleSchoolReports.Application.Reporting;
using AccessibleSchoolReports.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace AccessibleSchoolReports.Web.Api;

internal static class SchoolClassYears
{
    public static async Task<YearOptions> LoadAsync(
        SchoolReportsDbContext db,
        IReadOnlyCollection<int> schoolIds,
        string configuredYear,
        CancellationToken cancellationToken)
    {
        if (schoolIds.Count == 0)
        {
            var fallback = ReportYearCatalog.Distinct(configuredYear, []);
            return new YearOptions(fallback[0], fallback, new Dictionary<int, IReadOnlyList<string>>());
        }

        var graduateYears = await db.GraduateRecords
            .AsNoTracking()
            .Where(row => schoolIds.Contains(row.SchoolId) && row.ClassYear != null)
            .Select(row => new YearRow(row.SchoolId, row.ClassYear))
            .Distinct()
            .ToListAsync(cancellationToken);

        var knowledgeYears = await db.KnowledgeDocuments
            .AsNoTracking()
            .Where(document => document.SchoolId != null
                && schoolIds.Contains(document.SchoolId.Value)
                && document.ReportYear != null)
            .Select(document => new YearRow(document.SchoolId!.Value, document.ReportYear))
            .Distinct()
            .ToListAsync(cancellationToken);

        var outputPaths = await db.ReportRunItems
            .AsNoTracking()
            .Where(item => schoolIds.Contains(item.SchoolId) && item.OutputPath != null)
            .Select(item => new { item.SchoolId, item.OutputPath })
            .ToListAsync(cancellationToken);

        var pathYears = outputPaths
            .Select(item => GeneratedReportPath.TryParseClassYear(item.OutputPath, out var year)
                ? new YearRow(item.SchoolId, year)
                : null)
            .Where(row => row is not null)
            .Select(row => row!)
            .Distinct()
            .ToList();

        var bySchool = new Dictionary<int, IReadOnlyList<string>>();
        foreach (var schoolId in schoolIds)
        {
            var years = ReportYearCatalog.Distinct(
                configuredYear,
                graduateYears.Where(row => row.SchoolId == schoolId).Select(row => row.Year)
                    .Concat(knowledgeYears.Where(row => row.SchoolId == schoolId).Select(row => row.Year))
                    .Concat(pathYears.Where(row => row.SchoolId == schoolId).Select(row => row.Year)));
            bySchool[schoolId] = years;
        }

        var allYears = ReportYearCatalog.Distinct(
            configuredYear,
            graduateYears.Select(row => row.Year)
                .Concat(knowledgeYears.Select(row => row.Year))
                .Concat(pathYears.Select(row => row.Year)));

        return new YearOptions(allYears[0], allYears, bySchool);
    }

    public sealed record YearOptions(
        string DefaultYear,
        IReadOnlyList<string> Years,
        IReadOnlyDictionary<int, IReadOnlyList<string>> BySchool);

    private sealed record YearRow(int SchoolId, int? Year);
}
