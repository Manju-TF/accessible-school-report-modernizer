using AccessibleSchoolReports.Domain.Persistence;

namespace AccessibleSchoolReports.Application.Reporting;

public interface IReportGenerationService
{
    Task<SchoolReportGenerationResult> GenerateSchoolReportAsync(
        int schoolId,
        CancellationToken cancellationToken = default,
        string? classYear = null);

    Task<AllSchoolReportGenerationResult> GenerateAllSequentialAsync(
        CancellationToken cancellationToken = default,
        string? classYear = null,
        IProgress<SchoolGenerationProgress>? progress = null);

    Task<AllSchoolReportGenerationResult> GenerateAllParallelAsync(
        int maxDegreeOfParallelism = ReportGenerationOptions.DefaultMaxParallelism,
        CancellationToken cancellationToken = default,
        string? classYear = null,
        IProgress<SchoolGenerationProgress>? progress = null);
}

public sealed record SchoolGenerationProgress(int Total, int Completed, int Successful, int Failed);

public sealed class SchoolReportGenerationResult
{
    public int ReportRunId { get; init; }

    public int? ReportRunItemId { get; init; }

    public int SchoolId { get; init; }

    public string? SchoolCode { get; init; }

    public string? SchoolName { get; init; }

    public required RunStatus Status { get; init; }

    public string? OutputPath { get; init; }

    public string? Message { get; init; }

    public int GraduateCount { get; init; }

    public DateTimeOffset StartedUtc { get; init; }

    public DateTimeOffset CompletedUtc { get; init; }

    public TimeSpan Duration { get; init; }

    public long DurationMilliseconds => (long)Math.Max(0, Duration.TotalMilliseconds);
}

public sealed class AllSchoolReportGenerationResult
{
    public int ReportRunId { get; init; }

    public required RunStatus Status { get; init; }

    public int Total { get; init; }

    public int Successful { get; init; }

    public int Failed { get; init; }

    public string? OutputDirectory { get; init; }

    public string? Message { get; init; }

    public DateTimeOffset StartedUtc { get; init; }

    public DateTimeOffset CompletedUtc { get; init; }

    public TimeSpan Duration { get; init; }

    public long DurationMilliseconds => (long)Math.Max(0, Duration.TotalMilliseconds);

    public required IReadOnlyList<SchoolReportGenerationResult> Items { get; init; }
}

public sealed class ReportGenerationOptions
{
    public const string SectionName = "ReportGeneration";

    public const int DefaultMaxParallelism = 4;

    public const int MinMaxParallelism = 1;

    public const int MaxMaxParallelism = 8;

    public string OutputRoot { get; set; } = "output";

    public string ClassYear { get; set; } = "2025";

    public string FileName { get; set; } = "summary-report.pdf";

    public static int ClampMaxParallelism(int maxDegreeOfParallelism) =>
        Math.Clamp(maxDegreeOfParallelism, MinMaxParallelism, MaxMaxParallelism);
}
