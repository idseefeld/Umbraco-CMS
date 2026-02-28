// Copyright (c) Umbraco.
// See LICENSE for more details.

using NUnit.Framework;
using Umbraco.Cms.Core;
using Umbraco.Cms.Infrastructure.Persistence.Mappers;
using Umbraco.Cms.Tests.UnitTests.PostgreSql.TestHelpers;

namespace Umbraco.Cms.Tests.UnitTests.PostgreSql.Umbraco.Infrastructure.Persistence.Mappers;

[TestFixture]
public class PropertyTypeMapperTest
{
    private readonly string escapeChar = Our.Umbraco.PostgreSql.Constants.EscapeTableColumAliasNames ? "\"" : string.Empty;
    [Test]
    public void Can_Map_Id_Property()
    {
        // Act
        var column = new PropertyTypeMapper(TestHelper.GetMockSqlContext(), TestHelper.CreateMaps()).Map("Id");

        // Assert
        Assert.That(column, Is.EqualTo($"{escapeChar}cmsPropertyType{escapeChar}.{escapeChar}id{escapeChar}"));
    }

    [Test]
    public void Can_Map_Alias_Property()
    {
        // Act
        var column = new PropertyTypeMapper(TestHelper.GetMockSqlContext(), TestHelper.CreateMaps()).Map("Alias");

        // Assert
        Assert.That(column, Is.EqualTo($"{escapeChar}cmsPropertyType{escapeChar}.{escapeChar}Alias{escapeChar}"));
    }

    [Test]
    public void Can_Map_DataTypeDefinitionId_Property()
    {
        // Act
        var column = new PropertyTypeMapper(TestHelper.GetMockSqlContext(), TestHelper.CreateMaps()).Map("DataTypeId");

        // Assert
        Assert.That(column, Is.EqualTo($"{escapeChar}cmsPropertyType{escapeChar}.{escapeChar}dataTypeId{escapeChar}"));
    }

    [Test]
    public void Can_Map_SortOrder_Property()
    {
        // Act
        var column = new PropertyTypeMapper(TestHelper.GetMockSqlContext(), TestHelper.CreateMaps()).Map("SortOrder");

        // Assert
        Assert.That(column, Is.EqualTo($"{escapeChar}cmsPropertyType{escapeChar}.{escapeChar}sortOrder{escapeChar}"));
    }

    [Test]
    public void Can_Map_PropertyEditorAlias_Property()
    {
        // Act
        var column =
            new PropertyTypeMapper(TestHelper.GetMockSqlContext(), TestHelper.CreateMaps()).Map("PropertyEditorAlias");

        // Assert
        Assert.That(column, Is.EqualTo($"{escapeChar}{Constants.DatabaseSchema.Tables.DataType}{escapeChar}.{escapeChar}propertyEditorAlias{escapeChar}"));
    }

    [Test]
    public void Can_Map_DataTypeDatabaseType_Property()
    {
        // Act
        var column =
            new PropertyTypeMapper(TestHelper.GetMockSqlContext(), TestHelper.CreateMaps()).Map("ValueStorageType");

        // Assert
        Assert.That(column, Is.EqualTo($"{escapeChar}{Constants.DatabaseSchema.Tables.DataType}{escapeChar}.{escapeChar}dbType{escapeChar}"));
    }
}
