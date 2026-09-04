namespace AccessibleSchoolReports.Domain.Entities;

/// <summary>
/// One invalid Excel row from an import. Invalid data is stored, not discarded.
/// </summary>
public sealed class ImportRowIssue
{
    public int Id { get; set; }

    public int ImportRunId { get; set; }

    public ImportRun ImportRun { get; set; } = null!;

    public int RowNumber { get; set; }

    public string Reason { get; set; } = "";
}
