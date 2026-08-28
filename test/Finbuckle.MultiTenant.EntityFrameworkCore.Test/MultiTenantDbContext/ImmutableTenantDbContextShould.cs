// Copyright Finbuckle LLC, Andrew White, and Contributors.
// Refer to the solution LICENSE file for more information.

using Finbuckle.MultiTenant.EntityFrameworkCore.Extensions;
using Finbuckle.MultiTenant.Extensions;
using Finbuckle.MultiTenant.Test;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Finbuckle.MultiTenant.EntityFrameworkCore.Test.MultiTenantDbContext;

// Proves the per-tenant data DbContext works with a constructor-only immutable tenant type.
public class ImmutableTenantDbContextShould
{
    // A DbContext with a single DI-friendly constructor.
    private sealed class ImmutableBlogDbContext(DbContextOptions options) : EntityFrameworkCore.MultiTenantDbContext<string>(options)
    {
        public DbSet<Blog> Blogs => Set<Blog>();
    }

    [Fact]
    public void CreateDbContextWithImmutableTenant()
    {
        var tenant = new ImmutableTenantInfo("abc", "abc");
        var db = MultiTenantDbContextExtensions.Create<TestBlogDbContext, ImmutableTenantInfo, string>(tenant,
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

        using var setup = new TestBlogDbContext(options) { TenantInfo = tenant1 };
        setup.Database.EnsureCreated();
        setup.Blogs?.Add(new Blog { Title = "tenant1 blog" });
        setup.SaveChanges();

        using var asT2 = new TestBlogDbContext(options) { TenantInfo = tenant2 };
        Assert.Equal(0, asT2.Blogs?.Count());
        asT2.Blogs?.Add(new Blog { Title = "tenant2 blog" });
        asT2.SaveChanges();

        using var asT1 = new TestBlogDbContext(options) { TenantInfo = tenant1 };
        Assert.Equal(1, asT1.Blogs?.Count());
        Assert.Equal("tenant1 blog", asT1.Blogs?.Single().Title);
    }

    [Fact]
    public void ResolveDbContextFromAmbientTenantScope()
    {
        var services = new ServiceCollection();
        services.AddMultiTenant<ImmutableTenantInfo, string>().WithInMemoryStore();
        services.AddMultiTenantDbContext<ImmutableBlogDbContext, string>(o => o.UseSqlite("DataSource=:memory:"));
        var sp = services.BuildServiceProvider();

        using var scope = sp.CreateScope();
        var tenant = new ImmutableTenantInfo("abc", "abc");
        scope.ServiceProvider.BeginTenantScope(tenant);

        var db = scope.ServiceProvider.GetRequiredService<ImmutableBlogDbContext>();
        Assert.Same(tenant, db.TenantInfo);
    }
}
