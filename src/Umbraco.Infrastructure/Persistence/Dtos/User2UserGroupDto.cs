using NPoco;
using Umbraco.Cms.Core;
using Umbraco.Cms.Infrastructure.Persistence.DatabaseAnnotations;

namespace Umbraco.Cms.Infrastructure.Persistence.Dtos;

[TableName(TableName)]
[PrimaryKey([UserIdColumnName, UserGroupIdColumnName], AutoIncrement = false)]
[ExplicitColumns]
public class User2UserGroupDto
{
    public const string TableName = Constants.DatabaseSchema.Tables.User2UserGroup;

    internal const string ReferenceMemberName = "UserId"; // should be UserIdColumnName, but for database compatibility we keep it like this

    private const string UserIdColumnName = "userId";
    private const string UserGroupIdColumnName = "userGroupId";

    [Column(UserIdColumnName)]
    [PrimaryKeyColumn(AutoIncrement = false, Name = "PK_user2userGroup", OnColumns = $"{UserIdColumnName}, {UserGroupIdColumnName}")]
    [ForeignKey(typeof(UserDto))]
    public int UserId { get; set; }

    [Column(UserGroupIdColumnName)]
    [ForeignKey(typeof(UserGroupDto))]
    public int UserGroupId { get; set; }
}
