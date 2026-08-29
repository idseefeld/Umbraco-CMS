// Copyright (c) Umbraco.
// See LICENSE for more details.

using NUnit.Framework;
using Umbraco.Cms.Infrastructure.Persistence;
using Umbraco.Cms.Infrastructure.Persistence.SqlSyntax;
using Umbraco.Cms.Tests.Common.Testing;
using Umbraco.Cms.Tests.Integration.Testing;
using Umbraco.Extensions;

namespace Umbraco.Cms.Tests.Integration.Umbraco.Infrastructure.Persistence;

[TestFixture]
[UmbracoTest(Database = UmbracoTestOptions.Database.NewSchemaPerTest)]
internal sealed class TruncateTableTests : UmbracoIntegrationTest
{
    private const string TableName = "testTruncateTable";

    [Test]
    public void Can_Truncate_A_Table()
    {
        var tableName = SqlContext.SqlSyntax.GetQuotedTableName(TableName);
        // Arrange - a table of our own, so this asserts that emptying a table works rather than
        // anything about a particular table's contents.
        using (ScopeProvider.CreateScope(autoComplete: true))
        {
            IUmbracoDatabase database = ScopeAccessor.AmbientScope!.Database;
            database.Execute($"CREATE TABLE {tableName} (id int NOT NULL)");
            database.Execute($"INSERT INTO {tableName} (id) VALUES (1)");
            database.Execute($"INSERT INTO {tableName} (id) VALUES (2)");
        }

        Assume.That(CountRows(tableName), Is.EqualTo(2));

        // Act
        using (ScopeProvider.CreateScope(autoComplete: true))
        {
            ScopeAccessor.AmbientScope!.Database.TruncateTable(SqlContext.SqlSyntax, TableName);
        }

        // Assert
        Assert.That(CountRows(tableName), Is.Zero);
    }

    private int CountRows(string tableName)
    {
        using var scope = ScopeProvider.CreateScope(autoComplete: true);
        return ScopeAccessor.AmbientScope!.Database.ExecuteScalar<int>($"SELECT COUNT(*) FROM {tableName}");
    }
}
