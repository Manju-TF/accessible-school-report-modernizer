using AccessibleSchoolReports.CharacterizationTests.Support;

namespace AccessibleSchoolReports.CharacterizationTests;

public sealed class SalaryMeanTests
{
    [Fact]
    [Trait("RuleId", RuleId.SsSal05)]
    public void BaselinePdf_DisplayedMeans()
    {
        Assert.Equal(85802, BaselineSchoolReport.Women.Mean);
        Assert.Equal(103401, BaselineSchoolReport.Men.Mean);
        Assert.Equal(95236, BaselineSchoolReport.Employed.Mean);
        Assert.Equal(101425, BaselineSchoolReport.PrivateSector.Mean);
        Assert.Equal(78532, BaselineSchoolReport.PublicSector.Mean);
        Assert.Equal(120500, BaselineSchoolReport.Business.Mean);
    }

    [Fact]
    [Trait("RuleId", RuleId.SsSal05)]
    [Trait("RuleId", RuleId.SsSub01)]
    public void Mean_IsNotSubtotaled_OnTheReport()
    {
        Assert.Null(BaselineSchoolReport.MenOfColor.Mean);
        Assert.NotNull(BaselineSchoolReport.Women.Mean);
        Assert.NotNull(BaselineSchoolReport.Men.Mean);
    }

    [Fact(Skip = "TODO SS-SAL-05: mean is PROC UNIVARIATE mean of salftperm. Raw salaries for Test University are not in the repository.")]
    [Trait("RuleId", RuleId.SsSal05)]
    public void Mean_FromRawSalaries()
    {
    }
}
