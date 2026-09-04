using AccessibleSchoolReports.Infrastructure.Persistence;
using Microsoft.Data.Sqlite;

namespace AccessibleSchoolReports.UnitTests.Persistence;

public sealed class SqliteConnectionStringTests
{
    [Fact]
    public void FindRepositoryRoot_WalksUpToSolutionFile()
    {
        var root = Directory.CreateTempSubdirectory("asr-repo-");
        try
        {
            File.WriteAllText(Path.Combine(root.FullName, "AccessibleSchoolReports.sln"), string.Empty);
            var nested = Directory.CreateDirectory(Path.Combine(root.FullName, "src", "Web"));

            var found = SqliteConnectionString.FindRepositoryRoot(nested.FullName);

            Assert.Equal(root.FullName, found);
        }
        finally
        {
            root.Delete(recursive: true);
        }
    }

    [Fact]
    public void ResolveWorkingDatabase_UsesRepositoryDataFile()
    {
        var root = Directory.CreateTempSubdirectory("asr-repo-");
        try
        {
            File.WriteAllText(Path.Combine(root.FullName, "AccessibleSchoolReports.sln"), string.Empty);
            var nested = Directory.CreateDirectory(Path.Combine(root.FullName, "src", "Web"));

            var resolved = SqliteConnectionString.ResolveWorkingDatabase(
                $"Data Source={SqliteConnectionString.DefaultRelativePath}",
                nested.FullName);
            var path = SqliteConnectionString.GetDataSource(resolved);

            Assert.Equal(
                Path.GetFullPath(Path.Combine(root.FullName, "data", "schoolreports.db")),
                Path.GetFullPath(path));
            Assert.True(Directory.Exists(Path.Combine(root.FullName, "data")));
            Assert.False(Directory.Exists(Path.Combine(nested.FullName, "data")));
        }
        finally
        {
            root.Delete(recursive: true);
        }
    }

    [Fact]
    public void GetDataSource_ReadsConfiguredFile()
    {
        var source = SqliteConnectionString.GetDataSource("Data Source=data/schoolreports.db");
        Assert.Equal("data/schoolreports.db", source);
        Assert.Equal("data/schoolreports.db", new SqliteConnectionStringBuilder($"Data Source={source}").DataSource);
    }
}
