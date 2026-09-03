using AccessibleSchoolReports.CharacterizationTests.Support;

namespace AccessibleSchoolReports.CharacterizationTests;

public sealed class SubtotalTests
{
    [Theory]
    [InlineData("B", "Subtotal")]
    [InlineData("D1", "Subtotal")]
    [InlineData("E1", "Subtotal")]
    [InlineData("SOURCE", "Subtotal")]
    [InlineData("JOBREG3", "Total #")]
    [InlineData("DURATION", "Total Reported")]
    [InlineData("LAW SCHOOL FUNDED", "Total Reported")]
    [Trait("RuleId", RuleId.SsFmt03)]
    [Trait("RuleId", RuleId.SsSub01)]
    [Trait("RuleId", RuleId.SsSub02)]
    public void SubtotalLabel_DependsOnAnalvar(string analvar, string label)
    {
        Assert.Equal(label, LegacyRules.SubtotalLabel(analvar));
    }

    [Fact]
    [Trait("RuleId", RuleId.SsCalc01)]
    [Trait("RuleId", RuleId.SsCalc02)]
    public void GenderSubtotal_IsSumOfStoredCountAndPercent()
    {
        Assert.Equal(100, BaselineSchoolReport.Women.Count + BaselineSchoolReport.Men.Count);
        Assert.Equal(100.0m, LegacyRules.SumStoredPercents(
            BaselineSchoolReport.Women.Percent,
            BaselineSchoolReport.Men.Percent));
    }

    [Fact]
    [Trait("RuleId", RuleId.SsCalc02)]
    public void D1_SubtotalPercent_Stays93_NotForcedTo100()
    {
        Assert.Equal(93.0m, BaselineSchoolReport.Employed.Percent);
        Assert.Equal(93.0m, LegacyRules.SumStoredPercents(BaselineSchoolReport.Employed.Percent));
    }

    [Fact]
    [Trait("RuleId", RuleId.SsSub01)]
    public void SectorSubtotal_CountAndPercent()
    {
        Assert.Equal(93, BaselineSchoolReport.PrivateSector.Count + BaselineSchoolReport.PublicSector.Count);
        Assert.Equal(100.0m, LegacyRules.SumStoredPercents(
            BaselineSchoolReport.PrivateSector.Percent,
            BaselineSchoolReport.PublicSector.Percent));
    }

    [Fact]
    [Trait("RuleId", RuleId.SsSub01)]
    public void SalaryColumns_AreNotPartOfTheSubtotalRule()
    {
        int?[] salaryCells =
        [
            BaselineSchoolReport.Women.SalaryN,
            BaselineSchoolReport.Women.Pct25,
            BaselineSchoolReport.Women.Median,
            BaselineSchoolReport.Women.Pct75,
            BaselineSchoolReport.Women.Mean,
        ];
        Assert.All(salaryCells, cell => Assert.NotNull(cell));
        Assert.Equal("Subtotal", LegacyRules.SubtotalLabel("B"));
    }

    [Fact]
    [Trait("RuleId", RuleId.SsCalc02)]
    public void TimingSubtotal_SumsStoredPercents_EvenWhenTheyAre100Point1()
    {
        Assert.Equal(100.1m, LegacyRules.SumStoredPercents(61.3m, 38.8m));
    }

    [Fact(Skip = "TODO SS-CALC-01: D3 displayed counts 79+10+2+1+1+1 = 94; printed subtotal is 93. Do not invent which value a modernizer should emit.")]
    [Trait("RuleId", RuleId.SsCalc01)]
    public void D3_PrintedSubtotal_Vs_DisplayedDetailSum()
    {
    }
}
