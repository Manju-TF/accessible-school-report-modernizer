namespace AccessibleSchoolReports.IntegrationTests.Parity;

public sealed class LegacyModernParityTests
{
    [Fact]
    public async Task CharacterizedMetrics_MatchBetweenLegacyPdfAndModernCalculator()
    {
        var run = await ParityRunner.RunAsync();
        ParityRunner.WriteResultsDocument(run);
        var mismatches = run.Observations.Where(item => item.Status == ParityStatus.Mismatch).ToArray();

        Assert.True(
            mismatches.Length == 0,
            $"Parity failed for {mismatches.Length} of {run.Observations.Count} metrics "
            + $"(match {run.MatchCount}, unresolved {run.UnresolvedCount}). "
            + $"School selection: {run.SelectionReason} Modern school {run.ModernSchoolCode ?? "(none)"}. "
            + Environment.NewLine
            + string.Join(
                Environment.NewLine,
                mismatches.Select(item =>
                    $"{item.Metric.Id} {item.Metric.RuleId} {item.Metric.Field}: legacy={item.Metric.Expected?.ToString() ?? "."} modern={item.Modern?.ToString() ?? "."} ({item.Explanation})")));
    }
}
