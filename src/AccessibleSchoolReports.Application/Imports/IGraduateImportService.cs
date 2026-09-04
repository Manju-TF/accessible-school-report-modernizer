namespace AccessibleSchoolReports.Application.Imports;

public interface IGraduateImportService
{
    Task<GraduateImportResult> ImportAsync(
        Stream excelStream,
        string? fileName,
        CancellationToken cancellationToken = default);
}
