// Copyright Finbuckle LLC, Andrew White, and Contributors.
// Refer to the solution LICENSE file for more information.

using Finbuckle.MultiTenant.Abstractions;
using Microsoft.EntityFrameworkCore;

namespace Finbuckle.MultiTenant.EntityFrameworkCore.Test.Extensions.MultiTenantDbContextExtensions;

public class TestDbContext : EntityFrameworkCore.MultiTenantDbContext
{
    public DbSet<Blog>? Blogs { get; set; }
    public DbSet<Post>? Posts { get; set; }

    public TestDbContext(TenantInfo tenantInfo,
        DbContextOptions options) :
        base(new StaticMultiTenantContextAccessor<TenantInfo>(tenantInfo), options)
    {
    }
}

[MultiTenant]
public class Blog
{
    public int BlogId { get; set; }
    public string? Url { get; set; }
    public string? Title { get; set; }

    public List<Post>? Posts { get; set; }
}

[MultiTenant]
public class Post
{
    public int PostId { get; set; }
    public string? Title { get; set; }
    public string? Content { get; set; }

    public Blog? Blog { get; set; }
}