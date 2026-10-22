namespace Umbraco.Cms.Tests.PostgreSqlSecurity;

public static class StatisticsCalculator
{
    public static IReadOnlyList<StepStatistics> Calculate(IEnumerable<StepRecord> records) =>
        records
            .GroupBy(r => r.Step)
            .Select(g => Calculate(g.Key, g.ToList()))
            .OrderBy(s => s.Step, StringComparer.Ordinal)
            .ToList();

    public static StepStatistics Calculate(string step, IReadOnlyList<StepRecord> records)
    {
        if (records.Count == 0)
        {
            return new StepStatistics(step, 0, 0, 0, 0, 0, 0, 0);
        }

        var sorted = records.Select(r => r.Milliseconds).OrderBy(x => x).ToArray();
        return new StepStatistics(
            step,
            records.Count,
            records.Count(r => !r.Success),
            sorted.Average(),
            Median(sorted),
            sorted[0],
            sorted[^1],
            Percentile(sorted, 0.95));
    }

    private static double Median(double[] sorted)
    {
        var mid = sorted.Length / 2;
        return sorted.Length % 2 == 1 ? sorted[mid] : (sorted[mid - 1] + sorted[mid]) / 2;
    }

    private static double Percentile(double[] sorted, double p)
    {
        var rank = (int)Math.Ceiling(p * sorted.Length) - 1;
        return sorted[Math.Clamp(rank, 0, sorted.Length - 1)];
    }
}
