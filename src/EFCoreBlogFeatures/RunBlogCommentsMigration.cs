using Microsoft.EntityFrameworkCore;
using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Notifications;

namespace EFCoreBlogFeatures;

public class RunBlogCommentsMigration : INotificationAsyncHandler<UmbracoApplicationStartedNotification>
{
    private readonly BlogContext _blogContext;

    public RunBlogCommentsMigration(BlogContext blogContext)
    {
        _blogContext = blogContext;
    }

    public async Task HandleAsync(UmbracoApplicationStartedNotification notification, CancellationToken cancellationToken)
    {
        IEnumerable<string> pendingMigrations = await _blogContext.Database.GetPendingMigrationsAsync();

        if (pendingMigrations.Any())
        {
            await _blogContext.Database.MigrateAsync();
        }
    }
}
