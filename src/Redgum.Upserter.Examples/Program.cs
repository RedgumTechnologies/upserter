using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Redgum.Upserter.Examples.Data;

namespace Redgum.Upserter.Examples;

internal class Program
{
    static async Task Main(string[] args)
    {
        Console.WriteLine("Hello, World!");


        HostApplicationBuilder builder = Host.CreateApplicationBuilder(args);

        builder.Services.AddLogging(builder => builder.AddConsole());
        builder.Services.AddScoped<IBlogUpserter, BlogUpserter>();
        builder.Services.AddScoped<IPostUpserter, PostUpserter>();

        builder.Services.AddDbContext<Data.BloggingContext>(options => options.UseInMemoryDatabase("MyBloggingContext"));


        using IHost host = builder.Build();

        await RunExampleUpsertAsync(host.Services);

        await host.RunAsync();
    }


    static async Task RunExampleUpsertAsync(IServiceProvider hostProvider)
    {
        using var serviceScope = hostProvider.CreateScope();
        var provider = serviceScope.ServiceProvider;

        var blogUpserter = provider.GetRequiredService<IBlogUpserter>();
        var dbContext = provider.GetRequiredService<BloggingContext>();

        // this is an extremely naïve example of how to use the Upserter
        // put together some data to to mimic data that might have been Posted to an API endpoint
        var data = new List<BlogModel> {
            new BlogModel
            {
                Gid = Guid.NewGuid(),
                Url = "http://www.example01.com",
                Posts = new List<PostModel>
                {
                    new PostModel
                    {
                        Gid = Guid.NewGuid(),
                        Title = "Blog 01 - First Post",
                        Content = "This is the first post for Blog 01"
                    },
                    new PostModel
                    {
                        Gid = Guid.NewGuid(),
                        Title = "Blog 01 - Second Post",
                        Content = "This is the second post for Blog 01"
                    }
                }
            },
            new BlogModel
            {
                Gid = Guid.NewGuid(),
                Url = "http://www.example02.com",
                Posts = new List<PostModel>
                {
                    new PostModel
                    {
                        Gid = Guid.NewGuid(),
                        Title = "Blog 02 - First Post",
                        Content = "This is the first post for Blog 02"
                    },
                    new PostModel
                    {
                        Gid = Guid.NewGuid(),
                        Title = "Blog 02 - Second Post",
                        Content = "This is the second post for Blog 02"
                    }
                }
            },
        };

        // call the Upserter
        // normally you would query the DB for existing items, but for this example we will just pass ALL the Blogs
        blogUpserter.Upsert(dbContext.Blogs, data);

        // Save the changes the Upserters have made to the DB
        await dbContext.SaveChangesAsync();

        Console.WriteLine();
    }
}


public class BlogModel
{
    public Guid Gid { get; set; }
    public string Url { get; set; } = default!;

    public List<PostModel> Posts { get; set; } = [];
}

public class PostModel
{
    public Guid Gid { get; set; }
    public string Title { get; set; } = default!;
    public string Content { get; set; } = default!;
}