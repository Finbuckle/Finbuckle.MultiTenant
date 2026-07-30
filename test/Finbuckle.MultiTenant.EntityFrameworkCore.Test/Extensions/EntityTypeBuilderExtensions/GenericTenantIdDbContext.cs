// Copyright Finbuckle LLC, Andrew White, and Contributors.
// Refer to the solution LICENSE file for more information.

using Finbuckle.MultiTenant.Abstractions;
using Finbuckle.MultiTenant.EntityFrameworkCore.Extensions;
using Microsoft.EntityFrameworkCore;

namespace Finbuckle.MultiTenant.EntityFrameworkCore.Test.Extensions.EntityTypeBuilderExtensions;

// Test context exercising a non-string TId end-to-end (shadow property, SaveChanges enforcement, query filter).
internal sealed class GenericTenantIdDbContext<TId> : EntityFrameworkCore.MultiTenantDbContext<TId>
    where TId : IEquatable<TId>
{
    public GenericTenantIdDbContext(DbContextOptions options, ITenantInfo<TId> tenantInfo) : base(options)
    {
        TenantInfo = tenantInfo;
    }

    public DbSet<GenericTenantBlog<TId>> Blogs => Set<GenericTenantBlog<TId>>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<GenericTenantBlog<TId>>().IsMultiTenant<TId>();
        base.OnModelCreating(modelBuilder);
    }
}

internal sealed class GenericTenantBlog<TId> where TId : IEquatable<TId>
{
    public int Id { get; set; }
}

// Test context exercising AdjustKey propagating a non-string TenantId into a dependent foreign key.
internal sealed class GenericTenantRelationshipDbContext<TId> : EntityFrameworkCore.MultiTenantDbContext<TId>
    where TId : IEquatable<TId>
{
    public GenericTenantRelationshipDbContext(DbContextOptions options) : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        var blogBuilder = modelBuilder.Entity<GenericTenantRelationshipBlog<TId>>();
        blogBuilder.HasMany(blog => blog.Posts).WithOne(post => post.Blog).HasForeignKey(post => post.BlogId);
        var key = blogBuilder.Metadata.FindPrimaryKey()!;
        blogBuilder.IsMultiTenant<TId>().AdjustKey(key, modelBuilder);
        base.OnModelCreating(modelBuilder);
    }
}

internal sealed class GenericTenantRelationshipBlog<TId> where TId : IEquatable<TId>
{
    public int Id { get; set; }
    public List<GenericTenantRelationshipPost<TId>> Posts { get; set; } = [];
}

internal sealed class GenericTenantRelationshipPost<TId> where TId : IEquatable<TId>
{
    public int Id { get; set; }
    public int BlogId { get; set; }
    public GenericTenantRelationshipBlog<TId>? Blog { get; set; }
}
