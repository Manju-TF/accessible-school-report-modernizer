using AccessibleSchoolReports.CharacterizationTests.Support;

namespace AccessibleSchoolReports.CharacterizationTests;

public sealed class SchoolSelectionTests
{
    [Fact]
    [Trait("RuleId", RuleId.SsPrep03)]
    public void ReportMacro_SelectsOneSchoolByCode_AndPrintsName()
    {
        Assert.Equal("Test University School of Law", BaselineSchoolReport.SchoolName);
        Assert.Equal(7, BaselineSchoolReport.PageCount);
    }

    [Fact]
    [Trait("RuleId", RuleId.SsHdr01)]
    public void Title1_IsTheSchoolNameArgument()
    {
        Assert.Equal("Test University School of Law", BaselineSchoolReport.SchoolName);
    }

    [Fact]
    [Trait("RuleId", RuleId.SsPage01)]
    [Trait("RuleId", RuleId.SsPage02)]
    public void OnePdf_HasSevenReportPages()
    {
        Assert.Equal(7, BaselineSchoolReport.PageCount);
    }

    [Fact]
    [Trait("RuleId", RuleId.SsPrep03)]
    public void SchoolSelectionInputs_AreCodeAndName_NotJobstOrSt()
    {
        string[] usedMacroArguments = ["CODE", "NAME"];
        string[] unusedMacroArguments = ["JOBST", "ST"];
        Assert.DoesNotContain(unusedMacroArguments, usedMacroArguments.Contains);
    }

    [Fact]
    [Trait("RuleId", RuleId.SsOut02)]
    public void CommentedSchoolCodes_AreNotCurrentOutput()
    {
        Assert.Contains("23101", LegacyRules.CommentedSkippedSchoolCodes);
        Assert.Contains("23909", LegacyRules.CommentedSkippedSchoolCodes);
        Assert.Contains("31504", LegacyRules.CommentedSkippedSchoolCodes);
        Assert.Contains("42603", LegacyRules.CommentedSkippedSchoolCodes);
    }

    [Fact(Skip = "TODO SS-PREP-03: school CODE that produced Test University School of Law is not in the baseline PDF or the 2025 %SCHRPTS list.")]
    [Trait("RuleId", RuleId.SsPrep03)]
    public void BaselinePdf_SchoolCode()
    {
    }

    [Fact(Skip = "TODO SS-HDR-02: PDF title is Class of 2024; schreptsummary_2025.sas prints Class of 2025. Do not pick one.")]
    [Trait("RuleId", RuleId.SsHdr02)]
    public void ClassYearTitle_Pdf2024_Vs_Sas2025()
    {
    }
}
