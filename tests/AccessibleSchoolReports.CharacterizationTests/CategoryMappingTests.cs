using AccessibleSchoolReports.CharacterizationTests.Support;

namespace AccessibleSchoolReports.CharacterizationTests;

public sealed class CategoryMappingTests
{
    [Theory]
    [InlineData("LJD", "1-LJD")]
    [InlineData("NLJD", "2-NLJD")]
    [InlineData("NLP", "3-NLP")]
    [InlineData("NLO", "4-NLO")]
    [InlineData("WUNK", "5-WUNK")]
    [InlineData("ADVD", "6-ADVD")]
    [InlineData("UDEF", "7-UDEF")]
    [InlineData("USKW", "8-USKW")]
    [InlineData("UNWK", "9-UNWK")]
    [InlineData("UNKN", "UNKN")]
    [InlineData("FULL", "Full-time")]
    [InlineData("PART", "Part-time")]
    [Trait("RuleId", RuleId.CfFmt03)]
    [Trait("RuleId", RuleId.CfPrep01)]
    public void Jobcat1_FormatsToPrefixedJobcat(string jobcat1, string jobcat)
    {
        Assert.Equal(jobcat, LegacyRules.JobcatFormat[jobcat1]);
    }

    [Theory]
    [InlineData("ADMIN", "YADMIN")]
    [InlineData("OTHNL", "ZOTHNL")]
    [InlineData("STATTY", "ATTYST")]
    [InlineData("ATTY", "ATTY")]
    [Trait("RuleId", RuleId.CfPrep02)]
    public void Lfjob_RecodesSortLikePrefixes(string input, string expected)
    {
        Assert.Equal(expected, LegacyRules.RecodeLfjob(input));
    }

    [Theory]
    [InlineData("0", "X")]
    [InlineData("1", "1")]
    [InlineData("X", "X")]
    [Trait("RuleId", RuleId.CfPrep03)]
    public void Jobreg_ZeroBecomesX(string input, string expected)
    {
        Assert.Equal(expected, LegacyRules.RecodeJobreg(input));
    }

    [Theory]
    [InlineData("OTHER", "ZOTHER")]
    [InlineData("OCI", "AOCI")]
    [InlineData("JOBPST", "JOBPST")]
    [Trait("RuleId", RuleId.CfPrep04)]
    public void Source_OtherAndOci_ArePrefixed(string input, string expected)
    {
        Assert.Equal(expected, LegacyRules.RecodeSource(input));
    }

    [Theory]
    [InlineData("JCLOGV", "JCTLOG")]
    [InlineData("JCINGV", "JCXIOG")]
    [InlineData("JCOTGV", "JCUGOV")]
    [InlineData("JCUNGV", "JCUGOV")]
    [InlineData("JC", "JCUGOV")]
    [InlineData("JCSTGV", "JCSTGV")]
    [Trait("RuleId", RuleId.CfPrep05)]
    public void Emptype1_CollapsesSelectedClerkshipCodes(string input, string expected)
    {
        Assert.Equal(expected, LegacyRules.RecodeEmptype1(input));
    }

    [Theory]
    [InlineData("S", "SOLO")]
    [InlineData("1", "LF1")]
    [InlineData("8", "LF8")]
    [Trait("RuleId", RuleId.CfC16)]
    public void FirmSize_CountMap_IncludesSolo(string firm1, string expected)
    {
        Assert.Equal(expected, LegacyRules.MapFirmSizeForCounts(firm1));
    }

    [Fact]
    [Trait("RuleId", RuleId.CfS15)]
    [Trait("RuleId", RuleId.CfAmb05)]
    public void FirmSize_SalaryMap_DoesNotMapSolo()
    {
        Assert.Null(LegacyRules.MapFirmSizeForSalaries("S"));
        Assert.Equal("LF1", LegacyRules.MapFirmSizeForSalaries("1"));
    }

    [Theory]
    [InlineData("B", "Gender Reported:")]
    [InlineData("D2", "Employment by Sector:")]
    [InlineData("E1", "Employment Categories:")]
    [InlineData("SOURCE", "Source of Job:")]
    [Trait("RuleId", RuleId.SsFmt02)]
    public void Analvar_MapsToSectionHeader(string analvar, string header)
    {
        Assert.Equal(header, LegacyRules.SectionHeaders[analvar]);
    }

    [Theory]
    [InlineData("F", "Women")]
    [InlineData("EMPL", "Employed")]
    [InlineData("AOCI", "Career office recruitment program (e.g., OCI)")]
    [Trait("RuleId", RuleId.SsFmt04)]
    public void Newvar_MapsToReportRowLabel(string newvar, string label)
    {
        Assert.Equal(label, LegacyRules.ReportRowLabels[newvar]);
    }

    [Fact]
    [Trait("RuleId", RuleId.CfDead01)]
    public void CommentedSourceCollapse_ToOther_IsNotApplied()
    {
        Assert.Equal("ONLINE", LegacyRules.RecodeSource("ONLINE"));
        Assert.Equal("TEMPAG", LegacyRules.RecodeSource("TEMPAG"));
    }

    [Fact(Skip = "TODO SS-FMT-04: builder stores minstat||sex3 as MINORF; report $newvar lists 'MINOR F'. Baseline PDF printed Women of Color, but the 2025 key is not proven.")]
    [Trait("RuleId", RuleId.SsFmt04)]
    [Trait("RuleId", RuleId.CfC04)]
    public void CrossTabKey_Minorf_Vs_MinorSpaceF()
    {
    }

    [Fact(Skip = "TODO SS-FMT-04 / CF-P2-04: builder recodes AFTGRD to ZAFTGRD; report $newvar lists ZAFTGR. PDF printed After graduation.")]
    [Trait("RuleId", RuleId.SsFmt04)]
    [Trait("RuleId", RuleId.CfP204)]
    public void AfterGraduationKey_Zaftgrd_Vs_Zaftgr()
    {
    }

    [Fact(Skip = "TODO CF-AMB-01: $jobcat1 is used on jobftpt and is never defined in createschrptfiles2025.sas.")]
    [Trait("RuleId", RuleId.CfAmb01)]
    public void Jobftpt_FormatJobcat1_Undefined()
    {
    }
}
