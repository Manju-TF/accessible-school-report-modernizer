using AccessibleSchoolReports.Domain.Reporting;

namespace AccessibleSchoolReports.UnitTests.Reporting;

public sealed class SasUnivariateTests
{
    [Fact]
    [Trait("RuleId", "CF-S-00")]
    public void SuppressUnlessEligible_RequiresNGe5()
    {
        var four = SasUnivariate.Calculate([1m, 2m, 3m, 4m]);
        var five = SasUnivariate.Calculate([1m, 2m, 3m, 4m, 5m]);
        Assert.Null(SasUnivariate.SuppressUnlessEligible(four));
        Assert.NotNull(SasUnivariate.SuppressUnlessEligible(five));
    }

    [Fact]
    [Trait("RuleId", "CF-S-00")]
    public void Calculate_OmitsMissingSalariesFromN()
    {
        var stats = SasUnivariate.Calculate([10m, null, 20m, null, 30m, 40m, 50m]);
        Assert.Equal(5, stats!.N);
    }

    [Fact]
    [Trait("RuleId", "CF-S-00")]
    public void Mean_IsArithmeticMeanOfNonMissing()
    {
        var stats = SasUnivariate.Calculate([10m, 20m, 30m, 40m, 50m]);
        Assert.Equal(30m, stats!.Mean);
    }

    [Fact]
    [Trait("RuleId", "SS-SAL-03")]
    public void Median_Pctldef5_EvenN_AveragesPair()
    {
        // n=4, p=50, np=2 → (x2+x3)/2
        Assert.Equal(25m, SasUnivariate.PercentilePctldef5([10m, 20m, 30m, 40m], 50));
    }

    [Fact]
    [Trait("RuleId", "SS-SAL-03")]
    public void Median_Pctldef5_OddN_UsesCeil()
    {
        // n=5, p=50, np=2.5 → x3
        Assert.Equal(30m, SasUnivariate.PercentilePctldef5([10m, 20m, 30m, 40m, 50m], 50));
    }

    [Fact]
    [Trait("RuleId", "SS-SAL-02")]
    public void Quartile_Pctldef5_OddN()
    {
        var sorted = new decimal[] { 10, 20, 30, 40, 50 };
        Assert.Equal(20m, SasUnivariate.PercentilePctldef5(sorted, 25));
        Assert.Equal(40m, SasUnivariate.PercentilePctldef5(sorted, 75));
    }
}
