using Microsoft.EntityFrameworkCore;
using Umbraco.Cms.Persistence.EFCore.Migrations;

namespace Our.Umbraco.PostgreSql.EFCore
{
    public class PostgreSqlMigrationProviderSetup : IMigrationProviderSetup
    {
        public string ProviderName => Constants.ProviderName;

        public void Setup(DbContextOptionsBuilder builder, string? connectionString)
        {
            var assemblyName = GetType().Assembly.FullName;
            builder.UseNpgsql(connectionString, x => x.MigrationsAssembly(assemblyName));
        }
    }
}
