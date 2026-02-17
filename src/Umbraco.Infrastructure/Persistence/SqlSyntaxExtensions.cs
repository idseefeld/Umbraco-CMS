using System.Linq.Expressions;
using System.Reflection;
using NPoco;
using Umbraco.Cms.Core;
using Umbraco.Cms.Infrastructure.Persistence.SqlSyntax;

namespace Umbraco.Extensions;

/// <summary>
///     Provides extension methods to <see cref="ISqlSyntaxProvider" />.
/// </summary>
[Obsolete("This class is not intended for public use and will be removed in version 18. Instead, use the methods on ISqlSyntaxProvider directly.")]
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
