using AccessibleSchoolReports.Domain.Recodes;

namespace AccessibleSchoolReports.Domain.Reporting;

/// <summary>
/// SAS <c>PROC UNIVARIATE</c> defaults used by CF-S-*: missing <c>salftperm</c>
/// omitted; Q1/median/Q3 use default <c>PCTLDEF=5</c> (not set in the SAS file).
/// </summary>
public static class SasUnivariate
{
    public static SalaryStatistics? Calculate(IEnumerable<decimal?> salaries)
    {
        var values = salaries.Where(value => value.HasValue).Select(value => value!.Value).OrderBy(value => value).ToArray();
        if (values.Length == 0)
        {
            return null;
        }

        var n = values.Length;
        var mean = values.Sum() / n;
        return new SalaryStatistics(
            n,
            mean,
            PercentilePctldef5(values, 25),
            PercentilePctldef5(values, 50),
            PercentilePctldef5(values, 75));
    }

    /// <summary>CF-S-00: keep salary statistics only when <c>n ge 5</c>.</summary>
    public static SalaryStatistics? SuppressUnlessEligible(SalaryStatistics? statistics) =>
        statistics is not null && statistics.N >= LegacyRecodes.SalarySuppressionMinimumN
            ? statistics
            : null;

    internal static decimal PercentilePctldef5(IReadOnlyList<decimal> sortedAscending, int percentile)
    {
        var n = sortedAscending.Count;
        var np = n * (percentile / 100m);
        if (np == decimal.Truncate(np))
        {
            var index = (int)np;
            if (index <= 0)
            {
                return sortedAscending[0];
            }

            if (index >= n)
            {
                return sortedAscending[n - 1];
            }

            return (sortedAscending[index - 1] + sortedAscending[index]) / 2m;
        }

        var oneBased = (int)Math.Ceiling((double)np);
        var zeroBased = Math.Clamp(oneBased, 1, n) - 1;
        return sortedAscending[zeroBased];
    }
}
