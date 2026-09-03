namespace AccessibleSchoolReports.CharacterizationTests.Support;

/// <summary>
/// Observed values from legacy/baseline/test-school-report.pdf as recorded in
/// docs/capstone/report-map.md. These lock displayed legacy output. They are not
/// a reconstructed graduate-level file.
/// </summary>
internal static class BaselineSchoolReport
{
    public const string SchoolName = "Test University School of Law";
    public const string PdfTitleClassYear = "Class of 2024 Summary Report";
    public const string Sas2025TitleClassYear = "Class of 2025 Summary Report";
    public const int PageCount = 7;
    public const int TotalReported = 100;
    public const int StatesAndTerritoriesWithEmployedGrads = 14;
    public const string SalarySpanningHeader = "Full-time Long-term Salaries";

    public static readonly ObservedRow Women = new("B", "Women", 46, 46.0m, 40, 70000, 85500, 110000, 85802);
    public static readonly ObservedRow Men = new("B", "Men", 54, 54.0m, 30, 850000, 90500, 120000, 103401);

    public static readonly ObservedRow PeopleOfColor = new("C", "People of Color", 24, 34.3m, 8, 60000, 85000, 89000, 82604);
    public static readonly ObservedRow White = new("C", "White", 46, 65.7m, 34, 80000, 86000, 95000, 90559);

    public static readonly ObservedRow WomenOfColor = new("C1", "Women of Color", 10, 15.6m, 6, 65000, 88500, 90000, 84673);
    public static readonly ObservedRow MenOfColor = new("C1", "Men of Color", 4, 6.3m, null, null, null, null, null);
    public static readonly ObservedRow WhiteWomen = new("C1", "White Women", 30, 46.9m, 25, 75000, 80000, 101000, 95983);
    public static readonly ObservedRow WhiteMen = new("C1", "White Men", 20, 31.3m, 15, 85000, 88000, 105000, 107471);

    public static readonly ObservedRow Ljd = new("D", "Bar Admission Required/ Anticipated", 78, 78.0m, 69, 765000, 88000, 110000, 90397);
    public static readonly ObservedRow JdAdvantage = new("D", "JD Advantage", 12, 12.0m, 10, 72250, 93500, 104200, 100500);
    public static readonly ObservedRow OtherProfessional = new("D", "Other Professional", 1, 1.0m, null, null, null, null, null);
    public static readonly ObservedRow OtherPosition = new("D", "Other Position", 2, 2.0m, null, null, null, null, null);
    public static readonly ObservedRow NotEmployedSeeking = new("D", "Not Employed-Seeking", 3, 3.0m, null, null, null, null, null);
    public static readonly ObservedRow NotEmployedNotSeeking = new("D", "Not Employed-Not Seeking", 4, 4.0m, null, null, null, null, null);

    public static readonly ObservedRow Employed = new("D1", "Employed", 93, 93.0m, 83, 70000, 89000, 105000, 95236);
    public static readonly ObservedRow PrivateSector = new("D2", "Private Sector", 78, 83.9m, 58, 84000, 95500, 106000, 101425);
    public static readonly ObservedRow PublicSector = new("D2", "Public Sector", 15, 16.1m, 10, 66000, 73950, 92000, 78532);

    public static readonly ObservedRow LjdFullTime = new("D3", "Bar Admission Required/ Anticipated: Full-time", 79, 84.9m, 69, 74000, 88000, 104000, 88297);
    public static readonly ObservedRow JdAdvantageFullTime = new("D3", "JD Advantage: Full-time", 10, 10.8m, 12, 70250, 93500, 104200, 99325);
    public static readonly ObservedRow JdAdvantagePartTime = new("D3", "JD Advantage: Part-time", 2, 2.2m, null, null, null, null, null);

    public static readonly ObservedRow Business = new("E1", "Business", 14, 15.1m, 13, 95000, 108000, 110000, 120500);
    public static readonly ObservedRow Clerkships = new("E1", "Judicial Clerkships", 5, 5.4m, null, null, null, null, null);
    public static readonly ObservedRow PrivatePractice = new("E1", "Private Practice", 50, 53.8m, 45, 82000, 93000, 105000, 96901);
    public static readonly ObservedRow Government = new("E1", "Government", 16, 17.2m, 15, 65000, 84000, 93000, 82010);
    public static readonly ObservedRow PublicInterest = new("E1", "Public Interest", 8, 8.6m, 6, 64000, 73495, 92000, 76801);

    public static readonly ObservedRow NewEngland = new("JOBREG1", "New England", 69, 74.2m, 57, 84000, 93000, 105000, 92800);
    public static readonly ObservedRow MidAtlantic = new("JOBREG1", "Mid-Atlantic", 16, 17.2m, 11, 57950, 84000, 101000, 92727);
    public static readonly ObservedRow Mountain = new("JOBREG1", "Mountain", 5, 5.4m, null, null, null, null, null);

    public static readonly ObservedRow InState = new("JOBREG2", "In-State", 63, 67.8m, 51, 81000, 92000, 101000, 90470);
    public static readonly ObservedRow OutOfState = new("JOBREG2", "Out of State", 30, 32.2m, 20, 66000, 83000, 107000, 99226);

    public static readonly ObservedRow Oci = new("SOURCE", "Career office recruitment program (e.g., OCI)", 4, 4.4m, null, null, null, null, null);
    public static readonly ObservedRow BeforeGraduation = new("TIME", "Before graduation", 49, 61.3m, null, null, null, null, null);
    public static readonly ObservedRow AfterGraduation = new("TIME", "After graduation", 31, 38.8m, null, null, null, null, null);

    public static readonly IReadOnlyList<string> AbsentSectionsOnThisPdf =
    [
        "Education Jobs:",
        "Total Number of Jobs Reported as Funded by Law School:",
    ];

    public static readonly IReadOnlyList<string> AbsentRowLabelsOnThisPdf =
    [
        "Non-binary or Chose to Self-identify",
        "Enrolled in Graduate Studies",
        "Education",
        "Solo Practitioner",
        "Foreign",
    ];
}

internal sealed record ObservedRow(
    string Analvar,
    string Label,
    int Count,
    decimal Percent,
    int? SalaryN,
    int? Pct25,
    int? Median,
    int? Pct75,
    int? Mean)
{
    public bool HasDisplayedSalary => SalaryN is not null;
}
