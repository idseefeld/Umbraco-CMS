// Example: Add this to your EFCore project
using System.Data.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;
using Umbraco.Cms.Core;

namespace Our.Umbraco.PostgreSql.EFCore;

public class PostgreSqlDbContextFactory : IDesignTimeDbContextFactory<PostgreSqlDbContext>
{
    public PostgreSqlDbContext CreateDbContext(string[] args)
    {
        // Find the startup project directory (Umbraco.Web.UI)
        string projectDir = Directory.GetCurrentDirectory();
        string startupProjectPath = Path.GetFullPath(Path.Combine(projectDir, "../Umbraco.Web.UI"));

        Console.WriteLine($"Loading configuration from: {startupProjectPath}");

        // Load configuration from the startup project
        var configuration = new ConfigurationBuilder()
            .SetBasePath(startupProjectPath)
            .AddJsonFile("appsettings.json", optional: false)
            .Build();

        var connectionString = configuration.GetConnectionString("umbracoDbDSN");
        Console.WriteLine($"Connection string found: {!string.IsNullOrEmpty(connectionString)}");

        // Create options builder with Npgsql provider
        var optionsBuilder = new DbContextOptionsBuilder<PostgreSqlDbContext>();
        optionsBuilder.UseNpgsql(connectionString);

        // Create context with direct options (bypass ConfigureOptions)
        return new PostgreSqlDbContext(optionsBuilder.Options);
    }
}
