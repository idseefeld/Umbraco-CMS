using System.Collections.Concurrent;
using System.Diagnostics;
using Microsoft.Extensions.Configuration;
using NUnit.Framework;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Tests.Common.Testing;
using Umbraco.Cms.Tests.Integration.Testing;

namespace Umbraco.Cms.Tests.PostgreSqlSecurity;

[TestFixture]
[UmbracoTest(
    Database = UmbracoTestOptions.Database.NewSchemaPerTest,
    Logger = UmbracoTestOptions.Logger.Console,
    WithApplication = true)]
internal sealed class PostgreSqlConcurrencyTests : UmbracoIntegrationTest
{
    private IConfiguration _settings = null!;
    private TestEnvironmentGuardResult _guard = null!;

    [OneTimeSetUp]
    public void EnsureDisposableTestServer()
    {
        _settings = SecurityTestConfiguration.Build();
        _guard = TestEnvironmentGuard.Evaluate(_settings);

        if (!_guard.IsConfirmed)
        {
            Assert.Ignore("Set PostgreSqlSecurity:ConfirmDisposableTestServer=true to confirm the target is a dedicated test server.");
        }

        if (_guard.Violations.Count > 0)
        {
            Assert.Fail("Unsafe test environment: " + string.Join(" ", _guard.Violations));
        }
    }

    protected override void SetUpTestConfiguration(IConfigurationBuilder configBuilder) =>
        configBuilder.AddConfiguration(SecurityTestConfiguration.Build());

    [Test]
    public async Task Parallel_Document_Type_And_Content_Operations_Stay_Consistent()
    {
        var section = _settings.GetSection(SecurityTestConfiguration.Section);
        var tasks = Math.Max(10, section.GetValue("ParallelTasks", 10));
        var iterations = section.GetValue("Iterations", 5);
        var timeout = TimeSpan.FromMinutes(section.GetValue("TimeoutMinutes", 15));
        var reportPath = section["ReportPath"] ?? "TestResults/PostgreSqlSecurityReport.html";

        var records = new ConcurrentBag<StepRecord>();
        var scenario = new DocumentTypeContentScenario(
            GetRequiredService<IContentTypeService>(),
            GetRequiredService<IContentEditingService>(),
            records.Add);

        var started = DateTime.UtcNow;
        var stopwatch = Stopwatch.StartNew();
        var gate = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        Task[] workers;

        using (ExecutionContext.SuppressFlow())
        {
            workers = Enumerable.Range(1, tasks)
                .Select(worker => Task.Run(async () =>
                {
                    await gate.Task;
                    for (var iteration = 1; iteration <= iterations; iteration++)
                    {
                        await scenario.RunIterationAsync(worker, iteration);
                    }
                }))
                .ToArray();
        }

        gate.SetResult();
        var all = Task.WhenAll(workers);
        var timedOut = await Task.WhenAny(all, Task.Delay(timeout)) != all;
        stopwatch.Stop();

        var snapshot = records.ToArray();
        var info = new RunInfo(
            started,
            DateTime.UtcNow,
            _guard.Host ?? string.Empty,
            _guard.MasterDatabase ?? string.Empty,
            tasks,
            iterations,
            stopwatch.Elapsed.TotalMilliseconds);
        if (!Path.IsPathRooted(reportPath))
        {
            reportPath = Path.Combine(AppContext.BaseDirectory, reportPath);
        }

        var written = HtmlReportWriter.Write(reportPath, info, snapshot);
        TestContext.Progress.WriteLine($"PostgreSQL security report: {written}");

        var markdownWritten = MarkdownReportWriter.Write(Path.ChangeExtension(written, ".md"), info, snapshot);
        TestContext.Progress.WriteLine($"PostgreSQL security markdown report: {markdownWritten}");

        Assert.That(timedOut, Is.False, "Workers did not finish within the configured timeout.");
        Assert.That(snapshot.Where(r => !r.Success).Select(r => $"w{r.Worker}/i{r.Iteration}/{r.Step}: {r.ErrorType}"), Is.Empty);
        Assert.That(snapshot.Length, Is.GreaterThan(0));
    }
}
