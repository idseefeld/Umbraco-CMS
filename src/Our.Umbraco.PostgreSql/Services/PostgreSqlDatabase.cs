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

        //public new bool InTransaction { get; private set; }
        //protected override void OnAbortTransaction()
        //{
        //    InTransaction = false;

        //    // base.OnAbortTransaction();
        //    OnCompleteTransaction();
        //}

        public override object Insert<T>(string tableName, string primaryKeyName, bool autoIncrement, T poco)
        {
            autoIncrement = ValidateAutoIncrement(tableName, primaryKeyName, autoIncrement, poco);

#pragma warning disable CS8604 // Possible null reference argument.
            return base.Insert(tableName, FixPrimaryKey(primaryKeyName), autoIncrement, poco);
#pragma warning restore CS8604 // Possible null reference argument.
        }

        public override async Task<object> InsertAsync<T>(string tableName, string primaryKeyName, bool autoIncrement, T poco, CancellationToken cancellationToken = default)
        {
            autoIncrement = ValidateAutoIncrement(tableName, primaryKeyName, autoIncrement, poco);

#pragma warning disable CS8604 // Possible null reference argument.
            return await base.InsertAsync<T>(tableName, FixPrimaryKey(primaryKeyName), autoIncrement, poco, cancellationToken);
#pragma warning restore CS8604 // Possible null reference argument.
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
    }
}
