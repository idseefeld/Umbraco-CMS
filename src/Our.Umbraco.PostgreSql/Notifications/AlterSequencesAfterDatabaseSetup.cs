using Microsoft.Extensions.Logging;
using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Infrastructure.Migrations.Install;
using Umbraco.Cms.Infrastructure.Migrations.Notifications;
using Umbraco.Cms.Infrastructure.Persistence;
using Umbraco.Cms.Infrastructure.Persistence.SqlSyntax;
using Umbraco.Cms.Infrastructure.Scoping;

namespace Our.Umbraco.PostgreSql.Notifications
{
    public class AlterSequencesAfterDatabaseSetup(IScopeAccessor scopeAccessor, ILogger<AlterSequencesAfterDatabaseSetup> logger) : INotificationHandler<UmbracoPlanExecutedNotification>
    {
        private static Dictionary<string, long> _lastInsertIds = new Dictionary<string, long>();

        public void Handle(UmbracoPlanExecutedNotification notification)
        {
            logger.LogDebug("Altering sequences for PostgreSQL database after schema and data creation.");

            var tablesToAlter = new Dictionary<string, string>
            {
                {"cmsContentType","pk"},
                {"cmsDictionary","pk"},
                {"cmsLanguageText","pk"},
                {"cmsMemberType","pk"},
                {"cmsPropertyType","id"},
                {"cmsPropertyTypeGroup","id"},
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
                {"umbracoLanguage","id"},
                {"umbracoLogViewerQuery","id"},
                {"umbracoLog","id"},
                {"umbracoNode","id"},
                {"umbracoPropertyData","id"},
                {"umbracoRelation","id"},
                {"umbracoRelationType","id"},
                {"umbracoServer","id"},
                {"umbracoTwoFactorLogin","id"},
                {"umbracoUser","id"},
                {"umbracoUserGroup","id"},
                {"umbracoUserGroup2GranularPermission","id"},
                {"umbracoUserGroup2Permission","id"},
                {"umbracoUserStartNode","id"},
                {"umbracoWebhook","id"},
                {"umbracoWebhookLog","id"},
                {"umbracoWebhookRequest","id"},
            };
            foreach (var table in tablesToAlter)
            {
                AlterSequence(table.Key, table.Value);
            }

        }

        private void AlterSequence(string tableName, string primaryKeyName)
        {
            ISqlContext? sqlContext = scopeAccessor.AmbientScope?.SqlContext;
            if (sqlContext is null)
            {
                logger.LogWarning("No ambient scope or SQL context available, cannot alter sequences.");
                return;
            }

            IUmbracoDatabase? database = scopeAccessor.AmbientScope?.Database;
            if (database is null)
            {
                logger.LogWarning("No ambient scope or database available, cannot alter sequences.");
                return;
            }

            ISqlSyntaxProvider sqlSyntax = sqlContext.SqlSyntax;
            var quotedId = sqlSyntax.GetQuotedColumnName(primaryKeyName);
            var quotedTable = sqlSyntax.GetQuotedTableName(tableName);
            logger.LogDebug("Inserting into {TableName} with auto-incrementand sequence update.", quotedTable);

            string seqName = $"{tableName}_{primaryKeyName}_seq";
            try
            {
                if (_lastInsertIds.TryGetValue(seqName, out long currentSeqVal) is false)
                {
                    var maxIdSql = $"SELECT MAX({quotedId}) FROM {quotedTable}";
                    long maxId = database.ExecuteScalar<long>(maxIdSql);
                    _lastInsertIds[seqName] = maxId;
                    if (maxId > 0)
                    {
                        var alterSeqSql = $"ALTER SEQUENCE \"{seqName}\" RESTART WITH {maxId + 1}";
                        database.Execute(alterSeqSql);
                    }
                }
                else
                {
                    var maxIdSql = $"SELECT MAX({quotedId}) FROM {quotedTable}";
                    long maxId = database.ExecuteScalar<long>(maxIdSql);
                    _lastInsertIds[seqName] = maxId;
                    if (maxId > currentSeqVal)
                    {
                        _lastInsertIds[seqName] = maxId;
                        var alterSeqSql = $"ALTER SEQUENCE \"{seqName}\" RESTART WITH {maxId + 1}";
                        database.Execute(alterSeqSql);
                    }
                }
            }
            catch (Exception ex)
            {
                var msg = ex.Message;
                logger.LogError(ex, "Error updating sequence for {TableName}.{PrimaryKeyName}", tableName, primaryKeyName);
            }
        }
    }
}
