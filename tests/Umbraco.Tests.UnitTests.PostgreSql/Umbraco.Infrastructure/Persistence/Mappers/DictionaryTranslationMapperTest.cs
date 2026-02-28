// Copyright (c) Umbraco.
// See LICENSE for more details.

using NUnit.Framework;
using Umbraco.Cms.Infrastructure.Persistence.Mappers;
using Umbraco.Cms.Tests.UnitTests.PostgreSql.TestHelpers;

namespace Umbraco.Cms.Tests.UnitTests.PostgreSql.Umbraco.Infrastructure.Persistence.Mappers;

[TestFixture]
public class DictionaryTranslationMapperTest
{
    private readonly string escapeChar = Our.Umbraco.PostgreSql.Constants.EscapeTableColumAliasNames ? "\"" : string.Empty;
    [Test]
    public void Can_Map_Key_Property()
    {
        // Act
        var column =
            new DictionaryTranslationMapper(TestHelper.GetMockSqlContext(), TestHelper.CreateMaps()).Map("Key");

        // Assert
        Assert.That(column, Is.EqualTo($"{escapeChar}cmsLanguageText{escapeChar}.{escapeChar}UniqueId{escapeChar}"));
    }

    [Test]
    public void Can_Map_Value_Property()
    {
        // Act
        var column =
            new DictionaryTranslationMapper(TestHelper.GetMockSqlContext(), TestHelper.CreateMaps()).Map("Value");

        // Assert
        Assert.That(column, Is.EqualTo($"{escapeChar}cmsLanguageText{escapeChar}.{escapeChar}value{escapeChar}"));
    }
}
