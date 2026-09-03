using AccessibleSchoolReports.CharacterizationTests.Support;

namespace AccessibleSchoolReports.CharacterizationTests;

public sealed class SalarySuppressionTests
{
    [Theory]
    [InlineData(5, true)]
    [InlineData(6, true)]
    [InlineData(4, false)]
    [InlineData(0, false)]
    [Trait("RuleId", RuleId.CfS00)]
    public void SalaryRow_KeptOnlyWhenNGe5(int n, bool keep)
    {
        Assert.Equal(keep, LegacyRules.KeepSalaryRow(n));
    }

    [Fact]
    [Trait("RuleId", RuleId.SsSup01)]
    public void ReportNote_StatesTheSameFiveSalaryRule_NotASecondThreshold()
    {
        Assert.Equal(5, LegacyRules.SalarySuppressionMinimumN);
    }

    [Fact]
    [Trait("RuleId", RuleId.CfS00)]
    public void BaselinePdf_MenOfColor_Count4_HasNoSalaryCells()
    {
        Assert.Equal(4, BaselineSchoolReport.MenOfColor.Count);
        Assert.False(BaselineSchoolReport.MenOfColor.HasDisplayedSalary);
    }

    [Fact]
    [Trait("RuleId", RuleId.CfS00)]
    public void BaselinePdf_Mountain_Count5_HasNoSalaryCells()
    {
        Assert.Equal(5, BaselineSchoolReport.Mountain.Count);
        Assert.False(BaselineSchoolReport.Mountain.HasDisplayedSalary);
    }

    [Fact]
    [Trait("RuleId", RuleId.CfS00)]
    public void BaselinePdf_Clerkships_Count5_HasNoSalaryCells()
    {
        Assert.Equal(5, BaselineSchoolReport.Clerkships.Count);
        Assert.False(BaselineSchoolReport.Clerkships.HasDisplayedSalary);
    }

    [Fact]
    [Trait("RuleId", RuleId.CfS00)]
    public void BaselinePdf_PublicInterestLjd_SalaryN5_IsDisplayed()
    {
        Assert.True(BaselineSchoolReport.PublicInterest.HasDisplayedSalary);
        Assert.Equal(6, BaselineSchoolReport.PublicInterest.SalaryN);
        Assert.True(LegacyRules.KeepSalaryRow(BaselineSchoolReport.PublicInterest.SalaryN!.Value));
    }

    [Fact]
    [Trait("RuleId", RuleId.CfS00)]
    public void Suppression_UsesSalaryN_NotHeadcount()
    {
        Assert.True(LegacyRules.KeepSalaryRow(5));
        Assert.False(BaselineSchoolReport.Mountain.HasDisplayedSalary);
        Assert.Equal(5, BaselineSchoolReport.Mountain.Count);
    }
}
