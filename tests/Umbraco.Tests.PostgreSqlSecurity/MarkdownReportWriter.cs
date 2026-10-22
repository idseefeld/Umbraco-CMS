using System.Globalization;
using System.Text;

namespace Umbraco.Cms.Tests.PostgreSqlSecurity;

public static class MarkdownReportWriter
{
    public static string Render(RunInfo info, IReadOnlyList<StepRecord> records)
    {
        var stats = StatisticsCalculator.Calculate(records);
        var failures = records.Where(r => !r.Success).ToList();
        var sb = new StringBuilder();

        sb.AppendLine("# PostgreSQL Security & Concurrency Report").AppendLine();
        sb.AppendLine("## Run").AppendLine();
        sb.AppendLine("| Property | Value |").AppendLine("|---|---|");
        Row(sb, "Started (UTC)", info.StartedUtc.ToString("u", CultureInfo.InvariantCulture));
        Row(sb, "Finished (UTC)", info.FinishedUtc.ToString("u", CultureInfo.InvariantCulture));
        Row(sb, "Host", info.Host);
        Row(sb, "Master database", info.Database);
        Row(sb, "Parallel tasks", info.ParallelTasks.ToString(CultureInfo.InvariantCulture));
        Row(sb, "Iterations per task", info.Iterations.ToString(CultureInfo.InvariantCulture));
        Row(sb, "Total duration (ms)", Format(info.TotalMilliseconds));
        Row(sb, "Steps recorded", records.Count.ToString(CultureInfo.InvariantCulture));
        Row(sb, "Failed steps", failures.Count.ToString(CultureInfo.InvariantCulture));
        sb.AppendLine();

        sb.AppendLine("## Summary per step (ms)").AppendLine();
        sb.AppendLine("| Step | Count | Failures | Mean | Median | Min | Max | P95 |").AppendLine("|---|---:|---:|---:|---:|---:|---:|---:|");
        foreach (var s in stats)
        {
            sb.Append("| ").Append(Escape(s.Step)).Append(" | ").Append(s.Count).Append(" | ").Append(s.Failures)
                .Append(" | ").Append(Format(s.MeanMs)).Append(" | ").Append(Format(s.MedianMs))
                .Append(" | ").Append(Format(s.MinMs)).Append(" | ").Append(Format(s.MaxMs))
                .Append(" | ").Append(Format(s.P95Ms)).AppendLine(" |");
        }

        sb.AppendLine().AppendLine("## Errors").AppendLine();
        if (failures.Count == 0)
        {
            sb.AppendLine("No errors recorded.");
        }
        else
        {
            sb.AppendLine("| Worker | Iteration | Step | Type | Message |").AppendLine("|---:|---:|---|---|---|");
            foreach (var f in failures)
            {
                sb.Append("| ").Append(f.Worker).Append(" | ").Append(f.Iteration).Append(" | ").Append(Escape(f.Step))
                    .Append(" | ").Append(Escape(f.ErrorType)).Append(" | ").Append(Escape(f.ErrorMessage)).AppendLine(" |");
            }
        }

        var workers = records.Select(x => x.Worker).Distinct().OrderBy(x => x).ToList();
        var stepOrder = records.OrderBy(x => x.StartedUtc).Select(x => x.Step).Distinct().ToList();
        var lookup = records.GroupBy(x => (x.Iteration, x.Step, x.Worker)).ToDictionary(g => g.Key, g => g.First());

        sb.AppendLine().AppendLine("## All results per worker (duration in ms)");
        foreach (var iteration in records.Select(x => x.Iteration).Distinct().OrderBy(x => x))
        {
            sb.AppendLine().Append("### Iteration ").Append(iteration).AppendLine().AppendLine();
            sb.Append("| Step |");
            foreach (var w in workers)
            {
                sb.Append(" Worker ").Append(w).Append(" |");
            }

            sb.AppendLine().Append("|---|");
            foreach (var w in workers)
            {
                sb.Append("---:|");
            }

            sb.AppendLine();
            foreach (var step in stepOrder)
            {
                sb.Append("| ").Append(Escape(step)).Append(" |");
                foreach (var w in workers)
                {
                    sb.Append(' ');
                    if (lookup.TryGetValue((iteration, step, w), out var r))
                    {
                        sb.Append(r.Success ? Format(r.Milliseconds) : "**" + Format(r.Milliseconds) + " FAILED**");
                    }

                    sb.Append(" |");
                }

                sb.AppendLine();
            }
        }

        return sb.ToString();
    }

    public static string Write(string path, RunInfo info, IReadOnlyList<StepRecord> records)
    {
        var fullPath = Path.GetFullPath(path);
        var directory = Path.GetDirectoryName(fullPath);
        if (!string.IsNullOrEmpty(directory))
        {
            Directory.CreateDirectory(directory);
        }

        File.WriteAllText(fullPath, Render(info, records), Encoding.UTF8);
        return fullPath;
    }

    private static void Row(StringBuilder sb, string label, string value) =>
        sb.Append("| ").Append(Escape(label)).Append(" | ").Append(Escape(value)).AppendLine(" |");

    private static string Format(double value) => value.ToString("F2", CultureInfo.InvariantCulture);

    private static string Escape(string? value) =>
        (value ?? string.Empty).Replace("\\", "\\\\").Replace("|", "\\|").Replace("\r", " ").Replace("\n", " ");
}

