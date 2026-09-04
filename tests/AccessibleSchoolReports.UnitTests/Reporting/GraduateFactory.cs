using AccessibleSchoolReports.Domain.Entities;

namespace AccessibleSchoolReports.UnitTests.Reporting;

internal static class GraduateFactory
{
    public static GraduateRecord Create(
        string? sex3 = "F",
        string? minstat = "NONMIN",
        string? jobcat1 = "LJD",
        string? jobFtPt = "FULL",
        string? empgen = "FIRM",
        string? firm1 = "1",
        string? lfjob = "ATTY",
        string? jobreg = "1",
        string? locationFlag = "INSTATE",
        string? jobst = "107",
        string? source = "JOBPST",
        string? time1 = "BGRAD",
        string? status = "SET",
        string? duration = "PERM",
        string? schoolFund = "NO",
        decimal? salFtPerm = null,
        string? emptype1 = null)
    {
        return new GraduateRecord
        {
            Sex3 = sex3,
            Minstat = minstat,
            Jobcat1 = jobcat1,
            JobFtPt = jobFtPt,
            Empgen = empgen,
            Firm1 = firm1,
            Lfjob = lfjob,
            Jobreg = jobreg,
            LocationFlag = locationFlag,
            Jobst = jobst,
            Source = source,
            Time1 = time1,
            Status = status,
            Duration = duration,
            SchoolFund = schoolFund,
            SalFtPerm = salFtPerm,
            Emptype1 = emptype1,
        };
    }
}
