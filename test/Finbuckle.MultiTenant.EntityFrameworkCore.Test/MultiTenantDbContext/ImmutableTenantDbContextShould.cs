// Copyright Finbuckle LLC, Andrew White, and Contributors.
// Refer to the solution LICENSE file for more information.

using Finbuckle.MultiTenant.Abstractions;
using Finbuckle.MultiTenant.Test;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace Finbuckle.MultiTenant.EntityFrameworkCore.Test.MultiTenantDbContext;

// Proves the per-tenant data DbContext works with a constructor-only immutable tenant type.
public class ImmutableTenantDbContextShould
{
    [Fact]
    public void CreateDbContextWithImmutableTenant()
    {
        var tenant = new ImmutableTenantInfo("abc", "abc");
        var db = EntityFrameworkCore.MultiTenantDbContext.Create<TestBlogDbContext, ImmutableTenantInfo>(tenant,
            new DbContextOptions<TestBlogDbContext>());

        Assert.Same(tenant, db.TenantInfo);
    }

    [Fact]
    public void IsolateDataByImmutableTenant()
    {
        using var connection = new SqliteConnection("DataSource=:memory:");
        connection.Open();
        var options = new DbContextOptionsBuilder().UseSqlite(connection).Options;

        var tenant1 = new ImmutableTenantInfo("t1", "t1");
        var tenant2 = new ImmutableTenantInfo("t2", "t2");

        using var setup = new TestBlogDbContext(new StaticMultiTenantContextAccessor<ImmutableTenantInfo>(tenant1), options);
        setup.Database.EnsureCreated();
        setup.Blogs?.Add(new Blog { Title = "tenant1 blog" });
        setup.SaveChanges();

        using var asT2 = new TestBlogDbContext(new StaticMultiTenantContextAccessor<ImmutableTenantInfo>(tenant2), options);
        Assert.Equal(0, asT2.Blogs?.Count());
        asT2.Blogs?.Add(new Blog { Title = "tenant2 blog" });
        asT2.SaveChanges();

        using var asT1 = new TestBlogDbContext(new StaticMultiTenantContextAccessor<ImmutableTenantInfo>(tenant1), options);
        Assert.Equal(1, asT1.Blogs?.Count());
        Assert.Equal("tenant1 blog", asT1.Blogs?.Single().Title);
    }
}
