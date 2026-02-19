// Copyright (c) Umbraco.
// See LICENSE for more details.

using System.Data;
using System.Diagnostics.CodeAnalysis;
using NPoco;
using NUnit.Framework;
using Umbraco.Cms.Core.Persistence;
using Umbraco.Cms.Infrastructure.Persistence;
using Umbraco.Cms.Infrastructure.Persistence.DatabaseModelDefinitions;
using Umbraco.Cms.Infrastructure.Persistence.Dtos;
using Umbraco.Cms.Infrastructure.Persistence.SqlSyntax;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Infrastructure.Persistence.SqlSyntax;

[TestFixture]
public class SqlSyntaxProviderBaseTests
{
    /// <summary>
    ///     Minimal concrete implementation of <see cref="SqlSyntaxProviderBase{TSyntax}"/> used to
    ///     test the base-class <c>GetFieldName</c> logic without depending on any specific database provider.
    /// </summary>
    private sealed class TestSqlSyntaxProvider : SqlSyntaxProviderBase<TestSqlSyntaxProvider>
    {
        public override string ProviderName => "Test";

        public override string DbProvider => "Test";

        public override IsolationLevel DefaultIsolationLevel => IsolationLevel.ReadCommitted;

        public override IEnumerable<Tuple<string, string, string, bool>> GetDefinedIndexes(IDatabase db) =>
            Enumerable.Empty<Tuple<string, string, string, bool>>();

        public override bool TryGetDefaultConstraint(IDatabase db, string? tableName, string columnName, [MaybeNullWhen(false)] out string constraintName)
        {
            constraintName = null;
            return false;
        }

        public override Sql<ISqlContext>.SqlJoinClause<ISqlContext> LeftJoinWithNestedJoin<TDto>(
            Sql<ISqlContext> sql,
            Func<Sql<ISqlContext>, Sql<ISqlContext>> nestedJoin,
            string? alias = null) =>
            throw new NotImplementedException();

        public override Sql<ISqlContext> SelectTop(Sql<ISqlContext> sql, int top) =>
            throw new NotImplementedException();

        public override void HandleCreateTable(IDatabase database, TableDefinition tableDefinition, bool skipKeysAndIndexes = false) =>
            throw new NotImplementedException();

        protected override string? FormatSystemMethods(SystemMethods systemMethod) => null;

        protected override string FormatIdentity(ColumnDefinition column) => string.Empty;
    }

    private TestSqlSyntaxProvider SyntaxProvider => new();

    private string QuotedField(string tableName, string columnName) =>
        SyntaxProvider.GetQuotedTableName(tableName) + "." + SyntaxProvider.GetQuotedColumnName(columnName);

    [Test]
    public void Returns_Quoted_TableName_And_ColumnName()
    {
        // NodeDto has [TableName("umbracoNode")] and NodeId has [Column("id")]
        var result = SyntaxProvider.GetFieldName<NodeDto>(x => x.NodeId);

        Assert.AreEqual(QuotedField(NodeDto.TableName, NodeDto.IdColumnName), result);
    }

    [Test]
    public void Uses_Column_Attribute_Name_When_Different_From_Property_Name()
    {
        // NodeDto.UniqueId has [Column("uniqueId")]
        var result = SyntaxProvider.GetFieldName<NodeDto>(x => x.UniqueId);

        Assert.AreEqual(QuotedField(NodeDto.TableName, NodeDto.KeyColumnName), result);
    }

    [Test]
    public void Uses_Column_Attribute_Name_Matching_Property_Name()
    {
        // NodeDto.Path has [Column("path")]
        var result = SyntaxProvider.GetFieldName<NodeDto>(x => x.Path);

        Assert.AreEqual(QuotedField(NodeDto.TableName, NodeDto.PathColumnName), result);
    }

    [Test]
    public void Uses_TableAlias_When_Provided()
    {
        var result = SyntaxProvider.GetFieldName<NodeDto>(x => x.NodeId, "myAlias");

        Assert.AreEqual(QuotedField("myAlias", NodeDto.IdColumnName), result);
    }

    [Test]
    public void Uses_TableName_When_Alias_Is_Null()
    {
        var result = SyntaxProvider.GetFieldName<NodeDto>(x => x.NodeId, null);

        Assert.AreEqual(QuotedField(NodeDto.TableName, NodeDto.IdColumnName), result);
    }

    [Test]
    public void Handles_Nullable_Value_Type_Property()
    {
        // NodeDto.NodeObjectType is Guid? with [Column("nodeObjectType")]
        var result = SyntaxProvider.GetFieldName<NodeDto>(x => x.NodeObjectType);

        Assert.AreEqual(QuotedField(NodeDto.TableName, NodeDto.NodeObjectTypeColumnName), result);
    }

    [Test]
    public void Handles_Boolean_Property()
    {
        // NodeDto.Trashed has [Column("trashed")]
        var result = SyntaxProvider.GetFieldName<NodeDto>(x => x.Trashed);

        Assert.AreEqual(QuotedField(NodeDto.TableName, NodeDto.TrashedColumnName), result);
    }

    [Test]
    public void Handles_DateTime_Property()
    {
        // NodeDto.CreateDate has [Column("createDate")]
        var result = SyntaxProvider.GetFieldName<NodeDto>(x => x.CreateDate);

        Assert.AreEqual(QuotedField(NodeDto.TableName, NodeDto.CreateDateColumnName), result);
    }

    [Test]
    public void Handles_Short_Property()
    {
        // NodeDto.Level has [Column("level")]
        var result = SyntaxProvider.GetFieldName<NodeDto>(x => x.Level);

        Assert.AreEqual(QuotedField(NodeDto.TableName, NodeDto.LevelColumnName), result);
    }

    [Test]
    public void Throws_When_Expression_Selects_Field_Instead_Of_Property()
    {
        // When the expression resolves to a field (not a property), GetColumnName receives null
        // and should throw an ArgumentException.
        Assert.Throws<ArgumentException>(() =>
            SyntaxProvider.GetFieldName<DtoWithField>(x => x.SomeField));
    }

    /// <summary>
    ///     A DTO that exposes a public field instead of a property, causing <c>FindProperty</c>
    ///     to return a <see cref="System.Reflection.FieldInfo"/> which fails the <c>as PropertyInfo</c> cast.
    /// </summary>
    [NPoco.TableName("testTable")]
    private class DtoWithField
    {
#pragma warning disable SA1401 // Fields should be private - intentionally public for test
#pragma warning disable IDE1006 // Naming Styles
        public object? SomeField = null;
#pragma warning restore IDE1006 // Naming Styles
#pragma warning restore SA1401
    }
}
