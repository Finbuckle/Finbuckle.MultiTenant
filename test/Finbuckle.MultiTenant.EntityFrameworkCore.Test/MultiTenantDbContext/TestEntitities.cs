// Copyright Finbuckle LLC, Andrew White, and Contributors.
// Refer to the solution LICENSE file for more inforation.

using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace Finbuckle.MultiTenant.EntityFrameworkCore.Test.MultiTenantDbContext
{
    public class TestBlogDbContext : MultiTenant.MultiTenantDbContext
    {
        public DbSet<Blog> Blogs { get; set; }
        public DbSet<Post> Posts { get; set; }

        public TestBlogDbContext(TenantInfo tenantInfo,
            DbContextOptions options) :
            base(tenantInfo, options)
        {
        }

        public TestBlogDbContext(TenantInfo tenantInfo) : base(tenantInfo)
        {
        }
    }

    [MultiTenant]
    // ReSharper disable once ClassNeverInstantiated.Global
    public class Blog
    {
        public int BlogId { get; set; }
        public string Title { get; set; }
        public List<Post> Posts { get; set; }
    }

    [MultiTenant]
    // ReSharper disable once ClassNeverInstantiated.Global
    public class Post
    {
        public int PostId { get; set; }
        public string Title { get; set; }

        public int BlogId { get; set; }
        public Blog Blog { get; set; }
    }
}