using AccessibleSchoolReports.CharacterizationTests.Support;

namespace AccessibleSchoolReports.CharacterizationTests;

public sealed class EmployerTypeTests
{
    [Fact]
    [Trait("RuleId", RuleId.CfC09)]
    public void Empunk_BecomesZempun_OnEmployerTypeCounts()
    {
        Assert.Equal("ZEMPUN", LegacyRules.RecodeEmployerTypeForCounts("EMPUNK"));
        Assert.Equal("FIRM", LegacyRules.RecodeEmployerTypeForCounts("FIRM"));
        Assert.Equal("Unknown Type", LegacyRules.ReportRowLabels["ZEMPUN"]);
    }

    [Fact]
    [Trait("RuleId", RuleId.CfC09)]
    [Trait("RuleId", RuleId.CfS08)]
    public void BaselinePdf_EmployerTypeCounts()
    {
        Assert.Equal(14, BaselineSchoolReport.Business.Count);
        Assert.Equal(5, BaselineSchoolReport.Clerkships.Count);
        Assert.Equal(50, BaselineSchoolReport.PrivatePractice.Count);
        Assert.Equal(16, BaselineSchoolReport.Government.Count);
        Assert.Equal(8, BaselineSchoolReport.PublicInterest.Count);
        Assert.Equal(93, BaselineSchoolReport.Business.Count
            + BaselineSchoolReport.Clerkships.Count
            + BaselineSchoolReport.PrivatePractice.Count
            + BaselineSchoolReport.Government.Count
            + BaselineSchoolReport.PublicInterest.Count);
    }

    [Fact]
    [Trait("RuleId", RuleId.SsFil08)]
    public void BaselinePdf_OmitsEducationAndUnknownEmployerType()
    {
        Assert.Contains("Education", BaselineSchoolReport.AbsentRowLabelsOnThisPdf);
        Assert.Contains("Education Jobs:", BaselineSchoolReport.AbsentSectionsOnThisPdf);
    }

    [Theory]
    [InlineData("ACAD", "E2")]
    [InlineData("BUS", "E3")]
    [InlineData("FIRM", "E4")]
    [InlineData("GOVT", "E5")]
    [InlineData("CLERK", "E55")]
    [InlineData("PUBINT", "E6")]
    [Trait("RuleId", RuleId.CfC10)]
    [Trait("RuleId", RuleId.CfC11)]
    [Trait("RuleId", RuleId.CfC12)]
    [Trait("RuleId", RuleId.CfC13)]
    [Trait("RuleId", RuleId.CfC14)]
    [Trait("RuleId", RuleId.CfC15)]
    public void EmployerType_HasDedicatedAnalvarSlice(string empgen, string analvar)
    {
        Assert.True(LegacyRules.SectionHeaders.ContainsKey(analvar));
        Assert.Contains(empgen, new[] { "ACAD", "BUS", "FIRM", "GOVT", "CLERK", "PUBINT" });
    }
}
