using System.Data.Common;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using NPoco;
using Our.Umbraco.PostgreSql.Interceptors;
using Our.Umbraco.PostgreSql.Locking;
using Our.Umbraco.PostgreSql.Services;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.DependencyInjection;
using Umbraco.Cms.Core.DistributedLocking;
using Umbraco.Cms.Infrastructure.Migrations.Notifications;
using Umbraco.Cms.Infrastructure.Persistence;
using Umbraco.Cms.Infrastructure.Persistence.SqlSyntax;
using Umbraco.Extensions;

namespace Our.Umbraco.PostgreSql
{
    /// <summary>
    /// Provides extension methods for configuring Umbraco to support PostgreSQL as a database provider.
    /// </summary>
    /// <remarks>These extension methods register the necessary services and components required for Umbraco to
    /// operate with a PostgreSQL database. Use these methods during application startup to enable PostgreSQL support in
    /// your Umbraco application.</remarks>
    public static class UmbracoBuilderExtensions
    {
        /// <summary>
        ///     Add required services for PostgreSQL support.
        /// </summary>
        public static IUmbracoBuilder AddUmbracoPostgreSqlSupport(this IUmbracoBuilder builder)
        {
            //builder.AddNotificationAsyncHandler<DatabaseSchemaInitializedNotification, PostgreSqlDataInitializedHandler>();

            builder.Services.TryAddEnumerable(ServiceDescriptor
                .Singleton<ISqlSyntaxProvider, PostgreSqlSyntaxProvider>());
            builder.Services.TryAddEnumerable(ServiceDescriptor
                .Singleton<IBulkSqlInsertProvider, PostgreSqlBatchSqlInsertProvider>());
            builder.Services.TryAddEnumerable(ServiceDescriptor
                .Singleton<IDatabaseCreator, PostgreSqlDatabaseCreator>());
            builder.Services.TryAddEnumerable(ServiceDescriptor
                .Singleton<IProviderSpecificMapperFactory, PostgreSqlSpecificMapperFactory>());
            builder.Services.TryAddEnumerable(ServiceDescriptor
                .Singleton<IDatabaseProviderMetadata, PostgreSqlDatabaseProviderMetadata>());

            builder.Services.TryAddEnumerable(ServiceDescriptor
                .Singleton<IDistributedLockingMechanism, PostgreSqlDistributedLockingMechanism>());

            builder.Services.TryAddEnumerable(ServiceDescriptor
                .Singleton<IProviderSpecificInterceptor, PostgreSqlAddMiniProfilerInterceptor>());
            builder.Services.TryAddEnumerable(ServiceDescriptor
                .Singleton<IProviderSpecificInterceptor, PostgreSqlAddRetryPolicyInterceptor>());
            builder.Services.TryAddEnumerable(ServiceDescriptor
                .Singleton<IProviderSpecificInterceptor, PostgreSqlExecutingInterceptor>());
            builder.Services.TryAddEnumerable(ServiceDescriptor
                .Singleton<IProviderSpecificInterceptor, PostgreSqlDataInterceptor>());

            DbProviderFactories.UnregisterFactory(Constants.ProviderName);
            DbProviderFactories.RegisterFactory(Constants.ProviderName, PostgreSqlDbProviderFactory.Instance);

            builder.Services.Replace(ServiceDescriptor.Singleton<IUmbracoDatabaseFactory, PostgreSqlDatabaseFactory>());

            return builder;
        }
    }
}
