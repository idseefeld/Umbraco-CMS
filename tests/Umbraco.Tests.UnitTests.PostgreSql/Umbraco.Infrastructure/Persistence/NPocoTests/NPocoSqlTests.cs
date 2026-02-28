// Copyright (c) Umbraco.
// See LICENSE for more details.

using System.Diagnostics;
using NUnit.Framework;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Persistence;
using Umbraco.Cms.Infrastructure.Persistence.Dtos;
using Umbraco.Cms.Tests.Common.PostgreSql.TestHelpers;
using Umbraco.Cms.Tests.UnitTests.PostgreSql.TestHelpers;
using Umbraco.Extensions;

namespace Umbraco.Cms.Tests.UnitTests.PostgreSql.Umbraco.Infrastructure.Persistence.NPocoTests;

[TestFixture]
public class NPocoSqlTests : BaseUsingPostgreSqlSyntax
{
    private readonly string escapeChar = Our.Umbraco.PostgreSql.Constants.EscapeTableColumAliasNames ? "\"" : string.Empty;
    [Test]
    public void Where_Clause_With_Starts_With_Additional_Parameters()
    {
        var content = new NodeDto { NodeId = 123, Path = "-1,123" };
        var sql = Sql().SelectAll().From<NodeDto>()
            .Where<NodeDto>(x => x.Path.SqlStartsWith(content.Path, TextColumnType.NVarchar));

        Assert.AreEqual($"SELECT * FROM {escapeChar}umbracoNode{escapeChar} WHERE (UPPER({escapeChar}umbracoNode{escapeChar}.{escapeChar}path{escapeChar}::text) LIKE UPPER(@0))",
            sql.SQL.Replace("\n", " "));
        Assert.AreEqual(1, sql.Arguments.Length);
        Assert.AreEqual(content.Path + "%", sql.Arguments[0]);
    }

    [Test]
    public void Where_Clause_With_Starts_With_By_Variable()
    {
        var content = new NodeDto { NodeId = 123, Path = "-1,123" };
        var sql = Sql().SelectAll().From<NodeDto>()
            .Where<NodeDto>(x => x.Path.StartsWith(content.Path) && x.NodeId != content.NodeId);

        Assert.AreEqual($"SELECT * FROM {escapeChar}umbracoNode{escapeChar} WHERE ((UPPER({escapeChar}umbracoNode{escapeChar}.{escapeChar}path{escapeChar}::text) LIKE UPPER(@0) AND ({escapeChar}umbracoNode{escapeChar}.{escapeChar}id{escapeChar} <> @1)))",
            sql.SQL.Replace("\n", " "));
        Assert.AreEqual(2, sql.Arguments.Length);
        Assert.AreEqual(content.Path + "%", sql.Arguments[0]);
        Assert.AreEqual(content.NodeId, sql.Arguments[1]);
    }

    [Test]
    public void Where_Clause_With_Not_Starts_With()
    {
        const int level = 1;
        var sql = Sql().SelectAll().From<NodeDto>()
            .Where<NodeDto>(x => x.Level == level && !x.Path.StartsWith("-20"));

        Assert.AreEqual($"SELECT * FROM {escapeChar}umbracoNode{escapeChar} WHERE ((({escapeChar}umbracoNode{escapeChar}.{escapeChar}level{escapeChar} = @0) AND NOT (UPPER({escapeChar}umbracoNode{escapeChar}.{escapeChar}path{escapeChar}::text) LIKE UPPER(@1))))",
            sql.SQL.Replace("\n", " "));
        Assert.AreEqual(2, sql.Arguments.Length);
        Assert.AreEqual(level, sql.Arguments[0]);
        Assert.AreEqual($"-20%", sql.Arguments[1]);
    }

    [Test]
    public void Where_Clause_With_EqualsFalse_Starts_With()
    {
        const int level = 1;
        var sql = Sql().SelectAll().From<NodeDto>()
            .Where<NodeDto>(x => x.Level == level && x.Path.StartsWith("-20") == false);

        Assert.AreEqual($"SELECT * FROM {escapeChar}umbracoNode{escapeChar} WHERE ((({escapeChar}umbracoNode{escapeChar}.{escapeChar}level{escapeChar} = @0) AND NOT (UPPER({escapeChar}umbracoNode{escapeChar}.{escapeChar}path{escapeChar}::text) LIKE UPPER(@1))))",
            sql.SQL.Replace("\n", " "));
        Assert.AreEqual(2, sql.Arguments.Length);
        Assert.AreEqual(level, sql.Arguments[0]);
        Assert.AreEqual($"-20%", sql.Arguments[1]);
    }

    [Test]
    public void Where_Clause_With_Equals_Clause()
    {
        var sql = Sql().SelectAll().From<NodeDto>()
            .Where<NodeDto>(x => x.Text.Equals("Hello@world.com"));

        Assert.AreEqual($"SELECT * FROM {escapeChar}umbracoNode{escapeChar} WHERE (UPPER({escapeChar}umbracoNode{escapeChar}.{escapeChar}text{escapeChar}::text) = UPPER(@0))",
            sql.SQL.Replace("\n", " "));
        Assert.AreEqual(1, sql.Arguments.Length);
        Assert.AreEqual($"Hello@world.com", sql.Arguments[0]);
    }

    [Test]
    public void Where_Clause_With_False_Boolean()
    {
        var sql = Sql().SelectAll().From<NodeDto>()
            .Where<NodeDto>(x => x.Trashed == false);

        Assert.AreEqual($"SELECT * FROM {escapeChar}umbracoNode{escapeChar} WHERE ({escapeChar}umbracoNode{escapeChar}.{escapeChar}trashed{escapeChar} = @0)", sql.SQL.Replace("\n", " "));
        Assert.AreEqual(1, sql.Arguments.Length);
        Assert.AreEqual(false, sql.Arguments[0]);
    }

    [Test]
    public void Where_Clause_With_EqualsFalse_Boolean()
    {
        var sql = Sql().SelectAll().From<NodeDto>().Where<NodeDto>(x => x.Trashed == false);

        Assert.AreEqual($"SELECT * FROM {escapeChar}umbracoNode{escapeChar} WHERE ({escapeChar}umbracoNode{escapeChar}.{escapeChar}trashed{escapeChar} = @0)", sql.SQL.Replace("\n", " "));
        Assert.AreEqual(1, sql.Arguments.Length);
        Assert.AreEqual(false, sql.Arguments[0]);
    }

    [Test]
    public void Where_Clause_With_Boolean()
    {
        var sql = Sql().SelectAll().From<NodeDto>()
            .Where<NodeDto>(x => x.Trashed);

        Assert.AreEqual($"SELECT * FROM {escapeChar}umbracoNode{escapeChar} WHERE ({escapeChar}umbracoNode{escapeChar}.{escapeChar}trashed{escapeChar} = @0)", sql.SQL.Replace("\n", " "));
        Assert.AreEqual(1, sql.Arguments.Length);
        Assert.AreEqual(true, sql.Arguments[0]);
    }

    [Test]
    public void Where_Clause_With_ToUpper()
    {
        var sql = Sql().SelectAll().From<NodeDto>()
            .Where<NodeDto>(x => x.Text.ToUpper() == "hello".ToUpper());

        // NPoco translates ToUpper to upper() in SQL
        Assert.AreEqual($"SELECT * FROM {escapeChar}umbracoNode{escapeChar} WHERE ((upper({escapeChar}umbracoNode{escapeChar}.{escapeChar}text{escapeChar}) = upper(@0)))",
            sql.SQL.Replace("\n", " "));
        Assert.AreEqual(1, sql.Arguments.Length);
        Assert.AreEqual($"hello", sql.Arguments[0]);
    }

    [Test]
    public void Where_Clause_With_ToString()
    {
        var sql = Sql().SelectAll().From<NodeDto>()
            .Where<NodeDto>(x => x.Text == 1.ToString());

        Assert.AreEqual($"SELECT * FROM {escapeChar}umbracoNode{escapeChar} WHERE (({escapeChar}umbracoNode{escapeChar}.{escapeChar}text{escapeChar} = @0))", sql.SQL.Replace("\n", " "));
        Assert.AreEqual(1, sql.Arguments.Length);
        Assert.AreEqual($"1", sql.Arguments[0]);
    }

    [Test]
    public void Where_Clause_With_Wildcard()
    {
        var sql = Sql().SelectAll().From<NodeDto>()
            .Where<NodeDto>(x => x.Text.StartsWith("D"));

        Assert.AreEqual($"SELECT * FROM {escapeChar}umbracoNode{escapeChar} WHERE (UPPER({escapeChar}umbracoNode{escapeChar}.{escapeChar}text{escapeChar}::text) LIKE UPPER(@0))",
            sql.SQL.Replace("\n", " "));
        Assert.AreEqual(1, sql.Arguments.Length);
        Assert.AreEqual($"D%", sql.Arguments[0]);
    }

    [Test]
    public void Where_Clause_Single_Constant()
    {
        var sql = Sql().SelectAll().From<NodeDto>()
            .Where<NodeDto>(x => x.NodeId == 2);

        Assert.AreEqual($"SELECT * FROM {escapeChar}umbracoNode{escapeChar} WHERE (({escapeChar}umbracoNode{escapeChar}.{escapeChar}id{escapeChar} = @0))", sql.SQL.Replace("\n", " "));
        Assert.AreEqual(1, sql.Arguments.Length);
        Assert.AreEqual(2, sql.Arguments[0]);
    }

    [Test]
    public void Where_Clause_And_Constant()
    {
        var sql = Sql().SelectAll().From<NodeDto>()
            .Where<NodeDto>(x => x.NodeId != 2 && x.NodeId != 3);

        Assert.AreEqual($"SELECT * FROM {escapeChar}umbracoNode{escapeChar} WHERE ((({escapeChar}umbracoNode{escapeChar}.{escapeChar}id{escapeChar} <> @0) AND ({escapeChar}umbracoNode{escapeChar}.{escapeChar}id{escapeChar} <> @1)))",
            sql.SQL.Replace("\n", " "));
        Assert.AreEqual(2, sql.Arguments.Length);
        Assert.AreEqual(2, sql.Arguments[0]);
        Assert.AreEqual(3, sql.Arguments[1]);
    }

    [Test]
    public void Where_Clause_Or_Constant()
    {
        var sql = Sql().SelectAll().From<NodeDto>()
            .Where<NodeDto>(x => x.Text == "hello" || x.NodeId == 3);

        Assert.AreEqual($"SELECT * FROM {escapeChar}umbracoNode{escapeChar} WHERE ((({escapeChar}umbracoNode{escapeChar}.{escapeChar}text{escapeChar} = @0) OR ({escapeChar}umbracoNode{escapeChar}.{escapeChar}id{escapeChar} = @1)))",
            sql.SQL.Replace("\n", " "));
        Assert.AreEqual(2, sql.Arguments.Length);
        Assert.AreEqual($"hello", sql.Arguments[0]);
        Assert.AreEqual(3, sql.Arguments[1]);
    }

    [Test]
    public void Where_Null()
    {
        var sql = Sql().SelectAll().From<NodeDto>().WhereNull<NodeDto>(x => x.NodeId);
        Assert.AreEqual($"SELECT * FROM {escapeChar}umbracoNode{escapeChar} WHERE (({escapeChar}umbracoNode{escapeChar}.{escapeChar}id{escapeChar} IS NULL))", sql.SQL.Replace("\n", " "));
    }

    [Test]
    public void Where_Not_Null()
    {
        var sql = Sql().SelectAll().From<NodeDto>().WhereNotNull<NodeDto>(x => x.NodeId);
        Assert.AreEqual($"SELECT * FROM {escapeChar}umbracoNode{escapeChar} WHERE (({escapeChar}umbracoNode{escapeChar}.{escapeChar}id{escapeChar} IS NOT NULL))",
            sql.SQL.Replace("\n", " "));
    }

    [Test]
    public void Where_Any()
    {
        var sql = Sql().SelectAll().From<NodeDto>().WhereAny(
            s => s.Where<NodeDto>(x => x.NodeId == 1),
            s => s.Where<NodeDto>(x => x.NodeId == 2));

        Assert.AreEqual($"SELECT * FROM {escapeChar}umbracoNode{escapeChar} WHERE (( (({escapeChar}umbracoNode{escapeChar}.{escapeChar}id{escapeChar} = @0)) ) OR ( (({escapeChar}umbracoNode{escapeChar}.{escapeChar}id{escapeChar} = @1)) ))",
            sql.SQL.Replace("\n", " "));
    }

    [Test]
    public void Can_Select_From_With_Type()
    {
        var expected = Sql();
        expected.SelectAll().From($"{escapeChar}{Constants.DatabaseSchema.Tables.Content}{escapeChar}");

        var sql = Sql();
        sql.SelectAll().From<ContentDto>();

        Assert.That(sql.SQL, Is.EqualTo(expected.SQL));

        Debug.Print(sql.SQL);
    }

    [Test]
    public void Can_InnerJoin_With_Types()
    {
        var expected = Sql();
        expected.SelectAll()
            .From($"{escapeChar}{Constants.DatabaseSchema.Tables.DocumentVersion}{escapeChar}")
            .InnerJoin($"{escapeChar}{Constants.DatabaseSchema.Tables.ContentVersion}{escapeChar}")
            .On(
                $"{escapeChar}{Constants.DatabaseSchema.Tables.DocumentVersion}{escapeChar}.{escapeChar}id{escapeChar} = {escapeChar}{Constants.DatabaseSchema.Tables.ContentVersion}{escapeChar}.{escapeChar}id{escapeChar}");

        var sql = Sql();
        sql.SelectAll().From<DocumentVersionDto>()
            .InnerJoin<ContentVersionDto>()
            .On<DocumentVersionDto, ContentVersionDto>(left => left.Id, right => right.Id);

        Assert.That(sql.SQL, Is.EqualTo(expected.SQL));

        Debug.Print(sql.SQL);
    }

    [Test]
    public void Can_OrderBy_With_Type()
    {
        var expected = Sql();
        expected.SelectAll().From($"{escapeChar}{Constants.DatabaseSchema.Tables.Content}{escapeChar}")
            .OrderBy($"({escapeChar}{Constants.DatabaseSchema.Tables.Content}{escapeChar}.{escapeChar}contentTypeId{escapeChar})");

        var sql = Sql();
        sql.SelectAll().From<ContentDto>().OrderBy<ContentDto>(x => x.ContentTypeId);

        Assert.That(sql.SQL, Is.EqualTo(expected.SQL));

        Debug.Print(sql.SQL);
    }

    [Test]
    public void Can_GroupBy_With_Type()
    {
        var expected = Sql();
        expected.SelectAll().From($"{escapeChar}{Constants.DatabaseSchema.Tables.Content}{escapeChar}")
            .GroupBy($"{escapeChar}{Constants.DatabaseSchema.Tables.Content}{escapeChar}.{escapeChar}contentTypeId{escapeChar}");

        var sql = Sql();
        sql.SelectAll().From<ContentDto>().GroupBy<ContentDto>(x => x.ContentTypeId);

        Assert.That(sql.SQL, Is.EqualTo(expected.SQL));

        Debug.Print(sql.SQL);
    }

    [Test]
    public void Can_Use_Where_Predicate()
    {
        var expected = Sql();
        expected.SelectAll().From($"{escapeChar}{Constants.DatabaseSchema.Tables.Content}{escapeChar}")
            .Where($"({escapeChar}{Constants.DatabaseSchema.Tables.Content}{escapeChar}.{escapeChar}nodeId{escapeChar} = @0)", 1045);

        var sql = Sql();
        sql.SelectAll().From<ContentDto>().Where<ContentDto>(x => x.NodeId == 1045);

        Assert.That(sql.SQL, Is.EqualTo(expected.SQL));

        Debug.Print(sql.SQL);
    }

    [Test]
    public void Can_Use_Where_And_Predicate()
    {
        var expected = Sql();
        expected.SelectAll()
            .From($"{escapeChar}{Constants.DatabaseSchema.Tables.Content}{escapeChar}")
            .Where($"({escapeChar}{Constants.DatabaseSchema.Tables.Content}{escapeChar}.{escapeChar}nodeId{escapeChar} = @0)", 1045)
            .Where($"({escapeChar}{Constants.DatabaseSchema.Tables.Content}{escapeChar}.{escapeChar}contentTypeId{escapeChar} = @0)", 1050);

        var sql = Sql();
        sql.SelectAll()
            .From<ContentDto>()
            .Where<ContentDto>(x => x.NodeId == 1045)
            .Where<ContentDto>(x => x.ContentTypeId == 1050);

        Assert.That(sql.SQL, Is.EqualTo(expected.SQL));

        Debug.Print(sql.SQL);
    }

    [Test]
    public void ForUpdate()
    {
        var sessionId = Guid.NewGuid();

        var sql = Sql()
            .SelectAll()
            .From<UserLoginDto>()
            .Where<UserLoginDto>(x => x.SessionId == sessionId);

        Assert.AreEqual($"SELECT * FROM {escapeChar}umbracoUserLogin{escapeChar} WHERE (({escapeChar}umbracoUserLogin{escapeChar}.{escapeChar}sessionId{escapeChar} = @0))",
            sql.SQL.NoCrLf());

        sql = sql.ForUpdate();

        Assert.AreEqual($"SELECT * FROM {escapeChar}umbracoUserLogin{escapeChar} WHERE (({escapeChar}umbracoUserLogin{escapeChar}.{escapeChar}sessionId{escapeChar} = @0))",
            sql.SQL.NoCrLf());
    }
}
