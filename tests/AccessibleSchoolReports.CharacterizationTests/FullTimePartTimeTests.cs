using AccessibleSchoolReports.CharacterizationTests.Support;

namespace AccessibleSchoolReports.CharacterizationTests;

public sealed class FullTimePartTimeTests
{
    [Fact]
    [Trait("RuleId", RuleId.CfC07)]
    public void D3_Newvar_IsCompressedJobcatPlusJobftpt()
    {
        Assert.Equal("1-LJDFULL", LegacyRules.CompressJobcatAndFtPt("1-LJD", "FULL"));
        Assert.Equal("2-NLJDPART", LegacyRules.CompressJobcatAndFtPt("2-NLJD", "PART"));
        Assert.Equal("Bar Admission Required/ Anticipated: Full-time", LegacyRules.ReportRowLabels["1-LJDFULL"]);
    }

    [Fact]
    [Trait("RuleId", RuleId.CfS19)]
    [Trait("RuleId", RuleId.CfAmb04)]
    public void D3_SalaryStep_ForcesFull_WithoutFtFilter()
    {
        string newvar = LegacyRules.CompressJobcatAndFtPt("1-LJD", "FULL");
        Assert.Equal("1-LJDFULL", newvar);
        Assert.Contains("FULL", newvar, StringComparison.Ordinal);
    }

    [Fact]
    [Trait("RuleId", RuleId.CfC07)]
    public void BaselinePdf_FtPtCounts()
    {
        Assert.Equal(79, BaselineSchoolReport.LjdFullTime.Count);
        Assert.Equal(10, BaselineSchoolReport.JdAdvantageFullTime.Count);
        Assert.Equal(2, BaselineSchoolReport.JdAdvantagePartTime.Count);
    }

    [Fact]
    [Trait("RuleId", RuleId.CfC05)]
    [Trait("RuleId", RuleId.CfC07)]
    public void BaselinePdf_LjdFullTimeCount_IsNotTheSameAsPage1Ljd()
    {
        Assert.Equal(78, BaselineSchoolReport.Ljd.Count);
        Assert.Equal(79, BaselineSchoolReport.LjdFullTime.Count);
    }

    [Fact(Skip = "TODO CF-C-07 / CF-S-19: page 2 LJD Full-time is 79 while page 1 LJD is 78; D3 detail counts sum to 94 while the printed D3 subtotal is 93. Do not invent a reconciliation.")]
    [Trait("RuleId", RuleId.CfC07)]
    public void D3_DetailSum_Vs_PrintedSubtotal()
    {
    }

    [Fact(Skip = "TODO CF-AMB-01: FORMAT jobftpt $jobcat1. is undefined. Do not assume FULL/PART labels from $jobcat.")]
    [Trait("RuleId", RuleId.CfAmb01)]
    public void Jobftpt_FormattedValue_UsedInCompress()
    {
    }
}
