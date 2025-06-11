using Our.Umbraco.PostgreSql.Mappers;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NPoco;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Infrastructure.Persistence;
using Umbraco.Cms.Infrastructure.Persistence.DatabaseModelDefinitions;

namespace Our.Umbraco.PostgreSql.Services;

/// <summary>
///     Represents a SqlSyntaxProvider for PostgreSQL.
/// </summary>
public class PostgreSqlSyntaxProvider : NpgsqlSqlSyntaxProvider<PostgreSqlSyntaxProvider> // SqlSyntaxProviderBase<PostgreSqlSyntaxProvider>
{
    private readonly IOptions<GlobalSettings> _globalSettings;
    private readonly ILogger<PostgreSqlSyntaxProvider> _logger;
    private readonly IDictionary<Type, IScalarMapper> _scalarMappers;


    public PostgreSqlSyntaxProvider(IOptions<GlobalSettings> globalSettings, ILogger<PostgreSqlSyntaxProvider> logger)
    {
        _globalSettings = globalSettings;
        _logger = logger;

        _scalarMappers = new Dictionary<Type, IScalarMapper>
        {
            [typeof(Guid)] = new PostgreSqlGuidScalarMapper(),
            [typeof(Guid?)] = new PostgreSqlNullableGuidScalarMapper(),
            [typeof(DateTime)] = new PostgreSqlDateTimeScalarMapper(),
            [typeof(DateTime?)] = new PostgreSqlNullableDateTimeScalarMapper(),
        };
    }

    public override DatabaseType GetUpdatedDatabaseType(DatabaseType current, string? connectionString)
    {
        // For PostgreSQL, just return PostgreSQL type
        return DatabaseType.PostgreSQL;
    }

    public override void HandleCreateTable(IDatabase database, TableDefinition tableDefinition, bool skipKeysAndIndexes = false)
    {
        // Format columns, primary key, and foreign keys
        var columns = Format(tableDefinition.Columns);
        var primaryKey = FormatPrimaryKey(tableDefinition);

        var sb = new System.Text.StringBuilder();
        _ = sb.AppendLine($"CREATE TABLE {GetQuotedTableName(tableDefinition.Name)}")
            .AppendLine("(")
            .Append(columns);

        // Add primary key if present and not skipping keys
        if (!string.IsNullOrEmpty(primaryKey) && !skipKeysAndIndexes)
        {
            _ = sb.AppendLine($",\n {primaryKey}");
        }

        var createSql = sb
            .AppendLine(")")
            .ToString();

        _logger.LogInformation("Create table:\n {Sql}", createSql);
        _ = database.Execute(new Sql(createSql));

        if (skipKeysAndIndexes)
        {
            return;
        }

        // Create indexes
        List<string> indexSql = Format(tableDefinition.Indexes);
        foreach (var sql in indexSql)
        {
            _logger.LogDebug("Create Index:\n {Sql}", sql);
            _ = database.Execute(new Sql(sql));
        }

        // Create foreign keys
        List<string> foreignKeysSql = Format(tableDefinition.ForeignKeys);
        foreach (var sql in foreignKeysSql)
        {
            _logger.LogDebug("Create Index:\n {Sql}", sql);
            _ = database.Execute(new Sql(sql));
        }
    }
}
