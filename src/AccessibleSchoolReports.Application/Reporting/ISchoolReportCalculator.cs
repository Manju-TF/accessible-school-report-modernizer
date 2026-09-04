using AccessibleSchoolReports.Domain.Entities;

namespace AccessibleSchoolReports.Application.Reporting;

public interface ISchoolReportCalculator
{
    SchoolReport Calculate(string schoolCode, IReadOnlyList<GraduateRecord> graduates);
}
