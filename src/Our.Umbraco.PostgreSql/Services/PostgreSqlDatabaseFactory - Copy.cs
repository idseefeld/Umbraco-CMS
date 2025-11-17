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
    public class PostgreSqlDatabaseFactory : IUmbracoDatabaseFactory
    {
        private readonly Dictionary<string, long> _lastInsertIds = new Dictionary<string, long>();

        private readonly DatabaseSchemaCreatorFactory _databaseSchemaCreatorFactory;
        private readonly IDbProviderFactoryCreator _dbProviderFactoryCreator;
        private readonly ILogger<PostgreSqlDatabaseFactory> _logger;
        private readonly ILoggerFactory _loggerFactory;
        private readonly IMapperCollection _mappers;
        private readonly NPocoMapperCollection _npocoMappers;
        private IBulkSqlInsertProvider? _bulkSqlInsertProvider;
        private DatabaseType? _databaseType;

        private DbProviderFactory? _dbProviderFactory;
        private bool _initialized;

        private object _lock = new();

        private DatabaseFactory? _npocoDatabaseFactory;
        private IPocoDataFactory? _pocoDataFactory;
        private NPoco.MapperCollection? _pocoMappers;
        private SqlContext _sqlContext = null!;
        private ISqlSyntaxProvider? _sqlSyntax;

        private ConnectionStrings? _umbracoConnectionString;
        private bool _upgrading;

        /// <summary>
        ///     Initializes a new instance of the <see cref="PostgreSqlDatabaseFactory" />.
        /// </summary>
        /// <remarks>Used by the other ctor and in tests.</remarks>
        public PostgreSqlDatabaseFactory(
            ILogger<PostgreSqlDatabaseFactory> logger,
            ILoggerFactory loggerFactory,
            IOptionsMonitor<ConnectionStrings> connectionStrings,
            IMapperCollection mappers,
            IDbProviderFactoryCreator dbProviderFactoryCreator,
            DatabaseSchemaCreatorFactory databaseSchemaCreatorFactory,
            NPocoMapperCollection npocoMappers)
        {
            _mappers = mappers ?? throw new ArgumentNullException(nameof(mappers));
            _dbProviderFactoryCreator = dbProviderFactoryCreator ??
                                        throw new ArgumentNullException(nameof(dbProviderFactoryCreator));
            _databaseSchemaCreatorFactory = databaseSchemaCreatorFactory ??
                                            throw new ArgumentNullException(nameof(databaseSchemaCreatorFactory));
            _npocoMappers = npocoMappers;
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _loggerFactory = loggerFactory;

            ConnectionStrings umbracoConnectionString = connectionStrings.CurrentValue;
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

        /// <summary>
        /// Creates and returns a new instance of an <see cref="IUmbracoDatabase"/>.
        /// </summary>
        /// <remarks>This method ensures that the database factory is properly initialized before creating
        /// the database instance.</remarks>
        /// <returns>An instance of <see cref="IUmbracoDatabase"/> representing the newly created database.</returns>
        public IUmbracoDatabase CreateDatabase()
        {
            // must be initialized to create a database
            EnsureInitialized();
            IUmbracoDatabase database = (IUmbracoDatabase)_npocoDatabaseFactory!.GetDatabase();

            if (database.IsUmbracoInstalled())
            {
                // this is true on every restart!
                // ToDo: How to handle this better? Only after inital install or upgrade?
            }

            return database;
        }

        private void EnsureInitialized() =>
            LazyInitializer.EnsureInitialized(ref _sqlContext, ref _initialized, ref _lock, Initialize);

        private SqlContext Initialize()
        {
            if (_logger.IsEnabled(LogLevel.Debug))
            {
                _logger.LogDebug("Initializing.");
            }

            if (ConnectionString.IsNullOrWhiteSpace())
            {
                throw new InvalidOperationException("The factory has not been configured with a proper connection string.");
            }

            if (ProviderName.IsNullOrWhiteSpace())
            {
                throw new InvalidOperationException("The factory has not been configured with a proper provider name.");
            }

            if (DbProviderFactory == null)
            {
                throw new Exception($"Can't find a provider factory for provider name \"{ProviderName}\".");
            }

            _databaseType = DatabaseType.Resolve(DbProviderFactory.GetType().Name, ProviderName);
            if (_databaseType == null)
            {
                throw new Exception($"Can't find an NPoco database type for provider name \"{ProviderName}\".");
            }

            _sqlSyntax = _dbProviderFactoryCreator.GetSqlSyntaxProvider(ProviderName!);
            if (_sqlSyntax == null)
            {
                throw new Exception($"Can't find a sql syntax provider for provider name \"{ProviderName}\".");
            }

            _bulkSqlInsertProvider = _dbProviderFactoryCreator.CreateBulkSqlInsertProvider(ProviderName!);

            _databaseType = _sqlSyntax.GetUpdatedDatabaseType(_databaseType, ConnectionString);

            // ensure we have only 1 set of mappers, and 1 PocoDataFactory, for all database
            // so that everything NPoco is properly cached for the lifetime of the application
            _pocoMappers = new NPoco.MapperCollection();

            // add all registered mappers for NPoco
            _pocoMappers.AddRange(_npocoMappers);

            _pocoMappers.AddRange(_dbProviderFactoryCreator.ProviderSpecificMappers(ProviderName!));

            var factory = new FluentPocoDataFactory(GetPocoDataFactoryResolver, _pocoMappers);
            _pocoDataFactory = factory;
            var config = new FluentConfig(xmappers => factory);

            // create the database factory
            _npocoDatabaseFactory = DatabaseFactory.Config(cfg =>
            {
                cfg.UsingDatabase(CreateDatabaseInstance) // creating IUmbracoDatabase instances
                    .WithFluentConfig(config); // with proper configuration

                foreach (IProviderSpecificInterceptor interceptor in _dbProviderFactoryCreator
                             .GetProviderSpecificInterceptors(ProviderName!))
                {
                    cfg.WithInterceptor(interceptor);
                }
            });

            if (_npocoDatabaseFactory == null)
            {
                throw new NullReferenceException(
                    "The call to UmbracoDatabaseFactory.Config yielded a null UmbracoDatabaseFactory instance.");
            }
            if (_logger.IsEnabled(LogLevel.Debug))
            {
                _logger.LogDebug("Initialized.");
            }

            return new SqlContext(_sqlSyntax, _databaseType, _pocoDataFactory, _mappers);
        }

        private InitializedPocoDataBuilder GetPocoDataFactoryResolver(Type type, IPocoDataFactory factory)
        => new PostgreSqlPocoDataBuilder(type, _pocoMappers, _upgrading).Init();

        private DbProviderFactory? DbProviderFactory
        {
            get
            {
                _dbProviderFactory ??= string.IsNullOrWhiteSpace(ProviderName)
                        ? null
                        : _dbProviderFactoryCreator.CreateFactory(ProviderName);

                return _dbProviderFactory;
            }
        }

        /// <inheritdoc />
        public bool Configured
        {
            get
            {
                lock (_lock)
                {
                    return !ConnectionString.IsNullOrWhiteSpace() && !ProviderName.IsNullOrWhiteSpace();
                }
            }
        }

        /// <inheritdoc />
        public bool Initialized => Volatile.Read(ref _initialized);

        /// <inheritdoc />
        public string? ConnectionString => _umbracoConnectionString?.ConnectionString;

        /// <inheritdoc />
        public string? ProviderName => _umbracoConnectionString?.ProviderName;

        /// <inheritdoc />
        public bool CanConnect =>
            // actually tries to connect to the database (regardless of configured/initialized)
            !ConnectionString.IsNullOrWhiteSpace() && !ProviderName.IsNullOrWhiteSpace() &&
            DbConnectionExtensions.IsConnectionAvailable(ConnectionString, DbProviderFactory);

        /// <inheritdoc />
        public ISqlContext SqlContext
        {
            get
            {
                // must be initialized to have a context
                EnsureInitialized();

                return _sqlContext;
            }
        }

        /// <inheritdoc />
        public IBulkSqlInsertProvider? BulkSqlInsertProvider
        {
            get
            {
                // must be initialized to have a bulk insert provider
                EnsureInitialized();

                return _bulkSqlInsertProvider;
            }
        }

        /// <inheritdoc />
        public void Configure(ConnectionStrings umbracoConnectionString)
        {
            if (umbracoConnectionString is null)
            {
                throw new ArgumentNullException(nameof(umbracoConnectionString));
            }

            lock (_lock)
            {
                if (Volatile.Read(ref _initialized))
                {
                    throw new InvalidOperationException("Already initialized.");
                }

                _umbracoConnectionString = umbracoConnectionString;
            }

            // rest to be lazy-initialized
        }

        /// <inheritdoc />
        public void ConfigureForUpgrade() => _upgrading = true;

        /// <inheritdoc />
        public void Dispose() { }

        private PostgreSqlDatabase? CreateDatabaseInstance()
        {
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
