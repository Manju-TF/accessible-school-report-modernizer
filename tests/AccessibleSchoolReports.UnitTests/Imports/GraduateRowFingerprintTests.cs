using AccessibleSchoolReports.Application.Imports;

namespace AccessibleSchoolReports.UnitTests.Imports;

public sealed class GraduateRowFingerprintTests
{
    [Fact]
    public void Compute_IsStableForEquivalentSalaries()
    {
        var left = GraduateRowFingerprint.Compute("10701", 2025, "F", "NONMIN", "LJD", "FULL", "FIRM", "1", "ATTY", "1", "INSTATE", "107", "JOBPST", "BGRAD", "SET", "PERM", "NO", 85000m, null);
        var right = GraduateRowFingerprint.Compute("10701", 2025, "F", "NONMIN", "LJD", "FULL", "FIRM", "1", "ATTY", "1", "INSTATE", "107", "JOBPST", "BGRAD", "SET", "PERM", "NO", 85000.00m, null);

        Assert.Equal(left, right);
    }

    [Fact]
    public void ComputeSet_IgnoresRowOrder()
    {
        var a = GraduateRowFingerprint.Compute("10701", 2023, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null);
        var b = GraduateRowFingerprint.Compute("10702", 2024, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null);

        Assert.Equal(
            GraduateRowFingerprint.ComputeSet([a, b]),
            GraduateRowFingerprint.ComputeSet([b, a]));
    }
}
