using AccessibleSchoolReports.Application.Reporting;
using AccessibleSchoolReports.Domain.Entities;

namespace AccessibleSchoolReports.UnitTests.Reporting;

public sealed class SchoolReportCalculatorTests
{
    private readonly ISchoolReportCalculator _calculator = new SchoolReportCalculator();

    [Fact]
    [Trait("RuleId", "CF-C-01")]
    public void Total_CountsEveryRow_AsAnalvarA()
    {
        var report = Calculate(GraduateFactory.Create(), GraduateFactory.Create(sex3: "M"));
        var total = Row(report, "A", "A");
        Assert.Equal(2, total.Count);
        Assert.Equal(100m, total.Percent);
        Assert.Null(total.SalaryN);
    }

    [Fact]
    [Trait("RuleId", "CF-C-02")]
    [Trait("RuleId", "CF-PREP-06")]
    public void Gender_UsesRecodedSex3_AndOmitsNd()
    {
        var report = Calculate(
            GraduateFactory.Create(sex3: "W"),
            GraduateFactory.Create(sex3: "M"),
            GraduateFactory.Create(sex3: "ND"));
        Assert.Equal(1, Row(report, "B", "F").Count);
        Assert.Equal(1, Row(report, "B", "M").Count);
        Assert.DoesNotContain(report.Rows, row => row.Analvar == "B" && row.Newvar == "N");
        Assert.DoesNotContain(report.Rows, row => row.Analvar == "B" && row.Newvar == " ");
    }

    [Fact]
    [Trait("RuleId", "CF-C-03")]
    public void Minority_UsesOnlyNonminAndMinor()
    {
        var report = Calculate(
            GraduateFactory.Create(minstat: "MINOR"),
            GraduateFactory.Create(minstat: "NONMIN"),
            GraduateFactory.Create(minstat: "UNK"));
        Assert.Equal(1, Row(report, "C", "MINOR").Count);
        Assert.Equal(1, Row(report, "C", "NONMIN").Count);
        Assert.Equal(50m, Row(report, "C", "MINOR").Percent);
    }

    [Fact]
    [Trait("RuleId", "CF-C-04")]
    public void CrossTab_ConcatenatesWithoutSpace()
    {
        var report = Calculate(GraduateFactory.Create(minstat: "MINOR", sex3: "F"));
        Assert.Equal(1, Row(report, "C1", "MINORF").Count);
    }

    [Fact]
    [Trait("RuleId", "CF-C-05")]
    public void EmploymentStatus_ExcludesUnkn_AndUsesFormattedJobcat()
    {
        var report = Calculate(
            GraduateFactory.Create(jobcat1: "LJD"),
            GraduateFactory.Create(jobcat1: "UNKN"));
        Assert.Equal(1, Row(report, "D", "1-LJD").Count);
        Assert.DoesNotContain(report.Rows, row => row.Analvar == "D" && row.Newvar == "UNKN");
    }

    [Fact]
    [Trait("RuleId", "CF-C-06")]
    [Trait("RuleId", "CF-AMB-02")]
    public void D1_Rollup_KeepsWrittenNotIn_AndDoesNotExcludeFormattedUskw()
    {
        var report = Calculate(
            GraduateFactory.Create(jobcat1: "LJD"),
            GraduateFactory.Create(jobcat1: "ADVD"),
            GraduateFactory.Create(jobcat1: "USKW"),
            GraduateFactory.Create(jobcat1: "UNWK"));
        Assert.Equal(1, Row(report, "D1", "EMPL").Count);
        Assert.Equal(1, Row(report, "D1", "6-ADVD").Count);
        Assert.DoesNotContain(report.Rows, row => row.Analvar == "D1" && row.Newvar is "8-USKW" or "9-UNWK");
    }

    [Fact]
    [Trait("RuleId", "CF-C-08")]
    public void Sector_GroupsPublicPrivate_AndDeletesEmpunk()
    {
        var report = Calculate(
            GraduateFactory.Create(empgen: "FIRM"),
            GraduateFactory.Create(empgen: "GOVT"),
            GraduateFactory.Create(empgen: "EMPUNK"));
        Assert.Equal(1, Row(report, "D2", "PRIVATE").Count);
        Assert.Equal(1, Row(report, "D2", "PUBLIC").Count);
        Assert.DoesNotContain(report.Rows, row => row.Analvar == "D2" && row.Newvar == "EMPUNK");
    }

    [Fact]
    [Trait("RuleId", "CF-C-09")]
    public void EmployerType_RecodesEmpunkToZempun()
    {
        var report = Calculate(GraduateFactory.Create(empgen: "EMPUNK"));
        Assert.Equal(1, Row(report, "E1", "ZEMPUN").Count);
    }

    [Fact]
    [Trait("RuleId", "CF-C-16")]
    public void FirmSize_CountMapsSoloAndSizes()
    {
        var report = Calculate(
            GraduateFactory.Create(empgen: "FIRM", firm1: "S"),
            GraduateFactory.Create(empgen: "FIRM", firm1: "1"));
        Assert.Equal(1, Row(report, "FIRM", "SOLO").Count);
        Assert.Equal(1, Row(report, "FIRM", "LF1").Count);
    }

    [Fact]
    [Trait("RuleId", "CF-C-18")]
    [Trait("RuleId", "CF-PREP-03")]
    public void Region_IncludesRecodedX()
    {
        var report = Calculate(GraduateFactory.Create(jobreg: "0"));
        Assert.Equal(1, Row(report, "JOBREG1", "X").Count);
    }

    [Fact]
    [Trait("RuleId", "CF-C-20")]
    public void StateCount_IsDistinctJobstAfterForeignExclusion()
    {
        var report = Calculate(
            GraduateFactory.Create(jobreg: "1", jobst: "107"),
            GraduateFactory.Create(jobreg: "1", jobst: "107"),
            GraduateFactory.Create(jobreg: "1", jobst: "122"),
            GraduateFactory.Create(jobreg: "0", jobst: "999"));
        Assert.Equal(2, Row(report, "JOBREG3", "JOBREG3").Count);
    }

    [Fact]
    [Trait("RuleId", "CF-C-07")]
    [Trait("RuleId", "CF-AMB-01")]
    public void D3_CountKey_UsesRawJobftpt()
    {
        var report = Calculate(GraduateFactory.Create(jobcat1: "LJD", jobFtPt: "FULL"));
        Assert.Equal(1, Row(report, "D3", "1-LJDFULL").Count);
    }

    [Fact]
    [Trait("RuleId", "CF-S-00")]
    public void Salary_IsSuppressedWhenNIsBelow5()
    {
        var grads = Enumerable.Range(0, 4).Select(_ => GraduateFactory.Create(salFtPerm: 80000m)).ToArray();
        var report = Calculate(grads);
        Assert.Null(Row(report, "B", "F").SalaryN);
        Assert.Null(Row(report, "B", "F").Median);
    }

    [Fact]
    [Trait("RuleId", "CF-S-00")]
    public void Salary_IsEmittedWhenNIsAtLeast5()
    {
        var grads = Enumerable.Range(0, 5).Select(i => GraduateFactory.Create(salFtPerm: 80000m + i)).ToArray();
        var report = Calculate(grads);
        var women = Row(report, "B", "F");
        Assert.Equal(5, women.SalaryN);
        Assert.NotNull(women.Median);
        Assert.NotNull(women.Mean);
    }

    [Fact]
    [Trait("RuleId", "CF-S-00")]
    public void SalarySuppression_UsesSalaryN_NotHeadcount()
    {
        var grads = new[]
        {
            GraduateFactory.Create(salFtPerm: 10m),
            GraduateFactory.Create(salFtPerm: 20m),
            GraduateFactory.Create(salFtPerm: 30m),
            GraduateFactory.Create(salFtPerm: 40m),
            GraduateFactory.Create(salFtPerm: null),
        };
        var report = Calculate(grads);
        Assert.Equal(5, Row(report, "B", "F").Count);
        Assert.Null(Row(report, "B", "F").SalaryN);
    }

    [Fact]
    [Trait("RuleId", "CF-S-05")]
    [Trait("RuleId", "CF-AMB-03")]
    public void EmployedSalary_UsesAllNonMissingSalaries()
    {
        var grads = Enumerable.Range(0, 5)
            .Select(i => GraduateFactory.Create(jobcat1: "USKW", jobFtPt: "PART", salFtPerm: 50000m + i))
            .ToArray();
        var report = Calculate(grads);
        Assert.Equal(5, Row(report, "D1", "EMPL").SalaryN);
    }

    [Fact]
    [Trait("RuleId", "CF-S-15")]
    [Trait("RuleId", "CF-AMB-05")]
    public void FirmSizeSalary_DoesNotMapSolo()
    {
        var grads = Enumerable.Range(0, 5)
            .Select(i => GraduateFactory.Create(empgen: "FIRM", firm1: "S", salFtPerm: 90000m + i))
            .ToArray();
        var report = Calculate(grads);
        Assert.Equal(5, Row(report, "FIRM", "SOLO").Count);
        Assert.Null(Row(report, "FIRM", "SOLO").SalaryN);
    }

    [Fact]
    [Trait("RuleId", "CF-S-19")]
    [Trait("RuleId", "CF-AMB-04")]
    public void D3Salary_ForcesFullKey_WithoutFtFilter()
    {
        var grads = Enumerable.Range(0, 5)
            .Select(i => GraduateFactory.Create(jobcat1: "LJD", jobFtPt: "PART", salFtPerm: 70000m + i))
            .ToArray();
        var report = Calculate(grads);
        Assert.Equal(5, Row(report, "D3", "1-LJDFULL").SalaryN);
        Assert.Equal(5, Row(report, "D3", "1-LJDPART").Count);
        Assert.Null(Row(report, "D3", "1-LJDPART").SalaryN);
    }

    [Fact]
    [Trait("RuleId", "SS-SUB-01")]
    [Trait("RuleId", "SS-CALC-02")]
    public void SectionSubtotal_SumsStoredCountAndPercent_NotSalaries()
    {
        var report = Calculate(
            GraduateFactory.Create(sex3: "F", salFtPerm: 10m),
            GraduateFactory.Create(sex3: "M", salFtPerm: 20m));
        var section = report.Sections.Single(item => item.Analvar == "B");
        Assert.Equal(2, section.SubtotalCount);
        Assert.Equal(100m, section.SubtotalPercent);
        Assert.All(section.Details, row => Assert.Null(row.SalaryN));
    }

    [Fact]
    [Trait("RuleId", "CF-P2-04")]
    public void Timing_RecodesAftgrdToZaftgrd()
    {
        var report = Calculate(
            GraduateFactory.Create(time1: "BGRAD"),
            GraduateFactory.Create(time1: "AFTGRD"));
        Assert.Equal(1, Row(report, "TIME", "BGRAD").Count);
        Assert.Equal(1, Row(report, "TIME", "ZAFTGRD").Count);
    }

    [Fact]
    [Trait("RuleId", "CF-P2-02")]
    public void Funded_KeepsYesAndY()
    {
        var report = Calculate(
            GraduateFactory.Create(schoolFund: "YES"),
            GraduateFactory.Create(schoolFund: "Y"),
            GraduateFactory.Create(schoolFund: "NO"));
        Assert.Equal(1, Row(report, "LAW SCHOOL FUNDED", "YES").Count);
        Assert.Equal(1, Row(report, "LAW SCHOOL FUNDED", "Y").Count);
        Assert.DoesNotContain(report.Rows, row => row.Analvar == "LAW SCHOOL FUNDED" && row.Newvar == "NO");
    }

    [Fact]
    [Trait("RuleId", "CF-P2-01")]
    public void Duration_TransposesCodes_OverallAndByEmpgen()
    {
        var report = Calculate(
            GraduateFactory.Create(empgen: "FIRM", duration: "PERM"),
            GraduateFactory.Create(empgen: "GOVT", duration: "TEMP"));
        var overall = Row(report, "DURATION", "");
        Assert.Equal(1, overall.DurationCounts!["PERM"]);
        Assert.Equal(1, overall.DurationCounts["TEMP"]);
        Assert.Equal(1, Row(report, "DURATION", "FIRM").DurationCounts!["PERM"]);
    }

    [Fact]
    [Trait("RuleId", "CF-P2-05")]
    public void SearchStatus_DoesNotRequireEmployment()
    {
        var report = Calculate(GraduateFactory.Create(jobcat1: "USKW", status: "SET"));
        Assert.Equal(1, Row(report, "ZSTATUS", "SET").Count);
    }

    [Fact]
    [Trait("RuleId", "SS-FIL-08")]
    public void MissingCategories_AreNotEmitted()
    {
        var report = Calculate(GraduateFactory.Create(sex3: "F"));
        Assert.DoesNotContain(report.Rows, row => row.Analvar == "B" && row.Newvar == "M");
    }

    private SchoolReport Calculate(params GraduateRecord[] graduates) =>
        _calculator.Calculate("10701", graduates);

    private static SchoolReportRow Row(SchoolReport report, string analvar, string newvar) =>
        Assert.Single(report.Rows, row => row.Analvar == analvar && row.Newvar == newvar);
}
