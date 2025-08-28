using System;
using System.Collections.Generic;
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

            //if (autoIncrement && IsUmbracoInstalled())
            //{
            //    var quotedId = SqlContext.SqlSyntax.GetQuotedColumnName(primaryKeyName);
            //    var quotedTable = SqlContext.SqlSyntax.GetQuotedTableName(tableName);
            //    _logger.LogDebug("Inserting into {TableName} with auto-incrementand sequence update.", quotedTable);

            //    string seqName = $"{tableName}_{primaryKeyName}_seq";
            //    try
            //    {
            //        if (_lastInsertIds.TryGetValue(seqName, out long currentSeqVal) is false)
            //        {
            //            var maxIdSql = $"SELECT MAX({quotedId}) FROM {quotedTable}";
            //            long maxId = ExecuteScalar<long>(maxIdSql);
            //            _lastInsertIds[seqName] = maxId;
            //            if (maxId > 0)
            //            {
            //                var alterSeqSql = $"ALTER SEQUENCE \"{seqName}\" RESTART WITH {maxId + 1}";
            //                Execute(alterSeqSql);
            //            }
            //        }
            //        else
            //        {
            //            var maxIdSql = $"SELECT MAX({quotedId}) FROM {quotedTable}";
            //            long maxId = ExecuteScalar<long>(maxIdSql);
            //            _lastInsertIds[seqName] = maxId;
            //            if (maxId > currentSeqVal)
            //            {
            //                _lastInsertIds[seqName] = maxId;
            //                var alterSeqSql = $"ALTER SEQUENCE \"{seqName}\" RESTART WITH {maxId + 1}";
            //                Execute(alterSeqSql);
            //            }
            //        }
            //    }
            //    catch (Exception ex)
            //    {
            //        var msg = ex.Message;
            //        _logger.LogError(ex, "Error updating sequence for {TableName}.{PrimaryKeyName}", tableName, primaryKeyName);
            //    }
            //}

            return base.Insert(tableName, primaryKeyName, autoIncrement, poco);
        }
    }
}
