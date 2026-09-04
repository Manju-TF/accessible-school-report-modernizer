namespace AccessibleSchoolReports.Domain.Recodes;

/// <summary>
/// Confirmed SAS preprocessing from <c>createschrptfiles2025.sas</c>
/// Characterized recodes from the legacy SAS builder (CF-PREP-*). Ambiguous rules are not guessed.
/// </summary>
public static class LegacyRecodes
{
    /// <summary>CF-PREP-00: columns dropped on SET of erss2025.erss2025.</summary>
    public static readonly IReadOnlyList<string> DroppedSourceColumns =
    [
        "office_size",
        "othersource",
        "Field35b",
        "field9b",
        "Field36",
        "jobdesc",
    ];

    /// <summary>CF-FMT-03: confirmed <c>$jobcat</c> mappings used by CF-PREP-01.</summary>
    public static readonly IReadOnlyDictionary<string, string> JobCategoryFormat =
        new Dictionary<string, string>(StringComparer.Ordinal)
        {
            ["LJD"] = "1-LJD",
            ["NLJD"] = "2-NLJD",
            ["NLP"] = "3-NLP",
            ["NLO"] = "4-NLO",
            ["WUNK"] = "5-WUNK",
            ["ADVD"] = "6-ADVD",
            ["UDEF"] = "7-UDEF",
            ["USKW"] = "8-USKW",
            ["UNWK"] = "9-UNWK",
            ["UNKN"] = "UNKN",
            ["FULL"] = "Full-time",
            ["PART"] = "Part-time",
        };

    /// <summary>CF-PREP-00: true when the source column is dropped before later steps.</summary>
    public static bool IsDroppedSourceColumn(string columnName) =>
        DroppedSourceColumns.Contains(columnName, StringComparer.Ordinal);

    /// <summary>
    /// CF-PREP-01 / CF-FMT-03: <c>jobcat = put(jobcat1, $jobcat.);</c>
    /// Unknown values stay raw (SAS PUT). Do not apply this format to <c>jobftpt</c>
    /// (CF-AMB-01: <c>$jobcat1</c> is undefined).
    /// </summary>
    public static string? NormalizeJobCategory(string? jobcat1)
    {
        if (jobcat1 is null)
        {
            return null;
        }

        return JobCategoryFormat.TryGetValue(jobcat1, out var formatted)
            ? formatted
            : jobcat1;
    }

    /// <summary>
    /// CF-PREP-02: confirmed lfjob assignments only.
    /// Sort/display reason is INFERRED and is not implemented.
    /// </summary>
    public static string? NormalizeLawFirmJobType(string? lfjob) => lfjob switch
    {
        null => null,
        "ADMIN" => "YADMIN",
        "OTHNL" => "ZOTHNL",
        "STATTY" => "ATTYST",
        _ => lfjob,
    };

    /// <summary>
    /// CF-PREP-03: <c>if jobreg = '0' then jobreg = 'X';</c>
    /// Exact character <c>0</c> only. Meaning of 0/X is AMBIGUOUS and not guessed.
    /// </summary>
    public static string? NormalizeJobRegion(string? jobreg) =>
        jobreg == "0" ? "X" : jobreg;

    /// <summary>CF-PREP-04: <c>OTHER</c>→<c>ZOTHER</c>; <c>OCI</c>→<c>AOCI</c>.</summary>
    public static string? NormalizeJobSource(string? source) => source switch
    {
        null => null,
        "OTHER" => "ZOTHER",
        "OCI" => "AOCI",
        _ => source,
    };

    /// <summary>
    /// CF-PREP-05: confirmed emptype1 assignments only.
    /// Clerkship/court meaning is INFERRED and is not implemented as extra logic.
    /// </summary>
    public static string? NormalizeEmploymentType(string? emptype1) => emptype1 switch
    {
        null => null,
        "JCLOGV" => "JCTLOG",
        "JCINGV" => "JCXIOG",
        "JCOTGV" or "JCUNGV" or "JC" => "JCUGOV",
        _ => emptype1,
    };

    /// <summary>
    /// CF-PREP-06: <c>W</c>→<c>F</c>; <c>X</c>→<c>N</c>; <c>ND</c>→ SAS blank <c>' '</c>.
    /// Commented <c>sex</c> recodes (CF-DEAD-02) are not applied.
    /// </summary>
    public static string? NormalizeGender(string? sex3) => sex3 switch
    {
        null => null,
        "W" => "F",
        "X" => "N",
        "ND" => " ",
        _ => sex3,
    };

    /// <summary>CF-C-08: public employer types.</summary>
    public static readonly IReadOnlyList<string> PublicEmpgen = ["ACAD", "GOVT", "CLERK", "PUBINT"];

    /// <summary>CF-C-08: private employer types.</summary>
    public static readonly IReadOnlyList<string> PrivateEmpgen = ["BUS", "FIRM"];

    /// <summary>CF-C-06: listed employed rollup after <c>$jobcat</c>.</summary>
    public static readonly IReadOnlyList<string> EmployedRollupJobcats =
        ["1-LJD", "2-NLJD", "3-NLP", "4-NLO", "5-WUNK"];

    /// <summary>CF-C-06 / CF-AMB-02: written NOT IN list. Do not rewrite to 8-USKW/9-UNWK.</summary>
    public static readonly IReadOnlyList<string> WrittenD1Exclusions = ["7-USKW", "8-UNWK"];

    /// <summary>CF-S-04 and later salary JOBCAT filters.</summary>
    public static readonly IReadOnlyList<string> SalaryJobcats = ["1-LJD", "2-NLJD", "3-NLP", "4-NLO"];

    public const int SalarySuppressionMinimumN = 5;

    /// <summary>CF-C-16: firm-size count map. Unlisted firm1 is not guessed.</summary>
    public static string? MapFirmSizeForCounts(string? firm1) => firm1 switch
    {
        "S" => "SOLO",
        "1" => "LF1",
        "2" => "LF2",
        "3" => "LF3",
        "4" => "LF4",
        "5" => "LF5",
        "6" => "LF6",
        "7" => "LF7",
        "8" => "LF8",
        _ => null,
    };

    /// <summary>CF-S-15: salary map is 1–8 only. <c>S</c> is not mapped to SOLO.</summary>
    public static string? MapFirmSizeForSalaries(string? firm1) => firm1 switch
    {
        "1" => "LF1",
        "2" => "LF2",
        "3" => "LF3",
        "4" => "LF4",
        "5" => "LF5",
        "6" => "LF6",
        "7" => "LF7",
        "8" => "LF8",
        _ => null,
    };

    /// <summary>CF-C-08 sector grouping.</summary>
    public static string? MapSector(string? empgen)
    {
        if (empgen is null)
        {
            return null;
        }

        if (PublicEmpgen.Contains(empgen, StringComparer.Ordinal))
        {
            return "PUBLIC";
        }

        if (PrivateEmpgen.Contains(empgen, StringComparer.Ordinal))
        {
            return "PRIVATE";
        }

        return null;
    }

    /// <summary>CF-C-06 newvar after written NOT IN. Null means SAS left newvar missing.</summary>
    public static string? MapD1Newvar(string? jobcat)
    {
        if (jobcat == "6-ADVD")
        {
            return "6-ADVD";
        }

        if (jobcat is not null && EmployedRollupJobcats.Contains(jobcat, StringComparer.Ordinal))
        {
            return "EMPL";
        }

        return null;
    }

    /// <summary>CF-P2-04: <c>AFTGRD</c>→<c>ZAFTGRD</c>. Do not invent ZAFTGR.</summary>
    public static string? RecodeOfferTiming(string? time1) =>
        time1 == "AFTGRD" ? "ZAFTGRD" : time1;

    /// <summary>CF-C-07 / CF-S-19: SAS <c>compress(jobcat||jobftpt)</c> with no invented $jobcat1 labels.</summary>
    public static string CompressJobcatAndFtPt(string? jobcat, string? jobftpt) =>
        string.Concat(jobcat, jobftpt).Replace(" ", string.Empty, StringComparison.Ordinal);
}
