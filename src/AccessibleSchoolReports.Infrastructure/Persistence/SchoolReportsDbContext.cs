using AccessibleSchoolReports.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace AccessibleSchoolReports.Infrastructure.Persistence;

public sealed class SchoolReportsDbContext : DbContext
{
    public SchoolReportsDbContext(DbContextOptions<SchoolReportsDbContext> options)
        : base(options)
    {
    }

    public DbSet<School> Schools => Set<School>();

    public DbSet<GraduateRecord> GraduateRecords => Set<GraduateRecord>();

    public DbSet<ImportRun> ImportRuns => Set<ImportRun>();

    public DbSet<ImportRowIssue> ImportRowIssues => Set<ImportRowIssue>();

    public DbSet<ReportRun> ReportRuns => Set<ReportRun>();

    public DbSet<ReportRunItem> ReportRunItems => Set<ReportRunItem>();

    public async Task MigrateAsync(CancellationToken cancellationToken = default)
    {
        await Database.MigrateAsync(cancellationToken);
        await PrepareSqliteAsync(cancellationToken);
    }

    public Task PrepareSqliteAsync(CancellationToken cancellationToken = default) =>
        Database.ProviderName == "Microsoft.EntityFrameworkCore.Sqlite"
            ? Database.ExecuteSqlRawAsync("PRAGMA journal_mode=WAL; PRAGMA busy_timeout=5000;", cancellationToken)
            : Task.CompletedTask;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<School>(entity =>
        {
            entity.ToTable("Schools");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Code).HasMaxLength(32).IsRequired();
            entity.Property(e => e.Name).HasMaxLength(256);
            entity.HasIndex(e => e.Code).IsUnique();
        });

        modelBuilder.Entity<ImportRun>(entity =>
        {
            entity.ToTable("ImportRuns");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.FileName).HasMaxLength(512);
            entity.Property(e => e.ContentSha256).HasMaxLength(64);
            entity.Property(e => e.Status).HasConversion<string>().HasMaxLength(32);
            entity.Property(e => e.Message).HasMaxLength(2000);
            entity.HasIndex(e => e.StartedUtc);
            entity.HasIndex(e => e.ContentSha256);
        });

        modelBuilder.Entity<ImportRowIssue>(entity =>
        {
            entity.ToTable("ImportRowIssues");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Reason).HasMaxLength(2000).IsRequired();

            entity.HasOne(e => e.ImportRun)
                .WithMany(r => r.Issues)
                .HasForeignKey(e => e.ImportRunId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasIndex(e => e.ImportRunId);
            entity.HasIndex(e => new { e.ImportRunId, e.RowNumber });
        });

        modelBuilder.Entity<GraduateRecord>(entity =>
        {
            entity.ToTable("GraduateRecords");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Sex3).HasMaxLength(16);
            entity.Property(e => e.Minstat).HasMaxLength(16);
            entity.Property(e => e.Jobcat1).HasMaxLength(16);
            entity.Property(e => e.JobFtPt).HasMaxLength(16);
            entity.Property(e => e.Empgen).HasMaxLength(16);
            entity.Property(e => e.Firm1).HasMaxLength(16);
            entity.Property(e => e.Lfjob).HasMaxLength(16);
            entity.Property(e => e.Jobreg).HasMaxLength(16);
            entity.Property(e => e.LocationFlag).HasMaxLength(32);
            entity.Property(e => e.Jobst).HasMaxLength(16);
            entity.Property(e => e.Source).HasMaxLength(16);
            entity.Property(e => e.Time1).HasMaxLength(16);
            entity.Property(e => e.Status).HasMaxLength(16);
            entity.Property(e => e.Duration).HasMaxLength(16);
            entity.Property(e => e.SchoolFund).HasMaxLength(16);
            entity.Property(e => e.Emptype1).HasMaxLength(16);
            entity.Property(e => e.SalFtPerm).HasPrecision(12, 2);

            entity.HasOne(e => e.School)
                .WithMany(s => s.Graduates)
                .HasForeignKey(e => e.SchoolId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.ImportRun)
                .WithMany(r => r.Graduates)
                .HasForeignKey(e => e.ImportRunId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasIndex(e => e.SchoolId);
            entity.HasIndex(e => e.ImportRunId);
            entity.HasIndex(e => new { e.ImportRunId, e.SchoolId });
        });

        modelBuilder.Entity<ReportRun>(entity =>
        {
            entity.ToTable("ReportRuns");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Mode).HasConversion<string>().HasMaxLength(32);
            entity.Property(e => e.Status).HasConversion<string>().HasMaxLength(32);
            entity.Property(e => e.OutputDirectory).HasMaxLength(1024);
            entity.Property(e => e.Message).HasMaxLength(2000);
            entity.HasIndex(e => e.StartedUtc);
        });

        modelBuilder.Entity<ReportRunItem>(entity =>
        {
            entity.ToTable("ReportRunItems");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Status).HasConversion<string>().HasMaxLength(32);
            entity.Property(e => e.OutputPath).HasMaxLength(1024);
            entity.Property(e => e.Message).HasMaxLength(2000);

            entity.HasOne(e => e.ReportRun)
                .WithMany(r => r.Items)
                .HasForeignKey(e => e.ReportRunId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.School)
                .WithMany(s => s.ReportRunItems)
                .HasForeignKey(e => e.SchoolId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasIndex(e => e.ReportRunId);
            entity.HasIndex(e => e.SchoolId);
        });
    }
}
