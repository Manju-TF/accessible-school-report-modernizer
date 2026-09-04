namespace AccessibleSchoolReports.IntegrationTests.Parity;

/// <summary>
/// Characterized business values from <c>legacy/baseline/test-school-report.pdf</c>
/// as recorded in <c>docs/capstone/report-map.md</c>. Not a reconstructed
/// graduate file. Text-layer salary glitches are kept as observed.
/// </summary>
public static class LegacyExpectedSchoolReport
{
    public const string SchoolName = "Test University School of Law";
    public const string PdfClassYear = "Class of 2024";
    public const int TotalReported = 100;

    public static IReadOnlyList<LegacyExpectedMetric> All()
    {
        var metrics = new List<LegacyExpectedMetric>();

        metrics.Add(new("identity.school-code", "SS-PREP-03", "SCHOOL", null, ParityField.Count, null, "School CODE", "Baseline PDF school CODE is unknown and is not in the 2025 %SCHRPTS list. Sample identity cannot be proven.", Unresolved: true));
        metrics.Add(new("total.count", "CF-C-01", "A", "A", ParityField.Count, 100, "Total Reported"));
        metrics.Add(new("total.subtotal-count", "SS-CNT-02", "A", "A", ParityField.SubtotalCount, 100, "Total Reported"));

        Row(metrics, "gender-women", "CF-C-02", "B", "F", "Women", 46, 46.0m, 40, 70000, 85500, 110000, 85802);
        Row(metrics, "gender-men", "CF-C-02", "B", "M", "Men", 54, 54.0m, 30, 850000, 90500, 120000, 103401, "PDF text layer 25th is 850000 (comma missing).");
        Sub(metrics, "gender", "SS-SUB-01", "B", 100, 100.0m, "Gender Reported");
        Absent(metrics, "gender-nonbinary", "SS-FIL-08", "B", "N", "Non-binary");

        Row(metrics, "race-poc", "CF-C-03", "C", "MINOR", "People of Color", 24, 34.3m, 8, 60000, 85000, 89000, 82604);
        Row(metrics, "race-white", "CF-C-03", "C", "NONMIN", "White", 46, 65.7m, 34, 80000, 86000, 95000, 90559);
        Sub(metrics, "race", "SS-SUB-01", "C", 70, 100.0m, "Race Reported");

        Row(metrics, "cross-woc", "CF-C-04", "C1", "MINORF", "Women of Color", 10, 15.6m, 6, 65000, 88500, 90000, 84673);
        Row(metrics, "cross-moc", "CF-C-04", "C1", "MINORM", "Men of Color", 4, 6.3m, null, null, null, null, null);
        Row(metrics, "cross-ww", "CF-C-04", "C1", "NONMINF", "White Women", 30, 46.9m, 25, 75000, 80000, 101000, 95983);
        Row(metrics, "cross-wm", "CF-C-04", "C1", "NONMINM", "White Men", 20, 31.3m, 15, 85000, 88000, 105000, 107471);
        Sub(metrics, "cross", "SS-SUB-01", "C1", 64, 100.0m, "Gender & Race");

        Row(metrics, "emp-ljd", "CF-C-05", "D", "1-LJD", "Bar Admission Required/ Anticipated", 78, 78.0m, 69, 765000, 88000, 110000, 90397, "PDF text layer 25th is 765000 (comma missing).");
        Row(metrics, "emp-nljd", "CF-C-05", "D", "2-NLJD", "JD Advantage", 12, 12.0m, 10, 72250, 93500, 104200, 100500);
        Row(metrics, "emp-nlp", "CF-C-05", "D", "3-NLP", "Other Professional", 1, 1.0m, null, null, null, null, null);
        Row(metrics, "emp-nlo", "CF-C-05", "D", "4-NLO", "Other Position", 2, 2.0m, null, null, null, null, null);
        Row(metrics, "emp-uskw", "CF-C-05", "D", "8-USKW", "Not Employed-Seeking", 3, 3.0m, null, null, null, null, null);
        Row(metrics, "emp-unwk", "CF-C-05", "D", "9-UNWK", "Not Employed-Not Seeking", 4, 4.0m, null, null, null, null, null);
        Sub(metrics, "emp", "SS-SUB-01", "D", 100, 100.0m, "Employment Status Known");
        Absent(metrics, "emp-advd", "SS-FIL-08", "D", "6-ADVD", "Enrolled in Graduate Studies");

        Row(metrics, "d1-empl", "CF-C-06", "D1", "EMPL", "Employed", 93, 93.0m, 83, 70000, 89000, 105000, 95236);
        Sub(metrics, "d1", "SS-SUB-01", "D1", 93, 93.0m, "Total Employed or Enrolled");
        Absent(metrics, "d1-advd", "SS-FIL-08", "D1", "6-ADVD", "Enrolled in Graduate Studies");

        Row(metrics, "d2-private", "CF-C-08", "D2", "PRIVATE", "Private Sector", 78, 83.9m, 58, 84000, 95500, 106000, 101425);
        Row(metrics, "d2-public", "CF-C-08", "D2", "PUBLIC", "Public Sector", 15, 16.1m, 10, 66000, 73950, 92000, 78532);
        Sub(metrics, "d2", "SS-SUB-01", "D2", 93, 100.0m, "Employment by Sector");

        Row(metrics, "d3-ljd-ft", "CF-C-07", "D3", "1-LJDFULL", "LJD Full-time", 79, 84.9m, 69, 74000, 88000, 104000, 88297, "Page-2 LJD FT 79 vs page-1 LJD 78.");
        Row(metrics, "d3-nljd-ft", "CF-C-07", "D3", "2-NLJDFULL", "JD Advantage Full-time", 10, 10.8m, 12, 70250, 93500, 104200, 99325, "Salary n 12 > count 10.");
        Row(metrics, "d3-nljd-pt", "CF-C-07", "D3", "2-NLJDPART", "JD Advantage Part-time", 2, 2.2m, null, null, null, null, null);
        Row(metrics, "d3-nlp-ft", "CF-C-07", "D3", "3-NLPFULL", "Other Professional Full-time", 1, 1.1m, null, null, null, null, null);
        Row(metrics, "d3-nlo-ft", "CF-C-07", "D3", "4-NLOFULL", "Other Position Full-time", 1, 1.1m, null, null, null, null, null);
        Row(metrics, "d3-nlo-pt", "CF-C-07", "D3", "4-NLOPART", "Other Position Part-time", 1, 1.1m, null, null, null, null, null);
        Sub(metrics, "d3", "SS-CALC-01", "D3", 93, 100.0m, "FT/PT Job Status", "Displayed details sum to 94; printed subtotal is 93.");

        Row(metrics, "e1-bus", "CF-C-09", "E1", "BUS", "Business", 14, 15.1m, 13, 95000, 108000, 110000, 120500);
        Row(metrics, "e1-clerk", "CF-C-09", "E1", "CLERK", "Judicial Clerkships", 5, 5.4m, null, null, null, null, null);
        Row(metrics, "e1-firm", "CF-C-09", "E1", "FIRM", "Private Practice", 50, 53.8m, 45, 82000, 93000, 105000, 96901);
        Row(metrics, "e1-govt", "CF-C-09", "E1", "GOVT", "Government", 16, 17.2m, 15, 65000, 84000, 93000, 82010);
        Row(metrics, "e1-pi", "CF-C-09", "E1", "PUBINT", "Public Interest", 8, 8.6m, 6, 64000, 73495, 92000, 76801);
        Sub(metrics, "e1", "SS-SUB-01", "E1", 93, 100.0m, "Employment Categories");
        Absent(metrics, "e1-acad", "SS-FIL-08", "E1", "ACAD", "Education");

        Absent(metrics, "e2", "SS-FIL-08", "E2", null, "Education Jobs section", unresolved: false);

        Row(metrics, "e3-ljd", "CF-C-11", "E3", "1-LJD", "Business LJD", 2, 14.3m, null, null, null, null, null);
        Row(metrics, "e3-nljd", "CF-C-11", "E3", "2-NLJD", "Business JD Advantage", 10, 71.3m, 8, 95000, 108000, 109200, 109185);
        Row(metrics, "e3-nlp", "CF-C-11", "E3", "3-NLP", "Business Other Professional", 1, 7.1m, null, null, null, null, null);
        Row(metrics, "e3-nlo", "CF-C-11", "E3", "4-NLO", "Business Other Position", 1, 7.1m, null, null, null, null, null);
        Sub(metrics, "e3", "SS-SUB-01", "E3", 14, 100.0m, "Business Jobs");

        Row(metrics, "e4-ljd", "CF-C-12", "E4", "1-LJD", "Firm LJD", 46, 92.0m, 40, 82000, 95000, 102000, 96608);
        Row(metrics, "e4-nljd", "CF-C-12", "E4", "2-NLJD", "Firm JD Advantage", 4, 8.0m, null, null, null, null, null);
        Sub(metrics, "e4", "SS-SUB-01", "E4", 50, 100.0m, "Private Practice Jobs");

        Row(metrics, "e5-ljd", "CF-C-13", "E5", "1-LJD", "Government LJD", 13, 81.3m, 11, 78000, 88000, 90000, 83957);
        Row(metrics, "e5-nljd", "CF-C-13", "E5", "2-NLJD", "Government JD Advantage", 3, 18.8m, null, null, null, null, null);
        Sub(metrics, "e5", "SS-SUB-01", "E5", 16, 100.0m, "Government Jobs");

        Row(metrics, "e55-state", "CF-C-14", "E55", "JCSTGV", "Clerkships State", 4, 80.0m, null, null, null, null, null, "PDF label State maps to $newvar JCSTGV (SS-FMT-04). Sample export has no emptype1 column.");
        Row(metrics, "e55-local", "CF-C-14", "E55", "JCTLOG", "Clerkships Local", 1, 20.0m, null, null, null, null, null, "PDF label Local maps to $newvar JCTLOG (SS-FMT-04). Sample export has no emptype1 column.");
        Absent(metrics, "e55-federal", "SS-FIL-08", "E55", "JCFDGV", "Clerkships Federal");
        Absent(metrics, "e55-tribal", "SS-FIL-08", "E55", "JCTRGV", "Clerkships Tribal");
        Absent(metrics, "e55-unknown", "SS-FIL-08", "E55", "JCUGOV", "Clerkships Unknown");
        Sub(metrics, "e55", "SS-SUB-01", "E55", 5, 100.0m, "Judicial Clerkships");

        Row(metrics, "e6-ljd", "CF-C-15", "E6", "1-LJD", "Public Interest LJD", 6, 75.0m, 5, 66000, 71405, 88000, 73821);
        Row(metrics, "e6-nljd", "CF-C-15", "E6", "2-NLJD", "Public Interest JD Advantage", 1, 12.5m, null, null, null, null, null);
        Row(metrics, "e6-nlo", "CF-C-15", "E6", "4-NLO", "Public Interest Other Position", 1, 12.5m, null, null, null, null, null);
        Sub(metrics, "e6", "SS-SUB-01", "E6", 8, 100.0m, "Public Interest Jobs");

        Row(metrics, "firm-lf1", "CF-C-16", "FIRM", "LF1", "1-10", 30, 60.0m, 18, 78000, 84000, 90000, 83710);
        Row(metrics, "firm-lf2", "CF-C-16", "FIRM", "LF2", "11-25", 10, 20.0m, 10, 81000, 87000, 101000, 89300);
        Row(metrics, "firm-lf3", "CF-C-16", "FIRM", "LF3", "26-50", 3, 6.0m, null, null, null, null, null);
        Row(metrics, "firm-lf4", "CF-C-16", "FIRM", "LF4", "51-100", 4, 8.0m, null, null, null, null, null);
        Row(metrics, "firm-lf5", "CF-C-16", "FIRM", "LF5", "101-250", 1, 2.0m, null, null, null, null, null);
        Row(metrics, "firm-lf6", "CF-C-16", "FIRM", "LF6", "251-500", 1, 2.0m, null, null, null, null, null);
        Row(metrics, "firm-lf7", "CF-C-16", "FIRM", "LF7", "501+", 1, 2.0m, null, null, null, null, null);
        Sub(metrics, "firm", "SS-SUB-01", "FIRM", 50, 100.0m, "Size of Law Firm");
        Absent(metrics, "firm-solo", "SS-FIL-08", "FIRM", "SOLO", "Solo Practitioner");

        Row(metrics, "firm2-atty", "CF-C-17", "FIRM2", "ATTY", "Associate/Entry-level Attorney", 42, 84.0m, 37, 88000, 92000, 101000, 98074);
        Row(metrics, "firm2-lclerk", "CF-C-17", "FIRM2", "LCLERK", "Law Clerk", 8, 16.0m, 5, 76000, 80500, 92000, 81803);
        Sub(metrics, "firm2", "SS-SUB-01", "FIRM2", 50, 100.0m, "Type of Law Firm Job");

        Row(metrics, "reg-1", "CF-C-18", "JOBREG1", "1", "New England", 69, 74.2m, 57, 84000, 93000, 105000, 92800);
        Row(metrics, "reg-2", "CF-C-18", "JOBREG1", "2", "Mid-Atlantic", 16, 17.2m, 11, 57950, 84000, 101000, 92727);
        Row(metrics, "reg-5", "CF-C-18", "JOBREG1", "5", "South Atlantic", 2, 2.2m, null, null, null, null, null);
        Row(metrics, "reg-6", "CF-C-18", "JOBREG1", "6", "E South Central", 1, 1.1m, null, null, null, null, null);
        Row(metrics, "reg-8", "CF-C-18", "JOBREG1", "8", "Mountain", 5, 5.4m, null, null, null, null, null);
        Sub(metrics, "reg", "SS-SUB-01", "JOBREG1", 93, 100.0m, "Jobs Taken by Region");

        Row(metrics, "loc-in", "CF-C-19", "JOBREG2", "INSTATE", "In-State", 63, 67.8m, 51, 81000, 92000, 101000, 90470);
        Row(metrics, "loc-out", "CF-C-19", "JOBREG2", "OUTOFSTATE", "Out of State", 30, 32.2m, 20, 66000, 83000, 107000, 99226);
        Sub(metrics, "loc", "SS-SUB-01", "JOBREG2", 93, 100.0m, "Location of Jobs");
        Absent(metrics, "loc-foreign", "SS-FIL-08", "JOBREG2", "FOREIGN", "Foreign");

        metrics.Add(new("states", "CF-C-20", "JOBREG3", "JOBREG3", ParityField.Count, 14, "# States and Territories"));
        metrics.Add(new("states.percent", "CF-C-20", "JOBREG3", "JOBREG3", ParityField.Percent, null, "# States percent is missing on the PDF"));
        Sub(metrics, "states", "SS-SUB-02", "JOBREG3", 14, null, "# States");

        Row(metrics, "src-aoci", "CF-P2-03", "SOURCE", "AOCI", "OCI", 4, 4.4m, null, null, null, null, null);
        Row(metrics, "src-jobfrc", "CF-P2-03", "SOURCE", "JOBFRC", "Job fair", 1, 1.1m, null, null, null, null, null);
        Row(metrics, "src-jobpst", "CF-P2-03", "SOURCE", "JOBPST", "Career office job posting", 10, 11.1m, null, null, null, null, null);
        Row(metrics, "src-online", "CF-P2-03", "SOURCE", "ONLINE", "Non-career office job posting", 10, 11.1m, null, null, null, null, null);
        Row(metrics, "src-oscar", "CF-P2-03", "SOURCE", "OSCAR", "OSCAR", 2, 2.2m, null, null, null, null, null);
        Row(metrics, "src-prnsmj", "CF-P2-03", "SOURCE", "PRNSMJ", "Pre-law employer", 2, 2.2m, null, null, null, null, null);
        Row(metrics, "src-rffrnd", "CF-P2-03", "SOURCE", "RFFRND", "Referral", 10, 11.1m, null, null, null, null, null);
        Row(metrics, "src-slfini", "CF-P2-03", "SOURCE", "SLFINI", "Self-initiated", 12, 13.3m, null, null, null, null, null);
        Row(metrics, "src-zother", "CF-P2-03", "SOURCE", "ZOTHER", "Other", 39, 43.3m, null, null, null, null, null);
        Sub(metrics, "source", "SS-SUB-01", "SOURCE", 90, 100.0m, "Source of Job");

        Row(metrics, "time-bgrad", "CF-P2-04", "TIME", "BGRAD", "Before graduation", 49, 61.3m, null, null, null, null, null);
        Row(metrics, "time-zaftgrd", "CF-P2-04", "TIME", "ZAFTGRD", "After graduation", 31, 38.8m, null, null, null, null, null, "Builder stores ZAFTGRD; report $newvar lists ZAFTGR.");
        Sub(metrics, "time", "SS-SUB-01", "TIME", 80, 100.1m, "Timing of Job Offer", "Stored percents 61.3+38.8=100.1.");

        Row(metrics, "status-notset", "CF-P2-05", "ZSTATUS", "NOTSET", "Seeking a different job", 4, 4.4m, null, null, null, null, null);
        Row(metrics, "status-set", "CF-P2-05", "ZSTATUS", "SET", "Not seeking a different job", 86, 95.6m, null, null, null, null, null);
        Sub(metrics, "status", "SS-SUB-01", "ZSTATUS", 90, 100.0m, "Search Status");

        Duration(metrics, "dur-total", "", 85, 2);
        Duration(metrics, "dur-bus", "BUS", 10, 1);
        Duration(metrics, "dur-clerk", "CLERK", 5, null);
        Duration(metrics, "dur-firm", "FIRM", 50, null);
        Duration(metrics, "dur-govt", "GOVT", 12, 1);
        Duration(metrics, "dur-pi", "PUBINT", 8, null);
        Absent(metrics, "funded", "CF-P2-02", "LAW SCHOOL FUNDED", "YES", "Law-school-funded jobs");

        return metrics;
    }

    private static void Row(
        List<LegacyExpectedMetric> metrics,
        string id,
        string ruleId,
        string analvar,
        string newvar,
        string label,
        int count,
        decimal? percent,
        int? salaryN,
        int? pct25,
        int? median,
        int? pct75,
        int? mean,
        string? note = null)
    {
        metrics.Add(new($"{id}.count", ruleId, analvar, newvar, ParityField.Count, count, label, note));
        metrics.Add(new($"{id}.percent", ruleId, analvar, newvar, ParityField.Percent, percent, label, note));
        metrics.Add(new($"{id}.n", "CF-S-00", analvar, newvar, ParityField.SalaryN, salaryN, label, note));
        metrics.Add(new($"{id}.p25", "SS-SAL-02", analvar, newvar, ParityField.Pct25, pct25, label, note));
        metrics.Add(new($"{id}.median", "SS-SAL-03", analvar, newvar, ParityField.Median, median, label, note));
        metrics.Add(new($"{id}.p75", "SS-SAL-04", analvar, newvar, ParityField.Pct75, pct75, label, note));
        metrics.Add(new($"{id}.mean", "SS-SAL-05", analvar, newvar, ParityField.Mean, mean, label, note));
    }

    private static void Sub(
        List<LegacyExpectedMetric> metrics,
        string id,
        string ruleId,
        string analvar,
        int? count,
        decimal? percent,
        string label,
        string? note = null)
    {
        metrics.Add(new($"{id}.subtotal-count", ruleId, analvar, null, ParityField.SubtotalCount, count, label, note));
        metrics.Add(new($"{id}.subtotal-percent", ruleId, analvar, null, ParityField.SubtotalPercent, percent, label, note));
    }

    private static void Absent(
        List<LegacyExpectedMetric> metrics,
        string id,
        string ruleId,
        string analvar,
        string? newvar,
        string label,
        bool unresolved = false)
    {
        metrics.Add(new($"{id}.absent", ruleId, analvar, newvar, ParityField.Count, null, label, "Absent on the baseline PDF.", unresolved));
    }

    private static void Duration(List<LegacyExpectedMetric> metrics, string id, string empgen, int perm, int? temp)
    {
        metrics.Add(new($"{id}.perm", "CF-P2-01", "DURATION", empgen, ParityField.Count, perm, $"{empgen} PERM", "Compared to DurationCounts[PERM]. Long/short labels are not a characterized codebook."));
        metrics.Add(new($"{id}.temp", "CF-P2-01", "DURATION", empgen, ParityField.Count, temp, $"{empgen} TEMP", "Compared to DurationCounts[TEMP]. Missing PDF '.' is expected null."));
    }
}
