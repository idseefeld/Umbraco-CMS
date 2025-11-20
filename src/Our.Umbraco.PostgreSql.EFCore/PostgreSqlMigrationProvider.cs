using Microsoft.EntityFrameworkCore;
using Umbraco.Cms.Persistence.EFCore;
using Umbraco.Cms.Persistence.EFCore.Migrations;
using Umbraco.Extensions;

namespace Our.Umbraco.PostgreSql.EFCore
{
    public class PostgreSqlMigrationProvider : IMigrationProvider
    {
        private readonly IDbContextFactory<UmbracoDbContext> _dbContextFactory;

        public PostgreSqlMigrationProvider(IDbContextFactory<UmbracoDbContext> dbContextFactory) => _dbContextFactory = dbContextFactory;

        public string ProviderName => Constants.ProviderName;

        public async Task MigrateAsync(EFCoreMigration migration)
        {
            // ToDo: wait until BaseDataInitializedHandler.AlterSquences is done - see Umbraco.Cms.Infrastructure.Migrations.EFCoreMigrationProvider
            UmbracoDbContext context = await _dbContextFactory.CreateDbContextAsync();
            var migrationType = GetMigrationType(migration);
            await context.MigrateDatabaseAsync(migrationType);
        }

        public async Task MigrateAllAsync()
        {
            UmbracoDbContext context = await _dbContextFactory.CreateDbContextAsync();

            if (context.Database.CurrentTransaction is not null)
            {
                throw new InvalidOperationException("Cannot migrate all when a transaction is active.");
            }

            await context.Database.MigrateAsync();
        }

        private static Type GetMigrationType(EFCoreMigration migration) =>
            migration switch
            {
                EFCoreMigration.InitialCreate => typeof(Migrations.InitialCreate),
                EFCoreMigration.AddOpenIddict => typeof(Migrations.AddOpenIddict),
                EFCoreMigration.UpdateOpenIddictToV5 => typeof(Migrations.UpdateOpenIddictToV5),
                EFCoreMigration.UpdateOpenIddictToV7 => typeof(Migrations.UpdateOpenIddictToV7),
                _ => throw new ArgumentOutOfRangeException(nameof(migration), $@"Not expected migration value: {migration}"),
            };
    }
}
