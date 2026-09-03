using AccessibleSchoolReports.CharacterizationTests.Support;

namespace AccessibleSchoolReports.CharacterizationTests;

public sealed class JobSourceTests
{
    [Theory]
    [InlineData("OCI", "Career office recruitment program (e.g., OCI)")]
    [InlineData("JOBFRC", "Job fair or career conference")]
    [InlineData("SELFPR", "Started own business/practice")]
    [InlineData("TEMPAG", "Temp agency or legal search consultant")]
    [Trait("RuleId", RuleId.CfFmt02)]
    public void BuilderSourceFormat_UsesListedLabels(string code, string label)
    {
        Assert.Equal(label, LegacyRules.BuilderSourceFormat[code]);
    }

    [Fact]
    [Trait("RuleId", RuleId.CfP203)]
    [Trait("RuleId", RuleId.CfPrep04)]
    public void StoredSourceAfterRecode_UsesAociAndZother()
    {
        Assert.Equal("AOCI", LegacyRules.RecodeSource("OCI"));
        Assert.Equal("ZOTHER", LegacyRules.RecodeSource("OTHER"));
        Assert.False(LegacyRules.BuilderSourceFormat.ContainsKey("AOCI"));
        Assert.False(LegacyRules.BuilderSourceFormat.ContainsKey("ZOTHER"));
        Assert.Equal("Career office recruitment program (e.g., OCI)", LegacyRules.ReportRowLabels["AOCI"]);
        Assert.Equal("Other", LegacyRules.ReportRowLabels["ZOTHER"]);
    }

    [Fact]
    [Trait("RuleId", RuleId.CfP203)]
    public void BaselinePdf_OciSourceCount()
    {
        Assert.Equal(4, BaselineSchoolReport.Oci.Count);
        Assert.Equal(4.4m, BaselineSchoolReport.Oci.Percent);
        Assert.Equal("SOURCE", BaselineSchoolReport.Oci.Analvar);
    }

    [Fact]
    [Trait("RuleId", RuleId.CfDead01)]
    public void OnlineAndTempAgency_AreNotCollapsedToOther()
    {
        Assert.Equal("ONLINE", LegacyRules.RecodeSource("ONLINE"));
        Assert.Equal("TEMPAG", LegacyRules.RecodeSource("TEMPAG"));
    }
}
