using Microsoft.EntityFrameworkCore;
using Umbraco.Cms.Persistence.EFCore;
using Umbraco.Cms.Persistence.EFCore.Migrations;
using Umbraco.Extensions;

namespace Our.Umbraco.PostgreSql.EFCore
{
    public class PostgreSqlMigrationProvider : IMigrationProvider
    {
        private readonly IDbContextFactory<PostgreSqlDbContext> _dbContextFactory;

        public PostgreSqlMigrationProvider(IDbContextFactory<PostgreSqlDbContext> dbContextFactory) => _dbContextFactory = dbContextFactory;

        public string ProviderName => Constants.ProviderName;

        public async Task MigrateAsync(EFCoreMigration migration)
        {
            PostgreSqlDbContext context = await _dbContextFactory.CreateDbContextAsync();
            await context.MigrateDatabaseAsync(GetMigrationType(migration));
        }

        public async Task MigrateAllAsync()
        {
            PostgreSqlDbContext context = await _dbContextFactory.CreateDbContextAsync();
            await context.Database.MigrateAsync();
        }
        private static Type GetMigrationType(EFCoreMigration migration) =>
       migration switch
       {
           //EFCoreMigration.InitialCreate => typeof(Migrations.InitialCreate),
           //EFCoreMigration.AddOpenIddict => typeof(Migrations.AddOpenIddict),
           //EFCoreMigration.UpdateOpenIddictToV5 => typeof(Migrations.UpdateOpenIddictToV5),
           _ => throw new ArgumentOutOfRangeException(nameof(migration), $@"Not expected migration value: {migration}")
       };
    }
}
