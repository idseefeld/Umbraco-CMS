// Copyright (c) Umbraco.
// See LICENSE for more details.

using NUnit.Framework;
using Umbraco.Cms.Infrastructure.Persistence.Mappers;
using Umbraco.Cms.Tests.UnitTests.PostgreSql.TestHelpers;

namespace Umbraco.Cms.Tests.UnitTests.PostgreSql.Umbraco.Infrastructure.Persistence.Mappers;

[TestFixture]
public class ContentTypeMapperTest
{
    private readonly string escapeChar = Our.Umbraco.PostgreSql.Constants.EscapeTableColumAliasNames ? "\"" : string.Empty;

    [Test]
    public void Can_Map_Id_Property()
    {
        // Act
        var column = new ContentTypeMapper(TestHelper.GetMockSqlContext(), TestHelper.CreateMaps()).Map("Id");

        // Assert
        Assert.That(column, Is.EqualTo($"{escapeChar}umbracoNode{escapeChar}.{escapeChar}id{escapeChar}"));
    }

    [Test]
    public void Can_Map_Name_Property()
    {
        // Act
        var column = new ContentTypeMapper(TestHelper.GetMockSqlContext(), TestHelper.CreateMaps()).Map("Name");

        // Assert
        Assert.That(column, Is.EqualTo($"{escapeChar}umbracoNode{escapeChar}.{escapeChar}text{escapeChar}"));
    }

    [Test]
    public void Can_Map_Thumbnail_Property()
    {
        // Act
        var column = new ContentTypeMapper(TestHelper.GetMockSqlContext(), TestHelper.CreateMaps()).Map("Thumbnail");

        // Assert
        Assert.That(column, Is.EqualTo($"{escapeChar}cmsContentType{escapeChar}.{escapeChar}thumbnail{escapeChar}"));
    }

    [Test]
    public void Can_Map_Description_Property()
    {
        // Act
        var column = new ContentTypeMapper(TestHelper.GetMockSqlContext(), TestHelper.CreateMaps()).Map("Description");

        // Assert
        Assert.That(column, Is.EqualTo($"{escapeChar}cmsContentType{escapeChar}.{escapeChar}description{escapeChar}"));
    }
}
