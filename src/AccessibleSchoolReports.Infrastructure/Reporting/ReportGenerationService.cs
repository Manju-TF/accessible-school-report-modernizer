using AccessibleSchoolReports.Application.Reporting;
using AccessibleSchoolReports.Domain.Entities;
using AccessibleSchoolReports.Domain.Persistence;
using AccessibleSchoolReports.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace AccessibleSchoolReports.Infrastructure.Reporting;

public sealed class ReportGenerationService : IReportGenerationService
{
    private readonly SchoolReportsDbContext _db;
    private readonly IDbContextFactory<SchoolReportsDbContext> _dbFactory;
    private readonly ISchoolReportCalculator _calculator;
    private readonly IAccessiblePdfGenerator _pdfGenerator;
    private readonly ReportGenerationOptions _options;

    public ReportGenerationService(
        SchoolReportsDbContext db,
        IDbContextFactory<SchoolReportsDbContext> dbFactory,
        ISchoolReportCalculator calculator,
        IAccessiblePdfGenerator pdfGenerator,
        IOptions<ReportGenerationOptions> options)
    {
        _db = db;
        _dbFactory = dbFactory;
        _calculator = calculator;
        _pdfGenerator = pdfGenerator;
        _options = options.Value;
    }

    public async Task<SchoolReportGenerationResult> GenerateSchoolReportAsync(
        int schoolId,
        CancellationToken cancellationToken = default,
        string? classYear = null)
    {
        var started = DateTimeOffset.UtcNow;
        var yearFolder = YearFolder(classYear);
        var run = await CreateRunAsync(ReportGenerationMode.Single, yearFolder, started, maxParallelism: 1);

        try
        {
            cancellationToken.ThrowIfCancellationRequested();
            var school = await _db.Schools
                .AsNoTracking()
                .FirstOrDefaultAsync(row => row.Id == schoolId, cancellationToken);
            if (school is null)
            {
                var missing = FailedResult(run.Id, schoolId, started, "School was not found.");
                await CompleteRunAsync(run, started, RunStatus.Failed, total: 1, successful: 0, failed: 1, missing.Message!);
                return missing;
            }

            var item = await GenerateForSchoolAsync(_db, run.Id, school, yearFolder, cancellationToken);
            var status = item.Status == RunStatus.Completed ? RunStatus.Completed : item.Status;
            var successful = item.Status == RunStatus.Completed ? 1 : 0;
            var failed = item.Status == RunStatus.Failed ? 1 : 0;
            await CompleteRunAsync(run, started, status, total: 1, successful, failed, item.Message ?? status.ToString());
            return item;
        }
        catch (OperationCanceledException)
        {
            var cancelled = FailedResult(run.Id, schoolId, started, "Cancelled.", RunStatus.Cancelled);
            await CompleteRunAsync(run, started, RunStatus.Cancelled, total: 1, successful: 0, failed: 0, cancelled.Message!);
            return cancelled;
        }
    }

    public async Task<AllSchoolReportGenerationResult> GenerateAllSequentialAsync(
        CancellationToken cancellationToken = default,
        string? classYear = null,
        IProgress<SchoolGenerationProgress>? progress = null)
    {
        var started = DateTimeOffset.UtcNow;
        var yearFolder = YearFolder(classYear);
        var run = await CreateRunAsync(ReportGenerationMode.Sequential, yearFolder, started, maxParallelism: 1);
        var items = new List<SchoolReportGenerationResult>();

        try
        {
            var schools = await LoadEligibleSchoolsAsync(_db, cancellationToken);
            ReportProgress(progress, schools.Count, items);

            foreach (var school in schools)
            {
                cancellationToken.ThrowIfCancellationRequested();
                try
                {
                    items.Add(await GenerateForSchoolAsync(_db, run.Id, school, yearFolder, cancellationToken));
                }
                catch (OperationCanceledException)
                {
                    throw;
                }
                catch (Exception exception)
                {
                    items.Add(await PersistItemAsync(
                        _db,
                        run.Id,
                        school,
                        started: DateTimeOffset.UtcNow,
                        RunStatus.Failed,
                        outputPath: null,
                        graduateCount: 0,
                        Truncate(exception.Message)));
                }

                ReportProgress(progress, schools.Count, items);
            }

            var successful = items.Count(item => item.Status == RunStatus.Completed);
            var failed = items.Count(item => item.Status == RunStatus.Failed);
            var status = failed == 0 ? RunStatus.Completed : RunStatus.CompletedWithErrors;
            var message = $"Sequential generate-all. Total {schools.Count}. Successful {successful}. Failed {failed}.";
            await CompleteRunAsync(run, started, status, schools.Count, successful, failed, message);

            return ToAllResult(run, started, status, schools.Count, successful, failed, yearFolder, items);
        }
        catch (OperationCanceledException)
        {
            var successful = items.Count(item => item.Status == RunStatus.Completed);
            var failed = items.Count(item => item.Status == RunStatus.Failed);
            var total = await EligibleSchoolCountAsync(CancellationToken.None);
            var message = $"Cancelled after {items.Count} school(s). Total {total}. Successful {successful}. Failed {failed}.";
            await CompleteRunAsync(run, started, RunStatus.Cancelled, total, successful, failed, message);
            return ToAllResult(run, started, RunStatus.Cancelled, total, successful, failed, yearFolder, items);
        }
        catch (Exception exception)
        {
            var successful = items.Count(item => item.Status == RunStatus.Completed);
            var failed = items.Count(item => item.Status == RunStatus.Failed);
            var total = Math.Max(items.Count, successful + failed);
            var message = Truncate(exception.Message);
            await CompleteRunAsync(run, started, RunStatus.Failed, total, successful, failed, message);
            return ToAllResult(run, started, RunStatus.Failed, total, successful, failed, yearFolder, items);
        }
    }

    public async Task<AllSchoolReportGenerationResult> GenerateAllParallelAsync(
        int maxDegreeOfParallelism = ReportGenerationOptions.DefaultMaxParallelism,
        CancellationToken cancellationToken = default,
        string? classYear = null,
        IProgress<SchoolGenerationProgress>? progress = null)
    {
        var started = DateTimeOffset.UtcNow;
        var yearFolder = YearFolder(classYear);
        var degree = ReportGenerationOptions.ClampMaxParallelism(maxDegreeOfParallelism);
        var run = await CreateRunAsync(ReportGenerationMode.BoundedParallel, yearFolder, started, degree);
        var items = new System.Collections.Concurrent.ConcurrentBag<SchoolReportGenerationResult>();

        try
        {
            var schools = await LoadEligibleSchoolsAsync(_db, cancellationToken);
            ReportProgress(progress, schools.Count, items);
            await Parallel.ForEachAsync(
                schools,
                new ParallelOptions
                {
                    MaxDegreeOfParallelism = degree,
                    CancellationToken = cancellationToken,
                },
                async (school, token) =>
                {
                    await using var context = await _dbFactory.CreateDbContextAsync(token);
                    await context.PrepareSqliteAsync(CancellationToken.None);
                    try
                    {
                        items.Add(await GenerateForSchoolAsync(context, run.Id, school, yearFolder, token));
                    }
                    catch (OperationCanceledException)
                    {
                        throw;
                    }
                    catch (Exception exception)
                    {
                        items.Add(await PersistItemAsync(
                            context,
                            run.Id,
                            school,
                            started: DateTimeOffset.UtcNow,
                            RunStatus.Failed,
                            outputPath: null,
                            graduateCount: 0,
                            Truncate(exception.Message)));
                    }

                    ReportProgress(progress, schools.Count, items);
                });

            var ordered = OrderItems(items);
            var successful = ordered.Count(item => item.Status == RunStatus.Completed);
            var failed = ordered.Count(item => item.Status == RunStatus.Failed);
            var status = failed == 0 ? RunStatus.Completed : RunStatus.CompletedWithErrors;
            var message = $"Parallel generate-all. Total {schools.Count}. Successful {successful}. Failed {failed}. MaxDegreeOfParallelism {degree}.";
            await CompleteRunAsync(run, started, status, schools.Count, successful, failed, message);
            return ToAllResult(run, started, status, schools.Count, successful, failed, yearFolder, ordered);
        }
        catch (OperationCanceledException)
        {
            var ordered = OrderItems(items);
            var successful = ordered.Count(item => item.Status == RunStatus.Completed);
            var failed = ordered.Count(item => item.Status == RunStatus.Failed);
            var total = await EligibleSchoolCountAsync(CancellationToken.None);
            var message = $"Cancelled after {ordered.Count} school(s). Total {total}. Successful {successful}. Failed {failed}.";
            await CompleteRunAsync(run, started, RunStatus.Cancelled, total, successful, failed, message);
            return ToAllResult(run, started, RunStatus.Cancelled, total, successful, failed, yearFolder, ordered);
        }
        catch (Exception exception)
        {
            var ordered = OrderItems(items);
            var successful = ordered.Count(item => item.Status == RunStatus.Completed);
            var failed = ordered.Count(item => item.Status == RunStatus.Failed);
            var total = Math.Max(ordered.Count, successful + failed);
            await CompleteRunAsync(run, started, RunStatus.Failed, total, successful, failed, Truncate(exception.Message));
            return ToAllResult(run, started, RunStatus.Failed, total, successful, failed, yearFolder, ordered);
        }
    }

    private async Task<SchoolReportGenerationResult> GenerateForSchoolAsync(
        SchoolReportsDbContext db,
        int reportRunId,
        School school,
        string yearFolder,
        CancellationToken cancellationToken)
    {
        var started = DateTimeOffset.UtcNow;
        string? outputPath = null;
        try
        {
            var graduates = await db.GraduateRecords
                .AsNoTracking()
                .Where(row => row.SchoolId == school.Id)
                .ToListAsync(cancellationToken);
            if (graduates.Count == 0)
            {
                return await PersistItemAsync(
                    db,
                    reportRunId,
                    school,
                    started,
                    RunStatus.Failed,
                    outputPath: null,
                    graduateCount: 0,
                    $"No graduate records for school {school.Code}.");
            }

            cancellationToken.ThrowIfCancellationRequested();

            var calculated = _calculator.Calculate(school.Code, graduates);
            var report = new SchoolReport
            {
                SchoolCode = calculated.SchoolCode,
                SchoolName = school.Name,
                Rows = calculated.Rows,
                Sections = calculated.Sections,
            };

            outputPath = Path.Combine(yearFolder, SanitizeSchoolCode(school.Code), _options.FileName);
            Directory.CreateDirectory(Path.GetDirectoryName(outputPath)!);

            cancellationToken.ThrowIfCancellationRequested();

            await using (var stream = new FileStream(
                outputPath,
                FileMode.Create,
                FileAccess.Write,
                FileShare.None,
                bufferSize: 4096,
                options: FileOptions.Asynchronous))
            {
                _pdfGenerator.Generate(report, stream);
                await stream.FlushAsync(CancellationToken.None);
            }

            return await PersistItemAsync(
                db,
                reportRunId,
                school,
                started,
                RunStatus.Completed,
                outputPath,
                graduates.Count,
                $"Generated {graduates.Count} graduate(s).");
        }
        catch (OperationCanceledException)
        {
            TryDelete(outputPath);
            throw;
        }
        catch (Exception exception)
        {
            TryDelete(outputPath);
            return await PersistItemAsync(
                db,
                reportRunId,
                school,
                started,
                RunStatus.Failed,
                outputPath: null,
                graduateCount: 0,
                Truncate(exception.Message));
        }
    }

    private async Task<ReportRun> CreateRunAsync(
        ReportGenerationMode mode,
        string yearFolder,
        DateTimeOffset started,
        int maxParallelism)
    {
        var run = new ReportRun
        {
            Mode = mode,
            Status = RunStatus.Running,
            StartedUtc = started,
            MaxParallelism = maxParallelism,
            OutputDirectory = yearFolder,
        };
        _db.ReportRuns.Add(run);
        await _db.SaveChangesAsync(CancellationToken.None);
        return run;
    }

    private async Task CompleteRunAsync(
        ReportRun run,
        DateTimeOffset started,
        RunStatus status,
        int total,
        int successful,
        int failed,
        string message)
    {
        var completed = DateTimeOffset.UtcNow;
        var duration = completed - started;
        run.Status = status;
        run.CompletedUtc = completed;
        run.TotalCount = total;
        run.SuccessfulCount = successful;
        run.FailedCount = failed;
        run.DurationMilliseconds = Math.Max(0, (long)duration.TotalMilliseconds);
        run.Message = Truncate($"{message} Duration {FormatDuration(duration)}.");
        await _db.SaveChangesAsync(CancellationToken.None);
    }

    private async Task<SchoolReportGenerationResult> PersistItemAsync(
        SchoolReportsDbContext db,
        int reportRunId,
        School school,
        DateTimeOffset started,
        RunStatus status,
        string? outputPath,
        int graduateCount,
        string message)
    {
        var completed = DateTimeOffset.UtcNow;
        var duration = completed - started;
        var timedMessage = Truncate($"{message} Duration {FormatDuration(duration)}.");
        var item = new ReportRunItem
        {
            ReportRunId = reportRunId,
            SchoolId = school.Id,
            Status = status,
            OutputPath = outputPath,
            Message = timedMessage,
            StartedUtc = started,
            CompletedUtc = completed,
        };
        db.ReportRunItems.Add(item);
        await db.SaveChangesAsync(CancellationToken.None);

        return new SchoolReportGenerationResult
        {
            ReportRunId = reportRunId,
            ReportRunItemId = item.Id,
            SchoolId = school.Id,
            SchoolCode = school.Code,
            SchoolName = school.Name,
            Status = status,
            OutputPath = outputPath,
            Message = timedMessage,
            GraduateCount = graduateCount,
            StartedUtc = started,
            CompletedUtc = completed,
            Duration = duration,
        };
    }

    private async Task<int> EligibleSchoolCountAsync(CancellationToken cancellationToken) =>
        await _db.Schools.CountAsync(
            school => _db.GraduateRecords.Any(row => row.SchoolId == school.Id),
            cancellationToken);

    private AllSchoolReportGenerationResult ToAllResult(
        ReportRun run,
        DateTimeOffset started,
        RunStatus status,
        int total,
        int successful,
        int failed,
        string yearFolder,
        IReadOnlyList<SchoolReportGenerationResult> items)
    {
        var completed = run.CompletedUtc ?? DateTimeOffset.UtcNow;
        return new AllSchoolReportGenerationResult
        {
            ReportRunId = run.Id,
            Status = status,
            Total = total,
            Successful = successful,
            Failed = failed,
            OutputDirectory = yearFolder,
            Message = run.Message,
            StartedUtc = started,
            CompletedUtc = completed,
            Duration = completed - started,
            Items = OrderItems(items),
        };
    }

    private static async Task<List<School>> LoadEligibleSchoolsAsync(
        SchoolReportsDbContext db,
        CancellationToken cancellationToken) =>
        await db.Schools
            .AsNoTracking()
            .Where(school => db.GraduateRecords.Any(row => row.SchoolId == school.Id))
            .OrderBy(school => school.Code)
            .ToListAsync(cancellationToken);

    private static IReadOnlyList<SchoolReportGenerationResult> OrderItems(
        IEnumerable<SchoolReportGenerationResult> items) =>
        items
            .OrderBy(item => item.SchoolCode, StringComparer.Ordinal)
            .ThenBy(item => item.SchoolId)
            .ToArray();

    private static void ReportProgress(
        IProgress<SchoolGenerationProgress>? progress,
        int total,
        IEnumerable<SchoolReportGenerationResult> items)
    {
        if (progress is null)
        {
            return;
        }

        var snapshot = items.ToArray();
        progress.Report(new SchoolGenerationProgress(
            total,
            snapshot.Length,
            snapshot.Count(item => item.Status == RunStatus.Completed),
            snapshot.Count(item => item.Status == RunStatus.Failed)));
    }

    private static SchoolReportGenerationResult FailedResult(
        int runId,
        int schoolId,
        DateTimeOffset started,
        string message,
        RunStatus status = RunStatus.Failed)
    {
        var completed = DateTimeOffset.UtcNow;
        var duration = completed - started;
        return new SchoolReportGenerationResult
        {
            ReportRunId = runId,
            SchoolId = schoolId,
            Status = status,
            Message = Truncate($"{message} Duration {FormatDuration(duration)}."),
            StartedUtc = started,
            CompletedUtc = completed,
            Duration = duration,
        };
    }

    private string YearFolder(string? classYear = null) =>
        Path.Combine(ResolveOutputRoot(), ResolveClassYear(classYear));

    private string ResolveClassYear(string? classYear)
    {
        if (!string.IsNullOrWhiteSpace(classYear))
        {
            var trimmed = classYear.Trim();
            if (trimmed.Length == 4 && trimmed.All(char.IsAsciiDigit))
            {
                return trimmed;
            }
        }

        return string.IsNullOrWhiteSpace(_options.ClassYear) ? "2025" : _options.ClassYear.Trim();
    }

    private string ResolveOutputRoot()
    {
        var root = string.IsNullOrWhiteSpace(_options.OutputRoot) ? "output" : _options.OutputRoot;
        return Path.GetFullPath(root);
    }

    private static string SanitizeSchoolCode(string schoolCode)
    {
        if (string.IsNullOrWhiteSpace(schoolCode)
            || schoolCode.Contains("..", StringComparison.Ordinal)
            || schoolCode.IndexOfAny(Path.GetInvalidFileNameChars()) >= 0)
        {
            throw new InvalidOperationException($"School code '{schoolCode}' is not a valid output folder name.");
        }

        return schoolCode;
    }

    private static void TryDelete(string? path)
    {
        if (string.IsNullOrWhiteSpace(path) || !File.Exists(path))
        {
            return;
        }

        try
        {
            File.Delete(path);
        }
        catch (IOException)
        {
        }
    }

    private static string FormatDuration(TimeSpan duration) =>
        $"{Math.Max(0, (long)duration.TotalMilliseconds)} ms";

    private static string Truncate(string? value)
    {
        if (string.IsNullOrEmpty(value) || value.Length <= 2000)
        {
            return value ?? string.Empty;
        }

        return value[..2000];
    }
}
