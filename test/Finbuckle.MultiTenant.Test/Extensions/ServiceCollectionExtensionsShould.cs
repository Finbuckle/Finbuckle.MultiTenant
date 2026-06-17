// Copyright Finbuckle LLC, Andrew White, and Contributors.
// Refer to the solution LICENSE file for more information.

using Finbuckle.MultiTenant.Abstractions;
using Finbuckle.MultiTenant.Extensions;
using Finbuckle.MultiTenant.Options;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Xunit;

namespace Finbuckle.MultiTenant.Test.Extensions;

public class ServiceCollectionExtensionsShould
{
    [Fact]
    public void RegisterITenantResolverInDi()
    {
        var services = new ServiceCollection();
        services.AddMultiTenant<TenantInfo>();

        var service = services.SingleOrDefault(s => s.ServiceType == typeof(ITenantResolver));

        Assert.NotNull(service);
        Assert.Equal(ServiceLifetime.Scoped, service.Lifetime);
    }

    [Fact]
    public void RegisterITenantResolverGenericInDi()
    {
        var services = new ServiceCollection();
        services.AddMultiTenant<TenantInfo>();

        var service = services.SingleOrDefault(s => s.ServiceType == typeof(ITenantResolver<TenantInfo>));

        Assert.NotNull(service);
        Assert.Equal(ServiceLifetime.Scoped, service.Lifetime);
    }

    [Fact]
    public void RegisterMultiTenantOptionsInDi()
    {
        var services = new ServiceCollection();
        services.AddMultiTenant<TenantInfo>();

        var service = services.FirstOrDefault(s => s.Lifetime == ServiceLifetime.Singleton &&
                                                   s.ServiceType ==
                                                   typeof(IConfigureOptions<MultiTenantOptions<TenantInfo>>));

        Assert.NotNull(service);
        Assert.Equal(ServiceLifetime.Singleton, service.Lifetime);
    }

    public class TestOptions
    {
        public string? Prop1;
    }

    [Fact]
    public void RegisterNamedOptionsPerTenant()
    {
        var services = new ServiceCollection();
        services.AddMultiTenant<TenantInfo>();
        services.ConfigurePerTenant<TestOptions, TenantInfo>("name1",
            (option, tenant) => option.Prop1 = tenant.Id);
        var sp = services.BuildServiceProvider();

        var configs = sp.GetRequiredService<IEnumerable<IConfigureOptions<TestOptions>>>();
        var config = configs.Where(config =>
            config is ConfigureNamedOptions<TestOptions, ITenantContext<TenantInfo>>).ToList();

        Assert.Single(config);
        Assert.Equal("name1",
            config.Select(c => (ConfigureNamedOptions<TestOptions, ITenantContext<TenantInfo>>)c).Single()
                .Name);
    }

    [Fact]
    public void RegisterUnnamedOptionsPerTenant()
    {
        var services = new ServiceCollection();
        services.AddMultiTenant<TenantInfo>();
        services.ConfigurePerTenant<TestOptions, TenantInfo>((option, tenant) => option.Prop1 = tenant.Id);
        var sp = services.BuildServiceProvider();

        var configs = sp.GetRequiredService<IEnumerable<IConfigureOptions<TestOptions>>>();
        var config = configs.Where(config =>
            config is ConfigureNamedOptions<TestOptions, ITenantContext<TenantInfo>>).ToList();

        Assert.Single(config);
        Assert.Equal(Microsoft.Extensions.Options.Options.DefaultName,
            config.Select(c => (ConfigureNamedOptions<TestOptions, ITenantContext<TenantInfo>>)c).Single()
                .Name);
    }

    [Fact]
    public void RegisterAllOptionsPerTenant()
    {
        var services = new ServiceCollection();
        services.AddMultiTenant<TenantInfo>();
        services.ConfigureAllPerTenant<TestOptions, TenantInfo>((option, tenant) => option.Prop1 = tenant.Id);
        var sp = services.BuildServiceProvider();

        var configs = sp.GetRequiredService<IEnumerable<IConfigureOptions<TestOptions>>>();
        var config = configs.Where(config =>
            config is ConfigureNamedOptions<TestOptions, ITenantContext<TenantInfo>>).ToList();

        Assert.Single(config);
        Assert.Null(config.Select(c => (ConfigureNamedOptions<TestOptions, ITenantContext<TenantInfo>>)c)
            .Single().Name);
    }

    [Fact]
    public void RegisterPerTenantOptionsWithExpectedLifetimes()
    {
        var services = new ServiceCollection();
        services.AddMultiTenant<TenantInfo>();
        services.ConfigurePerTenant<TestOptions, TenantInfo>((option, tenant) => option.Prop1 = tenant.Id);

        var cache = services.LastOrDefault(s => s.ServiceType == typeof(MultiTenantOptionsCache<TestOptions>));
        Assert.NotNull(cache);
        Assert.Equal(ServiceLifetime.Singleton, cache.Lifetime);

        var monitor = services.LastOrDefault(s => s.ServiceType == typeof(IOptionsMonitor<TestOptions>));
        Assert.NotNull(monitor);
        Assert.Equal(ServiceLifetime.Scoped, monitor.Lifetime);

        var snapshot = services.LastOrDefault(s => s.ServiceType == typeof(IOptionsSnapshot<TestOptions>));
        Assert.NotNull(snapshot);
        Assert.Equal(ServiceLifetime.Scoped, snapshot.Lifetime);

        var options = services.LastOrDefault(s => s.ServiceType == typeof(IOptions<TestOptions>));
        Assert.NotNull(options);
        Assert.Equal(ServiceLifetime.Scoped, options.Lifetime);
    }

    [Fact]
    public void ReuseCacheAcrossScopesButScopeOptionsServices()
    {
        var services = new ServiceCollection();
        services.AddMultiTenant<TenantInfo>();
        services.ConfigurePerTenant<TestOptions, TenantInfo>((option, tenant) => option.Prop1 = tenant.Id);
        var provider = services.BuildServiceProvider();

        using var scope1 = provider.CreateScope();
        using var scope2 = provider.CreateScope();

        var cache1 = scope1.ServiceProvider.GetRequiredService<MultiTenantOptionsCache<TestOptions>>();
        var cache2 = scope2.ServiceProvider.GetRequiredService<MultiTenantOptionsCache<TestOptions>>();
        Assert.Same(cache1, cache2);

        var options1 = scope1.ServiceProvider.GetRequiredService<IOptions<TestOptions>>();
        var options1Again = scope1.ServiceProvider.GetRequiredService<IOptions<TestOptions>>();
        var options2 = scope2.ServiceProvider.GetRequiredService<IOptions<TestOptions>>();
        Assert.Same(options1, options1Again);
        Assert.NotSame(options1, options2); // Service is scoped.

        // IOptions service is scoped, but underlying value is shared via keyed singleton manager cache.
        var optionsValue1 = options1.Value;
        var optionsValue2 = options2.Value;
        Assert.Same(optionsValue1, optionsValue2); // Value cache for IOptions is singleton via keyed cache.

        var snapshot1 = scope1.ServiceProvider.GetRequiredService<IOptionsSnapshot<TestOptions>>();
        var snapshot1Again = scope1.ServiceProvider.GetRequiredService<IOptionsSnapshot<TestOptions>>();
        var snapshot2 = scope2.ServiceProvider.GetRequiredService<IOptionsSnapshot<TestOptions>>();
        Assert.Same(snapshot1, snapshot1Again);
        Assert.NotSame(snapshot1, snapshot2);

        // Snapshot keeps a scope-local cache and should not share value instances across scopes.
        var snapshotValue1 = snapshot1.Value;
        var snapshotValue2 = snapshot2.Value;
        Assert.NotSame(snapshotValue1, snapshotValue2);

        var monitor1 = scope1.ServiceProvider.GetRequiredService<IOptionsMonitor<TestOptions>>();
        var monitor1Again = scope1.ServiceProvider.GetRequiredService<IOptionsMonitor<TestOptions>>();
        var monitor2 = scope2.ServiceProvider.GetRequiredService<IOptionsMonitor<TestOptions>>();
        Assert.Same(monitor1, monitor1Again);
        Assert.NotSame(monitor1, monitor2);
    }

    [Fact]
    public void ShareOptionsAcrossScopesForSameTenantWhenUsingSingletonIOptionsCache()
    {
        var services = new ServiceCollection();
        var tenantHolder = new TenantInfoHolder();
        services.AddMultiTenant<TenantInfo>();
        services.AddSingleton(tenantHolder);
        services.AddScoped<ITenantContext<TenantInfo>>(sp => new TenantContext<TenantInfo>(sp.GetRequiredService<TenantInfoHolder>().Current));
        services.ConfigurePerTenant<TestOptions, TenantInfo>((option, tenant) => option.Prop1 = tenant.Id);
        var provider = services.BuildServiceProvider();

        tenantHolder.Current = new TenantInfo { Id = "tenant-1", Identifier = "tenant-1" };

        // Same tenant in different scopes should receive the same IOptions value instance.
        using var scope1 = provider.CreateScope();
        var options1 = scope1.ServiceProvider.GetRequiredService<IOptions<TestOptions>>().Value;

        using var scope2 = provider.CreateScope();
        var options2 = scope2.ServiceProvider.GetRequiredService<IOptions<TestOptions>>().Value;

        Assert.Same(options1, options2);
        Assert.Equal(tenantHolder.Current!.Id, options1.Prop1);
        Assert.Equal(tenantHolder.Current!.Id, options2.Prop1);
    }

    private sealed class TenantInfoHolder
    {
        public TenantInfo? Current { get; set; }
    }

    [Fact]
    public void DecorateService_WithImplementationType_WrapsService()
    {
        var services = new ServiceCollection();
        services.AddSingleton<ITestService, TestService>();
        bool result = services.DecorateService<ITestService, DecoratedTestService>();
        Assert.True(result);
        var provider = services.BuildServiceProvider();
        var service = provider.GetRequiredService<ITestService>();
        Assert.IsType<DecoratedTestService>(service);
        Assert.IsType<TestService>(((DecoratedTestService)service).Inner);
    }

    [Fact]
    public void DecorateService_WithImplementationInstance_WrapsService()
    {
        var instance = new TestService();
        var services = new ServiceCollection();
        services.AddSingleton<ITestService>(instance);
        bool result = services.DecorateService<ITestService, DecoratedTestService>();
        Assert.True(result);
        var provider = services.BuildServiceProvider();
        var service = provider.GetRequiredService<ITestService>();
        Assert.IsType<DecoratedTestService>(service);
        Assert.Same(instance, ((DecoratedTestService)service).Inner);
    }

    [Fact]
    public void DecorateService_WithImplementationFactory_WrapsService()
    {
        var services = new ServiceCollection();
        services.AddSingleton<ITestService>(_ => new TestService());
        bool result = services.DecorateService<ITestService, DecoratedTestService>();
        Assert.True(result);
        var provider = services.BuildServiceProvider();
        var service = provider.GetRequiredService<ITestService>();
        Assert.IsType<DecoratedTestService>(service);
        Assert.IsType<TestService>(((DecoratedTestService)service).Inner);
    }

    [Fact]
    public void DecorateService_ThrowsIfNoServiceFound()
    {
        var services = new ServiceCollection();
        Assert.Throws<ArgumentException>(() => services.DecorateService<ITestService, DecoratedTestService>());
    }

    [Fact]
    public void DecorateService_WrapsAllServicesOfSameType()
    {
        var services = new ServiceCollection();
        var instance1 = new TestService();
        var instance2 = new TestService();
        services.AddSingleton<ITestService>(instance1);
        services.AddSingleton<ITestService>(instance2);
        bool result = services.DecorateService<ITestService, DecoratedTestService>();
        Assert.True(result);
        var provider = services.BuildServiceProvider();
        var all = provider.GetServices<ITestService>().ToList();
        Assert.Equal(2, all.Count);
        Assert.All(all, s => Assert.IsType<DecoratedTestService>(s));
        var inners = all.Cast<DecoratedTestService>().Select(d => d.Inner).ToList();
        Assert.Contains(instance1, inners);
        Assert.Contains(instance2, inners);
    }

    public interface ITestService
    {
    }

    public class TestService : ITestService
    {
    }

    public class DecoratedTestService : ITestService
    {
        public ITestService Inner { get; }
        public DecoratedTestService(ITestService inner) => Inner = inner;
    }
}