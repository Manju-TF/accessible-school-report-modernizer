using AccessibleSchoolReports.CharacterizationTests.Support;

namespace AccessibleSchoolReports.CharacterizationTests;

public sealed class EmploymentStatusTests
{
    [Fact]
    [Trait("RuleId", RuleId.CfC05)]
    public void EmploymentStatus_IsAnalvarD_FromFormattedJobcat()
    {
        Assert.Equal("D", BaselineSchoolReport.Ljd.Analvar);
        Assert.Equal("Employment Status Known:", LegacyRules.SectionHeaders["D"]);
    }

    [Fact]
    [Trait("RuleId", RuleId.CfC05)]
    public void BaselinePdf_EmploymentStatusCounts()
    {
        Assert.Equal(78, BaselineSchoolReport.Ljd.Count);
        Assert.Equal(12, BaselineSchoolReport.JdAdvantage.Count);
        Assert.Equal(1, BaselineSchoolReport.OtherProfessional.Count);
        Assert.Equal(2, BaselineSchoolReport.OtherPosition.Count);
        Assert.Equal(3, BaselineSchoolReport.NotEmployedSeeking.Count);
        Assert.Equal(4, BaselineSchoolReport.NotEmployedNotSeeking.Count);
        Assert.Equal(100, BaselineSchoolReport.Ljd.Count
            + BaselineSchoolReport.JdAdvantage.Count
            + BaselineSchoolReport.OtherProfessional.Count
            + BaselineSchoolReport.OtherPosition.Count
            + BaselineSchoolReport.NotEmployedSeeking.Count
            + BaselineSchoolReport.NotEmployedNotSeeking.Count);
    }

    [Fact]
    [Trait("RuleId", RuleId.CfC06)]
    public void D1_Rollup_MapsListedJobcatsToEmpl_AndKeepsAdvancedDegree()
    {
        Assert.Equal("EMPL", LegacyRules.MapD1Newvar("1-LJD"));
        Assert.Equal("EMPL", LegacyRules.MapD1Newvar("5-WUNK"));
        Assert.Equal("6-ADVD", LegacyRules.MapD1Newvar("6-ADVD"));
        Assert.Null(LegacyRules.MapD1Newvar("7-UDEF"));
        Assert.Null(LegacyRules.MapD1Newvar("8-USKW"));
        Assert.Null(LegacyRules.MapD1Newvar("9-UNWK"));
    }

    [Fact]
    [Trait("RuleId", RuleId.CfC06)]
    [Trait("RuleId", RuleId.CfAmb02)]
    public void D1_WrittenExclusionList_DoesNotMatchFormattedUskwUnwk()
    {
        Assert.True(LegacyRules.WrittenD1ExclusionContains("7-USKW"));
        Assert.True(LegacyRules.WrittenD1ExclusionContains("8-UNWK"));
        Assert.False(LegacyRules.WrittenD1ExclusionContains("8-USKW"));
        Assert.False(LegacyRules.WrittenD1ExclusionContains("9-UNWK"));
        Assert.False(LegacyRules.WrittenD1ExclusionContains("7-UDEF"));
    }

    [Fact]
    [Trait("RuleId", RuleId.CfC06)]
    public void BaselinePdf_EmployedRollup_Is93()
    {
        Assert.Equal(93, BaselineSchoolReport.Employed.Count);
        Assert.Equal(93.0m, BaselineSchoolReport.Employed.Percent);
        Assert.Contains("Enrolled in Graduate Studies", BaselineSchoolReport.AbsentRowLabelsOnThisPdf);
    }

    [Fact(Skip = "TODO CF-AMB-02: do not rewrite NOT IN ('7-USKW','8-UNWK') to 8-USKW/9-UNWK. Expected D1 membership for seeking/not-seeking is the written list.")]
    [Trait("RuleId", RuleId.CfAmb02)]
    public void D1_IntendedExclusion_Vs_WrittenCodes()
    {
    }
}
