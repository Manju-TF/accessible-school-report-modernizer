namespace AccessibleSchoolReports.Application.Reporting;

/// <summary>
/// Renders a calculated <see cref="SchoolReport"/> to a tagged, PDF/UA-oriented PDF.
/// Does not recalculate counts or salaries.
/// </summary>
public interface IAccessiblePdfGenerator
{
    void Generate(SchoolReport report, Stream output);

    byte[] Generate(SchoolReport report);
}
