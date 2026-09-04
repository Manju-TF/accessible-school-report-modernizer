using Microsoft.Data.Sqlite;

namespace AccessibleSchoolReports.Infrastructure.Persistence;

public static class SqliteConnectionString
{
    public const string DefaultRelativePath = "data/schoolreports.db";

    public static string Resolve(string connectionString, string baseDirectory)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(connectionString);
        ArgumentException.ThrowIfNullOrWhiteSpace(baseDirectory);

        var builder = new SqliteConnectionStringBuilder(connectionString);
        if (string.IsNullOrWhiteSpace(builder.DataSource))
        {
            throw new InvalidOperationException("SQLite connection string must include Data Source.");
        }

        if (!Path.IsPathRooted(builder.DataSource))
        {
            builder.DataSource = Path.GetFullPath(builder.DataSource, baseDirectory);
        }

        var directory = Path.GetDirectoryName(builder.DataSource);
        if (!string.IsNullOrEmpty(directory))
        {
            Directory.CreateDirectory(directory);
        }

        return builder.ToString();
    }

    public static string ResolveWorkingDatabase(string connectionString, string startDirectory) =>
        Resolve(connectionString, FindRepositoryRoot(startDirectory));

    public static string GetDataSource(string connectionString)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(connectionString);
        return new SqliteConnectionStringBuilder(connectionString).DataSource;
    }

    public static string FindRepositoryRoot(string startDirectory)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(startDirectory);

        var directory = new DirectoryInfo(startDirectory);
        while (directory is not null)
        {
            if (File.Exists(Path.Combine(directory.FullName, "AccessibleSchoolReports.sln")))
            {
                return directory.FullName;
            }

            directory = directory.Parent;
        }

        return startDirectory;
    }
}
