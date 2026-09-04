using AccessibleSchoolReports.Domain.Persistence;
using AccessibleSchoolReports.Web.Ui;

namespace AccessibleSchoolReports.UnitTests.Ui;

public sealed class UiFormatTests
{
    [Fact]
    public void SchoolLabel_UsesSingularGraduate()
    {
        Assert.Equal("51012 (1 graduate)", UiFormat.SchoolLabel("51012", name: null, graduateCount: 1));
    }

    [Fact]
    public void SchoolLabel_UsesPluralGraduates()
    {
        Assert.Equal("10701 — Sample (12 graduates)", UiFormat.SchoolLabel("10701", "Sample", 12));
    }

    [Fact]
    public void StatusAccessibleName_PrefixesStatus()
    {
        Assert.Equal("Status: Completed", UiFormat.StatusAccessibleName(RunStatus.Completed));
        Assert.Equal("Status: Completed with errors", UiFormat.StatusAccessibleName(RunStatus.CompletedWithErrors));
    }
}
