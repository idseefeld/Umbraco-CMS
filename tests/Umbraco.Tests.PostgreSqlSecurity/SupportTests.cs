using Microsoft.Extensions.Configuration;
using NUnit.Framework;

namespace Umbraco.Cms.Tests.PostgreSqlSecurity;

[TestFixture]
internal sealed class SupportTests
{
    private static IConfiguration Config(bool confirmed, string host, string database, string type = "PostgreSql") =>
        new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string, string?>
        {
            ["PostgreSqlSecurity:ConfirmDisposableTestServer"] = confirmed.ToString(),
            ["PostgreSqlSecurity:AllowedHosts:0"] = "testhost",
            ["PostgreSqlSecurity:RequiredDatabasePrefix"] = "UmbracoTest",
            ["Tests:Database:DatabaseType"] = type,
            ["Tests:Database:SQLServerMasterConnectionString"] = $"Host={host};Database={database};Username=u;Password=p",
        }).Build();

    [Test]
    public void Guard_Accepts_Confirmed_Allowed_Host_And_Prefix() =>
        Assert.That(TestEnvironmentGuard.Evaluate(Config(true, "testhost", "UmbracoTest-1")).IsSafe, Is.True);

    [Test]
    public void Guard_Rejects_Unknown_Host() =>
        Assert.That(TestEnvironmentGuard.Evaluate(Config(true, "prod", "UmbracoTest-1")).Violations, Is.Not.Empty);

    [Test]
    public void Guard_Rejects_Wrong_Database_Prefix() =>
        Assert.That(TestEnvironmentGuard.Evaluate(Config(true, "testhost", "umbraco")).Violations, Is.Not.Empty);

    [Test]
    public void Guard_Rejects_Non_PostgreSql() =>
        Assert.That(TestEnvironmentGuard.Evaluate(Config(true, "testhost", "UmbracoTest-1", "SQLite")).Violations, Is.Not.Empty);

    [Test]
    public void Guard_Requires_Confirmation() =>
        Assert.That(TestEnvironmentGuard.Evaluate(Config(false, "testhost", "UmbracoTest-1")).IsSafe, Is.False);

    [Test]
    public void Statistics_Are_Calculated_Per_Step()
    {
        var records = Enumerable.Range(1, 10)
            .Select(i => new StepRecord(1, i, "A", DateTime.UtcNow, i, i != 10, null, null))
            .ToList();

        var stats = StatisticsCalculator.Calculate(records).Single();

        Assert.That(stats.Count, Is.EqualTo(10));
        Assert.That(stats.Failures, Is.EqualTo(1));
        Assert.That(stats.MeanMs, Is.EqualTo(5.5));
        Assert.That(stats.MedianMs, Is.EqualTo(5.5));
        Assert.That(stats.MinMs, Is.EqualTo(1));
        Assert.That(stats.MaxMs, Is.EqualTo(10));
        Assert.That(stats.P95Ms, Is.EqualTo(10));
    }

    [Test]
    public void Report_Encodes_Dynamic_Values()
    {
        var info = new RunInfo(DateTime.UtcNow, DateTime.UtcNow, "<host>", "db", 10, 5, 1);
        var records = new[] { new StepRecord(1, 1, "S", DateTime.UtcNow, 1, false, "Err", "<script>alert(1)</script>") };

        var html = HtmlReportWriter.Render(info, records);

        Assert.That(html, Does.Not.Contain("<script>"));
        Assert.That(html, Does.Contain("&lt;script&gt;"));
        Assert.That(html, Does.Contain("&lt;host&gt;"));
    }
}
