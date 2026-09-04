using AccessibleSchoolReports.Domain.Entities;
using AccessibleSchoolReports.Domain.Recodes;
using Microsoft.EntityFrameworkCore;

namespace AccessibleSchoolReports.Infrastructure.Persistence;

/// <summary>
/// Writes characterized SAS %SCHRPTS names into <see cref="School.Name"/>.
/// Display and PDF generation read the stored name, not this catalog.
/// </summary>
public static class SchoolNameCatalog
{
    public static async Task ApplyToExistingSchoolsAsync(
        SchoolReportsDbContext db,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(db);

        var schools = await db.Schools.ToListAsync(cancellationToken);
        var changed = false;
        foreach (var school in schools)
        {
            var name = LegacySchoolNames.Lookup(school.Code);
            if (name is null || string.Equals(school.Name, name, StringComparison.Ordinal))
            {
                continue;
            }

            school.Name = name;
            changed = true;
        }

        if (changed)
        {
            await db.SaveChangesAsync(cancellationToken);
        }
    }

    public static string DisplayName(string? code, string? storedName)
    {
        if (!string.IsNullOrWhiteSpace(storedName))
        {
            return storedName.Trim();
        }

        return string.IsNullOrWhiteSpace(code) ? string.Empty : code.Trim();
    }
}
