// Copyright (c) Umbraco.
// See LICENSE for more details.

using Umbraco.Cms.Infrastructure.Migrations;

namespace Umbraco.Cms.Tests.UnitTests.PostgreSql.Umbraco.Infrastructure.Migrations.Stubs;

public class AlterUserTableMigrationStub : AsyncMigrationBase
{
    public AlterUserTableMigrationStub(IMigrationContext context)
        : base(context)
    {
    }

    protected override Task MigrateAsync()
    {
        Alter.Table("umbracoUser")
            .AddColumn("Birthday")
            .AsDateTime()
            .Nullable();

        return Task.CompletedTask;
    }
}
