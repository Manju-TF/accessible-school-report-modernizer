using AccessibleSchoolReports.CharacterizationTests.Support;

namespace AccessibleSchoolReports.CharacterizationTests;

public sealed class JobTimingTests
{
    [Fact]
    [Trait("RuleId", RuleId.CfFmt01)]
    public void BuilderTimeFormat_UsesTitleCaseGraduation()
    {
        Assert.Equal("Before Graduation", LegacyRules.BuilderTimeFormat["BGRAD"]);
        Assert.Equal("After Graduation", LegacyRules.BuilderTimeFormat["AFTGRD"]);
    }

    [Fact]
    [Trait("RuleId", RuleId.SsFmt04)]
    [Trait("RuleId", RuleId.CfP204)]
    public void ReportTimeLabels_UseSentenceCase_OnBgrad()
    {
        Assert.Equal("Before graduation", LegacyRules.ReportRowLabels["BGRAD"]);
    }

    [Fact]
    [Trait("RuleId", RuleId.CfP204)]
    public void BaselinePdf_TimingCounts()
    {
        Assert.Equal(49, BaselineSchoolReport.BeforeGraduation.Count);
        Assert.Equal(31, BaselineSchoolReport.AfterGraduation.Count);
        Assert.Equal(80, BaselineSchoolReport.BeforeGraduation.Count + BaselineSchoolReport.AfterGraduation.Count);
        Assert.Equal(61.3m, BaselineSchoolReport.BeforeGraduation.Percent);
        Assert.Equal(38.8m, BaselineSchoolReport.AfterGraduation.Percent);
    }

    [Fact]
    [Trait("RuleId", RuleId.SsCalc02)]
    public void BaselinePdf_TimingPercents_SumTo100Point1_AtOneDecimal()
    {
        decimal sum = LegacyRules.SumStoredPercents(
            BaselineSchoolReport.BeforeGraduation.Percent,
            BaselineSchoolReport.AfterGraduation.Percent);
        Assert.Equal(100.1m, sum);
    }

    [Fact]
    [Trait("RuleId", RuleId.SsSup04)]
    public void Renderer_HasNoSelfprWhere_OnTiming()
    {
        Assert.Equal("Started own business/practice", LegacyRules.BuilderSourceFormat["SELFPR"]);
    }

    [Fact(Skip = "TODO SS-SUP-04: page 6 note says timing excludes own practice. schreptsummary_2025.sas has no SELFPR WHERE. Do not invent the exclusion.")]
    [Trait("RuleId", RuleId.SsSup04)]
    public void Timing_OwnPracticeExclusion_Mechanism()
    {
    }
}
