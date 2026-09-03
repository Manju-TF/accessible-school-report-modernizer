using AccessibleSchoolReports.CharacterizationTests.Support;

namespace AccessibleSchoolReports.CharacterizationTests;

public sealed class JobCategoryTests
{
    [Fact]
    [Trait("RuleId", RuleId.SsFmt04)]
    public void ReportLabels_ForFormattedJobcats()
    {
        Assert.Equal("Bar Admission Required/ Anticipated", LegacyRules.ReportRowLabels["1-LJD"]);
        Assert.Equal("JD Advantage", LegacyRules.ReportRowLabels["2-NLJD"]);
        Assert.Equal("Other Professional", LegacyRules.ReportRowLabels["3-NLP"]);
        Assert.Equal("Other Position", LegacyRules.ReportRowLabels["4-NLO"]);
        Assert.Equal("Job Type Unknown", LegacyRules.ReportRowLabels["5-WUNK"]);
        Assert.Equal("Enrolled in Graduate Studies", LegacyRules.ReportRowLabels["6-ADVD"]);
        Assert.Equal("Not Employed-Seeking", LegacyRules.ReportRowLabels["8-USKW"]);
        Assert.Equal("Not Employed-Not Seeking", LegacyRules.ReportRowLabels["9-UNWK"]);
    }

    [Fact]
    [Trait("RuleId", RuleId.CfS04)]
    public void SalaryJobcats_AreOnlyTheFourEmployedTypes()
    {
        Assert.Equal(["1-LJD", "2-NLJD", "3-NLP", "4-NLO"], LegacyRules.SalaryJobcats);
    }

    [Fact]
    [Trait("RuleId", RuleId.CfC11)]
    public void BaselinePdf_BusinessJobs_UseJobcatRows()
    {
        Assert.Equal("Business Jobs:", LegacyRules.SectionHeaders["E3"]);
        Assert.Equal(14, BaselineSchoolReport.Business.Count);
    }

    [Fact]
    [Trait("RuleId", RuleId.SsFil08)]
    public void BaselinePdf_OmitsUnknownAndDeferredJobcats()
    {
        Assert.DoesNotContain("Job Type Unknown", new[]
        {
            BaselineSchoolReport.Ljd.Label,
            BaselineSchoolReport.JdAdvantage.Label,
            BaselineSchoolReport.OtherProfessional.Label,
            BaselineSchoolReport.OtherPosition.Label,
            BaselineSchoolReport.NotEmployedSeeking.Label,
            BaselineSchoolReport.NotEmployedNotSeeking.Label,
        });
    }
}
