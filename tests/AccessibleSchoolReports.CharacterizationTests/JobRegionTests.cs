using AccessibleSchoolReports.CharacterizationTests.Support;

namespace AccessibleSchoolReports.CharacterizationTests;

public sealed class JobRegionTests
{
    [Theory]
    [InlineData("1", "New England")]
    [InlineData("2", "Mid-Atlantic")]
    [InlineData("5", "South Atlantic")]
    [InlineData("6", "E South Central")]
    [InlineData("8", "Mountain")]
    [InlineData("X", "Non-US locations")]
    [InlineData("T", "US Territories")]
    [Trait("RuleId", RuleId.SsFmt04)]
    [Trait("RuleId", RuleId.CfC18)]
    public void RegionCodes_MapToReportLabels(string code, string label)
    {
        Assert.Equal(label, LegacyRules.ReportRowLabels[code]);
    }

    [Fact]
    [Trait("RuleId", RuleId.CfC19)]
    public void LocationFlags_MapToInOutForeign()
    {
        Assert.Equal("In-State", LegacyRules.ReportRowLabels["INSTATE"]);
        Assert.Equal("Out of State", LegacyRules.ReportRowLabels["OUTOFSTATE"]);
        Assert.Equal("Foreign", LegacyRules.ReportRowLabels["FOREIGN"]);
    }

    [Fact]
    [Trait("RuleId", RuleId.CfC18)]
    public void BaselinePdf_RegionCounts()
    {
        Assert.Equal(69, BaselineSchoolReport.NewEngland.Count);
        Assert.Equal(16, BaselineSchoolReport.MidAtlantic.Count);
        Assert.Equal(5, BaselineSchoolReport.Mountain.Count);
        Assert.Equal(74.2m, BaselineSchoolReport.NewEngland.Percent);
    }

    [Fact]
    [Trait("RuleId", RuleId.CfC19)]
    public void BaselinePdf_LocationCounts()
    {
        Assert.Equal(63, BaselineSchoolReport.InState.Count);
        Assert.Equal(30, BaselineSchoolReport.OutOfState.Count);
        Assert.Equal(93, BaselineSchoolReport.InState.Count + BaselineSchoolReport.OutOfState.Count);
        Assert.Contains("Foreign", BaselineSchoolReport.AbsentRowLabelsOnThisPdf);
    }

    [Fact]
    [Trait("RuleId", RuleId.CfC20)]
    [Trait("RuleId", RuleId.SsSub02)]
    public void StateCount_UsesTotalHashLabel_NotSubtotal()
    {
        Assert.Equal("Total #", LegacyRules.SubtotalLabel("JOBREG3"));
        Assert.Equal("# States and Territories with Employed Grads:", LegacyRules.SectionHeaders["JOBREG3"]);
    }

    [Fact]
    [Trait("RuleId", RuleId.CfC20)]
    public void BaselinePdf_StateTerritoryCount_Is14()
    {
        Assert.Equal(14, BaselineSchoolReport.StatesAndTerritoriesWithEmployedGrads);
    }

    [Fact(Skip = "TODO CF-C-20: JOBREG3 is a second-freq plus MEANS SUM of jobst. The graduate-level rule for which states count is not fully settled (jobreg gt '0' and ne 'X').")]
    [Trait("RuleId", RuleId.CfC20)]
    public void StateCount_ExactJobstMembership()
    {
    }
}
