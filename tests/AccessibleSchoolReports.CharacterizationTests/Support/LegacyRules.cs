namespace AccessibleSchoolReports.CharacterizationTests.Support;

/// <summary>
/// Confirmed SAS mechanics only. No invented fallbacks, no percentile/mean formulas,
/// and no report-engine implementation.
/// </summary>
internal static class LegacyRules
{
    public static readonly IReadOnlyDictionary<string, string> JobcatFormat =
        AccessibleSchoolReports.Domain.Recodes.LegacyRecodes.JobCategoryFormat;

    public static readonly IReadOnlyDictionary<string, string> BuilderTimeFormat =
        new Dictionary<string, string>(StringComparer.Ordinal)
        {
            ["BGRAD"] = "Before Graduation",
            ["AFTGRD"] = "After Graduation",
        };

    public static readonly IReadOnlyDictionary<string, string> BuilderSourceFormat =
        new Dictionary<string, string>(StringComparer.Ordinal)
        {
            ["OCI"] = "Career office recruitment program (e.g., OCI)",
            ["JOBFRC"] = "Job fair or career conference",
            ["JOBPST"] = "Career office job posting",
            ["OTHER"] = "Other",
            ["PRNSMJ"] = "Returned to or continued with pre-law school employer",
            ["RFFRND"] = "Referral",
            ["SELFPR"] = "Started own business/practice",
            ["SLFINI"] = "Self-initiated contact/networking",
            ["TEMPAG"] = "Temp agency or legal search consultant",
            ["ONLINE"] = "Non-career office job posting",
            ["OSCAR"] = "Clerkship application process or OSCAR",
        };

    public static readonly IReadOnlyDictionary<string, string> ReportRowLabels =
        new Dictionary<string, string>(StringComparer.Ordinal)
        {
            ["F"] = "Women",
            ["M"] = "Men",
            ["N"] = "Non-binary or Chose to Self-identify",
            ["NONMIN"] = "White",
            ["MINOR"] = "People of Color",
            ["NONMINF"] = "White Women",
            ["NONMINM"] = "White Men",
            ["1-LJD"] = "Bar Admission Required/ Anticipated",
            ["2-NLJD"] = "JD Advantage",
            ["3-NLP"] = "Other Professional",
            ["4-NLO"] = "Other Position",
            ["5-WUNK"] = "Job Type Unknown",
            ["6-ADVD"] = "Enrolled in Graduate Studies",
            ["7-UDEF"] = "Employed-Start Date after March 16, 2026",
            ["8-USKW"] = "Not Employed-Seeking",
            ["9-UNWK"] = "Not Employed-Not Seeking",
            ["EMPL"] = "Employed",
            ["ACAD"] = "Education",
            ["BUS"] = "Business",
            ["CLERK"] = "Judicial Clerkships",
            ["GOVT"] = "Government",
            ["FIRM"] = "Private Practice",
            ["PUBINT"] = "Public Interest",
            ["ZEMPUN"] = "Unknown Type",
            ["EMPUNK"] = "Unknown Type",
            ["SOLO"] = "Solo Practitioner",
            ["LF1"] = "1-10",
            ["LF2"] = "11-25",
            ["LF3"] = "26-50",
            ["LF4"] = "51-100",
            ["LF5"] = "101-250",
            ["LF6"] = "251-500",
            ["LF7"] = "501+",
            ["LF8"] = "Unknown Size",
            ["PRIVATE"] = "Private Sector",
            ["PUBLIC"] = "Public Sector",
            ["INSTATE"] = "In-State",
            ["OUTOFSTATE"] = "Out of State",
            ["FOREIGN"] = "Foreign",
            ["1"] = "New England",
            ["2"] = "Mid-Atlantic",
            ["3"] = "E North Central",
            ["4"] = "W North Central",
            ["5"] = "South Atlantic",
            ["6"] = "E South Central",
            ["7"] = "W South Central",
            ["8"] = "Mountain",
            ["9"] = "Pacific",
            ["T"] = "US Territories",
            ["X"] = "Non-US locations",
            ["BGRAD"] = "Before graduation",
            ["AOCI"] = "Career office recruitment program (e.g., OCI)",
            ["ZOTHER"] = "Other",
            ["1-LJDFULL"] = "Bar Admission Required/ Anticipated: Full-time",
            ["2-NLJDFULL"] = "JD Advantage: Full-time ",
            ["2-NLJDPART"] = "JD Advantage: Part-time ",
            ["ATTY"] = "Associate/Entry-level Attorney",
            ["LCLERK"] = "Law Clerk",
            ["NOTSET"] = "Seeking a different job",
            ["SET"] = "Not seeking a different job",
        };

    public static readonly IReadOnlyDictionary<string, string> SectionHeaders =
        new Dictionary<string, string>(StringComparer.Ordinal)
        {
            ["B"] = "Gender Reported:",
            ["C"] = "Race Reported:",
            ["C1"] = "Gender & Race Reported:",
            ["D"] = "Employment Status Known:",
            ["D1"] = "Total Employed or Enrolled in Graduate Studies:",
            ["D2"] = "Employment by Sector:",
            ["D3"] = "Full-time or Part-time Job Status:",
            ["E1"] = "Employment Categories:",
            ["E2"] = "Education Jobs:",
            ["E3"] = "Business Jobs:",
            ["E4"] = "Private Practice Jobs:",
            ["E5"] = "Government Jobs:",
            ["E55"] = "Judicial Clerkships:",
            ["E6"] = "Public Interest Jobs:",
            ["FIRM"] = "Size of Law Firm (by # of Attorneys):",
            ["FIRM2"] = "Type of Law Firm Job:",
            ["JOBREG1"] = "Jobs Taken by Region:",
            ["JOBREG2"] = "Location of Jobs:",
            ["JOBREG3"] = "# States and Territories with Employed Grads:",
            ["SOURCE"] = "Source of Job:",
            ["TIME"] = "Timing of Job Offer:",
            ["ZSTATUS"] = "Search Status of Employed Grads:",
            ["DURATION"] = "Duration of Jobs by Employer Type:",
            ["LAW SCHOOL FUNDED"] = "Total Number of Jobs Reported as Funded by Law School:",
        };

    public static readonly IReadOnlyList<string> Page1Analvars = ["B", "C", "C1", "D"];
    public static readonly IReadOnlyList<string> Page3Analvars = ["E2", "E3", "E4", "E5", "E55"];
    public static readonly IReadOnlyList<string> Page4Analvars = ["E6", "FIRM", "FIRM2"];
    public static readonly IReadOnlyList<string> Page5Analvars = ["JOBREG1", "JOBREG2", "JOBREG3"];
    public static readonly IReadOnlyList<string> Page6Analvars = ["SOURCE", "TIME", "ZSTATUS"];
    public static readonly IReadOnlyList<string> Page7Analvars = ["DURATION", "LAW SCHOOL FUNDED"];

    public static readonly IReadOnlyList<string> PublicEmpgen = ["ACAD", "GOVT", "CLERK", "PUBINT"];
    public static readonly IReadOnlyList<string> PrivateEmpgen = ["BUS", "FIRM"];
    public static readonly IReadOnlyList<string> EmployedRollupJobcats = ["1-LJD", "2-NLJD", "3-NLP", "4-NLO", "5-WUNK"];
    public static readonly IReadOnlyList<string> WrittenD1Exclusions = ["7-USKW", "8-UNWK"];
    public static readonly IReadOnlyList<string> SalaryJobcats = ["1-LJD", "2-NLJD", "3-NLP", "4-NLO"];
    public static readonly IReadOnlyList<string> GenderCountValues = ["M", "F", "N"];
    public static readonly IReadOnlyList<string> MinorityCountValues = ["NONMIN", "MINOR"];

    public static readonly IReadOnlyList<string> CommentedSkippedSchoolCodes =
        ["23101", "23909", "31504", "42603"];

    public const int SalarySuppressionMinimumN = 5;
    public const string SalaryVariable = "salftperm";

    public static string RecodeSex3(string sex3) =>
        AccessibleSchoolReports.Domain.Recodes.LegacyRecodes.NormalizeGender(sex3)!;

    public static string RecodeSource(string source) =>
        AccessibleSchoolReports.Domain.Recodes.LegacyRecodes.NormalizeJobSource(source)!;

    public static string RecodeJobreg(string jobreg) =>
        AccessibleSchoolReports.Domain.Recodes.LegacyRecodes.NormalizeJobRegion(jobreg)!;

    public static string RecodeLfjob(string lfjob) =>
        AccessibleSchoolReports.Domain.Recodes.LegacyRecodes.NormalizeLawFirmJobType(lfjob)!;

    public static string RecodeEmptype1(string emptype1) =>
        AccessibleSchoolReports.Domain.Recodes.LegacyRecodes.NormalizeEmploymentType(emptype1)!;

    public static string? MapFirmSizeForCounts(string firm1) =>
        AccessibleSchoolReports.Domain.Recodes.LegacyRecodes.MapFirmSizeForCounts(firm1);

    public static string? MapFirmSizeForSalaries(string firm1) =>
        AccessibleSchoolReports.Domain.Recodes.LegacyRecodes.MapFirmSizeForSalaries(firm1);

    public static string? MapSector(string empgen) =>
        AccessibleSchoolReports.Domain.Recodes.LegacyRecodes.MapSector(empgen);

    public static bool DeleteFromSectorCounts(string empgen) =>
        empgen == "EMPUNK";

    public static string RecodeEmployerTypeForCounts(string empgen) =>
        empgen == "EMPUNK" ? "ZEMPUN" : empgen;

    public static bool IncludeInGenderCounts(string sex3AfterRecode) =>
        GenderCountValues.Contains(sex3AfterRecode, StringComparer.Ordinal);

    public static bool IncludeInEmploymentStatusCounts(string jobcat1) =>
        jobcat1 != "UNKN";

    public static string? MapD1Newvar(string jobcat) =>
        AccessibleSchoolReports.Domain.Recodes.LegacyRecodes.MapD1Newvar(jobcat);

    public static bool WrittenD1ExclusionContains(string jobcat) =>
        WrittenD1Exclusions.Contains(jobcat, StringComparer.Ordinal);

    public static bool KeepSalaryRow(int n) =>
        n >= SalarySuppressionMinimumN;

    public static string CompressJobcatAndFtPt(string jobcat, string jobftpt) =>
        AccessibleSchoolReports.Domain.Recodes.LegacyRecodes.CompressJobcatAndFtPt(jobcat, jobftpt);

    public static bool Page2CharacterFilterIncludes(string analvar) =>
        string.CompareOrdinal(analvar, "D1") >= 0
        && string.CompareOrdinal(analvar, "E2") < 0;

    public static string SubtotalLabel(string analvar) => analvar switch
    {
        "JOBREG3" => "Total #",
        "DURATION" => "Total Reported",
        "LAW SCHOOL FUNDED" => "Total Reported",
        _ => "Subtotal",
    };

    public static decimal SumStoredPercents(params decimal[] percents) =>
        percents.Sum();
}
