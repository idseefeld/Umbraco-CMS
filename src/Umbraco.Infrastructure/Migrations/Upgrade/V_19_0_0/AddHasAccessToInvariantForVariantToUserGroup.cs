using Microsoft.Extensions.Options;
using NPoco;
using NPoco.Expressions;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Infrastructure.Persistence;
using Umbraco.Cms.Infrastructure.Persistence.Dtos;
using Umbraco.Extensions;

namespace Umbraco.Cms.Infrastructure.Migrations.Upgrade.V_19_0_0;

/// <summary>
///     Grants the invariant-for-variant permission to the existing user groups, based on the previous
///     <see cref="ContentSettings.AllowEditInvariantFromNonDefault"/> configuration value.
/// </summary>
/// <remarks>
///     The <c>hasAccessToInvariantForVariant</c> column is added by the <see cref="AddHasAccessToInvariantForVariantColumnToUserGroup"/>
///     pre-migration.
///     This is the last consumer of <see cref="ContentSettings.AllowEditInvariantFromNonDefault"/>.
///     Do not remove the setting until Umbraco 21 per the obsoletion schedule.
/// </remarks>
public class AddHasAccessToInvariantForVariantToUserGroup : AsyncMigrationBase
{
    private const string ColumnName = "hasAccessToInvariantForVariant";

    private readonly IOptions<ContentSettings> _contentSettings;

    /// <summary>
    ///     Initializes a new instance of the <see cref="AddHasAccessToInvariantForVariantToUserGroup"/> class.
    /// </summary>
    /// <param name="context">The migration context.</param>
    /// <param name="contentSettings">The content settings used to backfill the permission from the previous config flag.</param>
    public AddHasAccessToInvariantForVariantToUserGroup(
        IMigrationContext context,
        IOptions<ContentSettings> contentSettings)
        : base(context)
    {
        _contentSettings = contentSettings;
    }

    /// <inheritdoc />
    protected override Task MigrateAsync()
    {
        if (TableExists(Constants.DatabaseSchema.Tables.UserGroup) is false)
        {
            return Task.CompletedTask;
        }

        BackfillFromLegacySetting();

        return Task.CompletedTask;
    }

    private void BackfillFromLegacySetting()
    {
#pragma warning disable CS0618 // Type or member is obsolete
        var wasPermissive = _contentSettings.Value.AllowEditInvariantFromNonDefault;
#pragma warning restore CS0618 // Type or member is obsolete

        if (wasPermissive)
        {
            // The old setting was permissive at the tenant level: every group gets the permission.
            Database.Execute(Sql()
                .Update<UserGroupDto>(c => c.Set(n => n.HasAccessToInvariantForVariant, true)));
            return;
        }

        // The old setting was restrictive: only grant to groups that could already edit invariant data
        // under the old default-language coupling, plus the built-in admin group (id = 1).
        Sql<ISqlContext> inSql = Sql()
            .Select<UserGroup2LanguageDto>(l => l.UserGroupId)
            .From<UserGroup2LanguageDto>()
            .InnerJoin<LanguageDto>()
            .On<UserGroup2LanguageDto, LanguageDto>((l, lang) => l.LanguageId == lang.Id)
            .Where<LanguageDto>(lang => lang.IsDefault);

        Sql<ISqlContext> sql2 = Sql()
            .Update<UserGroupDto>(c => c.Set(n => n.HasAccessToInvariantForVariant, true))
            .Where<UserGroupDto>(c => c.HasAccessToAllLanguages || c.Id == 1 || c.Id.In(inSql));
        Database.Execute(sql2);
    }
}
