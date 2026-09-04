using System.Globalization;

namespace AccessibleSchoolReports.Application.Reporting;

/// <summary>
/// Report chrome and labels. Presentation only.
/// Organization names are a fictional test client, not NALP or ABA.
/// Does not change calculator output.
/// </summary>
public static class SchoolReportPresentation
{
    public const string Language = "en-US";
    public const string Publisher = "Meridian Test Client";
    public const string ComparisonPublisher = "Sample Board";
    public const string ClassYearTitle = "Class of 2025 Summary Report";
    public const string PreparedLine = "Table prepared by Meridian Test Client, July 2026";
    public const string FooterUrl = "www.example.com/report-info";
    public const string FooterUrlHref = "https://www.example.com/report-info";
    public const string FooterLinkName = "Test client report information";
    public const string NotDisplayed = ".";
    public const string NotDisplayedAccessibleName = "Not displayed";
    public const string SalaryGroupCaption = "Full-time Long-term Salaries";

    public const string Disclaimer =
        "Meridian Test Client Summary Report data may vary slightly from the school-specific data published by the Sample Board because of definitional differences between the two organizations and because Meridian Test Client's quality control process can result in changes which may not be reflected in Sample Board data. For more on this, see www.example.com/report-info.";

    public const string NotePage1 =
        "Categories with no graduates reported are not shown. At least five salaries are required for each salary analysis. The non-binary or chose to self-identify category also includes graduates who selected multiple gender identities. Salaries are reported only for full-time, long-term positions. Salaries for graduates in law firm solo practice have been excluded from the analysis.";

    public const string NotePage2 =
        "Categories with no graduates reported are not shown. At least five salaries are required for each salary analysis. The non-binary or chose to self-identify category also includes graduates who selected multiple gender identities. Private sector includes jobs in law firms and business. All other jobs are considered public sector. Employment by sector does not include graduates for whom employer type was not reported. Salaries are reported only for full-time, long-term positions. Salaries for graduates in law firm solo practice have been excluded from the analysis.";

    public const string NotePages3To5 =
        "Categories with no graduates reported are not shown. At least five salaries are required for each salary analysis. Salaries are reported only for full-time, long-term positions. Salaries for graduates in law firm solo practice have been excluded from the analysis.";

    public const string NotePage6 =
        "Figures are based on jobs for which the item was reported, and thus may not add to the total number of jobs. Timing of job offer figures exclude any graduates starting their own practice.";

    public const string NotePage7 =
        "Figures for job duration are based on jobs for which the item was reported, and thus may not add to the total number of jobs. The count of jobs funded by the law school is a total, regardless of duration.";

    public static readonly IReadOnlyList<string> Page1Analvars = ["B", "C", "C1", "D"];
    public static readonly IReadOnlyList<string> Page3Analvars = ["E2", "E3", "E4", "E5", "E55"];
    public static readonly IReadOnlyList<string> Page4Analvars = ["E6", "FIRM", "FIRM2"];
    public static readonly IReadOnlyList<string> Page5Analvars = ["JOBREG1", "JOBREG2", "JOBREG3"];
    public static readonly IReadOnlyList<string> Page6Analvars = ["SOURCE", "TIME", "ZSTATUS"];
    public static readonly IReadOnlyList<string> Page7Analvars = ["DURATION", "LAW SCHOOL FUNDED"];

    public static bool IsPage2Analvar(string analvar) =>
        string.CompareOrdinal(analvar, "D1") >= 0 && string.CompareOrdinal(analvar, "E2") < 0;

    public static string PageNote(int pageNumber) => pageNumber switch
    {
        1 => NotePage1,
        2 => NotePage2,
        3 or 4 or 5 => NotePages3To5,
        6 => NotePage6,
        7 => NotePage7,
        _ => string.Empty,
    };

    public static string PageHeading(int pageNumber) =>
        pageNumber == 1 ? ClassYearTitle : $"{ClassYearTitle} - Page {pageNumber}";

    public static string SectionHeading(string analvar) =>
        SectionTitles.TryGetValue(analvar, out var title) ? title : analvar;

    public static string SubtotalLabel(string analvar) => analvar switch
    {
        "JOBREG3" => "Total #",
        "DURATION" or "LAW SCHOOL FUNDED" => "Total Reported",
        _ => "Subtotal",
    };

    public static string RowLabel(string analvar, string newvar)
    {
        if (analvar == "JOBREG3")
        {
            return "# States and Territories with Employed Grads";
        }

        if (analvar == "LAW SCHOOL FUNDED")
        {
            return "Funded by law school";
        }

        if (analvar == "DURATION" && string.IsNullOrEmpty(newvar))
        {
            return "Total Reported";
        }

        if (RowTitles.TryGetValue(newvar, out var title))
        {
            return title;
        }

        if (newvar.EndsWith("FULL", StringComparison.Ordinal) &&
            RowTitles.TryGetValue(newvar[..^4], out var fullBase))
        {
            return $"{fullBase}: Full-time";
        }

        if (newvar.EndsWith("PART", StringComparison.Ordinal) &&
            RowTitles.TryGetValue(newvar[..^4], out var partBase))
        {
            return $"{partBase}: Part-time";
        }

        return newvar;
    }

    public static string FormatCount(int? value) =>
        value is null ? NotDisplayed : value.Value.ToString("N0", CultureInfo.GetCultureInfo("en-US"));

    public static string FormatPercent(decimal? value) =>
        value is null
            ? NotDisplayed
            : decimal.Round(value.Value, 1, MidpointRounding.AwayFromZero).ToString("N1", CultureInfo.GetCultureInfo("en-US"));

    public static string FormatMoney(decimal? value) =>
        value is null
            ? NotDisplayed
            : decimal.Round(value.Value, 0, MidpointRounding.AwayFromZero).ToString("N0", CultureInfo.GetCultureInfo("en-US"));

    private static readonly Dictionary<string, string> SectionTitles = new(StringComparer.Ordinal)
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

    private static readonly Dictionary<string, string> RowTitles = new(StringComparer.Ordinal)
    {
        ["F"] = "Women",
        ["M"] = "Men",
        ["N"] = "Non-binary or Chose to Self-identify",
        ["NONMIN"] = "White",
        ["MINOR"] = "People of Color",
        ["NONMINF"] = "White Women",
        ["NONMINM"] = "White Men",
        ["NONMINN"] = "White Non-binary or Chose to Self-identify",
        ["MINORF"] = "Women of Color",
        ["MINORM"] = "Men of Color",
        ["MINORN"] = "Non-binary or Chose to Self-identify People of Color",
        ["MINOR F"] = "Women of Color",
        ["MINOR M"] = "Men of Color",
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
        ["ZAFTGRD"] = "After graduation",
        ["ZAFTGR"] = "After graduation",
        ["AOCI"] = "Career office recruitment program (e.g., OCI)",
        ["JOBFRC"] = "Job fair or career conference",
        ["JOBPST"] = "Career office job posting",
        ["ONLINE"] = "Non-career office job posting",
        ["OSCAR"] = "Clerkship application process or OSCAR",
        ["PRNSMJ"] = "Returned to or continued with pre-law school employer",
        ["RFFRND"] = "Referral",
        ["SLFINI"] = "Self-initiated contact/networking",
        ["SELFPR"] = "Started own business/practice",
        ["TEMPAG"] = "Temp agency or legal search consultant",
        ["ZOTHER"] = "Other",
        ["ATTY"] = "Associate/Entry-level Attorney",
        ["LCLERK"] = "Law Clerk",
        ["NOTSET"] = "Seeking a different job",
        ["SET"] = "Not seeking a different job",
        ["JCFDGV"] = "Federal",
        ["JCSTGV"] = "State",
        ["JCTLOG"] = "Local",
        ["JCTRGV"] = "Tribal",
        ["JCUGOV"] = "Unknown",
        ["JCINGV"] = "International",
        ["JCXIOG"] = "International",
    };
}
