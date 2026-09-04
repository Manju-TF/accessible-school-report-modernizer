using AccessibleSchoolReports.Domain.Persistence;
using AccessibleSchoolReports.Web.Ui;

namespace AccessibleSchoolReports.UnitTests.Ui;

public sealed class UiFormatTests
{
    [Fact]
    public void SchoolLabel_UsesSingularGraduate()
    {
        Assert.Equal(
            "99999 (1 graduate)",
            UiFormat.SchoolLabel("99999", name: null, graduateCount: 1));
    }

    [Fact]
    public void SchoolLabel_UsesPluralGraduates()
    {
        Assert.Equal("99999 — Sample (12 graduates)", UiFormat.SchoolLabel("99999", "Sample", 12));
    }

    [Fact]
    [Trait("RuleId", "SS-HDR-01")]
    public void SchoolLabel_UsesStoredSchoolName()
    {
        Assert.Equal(
            "23306 — Hofstra University Maurice A. Deane School of Law (31 graduates)",
            UiFormat.SchoolLabel(
                "23306",
                "Hofstra University Maurice A. Deane School of Law",
                graduateCount: 31));
    }

    [Fact]
    public void StatusAccessibleName_PrefixesStatus()
    {
        Assert.Equal("Status: Completed", UiFormat.StatusAccessibleName(RunStatus.Completed));
        Assert.Equal("Status: Completed with errors", UiFormat.StatusAccessibleName(RunStatus.CompletedWithErrors));
    }
}
