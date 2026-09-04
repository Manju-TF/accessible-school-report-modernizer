using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace AccessibleSchoolReports.Infrastructure.Persistence;

public sealed class SchoolReportsDbContextFactory : IDesignTimeDbContextFactory<SchoolReportsDbContext>
{
    public SchoolReportsDbContext CreateDbContext(string[] args)
    {
        var connectionString = SqliteConnectionString.Resolve(
            "Data Source=data/schoolreports.db",
            Directory.GetCurrentDirectory());

        var options = new DbContextOptionsBuilder<SchoolReportsDbContext>()
            .UseSqlite(connectionString)
            .Options;

        return new SchoolReportsDbContext(options);
    }
}
