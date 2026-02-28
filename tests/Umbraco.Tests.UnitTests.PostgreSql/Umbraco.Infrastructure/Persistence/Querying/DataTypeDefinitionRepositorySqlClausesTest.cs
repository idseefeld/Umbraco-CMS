// Copyright (c) Umbraco.
// See LICENSE for more details.

using System.Diagnostics;
using NPoco;
using NUnit.Framework;
using Umbraco.Cms.Core;
using Umbraco.Cms.Infrastructure.Persistence.Dtos;
using Umbraco.Cms.Tests.UnitTests.PostgreSql.TestHelpers;
using Umbraco.Extensions;

namespace Umbraco.Cms.Tests.UnitTests.PostgreSql.Umbraco.Infrastructure.Persistence.Querying;

[TestFixture]
public class DataTypeDefinitionRepositorySqlClausesTest : BaseUsingPostgreSqlSyntax
{
    private readonly string escapeChar = Our.Umbraco.PostgreSql.Constants.EscapeTableColumAliasNames ? "\"" : string.Empty;
    [Test]
    public void Can_Verify_Base_Clause()
    {
        var nodeObjectTypeId = Constants.ObjectTypes.DataType;

        var expected = new Sql();
        expected.Select("*")
            .From($"{escapeChar}{Constants.DatabaseSchema.Tables.DataType}{escapeChar}")
            .InnerJoin($"{escapeChar}umbracoNode{escapeChar}")
            .On($"{escapeChar}{Constants.DatabaseSchema.Tables.DataType}{escapeChar}.{escapeChar}nodeId{escapeChar} = {escapeChar}umbracoNode{escapeChar}.{escapeChar}id{escapeChar}")
            .Where($"({escapeChar}umbracoNode{escapeChar}.{escapeChar}nodeObjectType{escapeChar} = @0)", new Guid("30a2a501-1978-4ddb-a57b-f7efed43ba3c"));

        var sql = Sql();
        sql.SelectAll()
            .From<DataTypeDto>()
            .InnerJoin<NodeDto>()
            .On<DataTypeDto, NodeDto>(left => left.NodeId, right => right.NodeId)
            .Where<NodeDto>(x => x.NodeObjectType == nodeObjectTypeId);

        Assert.That(sql.SQL, Is.EqualTo(expected.SQL));

        Assert.AreEqual(expected.Arguments.Length, sql.Arguments.Length);
        for (var i = 0; i < expected.Arguments.Length; i++)
        {
            Assert.AreEqual(expected.Arguments[i], sql.Arguments[i]);
        }

        Debug.Print(sql.SQL);
    }
}
