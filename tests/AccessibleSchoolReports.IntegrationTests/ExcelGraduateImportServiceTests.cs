using AccessibleSchoolReports.Application.Imports;
using AccessibleSchoolReports.Domain.Persistence;
using AccessibleSchoolReports.Infrastructure.Import;
using ClosedXML.Excel;
using Microsoft.EntityFrameworkCore;

namespace AccessibleSchoolReports.IntegrationTests;

public sealed class ExcelGraduateImportServiceTests
{
    [Fact]
    public async Task Import_PersistsValidRowsAndImportRun()
    {
        await using var db = await SqliteTestDatabase.CreateAsync();
        var service = new ExcelGraduateImportService(db.Context);
        using var stream = CreateWorkbook(
            GraduateImportColumns.Required,
            ValidRow("10701", 85000m, "W"),
            ValidRow("10702", null, "M"));

        var result = await service.ImportAsync(stream, "sample-export.xlsx");

        Assert.False(result.WasDuplicate);
        Assert.Equal(RunStatus.Completed, result.Status);
        Assert.Equal(2, result.ImportedRowCount);
        Assert.Equal(0, result.InvalidRowCount);
        Assert.NotNull(result.ImportRunId);

        var run = await db.Context.ImportRuns.SingleAsync();
        Assert.Equal("sample-export.xlsx", run.FileName);
        Assert.False(string.IsNullOrWhiteSpace(run.ContentSha256));
        Assert.Equal(2, run.ImportedRowCount);
        Assert.Equal(RunStatus.Completed, run.Status);

        var graduates = await db.Context.GraduateRecords.Include(g => g.School).OrderBy(g => g.School.Code).ToListAsync();
        Assert.Equal(2, graduates.Count);
        Assert.Equal("10701", graduates[0].School.Code);
        Assert.Equal("W", graduates[0].Sex3);
        Assert.Equal(85000m, graduates[0].SalFtPerm);
        Assert.Equal("10702", graduates[1].School.Code);
        Assert.Null(graduates[1].SalFtPerm);
        Assert.Equal(2, await db.Context.Schools.CountAsync());
    }

    [Fact]
    public async Task Import_CapturesInvalidRows_AndPersistsValidOnes()
    {
        await using var db = await SqliteTestDatabase.CreateAsync();
        var service = new ExcelGraduateImportService(db.Context);
        var invalid = ValidRow("10701");
        invalid[GraduateImportColumns.SalFtPerm] = "abc";
        using var stream = CreateWorkbook(
            GraduateImportColumns.Required,
            invalid,
            ValidRow("10702", 90000m));

        var result = await service.ImportAsync(stream, "mixed.xlsx");

        Assert.Equal(RunStatus.CompletedWithErrors, result.Status);
        Assert.Equal(1, result.ImportedRowCount);
        Assert.Equal(1, result.InvalidRowCount);
        var issue = Assert.Single(result.Issues);
        Assert.Equal(2, issue.RowNumber);
        Assert.Contains("salftperm", issue.Reason, StringComparison.OrdinalIgnoreCase);

        Assert.Equal("10702", Assert.Single(await db.Context.GraduateRecords.Include(g => g.School).ToListAsync()).School.Code);
        var storedIssue = await db.Context.ImportRowIssues.SingleAsync();
        Assert.Equal(2, storedIssue.RowNumber);
        Assert.Equal(issue.Reason, storedIssue.Reason);
    }

    [Fact]
    public async Task Import_IgnoresBlankRows()
    {
        await using var db = await SqliteTestDatabase.CreateAsync();
        var service = new ExcelGraduateImportService(db.Context);
        using var stream = CreateWorkbook(
            GraduateImportColumns.Required,
            ValidRow("10701"),
            new Dictionary<string, object?>(),
            ValidRow("10701", 1m, "N"));

        var result = await service.ImportAsync(stream, "blanks.xlsx");

        Assert.Equal(2, result.ImportedRowCount);
        Assert.Equal(1, result.BlankRowCount);
        Assert.Empty(result.Issues);
        Assert.Equal(1, await db.Context.Schools.CountAsync());
    }

    [Fact]
    public async Task Import_MissingRequiredColumns_RecordsFailedRun()
    {
        await using var db = await SqliteTestDatabase.CreateAsync();
        var service = new ExcelGraduateImportService(db.Context);
        var headers = GraduateImportColumns.Required.Where(name => name != GraduateImportColumns.Code).ToArray();
        using var stream = CreateWorkbook(headers, ValidRow("10701"));

        var result = await service.ImportAsync(stream, "missing-code.xlsx");

        Assert.Equal(RunStatus.Failed, result.Status);
        Assert.Equal(0, result.ImportedRowCount);
        Assert.True(result.InvalidRowCount > 0);
        Assert.Empty(await db.Context.GraduateRecords.ToListAsync());
        var run = await db.Context.ImportRuns.SingleAsync();
        Assert.Equal(RunStatus.Failed, run.Status);
        Assert.Contains("code", Assert.Single(await db.Context.ImportRowIssues.ToListAsync()).Reason);
    }

    [Fact]
    public async Task Import_SameFile_IsRejectedAsDuplicate()
    {
        await using var db = await SqliteTestDatabase.CreateAsync();
        var service = new ExcelGraduateImportService(db.Context);
        var bytes = CreateWorkbook(GraduateImportColumns.Required, ValidRow("10701")).ToArray();

        var first = await service.ImportAsync(new MemoryStream(bytes), "once.xlsx");
        var second = await service.ImportAsync(new MemoryStream(bytes), "once.xlsx");

        Assert.False(first.WasDuplicate);
        Assert.True(second.WasDuplicate);
        Assert.Equal(first.ImportRunId, second.DuplicateOfImportRunId);
        Assert.Equal(1, await db.Context.ImportRuns.CountAsync());
        Assert.Equal(1, await db.Context.GraduateRecords.CountAsync());
    }

    [Fact]
    public async Task Import_FailedRun_DoesNotBlockRetryAfterFix()
    {
        await using var db = await SqliteTestDatabase.CreateAsync();
        var service = new ExcelGraduateImportService(db.Context);
        var headers = GraduateImportColumns.Required.Where(name => name != GraduateImportColumns.Code).ToArray();
        using var bad = CreateWorkbook(headers, ValidRow("10701"));
        using var good = CreateWorkbook(GraduateImportColumns.Required, ValidRow("10701"));

        var failed = await service.ImportAsync(bad, "retry.xlsx");
        var retry = await service.ImportAsync(good, "retry.xlsx");

        Assert.Equal(RunStatus.Failed, failed.Status);
        Assert.False(retry.WasDuplicate);
        Assert.Equal(RunStatus.Completed, retry.Status);
        Assert.Equal(1, await db.Context.GraduateRecords.CountAsync());
        Assert.Equal(2, await db.Context.ImportRuns.CountAsync());
    }

    [Fact]
    public async Task Import_SampleExport_PersistsAllRows()
    {
        var samplePath = FindSampleExport();
        await using var db = await SqliteTestDatabase.CreateAsync();
        var service = new ExcelGraduateImportService(db.Context);
        await using var stream = File.OpenRead(samplePath);

        var result = await service.ImportAsync(stream, "sample-export.xlsx");

        Assert.Equal(RunStatus.Completed, result.Status);
        Assert.Equal(3534, result.ImportedRowCount);
        Assert.Equal(0, result.InvalidRowCount);
        Assert.Equal(189, await db.Context.Schools.CountAsync());
        Assert.Equal(3534, await db.Context.GraduateRecords.CountAsync());
        Assert.Equal(0, await db.Context.ImportRowIssues.CountAsync());
    }

    private static string FindSampleExport()
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);
        while (directory is not null)
        {
            var candidate = Path.Combine(directory.FullName, "legacy", "samples", "sample-export.xlsx");
            if (File.Exists(candidate))
            {
                return candidate;
            }

            directory = directory.Parent;
        }

        throw new FileNotFoundException("legacy/samples/sample-export.xlsx");
    }

    private static MemoryStream CreateWorkbook(
        IReadOnlyList<string> headers,
        params IReadOnlyDictionary<string, object?>[] rows)
    {
        var stream = new MemoryStream();
        using var workbook = new XLWorkbook();
        var worksheet = workbook.AddWorksheet("Sheet1");
        for (var column = 0; column < headers.Count; column++)
        {
            worksheet.Cell(1, column + 1).Value = headers[column];
        }

        for (var rowIndex = 0; rowIndex < rows.Length; rowIndex++)
        {
            var row = rows[rowIndex];
            for (var column = 0; column < headers.Count; column++)
            {
                if (!row.TryGetValue(headers[column], out var value) || value is null)
                {
                    continue;
                }

                var cell = worksheet.Cell(rowIndex + 2, column + 1);
                switch (value)
                {
                    case int number:
                        cell.Value = number;
                        break;
                    case decimal number:
                        cell.Value = number;
                        break;
                    case double number:
                        cell.Value = number;
                        break;
                    default:
                        cell.Value = value.ToString();
                        break;
                }
            }
        }

        workbook.SaveAs(stream);
        stream.Position = 0;
        return stream;
    }

    private static Dictionary<string, object?> ValidRow(
        string code,
        decimal? salary = 85000m,
        string? sex3 = "F")
    {
        return new Dictionary<string, object?>(StringComparer.Ordinal)
        {
            [GraduateImportColumns.Code] = code,
            [GraduateImportColumns.Sex3] = sex3,
            [GraduateImportColumns.Minstat] = "NONMIN",
            [GraduateImportColumns.Jobcat1] = "LJD",
            [GraduateImportColumns.JobFtPt] = "FULL",
            [GraduateImportColumns.Empgen] = "FIRM",
            [GraduateImportColumns.Firm1] = "1",
            [GraduateImportColumns.Lfjob] = "ATTY",
            [GraduateImportColumns.Jobreg] = "1",
            [GraduateImportColumns.LocationFlag] = "INSTATE",
            [GraduateImportColumns.Jobst] = "107",
            [GraduateImportColumns.Source] = "JOBPST",
            [GraduateImportColumns.Time1] = "BGRAD",
            [GraduateImportColumns.Status] = "SET",
            [GraduateImportColumns.Duration] = "PERM",
            [GraduateImportColumns.SchoolFund] = "NO",
            [GraduateImportColumns.SalFtPerm] = salary,
        };
    }
}
