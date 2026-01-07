// Copyright Finbuckle LLC, Andrew White, and Contributors.
// Refer to the solution LICENSE file for more information.

using Finbuckle.MultiTenant.Abstractions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;

namespace Finbuckle.MultiTenant.EntityFrameworkCore.Test.Extensions.MultiTenantEntityTypeBuilderExtensions;

public class TestDbContext : EntityFrameworkCore.MultiTenantDbContext
{
    private readonly Action<ModelBuilder> _config;

    public TestDbContext(Action<ModelBuilder> config, DbContextOptions options) :
        base(new StaticMultiTenantContextAccessor<TenantInfo>(new TenantInfo { Id = "dummy", Identifier = "" }), options)
    {
        _config = config;
    }

    public DbSet<Blog>? Blogs { get; set; }
    public DbSet<Post>? Posts { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseSqlite("DataSource=:memory:");
        base.OnConfiguring(optionsBuilder);
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        _config(modelBuilder);
    }
}

public class Blog
{
    public int BlogId { get; set; }
    public string? Url { get; set; }

    public List<Post>? Posts { get; set; }
}

public class Post
{
    public int PostId { get; set; }
    public string? Title { get; set; }
    public string? Content { get; set; }

    public Blog? Blog { get; set; }
}

public class DynamicModelCacheKeyFactory : IModelCacheKeyFactory
{
    public object Create(DbContext context)
    {
        return new object();
    }

    public object Create(DbContext context, bool designTime)
    {
        // Needed for tests that change the model.
        return new object();
    }
}