using System.Linq.Expressions;
using Umbraco.Cms.Infrastructure.Persistence.SqlSyntax;

namespace Umbraco.Extensions;

/// <summary>
///     Provides extension methods to <see cref="ISqlSyntaxProvider" />.
/// </summary>
[Obsolete("Methods in this class are no longer used in Umbraco. Instead use the methods on ISqlSyntaxProvider directly. Scheduled for removal in Umbraco 19.")]
public static class SqlSyntaxExtensions
{
    /// <summary>
    ///     Gets a quoted table and field name.
    /// </summary>
    /// <typeparam name="TDto">The type of the DTO.</typeparam>
    /// <param name="sqlSyntax">An <see cref="ISqlSyntaxProvider" />.</param>
    /// <param name="fieldSelector">An expression specifying the field.</param>
    /// <param name="tableAlias">An optional table alias.</param>
    /// <returns></returns>
    public static string GetFieldName<TDto>(this ISqlSyntaxProvider sqlSyntax, Expression<Func<TDto, object?>> fieldSelector, string? tableAlias = null)
        => sqlSyntax.GetFieldName(fieldSelector, tableAlias);
}
