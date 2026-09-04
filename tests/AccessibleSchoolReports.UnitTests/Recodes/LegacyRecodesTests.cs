using AccessibleSchoolReports.Application.Imports;
using AccessibleSchoolReports.Domain.Entities;
using AccessibleSchoolReports.Domain.Recodes;

namespace AccessibleSchoolReports.UnitTests.Recodes;

public sealed class LegacyRecodesTests
{
    [Fact]
    [Trait("RuleId", "CF-PREP-00")]
    public void DroppedSourceColumns_MatchSasDropList()
    {
        Assert.Equal(
            ["office_size", "othersource", "Field35b", "field9b", "Field36", "jobdesc"],
            LegacyRecodes.DroppedSourceColumns);
    }

    [Theory]
    [InlineData("office_size")]
    [InlineData("othersource")]
    [InlineData("Field35b")]
    [InlineData("field9b")]
    [InlineData("Field36")]
    [InlineData("jobdesc")]
    [Trait("RuleId", "CF-PREP-00")]
    public void IsDroppedSourceColumn_ConfirmedDrops(string column)
    {
        Assert.True(LegacyRecodes.IsDroppedSourceColumn(column));
    }

    [Fact]
    [Trait("RuleId", "CF-PREP-00")]
    public void DroppedSourceColumns_AreNotPersistedOnGraduateRecord()
    {
        var persisted = typeof(GraduateRecord)
            .GetProperties()
            .Select(property => property.Name)
            .ToHashSet(StringComparer.OrdinalIgnoreCase);
        var imported = GraduateImportColumns.Required
            .Concat(GraduateImportColumns.Optional)
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        foreach (var column in LegacyRecodes.DroppedSourceColumns)
        {
            Assert.False(persisted.Contains(column), column);
            Assert.False(imported.Contains(column), column);
        }
    }

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
    [Trait("RuleId", "CF-PREP-01")]
    [Trait("RuleId", "CF-FMT-03")]
    public void NormalizeJobCategory_FormatsKnownCodes(string jobcat1, string expected)
    {
        Assert.Equal(expected, LegacyRecodes.NormalizeJobCategory(jobcat1));
    }

    [Fact]
    [Trait("RuleId", "CF-PREP-01")]
    public void NormalizeJobCategory_UnknownValue_StaysRaw()
    {
        Assert.Equal("NOTAJOBCAT", LegacyRecodes.NormalizeJobCategory("NOTAJOBCAT"));
        Assert.Null(LegacyRecodes.NormalizeJobCategory(null));
    }

    [Fact(Skip = "TODO CF-AMB-01: $jobcat1 is used on jobftpt and is never defined in createschrptfiles2025.sas. Do not assume Full-time/Part-time labels.")]
    [Trait("RuleId", "CF-AMB-01")]
    public void NormalizeJobFtPt_UndefinedJobcat1Format()
    {
    }

    [Theory]
    [InlineData("ADMIN", "YADMIN")]
    [InlineData("OTHNL", "ZOTHNL")]
    [InlineData("STATTY", "ATTYST")]
    [InlineData("ATTY", "ATTY")]
    [InlineData("LCLERK", "LCLERK")]
    [Trait("RuleId", "CF-PREP-02")]
    public void NormalizeLawFirmJobType_RecodesListedCodesOnly(string input, string expected)
    {
        Assert.Equal(expected, LegacyRecodes.NormalizeLawFirmJobType(input));
    }

    [Theory]
    [InlineData("0", "X")]
    [InlineData("1", "1")]
    [InlineData("X", "X")]
    [InlineData("00", "00")]
    [InlineData(" 0", " 0")]
    [Trait("RuleId", "CF-PREP-03")]
    public void NormalizeJobRegion_ExactCharacterZeroOnly(string input, string expected)
    {
        Assert.Equal(expected, LegacyRecodes.NormalizeJobRegion(input));
    }

    [Theory]
    [InlineData("OTHER", "ZOTHER")]
    [InlineData("OCI", "AOCI")]
    [InlineData("JOBPST", "JOBPST")]
    [InlineData("SLFINI", "SLFINI")]
    [Trait("RuleId", "CF-PREP-04")]
    public void NormalizeJobSource_PrefixesOtherAndOci(string input, string expected)
    {
        Assert.Equal(expected, LegacyRecodes.NormalizeJobSource(input));
    }

    [Fact]
    [Trait("RuleId", "CF-DEAD-01")]
    public void NormalizeJobSource_DoesNotApplyCommentedCollapseToOther()
    {
        Assert.Equal("ONLINE", LegacyRecodes.NormalizeJobSource("ONLINE"));
        Assert.Equal("TEMPAG", LegacyRecodes.NormalizeJobSource("TEMPAG"));
        Assert.Equal("SOCI", LegacyRecodes.NormalizeJobSource("SOCI"));
    }

    [Theory]
    [InlineData("JCLOGV", "JCTLOG")]
    [InlineData("JCINGV", "JCXIOG")]
    [InlineData("JCOTGV", "JCUGOV")]
    [InlineData("JCUNGV", "JCUGOV")]
    [InlineData("JC", "JCUGOV")]
    [InlineData("JCSTGV", "JCSTGV")]
    [Trait("RuleId", "CF-PREP-05")]
    public void NormalizeEmploymentType_CollapsesListedCodesOnly(string input, string expected)
    {
        Assert.Equal(expected, LegacyRecodes.NormalizeEmploymentType(input));
    }

    [Theory]
    [InlineData("W", "F")]
    [InlineData("X", "N")]
    [InlineData("ND", " ")]
    [InlineData("F", "F")]
    [InlineData("M", "M")]
    [InlineData("N", "N")]
    [Trait("RuleId", "CF-PREP-06")]
    public void NormalizeGender_RecodesSex3(string input, string expected)
    {
        Assert.Equal(expected, LegacyRecodes.NormalizeGender(input));
    }

    [Fact]
    [Trait("RuleId", "CF-DEAD-02")]
    public void NormalizeGender_DoesNotApplyCommentedSexRecodes()
    {
        Assert.Equal("TW", LegacyRecodes.NormalizeGender("TW"));
        Assert.Equal("TM", LegacyRecodes.NormalizeGender("TM"));
    }

    [Fact]
    [Trait("RuleId", "CF-PREP-06")]
    public void NormalizeGender_NullStaysNull()
    {
        Assert.Null(LegacyRecodes.NormalizeGender(null));
    }
}
