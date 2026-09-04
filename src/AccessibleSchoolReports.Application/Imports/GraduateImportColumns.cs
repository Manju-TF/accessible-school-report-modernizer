namespace AccessibleSchoolReports.Application.Imports;

/// <summary>
/// Excel columns mapped by normalized header name to SAS builder inputs.
/// Observed on <c>legacy/samples/sample-export.xlsx</c> Sheet1.
/// Unmapped sample columns (not persisted): bizjobtype, city1, time2, basic,
/// level, saltype, govtjob_t, empgen_m, schst.
/// <c>emptype1</c> is a SAS input but is not in the sample; map it when present.
/// </summary>
public static class GraduateImportColumns
{
    public const string Code = "code";
    public const string Sex3 = "sex3";
    public const string Minstat = "minstat";
    public const string Jobcat1 = "jobcat1";
    public const string JobFtPt = "jobftpt";
    public const string Empgen = "empgen";
    public const string Firm1 = "firm1";
    public const string Lfjob = "lfjob";
    public const string Jobreg = "jobreg";
    public const string LocationFlag = "locationflag";
    public const string Jobst = "jobst";
    public const string Source = "source";
    public const string Time1 = "time1";
    public const string Status = "status";
    public const string Duration = "duration";
    public const string SchoolFund = "schoolfund";
    public const string SalFtPerm = "salftperm";
    public const string Emptype1 = "emptype1";

    public static readonly IReadOnlyList<string> Required =
    [
        Code,
        Sex3,
        Minstat,
        Jobcat1,
        JobFtPt,
        Empgen,
        Firm1,
        Lfjob,
        Jobreg,
        LocationFlag,
        Jobst,
        Source,
        Time1,
        Status,
        Duration,
        SchoolFund,
        SalFtPerm,
    ];

    public static readonly IReadOnlyList<string> Optional = [Emptype1];

    public static readonly IReadOnlyDictionary<string, int> TextMaxLengths =
        new Dictionary<string, int>(StringComparer.Ordinal)
        {
            [Code] = 32,
            [Sex3] = 16,
            [Minstat] = 16,
            [Jobcat1] = 16,
            [JobFtPt] = 16,
            [Empgen] = 16,
            [Firm1] = 16,
            [Lfjob] = 16,
            [Jobreg] = 16,
            [LocationFlag] = 32,
            [Jobst] = 16,
            [Source] = 16,
            [Time1] = 16,
            [Status] = 16,
            [Duration] = 16,
            [SchoolFund] = 16,
            [Emptype1] = 16,
        };
}
