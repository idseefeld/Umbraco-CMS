using EFCoreBlogFeatures.Dto;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Persistence.EFCore.Scoping;

namespace EFCoreBlogFeatures;

[ApiController]
[Route("/umbraco/api/blogcomments")]
public class BlogCommentsController : Controller
{
    private readonly IEFCoreScopeProvider<BlogDbContext> _efCoreScopeProvider;

    public BlogCommentsController(IEFCoreScopeProvider<BlogDbContext> efCoreScopeProvider)
        => _efCoreScopeProvider = efCoreScopeProvider;

    [HttpGet("all")]
    public async Task<IActionResult> All()
    {
        using IEfCoreScope<BlogDbContext> scope = _efCoreScopeProvider.CreateScope();
        IEnumerable<BlogComment> comments = await scope.ExecuteWithContextAsync(async db => db.BlogComments.ToArray());
        scope.Complete();
        return Ok(comments);
    }

    [HttpGet("getcomments")]
    public async Task<IActionResult> GetComments(Guid umbracoNodeKey)
    {
        using IEfCoreScope<BlogDbContext> scope = _efCoreScopeProvider.CreateScope();
        IEnumerable<BlogComment> comments = await scope.ExecuteWithContextAsync(async db =>
        {
            return db.BlogComments.Where(x => x.BlogPostUmbracoKey == umbracoNodeKey).ToArray();
        });

        scope.Complete();
        return Ok(comments);
    }

    [HttpPost("insertcomment")]
    public async Task InsertComment(BlogComment comment)
    {
        using IEfCoreScope<BlogDbContext> scope = _efCoreScopeProvider.CreateScope();

        await scope.ExecuteWithContextAsync<Task>(async db =>
        {
            db.BlogComments.Add(comment);
            await db.SaveChangesAsync();
        });

        scope.Complete();
    }
}
