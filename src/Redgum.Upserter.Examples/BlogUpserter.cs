using Microsoft.Extensions.Logging;
using Redgum.Upserter.Examples.Data;
using Redgum.Upserter.Services;

namespace Redgum.Upserter.Examples;

public interface IBlogUpserter
{
    UpsertResult<BlogEntity, BlogModel> Upsert(IEnumerable<BlogEntity> existingItems, IEnumerable<BlogModel> suppliedItems);
}

public class BlogUpserter : UpserterService<BlogEntity, BlogModel>, IBlogUpserter
{
    private readonly ILogger _logger;
    private readonly BloggingContext _dbContext;
    private readonly IPostUpserter _postUpserter;
    public BlogUpserter(ILogger<BlogUpserter> logger, BloggingContext dbContext, IPostUpserter postUpserter)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
        _postUpserter = postUpserter ?? throw new ArgumentNullException(nameof(postUpserter));
    }

    public UpsertResult<BlogEntity, BlogModel> Upsert(
        IEnumerable<BlogEntity> existingItems,
        IEnumerable<BlogModel> suppliedItems
        )
    {
        _logger.LogInformation("BlogUpserter.Upsert");

        ArgumentNullException.ThrowIfNull(existingItems);
        ArgumentNullException.ThrowIfNull(suppliedItems);

        return base.Upsert(
            existingItems: existingItems,
            suppliedItems: suppliedItems,
            selectKeyForExistingItems: i => i.Gid,
            selectKeyForSuppliedItems: i => i.Gid
            );
    }

    protected override void Insert(BlogModel suppliedItem)
    {
        _logger.LogInformation("BlogUpserter.Insert");

        var newBlog = new BlogEntity
        {
            Gid = suppliedItem.Gid,
            Url = suppliedItem.Url
        };
        _dbContext.Blogs.Add(newBlog);

        // this is a naïve implementation that will end up inserting all supplied items in this case because newBlog.Posts is empty
        // it's naïve because it doesn't allow for Posts to move between Blogs
        _postUpserter.Upsert(
            existingItems: newBlog.Posts,
            suppliedItems: suppliedItem.Posts,
            parent: newBlog
            );
    }

    protected override void Update(BlogEntity existingItem, BlogModel suppliedItem)
    {
        _logger.LogInformation("BlogUpserter.Update");

        existingItem.Url = suppliedItem.Url;

        _postUpserter.Upsert(
            existingItems: existingItem.Posts,
            suppliedItems: suppliedItem.Posts,
            parent: existingItem
            );
    }

    protected override void Delete(BlogEntity existingItem)
    {
        _logger.LogInformation("BlogUpserter.Delete");

        existingItem.IsDeleted = true;

        // this allows all deletion logic to live in the PostUpserter
        _ = _postUpserter.Upsert(
            existingItems: existingItem.Posts,
            suppliedItems: [],
            parent: existingItem
            );

        // we could just hard delete the item
        //_dbContext.Blogs.Remove(existingItem);
    }
}
