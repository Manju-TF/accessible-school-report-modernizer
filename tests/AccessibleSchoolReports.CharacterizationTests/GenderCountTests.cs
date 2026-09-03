using AccessibleSchoolReports.CharacterizationTests.Support;

namespace AccessibleSchoolReports.CharacterizationTests;

public sealed class GenderCountTests
{
    [Theory]
    [InlineData("W", "F")]
    [InlineData("X", "N")]
    [InlineData("ND", " ")]
    [InlineData("F", "F")]
    [InlineData("M", "M")]
    [InlineData("N", "N")]
    [Trait("RuleId", RuleId.CfPrep06)]
    public void Sex3_RecodesBeforeGenderTables(string input, string expected)
    {
        Assert.Equal(expected, LegacyRules.RecodeSex3(input));
    }

    [Fact]
    [Trait("RuleId", RuleId.CfC02)]
    public void GenderCounts_UseRecodedSex3_AsNewvar_AnalvarB()
    {
        Assert.Equal("B", BaselineSchoolReport.Women.Analvar);
        Assert.Equal("Women", LegacyRules.ReportRowLabels["F"]);
        Assert.Equal("Men", LegacyRules.ReportRowLabels["M"]);
    }

    [Fact]
    [Trait("RuleId", RuleId.CfC02)]
    [Trait("RuleId", RuleId.SsPrep02)]
    public void BaselinePdf_GenderCounts_Women46_Men54()
    {
        Assert.Equal(46, BaselineSchoolReport.Women.Count);
        Assert.Equal(54, BaselineSchoolReport.Men.Count);
        Assert.Equal(46.0m, BaselineSchoolReport.Women.Percent);
        Assert.Equal(54.0m, BaselineSchoolReport.Men.Percent);
        Assert.Equal(100, BaselineSchoolReport.Women.Count + BaselineSchoolReport.Men.Count);
        Assert.Equal(BaselineSchoolReport.TotalReported, BaselineSchoolReport.Women.Count + BaselineSchoolReport.Men.Count);
    }

    [Fact]
    [Trait("RuleId", RuleId.SsFil08)]
    public void BaselinePdf_HasNoNonBinaryGenderRow()
    {
        Assert.Contains("Non-binary or Chose to Self-identify", BaselineSchoolReport.AbsentRowLabelsOnThisPdf);
    }

    [Fact]
    [Trait("RuleId", RuleId.CfDead02)]
    public void CommentedSexRecodes_AreNotTheLiveGenderRule()
    {
        Assert.Equal("F", LegacyRules.RecodeSex3("W"));
        Assert.Equal("N", LegacyRules.RecodeSex3("X"));
    }

    [Fact]
    [Trait("RuleId", RuleId.CfC04)]
    public void BaselinePdf_GenderAndRaceCounts()
    {
        Assert.Equal(10, BaselineSchoolReport.WomenOfColor.Count);
        Assert.Equal(4, BaselineSchoolReport.MenOfColor.Count);
        Assert.Equal(30, BaselineSchoolReport.WhiteWomen.Count);
        Assert.Equal(20, BaselineSchoolReport.WhiteMen.Count);
        Assert.Equal(64, BaselineSchoolReport.WomenOfColor.Count
            + BaselineSchoolReport.MenOfColor.Count
            + BaselineSchoolReport.WhiteWomen.Count
            + BaselineSchoolReport.WhiteMen.Count);
    }
}
