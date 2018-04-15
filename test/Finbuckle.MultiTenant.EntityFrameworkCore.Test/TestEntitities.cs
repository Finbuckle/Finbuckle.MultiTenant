using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using Finbuckle.MultiTenant.EntityFrameworkCore;
using Finbuckle.MultiTenant.Core;
using System.ComponentModel.DataAnnotations;

public class TestDbContext : MultiTenantDbContext
{
    public DbSet<Config> Configs { get; set; }
    public DbSet<Blog> Blogs { get; set; }
    public DbSet<Post> Posts { get; set; }
    public DbSet<ThingWithTenantId> Things { get; set; }

    public TestDbContext(TenantContext tenantContext,
        DbContextOptions<TestDbContext> options) :
        base(tenantContext, options)
    { }
}

public class TestWrongTenantIdTypeDbContext : MultiTenantDbContext
{
    public DbSet<ThingWithIntTenantId> Thing2s { get; set; }

    public TestWrongTenantIdTypeDbContext(TenantContext tenantContext,
        DbContextOptions<TestWrongTenantIdTypeDbContext> options) :
        base(tenantContext, options)
    { }
}

public class TestTenantIdConstraintsTypeDbContext : MultiTenantDbContext
{
    public DbSet<Post> PostWithShadowTenantId { get; set; }
    public DbSet<ThingWithTenantId> ThingsWithTenantId { get; set; }
    public DbSet<ThingWithLowerTenantIdMaxLength> ThingsWithLowerTenantIdsMaxLength { get; set; }

    public DbSet<ThingWithHigherTenantIdMaxLength> ThingsWithHigherTenantIdsMaxLength { get; set; }

    public TestTenantIdConstraintsTypeDbContext(TenantContext tenantContext,
        DbContextOptions<TestTenantIdConstraintsTypeDbContext> options) :
        base(tenantContext, options)
    { }
}


public class Config
{
    public int ConfigId { get; set; }
    public List<Blog> Blogs { get; set; }
}

[MultiTenant]
public class Blog
{
    public int BlogId { get; set; }
    public string Title { get; set; }

    public int? ConfigId { get; set; }
    public Config Config { get; set; }
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

[MultiTenant]
public class ThingWithTenantId
{
    public int Id { get; set; }
    public string Title { get; set; }

    public string TenantId { get; set; }
}

[MultiTenant]
public class ThingWithIntTenantId
{
    public int Id { get; set; }
    public string Title { get; set; }

    public int TenantId { get; set; }
}

[MultiTenant]
public class ThingWithLowerTenantIdMaxLength
{
    public int Id { get; set; }
    public string Title { get; set; }

    [MaxLength(10)]
    public string TenantId { get; set; }
}

[MultiTenant]
public class ThingWithHigherTenantIdMaxLength
{
    public int Id { get; set; }
    public string Title { get; set; }

    [MaxLength(100)]
    public string TenantId { get; set; }
}