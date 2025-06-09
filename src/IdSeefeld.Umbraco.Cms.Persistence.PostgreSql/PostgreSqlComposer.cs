using Umbraco.Cms.Core.Composing;
using Umbraco.Cms.Core.DependencyInjection;

namespace IdSeefeld.Umbraco.Cms.Persistence.PostgreSql;

/// <summary>
///     Automatically adds SQL Server support to Umbraco when this project is referenced.
/// </summary>
public class PostgreSqlComposer : IComposer
{
    /// <inheritdoc />
    public void Compose(IUmbracoBuilder builder)
        => builder.AddUmbracoPostgreSqlSupport();
}
