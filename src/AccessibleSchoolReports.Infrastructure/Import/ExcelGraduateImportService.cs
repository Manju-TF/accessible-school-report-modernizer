using System.Security.Cryptography;
using AccessibleSchoolReports.Application.Imports;
using AccessibleSchoolReports.Domain.Entities;
using AccessibleSchoolReports.Domain.Persistence;
using AccessibleSchoolReports.Domain.Recodes;
using AccessibleSchoolReports.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace AccessibleSchoolReports.Infrastructure.Import;

public sealed class ExcelGraduateImportService : IGraduateImportService
{
    private readonly SchoolReportsDbContext _db;

    public ExcelGraduateImportService(SchoolReportsDbContext db)
    {
        _db = db;
    }

    public async Task<GraduateImportResult> ImportAsync(
        Stream excelStream,
        string? fileName,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(excelStream);

        var buffer = new MemoryStream();
        await excelStream.CopyToAsync(buffer, cancellationToken);
        buffer.Position = 0;
        var contentSha256 = Convert.ToHexString(SHA256.HashData(buffer)).ToLowerInvariant();
        buffer.Position = 0;

        var safeFileName = string.IsNullOrWhiteSpace(fileName)
            ? null
            : Path.GetFileName(fileName);

        var duplicate = await _db.ImportRuns
            .AsNoTracking()
            .Where(run =>
                run.ContentSha256 == contentSha256
                && run.ImportedRowCount > 0
                && (run.Status == RunStatus.Completed || run.Status == RunStatus.CompletedWithErrors))
            .OrderBy(run => run.Id)
            .FirstOrDefaultAsync(cancellationToken);

        if (duplicate is not null)
        {
            return new GraduateImportResult
            {
                ImportRunId = duplicate.Id,
                Status = duplicate.Status,
                ImportedRowCount = duplicate.ImportedRowCount,
                InvalidRowCount = duplicate.InvalidRowCount,
                BlankRowCount = duplicate.BlankRowCount,
                WasDuplicate = true,
                DuplicateOfImportRunId = duplicate.Id,
                Message = $"This file was already imported as ImportRun {duplicate.Id}.",
                Issues = [],
            };
        }

        var parsed = ExcelGraduateWorkbookParser.Parse(buffer);
        var startedUtc = DateTimeOffset.UtcNow;

        await using var transaction = await _db.Database.BeginTransactionAsync(cancellationToken);
        var importRun = new ImportRun
        {
            FileName = safeFileName,
            ContentSha256 = contentSha256,
            StartedUtc = startedUtc,
            Status = RunStatus.Running,
        };
        _db.ImportRuns.Add(importRun);
        await _db.SaveChangesAsync(cancellationToken);

        var issues = parsed.FileIssues.Concat(parsed.RowIssues).ToList();
        if (!parsed.HasRequiredColumns || parsed.FileIssues.Count > 0)
        {
            importRun.Status = RunStatus.Failed;
            importRun.CompletedUtc = DateTimeOffset.UtcNow;
            importRun.InvalidRowCount = issues.Count;
            importRun.Message = Truncate(Summarize(issues), 2000);
            PersistIssues(importRun.Id, issues);
            await _db.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);
            return ToResult(importRun, issues, wasDuplicate: false);
        }

        var schoolCodes = parsed.ValidRows
            .Select(row => row.SchoolCode)
            .Distinct(StringComparer.Ordinal)
            .ToArray();
        var existingSchools = await _db.Schools
            .Where(school => schoolCodes.Contains(school.Code))
            .ToListAsync(cancellationToken);
        var schoolsByCode = existingSchools.ToDictionary(school => school.Code, StringComparer.Ordinal);
        foreach (var code in schoolCodes)
        {
            var sasName = LegacySchoolNames.Lookup(code);
            if (schoolsByCode.TryGetValue(code, out var existing))
            {
                if (sasName is not null && existing.Name != sasName)
                {
                    existing.Name = sasName;
                }

                continue;
            }

            var school = new School { Code = code, Name = sasName };
            _db.Schools.Add(school);
            schoolsByCode[code] = school;
        }

        if (schoolCodes.Length > 0)
        {
            await _db.SaveChangesAsync(cancellationToken);
        }

        foreach (var row in parsed.ValidRows)
        {
            _db.GraduateRecords.Add(new GraduateRecord
            {
                ImportRunId = importRun.Id,
                SchoolId = schoolsByCode[row.SchoolCode].Id,
                Sex3 = row.Sex3,
                Minstat = row.Minstat,
                Jobcat1 = row.Jobcat1,
                JobFtPt = row.JobFtPt,
                Empgen = row.Empgen,
                Firm1 = row.Firm1,
                Lfjob = row.Lfjob,
                Jobreg = row.Jobreg,
                LocationFlag = row.LocationFlag,
                Jobst = row.Jobst,
                Source = row.Source,
                Time1 = row.Time1,
                Status = row.Status,
                Duration = row.Duration,
                SchoolFund = row.SchoolFund,
                SalFtPerm = row.SalFtPerm,
                Emptype1 = row.Emptype1,
            });
        }

        PersistIssues(importRun.Id, parsed.RowIssues);

        importRun.ImportedRowCount = parsed.ValidRows.Count;
        importRun.InvalidRowCount = parsed.RowIssues.Count;
        importRun.BlankRowCount = parsed.BlankRowCount;
        importRun.CompletedUtc = DateTimeOffset.UtcNow;
        importRun.Status = parsed.ValidRows.Count == 0
            ? RunStatus.Failed
            : parsed.RowIssues.Count == 0
                ? RunStatus.Completed
                : RunStatus.CompletedWithErrors;
        importRun.Message = Truncate(Summarize(parsed.RowIssues), 2000);

        await _db.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);
        return ToResult(importRun, parsed.FileIssues.Concat(parsed.RowIssues).ToList(), wasDuplicate: false);
    }

    private void PersistIssues(int importRunId, IEnumerable<ImportValidationIssue> issues)
    {
        foreach (var issue in issues)
        {
            _db.ImportRowIssues.Add(new ImportRowIssue
            {
                ImportRunId = importRunId,
                RowNumber = issue.RowNumber,
                Reason = Truncate(issue.Reason, 2000) ?? issue.Reason,
            });
        }
    }

    private static GraduateImportResult ToResult(
        ImportRun importRun,
        IReadOnlyList<ImportValidationIssue> issues,
        bool wasDuplicate)
    {
        return new GraduateImportResult
        {
            ImportRunId = importRun.Id,
            Status = importRun.Status,
            ImportedRowCount = importRun.ImportedRowCount,
            InvalidRowCount = importRun.InvalidRowCount,
            BlankRowCount = importRun.BlankRowCount,
            WasDuplicate = wasDuplicate,
            Message = importRun.Message,
            Issues = issues,
        };
    }

    private static string? Summarize(IReadOnlyList<ImportValidationIssue> issues)
    {
        if (issues.Count == 0)
        {
            return null;
        }

        return string.Join(" | ", issues.Take(5).Select(issue => $"Row {issue.RowNumber}: {issue.Reason}"));
    }

    private static string? Truncate(string? value, int maxLength)
    {
        if (string.IsNullOrEmpty(value) || value.Length <= maxLength)
        {
            return value;
        }

        return value[..(maxLength - 3)] + "...";
    }
}
