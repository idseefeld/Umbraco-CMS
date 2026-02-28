// Copyright (c) Umbraco.
// See LICENSE for more details.

using NUnit.Framework;
using Umbraco.Cms.Infrastructure.Persistence.Mappers;
using Umbraco.Cms.Tests.UnitTests.PostgreSql.TestHelpers;

namespace Umbraco.Cms.Tests.UnitTests.PostgreSql.Umbraco.Infrastructure.Persistence.Mappers;

[TestFixture]
public class DictionaryMapperTest
{
    private readonly string escapeChar = Our.Umbraco.PostgreSql.Constants.EscapeTableColumAliasNames ? "\"" : string.Empty;

    [Test]
    public void Can_Map_Id_Property()
    {
        // Act
        var column = new DictionaryMapper(TestHelper.GetMockSqlContext(), TestHelper.CreateMaps()).Map("Id");

        // Assert
        Assert.That(column, Is.EqualTo($"{escapeChar}cmsDictionary{escapeChar}.{escapeChar}pk{escapeChar}"));
    }

    [Test]
    public void Can_Map_Key_Property()
    {
        // Act
        var column = new DictionaryMapper(TestHelper.GetMockSqlContext(), TestHelper.CreateMaps()).Map("Key");

        // Assert
        Assert.That(column, Is.EqualTo($"{escapeChar}cmsDictionary{escapeChar}.{escapeChar}id{escapeChar}"));
    }

    [Test]
    public void Can_Map_ItemKey_Property()
    {
        // Act
        var column = new DictionaryMapper(TestHelper.GetMockSqlContext(), TestHelper.CreateMaps()).Map("ItemKey");

        // Assert
        Assert.That(column, Is.EqualTo($"{escapeChar}cmsDictionary{escapeChar}.{escapeChar}key{escapeChar}"));
    }
}
