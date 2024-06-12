using Microsoft.Extensions.Logging;
using Redgum.Upserter.Examples.Data;
using Redgum.Upserter.Services;

namespace Redgum.Upserter.Examples;

public interface IPostUpserter
{
    UpsertResult<PostEntity, PostModel> Upsert(IEnumerable<PostEntity> existingItems, IEnumerable<PostModel> suppliedItems, BlogEntity parent);
}

public class PostUpserter : UpserterService<PostEntity, PostModel, BlogEntity>, IPostUpserter
{
    private readonly ILogger _logger;
    private readonly BloggingContext _dbContext;
    public PostUpserter( ILogger<PostUpserter> logger, BloggingContext dbContext)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
    }

    public UpsertResult<PostEntity, PostModel> Upsert(
        IEnumerable<PostEntity> existingItems,
        IEnumerable<PostModel> suppliedItems,
        BlogEntity parent
        )
    {
        _logger.LogInformation("PostUpserter.Upsert");

        ArgumentNullException.ThrowIfNull(existingItems);
        ArgumentNullException.ThrowIfNull(suppliedItems);
        ArgumentNullException.ThrowIfNull(parent);

        return base.Upsert(
            existingItems: existingItems,
            suppliedItems: suppliedItems,
            selectKeyForExistingItems: i => i.Gid,
            selectKeyForSuppliedItems: i => i.Gid,
            data: parent
            );
    }

    protected override void Insert(PostModel suppliedItem, BlogEntity data)
    {
        _logger.LogInformation("PostUpserter.Insert");

        var newPost = new PostEntity
        {
            Gid = suppliedItem.Gid,
            Title = suppliedItem.Title,
            Content = suppliedItem.Content,
            Blog = data
        };
        data.Posts.Add(newPost);
        _dbContext.Posts.Add(newPost);
    }

    protected override void Update(PostEntity existingItem, PostModel suppliedItem, BlogEntity data)
    {
        _logger.LogInformation("PostUpserter.Update");

        existingItem.Title = suppliedItem.Title;
        existingItem.Content = suppliedItem.Content;
    }

    protected override void Delete(PostEntity existingItem, BlogEntity data)
    {
        _logger.LogInformation("PostUpserter.Delete");

        existingItem.IsDeleted = true;

        // we could also remove the link to the parent
        //data.Posts.Remove(existingItem);
        // and then hard delete the item
        //_dbContext.Blogs.Remove(existingItem);
    }
}