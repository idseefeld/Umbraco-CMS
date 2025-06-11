using Microsoft.EntityFrameworkCore;
using Npgsql.EntityFrameworkCore.PostgreSQL; // Ensure this namespace is included for UseNpgsql
using Umbraco.Cms.Persistence.EFCore.Migrations;

namespace Our.Umbraco.PostgreSql.EFCore
{
    public class PostgreSqlMigrationProviderSetup : IMigrationProviderSetup
    {
        public string ProviderName => Constants.ProviderName;

        public void Setup(DbContextOptionsBuilder builder, string? connectionString) =>
            builder.UseNpgsql(connectionString, x => x.MigrationsAssembly(GetType().Assembly.FullName));
    }
}
