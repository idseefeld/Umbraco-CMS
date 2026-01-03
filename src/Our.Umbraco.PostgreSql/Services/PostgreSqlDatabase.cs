using System.Data;
using System.Data.Common;
using Microsoft.Extensions.Logging;
using NPoco;
using Umbraco.Cms.Core.Semver;
using Umbraco.Cms.Infrastructure.Migrations.Install;
using Umbraco.Cms.Infrastructure.Persistence;
using Umbraco.Extensions;

namespace Our.Umbraco.PostgreSql.Services
{
    public class PostgreSqlDatabase : UmbracoDatabase
    {
#pragma warning disable CS8604 // Possible null reference argument.
        private readonly ILogger<PostgreSqlDatabase> _logger;

        /// <summary>
        ///     Initializes a new instance of the <see cref="PostgreSqlDatabase" /> class.
        /// </summary>
        /// <remarks>
        ///     <para>Used by PostgreSqlDatabaseFactory to create databases.</para>
        ///     <para>Also used by DatabaseBuilder for creating databases and installing/upgrading.</para>
        /// </remarks>
        public PostgreSqlDatabase(
            string connectionString,
            ISqlContext sqlContext,
            DbProviderFactory provider,
            ILogger<PostgreSqlDatabase> logger,
            IBulkSqlInsertProvider? bulkSqlInsertProvider,
            DatabaseSchemaCreatorFactory databaseSchemaCreatorFactory,
            IEnumerable<IMapper>? mapperCollection = null)
            : base(connectionString, sqlContext, provider, logger, bulkSqlInsertProvider, databaseSchemaCreatorFactory, mapperCollection)
        {
            _logger = logger;
        }

        public override object Insert<T>(string tableName, string primaryKeyName, bool autoIncrement, T poco)
        {
            autoIncrement = ValidateAutoIncrement(tableName, primaryKeyName, autoIncrement, poco);

            return base.Insert(tableName, FixPrimaryKey(primaryKeyName), autoIncrement, poco);
        }

        public override async Task<object> InsertAsync<T>(string tableName, string primaryKeyName, bool autoIncrement, T poco, CancellationToken cancellationToken = default)
        {
            autoIncrement = ValidateAutoIncrement(tableName, primaryKeyName, autoIncrement, poco);

            return await base.InsertAsync<T>(tableName, FixPrimaryKey(primaryKeyName), autoIncrement, poco, cancellationToken);
        }

        /// <summary>
        /// Retrieves the semantic version of the database server from the specified connection.
        /// </summary>
        /// <remarks>The method attempts to parse the <see cref="DbConnection.ServerVersion"/> property as
        /// a semantic version. If the server version string does not conform to semantic versioning, the method returns
        /// <see langword="null"/>.</remarks>
        /// <param name="connection">The database connection from which to obtain the server version. Must not be null and must be open.</param>
        /// <returns>A <see cref="SemVersion"/> representing the server's semantic version if parsing succeeds; otherwise, <see
        /// langword="null"/>.</returns>
        public static SemVersion? GetServerVersion(DbConnection connection)
        {
            SemVersion.TryParse(connection?.ServerVersion, out SemVersion? semver);
            return semver;
        }

        /// <inheritdoc />
        public override DbCommand CreateCommand(DbConnection connection, CommandType commandType, string sql, params object[] args)
        {
            foreach (var arg in args)
            {
                if (arg is DateTime dt)
                {
                    if (dt.Kind == DateTimeKind.Unspecified)
                    {
                        args.Replace(arg, dt.ToLocalTime().ToUniversalTime());
                    }
                    else if (dt.Kind == DateTimeKind.Local)
                    {
                        args.Replace(arg, dt.ToUniversalTime());
                    }
                }
            }

            return base.CreateCommand(connection, commandType, sql, args);
        }

        private static string? FixPrimaryKey(string primaryKeyName)
        {
            if (primaryKeyName == null || primaryKeyName.Contains(',') || primaryKeyName == "ID")
            {
                return null;
            }

            return primaryKeyName;
        }

        private bool ValidateAutoIncrement<T>(string tableName, string primaryKeyName, bool autoIncrement, T poco)
        {
            PocoData pocoData = PocoDataFactory.ForObject(poco, primaryKeyName, autoIncrement);
            if (autoIncrement != pocoData.TableInfo.AutoIncrement)
            {
                _logger.LogDebug("AutoIncrement mismatch for \"{TableName}\": method parameter is {MethodAutoIncrement} but PocoData is {PocoDataAutoIncrement}", tableName, autoIncrement, pocoData.TableInfo.AutoIncrement);
            }

            return pocoData.TableInfo.AutoIncrement;
        }
#pragma warning restore CS8604 // Possible null reference argument.
    }
}
