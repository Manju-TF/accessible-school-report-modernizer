using AccessibleSchoolReports.Application.Imports;
using AccessibleSchoolReports.Infrastructure.Import;

namespace AccessibleSchoolReports.UnitTests.Imports;

public sealed class ExcelGraduateWorkbookParserTests
{
    [Fact]
    public void Parse_MapsHeadersStartingAfterColumnA()
    {
        using var stream = new MemoryStream();
        using (var workbook = new ClosedXML.Excel.XLWorkbook())
        {
            var worksheet = workbook.AddWorksheet("Sheet1");
            var row = TestExcelWorkbook.ValidRow("10701", 50000m);
            var headers = GraduateImportColumns.Required;
            for (var column = 0; column < headers.Count; column++)
            {
                worksheet.Cell(1, column + 2).Value = headers[column];
                if (row.TryGetValue(headers[column], out var value) && value is not null)
                {
                    var cell = worksheet.Cell(2, column + 2);
                    switch (value)
                    {
                        case decimal number:
                            cell.Value = number;
                            break;
                        case int number:
                            cell.Value = number;
                            break;
                        default:
                            cell.Value = value.ToString();
                            break;
                    }
                }
            }

            workbook.SaveAs(stream);
        }

        stream.Position = 0;
        var parsed = ExcelGraduateWorkbookParser.Parse(stream);
        var graduate = Assert.Single(parsed.ValidRows);
        Assert.Equal("10701", graduate.SchoolCode);
        Assert.Equal(50000m, graduate.SalFtPerm);
    }

    [Fact]
    public void Parse_MapsByHeaderName_NotColumnPosition()
    {
        var reversed = GraduateImportColumns.Required.Reverse().ToArray();
        using var stream = TestExcelWorkbook.Create(
            reversed,
            TestExcelWorkbook.ValidRow("10701", 100500m, "W"));

        var parsed = ExcelGraduateWorkbookParser.Parse(stream);

        Assert.True(parsed.HasRequiredColumns);
        var row = Assert.Single(parsed.ValidRows);
        Assert.Equal(2, row.ExcelRowNumber);
        Assert.Equal("10701", row.SchoolCode);
        Assert.Equal("W", row.Sex3);
        Assert.Equal(100500m, row.SalFtPerm);
        Assert.Empty(parsed.RowIssues);
    }

    [Fact]
    public void Parse_MissingRequiredColumn_IsFileError()
    {
        var headers = GraduateImportColumns.Required.Where(name => name != GraduateImportColumns.Code).ToArray();
        var row = TestExcelWorkbook.ValidRow("10701");
        using var stream = TestExcelWorkbook.Create(headers, row);

        var parsed = ExcelGraduateWorkbookParser.Parse(stream);

        Assert.False(parsed.HasRequiredColumns);
        Assert.Contains(GraduateImportColumns.Code, parsed.MissingRequiredColumns);
        Assert.Contains(parsed.FileIssues, issue => issue.RowNumber == 1 && issue.Reason.Contains("code"));
        Assert.Empty(parsed.ValidRows);
    }

    [Fact]
    public void Parse_IgnoresBlankRows()
    {
        using var stream = TestExcelWorkbook.Create(
            GraduateImportColumns.Required,
            TestExcelWorkbook.ValidRow("10701"),
            new Dictionary<string, object?>(),
            TestExcelWorkbook.ValidRow("10702"));

        var parsed = ExcelGraduateWorkbookParser.Parse(stream);

        Assert.Equal(2, parsed.ValidRows.Count);
        Assert.Equal(1, parsed.BlankRowCount);
        Assert.Empty(parsed.RowIssues);
        Assert.Equal(new[] { "10701", "10702" }, parsed.ValidRows.Select(row => row.SchoolCode));
    }

    [Fact]
    public void Parse_InvalidSalary_IsCapturedWithRowNumber()
    {
        var row = TestExcelWorkbook.ValidRow("10701");
        row[GraduateImportColumns.SalFtPerm] = "not-a-salary";
        using var stream = TestExcelWorkbook.Create(GraduateImportColumns.Required, row);

        var parsed = ExcelGraduateWorkbookParser.Parse(stream);

        Assert.Empty(parsed.ValidRows);
        var issue = Assert.Single(parsed.RowIssues);
        Assert.Equal(2, issue.RowNumber);
        Assert.Contains("salftperm", issue.Reason, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void Parse_MissingSchoolCode_IsCaptured()
    {
        var row = TestExcelWorkbook.ValidRow("10701");
        row[GraduateImportColumns.Code] = null;
        using var stream = TestExcelWorkbook.Create(GraduateImportColumns.Required, row);

        var parsed = ExcelGraduateWorkbookParser.Parse(stream);

        Assert.Empty(parsed.ValidRows);
        var issue = Assert.Single(parsed.RowIssues);
        Assert.Equal(2, issue.RowNumber);
        Assert.Contains("code is required", issue.Reason);
    }

    [Fact]
    public void Parse_DoesNotApplySex3Recodes()
    {
        using var stream = TestExcelWorkbook.Create(
            GraduateImportColumns.Required,
            TestExcelWorkbook.ValidRow("10701", sex3: "W"));

        var parsed = ExcelGraduateWorkbookParser.Parse(stream);

        Assert.Equal("W", Assert.Single(parsed.ValidRows).Sex3);
    }

    [Fact]
    public void Parse_IgnoresUnmappedColumns()
    {
        var headers = GraduateImportColumns.Required.Concat(["bizjobtype", "city1"]).ToArray();
        var row = TestExcelWorkbook.ValidRow("10701");
        row["bizjobtype"] = "EXTRA";
        row["city1"] = "10716";
        using var stream = TestExcelWorkbook.Create(headers, row);

        var parsed = ExcelGraduateWorkbookParser.Parse(stream);

        Assert.Empty(parsed.RowIssues);
        Assert.Single(parsed.ValidRows);
    }

    [Fact]
    public void Parse_OptionalEmptype1_IsMappedWhenPresent()
    {
        var headers = GraduateImportColumns.Required.Concat([GraduateImportColumns.Emptype1]).ToArray();
        var row = TestExcelWorkbook.ValidRow("10701");
        row[GraduateImportColumns.Emptype1] = "FED";
        using var stream = TestExcelWorkbook.Create(headers, row);

        var parsed = ExcelGraduateWorkbookParser.Parse(stream);

        Assert.Equal("FED", Assert.Single(parsed.ValidRows).Emptype1);
    }

    [Fact]
    public void Parse_InvalidWorkbook_IsCaptured()
    {
        using var stream = new MemoryStream("this is not excel"u8.ToArray());

        var parsed = ExcelGraduateWorkbookParser.Parse(stream);

        Assert.False(parsed.HasRequiredColumns);
        Assert.Contains(parsed.FileIssues, issue => issue.Reason.Contains("not a valid Excel workbook"));
    }
}
