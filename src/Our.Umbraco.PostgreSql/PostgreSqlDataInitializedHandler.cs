using Microsoft.Extensions.Logging;
using Our.Umbraco.PostgreSql.Services;
using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Notifications;
using Umbraco.Cms.Infrastructure.Migrations.Notifications;
using Umbraco.Cms.Infrastructure.Persistence;
using Umbraco.Cms.Infrastructure.Persistence.SqlSyntax;

namespace Our.Umbraco.PostgreSql
{
    public class PostgreSqlDataInitializedHandler : INotificationAsyncHandler<DatabaseSchemaInitializedNotification>
    {
        private readonly Dictionary<string, long> _lastInsertIds = new Dictionary<string, long>();
        private readonly ILogger<PostgreSqlDataInitializedHandler> _logger;
        private readonly ISqlSyntaxProvider _syntaxProvider;

        public PostgreSqlDataInitializedHandler(ILogger<PostgreSqlDataInitializedHandler> logger, ISqlSyntaxProvider syntaxProvider)
        {
            _logger = logger;
            _syntaxProvider = syntaxProvider;
        }

        public async Task HandleAsync(DatabaseSchemaInitializedNotification notification, CancellationToken cancellationToken)
        {
            if (notification.Database.SqlContext?.SqlSyntax is not PostgreSqlSyntaxProvider)
            {
                return;
            }

            _syntaxProvider.AlterSequences(notification.Database);
        }

        private void AlterSequences(IUmbracoDatabase database)
        {
            var tablesToAlter = new Dictionary<string, string>
            {
                {"cmsContentType","pk"},//#
                {"cmsPropertyType","id"},//#
                {"cmsPropertyTypeGroup","id"},//#
                {"umbracoLanguage","id"},//#
                {"umbracoUser","id"},//#
                {"umbracoUserGroup","id"},//#
                {"umbracoNode","id"},//#
                {"umbracoUserStartNode","id"},

                {"cmsDictionary","pk"},
                {"cmsLanguageText","pk"},
                {"cmsMemberType","pk"},
                {"cmsTags","id"},
                {"cmsTemplate","pk"},
                {"umbracoAudit","id"},
                {"umbracoCacheInstruction","id"},
                {"umbracoConsent","id"},
                {"umbracoContentVersionCultureVariation","id"},
                {"umbracoContentVersion","id"},
                {"umbracoCreatedPackageSchema","id"},
                {"umbracoDocumentCultureVariation","id"},
                {"umbracoDocumentUrl","id"},
                {"umbracoDomain","id"},
                {"umbracoExternalLogin","id"},
                {"umbracoExternalLoginToken","id"},
                {"umbracoLogViewerQuery","id"},
                {"umbracoLog","id"},
                {"umbracoPropertyData","id"},
                {"umbracoRelation","id"},
                {"umbracoRelationType","id"},
                {"umbracoServer","id"},
                {"umbracoTwoFactorLogin","id"},
                {"umbracoUserGroup2GranularPermission","id"},
                {"umbracoUserGroup2Permission","id"},
                {"umbracoWebhook","id"},
                {"umbracoWebhookLog","id"},
                {"umbracoWebhookRequest","id"},
            };
            if (_lastInsertIds.Count < tablesToAlter.Count)
            {
                _logger.LogDebug("Altering sequences for PostgreSQL database after schema and data creation.");

                foreach (var table in tablesToAlter)
                {
                    AlterSequence(database, table.Key, table.Value);
                }
            }
        }


        private void AlterSequence(IUmbracoDatabase database, string tableName, string primaryKeyName)
        {
            ISqlContext? sqlContext = database.SqlContext;
            if (sqlContext is null)
            {
                _logger.LogWarning("No ambient scope or SQL context available, cannot alter sequences.");
                return;
            }

            ISqlSyntaxProvider sqlSyntax = sqlContext.SqlSyntax;
            var quotedId = sqlSyntax.GetQuotedColumnName(primaryKeyName);
            var quotedTable = sqlSyntax.GetQuotedTableName(tableName);

            string seqName = sqlSyntax.GetQuotedTableName($"{tableName}_{primaryKeyName}_seq");
            try
            {
                var maxIdSql = $"SELECT MAX({quotedId}) FROM {quotedTable}";
                long maxId = database.ExecuteScalar<long>(maxIdSql);
                _lastInsertIds[seqName] = maxId;
                if (maxId > 0)
                {
                    var alterSeqSql = $"ALTER SEQUENCE {seqName} RESTART WITH {maxId + 1}";
                    _logger.LogDebug("Identity sequence updated: {alterSeqSql}", alterSeqSql);
                    database.Execute(alterSeqSql);
                }
            }
            catch (Exception ex)
            {
                var msg = ex.Message;
                _logger.LogError(ex, "Error updating sequence for {TableName}.{PrimaryKeyName}", tableName, primaryKeyName);
            }
        }

    }
}
