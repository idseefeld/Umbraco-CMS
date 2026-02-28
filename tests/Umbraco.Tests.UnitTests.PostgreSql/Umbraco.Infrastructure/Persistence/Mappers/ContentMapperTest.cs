// Copyright (c) Umbraco.
// See LICENSE for more details.

using NUnit.Framework;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Infrastructure.Persistence.Mappers;
using Umbraco.Cms.Tests.UnitTests.PostgreSql.TestHelpers;

namespace Umbraco.Cms.Tests.UnitTests.PostgreSql.Umbraco.Infrastructure.Persistence.Mappers;

[TestFixture]
public class ContentMapperTest
{
    private readonly string escapeChar = Our.Umbraco.PostgreSql.Constants.EscapeTableColumAliasNames ? "\"" : string.Empty;

    [Test]
    public void Can_Map_Id_Property()
    {
        var column = new ContentMapper(TestHelper.GetMockSqlContext(), TestHelper.CreateMaps()).Map(nameof(Content.Id));
        Assert.That(column, Is.EqualTo($"{escapeChar}{Constants.DatabaseSchema.Tables.Node}{escapeChar}.{escapeChar}id{escapeChar}"));
    }

    [Test]
    public void Can_Map_Trashed_Property()
    {
        var column =
            new ContentMapper(TestHelper.GetMockSqlContext(), TestHelper.CreateMaps()).Map(nameof(Content.Trashed));
        Assert.That(column, Is.EqualTo($"{escapeChar}{Constants.DatabaseSchema.Tables.Node}{escapeChar}.{escapeChar}trashed{escapeChar}"));
    }

    [Test]
    public void Can_Map_Published_Property()
    {
        var column =
            new ContentMapper(TestHelper.GetMockSqlContext(), TestHelper.CreateMaps()).Map(nameof(Content.Published));
        Assert.That(column, Is.EqualTo($"{escapeChar}{Constants.DatabaseSchema.Tables.Document}{escapeChar}.{escapeChar}published{escapeChar}"));
    }

    [Test]
    public void Can_Map_Version_Property()
    {
        var column =
            new ContentMapper(TestHelper.GetMockSqlContext(), TestHelper.CreateMaps()).Map(nameof(Content.VersionId));
        Assert.That(column, Is.EqualTo($"{escapeChar}{Constants.DatabaseSchema.Tables.ContentVersion}{escapeChar}.{escapeChar}id{escapeChar}"));
    }
}
