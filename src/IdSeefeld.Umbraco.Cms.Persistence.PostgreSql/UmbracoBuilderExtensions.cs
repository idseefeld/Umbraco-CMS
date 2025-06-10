using System.Data.Common;
using IdSeefeld.Umbraco.Cms.Persistence.PostgreSql.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.DependencyInjection;
using Umbraco.Cms.Core.DistributedLocking;
using Umbraco.Cms.Infrastructure.Persistence;
using Umbraco.Cms.Infrastructure.Persistence.SqlSyntax;

namespace IdSeefeld.Umbraco.Cms.Persistence.PostgreSql
{
    /// <summary>
    ///     SQLite support extensions for IUmbracoBuilder.
    /// </summary>
    public static class UmbracoBuilderExtensions
    {
        /// <summary>
        ///     Add required services for PostgreSQL support.
        /// </summary>
        public static IUmbracoBuilder AddUmbracoPostgreSqlSupport(this IUmbracoBuilder builder)
        {
            builder.Services.TryAddEnumerable(ServiceDescriptor.Singleton<ISqlSyntaxProvider, PostgreSqlSyntaxProvider>());

            builder.Services.TryAddEnumerable(ServiceDescriptor.Singleton<IBulkSqlInsertProvider, PostgreSqlBulkSqlInsertProvider>());

            builder.Services.TryAddEnumerable(ServiceDescriptor.Singleton<IDatabaseCreator, PostgreSqlDatabaseCreator>());
            builder.Services.TryAddEnumerable(ServiceDescriptor.Singleton<IProviderSpecificMapperFactory, PostgreSqlSpecificMapperFactory>());

            builder.Services.TryAddEnumerable(ServiceDescriptor.Singleton<IDatabaseProviderMetadata, PostgreSqlDatabaseProviderMetadata>());

            builder.Services.TryAddEnumerable(ServiceDescriptor.Singleton<IDistributedLockingMechanism, PostgreSqlDistributedLockingMechanism>());
            builder.Services.TryAddEnumerable(ServiceDescriptor.Singleton<IProviderSpecificInterceptor, PostgreSqlAddMiniProfilerInterceptor>());
            builder.Services.TryAddEnumerable(ServiceDescriptor.Singleton<IProviderSpecificInterceptor, PostgreSqlAddRetryPolicyInterceptor>());


            _ = DbProviderFactories.UnregisterFactory(Constants.ProviderName);
            DbProviderFactories.RegisterFactory(Constants.ProviderName, Npgsql.NpgsqlFactory.Instance);

            // Support provider name set by the configuration API for connection string environment variables
            builder.Services.ConfigureAll<ConnectionStrings>(options =>
            {
                if (options.ProviderName == Constants.Name)
                {
                    options.ProviderName = Constants.ProviderName;
                }
            });
            return builder;
        }
    }
}
