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
    private static IServiceProvider BuildPooledServiceProvider(string dbName, TenantInfoHolder tenantHolder)
    {
        var services = new ServiceCollection();
        services.AddMultiTenant<TenantInfo>();
        services.AddSingleton(tenantHolder);
        services.AddScoped<ITenantContext<TenantInfo>>(sp =>
            new TestTenantContext<TenantInfo> { TenantInfo = sp.GetRequiredService<TenantInfoHolder>().Current });
        services.AddScoped<ITenantContext>(sp => sp.GetRequiredService<ITenantContext<TenantInfo>>());
        services.AddPooledMultiTenantDbContext<PooledTestDbContext>(
            options => options.UseSqlite($"DataSource={dbName};Mode=Memory;Cache=Shared"),
            poolSize: 1);
        return services.BuildServiceProvider();
    }

    private static IServiceProvider BuildNonPooledServiceProvider(string dbName, TenantInfoHolder tenantHolder)
    {
        var services = new ServiceCollection();
        services.AddMultiTenant<TenantInfo>();
        services.AddSingleton(tenantHolder);
        services.AddScoped<ITenantContext<TenantInfo>>(sp =>
            new TestTenantContext<TenantInfo> { TenantInfo = sp.GetRequiredService<TenantInfoHolder>().Current });
        services.AddScoped<ITenantContext>(sp => sp.GetRequiredService<ITenantContext<TenantInfo>>());
        services.AddMultiTenantDbContext<NonPooledTestDbContext>(
            options => options.UseSqlite($"DataSource={dbName};Mode=Memory;Cache=Shared"));
        return services.BuildServiceProvider();
    }

    // AddPooledMultiTenantDbContext tests

    [Fact]
    public void AddPooledMultiTenantDbContext_ReusesPooledContextInstance()
    {
        var tenantHolder = new TenantInfoHolder();
        var sp = BuildPooledServiceProvider("pool-reuse-test", tenantHolder);
        tenantHolder.Current = new TenantInfo { Id = "tenant-a", Identifier = "tenant-a" };

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
        var tenantHolder = new TenantInfoHolder();
        var sp = BuildPooledServiceProvider("pool-clear-rebind-test", tenantHolder);
        var tenantA = new TenantInfo { Id = "tenant-a", Identifier = "tenant-a" };
        var tenantB = new TenantInfo { Id = "tenant-b", Identifier = "tenant-b" };

        // Scope 1: track an unsaved entity
        tenantHolder.Current = tenantA;
        using (var scope1 = sp.CreateScope())
        {
            var ctx1 = scope1.ServiceProvider.GetRequiredService<PooledTestDbContext>();
            ctx1.Add(new PooledBlog { Title = "unsaved" });
            Assert.Single(ctx1.ChangeTracker.Entries());
        }
        // ctx1 returned to pool with state intact; AddPooledMultiTenantDbContext clears it on next resolution

        // Scope 2: same pooled context instance, but cleared and rebound to tenantB
        tenantHolder.Current = tenantB;
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
        var tenantHolder = new TenantInfoHolder();
        var sp = BuildNonPooledServiceProvider("non-pooled-new-instance", tenantHolder);
        var tenant = new TenantInfo { Id = "tenant-a", Identifier = "tenant-a" };
        tenantHolder.Current = tenant;

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
        var tenantHolder = new TenantInfoHolder();
        var sp = BuildNonPooledServiceProvider("non-pooled-tenant-info", tenantHolder);
        var tenant = new TenantInfo { Id = "tenant-x", Identifier = "tenant-x" };
        tenantHolder.Current = tenant;

        using var scope = sp.CreateScope();
        var ctx = scope.ServiceProvider.GetRequiredService<NonPooledTestDbContext>();

        Assert.Equal("tenant-x", ctx.TenantInfo!.Id);
    }

    [Fact]
    public void AddMultiTenantDbContext_EnforcesMultiTenantOnTracking()
    {
        var tenantHolder = new TenantInfoHolder();
        var sp = BuildNonPooledServiceProvider("non-pooled-tracking", tenantHolder);
        var tenant = new TenantInfo { Id = "tenant-t", Identifier = "tenant-t" };
        tenantHolder.Current = tenant;

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
        var tenantHolder = new TenantInfoHolder();
        services.AddMultiTenant<TenantInfo>();
        services.AddSingleton(tenantHolder);
        services.AddScoped<ITenantContext<TenantInfo>>(sp =>
            new TestTenantContext<TenantInfo> { TenantInfo = sp.GetRequiredService<TenantInfoHolder>().Current });
        services.AddScoped<ITenantContext>(sp => sp.GetRequiredService<ITenantContext<TenantInfo>>());
        services.AddSingleton("test-connection-string");
        services.AddMultiTenantDbContext<NonPooledTestDbContext>((sp, options) =>
        {
            capturedValue = sp.GetRequiredService<string>();
            options.UseSqlite("DataSource=:memory:");
        });

        var provider = services.BuildServiceProvider();
        tenantHolder.Current = new TenantInfo { Id = "t", Identifier = "t" };

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

    /// <summary>
    /// Regression test: AddMultiTenantDbContext must set TenantInfo from the scoped
    /// IMultiTenantContextAccessor even when the DbContext does NOT inherit from
    /// MultiTenantDbContext (i.e. no constructor-injected accessor).
    /// Without the explicit assignment in the scoped factory lambda, TenantInfo would
    /// remain null for such custom contexts.
    /// </summary>
    [Fact]
    public void AddMultiTenantDbContext_SetsTenantInfoOnCustomContext_WithoutConstructorInjection()
    {
        var services = new ServiceCollection();
        var tenantHolder = new TenantInfoHolder();
        services.AddMultiTenant<TenantInfo>();
        services.AddSingleton(tenantHolder);
        services.AddScoped<ITenantContext<TenantInfo>>(sp =>
            new TestTenantContext<TenantInfo> { TenantInfo = sp.GetRequiredService<TenantInfoHolder>().Current });
        services.AddScoped<ITenantContext>(sp => sp.GetRequiredService<ITenantContext<TenantInfo>>());
        services.AddMultiTenantDbContext<CustomIMultiTenantDbContext>(
            options => options.UseSqlite("DataSource=:memory:"));

        var sp = services.BuildServiceProvider();
        var tenant = new TenantInfo { Id = "tenant-custom", Identifier = "tenant-custom" };
        tenantHolder.Current = tenant;

        using var scope = sp.CreateScope();
        var ctx = scope.ServiceProvider.GetRequiredService<CustomIMultiTenantDbContext>();

        Assert.Equal("tenant-custom", ctx.TenantInfo!.Id);
    }

    private sealed class TenantInfoHolder
    {
        public TenantInfo? Current { get; set; }
    }

    private sealed class TestTenantContext<TTenantInfo> : ITenantContext<TTenantInfo>
        where TTenantInfo : ITenantInfo
    {
        public TTenantInfo? TenantInfo { get; set; }
        ITenantInfo? ITenantContext.TenantInfo
        {
            get => TenantInfo;
            set => TenantInfo = (TTenantInfo?)value;
        }

        public bool IsResolved => TenantInfo != null;
        public IDictionary<object, object> Items { get; } = new Dictionary<object, object>();
    }
}

public class NonPooledTestDbContext(DbContextOptions<NonPooledTestDbContext> options)
    : EntityFrameworkCore.MultiTenantDbContext(options)
{
    public DbSet<NonPooledBlog>? Blogs { get; set; }
}

[MultiTenant]
public class NonPooledBlog
{
    public int Id { get; set; }
    public string? Title { get; set; }
}

public class PooledTestDbContext(DbContextOptions<PooledTestDbContext> options)
    : EntityFrameworkCore.MultiTenantDbContext(options)
{
    public DbSet<PooledBlog>? Blogs { get; set; }
}

[MultiTenant]
public class PooledBlog
{
    public int Id { get; set; }
    public string? Title { get; set; }
}

/// <summary>
/// A DbContext that implements IMultiTenantDbContext directly, WITHOUT inheriting from
/// MultiTenantDbContext and WITHOUT constructor-injecting IMultiTenantContextAccessor.
/// TenantInfo must be wired up externally by the service registration.
/// </summary>
public class CustomIMultiTenantDbContext(DbContextOptions<CustomIMultiTenantDbContext> options)
    : DbContext(options), IMultiTenantDbContext
{
    public ITenantInfo? TenantInfo { get; set; }
    public TenantMismatchMode TenantMismatchMode { get; } = TenantMismatchMode.Throw;
    public TenantNotSetMode TenantNotSetMode { get; } = TenantNotSetMode.Throw;

    public DbSet<CustomBlog>? Blogs { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
        => modelBuilder.ConfigureMultiTenant();
}

[MultiTenant]
public class CustomBlog
{
    public int Id { get; set; }
    public string? Title { get; set; }
}
