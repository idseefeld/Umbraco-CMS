using System.Globalization;
using System.Net;
using System.Text;

namespace Umbraco.Cms.Tests.PostgreSqlSecurity;

public static class HtmlReportWriter
{
    public static string Render(RunInfo info, IReadOnlyList<StepRecord> records)
    {
        var stats = StatisticsCalculator.Calculate(records);
        var failures = records.Where(r => !r.Success).ToList();
        var sb = new StringBuilder();

        sb.AppendLine("<!DOCTYPE html><html lang=\"en\"><head><meta charset=\"utf-8\">");
        sb.AppendLine("<title>PostgreSQL Security &amp; Concurrency Report</title>");
        sb.AppendLine("<style>body{font-family:Segoe UI,Arial,sans-serif;margin:2rem;color:#222}table{border-collapse:collapse;margin:1rem 0}th,td{border:1px solid #ccc;padding:4px 10px;text-align:right}th{background:#f0f0f0}td.l,th.l{text-align:left}.fail{background:#fde8e8}.ok{color:#176b2c}.bad{color:#b00020}</style>");
        sb.AppendLine("</head><body><h1>PostgreSQL Security &amp; Concurrency Report</h1>");

        sb.AppendLine("<h2>Run</h2><table>");
        Row(sb, "Started (UTC)", info.StartedUtc.ToString("u", CultureInfo.InvariantCulture));
        Row(sb, "Finished (UTC)", info.FinishedUtc.ToString("u", CultureInfo.InvariantCulture));
        Row(sb, "Host", info.Host);
        Row(sb, "Master database", info.Database);
        Row(sb, "Parallel tasks", info.ParallelTasks.ToString(CultureInfo.InvariantCulture));
        Row(sb, "Iterations per task", info.Iterations.ToString(CultureInfo.InvariantCulture));
        Row(sb, "Total duration (ms)", Format(info.TotalMilliseconds));
        Row(sb, "Steps recorded", records.Count.ToString(CultureInfo.InvariantCulture));
        Row(sb, "Failed steps", failures.Count.ToString(CultureInfo.InvariantCulture));
        sb.AppendLine("</table>");

        sb.AppendLine("<h2>Summary per step (ms)</h2><table><tr><th class=\"l\">Step</th><th>Count</th><th>Failures</th><th>Mean</th><th>Median</th><th>Min</th><th>Max</th><th>P95</th></tr>");
        foreach (var s in stats)
        {
            sb.Append("<tr><td class=\"l\">").Append(Encode(s.Step)).Append("</td><td>").Append(s.Count)
                .Append("</td><td class=\"").Append(s.Failures == 0 ? "ok" : "bad").Append("\">").Append(s.Failures)
                .Append("</td><td>").Append(Format(s.MeanMs))
                .Append("</td><td>").Append(Format(s.MedianMs))
                .Append("</td><td>").Append(Format(s.MinMs))
                .Append("</td><td>").Append(Format(s.MaxMs))
                .Append("</td><td>").Append(Format(s.P95Ms)).AppendLine("</td></tr>");
        }

        sb.AppendLine("</table>");

        sb.AppendLine("<h2>Errors</h2>");
        if (failures.Count == 0)
        {
            sb.AppendLine("<p class=\"ok\">No errors recorded.</p>");
        }
        else
        {
            sb.AppendLine("<table><tr><th>Worker</th><th>Iteration</th><th class=\"l\">Step</th><th class=\"l\">Type</th><th class=\"l\">Message</th></tr>");
            foreach (var f in failures)
            {
                sb.Append("<tr class=\"fail\"><td>").Append(f.Worker).Append("</td><td>").Append(f.Iteration)
                    .Append("</td><td class=\"l\">").Append(Encode(f.Step))
                    .Append("</td><td class=\"l\">").Append(Encode(f.ErrorType))
                    .Append("</td><td class=\"l\">").Append(Encode(f.ErrorMessage)).AppendLine("</td></tr>");
            }

            sb.AppendLine("</table>");
        }

        var workers = records.Select(x => x.Worker).Distinct().OrderBy(x => x).ToList();
        var stepOrder = records.OrderBy(x => x.StartedUtc).Select(x => x.Step).Distinct().ToList();
        var lookup = records.GroupBy(x => (x.Iteration, x.Step, x.Worker)).ToDictionary(g => g.Key, g => g.First());
        sb.AppendLine("<h2>All results per worker (duration in ms)</h2>");
        foreach (var iteration in records.Select(x => x.Iteration).Distinct().OrderBy(x => x))
        {
            sb.Append("<h3>Iteration ").Append(iteration).AppendLine("</h3><table><tr><th class=\"l\">Step</th>");
            foreach (var w in workers)
            {
                sb.Append("<th>Worker ").Append(w).AppendLine("</th>");
            }

            sb.AppendLine("</tr>");
            foreach (var step in stepOrder)
            {
                sb.Append("<tr><td class=\"l\">").Append(Encode(step)).Append("</td>");
                foreach (var w in workers)
                {
                    if (lookup.TryGetValue((iteration, step, w), out var r))
                    {
                        sb.Append(r.Success ? "<td>" : "<td class=\"fail\" title=\"" + Encode(r.ErrorType) + "\">")
                            .Append(Format(r.Milliseconds)).Append(r.Success ? string.Empty : " FAILED").Append("</td>");
                    }
                    else
                    {
                        sb.Append("<td></td>");
                    }
                }

                sb.AppendLine("</tr>");
            }

            sb.AppendLine("</table>");
        }

        sb.AppendLine("</body></html>");
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
        sb.Append("<tr><th class=\"l\">").Append(Encode(label)).Append("</th><td class=\"l\">")
            .Append(Encode(value)).AppendLine("</td></tr>");

    private static string Format(double value) => value.ToString("F2", CultureInfo.InvariantCulture);

    private static string Encode(string? value) => WebUtility.HtmlEncode(value ?? string.Empty);
}
