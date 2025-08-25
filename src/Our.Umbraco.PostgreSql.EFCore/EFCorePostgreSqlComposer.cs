using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Our.Umbraco.PostgreSql.EFCore.Locking;
using Umbraco.Cms.Core.Composing;
using Umbraco.Cms.Core.DependencyInjection;
using Umbraco.Cms.Core.DistributedLocking;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Persistence.EFCore;
using Umbraco.Cms.Persistence.EFCore.Composition;
using Umbraco.Cms.Persistence.EFCore.Migrations;
using Umbraco.Extensions;

namespace Our.Umbraco.PostgreSql.EFCore;

public class EFCorePostgreSqlComposer : IComposer
{
    public new void Compose(IUmbracoBuilder builder)
    {
        //var connectionString = builder.Config.GetUmbracoConnectionString();
        //var providerName = builder.Config.GetConnectionStringProviderName("umbracoDbDSN");

        //if (string.IsNullOrWhiteSpace(connectionString) || providerName != "Npgsql")
        //{
        //    throw new InvalidOperationException("The PostgreSQL EF Core provider requires a valid connection string with the provider name 'Npgsql'.");
        //}

        //builder.Services.AddDbContext<UmbracoDbContext>(options =>
        //    options.UseNpgsql(connectionString));

        builder.Services.AddSingleton<IMigrationProvider, PostgreSqlMigrationProvider>();
        builder.Services.AddSingleton<IMigrationProviderSetup, PostgreSqlMigrationProviderSetup>();

        builder.Services.AddSingleton<IDistributedLockingMechanism, PostgreSqlEFCoreDistributedLockingMechanism<UmbracoDbContext>>();
    }
}
