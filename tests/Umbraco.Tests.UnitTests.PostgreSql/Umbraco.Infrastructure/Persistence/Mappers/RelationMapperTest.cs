// Copyright (c) Umbraco.
// See LICENSE for more details.

using NUnit.Framework;
using Umbraco.Cms.Infrastructure.Persistence.Mappers;
using Umbraco.Cms.Tests.UnitTests.PostgreSql.TestHelpers;

namespace Umbraco.Cms.Tests.UnitTests.PostgreSql.Umbraco.Infrastructure.Persistence.Mappers;

[TestFixture]
public class RelationMapperTest
{
    private readonly string escapeChar = Our.Umbraco.PostgreSql.Constants.EscapeTableColumAliasNames ? "\"" : string.Empty;
    [Test]
    public void Can_Map_Id_Property()
    {
        // Act
        var column = new RelationMapper(TestHelper.GetMockSqlContext(), TestHelper.CreateMaps()).Map("Id");

        // Assert
        Assert.That(column, Is.EqualTo($"{escapeChar}umbracoRelation{escapeChar}.{escapeChar}id{escapeChar}"));
    }

    [Test]
    public void Can_Map_ChildId_Property()
    {
        // Act
        var column = new RelationMapper(TestHelper.GetMockSqlContext(), TestHelper.CreateMaps()).Map("ChildId");

        // Assert
        Assert.That(column, Is.EqualTo($"{escapeChar}umbracoRelation{escapeChar}.{escapeChar}childId{escapeChar}"));
    }

    [Test]
    public void Can_Map_Datetime_Property()
    {
        // Act
        var column = new RelationMapper(TestHelper.GetMockSqlContext(), TestHelper.CreateMaps()).Map("CreateDate");

        // Assert
        Assert.That(column, Is.EqualTo($"{escapeChar}umbracoRelation{escapeChar}.{escapeChar}datetime{escapeChar}"));
    }

    [Test]
    public void Can_Map_Comment_Property()
    {
        // Act
        var column = new RelationMapper(TestHelper.GetMockSqlContext(), TestHelper.CreateMaps()).Map("Comment");

        // Assert
        Assert.That(column, Is.EqualTo($"{escapeChar}umbracoRelation{escapeChar}.{escapeChar}comment{escapeChar}"));
    }

    [Test]
    public void Can_Map_RelationType_Property()
    {
        // Act
        var column = new RelationMapper(TestHelper.GetMockSqlContext(), TestHelper.CreateMaps()).Map("RelationTypeId");

        // Assert
        Assert.That(column, Is.EqualTo($"{escapeChar}umbracoRelation{escapeChar}.{escapeChar}relType{escapeChar}"));
    }
}
