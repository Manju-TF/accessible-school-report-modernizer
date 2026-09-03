using AccessibleSchoolReports.CharacterizationTests.Support;

namespace AccessibleSchoolReports.CharacterizationTests;

public sealed class SalaryPercentileTests
{
    [Fact]
    [Trait("RuleId", RuleId.SsSal02)]
    public void BaselinePdf_25thPercentile_WomenAndEmployed()
    {
        Assert.Equal(70000, BaselineSchoolReport.Women.Pct25);
        Assert.Equal(70000, BaselineSchoolReport.Employed.Pct25);
        Assert.Equal(84000, BaselineSchoolReport.PrivateSector.Pct25);
        Assert.Equal(66000, BaselineSchoolReport.PublicSector.Pct25);
    }

    [Fact]
    [Trait("RuleId", RuleId.SsSal03)]
    public void BaselinePdf_Median_WomenMenAndEmployed()
    {
        Assert.Equal(85500, BaselineSchoolReport.Women.Median);
        Assert.Equal(90500, BaselineSchoolReport.Men.Median);
        Assert.Equal(89000, BaselineSchoolReport.Employed.Median);
        Assert.Equal(73950, BaselineSchoolReport.PublicSector.Median);
    }

    [Fact]
    [Trait("RuleId", RuleId.SsSal04)]
    public void BaselinePdf_75thPercentile_WomenAndSectors()
    {
        Assert.Equal(110000, BaselineSchoolReport.Women.Pct75);
        Assert.Equal(120000, BaselineSchoolReport.Men.Pct75);
        Assert.Equal(105000, BaselineSchoolReport.Employed.Pct75);
        Assert.Equal(106000, BaselineSchoolReport.PrivateSector.Pct75);
        Assert.Equal(92000, BaselineSchoolReport.PublicSector.Pct75);
    }

    [Fact]
    [Trait("RuleId", RuleId.SsSal02)]
    public void BaselinePdf_SuppressedRows_HaveNoPercentiles()
    {
        Assert.Null(BaselineSchoolReport.MenOfColor.Pct25);
        Assert.Null(BaselineSchoolReport.MenOfColor.Median);
        Assert.Null(BaselineSchoolReport.MenOfColor.Pct75);
        Assert.Null(BaselineSchoolReport.Mountain.Pct25);
    }

    [Fact]
    [Trait("RuleId", RuleId.SsSal02)]
    public void Men25th_IsRecordedFromPdfTextLayer_As850000()
    {
        Assert.Equal(850000, BaselineSchoolReport.Men.Pct25);
    }

    [Fact(Skip = "TODO SS-SAL-02/03/04: PROC UNIVARIATE q1/median/q3 on salftperm. PCTLDEF is not set in createschrptfiles2025.sas. Do not invent a percentile algorithm or raw salary list.")]
    [Trait("RuleId", RuleId.SsSal02)]
    [Trait("RuleId", RuleId.SsSal03)]
    [Trait("RuleId", RuleId.SsSal04)]
    public void Percentiles_FromRawSalaries()
    {
    }
}
