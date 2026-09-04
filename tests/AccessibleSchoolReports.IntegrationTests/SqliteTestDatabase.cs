using AccessibleSchoolReports.Infrastructure.Persistence;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace AccessibleSchoolReports.IntegrationTests;

internal sealed class SqliteTestDatabase : IAsyncDisposable
{
    private readonly string _directory;

    public string ConnectionString { get; }

    public SchoolReportsDbContext Context { get; }

    private SqliteTestDatabase(string directory, string connectionString, SchoolReportsDbContext context)
    {
        _directory = directory;
        ConnectionString = connectionString;
        Context = context;
    }

    public static async Task<SqliteTestDatabase> CreateAsync(CancellationToken cancellationToken = default)
    {
        var directory = Path.Combine(Path.GetTempPath(), "asr-sqlite-tests", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(directory);

        var connectionString = new SqliteConnectionStringBuilder
        {
            DataSource = Path.Combine(directory, "schoolreports.db"),
            Mode = SqliteOpenMode.ReadWriteCreate,
            Cache = SqliteCacheMode.Shared,
            Pooling = false,
        }.ToString();
        var options = new DbContextOptionsBuilder<SchoolReportsDbContext>()
            .UseSqlite(connectionString)
            .Options;

        var context = new SchoolReportsDbContext(options);
        await context.MigrateAsync(cancellationToken);

        return new SqliteTestDatabase(directory, connectionString, context);
    }

    public IDbContextFactory<SchoolReportsDbContext> CreateFactory() =>
        new SqliteTestContextFactory(ConnectionString);

    public async ValueTask DisposeAsync()
    {
        await Context.DisposeAsync();
        SqliteConnection.ClearAllPools();
        if (Directory.Exists(_directory))
        {
            Directory.Delete(_directory, recursive: true);
        }
    }
}

internal sealed class SqliteTestContextFactory : IDbContextFactory<SchoolReportsDbContext>
{
    private readonly DbContextOptions<SchoolReportsDbContext> _options;

    public SqliteTestContextFactory(string connectionString)
    {
        _options = new DbContextOptionsBuilder<SchoolReportsDbContext>()
            .UseSqlite(connectionString)
            .Options;
    }

    public SchoolReportsDbContext CreateDbContext() => new(_options);

    public Task<SchoolReportsDbContext> CreateDbContextAsync(CancellationToken cancellationToken = default) =>
        Task.FromResult(CreateDbContext());
}
