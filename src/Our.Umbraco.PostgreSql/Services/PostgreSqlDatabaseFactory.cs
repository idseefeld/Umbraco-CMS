using System.Data.Common;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NPoco;
using NPoco.FluentMappings;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Infrastructure.Migrations.Install;
using Umbraco.Cms.Infrastructure.Persistence;
using Umbraco.Cms.Infrastructure.Persistence.Mappers;
using Umbraco.Cms.Infrastructure.Persistence.SqlSyntax;
using Umbraco.Extensions;
using IMapperCollection = Umbraco.Cms.Infrastructure.Persistence.Mappers.IMapperCollection;

namespace Our.Umbraco.PostgreSql.Services
{
    public class PostgreSqlDatabaseFactory : UmbracoDatabaseFactory
    {
        private readonly Dictionary<string, long> _lastInsertIds = new Dictionary<string, long>();

        private readonly DatabaseSchemaCreatorFactory _databaseSchemaCreatorFactory;
        private readonly ILogger<PostgreSqlDatabaseFactory> _logger;
        private readonly ILoggerFactory _loggerFactory;
        private IBulkSqlInsertProvider? _bulkSqlInsertProvider;

        private NPoco.MapperCollection? _pocoMappers;

        /// <summary>
        ///     Initializes a new instance of the <see cref="PostgreSqlDatabaseFactory" />.
        /// </summary>
        /// <remarks>Used by the other ctor and in tests.</remarks>
        public PostgreSqlDatabaseFactory(
            ILogger<PostgreSqlDatabaseFactory> logger,
            ILoggerFactory loggerFactory,
            IOptions<GlobalSettings> globalSettings,
            IOptionsMonitor<ConnectionStrings> connectionStrings,
            IMapperCollection mappers,
            IDbProviderFactoryCreator dbProviderFactoryCreator,
            DatabaseSchemaCreatorFactory databaseSchemaCreatorFactory,
            NPocoMapperCollection npocoMappers)
            : base(logger, loggerFactory, globalSettings, connectionStrings, mappers, dbProviderFactoryCreator, databaseSchemaCreatorFactory, npocoMappers)
        {
            _databaseSchemaCreatorFactory = databaseSchemaCreatorFactory ??
                                            throw new ArgumentNullException(nameof(databaseSchemaCreatorFactory));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _loggerFactory = loggerFactory;

            ConnectionStrings umbracoConnectionString = connectionStrings.CurrentValue;
            if (umbracoConnectionString.ProviderName.IsNullOrWhiteSpace())
            {
                umbracoConnectionString.ProviderName = Constants.ProviderName;
            }

            if (!umbracoConnectionString.IsConnectionStringConfigured())
            {
                if (_logger.IsEnabled(LogLevel.Debug))
                {
                    logger.LogDebug("Missing connection string, defer configuration.");
                }
                return; // not configured
            }

            Configure(umbracoConnectionString);
        }

        protected override UmbracoDatabase? CreateDatabaseInstance()
        {
            if (ProviderName != Constants.ProviderName)
            {
                return base.CreateDatabaseInstance();
            }

            if (ConnectionString is null || SqlContext is null || DbProviderFactory is null)
            {
                return null;
            }

            return new PostgreSqlDatabase(
                ConnectionString,
                SqlContext,
                DbProviderFactory,
                _loggerFactory.CreateLogger<PostgreSqlDatabase>(),
                _bulkSqlInsertProvider,
                _databaseSchemaCreatorFactory,
                _pocoMappers);
        }
    }
}
