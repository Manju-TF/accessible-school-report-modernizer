using AccessibleSchoolReports.Application.Imports;

namespace AccessibleSchoolReports.UnitTests.Imports;

public sealed class ExcelHeaderNormalizerTests
{
    [Theory]
    [InlineData("salftperm", "salftperm")]
    [InlineData("Sal Ft Perm", "salftperm")]
    [InlineData("sal_ft_perm", "salftperm")]
    [InlineData("SAL-FT-PERM", "salftperm")]
    [InlineData("  Code  ", "code")]
    [InlineData("locationflag", "locationflag")]
    [InlineData("Location Flag", "locationflag")]
    public void Normalize_MapsDisplayHeadersToCanonicalNames(string header, string expected)
    {
        Assert.Equal(expected, ExcelHeaderNormalizer.Normalize(header));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("---")]
    public void Normalize_EmptyOrPunctuation_IsEmpty(string? header)
    {
        Assert.Equal(string.Empty, ExcelHeaderNormalizer.Normalize(header));
    }
}
