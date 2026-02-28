// Copyright (c) Umbraco.
// See LICENSE for more details.

using NUnit.Framework;
using Umbraco.Cms.Infrastructure.Persistence.Mappers;
using Umbraco.Cms.Tests.UnitTests.PostgreSql.TestHelpers;

namespace Umbraco.Cms.Tests.UnitTests.PostgreSql.Umbraco.Infrastructure.Persistence.Mappers;

[TestFixture]
public class PropertyGroupMapperTest
{
    private readonly string escapeChar = Our.Umbraco.PostgreSql.Constants.EscapeTableColumAliasNames ? "\"" : string.Empty;
    [Test]
    public void Can_Map_Id_Property()
    {
        // Act
        var column = new PropertyGroupMapper(TestHelper.GetMockSqlContext(), TestHelper.CreateMaps()).Map("Id");

        // Assert
        Assert.That(column, Is.EqualTo($"{escapeChar}cmsPropertyTypeGroup{escapeChar}.{escapeChar}id{escapeChar}"));
    }

    [Test]
    public void Can_Map_SortOrder_Property()
    {
        // Act
        var column = new PropertyGroupMapper(TestHelper.GetMockSqlContext(), TestHelper.CreateMaps()).Map("SortOrder");

        // Assert
        Assert.That(column, Is.EqualTo($"{escapeChar}cmsPropertyTypeGroup{escapeChar}.{escapeChar}sortorder{escapeChar}"));
    }

    [Test]
    public void Can_Map_Name_Property()
    {
        // Act
        var column = new PropertyGroupMapper(TestHelper.GetMockSqlContext(), TestHelper.CreateMaps()).Map("Name");

        // Assert
        Assert.That(column, Is.EqualTo($"{escapeChar}cmsPropertyTypeGroup{escapeChar}.{escapeChar}text{escapeChar}"));
    }
}
