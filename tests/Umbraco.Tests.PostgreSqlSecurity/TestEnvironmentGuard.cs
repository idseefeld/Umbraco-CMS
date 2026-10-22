using Microsoft.Extensions.Configuration;
using Npgsql;

namespace Umbraco.Cms.Tests.PostgreSqlSecurity;

public static class SecurityTestConfiguration
{
    public const string Section = "PostgreSqlSecurity";

    public static IConfiguration Build()
    {
        var builder = new ConfigurationBuilder();
        builder.AddJsonFile("appsettings.Tests.json", optional: true);
        builder.AddJsonFile("appsettings.PostgreSqlSecurity.json", optional: false);
        builder.AddJsonFile("appsettings.Tests.Local.json", optional: true);
        builder.AddJsonFile("appsettings.PostgreSqlSecurity.Local.json", optional: true);
        builder.AddEnvironmentVariables();
        return builder.Build();
    }
}

public sealed class TestEnvironmentGuardResult
{
    public bool IsConfirmed { get; init; }

    public IReadOnlyList<string> Violations { get; init; } = [];

    public string? Host { get; init; }

    public string? MasterDatabase { get; init; }

    public bool IsSafe => IsConfirmed && Violations.Count == 0;
}

public static class TestEnvironmentGuard
{
    private const string ConnectionStringKey = "Tests:Database:SQLServerMasterConnectionString";
    private const string DatabaseTypeKey = "Tests:Database:DatabaseType";

    public static TestEnvironmentGuardResult Evaluate(IConfiguration configuration)
    {
        var violations = new List<string>();
        var section = configuration.GetSection(SecurityTestConfiguration.Section);
        var confirmed = section.GetValue<bool>("ConfirmDisposableTestServer");
        var allowedHosts = section.GetSection("AllowedHosts").Get<string[]>() ?? [];
        var prefix = section["RequiredDatabasePrefix"];

        if (!string.Equals(configuration[DatabaseTypeKey], "PostgreSql", StringComparison.OrdinalIgnoreCase))
        {
            violations.Add($"{DatabaseTypeKey} must be PostgreSql.");
        }

        if (string.IsNullOrWhiteSpace(prefix))
        {
            violations.Add("PostgreSqlSecurity:RequiredDatabasePrefix must be configured.");
        }

        if (allowedHosts.Length == 0)
        {
            violations.Add("PostgreSqlSecurity:AllowedHosts must list at least one dedicated test host.");
        }

        string? host = null;
        string? database = null;
        var connectionString = configuration[ConnectionStringKey];
        if (string.IsNullOrWhiteSpace(connectionString))
        {
            violations.Add($"{ConnectionStringKey} is not configured.");
        }
        else
        {
            try
            {
                var builder = new NpgsqlConnectionStringBuilder(connectionString);
                host = builder.Host;
                database = builder.Database;

                if (string.IsNullOrWhiteSpace(host) ||
                    !allowedHosts.Contains(host, StringComparer.OrdinalIgnoreCase))
                {
                    violations.Add($"Host '{host}' is not in PostgreSqlSecurity:AllowedHosts.");
                }

                if (!string.IsNullOrWhiteSpace(prefix) &&
                    (string.IsNullOrWhiteSpace(database) ||
                     !database.StartsWith(prefix, StringComparison.Ordinal)))
                {
                    violations.Add($"Master database '{database}' does not start with required prefix '{prefix}'.");
                }
            }
            catch (ArgumentException)
            {
                violations.Add($"{ConnectionStringKey} is not a valid PostgreSQL connection string.");
            }
        }

        return new TestEnvironmentGuardResult
        {
            IsConfirmed = confirmed,
            Violations = violations,
            Host = host,
            MasterDatabase = database,
        };
    }
}
