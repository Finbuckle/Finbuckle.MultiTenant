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
    private static IServiceProvider BuildPooledServiceProvider(string dbName)
    {
        var services = new ServiceCollection();
        services.AddMultiTenant<TenantInfo>();
        services.AddPooledMultiTenantDbContext<PooledTestDbContext>(
            options => options.UseSqlite($"DataSource={dbName};Mode=Memory;Cache=Shared"),
            poolSize: 1);
        return services.BuildServiceProvider();
    }

    private static IServiceProvider BuildNonPooledServiceProvider(string dbName)
    {
        var services = new ServiceCollection();
        services.AddMultiTenant<TenantInfo>();
        services.AddMultiTenantDbContext<NonPooledTestDbContext>(
            options => options.UseSqlite($"DataSource={dbName};Mode=Memory;Cache=Shared"));
        return services.BuildServiceProvider();
    }

    // AddPooledMultiTenantDbContext tests

    [Fact]
    public void AddPooledMultiTenantDbContext_ReusesPooledContextInstance()
    {
        var sp = BuildPooledServiceProvider("pool-reuse-test");
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
    public void AddPooledMultiTenantDbContext_ClearsChangeTrackerAndRebindsTenantOnReuse()
    {
        var sp = BuildPooledServiceProvider("pool-clear-rebind-test");
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

    // AddMultiTenantDbContext tests

    [Fact]
    public void AddMultiTenantDbContext_RegistersDbContextAsScoped()
    {
        var services = new ServiceCollection();
        services.AddMultiTenant<TenantInfo>();
        services.AddMultiTenantDbContext<NonPooledTestDbContext>(
            options => options.UseSqlite("DataSource=:memory:"));

        var descriptor = services.FirstOrDefault(d => d.ServiceType == typeof(NonPooledTestDbContext));
        Assert.NotNull(descriptor);
        Assert.Equal(ServiceLifetime.Scoped, descriptor.Lifetime);
    }

    [Fact]
    public void AddMultiTenantDbContext_CreatesNewInstancePerScope()
    {
        var sp = BuildNonPooledServiceProvider("non-pooled-new-instance");
        var setter = sp.GetRequiredService<IMultiTenantContextSetter>();
        var tenant = new TenantInfo { Id = "tenant-a", Identifier = "tenant-a" };
        setter.MultiTenantContext = new MultiTenantContext<TenantInfo>(tenant);

        NonPooledTestDbContext ctx1;
        using (var scope1 = sp.CreateScope())
        {
            ctx1 = scope1.ServiceProvider.GetRequiredService<NonPooledTestDbContext>();
        }

        using var scope2 = sp.CreateScope();
        var ctx2 = scope2.ServiceProvider.GetRequiredService<NonPooledTestDbContext>();

        Assert.False(ReferenceEquals(ctx1, ctx2));
    }

    [Fact]
    public void AddMultiTenantDbContext_SetsTenantInfoOnContext()
    {
        var sp = BuildNonPooledServiceProvider("non-pooled-tenant-info");
        var setter = sp.GetRequiredService<IMultiTenantContextSetter>();
        var tenant = new TenantInfo { Id = "tenant-x", Identifier = "tenant-x" };
        setter.MultiTenantContext = new MultiTenantContext<TenantInfo>(tenant);

        using var scope = sp.CreateScope();
        var ctx = scope.ServiceProvider.GetRequiredService<NonPooledTestDbContext>();

        Assert.Equal("tenant-x", ctx.TenantInfo!.Id);
    }

    [Fact]
    public void AddMultiTenantDbContext_EnforcesMultiTenantOnTracking()
    {
        var sp = BuildNonPooledServiceProvider("non-pooled-tracking");
        var setter = sp.GetRequiredService<IMultiTenantContextSetter>();
        var tenant = new TenantInfo { Id = "tenant-t", Identifier = "tenant-t" };
        setter.MultiTenantContext = new MultiTenantContext<TenantInfo>(tenant);

        using var scope = sp.CreateScope();
        var ctx = scope.ServiceProvider.GetRequiredService<NonPooledTestDbContext>();
        ctx.Add(new NonPooledBlog { Title = "test" });

        var tenantId = ctx.ChangeTracker.Entries().Single().Property("TenantId").CurrentValue;
        Assert.Equal("tenant-t", tenantId);
    }

    [Fact]
    public void AddMultiTenantDbContext_ServiceProviderOverloadPassesServiceProviderToOptionsAction()
    {
        string? capturedValue = null;
        var services = new ServiceCollection();
        services.AddMultiTenant<TenantInfo>();
        services.AddSingleton("test-connection-string");
        services.AddMultiTenantDbContext<NonPooledTestDbContext>((sp, options) =>
        {
            capturedValue = sp.GetRequiredService<string>();
            options.UseSqlite("DataSource=:memory:");
        });

        var provider = services.BuildServiceProvider();
        var setter = provider.GetRequiredService<IMultiTenantContextSetter>();
        setter.MultiTenantContext = new MultiTenantContext<TenantInfo>(
            new TenantInfo { Id = "t", Identifier = "t" });

        using var scope = provider.CreateScope();
        _ = scope.ServiceProvider.GetRequiredService<NonPooledTestDbContext>();

        Assert.Equal("test-connection-string", capturedValue);
    }

    [Fact]
    public void AddMultiTenantDbContext_ParameterlessOverloadRegistersDbContextAsScoped()
    {
        var services = new ServiceCollection();
        services.AddMultiTenant<TenantInfo>();
        services.AddMultiTenantDbContext<NonPooledTestDbContext>();

        var descriptor = services.FirstOrDefault(d => d.ServiceType == typeof(NonPooledTestDbContext));
        Assert.NotNull(descriptor);
        Assert.Equal(ServiceLifetime.Scoped, descriptor.Lifetime);
    }
}

public class NonPooledTestDbContext(
    IMultiTenantContextAccessor multiTenantContextAccessor,
    DbContextOptions<NonPooledTestDbContext> options)
    : EntityFrameworkCore.MultiTenantDbContext(multiTenantContextAccessor, options)
{
    public DbSet<NonPooledBlog>? Blogs { get; set; }
}

[MultiTenant]
public class NonPooledBlog
{
    public int Id { get; set; }
    public string? Title { get; set; }
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

