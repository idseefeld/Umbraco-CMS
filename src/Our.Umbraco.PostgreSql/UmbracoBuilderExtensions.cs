using System.Data.Common;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Our.Umbraco.PostgreSql.Interceptors;
using Our.Umbraco.PostgreSql.Locking;
using Our.Umbraco.PostgreSql.Services;
using Umbraco.Cms.Core.DependencyInjection;
using Umbraco.Cms.Core.DistributedLocking;
using Umbraco.Cms.Infrastructure.Persistence;
using Umbraco.Cms.Infrastructure.Persistence.SqlSyntax;
using Umbraco.Extensions;

namespace Our.Umbraco.PostgreSql
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
            builder.Services.TryAddEnumerable(ServiceDescriptor
                .Singleton<IProviderSpecificMapperFactory, PostgreSqlSpecificMapperFactory>());
            builder.Services.TryAddEnumerable(ServiceDescriptor.Singleton<ISqlSyntaxProvider, PostgreSqlSyntaxProvider>());
            builder.Services.TryAddEnumerable(ServiceDescriptor
                .Singleton<IBulkSqlInsertProvider, PostgreSqlBulkSqlInsertProvider>());
            builder.Services.TryAddEnumerable(ServiceDescriptor.Singleton<IDatabaseCreator, PostgreSqlDatabaseCreator>());

            builder.Services.TryAddEnumerable(ServiceDescriptor
                .Singleton<IDatabaseProviderMetadata, PostgreSqlDatabaseProviderMetadata>());

            builder.Services.TryAddEnumerable(ServiceDescriptor
                .Singleton<IDistributedLockingMechanism, PostgreSqlDistributedLockingMechanism>());

            builder.Services.TryAddEnumerable(ServiceDescriptor
                .Singleton<IProviderSpecificInterceptor, PostgreSqlAddMiniProfilerInterceptor>());
            builder.Services.TryAddEnumerable(ServiceDescriptor
                .Singleton<IProviderSpecificInterceptor, PostgreSqlAddRetryPolicyInterceptor>());

            DbProviderFactories.UnregisterFactory(Constants.ProviderName);
            DbProviderFactories.RegisterFactory(Constants.ProviderName, Npgsql.NpgsqlFactory.Instance);

            return builder;
        }
    }
}
