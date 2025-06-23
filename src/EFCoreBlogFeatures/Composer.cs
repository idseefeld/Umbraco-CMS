using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Umbraco.Cms.Core.Composing;
using Umbraco.Cms.Core.DependencyInjection;
using Umbraco.Cms.Core.Notifications;
using Umbraco.Extensions;

namespace EFCoreBlogFeatures;

public class Composer : IComposer
{
    public void Compose(IUmbracoBuilder builder)
    {
        // Build a temporary service provider to access IConfiguration
        using var serviceProvider = builder.Services.BuildServiceProvider();
        var config = serviceProvider.GetRequiredService<IConfiguration>();
        var connectionString = config.GetConnectionString("umbracoDbDSN");

        const bool useSQLite = false;
        if (useSQLite)
        {
            _ = builder.Services.AddUmbracoDbContext<BlogContext>(options =>
            {
                options.UseSqlite(connectionString);
            });
        }
        else
        {
            _ = builder.Services.AddUmbracoDbContext<BlogContext>(options =>
            {
                options.UseSqlServer(connectionString);
            });
        }

        builder.AddNotificationAsyncHandler<UmbracoApplicationStartedNotification, RunBlogCommentsMigration>();
    }
}

