using Microsoft.Extensions.DependencyInjection;
using Our.Umbraco.PostgreSql.EFCore.Locking;
using Umbraco.Cms.Core.Composing;
using Umbraco.Cms.Core.DependencyInjection;
using Umbraco.Cms.Core.DistributedLocking;
using Umbraco.Cms.Persistence.EFCore;
using Umbraco.Cms.Persistence.EFCore.Migrations;

namespace Our.Umbraco.PostgreSql.EFCore;

public class PostgreSqlEFCoreComposer : IComposer
{
    public new void Compose(IUmbracoBuilder builder)
    {
        builder
            .AddUmbracoPostgreSqlEFCoreSupport();
    }
}
