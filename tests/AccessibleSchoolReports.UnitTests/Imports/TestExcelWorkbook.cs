using AccessibleSchoolReports.Application.Imports;
using ClosedXML.Excel;

namespace AccessibleSchoolReports.UnitTests.Imports;

internal static class TestExcelWorkbook
{
    public static MemoryStream Create(
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
                    case long number:
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

    public static Dictionary<string, object?> ValidRow(
        string code,
        decimal? salary = 85000m,
        string? sex3 = "F")
    {
        var row = GraduateImportColumns.Required.ToDictionary(
            header => header,
            _ => (object?)null,
            StringComparer.Ordinal);
        row[GraduateImportColumns.Code] = code;
        row[GraduateImportColumns.Sex3] = sex3;
        row[GraduateImportColumns.Minstat] = "NONMIN";
        row[GraduateImportColumns.Jobcat1] = "LJD";
        row[GraduateImportColumns.JobFtPt] = "FULL";
        row[GraduateImportColumns.Empgen] = "FIRM";
        row[GraduateImportColumns.Firm1] = "1";
        row[GraduateImportColumns.Lfjob] = "ATTY";
        row[GraduateImportColumns.Jobreg] = "1";
        row[GraduateImportColumns.LocationFlag] = "INSTATE";
        row[GraduateImportColumns.Jobst] = "107";
        row[GraduateImportColumns.Source] = "JOBPST";
        row[GraduateImportColumns.Time1] = "BGRAD";
        row[GraduateImportColumns.Status] = "SET";
        row[GraduateImportColumns.Duration] = "PERM";
        row[GraduateImportColumns.SchoolFund] = "NO";
        row[GraduateImportColumns.SalFtPerm] = salary;
        return row;
    }
}
