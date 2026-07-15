// Copyright Finbuckle LLC, Andrew White, and Contributors.
// Refer to the solution LICENSE file for more information.

using Finbuckle.MultiTenant.Abstractions;
using Microsoft.EntityFrameworkCore;

namespace Finbuckle.MultiTenant.EntityFrameworkCore.Test.MultiTenantDbContext;

public class TestBlogDbContext : EntityFrameworkCore.MultiTenantDbContext
{
    public DbSet<Blog>? Blogs { get; set; }
    public DbSet<Post>? Posts { get; set; }

    public TestBlogDbContext()
    {
    }

    public TestBlogDbContext(DbContextOptions options) : base(options)
    {
    }

    public TestBlogDbContext(object dependency)
    {
    }
}

[MultiTenant]
public class Blog
{
    public int BlogId { get; set; }
    public string? Title { get; set; }
    public List<Post>? Posts { get; set; }
}

[MultiTenant]
public class Post
{
    public int PostId { get; set; }
    public string? Title { get; set; }

    public int BlogId { get; set; }
    public Blog? Blog { get; set; }
}
