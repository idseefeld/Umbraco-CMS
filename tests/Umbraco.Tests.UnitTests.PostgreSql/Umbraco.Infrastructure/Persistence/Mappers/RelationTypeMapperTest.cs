// Copyright (c) Umbraco.
// See LICENSE for more details.

using NUnit.Framework;
using Umbraco.Cms.Infrastructure.Persistence.Mappers;
using Umbraco.Cms.Tests.UnitTests.PostgreSql.TestHelpers;

namespace Umbraco.Cms.Tests.UnitTests.PostgreSql.Umbraco.Infrastructure.Persistence.Mappers;

[TestFixture]
public class RelationTypeMapperTest
{
    private readonly string escapeChar = Our.Umbraco.PostgreSql.Constants.EscapeTableColumAliasNames ? "\"" : string.Empty;
    [Test]
    public void Can_Map_Id_Property()
    {
        // Act
        var column = new RelationTypeMapper(TestHelper.GetMockSqlContext(), TestHelper.CreateMaps()).Map("Id");

        // Assert
        Assert.That(column, Is.EqualTo($"{escapeChar}umbracoRelationType{escapeChar}.{escapeChar}id{escapeChar}"));
    }

    [Test]
    public void Can_Map_Alias_Property()
    {
        // Act
        var column = new RelationTypeMapper(TestHelper.GetMockSqlContext(), TestHelper.CreateMaps()).Map("Alias");

        // Assert
        Assert.That(column, Is.EqualTo($"{escapeChar}umbracoRelationType{escapeChar}.{escapeChar}alias{escapeChar}"));
    }

    [Test]
    public void Can_Map_ChildObjectType_Property()
    {
        // Act
        var column =
            new RelationTypeMapper(TestHelper.GetMockSqlContext(), TestHelper.CreateMaps()).Map("ChildObjectType");

        // Assert
        Assert.That(column, Is.EqualTo($"{escapeChar}umbracoRelationType{escapeChar}.{escapeChar}childObjectType{escapeChar}"));
    }

    [Test]
    public void Can_Map_IsBidirectional_Property()
    {
        // Act
        var column =
            new RelationTypeMapper(TestHelper.GetMockSqlContext(), TestHelper.CreateMaps()).Map("IsBidirectional");

        // Assert
        Assert.That(column, Is.EqualTo($"{escapeChar}umbracoRelationType{escapeChar}.{escapeChar}dual{escapeChar}"));
    }
}
