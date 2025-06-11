using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.DependencyInjection;
using Umbraco.Cms.Persistence.EFCore.Migrations;

namespace Our.Umbraco.PostgreSql.EFCore
{
    public class PostgreSqlDbContext : DbContext
    {
        public PostgreSqlDbContext(DbContextOptions<PostgreSqlDbContext> options)
        : base(ConfigureOptions(options))
        {

        }

        private static DbContextOptions<PostgreSqlDbContext> ConfigureOptions(DbContextOptions<PostgreSqlDbContext> options)
        {
            IOptionsMonitor<ConnectionStrings> connectionStringsOptionsMonitor = StaticServiceProvider.Instance.GetRequiredService<IOptionsMonitor<ConnectionStrings>>();

            ConnectionStrings connectionStrings = connectionStringsOptionsMonitor.CurrentValue;

            if (string.IsNullOrWhiteSpace(connectionStrings.ConnectionString))
            {
                ILogger<PostgreSqlDbContext> logger = StaticServiceProvider.Instance.GetRequiredService<ILogger<PostgreSqlDbContext>>();
                logger.LogCritical("No connection string was found, cannot setup Umbraco EF Core context");

                // we're throwing an exception here to make it abundantly clear that one should never utilize (or have a
                // dependency on) the DbContext before the connection string has been initialized by the installer.
                throw new InvalidOperationException("No connection string was found, cannot setup Umbraco EF Core context");
            }

            IEnumerable<IMigrationProviderSetup> migrationProviders = StaticServiceProvider.Instance.GetServices<IMigrationProviderSetup>();
            IMigrationProviderSetup? migrationProvider = migrationProviders
                .FirstOrDefault(x => x.ProviderName.Equals(Constants.ProviderName));

            if (migrationProvider == null && connectionStrings.ProviderName != null)
            {
                throw new InvalidOperationException($"No migration provider found for provider name {connectionStrings.ProviderName}");
            }

            var optionsBuilder = new DbContextOptionsBuilder<PostgreSqlDbContext>(options);
            migrationProvider?.Setup(optionsBuilder, connectionStrings.ConnectionString);
            return optionsBuilder.Options;
        }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            foreach (IMutableEntityType entity in modelBuilder.Model.GetEntityTypes())
            {
                entity.SetTableName(global::Umbraco.Cms.Core.Constants.DatabaseSchema.TableNamePrefix + entity.GetTableName());
            }
        }
    }
}
