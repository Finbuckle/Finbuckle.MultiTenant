// Copyright Finbuckle LLC, Andrew White, and Contributors.
// Refer to the solution LICENSE file for more inforation.

using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace Finbuckle.MultiTenant.EntityFrameworkCore.Test
{
    public class TestBlogDbContext : MultiTenantDbContext
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

    public class TestDbContextWithExistingGlobalFilter : TestBlogDbContext
    {
        public TestDbContextWithExistingGlobalFilter(TenantInfo tenantInfo) : base(tenantInfo)
        {
        }

        public TestDbContextWithExistingGlobalFilter(TenantInfo tenantInfo, DbContextOptions options) : base(tenantInfo, options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Post>().HasQueryFilter(p => p.Title == "Filtered Title");

            base.OnModelCreating(modelBuilder);
        }
    }

// public class TestWrongTenantIdTypeDbContext : MultiTenantDbContext
// {
//     public DbSet<ThingWithIntTenantId> Thing2s { get; set; }

//     public TestWrongTenantIdTypeDbContext(TenantInfo tenantInfo,
//         DbContextOptions<TestWrongTenantIdTypeDbContext> options) :
//         base(tenantInfo, options)
//     { }
// }

// public class TestTenantIdConstraintsTypeDbContext : MultiTenantDbContext
// {
//     public DbSet<Post> PostWithShadowTenantId { get; set; }
//     public DbSet<ThingWithTenantId> ThingsWithTenantId { get; set; }
//     public DbSet<ThingWithLowerTenantIdMaxLength> ThingsWithLowerTenantIdsMaxLength { get; set; }

//     public DbSet<ThingWithHigherTenantIdMaxLength> ThingsWithHigherTenantIdsMaxLength { get; set; }

//     public TestTenantIdConstraintsTypeDbContext(TenantInfo tenantInfo,
//         DbContextOptions<TestTenantIdConstraintsTypeDbContext> options) :
//         base(tenantInfo, options)
//     { }
// }

// public class NonMultiTenantThing
// {
//     public int Id { get; set; }
//     public List<Blog> Blogs { get; set; }
// }

    [MultiTenant]
    public class Blog
    {
        public int BlogId { get; set; }
        public string Title { get; set; }
        public List<Post> Posts { get; set; }
    }

    [MultiTenant]
    public class Post
    {
        public int PostId { get; set; }
        public string Title { get; set; }

        public int BlogId { get; set; }
        public Blog Blog { get; set; }
    }
}