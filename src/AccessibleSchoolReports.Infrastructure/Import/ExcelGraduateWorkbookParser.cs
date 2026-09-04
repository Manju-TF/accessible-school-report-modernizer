using System.Globalization;
using AccessibleSchoolReports.Application.Imports;
using ClosedXML.Excel;

namespace AccessibleSchoolReports.Infrastructure.Import;

internal static class ExcelGraduateWorkbookParser
{
    public static ParsedWorkbook Parse(Stream excelStream)
    {
        XLWorkbook workbook;
        try
        {
            workbook = new XLWorkbook(excelStream);
        }
        catch (Exception ex)
        {
            return new ParsedWorkbook
            {
                FileIssues =
                [
                    new ImportValidationIssue(0, $"The file is not a valid Excel workbook: {ex.Message}"),
                ],
                MissingRequiredColumns = GraduateImportColumns.Required,
            };
        }

        using (workbook)
        {
            var worksheet = workbook.Worksheets.FirstOrDefault();
            if (worksheet is null)
            {
                return new ParsedWorkbook
                {
                    FileIssues = [new ImportValidationIssue(0, "The workbook has no worksheets.")],
                    MissingRequiredColumns = GraduateImportColumns.Required,
                };
            }

            var used = worksheet.RangeUsed();
            if (used is null)
            {
                return new ParsedWorkbook
                {
                    FileIssues = [new ImportValidationIssue(1, "The worksheet is empty.")],
                    MissingRequiredColumns = GraduateImportColumns.Required,
                };
            }

            var headerRowNumber = used.FirstRow().RowNumber();
            var headerMap = new Dictionary<string, int>(StringComparer.Ordinal);
            var headerIssues = new List<ImportValidationIssue>();
            var firstColumn = used.FirstColumn().ColumnNumber();
            var lastColumn = used.LastColumn().ColumnNumber();

            for (var column = firstColumn; column <= lastColumn; column++)
            {
                var raw = worksheet.Cell(headerRowNumber, column).GetString();
                var normalized = ExcelHeaderNormalizer.Normalize(raw);
                if (normalized.Length == 0)
                {
                    continue;
                }

                if (!headerMap.TryAdd(normalized, column))
                {
                    headerIssues.Add(
                        new ImportValidationIssue(headerRowNumber, $"Duplicate column header '{raw}'."));
                }
            }

            var missing = GraduateImportColumns.Required
                .Where(required => !headerMap.ContainsKey(required))
                .ToArray();
            if (missing.Length > 0)
            {
                headerIssues.Add(
                    new ImportValidationIssue(
                        headerRowNumber,
                        "Missing required column(s): " + string.Join(", ", missing) + "."));
                return new ParsedWorkbook
                {
                    MissingRequiredColumns = missing,
                    FileIssues = headerIssues,
                };
            }

            if (headerIssues.Count > 0)
            {
                return new ParsedWorkbook
                {
                    FileIssues = headerIssues,
                };
            }

            var validRows = new List<ParsedGraduateRow>();
            var rowIssues = new List<ImportValidationIssue>();
            var blankRowCount = 0;
            var lastRow = used.LastRow().RowNumber();
            var firstDataRow = headerRowNumber + 1;

            for (var rowNumber = firstDataRow; rowNumber <= lastRow; rowNumber++)
            {
                var row = worksheet.Row(rowNumber);
                var cells = new Dictionary<string, IXLCell>(StringComparer.Ordinal);
                foreach (var (name, column) in headerMap)
                {
                    cells[name] = row.Cell(column);
                }

                if (IsBlankRow(cells))
                {
                    blankRowCount++;
                    continue;
                }

                var reasons = new List<string>();
                var texts = new Dictionary<string, string?>(StringComparer.Ordinal);
                foreach (var name in GraduateImportColumns.Required.Concat(GraduateImportColumns.Optional))
                {
                    if (!cells.TryGetValue(name, out var cell))
                    {
                        texts[name] = null;
                        continue;
                    }

                    if (name == GraduateImportColumns.SalFtPerm)
                    {
                        continue;
                    }

                    var text = ReadText(cell);
                    if (text is not null
                        && GraduateImportColumns.TextMaxLengths.TryGetValue(name, out var maxLength)
                        && text.Length > maxLength)
                    {
                        reasons.Add($"{name} exceeds {maxLength} characters.");
                    }

                    texts[name] = text;
                }

                if (string.IsNullOrWhiteSpace(texts[GraduateImportColumns.Code]))
                {
                    reasons.Add("code is required.");
                }

                decimal? salary = null;
                if (headerMap.ContainsKey(GraduateImportColumns.SalFtPerm)
                    && !TryReadSalary(cells[GraduateImportColumns.SalFtPerm], out salary, out var salaryError)
                    && salaryError is not null)
                {
                    reasons.Add(salaryError);
                }

                if (reasons.Count > 0)
                {
                    rowIssues.Add(new ImportValidationIssue(rowNumber, string.Join(" ", reasons)));
                    continue;
                }

                validRows.Add(new ParsedGraduateRow
                {
                    ExcelRowNumber = rowNumber,
                    SchoolCode = texts[GraduateImportColumns.Code]!,
                    Sex3 = texts[GraduateImportColumns.Sex3],
                    Minstat = texts[GraduateImportColumns.Minstat],
                    Jobcat1 = texts[GraduateImportColumns.Jobcat1],
                    JobFtPt = texts[GraduateImportColumns.JobFtPt],
                    Empgen = texts[GraduateImportColumns.Empgen],
                    Firm1 = texts[GraduateImportColumns.Firm1],
                    Lfjob = texts[GraduateImportColumns.Lfjob],
                    Jobreg = texts[GraduateImportColumns.Jobreg],
                    LocationFlag = texts[GraduateImportColumns.LocationFlag],
                    Jobst = texts[GraduateImportColumns.Jobst],
                    Source = texts[GraduateImportColumns.Source],
                    Time1 = texts[GraduateImportColumns.Time1],
                    Status = texts[GraduateImportColumns.Status],
                    Duration = texts[GraduateImportColumns.Duration],
                    SchoolFund = texts[GraduateImportColumns.SchoolFund],
                    SalFtPerm = salary,
                    Emptype1 = texts.GetValueOrDefault(GraduateImportColumns.Emptype1),
                });
            }

            return new ParsedWorkbook
            {
                ValidRows = validRows,
                RowIssues = rowIssues,
                BlankRowCount = blankRowCount,
            };
        }
    }

    private static bool IsBlankRow(IReadOnlyDictionary<string, IXLCell> cells)
    {
        foreach (var cell in cells.Values)
        {
            if (!cell.IsEmpty() && !string.IsNullOrWhiteSpace(cell.GetFormattedString()))
            {
                return false;
            }
        }

        return true;
    }

    private static string? ReadText(IXLCell cell)
    {
        if (cell.IsEmpty() || cell.DataType == XLDataType.Blank)
        {
            return null;
        }

        if (cell.DataType == XLDataType.Error)
        {
            return null;
        }

        if (cell.DataType == XLDataType.Number)
        {
            var number = cell.GetDouble();
            if (double.IsNaN(number) || double.IsInfinity(number))
            {
                return null;
            }

            if (Math.Abs(number - Math.Round(number, MidpointRounding.AwayFromZero)) < 1e-9)
            {
                return Convert.ToInt64(Math.Round(number, MidpointRounding.AwayFromZero))
                    .ToString(CultureInfo.InvariantCulture);
            }

            return number.ToString("G", CultureInfo.InvariantCulture);
        }

        var text = cell.GetString().Trim();
        return text.Length == 0 ? null : text;
    }

    private static bool TryReadSalary(IXLCell cell, out decimal? value, out string? error)
    {
        value = null;
        error = null;
        if (cell.IsEmpty() || cell.DataType == XLDataType.Blank)
        {
            return true;
        }

        if (cell.DataType == XLDataType.Error)
        {
            error = "salftperm is not a valid number.";
            return false;
        }

        if (cell.DataType is XLDataType.Boolean or XLDataType.DateTime)
        {
            error = "salftperm is not a valid number.";
            return false;
        }

        if (cell.DataType == XLDataType.Number)
        {
            try
            {
                value = cell.GetValue<decimal>();
                return true;
            }
            catch (Exception)
            {
                error = "salftperm is not a valid number.";
                return false;
            }
        }

        var text = cell.GetString().Trim();
        if (text.Length == 0)
        {
            return true;
        }

        if (decimal.TryParse(text, NumberStyles.Number, CultureInfo.InvariantCulture, out var parsed))
        {
            value = parsed;
            return true;
        }

        error = $"salftperm '{text}' is not a valid number.";
        return false;
    }
}
