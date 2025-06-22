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
        builder.Services.AddSingleton<IMigrationProvider, PostgreSqlMigrationProvider>();
        builder.Services.AddSingleton<IMigrationProviderSetup, PostgreSqlMigrationProviderSetup>();

        builder.Services.AddSingleton<IDistributedLockingMechanism, PostgreSqlEFCoreDistributedLockingMechanism<UmbracoDbContext>>();

        builder.Services.AddUmbracoDbContext<UmbracoDbContext>((serviceProvider, options) =>
        {
            // Register the entity sets needed by OpenIddict.
            options.UseOpenIddict();
        });
    }
}
