using System;
using System.Collections.Generic;
using System.Data.Common;
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
            //base.OnAbortTransaction();
            base.OnCompleteTransaction();
        }
        public override object Insert<T>(string tableName, string primaryKeyName, bool autoIncrement, T poco)
        {
            PocoData pocoData = PocoDataFactory.ForObject(poco, primaryKeyName, autoIncrement);
            if (autoIncrement)
            {
                string[] noAutoIncrementTableNames = [
                    "cmsContentNu",
                    "cmsContentType2ContentType",
                    "cmsContentTypeAllowedContentType",
                    "cmsDocumentType",
                    "cmsMember2MemberGroup",
                    "cmsTagRelationship",
                    "umbracoUser2ClientId",
                    "umbracoUser2NodeNotify",
                    "umbracoUser2UserGroup",
                    "umbracoUserGroup2App",
                    "umbracoUserGroup2Language",
                    "umbracoWebhook2ContentTypeKeys",
                    "umbracoWebhook2Events",
                    "umbracoWebhook2Headers"
                    ];
                if (noAutoIncrementTableNames.Contains(pocoData.TableInfo.TableName))
                {
                    autoIncrement = false;
                }
            }

            return base.Insert(tableName, primaryKeyName, autoIncrement, poco);
        }
    }
}
