using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Our.Umbraco.PostgreSql.EFCore.Locking;
using Umbraco.Cms.Core.DependencyInjection;
using Umbraco.Cms.Core.DistributedLocking;
using Umbraco.Cms.Persistence.EFCore;
using Umbraco.Cms.Persistence.EFCore.Migrations;

namespace Our.Umbraco.PostgreSql.EFCore;

/// <summary>
/// Provides extension methods for configuring Umbraco to support PostgreSQL using Entity Framework Core.
/// </summary>
public static class UmbracoBuilderExtensions
{
    /// <summary>
    ///     Add required services for PostgreSQL support.
    /// </summary>
    public static IUmbracoBuilder AddUmbracoPostgreSqlEFCoreSupport(this IUmbracoBuilder builder)
    {
        builder.Services.TryAddEnumerable(ServiceDescriptor
            .Singleton<IMigrationProvider, PostgreSqlMigrationProvider>());
        builder.Services.TryAddEnumerable(ServiceDescriptor
            .Singleton<IMigrationProviderSetup, PostgreSqlMigrationProviderSetup>());

        builder.Services.TryAddEnumerable(ServiceDescriptor
            .Singleton<IDistributedLockingMechanism, PostgreSqlEFCoreDistributedLockingMechanism<UmbracoDbContext>>());

        return builder;
    }
}
