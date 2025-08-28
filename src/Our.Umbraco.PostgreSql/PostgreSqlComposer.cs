using Umbraco.Cms.Core.Composing;
using Umbraco.Cms.Core.DependencyInjection;

namespace Our.Umbraco.PostgreSql;

/// <summary>
///     Automatically adds SQL Server support to Umbraco when this project is referenced.
/// </summary>
public class PostgreSqlComposer : IComposer
{
    /// <inheritdoc />
    public void Compose(IUmbracoBuilder builder)
    {
        builder.AddUmbracoPostgreSqlSupport();
    }
}
