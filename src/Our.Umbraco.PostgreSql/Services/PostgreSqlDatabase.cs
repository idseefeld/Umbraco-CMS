using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Diagnostics.Metrics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using NPoco;
using Umbraco.Cms.Infrastructure.Migrations.Install;
using Umbraco.Cms.Infrastructure.Persistence;

namespace Our.Umbraco.PostgreSql.Services
{
    public class PostgreSqlDatabase : UmbracoDatabase
    {
        // private static Dictionary<string, long> _lastInsertIds = new Dictionary<string, long>();

        private readonly ILogger<PostgreSqlDatabase> _logger;
        private readonly IBulkSqlInsertProvider? _bulkSqlInsertProvider;
        private readonly DatabaseSchemaCreatorFactory? _databaseSchemaCreatorFactory;
        private readonly IEnumerable<IMapper>? _mapperCollection;
        private readonly Guid _instanceGuid = Guid.NewGuid();
        private List<CommandInfo>? _commands;


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
            _bulkSqlInsertProvider = bulkSqlInsertProvider;
            _databaseSchemaCreatorFactory = databaseSchemaCreatorFactory;
            _mapperCollection = mapperCollection;
        }
        public new bool InTransaction { get; private set; }
        protected override void OnAbortTransaction()
        {
            InTransaction = false;

            // base.OnAbortTransaction();
            OnCompleteTransaction();
        }

        public override object Insert<T>(string tableName, string primaryKeyName, bool autoIncrement, T poco)
        {
            PocoData pocoData = PocoDataFactory.ForObject(poco, primaryKeyName, autoIncrement);
            if (autoIncrement)
            {
                string[] noAutoIncrementTableNames = Constants.NoAutoIncrementTableNames.Split(',');

                if (noAutoIncrementTableNames.Contains(pocoData.TableInfo.TableName))
                {
                    autoIncrement = false;
                }
            }
            else
            {
                _logger.LogDebug("Inserting into \"{TableName}\" without auto-increment", tableName);
            }

            if (primaryKeyName.Contains(',') || primaryKeyName == "ID")
            {
#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.
                // NPoco Insert for PostgreSQL only returns nothing when primaryKey is null
                return base.Insert(tableName, null, autoIncrement, poco);
#pragma warning restore CS8625 // Cannot convert null literal to non-nullable reference type.
            }

            return base.Insert(tableName, primaryKeyName, autoIncrement, poco);
        }

        /// <summary>
        /// Inserts a collection of objects into the database in bulk, using the specified options.
        /// </summary>
        /// <typeparam name="T">The type of objects to insert. Must be compatible with the database schema.</typeparam>
        /// <param name="pocos">The collection of objects to be inserted. Cannot be null.</param>
        /// <param name="options">Optional settings that control bulk insert behavior, such as batching or transaction options. If null,
        /// default options are used.</param>
        public new void InsertBulk<T>(IEnumerable<T> pocos, InsertBulkOptions? options = null)
        {
            if (options?.BulkCopyTimeout is not null)
            {
                // throw new NotSupportedException("The BulkCopyTimeout option is not supported for PostgreSQL bulk inserts.");
            }

            base.BulkInsertRecords<T>(pocos);
        }

        //public void ExecuteInsert(string sql, params object[] args)
        //{
        //    _commands ??= new List<CommandInfo>();
        //    var cmd = CreateCommand(sql, args);
        //    _commands.Add(cmd);
        //    Execute(cmd);
        //}

        protected override void OnExecutingCommand(DbCommand cmd)
        {
            base.OnExecutingCommand(cmd);
        }

        /// <inheritdoc cref="Database.ExecuteScalar{T}(string,object[])" />
        public new T ExecuteScalar<T>(string sql, params object[] args)
            => ExecuteScalar<T>(new Sql(sql, args));

        /// <inheritdoc cref="Database.ExecuteScalar{T}(Sql)" />
        public new T ExecuteScalar<T>(Sql sql)
            => ExecuteScalar<T>(sql.SQL, CommandType.Text, sql.Arguments);

        /// <inheritdoc cref="Database.ExecuteScalar{T}(string,CommandType,object[])" />
        /// <remarks>
        ///     Be nice if handled upstream <a href="https://github.com/schotime/NPoco/issues/653">GH issue</a>
        /// </remarks>
        public new T ExecuteScalar<T>(string sql, CommandType commandType, params object[] args)
        {
            if (SqlContext.SqlSyntax.ScalarMappers == null)
            {
                return base.ExecuteScalar<T>(sql, commandType, args);
            }

            if (!SqlContext.SqlSyntax.ScalarMappers.TryGetValue(typeof(T), out IScalarMapper? mapper))
            {
                return base.ExecuteScalar<T>(sql, commandType, args);
            }

            var result = base.ExecuteScalar<object>(sql, commandType, args);
            return (T)mapper.Map(result);
        }
    }
}
