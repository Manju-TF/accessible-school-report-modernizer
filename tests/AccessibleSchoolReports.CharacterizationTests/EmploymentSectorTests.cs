using AccessibleSchoolReports.CharacterizationTests.Support;

namespace AccessibleSchoolReports.CharacterizationTests;

public sealed class EmploymentSectorTests
{
    [Theory]
    [InlineData("ACAD", "PUBLIC")]
    [InlineData("GOVT", "PUBLIC")]
    [InlineData("CLERK", "PUBLIC")]
    [InlineData("PUBINT", "PUBLIC")]
    [InlineData("BUS", "PRIVATE")]
    [InlineData("FIRM", "PRIVATE")]
    [Trait("RuleId", RuleId.CfC08)]
    public void Empgen_MapsToPublicOrPrivate(string empgen, string sector)
    {
        Assert.Equal(sector, LegacyRules.MapSector(empgen));
    }

    [Fact]
    [Trait("RuleId", RuleId.CfC08)]
    public void Empunk_IsDeletedFromSectorCounts_NotMapped()
    {
        Assert.True(LegacyRules.DeleteFromSectorCounts("EMPUNK"));
        Assert.Null(LegacyRules.MapSector("EMPUNK"));
    }

    [Fact]
    [Trait("RuleId", RuleId.CfC08)]
    public void UnlistedEmpgen_IsNotInventedAsASector()
    {
        Assert.Null(LegacyRules.MapSector("OTHER"));
    }

    [Fact]
    [Trait("RuleId", RuleId.CfC08)]
    [Trait("RuleId", RuleId.CfS06)]
    [Trait("RuleId", RuleId.CfS07)]
    public void BaselinePdf_SectorCounts()
    {
        Assert.Equal(78, BaselineSchoolReport.PrivateSector.Count);
        Assert.Equal(15, BaselineSchoolReport.PublicSector.Count);
        Assert.Equal(83.9m, BaselineSchoolReport.PrivateSector.Percent);
        Assert.Equal(16.1m, BaselineSchoolReport.PublicSector.Percent);
        Assert.Equal(93, BaselineSchoolReport.PrivateSector.Count + BaselineSchoolReport.PublicSector.Count);
    }
}
