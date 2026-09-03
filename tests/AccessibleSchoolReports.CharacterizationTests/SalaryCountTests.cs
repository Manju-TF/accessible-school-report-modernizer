using AccessibleSchoolReports.CharacterizationTests.Support;

namespace AccessibleSchoolReports.CharacterizationTests;

public sealed class SalaryCountTests
{
    [Fact]
    [Trait("RuleId", RuleId.SsSal01)]
    public void SalaryN_IsDisplayed_NotSubtotaled()
    {
        Assert.Equal(40, BaselineSchoolReport.Women.SalaryN);
        Assert.Equal(30, BaselineSchoolReport.Men.SalaryN);
        Assert.Null(BaselineSchoolReport.MenOfColor.SalaryN);
    }

    [Fact]
    [Trait("RuleId", RuleId.CfC01)]
    [Trait("RuleId", RuleId.SsPrep02)]
    public void TotalReportedBanner_Is100()
    {
        Assert.Equal(100, BaselineSchoolReport.TotalReported);
    }

    [Fact]
    [Trait("RuleId", RuleId.SsSal01)]
    public void BaselinePdf_EmployedSalaryN_Is83()
    {
        Assert.Equal(83, BaselineSchoolReport.Employed.SalaryN);
        Assert.Equal(93, BaselineSchoolReport.Employed.Count);
    }

    [Fact]
    [Trait("RuleId", RuleId.CfS19)]
    public void BaselinePdf_JdAdvantageFullTime_SalaryN_ExceedsCount()
    {
        Assert.Equal(10, BaselineSchoolReport.JdAdvantageFullTime.Count);
        Assert.Equal(12, BaselineSchoolReport.JdAdvantageFullTime.SalaryN);
    }

    [Fact(Skip = "TODO CF-S-19: JD Advantage Full-time # with Salary is 12 while Number Reported is 10. Do not invent a join or filter that 'fixes' this.")]
    [Trait("RuleId", RuleId.CfS19)]
    public void D3_SalaryN_Vs_Count_Reconciliation()
    {
    }
}
