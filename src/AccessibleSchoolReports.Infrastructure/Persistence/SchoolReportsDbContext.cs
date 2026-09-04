using AccessibleSchoolReports.Domain.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace AccessibleSchoolReports.Infrastructure.Persistence;

public sealed class SchoolReportsDbContext : IdentityDbContext<IdentityUser>
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

    public DbSet<UserSchoolAccess> UserSchoolAccess => Set<UserSchoolAccess>();

    public DbSet<KnowledgeDocument> KnowledgeDocuments => Set<KnowledgeDocument>();

    public DbSet<KnowledgeChunk> KnowledgeChunks => Set<KnowledgeChunk>();

    public async Task MigrateAsync(CancellationToken cancellationToken = default)
    {
        await Database.MigrateAsync(cancellationToken);
        await PrepareSqliteAsync(cancellationToken);
        await SchoolNameCatalog.ApplyToExistingSchoolsAsync(this, cancellationToken);
    }

    public Task PrepareSqliteAsync(CancellationToken cancellationToken = default) =>
        Database.ProviderName == "Microsoft.EntityFrameworkCore.Sqlite"
            ? Database.ExecuteSqlRawAsync("PRAGMA journal_mode=WAL; PRAGMA busy_timeout=5000;", cancellationToken)
            : Task.CompletedTask;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

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

        modelBuilder.Entity<UserSchoolAccess>(entity =>
        {
            entity.ToTable("UserSchoolAccess");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.UserId).HasMaxLength(450).IsRequired();
            entity.Property(e => e.AccessLevel).HasConversion<int>();
            entity.HasOne(e => e.School)
                .WithMany(school => school.UserAccess)
                .HasForeignKey(e => e.SchoolId)
                .OnDelete(DeleteBehavior.Cascade);
            entity.HasIndex(e => new { e.UserId, e.SchoolId }).IsUnique();
            entity.HasIndex(e => e.SchoolId);
        });

        modelBuilder.Entity<KnowledgeDocument>(entity =>
        {
            entity.ToTable("KnowledgeDocuments");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.FileName).HasMaxLength(512).IsRequired();
            entity.Property(e => e.DocumentType).HasConversion<string>().HasMaxLength(32);
            entity.Property(e => e.ContentHash).HasMaxLength(64).IsRequired();
            entity.Property(e => e.SourceIdentifier).HasMaxLength(1024).IsRequired();
            entity.Property(e => e.SchoolCode).HasMaxLength(32);
            entity.Property(e => e.ReportType).HasMaxLength(64);
            entity.Property(e => e.AuthorizationScope).HasConversion<int>();

            entity.HasOne(e => e.School)
                .WithMany(school => school.KnowledgeDocuments)
                .HasForeignKey(e => e.SchoolId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.Report)
                .WithMany(item => item.KnowledgeDocuments)
                .HasForeignKey(e => e.ReportId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.ReportRun)
                .WithMany(run => run.KnowledgeDocuments)
                .HasForeignKey(e => e.ReportRunId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasIndex(e => e.ContentHash);
            entity.HasIndex(e => e.SourceIdentifier);
            entity.HasIndex(e => e.AuthorizationScope);
            entity.HasIndex(e => e.SchoolId);
            entity.HasIndex(e => e.SchoolCode);
            entity.HasIndex(e => e.ReportId);
            entity.HasIndex(e => e.ReportRunId);
            entity.HasIndex(e => new { e.AuthorizationScope, e.SchoolId });
        });

        modelBuilder.Entity<KnowledgeChunk>(entity =>
        {
            entity.ToTable("KnowledgeChunks");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Content).IsRequired();
            entity.Property(e => e.RuleId).HasMaxLength(32);
            entity.Property(e => e.Category).HasMaxLength(64).IsRequired();
            entity.Property(e => e.SourceLocation).HasMaxLength(256).IsRequired();
            entity.Property(e => e.EmbeddingModel).HasMaxLength(128);

            entity.HasOne(e => e.KnowledgeDocument)
                .WithMany(document => document.Chunks)
                .HasForeignKey(e => e.KnowledgeDocumentId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasIndex(e => e.KnowledgeDocumentId);
            entity.HasIndex(e => new { e.KnowledgeDocumentId, e.ChunkNumber }).IsUnique();
        });
    }
}
