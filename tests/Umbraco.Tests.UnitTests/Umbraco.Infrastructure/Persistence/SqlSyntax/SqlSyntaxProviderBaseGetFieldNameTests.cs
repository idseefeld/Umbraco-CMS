// Copyright (c) Umbraco.
// See LICENSE for more details.

using Microsoft.Extensions.Options;
using NUnit.Framework;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Infrastructure.Persistence.Dtos;
using Umbraco.Cms.Persistence.SqlServer.Services;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Infrastructure.Persistence.SqlSyntax;

[TestFixture]
public class SqlSyntaxProviderBaseGetFieldNameTests
{
    private SqlServerSyntaxProvider SyntaxProvider => new(Options.Create(new GlobalSettings()));

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
}
