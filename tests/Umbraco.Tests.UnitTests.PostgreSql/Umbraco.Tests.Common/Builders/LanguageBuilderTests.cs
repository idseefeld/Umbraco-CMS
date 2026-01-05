// Copyright (c) Umbraco.
// See LICENSE for more details.

using System.Globalization;
using NUnit.Framework;
using Umbraco.Cms.Tests.Common.PostgreSql.Builders;
using Umbraco.Cms.Tests.Common.PostgreSql.Builders.Extensions;

namespace Umbraco.Cms.Tests.UnitTests.PostgreSql.Umbraco.Tests.Common.PostgreSql.Builders;

[TestFixture]
public class LanguageBuilderTests
{
    [Test]
    public void Is_Built_Correctly()
    {
        // Arrange
        var builder = new LanguageBuilder();

        var expected = CultureInfo.GetCultureInfo("en-GB");

        // Act
        var language = builder
            .WithCultureInfo(expected.Name)
            .Build();

        // Assert
        Assert.AreEqual(expected.Name, language.IsoCode);
        Assert.AreEqual(expected.EnglishName, language.CultureName);
    }
}
