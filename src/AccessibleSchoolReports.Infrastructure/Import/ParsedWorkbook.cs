using AccessibleSchoolReports.Application.Imports;

namespace AccessibleSchoolReports.Infrastructure.Import;

internal sealed class ParsedWorkbook
{
    public IReadOnlyList<string> MissingRequiredColumns { get; init; } = [];

    public IReadOnlyList<ImportValidationIssue> FileIssues { get; init; } = [];

    public IReadOnlyList<ParsedGraduateRow> ValidRows { get; init; } = [];

    public IReadOnlyList<ImportValidationIssue> RowIssues { get; init; } = [];

    public int BlankRowCount { get; init; }

    public bool HasRequiredColumns => MissingRequiredColumns.Count == 0;
}
