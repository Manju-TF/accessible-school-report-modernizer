using AccessibleSchoolReports.Domain.Entities;
using AccessibleSchoolReports.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace AccessibleSchoolReports.IntegrationTests;

public sealed class SchoolNameCatalogTests
{
    [Fact]
    [Trait("RuleId", "SS-HDR-01")]
    public async Task ApplyToExistingSchools_WritesSasNamesIntoSchoolsTable()
    {
        await using var db = await SqliteTestDatabase.CreateAsync();
        db.Context.Schools.AddRange(
            new School { Code = "10701" },
            new School { Code = "23306" },
            new School { Code = "99999", Name = "Test University School of Law" });
        await db.Context.SaveChangesAsync();

        await SchoolNameCatalog.ApplyToExistingSchoolsAsync(db.Context);

        var schools = await db.Context.Schools.OrderBy(school => school.Code).ToListAsync();
        Assert.Equal("Quinnipiac University School of Law", schools[0].Name);
        Assert.Equal("Hofstra University Maurice A. Deane School of Law", schools[1].Name);
        Assert.Equal("Test University School of Law", schools[2].Name);
    }

    [Fact]
    public void DisplayName_UsesStoredNameThenCode()
    {
        Assert.Equal("Yale Law School", SchoolNameCatalog.DisplayName("10703", "Yale Law School"));
        Assert.Equal("10703", SchoolNameCatalog.DisplayName("10703", storedName: null));
    }
}
