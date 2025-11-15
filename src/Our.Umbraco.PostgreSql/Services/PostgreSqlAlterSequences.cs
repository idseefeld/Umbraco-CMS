using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Extensions.Logging;
using NPoco;
using Org.BouncyCastle.Bcpg.OpenPgp;
using Umbraco.Cms.Infrastructure.Persistence;
using Umbraco.Cms.Infrastructure.Persistence.SqlSyntax;
using static Umbraco.Cms.Core.Constants;

namespace Our.Umbraco.PostgreSql.Services
{
    public class PostgreSqlAlterSequences : IPostgreSqlAlterSequences
    {
        private readonly Dictionary<string, long> _lastInsertIds = new Dictionary<string, long>();
        // private readonly ILogger<PostgreSqlAlterSequences> _logger;
        private IUmbracoDatabase? _database = null;
        private ISqlContext? _sqlContext = null;

        public PostgreSqlAlterSequences() // ILogger<PostgreSqlAlterSequences> logger)
        {
            // _logger = logger;

            // _database = database;
            // if (_database is PostgreSqlDatabase)
            // {
            //    _sqlContext = database.SqlContext;
            // }
        }

        public void AlterSequences()
        {
            if (_database is not PostgreSqlDatabase database)
            {
                //_logger.LogWarning("No database instance available, cannot alter sequences.");
                return;
            }

            var tablesToAlter = new Dictionary<string, string>
            {
                {"cmsContentType","pk"},
                {"cmsDictionary","pk"},//
                {"cmsLanguageText","pk"},//
                {"cmsMemberType","pk"},//
                {"cmsPropertyType","id"},
                {"cmsPropertyTypeGroup","id"},
                {"cmsTags","id"},//
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
            if (_lastInsertIds.Count < tablesToAlter.Count)
            {
                //_logger.LogDebug("Altering sequences for PostgreSQL database after schema and data creation.");

                foreach (var table in tablesToAlter)
                {
                    AlterSequence(database, table.Key, table.Value);
                }
            }
        }

        private void AlterSequence(PostgreSqlDatabase database,string tableName, string primaryKeyName)
        {
            ISqlSyntaxProvider? sqlSyntax = _sqlContext?.SqlSyntax;
            if (sqlSyntax is null)
            {
                //_logger.LogWarning("No ambient scope or SQL context available, cannot alter sequences.");
                return;
            }

            var quotedId = sqlSyntax.GetQuotedColumnName(primaryKeyName);
            var quotedTable = sqlSyntax.GetQuotedTableName(tableName);

            string seqName = $"{tableName}_{primaryKeyName}_seq";
            try
            {
                var maxIdSql = $"SELECT MAX({quotedId}) FROM {quotedTable}";
                long maxId = database.ExecuteScalar<long>(maxIdSql);
                _lastInsertIds[seqName] = maxId;
                if (maxId > 0)
                {
                    var alterSeqSql = $"ALTER SEQUENCE \"{seqName}\" RESTART WITH {maxId + 1}";
                    //_logger.LogDebug("Identity sequence updated: {alterSeqSql}", alterSeqSql);
                    database.Execute(alterSeqSql);
                }
            }
            catch (Exception ex)
            {
                var msg = ex.Message;
                //_logger.LogError(ex, "Error updating sequence for {TableName}.{PrimaryKeyName}", tableName, primaryKeyName);
            }
        }
    }
}
