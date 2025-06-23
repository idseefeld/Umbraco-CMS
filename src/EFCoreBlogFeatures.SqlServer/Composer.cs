using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Umbraco.Cms.Core.Composing;
using Umbraco.Cms.Core.DependencyInjection;
using Umbraco.Cms.Core.Notifications;
using Umbraco.Extensions;

namespace EFCoreBlogFeatures.SqlServer;

public class Composer : IComposer
{
    public void Compose(IUmbracoBuilder builder)
    {
        // Build a temporary service provider to access IConfiguration
        using var serviceProvider = builder.Services.BuildServiceProvider();
        var config = serviceProvider.GetRequiredService<IConfiguration>();
        var connectionString = config.GetConnectionString("umbracoDbDSN");

        string provider = config.GetConnectionString("umbracoDbDSN_ProviderName")
            ?? string.Empty;
        if (provider != Constants.ProviderNames.SQLServer)
        { return; }

        _ = builder.Services.AddUmbracoDbContext<BlogDbContext>(options =>
        {
            var assemblyName = GetType().Assembly.FullName;
            Console.WriteLine($"Using PostgreSQL with assembly: {assemblyName}");
            options.UseSqlServer(connectionString, b => b.MigrationsAssembly(assemblyName));// ("EFCoreBlogFeatures.SqlServer"));
        });

        builder.AddNotificationAsyncHandler<UmbracoApplicationStartedNotification, RunBlogCommentsMigration>();
    }
}

