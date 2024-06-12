using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;

namespace Redgum.Upserter.Examples.Data;

public class BloggingContext : DbContext
{
    public DbSet<BlogEntity> Blogs { get; set; }
    public DbSet<PostEntity> Posts { get; set; }

    //public string DbPath { get; }

    public BloggingContext(DbContextOptions<BloggingContext> options) : base(options)
    {
        //
    }


    //public BloggingContext()
    //{
    //    var folder = Environment.SpecialFolder.LocalApplicationData;
    //    var path = Environment.GetFolderPath(folder);
    //    DbPath = System.IO.Path.Join(path, "blogging.db");
    //}

    //// The following configures EF to create a Sqlite database file in the
    //// special "local" folder for your platform.
    //protected override void OnConfiguring(DbContextOptionsBuilder options)
    //    => options.UseSqlite($"Data Source={DbPath}");
    //}
}
public class BlogEntity
{
    public int Id { get; set; }
    public Guid Gid { get; set; }
    public string Url { get; set; } = default!;
    public bool IsDeleted { get; set; }
    public List<PostEntity> Posts { get; } = new();
}

public class PostEntity
{
    public int Id { get; set; }
    public Guid Gid { get; set; }
    public string Title { get; set; } = default!;
    public string Content { get; set; } = default!;
    public bool IsDeleted { get; set; }

    public int BlogId { get; set; }
    public BlogEntity Blog { get; set; } = default!;
}