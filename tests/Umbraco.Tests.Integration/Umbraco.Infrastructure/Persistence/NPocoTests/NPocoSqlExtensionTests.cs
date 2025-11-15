// Copyright (c) Umbraco.
// See LICENSE for more details.

using NPoco;
using NUnit.Framework;
using Umbraco.Cms.Tests.Common.Testing;
using Umbraco.Cms.Tests.Integration.Testing;
using Umbraco.Extensions;

namespace Umbraco.Cms.Tests.Integration.Umbraco.Infrastructure.Persistence.NPocoTests;

[TestFixture]
[UmbracoTest(Database = UmbracoTestOptions.Database.NewSchemaPerTest, WithApplication = true)]
internal sealed class NPocoSqlExtensionTests : UmbracoIntegrationTest
{
    private const string TableName = "zbFromTest";

    [Test]
    public void From_Appends_TableName()
    {
        using var scope = ScopeProvider.CreateScope();

        var sql = ScopeAccessor.AmbientScope.SqlContext.Sql()
            .SelectAll()
            .From<FromTestDto>();

        var quotedName = ScopeAccessor.AmbientScope.SqlContext.SqlSyntax.GetQuotedTableName(TableName);

        Assert.AreEqual($"SELECT *\nFROM {quotedName}", sql.SQL);
    }

    [Test]
    public void From_Appends_TableName_With_Alias_When_Specified()
    {
        using var scope = ScopeProvider.CreateScope();

        var sql = ScopeAccessor.AmbientScope.SqlContext.Sql()
            .SelectAll()
            .From<FromTestDto>("f");

        var quotedName = ScopeAccessor.AmbientScope.SqlContext.SqlSyntax.GetQuotedTableName(TableName);
        var quotedAlias = ScopeAccessor.AmbientScope.SqlContext.SqlSyntax.GetQuotedName("f");

        Assert.AreEqual($"SELECT *\nFROM {quotedName} {quotedAlias}", sql.SQL);
    }

    [Test]
    public void From_Does_Not_Append_Alias_When_Empty_Or_Whitespace()
    {
        using var scope = ScopeProvider.CreateScope();

        var sqlEmpty = ScopeAccessor.AmbientScope.SqlContext.Sql()
            .SelectAll()
            .From<FromTestDto>(string.Empty);

        var quotedName = ScopeAccessor.AmbientScope.SqlContext.SqlSyntax.GetQuotedTableName(TableName);

        Assert.AreEqual($"SELECT *\nFROM {quotedName}", sqlEmpty.SQL);

        var sqlWhitespace = ScopeAccessor.AmbientScope.SqlContext.Sql()
            .SelectAll()
            .From<FromTestDto>("   ");

        Assert.AreEqual($"SELECT *\nFROM {quotedName}", sqlWhitespace.SQL);
    }

    [TableName(TableName)]
    private class FromTestDto
    {
        // No columns required for the From() tests
    }
}
