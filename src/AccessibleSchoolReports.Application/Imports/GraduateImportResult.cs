using AccessibleSchoolReports.Domain.Persistence;

namespace AccessibleSchoolReports.Application.Imports;

public sealed class GraduateImportResult
{
    public int? ImportRunId { get; init; }

    public required RunStatus Status { get; init; }

    public int ImportedRowCount { get; init; }

    public int InvalidRowCount { get; init; }

    public int BlankRowCount { get; init; }

    public bool WasDuplicate { get; init; }

    public int? DuplicateOfImportRunId { get; init; }

    public string? Message { get; init; }

    public required IReadOnlyList<ImportValidationIssue> Issues { get; init; }
}
