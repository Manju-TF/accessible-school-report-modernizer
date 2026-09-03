using AccessibleSchoolReports.CharacterizationTests.Support;

namespace AccessibleSchoolReports.CharacterizationTests;

public sealed class SalaryPopulationTests
{
    [Fact]
    [Trait("RuleId", RuleId.CfS01)]
    public void SalaryVariable_IsSalftperm_OnEveryUnivariate()
    {
        Assert.Equal("salftperm", LegacyRules.SalaryVariable);
    }

    [Fact]
    [Trait("RuleId", RuleId.CfS01)]
    public void GenderSalaries_UseSameSex3FilterAsCounts()
    {
        Assert.Equal(["M", "F", "N"], LegacyRules.GenderCountValues);
    }

    [Fact]
    [Trait("RuleId", RuleId.CfS04)]
    public void JobcatSalaries_IncludeOnlyFourEmployedCategories()
    {
        Assert.DoesNotContain("5-WUNK", LegacyRules.SalaryJobcats);
        Assert.DoesNotContain("8-USKW", LegacyRules.SalaryJobcats);
        Assert.DoesNotContain("9-UNWK", LegacyRules.SalaryJobcats);
    }

    [Fact]
    [Trait("RuleId", RuleId.CfS06)]
    [Trait("RuleId", RuleId.CfS07)]
    public void SectorSalaries_UseSamePublicPrivateEmpgenLists()
    {
        Assert.Equal(["BUS", "FIRM"], LegacyRules.PrivateEmpgen);
        Assert.Equal(["ACAD", "GOVT", "CLERK", "PUBINT"], LegacyRules.PublicEmpgen);
    }

    [Fact]
    [Trait("RuleId", RuleId.CfS05)]
    [Trait("RuleId", RuleId.CfAmb03)]
    public void EmployedSalaryStep_IsByCodeOnly_LabeledEmpl()
    {
        Assert.Equal("Employed", LegacyRules.ReportRowLabels["EMPL"]);
        Assert.Equal("D1", BaselineSchoolReport.Employed.Analvar);
    }

    [Fact]
    [Trait("RuleId", RuleId.SsSal07)]
    public void FullTimeLongTermAndSoloSentences_AreReportNotes_NotRendererFilters()
    {
        Assert.Equal("Full-time Long-term Salaries", BaselineSchoolReport.SalarySpanningHeader);
    }

    [Fact(Skip = "TODO CF-AMB-07 / SS-SAL-07: salftperm has no extra FT/long-term WHERE in the builder. Do not add filters from the PDF note.")]
    [Trait("RuleId", RuleId.SsSal07)]
    public void SalaryPopulation_AdditionalFtLongTermFilter()
    {
    }

    [Fact(Skip = "TODO CF-AMB-05 / SS-SUP-03: count maps firm1 S to SOLO; salary step does not. Solo exclusion mechanism is not settled.")]
    [Trait("RuleId", RuleId.SsSup03)]
    [Trait("RuleId", RuleId.CfAmb05)]
    public void SoloSalary_ExclusionMechanism()
    {
    }

    [Fact(Skip = "TODO CF-S-05: step title says ft jobs but the SAS has no FT filter. Do not invent the population.")]
    [Trait("RuleId", RuleId.CfS05)]
    public void EmployedSalary_FtFilterPresence()
    {
    }
}
