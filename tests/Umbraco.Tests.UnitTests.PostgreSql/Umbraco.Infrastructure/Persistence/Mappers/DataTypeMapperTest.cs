// Copyright (c) Umbraco.
// See LICENSE for more details.

using NUnit.Framework;
using Umbraco.Cms.Core;
using Umbraco.Cms.Infrastructure.Persistence.Mappers;
using Umbraco.Cms.Tests.UnitTests.PostgreSql.TestHelpers;

namespace Umbraco.Cms.Tests.UnitTests.PostgreSql.Umbraco.Infrastructure.Persistence.Mappers;

[TestFixture]
public class DataTypeMapperTest
{
    private readonly string escapeChar = Our.Umbraco.PostgreSql.Constants.EscapeTableColumAliasNames ? "\"" : string.Empty;

    [Test]
    public void Can_Map_Id_Property()
    {
        // Act
        var column = new DataTypeMapper(TestHelper.GetMockSqlContext(), TestHelper.CreateMaps()).Map("Id");

        // Assert
        Assert.That(column, Is.EqualTo($"{escapeChar}umbracoNode{escapeChar}.{escapeChar}id{escapeChar}"));
    }

    [Test]
    public void Can_Map_Key_Property()
    {
        // Act
        var column = new DataTypeMapper(TestHelper.GetMockSqlContext(), TestHelper.CreateMaps()).Map("Key");

        // Assert
        Assert.That(column, Is.EqualTo($"{escapeChar}umbracoNode{escapeChar}.{escapeChar}uniqueId{escapeChar}"));
    }

    [Test]
    public void Can_Map_DatabaseType_Property()
    {
        // Act
        var column = new DataTypeMapper(TestHelper.GetMockSqlContext(), TestHelper.CreateMaps()).Map("DatabaseType");

        // Assert
        Assert.That(column, Is.EqualTo($"{escapeChar}{Constants.DatabaseSchema.Tables.DataType}{escapeChar}.{escapeChar}dbType{escapeChar}"));
    }

    [Test]
    public void Can_Map_PropertyEditorAlias_Property()
    {
        // Act
        var column = new DataTypeMapper(TestHelper.GetMockSqlContext(), TestHelper.CreateMaps()).Map("EditorAlias");

        // Assert
        Assert.That(column, Is.EqualTo($"{escapeChar}{Constants.DatabaseSchema.Tables.DataType}{escapeChar}.{escapeChar}propertyEditorAlias{escapeChar}"));
    }
}
