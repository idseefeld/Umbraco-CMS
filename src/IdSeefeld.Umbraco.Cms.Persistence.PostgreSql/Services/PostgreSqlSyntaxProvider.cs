using System.Data;
using System.Data.Common;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Linq.Expressions;
using IdSeefeld.Umbraco.Cms.Persistence.PostgreSql.Dtos;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Npgsql;
using NPoco;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.Persistence;
using Umbraco.Cms.Infrastructure.Persistence;
using Umbraco.Cms.Infrastructure.Persistence.DatabaseAnnotations;
using Umbraco.Cms.Infrastructure.Persistence.DatabaseModelDefinitions;
using Umbraco.Cms.Infrastructure.Persistence.SqlSyntax;
using Umbraco.Extensions;
using ColumnInfo = Umbraco.Cms.Infrastructure.Persistence.SqlSyntax.ColumnInfo;

namespace IdSeefeld.Umbraco.Cms.Persistence.PostgreSql.Services;

/// <summary>
///     Represents a SqlSyntaxProvider for PostgreSQL.
/// </summary>
[DebuggerDisplay($"{{{nameof(GetDebuggerDisplay)}(),nq}}")]
public class PostgreSqlSyntaxProvider : SqlSyntaxProviderBase<PostgreSqlSyntaxProvider>
{
    private readonly IOptions<GlobalSettings> _globalSettings;
    private readonly ILogger<PostgreSqlSyntaxProvider> _logger;


    public PostgreSqlSyntaxProvider(IOptions<GlobalSettings> globalSettings, ILogger<PostgreSqlSyntaxProvider> logger)
    {
        _globalSettings = globalSettings;
        _logger = logger;

        GuidColumnDefinition = "UUID";
    }

    public override string ProviderName => Constants.ProviderName;

    public override string DbProvider => Constants.DbProvider;

    public override IsolationLevel DefaultIsolationLevel => IsolationLevel.ReadCommitted;

    public override string DeleteDefaultConstraint => "ALTER TABLE {0} ALTER COLUMN {1} DROP DEFAULT";

    public override string DropIndex => "DROP INDEX IF EXISTS {0}";

    public override string RenameColumn => "ALTER TABLE {0} RENAME COLUMN {1} TO {2}";

    public override string CreateIndex => "CREATE {0}{1}INDEX {2} ON {3} ({4}){5}";

    public override string StringLengthUnicodeColumnDefinitionFormat => base.StringLengthUnicodeColumnDefinitionFormat;

    public override string StringColumnDefinition
    {
        get
        {
            // PostgreSQL does not have a specific string type, it uses TEXT for variable-length strings
            return "TEXT";
        }
    }

    public override string CreateForeignKeyConstraint => base.CreateForeignKeyConstraint;

    public override string CreateDefaultConstraint => base.CreateDefaultConstraint;

    public override IDictionary<Type, IScalarMapper>? ScalarMappers => base.ScalarMappers;

    public override string Length => base.Length;

    public override string Substring => base.Substring;

    public override string CreateTable => base.CreateTable;

    public override string DropTable => base.DropTable;

    public override string AddColumn => base.AddColumn;

    public override string DropColumn => base.DropColumn;

    public override string AlterColumn => base.AlterColumn;

    public override string RenameTable => base.RenameTable;

    public override string CreateSchema => base.CreateSchema;

    public override string AlterSchema => base.AlterSchema;

    public override string DropSchema => base.DropSchema;

    public override string InsertData => base.InsertData;

    public override string UpdateData => base.UpdateData;

    public override string DeleteData => base.DeleteData;

    public override string TruncateTable => base.TruncateTable;

    public override string CreateConstraint => base.CreateConstraint;

    public override string DeleteConstraint => base.DeleteConstraint;

    public override string ConvertIntegerToOrderableString => base.ConvertIntegerToOrderableString;

    public override string ConvertDateToOrderableString => base.ConvertDateToOrderableString;

    public override string ConvertDecimalToOrderableString => base.ConvertDecimalToOrderableString;

    public override DatabaseType GetUpdatedDatabaseType(DatabaseType current, string? connectionString)
    {
        // For PostgreSQL, just return PostgreSQL type
        return DatabaseType.PostgreSQL;
    }

    public override IEnumerable<string> GetTablesInSchema(IDatabase db) =>
        db.Fetch<string>(
            "SELECT table_name FROM information_schema.tables WHERE table_schema = current_schema() AND table_type = 'BASE TABLE'");

    public override IEnumerable<ColumnInfo> GetColumnsInSchema(IDatabase db)
    {
        var items = db.Fetch<ColumnInSchemaDto>(
            @"SELECT table_name, column_name, ordinal_position, column_default, is_nullable, data_type
              FROM information_schema.columns
              WHERE table_schema = current_schema()");
        return items.Select(item =>
            new ColumnInfo(item.TableName, item.ColumnName, item.OrdinalPosition, item.ColumnDefault, item.IsNullable, item.DataType)).ToList();
    }

    public override IEnumerable<Tuple<string, string>> GetConstraintsPerTable(IDatabase db)
    {
        var items = db.Fetch<ConstraintPerTableDto>(
            @"SELECT table_name, constraint_name
              FROM information_schema.table_constraints
              WHERE table_schema = current_schema()");
        return items.Select(item => new Tuple<string, string>(item.TableName, item.ConstraintName)).ToList();
    }

    public override IEnumerable<Tuple<string, string, string>> GetConstraintsPerColumn(IDatabase db)
    {
        var items = db.Fetch<ConstraintPerColumnDto>(
            @"SELECT table_name, column_name, constraint_name
              FROM information_schema.constraint_column_usage
              WHERE table_schema = current_schema()");
        return items.Select(item =>
            new Tuple<string, string, string>(item.TableName, item.ColumnName, item.ConstraintName)).ToList();
    }

    public override IEnumerable<Tuple<string, string, string, bool>> GetDefinedIndexes(IDatabase db)
    {
        var items = db.Fetch<DefinedIndexDto>(
            @"SELECT
                t.relname AS table_name,
                i.relname AS index_name,
                a.attname AS column_name,
                ix.indisunique AS unique
            FROM
                pg_class t,
                pg_class i,
                pg_index ix,
                pg_attribute a
            WHERE
                t.oid = ix.indrelid
                AND i.oid = ix.indexrelid
                AND a.attrelid = t.oid
                AND a.attnum = ANY(ix.indkey)
                AND t.relkind = 'r'
                AND t.relnamespace = (SELECT oid FROM pg_namespace WHERE nspname = current_schema())
                AND ix.indisprimary = false
            ORDER BY t.relname, i.relname");
        return items.Select(item =>
            new Tuple<string, string, string, bool>(item.TableName, item.IndexName, item.ColumnName, item.Unique == 1)).ToList();
    }

    public override bool TryGetDefaultConstraint(IDatabase db, string? tableName, string columnName, [MaybeNullWhen(false)] out string constraintName)
    {
        // PostgreSQL does not use named default constraints like SQL Server.
        // Instead, we can check if a default exists for the column.
        var result = db.ExecuteScalar<string>(
            @"SELECT column_default
              FROM information_schema.columns
              WHERE table_name = @0 AND column_name = @1 AND table_schema = current_schema()",
            tableName,
            columnName);

        constraintName = !result.IsNullOrWhiteSpace() ? $"{tableName}_{columnName}_default" : null;
        return constraintName != null;
    }

    public override bool DoesPrimaryKeyExist(IDatabase db, string tableName, string primaryKeyName)
    {
        var result = db.ExecuteScalar<int>(
            @"SELECT COUNT(*)
              FROM information_schema.table_constraints
              WHERE table_name = @0 AND constraint_name = @1 AND constraint_type = 'PRIMARY KEY' AND table_schema = current_schema()",
            tableName, primaryKeyName);
        return result > 0;
    }

    public override bool DoesTableExist(IDatabase db, string tableName)
    {
        var result = db.ExecuteScalar<long>(
            @"SELECT COUNT(*)
              FROM information_schema.tables
              WHERE table_name = @0 AND table_schema = current_schema()",
            tableName);
        return result > 0;
    }

    public override string FormatColumnRename(string? tableName, string? oldName, string? newName) =>
        string.Format(RenameColumn, tableName, oldName, newName);

    public override string FormatTableRename(string? oldName, string? newName) =>
        $"ALTER TABLE {oldName} RENAME TO {newName}";

    protected override string FormatIdentity(ColumnDefinition column) =>
        column.IsIdentity ? "GENERATED BY DEFAULT AS IDENTITY" : string.Empty;

    public override Sql<ISqlContext> SelectTop(Sql<ISqlContext> sql, int top)
    {
        // PostgreSQL uses LIMIT
        return sql.Append($" LIMIT {top}");
    }

    protected override string? FormatSystemMethods(SystemMethods systemMethod)
    {
        return systemMethod switch
        {
            SystemMethods.NewGuid => "gen_random_uuid()",
            SystemMethods.CurrentDateTime => "CURRENT_TIMESTAMP",
            _ => null
        };
    }

    public override string Format(IndexDefinition index)
    {
        var name = string.IsNullOrEmpty(index.Name)
            ? $"IX_{index.TableName}_{index.ColumnName}"
            : index.Name;

        var columns = index.Columns.Any()
            ? string.Join(",", index.Columns.Select(x => GetQuotedColumnName(x.Name)))
            : GetQuotedColumnName(index.ColumnName);

        // PostgreSQL does not support INCLUDE columns in the same way as SQL Server
        var includeColumns = string.Empty;

        return string.Format(CreateIndex, GetIndexType(index.IndexType), " ", GetQuotedName(name), GetQuotedTableName(index.TableName), columns, includeColumns);
    }

    public override Sql<ISqlContext> InsertForUpdateHint(Sql<ISqlContext> sql)
    {
        // PostgreSQL uses FOR UPDATE
        return sql.Append(" FOR UPDATE");
    }

    public override Sql<ISqlContext> AppendForUpdateHint(Sql<ISqlContext> sql)
        => sql.Append(" FOR UPDATE ");

    public override Sql<ISqlContext>.SqlJoinClause<ISqlContext> LeftJoinWithNestedJoin<TDto>(
        Sql<ISqlContext> sql,
        Func<Sql<ISqlContext>, Sql<ISqlContext>> nestedJoin,
        string? alias = null)
    {
        Type type = typeof(TDto);

        var tableName = GetQuotedTableName(type.GetTableName());
        var join = tableName;

        if (alias != null)
        {
            var quotedAlias = GetQuotedTableName(alias);
            join += " " + quotedAlias;
        }

        var nestedSql = new Sql<ISqlContext>(sql.SqlContext);
        nestedSql = nestedJoin(nestedSql);

        Sql<ISqlContext>.SqlJoinClause<ISqlContext> sqlJoin = sql.LeftJoin(join);
        sql.Append(nestedSql);
        return sqlJoin;
    }

    private string? GetDebuggerDisplay()
    {
        return ToString();
    }

    public override bool Equals(object? obj) => base.Equals(obj);
    public override int GetHashCode() => base.GetHashCode();
    public override string? ToString() => base.ToString();
    public override string EscapeString(string val) => base.EscapeString(val);
    public override string GetStringColumnEqualComparison(string column, int paramIndex, TextColumnType columnType) => base.GetStringColumnEqualComparison(column, paramIndex, columnType);
    public override string GetStringColumnWildcardComparison(string column, int paramIndex, TextColumnType columnType) => base.GetStringColumnWildcardComparison(column, paramIndex, columnType);
    public override string GetConcat(params string[] args) => base.GetConcat(args);
    public override string GetQuotedTableName(string? tableName) => base.GetQuotedTableName(tableName);
    public override string GetQuotedColumnName(string? columnName) => base.GetQuotedColumnName(columnName);
    public override string GetQuotedName(string? name) => base.GetQuotedName(name);
    public override string GetQuotedValue(string value) => base.GetQuotedValue(value);
    public override string GetIndexType(IndexTypes indexTypes) => base.GetIndexType(indexTypes);
    public override string GetSpecialDbType(SpecialDbType dbType) => "TEXT";
    public override string GetColumn(DatabaseType dbType, string tableName, string columnName, string columnAlias, string? referenceName = null, bool forInsert = false) => base.GetColumn(dbType, tableName, columnName, columnAlias, referenceName, forInsert);
    public override string GetFieldNameForUpdate<TDto>(Expression<Func<TDto, object?>> fieldSelector, string? tableAlias = null) => base.GetFieldNameForUpdate(fieldSelector, tableAlias);
    public override bool SupportsClustered() => base.SupportsClustered();
    public override bool SupportsIdentityInsert() => base.SupportsIdentityInsert();
    public override string FormatDateTime(DateTime date, bool includeTime = true) => base.FormatDateTime(date, includeTime);
    public override string Format(TableDefinition table) => base.Format(table);
    public override List<string> Format(IEnumerable<IndexDefinition> indexes) => base.Format(indexes);
    public override List<string> Format(IEnumerable<ForeignKeyDefinition> foreignKeys) => base.Format(foreignKeys);
    public override string Format(ForeignKeyDefinition foreignKey) => base.Format(foreignKey);
    public override string Format(IEnumerable<ColumnDefinition> columns) => base.Format(columns);
    public override string Format(ColumnDefinition column) => base.Format(column);
    public override string Format(ColumnDefinition column, string tableName, out IEnumerable<string> sqls) => base.Format(column, tableName, out sqls);
    public override string FormatPrimaryKey(TableDefinition table) => base.FormatPrimaryKey(table);
    public override void HandleCreateTable(IDatabase database, TableDefinition tableDefinition, bool skipKeysAndIndexes = false)
    {
        // Format columns, primary key, and foreign keys
        var columns = Format(tableDefinition.Columns);
        var primaryKey = FormatPrimaryKey(tableDefinition);
        List<string> foreignKeys = Format(tableDefinition.ForeignKeys);

        var sb = new System.Text.StringBuilder();
        sb.AppendLine($"CREATE TABLE {GetQuotedTableName(tableDefinition.Name)}");
        sb.AppendLine("(");
        sb.Append(columns);

        // Add primary key if present and not skipping keys
        if (!string.IsNullOrEmpty(primaryKey) && !skipKeysAndIndexes)
        {
            sb.AppendLine($", {primaryKey}");
        }

        // Add foreign keys if not skipping keys
        if (!skipKeysAndIndexes)
        {
            foreach (var foreignKey in foreignKeys)
            {
                sb.AppendLine($", {foreignKey}");
            }
        }

        sb.AppendLine(")");

        var createSql = sb.ToString();

        _logger.LogInformation("Create table:\n {Sql}", createSql);
        database.Execute(new Sql(createSql));

        if (skipKeysAndIndexes)
        {
            return;
        }

        // Create indexes
        List<string> indexSql = Format(tableDefinition.Indexes);
        foreach (var sql in indexSql)
        {
            _logger.LogInformation("Create Index:\n {Sql}", sql);
            database.Execute(new Sql(sql));
        }
    }
    public override string GetSpecialDbType(SpecialDbType dbType, int customSize) => base.GetSpecialDbType(dbType, customSize);
    protected override string FormatCascade(string onWhat, Rule rule) => base.FormatCascade(onWhat, rule);
    protected override string FormatString(ColumnDefinition column) => base.FormatString(column);
    protected override string FormatType(ColumnDefinition column) => base.FormatType(column);
    protected override string FormatNullable(ColumnDefinition column) => base.FormatNullable(column);
    protected override string FormatConstraint(ColumnDefinition column) => base.FormatConstraint(column);
    protected override string FormatDefaultValue(ColumnDefinition column) => base.FormatDefaultValue(column);
    protected override string FormatPrimaryKey(ColumnDefinition column) => base.FormatPrimaryKey(column);
}
