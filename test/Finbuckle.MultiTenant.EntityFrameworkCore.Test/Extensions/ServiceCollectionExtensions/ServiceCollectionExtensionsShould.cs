// Copyright Finbuckle LLC, Andrew White, and Contributors.
// Refer to the solution LICENSE file for more information.

using Finbuckle.MultiTenant.Abstractions;
using Finbuckle.MultiTenant.EntityFrameworkCore.Extensions;
using Finbuckle.MultiTenant.Extensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Finbuckle.MultiTenant.EntityFrameworkCore.Test.Extensions.ServiceCollectionExtensions;

public class ServiceCollectionExtensionsShould
{
    private static IServiceProvider BuildServiceProvider(string dbName)
    {
        var services = new ServiceCollection();
        services.AddMultiTenant<TenantInfo>();
        services.AddPooledMultiTenantDbContext<PooledTestDbContext>(
            options => options.UseSqlite($"DataSource={dbName};Mode=Memory;Cache=Shared"),
            poolSize: 1);
        return services.BuildServiceProvider();
    }

    [Fact]
    public void ReusesPooledContextInstance()
    {
        var sp = BuildServiceProvider("pool-reuse-test");
        var setter = sp.GetRequiredService<IMultiTenantContextSetter>();
        var tenant = new TenantInfo { Id = "tenant-a", Identifier = "tenant-a" };
        setter.MultiTenantContext = new MultiTenantContext<TenantInfo>(tenant);

        PooledTestDbContext ctx1;
        using (var scope1 = sp.CreateScope())
        {
            ctx1 = scope1.ServiceProvider.GetRequiredService<PooledTestDbContext>();
        }
        // scope1 disposed – context returned to pool

        using var scope2 = sp.CreateScope();
        var ctx2 = scope2.ServiceProvider.GetRequiredService<PooledTestDbContext>();

        Assert.True(ReferenceEquals(ctx1, ctx2));
    }

    [Fact]
    public void ClearsChangeTrackerAndRebindsTenantOnReuse()
    {
        var sp = BuildServiceProvider("pool-clear-rebind-test");
        var setter = sp.GetRequiredService<IMultiTenantContextSetter>();
        var tenantA = new TenantInfo { Id = "tenant-a", Identifier = "tenant-a" };
        var tenantB = new TenantInfo { Id = "tenant-b", Identifier = "tenant-b" };

        // Scope 1: track an unsaved entity
        using (var scope1 = sp.CreateScope())
        {
            setter.MultiTenantContext = new MultiTenantContext<TenantInfo>(tenantA);
            var ctx1 = scope1.ServiceProvider.GetRequiredService<PooledTestDbContext>();
            ctx1.Add(new PooledBlog { Title = "unsaved" });
            Assert.Single(ctx1.ChangeTracker.Entries());
        }
        // ctx1 returned to pool with state intact; AddPooledMultiTenantDbContext clears it on next resolution

        // Scope 2: same pooled context instance, but cleared and rebound to tenantB
        setter.MultiTenantContext = new MultiTenantContext<TenantInfo>(tenantB);
        using var scope2 = sp.CreateScope();
        var ctx2 = scope2.ServiceProvider.GetRequiredService<PooledTestDbContext>();

        Assert.Empty(ctx2.ChangeTracker.Entries());
        Assert.Equal(tenantB.Id, ctx2.TenantInfo!.Id);
    }
}

public class PooledTestDbContext(
    IMultiTenantContextAccessor multiTenantContextAccessor,
    DbContextOptions<PooledTestDbContext> options)
    : EntityFrameworkCore.MultiTenantDbContext(multiTenantContextAccessor, options)
{
    public DbSet<PooledBlog>? Blogs { get; set; }
}

[MultiTenant]
public class PooledBlog
{
    public int Id { get; set; }
    public string? Title { get; set; }
}

