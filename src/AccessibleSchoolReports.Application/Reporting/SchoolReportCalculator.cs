using AccessibleSchoolReports.Domain.Entities;
using AccessibleSchoolReports.Domain.Recodes;
using AccessibleSchoolReports.Domain.Reporting;

namespace AccessibleSchoolReports.Application.Reporting;

public sealed class SchoolReportCalculator : ISchoolReportCalculator
{
    public SchoolReport Calculate(string schoolCode, IReadOnlyList<GraduateRecord> graduates)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(schoolCode);
        ArgumentNullException.ThrowIfNull(graduates);

        var prepared = graduates.Select(Prepare).ToArray();
        var rows = new RowMap();

        AddTotal(rows, prepared); // CF-C-01
        AddGender(rows, prepared);
        AddMinority(rows, prepared);
        AddMinorityGender(rows, prepared);
        AddEmploymentStatus(rows, prepared);
        AddEmployedRollup(rows, prepared);
        AddFullTimePartTime(rows, prepared);
        AddSector(rows, prepared);
        AddEmployerType(rows, prepared);
        AddJobsByEmployerAndJobcat(rows, prepared, "ACAD", "E2"); // CF-C-10 / CF-S-10
        AddJobsByEmployerAndJobcat(rows, prepared, "BUS", "E3"); // CF-C-11 / CF-S-09
        AddJobsByEmployerAndJobcat(rows, prepared, "FIRM", "E4"); // CF-C-12 / CF-S-11
        AddJobsByEmployerAndJobcat(rows, prepared, "GOVT", "E5"); // CF-C-13 / CF-S-12
        AddClerkships(rows, prepared);
        AddJobsByEmployerAndJobcat(rows, prepared, "PUBINT", "E6"); // CF-C-15 / CF-S-14
        AddFirmSize(rows, prepared);
        AddFirmJobType(rows, prepared);
        AddRegion(rows, prepared);
        AddLocation(rows, prepared);
        AddStateCount(rows, prepared);
        AddEmployedSalaries(rows, prepared); // CF-S-05
        AddD3Salaries(rows, prepared); // CF-S-19
        AddSource(rows, prepared);
        AddTiming(rows, prepared);
        AddSearchStatus(rows, prepared);
        AddDuration(rows, prepared);
        AddFunded(rows, prepared);

        var ordered = rows.Values
            .OrderBy(row => row.Analvar, StringComparer.Ordinal)
            .ThenBy(row => row.Newvar, StringComparer.Ordinal)
            .ToArray();

        var sections = ordered
            .GroupBy(row => row.Analvar, StringComparer.Ordinal)
            .Select(group => new SchoolReportSection
            {
                Analvar = group.Key,
                Details = group.ToArray(),
                SubtotalCount = group.Sum(row => row.Count ?? 0),
                SubtotalPercent = group.Sum(row => row.Percent ?? 0m),
            })
            .ToArray();

        return new SchoolReport
        {
            SchoolCode = schoolCode,
            Rows = ordered,
            Sections = sections,
        };
    }

    private static PreparedGraduate Prepare(GraduateRecord graduate)
    {
        var sex3 = LegacyRecodes.NormalizeGender(graduate.Sex3);
        var lfjob = LegacyRecodes.NormalizeLawFirmJobType(graduate.Lfjob);
        var jobreg = LegacyRecodes.NormalizeJobRegion(graduate.Jobreg);
        var source = LegacyRecodes.NormalizeJobSource(graduate.Source);
        var emptype1 = LegacyRecodes.NormalizeEmploymentType(graduate.Emptype1);
        var jobcat1 = graduate.Jobcat1;
        return new PreparedGraduate(
            sex3,
            graduate.Minstat,
            jobcat1,
            LegacyRecodes.NormalizeJobCategory(jobcat1),
            graduate.JobFtPt,
            graduate.Empgen,
            graduate.Firm1,
            lfjob,
            jobreg,
            graduate.LocationFlag,
            graduate.Jobst,
            source,
            graduate.Time1,
            graduate.Status,
            graduate.Duration,
            graduate.SchoolFund,
            graduate.SalFtPerm,
            emptype1);
    }

    private static void AddTotal(RowMap rows, IReadOnlyList<PreparedGraduate> prepared)
    {
        // CF-C-01
        if (prepared.Count == 0)
        {
            return;
        }

        MergeCount(rows, "A", "A", prepared.Count, 100m);
    }

    private static void AddGender(RowMap rows, IReadOnlyList<PreparedGraduate> prepared)
    {
        // CF-C-02 / CF-S-01
        var filtered = prepared.Where(row => In(row.Sex3, "M", "F", "N")).ToArray();
        AddFreqAndSalary(rows, "B", filtered, row => row.Sex3, filtered);
    }

    private static void AddMinority(RowMap rows, IReadOnlyList<PreparedGraduate> prepared)
    {
        // CF-C-03 / CF-S-02
        var filtered = prepared.Where(row => In(row.Minstat, "NONMIN", "MINOR")).ToArray();
        AddFreqAndSalary(rows, "C", filtered, row => row.Minstat, filtered);
    }

    private static void AddMinorityGender(RowMap rows, IReadOnlyList<PreparedGraduate> prepared)
    {
        // CF-C-04 / CF-S-03
        var filtered = prepared
            .Where(row => In(row.Minstat, "NONMIN", "MINOR") && In(row.Sex3, "F", "M", "N"))
            .ToArray();
        AddFreqAndSalary(rows, "C1", filtered, row => string.Concat(row.Minstat, row.Sex3), filtered);
    }

    private static void AddEmploymentStatus(RowMap rows, IReadOnlyList<PreparedGraduate> prepared)
    {
        // CF-C-05 / CF-S-04
        var countRows = prepared.Where(row => row.Jobcat1 != "UNKN").ToArray();
        AddFreq(rows, "D", countRows, row => row.Jobcat);
        var salaryRows = prepared.Where(row => In(row.Jobcat, LegacyRecodes.SalaryJobcats)).ToArray();
        AddSalary(rows, "D", salaryRows, row => row.Jobcat);
    }

    private static void AddEmployedRollup(RowMap rows, IReadOnlyList<PreparedGraduate> prepared)
    {
        // CF-C-06: written NOT IN ('7-USKW','8-UNWK'); sum count/percent.
        var status = Freq(prepared.Where(row => row.Jobcat1 != "UNKN"), row => row.Jobcat);
        var rolled = status
            .Where(item => !LegacyRecodes.WrittenD1Exclusions.Contains(item.Key, StringComparer.Ordinal))
            .GroupBy(item => LegacyRecodes.MapD1Newvar(item.Key) ?? string.Empty, StringComparer.Ordinal)
            .Select(group => (
                Key: group.Key,
                Count: group.Sum(item => item.Count),
                Percent: group.Sum(item => item.Percent)))
            .Where(item => item.Key.Length > 0)
            .ToArray();

        foreach (var item in rolled)
        {
            MergeCount(rows, "D1", item.Key, item.Count, item.Percent);
        }
    }

    private static void AddEmployedSalaries(RowMap rows, IReadOnlyList<PreparedGraduate> prepared)
    {
        // CF-S-05: BY code only; no FT/employment filter. newvar EMPL.
        AddSalary(rows, "D1", prepared, _ => "EMPL");
    }

    private static void AddFullTimePartTime(RowMap rows, IReadOnlyList<PreparedGraduate> prepared)
    {
        // CF-C-07: jobftpt ne ' '; compress(jobcat||jobftpt). $jobcat1 is not applied (CF-AMB-01).
        var countRows = prepared.Where(row => !IsBlank(row.JobFtPt)).ToArray();
        AddFreq(rows, "D3", countRows, row => LegacyRecodes.CompressJobcatAndFtPt(row.Jobcat, row.JobFtPt));
    }

    private static void AddD3Salaries(RowMap rows, IReadOnlyList<PreparedGraduate> prepared)
    {
        // CF-S-19: force FULL on the key; do not filter jobftpt.
        var salaryRows = prepared.Where(row => In(row.Jobcat, LegacyRecodes.SalaryJobcats)).ToArray();
        AddSalary(rows, "D3", salaryRows, row => LegacyRecodes.CompressJobcatAndFtPt(row.Jobcat, "FULL"));
    }

    private static void AddSector(RowMap rows, IReadOnlyList<PreparedGraduate> prepared)
    {
        // CF-C-08: delete EMPUNK; PUBLIC/PRIVATE; sum count/percent.
        var status = Freq(prepared.Where(row => !IsBlank(row.Empgen)), row => row.Empgen)
            .Where(item => item.Key != "EMPUNK")
            .ToArray();
        foreach (var group in status.GroupBy(item => LegacyRecodes.MapSector(item.Key) ?? string.Empty, StringComparer.Ordinal))
        {
            if (group.Key.Length == 0)
            {
                continue;
            }

            MergeCount(rows, "D2", group.Key, group.Sum(item => item.Count), group.Sum(item => item.Percent));
        }

        // CF-S-06 / CF-S-07
        AddSalary(rows, "D2", prepared.Where(row => In(row.Empgen, LegacyRecodes.PrivateEmpgen)).ToArray(), _ => "PRIVATE");
        AddSalary(rows, "D2", prepared.Where(row => In(row.Empgen, LegacyRecodes.PublicEmpgen)).ToArray(), _ => "PUBLIC");
    }

    private static void AddEmployerType(RowMap rows, IReadOnlyList<PreparedGraduate> prepared)
    {
        // CF-C-09 / CF-S-08
        var countRows = prepared.Where(row => !IsBlank(row.Empgen)).ToArray();
        AddFreq(rows, "E1", countRows, row => row.Empgen == "EMPUNK" ? "ZEMPUN" : row.Empgen);
        AddSalary(rows, "E1", prepared, row => row.Empgen == "EMPUNK" ? "ZEMPUN" : row.Empgen);
    }

    private static void AddJobsByEmployerAndJobcat(
        RowMap rows,
        IReadOnlyList<PreparedGraduate> prepared,
        string empgen,
        string analvar)
    {
        var forEmployer = prepared.Where(row => row.Empgen == empgen).ToArray();
        AddFreq(rows, analvar, forEmployer, row => row.Jobcat);
        var salaryRows = forEmployer.Where(row => In(row.Jobcat, LegacyRecodes.SalaryJobcats)).ToArray();
        AddSalary(rows, analvar, salaryRows, row => row.Jobcat);
    }

    private static void AddClerkships(RowMap rows, IReadOnlyList<PreparedGraduate> prepared)
    {
        // CF-C-14 / CF-S-13
        var clerks = prepared.Where(row => row.Empgen == "CLERK").ToArray();
        AddFreq(rows, "E55", clerks.Where(row => !IsBlank(row.Emptype1)).ToArray(), row => row.Emptype1);
        AddSalary(rows, "E55", clerks, row => row.Emptype1);
    }

    private static void AddFirmSize(RowMap rows, IReadOnlyList<PreparedGraduate> prepared)
    {
        // CF-C-16 / CF-S-15
        var firms = prepared.Where(row => row.Empgen == "FIRM").ToArray();
        foreach (var item in Freq(firms, row => row.Firm1))
        {
            var mapped = LegacyRecodes.MapFirmSizeForCounts(item.Key);
            if (mapped is null)
            {
                continue;
            }

            MergeCount(rows, "FIRM", mapped, item.Count, item.Percent);
        }

        foreach (var group in firms.GroupBy(row => LegacyRecodes.MapFirmSizeForSalaries(row.Firm1), StringComparer.Ordinal))
        {
            if (group.Key is null)
            {
                continue;
            }

            MergeSalary(rows, "FIRM", group.Key, group.Select(row => row.SalFtPerm));
        }
    }

    private static void AddFirmJobType(RowMap rows, IReadOnlyList<PreparedGraduate> prepared)
    {
        // CF-C-17 / CF-S-16
        var firms = prepared.Where(row => row.Empgen == "FIRM" && !IsBlank(row.Lfjob)).ToArray();
        AddFreqAndSalary(rows, "FIRM2", firms, row => row.Lfjob, firms);
    }

    private static void AddRegion(RowMap rows, IReadOnlyList<PreparedGraduate> prepared)
    {
        // CF-C-18 / CF-S-17
        var filtered = prepared.Where(row => Ge0(row.Jobreg)).ToArray();
        AddFreqAndSalary(rows, "JOBREG1", filtered, row => row.Jobreg, filtered);
    }

    private static void AddLocation(RowMap rows, IReadOnlyList<PreparedGraduate> prepared)
    {
        // CF-C-19 counts use jobreg ge '0'; CF-S-18 salaries use locationflag ne ' ' only.
        var countRows = prepared.Where(row => Ge0(row.Jobreg)).ToArray();
        AddFreq(rows, "JOBREG2", countRows, row => row.LocationFlag);
        var salaryRows = prepared.Where(row => !IsBlank(row.LocationFlag)).ToArray();
        AddSalary(rows, "JOBREG2", salaryRows, row => row.LocationFlag);
    }

    private static void AddStateCount(RowMap rows, IReadOnlyList<PreparedGraduate> prepared)
    {
        // CF-C-20: freq jobst, freq that output, SUM count → distinct jobst.
        var first = prepared
            .Where(row => Gt0(row.Jobreg) && row.Jobreg != "X" && !IsBlank(row.Jobst))
            .Select(row => row.Jobst!)
            .Distinct(StringComparer.Ordinal)
            .ToArray();
        if (first.Length == 0)
        {
            return;
        }

        MergeCount(rows, "JOBREG3", "JOBREG3", first.Length, 100m);
    }

    private static void AddSource(RowMap rows, IReadOnlyList<PreparedGraduate> prepared)
    {
        // CF-P2-03
        AddFreq(rows, "SOURCE", prepared.Where(row => !IsBlank(row.Source)).ToArray(), row => row.Source);
    }

    private static void AddTiming(RowMap rows, IReadOnlyList<PreparedGraduate> prepared)
    {
        // CF-P2-04
        AddFreq(
            rows,
            "TIME",
            prepared.Where(row => !IsBlank(row.Time1)).ToArray(),
            row => LegacyRecodes.RecodeOfferTiming(row.Time1));
    }

    private static void AddSearchStatus(RowMap rows, IReadOnlyList<PreparedGraduate> prepared)
    {
        // CF-P2-05: status ne ' ' only — no employment filter.
        AddFreq(rows, "ZSTATUS", prepared.Where(row => !IsBlank(row.Status)).ToArray(), row => row.Status);
    }

    private static void AddDuration(RowMap rows, IReadOnlyList<PreparedGraduate> prepared)
    {
        // CF-P2-01: duration codes become columns; overall + by empgen.
        var eligible = prepared.Where(row => !IsBlank(row.Empgen) && !IsBlank(row.Duration)).ToArray();
        if (eligible.Length == 0)
        {
            return;
        }

        MergeDuration(rows, string.Empty, eligible);
        foreach (var group in eligible.GroupBy(row => row.Empgen!, StringComparer.Ordinal))
        {
            MergeDuration(rows, group.Key, group.ToArray());
        }
    }

    private static void AddFunded(RowMap rows, IReadOnlyList<PreparedGraduate> prepared)
    {
        // CF-P2-02: keep YES/Y; count stored as DurationCounts["PERM"] analog via Count.
        var funded = Freq(prepared.Where(row => !IsBlank(row.SchoolFund)), row => row.SchoolFund)
            .Where(item => item.Key is "YES" or "Y");
        foreach (var item in funded)
        {
            MergeCount(rows, "LAW SCHOOL FUNDED", item.Key, item.Count, null);
        }
    }

    private static void AddFreqAndSalary(
        RowMap rows,
        string analvar,
        IReadOnlyList<PreparedGraduate> countRows,
        Func<PreparedGraduate, string?> key,
        IReadOnlyList<PreparedGraduate> salaryRows)
    {
        AddFreq(rows, analvar, countRows, key);
        AddSalary(rows, analvar, salaryRows, key);
    }

    private static void AddFreq(
        RowMap rows,
        string analvar,
        IReadOnlyList<PreparedGraduate> source,
        Func<PreparedGraduate, string?> key)
    {
        foreach (var item in Freq(source, key))
        {
            MergeCount(rows, analvar, item.Key, item.Count, item.Percent);
        }
    }

    private static void AddSalary(
        RowMap rows,
        string analvar,
        IReadOnlyList<PreparedGraduate> source,
        Func<PreparedGraduate, string?> key)
    {
        foreach (var group in source.GroupBy(key, StringComparer.Ordinal))
        {
            if (group.Key is null || IsBlank(group.Key))
            {
                continue;
            }

            MergeSalary(rows, analvar, group.Key, group.Select(row => row.SalFtPerm));
        }
    }

    private static IReadOnlyList<(string Key, int Count, decimal Percent)> Freq(
        IEnumerable<PreparedGraduate> source,
        Func<PreparedGraduate, string?> key)
    {
        var groups = source
            .Select(key)
            .Where(value => value is not null && !IsBlank(value))
            .GroupBy(value => value!, StringComparer.Ordinal)
            .Select(group => (Key: group.Key, Count: group.Count()))
            .ToArray();
        var total = groups.Sum(item => item.Count);
        if (total == 0)
        {
            return [];
        }

        return groups
            .Select(item => (item.Key, item.Count, 100m * item.Count / total))
            .ToArray();
    }

    private static void MergeCount(RowMap rows, string analvar, string newvar, int count, decimal? percent)
    {
        var key = (analvar, newvar);
        rows.TryGetValue(key, out var existing);
        rows[key] = WithCount(existing, analvar, newvar, count, percent);
    }

    private static void MergeSalary(RowMap rows, string analvar, string newvar, IEnumerable<decimal?> salaries)
    {
        var stats = SasUnivariate.SuppressUnlessEligible(SasUnivariate.Calculate(salaries));
        if (stats is null)
        {
            return;
        }

        var key = (analvar, newvar);
        rows.TryGetValue(key, out var existing);
        rows[key] = WithSalary(existing, analvar, newvar, stats);
    }

    private static void MergeDuration(RowMap rows, string newvar, IReadOnlyList<PreparedGraduate> source)
    {
        var counts = source
            .GroupBy(row => row.Duration!, StringComparer.Ordinal)
            .ToDictionary(group => group.Key, group => group.Count(), StringComparer.Ordinal);
        var key = ("DURATION", newvar);
        rows[key] = new SchoolReportRow
        {
            Analvar = "DURATION",
            Newvar = newvar,
            Count = counts.Values.Sum(),
            DurationCounts = counts,
        };
    }

    private static SchoolReportRow WithCount(
        SchoolReportRow? existing,
        string analvar,
        string newvar,
        int count,
        decimal? percent)
    {
        return new SchoolReportRow
        {
            Analvar = analvar,
            Newvar = newvar,
            Count = count,
            Percent = percent,
            SalaryN = existing?.SalaryN,
            Pct25 = existing?.Pct25,
            Median = existing?.Median,
            Pct75 = existing?.Pct75,
            Mean = existing?.Mean,
            DurationCounts = existing?.DurationCounts,
        };
    }

    private static SchoolReportRow WithSalary(
        SchoolReportRow? existing,
        string analvar,
        string newvar,
        SalaryStatistics stats)
    {
        return new SchoolReportRow
        {
            Analvar = analvar,
            Newvar = newvar,
            Count = existing?.Count,
            Percent = existing?.Percent,
            SalaryN = stats.N,
            Pct25 = stats.Pct25,
            Median = stats.Median,
            Pct75 = stats.Pct75,
            Mean = stats.Mean,
            DurationCounts = existing?.DurationCounts,
        };
    }

    private static bool IsBlank(string? value) =>
        string.IsNullOrEmpty(value) || value == " ";

    private static bool In(string? value, params string[] allowed) =>
        value is not null && allowed.Contains(value, StringComparer.Ordinal);

    private static bool In(string? value, IReadOnlyList<string> allowed) =>
        value is not null && allowed.Contains(value, StringComparer.Ordinal);

    private static bool Ge0(string? jobreg) =>
        !IsBlank(jobreg) && string.CompareOrdinal(jobreg, "0") >= 0;

    private static bool Gt0(string? jobreg) =>
        !IsBlank(jobreg) && string.CompareOrdinal(jobreg, "0") > 0;

    private sealed record PreparedGraduate(
        string? Sex3,
        string? Minstat,
        string? Jobcat1,
        string? Jobcat,
        string? JobFtPt,
        string? Empgen,
        string? Firm1,
        string? Lfjob,
        string? Jobreg,
        string? LocationFlag,
        string? Jobst,
        string? Source,
        string? Time1,
        string? Status,
        string? Duration,
        string? SchoolFund,
        decimal? SalFtPerm,
        string? Emptype1);

    private sealed class KeyComparer : IEqualityComparer<(string Analvar, string Newvar)>
    {
        public static readonly KeyComparer Instance = new();

        public bool Equals((string Analvar, string Newvar) x, (string Analvar, string Newvar) y) =>
            string.Equals(x.Analvar, y.Analvar, StringComparison.Ordinal)
            && string.Equals(x.Newvar, y.Newvar, StringComparison.Ordinal);

        public int GetHashCode((string Analvar, string Newvar) obj) =>
            HashCode.Combine(StringComparer.Ordinal.GetHashCode(obj.Analvar), StringComparer.Ordinal.GetHashCode(obj.Newvar));
    }

    private sealed class RowMap : Dictionary<(string Analvar, string Newvar), SchoolReportRow>
    {
        public RowMap()
            : base(KeyComparer.Instance)
        {
        }
    }
}
