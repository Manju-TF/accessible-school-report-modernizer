using AccessibleSchoolReports.Domain.Recodes;

namespace AccessibleSchoolReports.UnitTests.Recodes;

public sealed class LegacySchoolNamesTests
{
    [Fact]
    [Trait("RuleId", "SS-HDR-01")]
    [Trait("RuleId", "SS-PREP-03")]
    public void Lookup_UsesLiveSchrptsName()
    {
        Assert.Equal("Quinnipiac University School of Law", LegacySchoolNames.Lookup("10701"));
        Assert.Equal("Hofstra University Maurice A. Deane School of Law", LegacySchoolNames.Lookup("23306"));
        Assert.Equal("Yale Law School", LegacySchoolNames.Lookup("10703"));
    }

    [Fact]
    [Trait("RuleId", "SS-OUT-02")]
    public void Lookup_OmitsCommentedSchrptsCodes()
    {
        Assert.Null(LegacySchoolNames.Lookup("23101"));
        Assert.Null(LegacySchoolNames.Lookup("23909"));
        Assert.Null(LegacySchoolNames.Lookup("31504"));
        Assert.Null(LegacySchoolNames.Lookup("42603"));
        Assert.Null(LegacySchoolNames.Lookup("53404"));
        Assert.Null(LegacySchoolNames.Lookup("54703"));
        Assert.Null(LegacySchoolNames.Lookup("90506"));
    }

    [Fact]
    [Trait("RuleId", "SS-HDR-01")]
    public void Resolve_PrefersSasNameOverStoredName()
    {
        Assert.Equal(
            "Quinnipiac University School of Law",
            LegacySchoolNames.Resolve("10701", "Sample Law School"));
    }

    [Fact]
    [Trait("RuleId", "SS-HDR-01")]
    public void Resolve_UsesStoredNameWhenCodeIsNotInSchrpts()
    {
        Assert.Equal("Test University School of Law", LegacySchoolNames.Resolve("99999", "Test University School of Law"));
        Assert.Equal("99999", LegacySchoolNames.Resolve("99999", storedName: null));
    }

    [Fact]
    [Trait("RuleId", "SS-PREP-03")]
    public void All_ContainsActiveSchrptsSchoolsOnly()
    {
        Assert.Equal(192, LegacySchoolNames.All.Count);
        Assert.DoesNotContain("23101", LegacySchoolNames.All.Keys);
    }
}
