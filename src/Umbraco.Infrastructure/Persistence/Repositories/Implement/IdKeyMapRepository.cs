using NPoco;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Persistence.Repositories;
using Umbraco.Cms.Infrastructure.Persistence.Dtos;
using Umbraco.Cms.Infrastructure.Scoping;
using Umbraco.Extensions;

namespace Umbraco.Cms.Infrastructure.Persistence.Repositories.Implement;

/// <summary>
///     Repository for mapping between integer IDs and GUID keys for Umbraco objects.
///     Provides methods to retrieve an ID for a given key and object type, and vice versa.
/// </summary>
public class IdKeyMapRepository(IScopeAccessor scopeAccessor) : IIdKeyMapRepository
{
    private readonly ISqlContext? _sqlContext = scopeAccessor.AmbientScope?.SqlContext;

    private readonly SqlSyntax.ISqlSyntaxProvider? _sqlSyntax = scopeAccessor.AmbientScope?.SqlContext.SqlSyntax;

    private Sql<ISqlContext> EnsureSqlContext() => _sqlContext?.Sql() ?? throw new InvalidOperationException("No SQL context available.");

    private SqlSyntax.ISqlSyntaxProvider EnsureSqlSyntax()
        => _sqlSyntax ?? throw new InvalidOperationException("No SQL syntax provider available.");

    /// <summary>
    /// Retrieves the identifier (ID) associated with the specified key and object type.
    /// </summary>
    /// <remarks>If <paramref name="umbracoObjectType"/> is <see cref="UmbracoObjectTypes.Unknown"/>, the
    /// query will only match the key without considering the object type. For other object types, the query will match
    /// both the key and the specified object type or reserved object type.</remarks>
    /// <param name="key">The unique identifier (GUID) of the object to look up.</param>
    /// <param name="umbracoObjectType">The type of the Umbraco object to query. Use <see cref="UmbracoObjectTypes.Unknown"/> to exclude the object type
    /// from the query.</param>
    /// <returns>The identifier (ID) of the object if found; otherwise, <see langword="null"/>.</returns>
    public int? GetIdForKey(Guid key, UmbracoObjectTypes umbracoObjectType)
    {
        Sql<ISqlContext>? sql;

        // if it's unknown don't include the nodeObjectType in the query
        if (umbracoObjectType == UmbracoObjectTypes.Unknown)
        {
            sql = EnsureSqlContext()
                .Select(EnsureSqlSyntax().GetQuotedColumnName(NodeDto.IdColumnName))
                .From<NodeDto>()
                .Where<NodeDto>(n => n.UniqueId == key);
            return scopeAccessor.AmbientScope?.Database.ExecuteScalar<int?>(sql);
        }

        sql = EnsureSqlContext()
                .Select(EnsureSqlSyntax().GetQuotedColumnName(NodeDto.IdColumnName))
                .From<NodeDto>()
                .Where<NodeDto>(n =>
                    n.UniqueId == key
                    && (
                        n.NodeObjectType == GetNodeObjectTypeGuid(umbracoObjectType)
                        || n.NodeObjectType == Constants.ObjectTypes.IdReservation));
        return scopeAccessor.AmbientScope?.Database.ExecuteScalar<int?>(sql);
    }

    /// <summary>
    /// Retrieves the unique identifier (GUID) associated with a specified node ID and object type.
    /// </summary>
    /// <remarks>If <paramref name="umbracoObjectType"/> is <see cref="UmbracoObjectTypes.Unknown"/>, the
    /// query will not filter by object type.</remarks>
    /// <param name="id">The ID of the node to retrieve the unique identifier for.</param>
    /// <param name="umbracoObjectType">The type of the Umbraco object to filter the query. Use <see cref="UmbracoObjectTypes.Unknown"/> to ignore the
    /// object type filter.</param>
    /// <returns>The unique identifier (GUID) of the node if found; otherwise, <see langword="null"/>.</returns>
    public Guid? GetIdForKey(int id, UmbracoObjectTypes umbracoObjectType)
    {
        Sql<ISqlContext>? sql;

        // if it's unknown don't include the nodeObjectType in the query
        if (umbracoObjectType == UmbracoObjectTypes.Unknown)
        {
            sql = EnsureSqlContext()
                .Select(EnsureSqlSyntax().GetQuotedColumnName(NodeDto.IdColumnName))
                .From<NodeDto>()
                .Where<NodeDto>(n => n.NodeId == id);
            return scopeAccessor.AmbientScope?.Database.ExecuteScalar<Guid?>(sql);
        }

        sql = EnsureSqlContext()
                .Select(EnsureSqlSyntax().GetQuotedColumnName(NodeDto.IdColumnName))
                .From<NodeDto>()
                .Where<NodeDto>(n =>
                    n.NodeId == id
                    && (
                        n.NodeObjectType == GetNodeObjectTypeGuid(umbracoObjectType)
                        || n.NodeObjectType == Constants.ObjectTypes.IdReservation));
        return scopeAccessor.AmbientScope?.Database.ExecuteScalar<Guid?>(sql);
    }

    private Guid GetNodeObjectTypeGuid(UmbracoObjectTypes umbracoObjectType)
    {
        Guid guid = umbracoObjectType.GetGuid();
        if (guid == Guid.Empty)
        {
            throw new NotSupportedException("Unsupported object type (" + umbracoObjectType + ").");
        }

        return guid;
    }
}
