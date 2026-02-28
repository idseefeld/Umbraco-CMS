// Copyright (c) Umbraco.
// See LICENSE for more details.

using System.Collections.Generic;
using NPoco;
using NUnit.Framework;
using Umbraco.Cms.Core.Persistence;
using Umbraco.Cms.Infrastructure.Persistence;
using Umbraco.Cms.Infrastructure.Persistence.Dtos;
using Umbraco.Cms.Tests.Common.PostgreSql.TestHelpers;
using Umbraco.Cms.Tests.UnitTests.PostgreSql.TestHelpers;
using Umbraco.Extensions;
using static Umbraco.Cms.Core.Persistence.SqlExtensionsStatics;

namespace Umbraco.Cms.Tests.UnitTests.PostgreSql.Umbraco.Infrastructure.Persistence.NPocoTests;

[TestFixture]
public class NPocoSqlExtensionsTests : BaseUsingPostgreSqlSyntax
{
    private readonly string escapeChar = Our.Umbraco.PostgreSql.Constants.EscapeTableColumAliasNames ? "\"" : string.Empty;
    [Test]
    public void WhereTest()
    {
        var sql = new Sql<ISqlContext>(SqlContext)
            .Select("*")
            .From<PropertyDataDto>()
            .Where<PropertyDataDto>(x => x.LanguageId == null);
        Assert.AreEqual($"SELECT *\nFROM {escapeChar}umbracoPropertyData{escapeChar}\nWHERE (({escapeChar}umbracoPropertyData{escapeChar}.{escapeChar}languageId{escapeChar} is null))",
            sql.SQL,
            sql.SQL);

        sql = new Sql<ISqlContext>(SqlContext)
            .Select("*")
            .From<PropertyDataDto>()
            .Where<PropertyDataDto>(x => x.LanguageId == 123);
        Assert.AreEqual($"SELECT *\nFROM {escapeChar}umbracoPropertyData{escapeChar}\nWHERE (({escapeChar}umbracoPropertyData{escapeChar}.{escapeChar}languageId{escapeChar} = @0))",
            sql.SQL,
            sql.SQL);

        var id = 123;

        sql = new Sql<ISqlContext>(SqlContext)
            .Select("*")
            .From<PropertyDataDto>()
            .Where<PropertyDataDto>(x => x.LanguageId == id);
        Assert.AreEqual($"SELECT *\nFROM {escapeChar}umbracoPropertyData{escapeChar}\nWHERE (({escapeChar}umbracoPropertyData{escapeChar}.{escapeChar}languageId{escapeChar} = @0))",
            sql.SQL,
            sql.SQL);

        int? nid = 123;

        sql = new Sql<ISqlContext>(SqlContext)
            .Select("*")
            .From<PropertyDataDto>()
            .Where<PropertyDataDto>(x => x.LanguageId == nid);
        Assert.AreEqual($"SELECT *\nFROM {escapeChar}umbracoPropertyData{escapeChar}\nWHERE (({escapeChar}umbracoPropertyData{escapeChar}.{escapeChar}languageId{escapeChar} = @0))",
            sql.SQL,
            sql.SQL);

        // but the above comparison fails if @0 is null
        // what we want is something similar to:
        sql = new Sql<ISqlContext>(SqlContext)
            .Select("*")
            .From<PropertyDataDto>()
            .Where<PropertyDataDto>(x => (nid == null && x.LanguageId == null) || (nid != null && x.LanguageId == nid));
        Assert.AreEqual($"SELECT *\nFROM {escapeChar}umbracoPropertyData{escapeChar}\nWHERE ((((@0 is null) AND ({escapeChar}umbracoPropertyData{escapeChar}.{escapeChar}languageId{escapeChar} is null)) OR ((@1 is not null) AND ({escapeChar}umbracoPropertyData{escapeChar}.{escapeChar}languageId{escapeChar} = @2))))",
            sql.SQL,
            sql.SQL);

        // new SqlNullableEquals method does it automatically
        // 'course it would be nicer if '==' could do it
        // see note in ExpressionVisitorBase for SqlNullableEquals

        // sql = new Sql<ISqlContext>(SqlContext)
        //    .Select("*")
        //    .From<PropertyDataDto>()
        //    .Where<PropertyDataDto>(x => x.LanguageId.SqlNullableEquals(nid));
        // Assert.AreEqual($"SELECT *\nFROM {escapeChar}umbracoPropertyData{escapeChar}\nWHERE ((((@0 is null) AND ({escapeChar}umbracoPropertyData{escapeChar}.{escapeChar}languageId{escapeChar} is null)) OR ((@0 is not null) AND ({escapeChar}umbracoPropertyData{escapeChar}.{escapeChar}languageId{escapeChar} = @0))))", sql.SQL, sql.SQL);

        // but, the expression above fails with SQL CE, 'specified argument for the function is not valid' in 'isnull' function
        // so... compare with fallback values
        sql = new Sql<ISqlContext>(SqlContext)
            .Select("*")
            .From<PropertyDataDto>()
            .Where<PropertyDataDto>(x => x.LanguageId.SqlNullableEquals(nid, -1));
        Assert.AreEqual($"SELECT *\nFROM {escapeChar}umbracoPropertyData{escapeChar}\nWHERE ((COALESCE({escapeChar}umbracoPropertyData{escapeChar}.{escapeChar}languageId{escapeChar},@0) = COALESCE(@1,@0)))",
            sql.SQL,
            sql.SQL);
    }

    [Test]
    public void SqlNullableEqualsTest()
    {
        int? a, b;
        a = b = null;
        Assert.IsTrue(a.SqlNullableEquals(b, -1));
        b = 2;
        Assert.IsFalse(a.SqlNullableEquals(b, -1));
        a = 2;
        Assert.IsTrue(a.SqlNullableEquals(b, -1));
        b = null;
        Assert.IsFalse(a.SqlNullableEquals(b, -1));
    }

    [Test]
    public void WhereInValueFieldTest()
    {
        var sql = new Sql<ISqlContext>(SqlContext)
            .Select("*")
            .From<NodeDto>()
            .WhereIn<NodeDto>(x => x.NodeId, new[] { 1, 2, 3 });
        Assert.AreEqual($"SELECT *\nFROM {escapeChar}umbracoNode{escapeChar}\nWHERE ({escapeChar}umbracoNode{escapeChar}.{escapeChar}id{escapeChar} IN (@0,@1,@2))", sql.SQL);
    }

    [Test]
    public void WhereInObjectFieldTest()
    {
        var sql = new Sql<ISqlContext>(SqlContext)
            .Select("*")
            .From<NodeDto>()
            .WhereIn<NodeDto>(x => x.Text, new[] { "a", "b", "c" });

        // Assert.AreEqual($"SELECT *\nFROM {escapeChar}umbracoNode{escapeChar}\nWHERE (LOWER({escapeChar}umbracoNode{escapeChar}.{escapeChar}text{escapeChar}) IN (@0,@1,@2))", sql.SQL);
        Assert.AreEqual($"SELECT *\nFROM {escapeChar}umbracoNode{escapeChar}\nWHERE ({escapeChar}umbracoNode{escapeChar}.{escapeChar}text{escapeChar} IN (@0,@1,@2))", sql.SQL);
    }

    [Test]
    [NUnit.Framework.Ignore("This test is to verify that mixed value types in the arguments throws an exception, but it currently does not throw, so needs to be fixed before enabling this test.")]
    public void WhereInMixedValueTypesThrowsTest()
    {
        var guid = Guid.NewGuid();
        var arguments = new object[] { "a", "b", "c", 1, 234, guid };
        Assert.Throws<ArgumentException>(() => new Sql<ISqlContext>(SqlContext)
            .Select("*")
            .From<NodeDto>()
            .WhereIn<NodeDto>(x => x.Text, arguments));
    }

    [Test]
    public void SelectTests()
    {
        // select the whole DTO
        var sql = Sql()
            .Select<Dto1>()
            .From<Dto1>();
        Assert.AreEqual($"SELECT {escapeChar}dto1{escapeChar}.{escapeChar}id{escapeChar} AS {escapeChar}Id{escapeChar}, {escapeChar}dto1{escapeChar}.{escapeChar}name{escapeChar} AS {escapeChar}Name{escapeChar}, {escapeChar}dto1{escapeChar}.{escapeChar}value{escapeChar} AS {escapeChar}Value{escapeChar} FROM {escapeChar}dto1{escapeChar}",
            sql.SQL.NoCrLf());

        // select only 1 field
        sql = Sql()
            .Select<Dto1>(x => x.Id)
            .From<Dto1>();
        Assert.AreEqual($"SELECT {escapeChar}dto1{escapeChar}.{escapeChar}id{escapeChar} AS {escapeChar}Id{escapeChar} FROM {escapeChar}dto1{escapeChar}", sql.SQL.NoCrLf());

        // select 2 fields
        sql = Sql()
            .Select<Dto1>(x => x.Id, x => x.Name)
            .From<Dto1>();
        Assert.AreEqual($"SELECT {escapeChar}dto1{escapeChar}.{escapeChar}id{escapeChar} AS {escapeChar}Id{escapeChar}, {escapeChar}dto1{escapeChar}.{escapeChar}name{escapeChar} AS {escapeChar}Name{escapeChar} FROM {escapeChar}dto1{escapeChar}", sql.SQL.NoCrLf());

        // select the whole DTO and a referenced DTO
        sql = Sql()
            .Select<Dto1>(r => r.Select(x => x.Dto2))
            .From<Dto1>()
            .InnerJoin<Dto2>().On<Dto1, Dto2>(left => left.Id, right => right.Dto1Id);
        Assert.AreEqual($"SELECT {escapeChar}dto1{escapeChar}.{escapeChar}id{escapeChar} AS {escapeChar}Id{escapeChar}, {escapeChar}dto1{escapeChar}.{escapeChar}name{escapeChar} AS {escapeChar}Name{escapeChar}, {escapeChar}dto1{escapeChar}.{escapeChar}value{escapeChar} AS {escapeChar}Value{escapeChar} , {escapeChar}dto2{escapeChar}.{escapeChar}id{escapeChar} AS {escapeChar}Dto2__Id{escapeChar}, {escapeChar}dto2{escapeChar}.{escapeChar}dto1id{escapeChar} AS {escapeChar}Dto2__Dto1Id{escapeChar}, {escapeChar}dto2{escapeChar}.{escapeChar}name{escapeChar} AS {escapeChar}Dto2__Name{escapeChar} FROM {escapeChar}dto1{escapeChar} INNER JOIN {escapeChar}dto2{escapeChar} ON {escapeChar}dto1{escapeChar}.{escapeChar}id{escapeChar} = {escapeChar}dto2{escapeChar}.{escapeChar}dto1id{escapeChar}".NoCrLf(),
            sql.SQL.NoCrLf(),
            sql.SQL);

        // select the whole DTO and nested referenced DTOs
        sql = Sql()
            .Select<Dto1>(r => r.Select(x => x.Dto2, r1 => r1.Select(x => x.Dto3)))
            .From<Dto1>()
            .InnerJoin<Dto2>().On<Dto1, Dto2>(left => left.Id, right => right.Dto1Id)
            .InnerJoin<Dto3>().On<Dto2, Dto3>(left => left.Id, right => right.Dto2Id);
        Assert.AreEqual($"SELECT {escapeChar}dto1{escapeChar}.{escapeChar}id{escapeChar} AS {escapeChar}Id{escapeChar}, {escapeChar}dto1{escapeChar}.{escapeChar}name{escapeChar} AS {escapeChar}Name{escapeChar}, {escapeChar}dto1{escapeChar}.{escapeChar}value{escapeChar} AS {escapeChar}Value{escapeChar} , {escapeChar}dto2{escapeChar}.{escapeChar}id{escapeChar} AS {escapeChar}Dto2__Id{escapeChar}, {escapeChar}dto2{escapeChar}.{escapeChar}dto1id{escapeChar} AS {escapeChar}Dto2__Dto1Id{escapeChar}, {escapeChar}dto2{escapeChar}.{escapeChar}name{escapeChar} AS {escapeChar}Dto2__Name{escapeChar} , {escapeChar}dto3{escapeChar}.{escapeChar}id{escapeChar} AS {escapeChar}Dto2__Dto3__Id{escapeChar}, {escapeChar}dto3{escapeChar}.{escapeChar}dto2id{escapeChar} AS {escapeChar}Dto2__Dto3__Dto2Id{escapeChar}, {escapeChar}dto3{escapeChar}.{escapeChar}name{escapeChar} AS {escapeChar}Dto2__Dto3__Name{escapeChar} FROM {escapeChar}dto1{escapeChar} INNER JOIN {escapeChar}dto2{escapeChar} ON {escapeChar}dto1{escapeChar}.{escapeChar}id{escapeChar} = {escapeChar}dto2{escapeChar}.{escapeChar}dto1id{escapeChar} INNER JOIN {escapeChar}dto3{escapeChar} ON {escapeChar}dto2{escapeChar}.{escapeChar}id{escapeChar} = {escapeChar}dto3{escapeChar}.{escapeChar}dto2id{escapeChar}".NoCrLf(),
            sql.SQL.NoCrLf());

        // select the whole DTO and referenced DTOs
        sql = Sql()
            .Select<Dto1>(r => r.Select(x => x.Dto2s))
            .From<Dto1>()
            .InnerJoin<Dto2>().On<Dto1, Dto2>(left => left.Id, right => right.Dto1Id);
        Assert.AreEqual($"SELECT {escapeChar}dto1{escapeChar}.{escapeChar}id{escapeChar} AS {escapeChar}Id{escapeChar}, {escapeChar}dto1{escapeChar}.{escapeChar}name{escapeChar} AS {escapeChar}Name{escapeChar}, {escapeChar}dto1{escapeChar}.{escapeChar}value{escapeChar} AS {escapeChar}Value{escapeChar} , {escapeChar}dto2{escapeChar}.{escapeChar}id{escapeChar} AS {escapeChar}Dto2s__Id{escapeChar}, {escapeChar}dto2{escapeChar}.{escapeChar}dto1id{escapeChar} AS {escapeChar}Dto2s__Dto1Id{escapeChar}, {escapeChar}dto2{escapeChar}.{escapeChar}name{escapeChar} AS {escapeChar}Dto2s__Name{escapeChar} FROM {escapeChar}dto1{escapeChar} INNER JOIN {escapeChar}dto2{escapeChar} ON {escapeChar}dto1{escapeChar}.{escapeChar}id{escapeChar} = {escapeChar}dto2{escapeChar}.{escapeChar}dto1id{escapeChar}".NoCrLf(),
            sql.SQL.NoCrLf());
    }

    [Test]
    public void SelectAliasTests()
    {
        // and select - not good
        var sql = Sql()
            .Select<Dto1>(x => x.Id)
            .Select<Dto2>(x => x.Id);
        Assert.AreEqual($"SELECT {escapeChar}dto1{escapeChar}.{escapeChar}id{escapeChar} AS {escapeChar}Id{escapeChar} SELECT {escapeChar}dto2{escapeChar}.{escapeChar}id{escapeChar} AS {escapeChar}Id{escapeChar}".NoCrLf(), sql.SQL.NoCrLf());

        // and select - good
        sql = Sql()
            .Select<Dto1>(x => x.Id)
            .AndSelect<Dto2>(x => x.Id);
        Assert.AreEqual($"SELECT {escapeChar}dto1{escapeChar}.{escapeChar}id{escapeChar} AS {escapeChar}Id{escapeChar} , {escapeChar}dto2{escapeChar}.{escapeChar}id{escapeChar} AS {escapeChar}Id{escapeChar}".NoCrLf(), sql.SQL.NoCrLf());

        // and select + alias
        sql = Sql()
            .Select<Dto1>(x => x.Id)
            .AndSelect<Dto2>(x => Alias(x.Id, "id2"));
        Assert.AreEqual($"SELECT {escapeChar}dto1{escapeChar}.{escapeChar}id{escapeChar} AS {escapeChar}Id{escapeChar} , {escapeChar}dto2{escapeChar}.{escapeChar}id{escapeChar} AS {escapeChar}id2{escapeChar}".NoCrLf(), sql.SQL.NoCrLf());
    }

    [Test]
    public void UpdateTests()
    {
        var sql = Sql()
            .Update<DataTypeDto>(u => u.Set(x => x.EditorAlias, "Umbraco.ColorPicker"))
            .Where<DataTypeDto>(x => x.EditorAlias == "Umbraco.ColorPickerAlias");
    }

    [TableName("dto1")]
    [PrimaryKey("id", AutoIncrement = false)]
    [ExplicitColumns]
    public class Dto1
    {
        [Column("id")]
        public int Id { get; set; }

        [Column("name")]
        public string Name { get; set; }

        [Column("value")]
        public int Value { get; set; }

        [Reference]
        public Dto2 Dto2 { get; set; }

        [Reference]
        public List<Dto2> Dto2s { get; set; }
    }

    [TableName("dto2")]
    [PrimaryKey("id", AutoIncrement = false)]
    [ExplicitColumns]
    public class Dto2
    {
        [Column("id")]
        public int Id { get; set; }

        [Column("dto1id")]
        public int Dto1Id { get; set; }

        [Column("name")]
        public string Name { get; set; }

        [Reference]
        public Dto3 Dto3 { get; set; }
    }

    [TableName("dto3")]
    [PrimaryKey("id", AutoIncrement = false)]
    [ExplicitColumns]
    public class Dto3
    {
        [Column("id")]
        public int Id { get; set; }

        [Column("dto2id")]
        public int Dto2Id { get; set; }

        [Column("name")]
        public string Name { get; set; }
    }
}
