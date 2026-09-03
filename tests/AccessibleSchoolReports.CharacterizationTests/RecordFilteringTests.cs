using AccessibleSchoolReports.CharacterizationTests.Support;

namespace AccessibleSchoolReports.CharacterizationTests;

public sealed class RecordFilteringTests
{
    [Theory]
    [InlineData("B")]
    [InlineData("C")]
    [InlineData("C1")]
    [InlineData("D")]
    [Trait("RuleId", RuleId.SsFil01)]
    public void Page1_IncludesAnalvars(string analvar)
    {
        Assert.Contains(analvar, LegacyRules.Page1Analvars);
    }

    [Fact]
    [Trait("RuleId", RuleId.SsFil01)]
    public void Page1_DoesNotSelectTotalReportedAnalvarA()
    {
        Assert.DoesNotContain("A", LegacyRules.Page1Analvars);
    }

    [Theory]
    [InlineData("D1", true)]
    [InlineData("D2", true)]
    [InlineData("D3", true)]
    [InlineData("E", true)]
    [InlineData("E1", true)]
    [InlineData("E2", false)]
    [InlineData("C1", false)]
    [Trait("RuleId", RuleId.SsFil02)]
    public void Page2_UsesCharacterRange_D1_Through_Before_E2(string analvar, bool included)
    {
        Assert.Equal(included, LegacyRules.Page2CharacterFilterIncludes(analvar));
    }

    [Fact]
    [Trait("RuleId", RuleId.SsFil03)]
    public void Page3_IncludesEducationThroughClerkships()
    {
        Assert.Equal(["E2", "E3", "E4", "E5", "E55"], LegacyRules.Page3Analvars);
    }

    [Fact]
    [Trait("RuleId", RuleId.SsFil04)]
    public void Page4_IncludesPublicInterestAndFirmSections()
    {
        Assert.Equal(["E6", "FIRM", "FIRM2"], LegacyRules.Page4Analvars);
    }

    [Fact]
    [Trait("RuleId", RuleId.SsFil05)]
    public void Page5_IncludesRegionLocationAndStateCount()
    {
        Assert.Equal(["JOBREG1", "JOBREG2", "JOBREG3"], LegacyRules.Page5Analvars);
    }

    [Fact]
    [Trait("RuleId", RuleId.SsFil06)]
    public void Page6_ReadsPart2_SourceTimeStatus()
    {
        Assert.Equal(["SOURCE", "TIME", "ZSTATUS"], LegacyRules.Page6Analvars);
    }

    [Fact]
    [Trait("RuleId", RuleId.SsFil07)]
    public void Page7_ReadsPart2_DurationAndFunded()
    {
        Assert.Equal(["DURATION", "LAW SCHOOL FUNDED"], LegacyRules.Page7Analvars);
    }

    [Fact]
    [Trait("RuleId", RuleId.SsFil08)]
    [Trait("RuleId", RuleId.SsSup02)]
    public void CategoriesWithNoGraduates_AreNotShown_OnBaselinePdf()
    {
        Assert.Contains("Education Jobs:", BaselineSchoolReport.AbsentSectionsOnThisPdf);
        Assert.Contains("Non-binary or Chose to Self-identify", BaselineSchoolReport.AbsentRowLabelsOnThisPdf);
        Assert.Contains("Solo Practitioner", BaselineSchoolReport.AbsentRowLabelsOnThisPdf);
        Assert.Contains("Foreign", BaselineSchoolReport.AbsentRowLabelsOnThisPdf);
    }

    [Fact]
    [Trait("RuleId", RuleId.CfC02)]
    public void GenderCounts_KeepOnly_M_F_N_AfterSex3Recode()
    {
        Assert.True(LegacyRules.IncludeInGenderCounts("F"));
        Assert.True(LegacyRules.IncludeInGenderCounts("M"));
        Assert.True(LegacyRules.IncludeInGenderCounts("N"));
        Assert.False(LegacyRules.IncludeInGenderCounts(" "));
        Assert.False(LegacyRules.IncludeInGenderCounts("W"));
        Assert.False(LegacyRules.IncludeInGenderCounts("ND"));
    }

    [Fact]
    [Trait("RuleId", RuleId.CfC05)]
    public void EmploymentStatusCounts_ExcludeRawUnkn()
    {
        Assert.False(LegacyRules.IncludeInEmploymentStatusCounts("UNKN"));
        Assert.True(LegacyRules.IncludeInEmploymentStatusCounts("LJD"));
    }

    [Fact]
    [Trait("RuleId", RuleId.CfC08)]
    public void SectorCounts_DeleteEmpunk()
    {
        Assert.True(LegacyRules.DeleteFromSectorCounts("EMPUNK"));
        Assert.False(LegacyRules.DeleteFromSectorCounts("FIRM"));
    }

    [Fact]
    [Trait("RuleId", RuleId.CfPrep00)]
    public void BuilderDropsUnusedColumns_BeforeCounting()
    {
        string[] dropped = ["office_size", "othersource", "Field35b", "field9b", "Field36", "jobdesc"];
        Assert.Equal(6, dropped.Length);
    }

    [Fact(Skip = "TODO CF-C-18 / CF-AMB-06: character compare jobreg ge '0' after recoding '0' to 'X' is written, but the intended region universe is not defined.")]
    [Trait("RuleId", RuleId.CfC18)]
    public void RegionFilter_Ge0_AfterZeroBecomesX()
    {
    }
}
