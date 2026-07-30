// Copyright Finbuckle LLC, Andrew White, and Contributors.
// Refer to the solution LICENSE file for more information.

using Finbuckle.MultiTenant.Abstractions;
using Finbuckle.MultiTenant.Extensions;
using Finbuckle.MultiTenant.StoreCaches;
using Finbuckle.MultiTenant.Stores;
using Finbuckle.MultiTenant.Strategies;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Finbuckle.MultiTenant.Test.Extensions;

public class MultiTenantBuilderExtensionsShould
{
    [Fact]
    public void AddDistributedCacheStoreCacheDefault()
    {
        var services = new ServiceCollection();
        services.AddDistributedMemoryCache();
        var builder = new MultiTenantBuilder<TenantInfo, string>(services);
        builder.WithDistributedCacheStoreCache();
        var sp = services.BuildServiceProvider();
        var cache = sp.GetRequiredService<IMultiTenantStoreCache<TenantInfo, string>>();
        Assert.IsType<DistributedCacheStoreCache<TenantInfo, string>>(cache);
    }

    [Fact]
    public void AddDistributedCacheStoreCacheWithOptions()
    {
        var services = new ServiceCollection();
        services.AddDistributedMemoryCache();
        var builder = new MultiTenantBuilder<TenantInfo, string>(services);
        builder.WithDistributedCacheStoreCache(options => options.SlidingExpiration = TimeSpan.FromMinutes(5));
        var sp = services.BuildServiceProvider();
        var cache = sp.GetRequiredService<IMultiTenantStoreCache<TenantInfo, string>>();
        Assert.IsType<DistributedCacheStoreCache<TenantInfo, string>>(cache);
    }

    [Fact]
    public void AddMemoryCacheStoreCacheDefault()
    {
        var services = new ServiceCollection();
        var builder = new MultiTenantBuilder<TenantInfo, string>(services);
        builder.WithMemoryCacheStoreCache();
        var sp = services.BuildServiceProvider();
        var cache = sp.GetRequiredService<IMultiTenantStoreCache<TenantInfo, string>>();
        Assert.IsType<MemoryCacheStoreCache<TenantInfo, string>>(cache);
    }

    [Fact]
    public void AddMemoryCacheStoreCacheWithOptions()
    {
        var services = new ServiceCollection();
        var builder = new MultiTenantBuilder<TenantInfo, string>(services);
        builder.WithMemoryCacheStoreCache(options => options.SlidingExpiration = TimeSpan.FromMinutes(5));
        var sp = services.BuildServiceProvider();
        var cache = sp.GetRequiredService<IMultiTenantStoreCache<TenantInfo, string>>();
        Assert.IsType<MemoryCacheStoreCache<TenantInfo, string>>(cache);
    }

    [Fact]
    public void AddHttpRemoteStoreAndHttpRemoteStoreClient()
    {
        var services = new ServiceCollection();
        var builder = new MultiTenantBuilder<TenantInfo, string>(services);
        builder.WithHttpRemoteStore("https://example.com");
        var sp = services.BuildServiceProvider();

        sp.GetRequiredService<HttpRemoteStoreClient<TenantInfo, string>>();
        var store = sp.GetRequiredService<IMultiTenantStore<TenantInfo, string>>();
        Assert.IsType<HttpRemoteStore<TenantInfo, string>>(store);
    }

    [Fact]
    public void AddHttpRemoteStoreWithHttpClientBuilders()
    {
        var services = new ServiceCollection();
        var builder = new MultiTenantBuilder<TenantInfo, string>(services);
        var flag = false;
        builder.WithHttpRemoteStore("https://example.com", _ => flag = true);
        var sp = services.BuildServiceProvider();

        sp.GetRequiredService<HttpRemoteStoreClient<TenantInfo, string>>();
        var store = sp.GetRequiredService<IMultiTenantStore<TenantInfo, string>>();
        Assert.IsType<HttpRemoteStore<TenantInfo, string>>(store);
        Assert.True(flag);
    }

    [Fact]
    public async Task AddConfigurationStoreWithDefaults()
    {
        var configBuilder = new ConfigurationBuilder();
        configBuilder.AddJsonFile("ConfigurationStoreTestSettings.json");
        var configuration = configBuilder.Build();

        var services = new ServiceCollection();
        var builder = new MultiTenantBuilder<TenantInfo, string>(services);
        builder.WithConfigurationStore();
        services.AddSingleton<IConfiguration>(configuration);
        var sp = services.BuildServiceProvider();

        var store = sp.GetRequiredService<IMultiTenantStore<TenantInfo, string>>();
        Assert.IsType<ConfigurationStore<TenantInfo, string>>(store);

        var tc = await store.GetByIdentifierAsync("initech");
        Assert.Equal("initech-id", tc!.Id);
        Assert.Equal("initech", tc.Identifier);

        tc = await store.GetByIdentifierAsync("lol");
        Assert.Equal("lol-id", tc!.Id);
        Assert.Equal("lol", tc.Identifier);
    }

    [Fact]
    public async Task AddConfigurationStoreWithSectionName()
    {
        var configBuilder = new ConfigurationBuilder();
        configBuilder.AddJsonFile("ConfigurationStoreTestSettings.json");
        IConfiguration configuration = configBuilder.Build();

        var services = new ServiceCollection();
        var builder = new MultiTenantBuilder<TenantInfo, string>(services);

        // Non-default section name.
        configuration = configuration.GetSection("Finbuckle");
        builder.WithConfigurationStore(configuration, "MultiTenant:Stores:ConfigurationStore");
        var sp = services.BuildServiceProvider();

        var store = sp.GetRequiredService<IMultiTenantStore<TenantInfo, string>>();
        Assert.IsType<ConfigurationStore<TenantInfo, string>>(store);

        var tc = await store.GetByIdentifierAsync("initech");
        Assert.Equal("initech-id", tc!.Id);
        Assert.Equal("initech", tc.Identifier);

        tc = await store.GetByIdentifierAsync("lol");
        Assert.Equal("lol-id", tc!.Id);
        Assert.Equal("lol", tc.Identifier);
    }

    [Fact]
    public async Task AddCaseInsensitiveInMemoryStore()
    {
        var services = new ServiceCollection();
        var builder = new MultiTenantBuilder<TenantInfo, string>(services);
        builder.WithInMemoryStore();
        var sp = services.BuildServiceProvider();

        var store = sp.GetRequiredService<IMultiTenantStore<TenantInfo, string>>();
        Assert.IsType<InMemoryStore<TenantInfo, string>>(store);
        await store.AddAsync(new TenantInfo { Id = "lol", Identifier = "lol" });

        var tc = await store.GetByIdentifierAsync("LOL");
        Assert.Equal("lol", tc!.Id);
        Assert.Equal("lol", tc.Identifier);
    }

    [Fact]
    public async Task AddEchoStore()
    {
        var services = new ServiceCollection();
        var builder = new MultiTenantBuilder<TenantInfo, string>(services);
        builder.WithEchoStore(identifier => identifier);
        var sp = services.BuildServiceProvider();

        var store = sp.GetRequiredService<IMultiTenantStore<TenantInfo, string>>();
        Assert.IsType<EchoStore<TenantInfo, string>>(store);

        var tc = await store.GetByIdentifierAsync("initech");
        Assert.Equal("initech", tc!.Id);
        Assert.Equal("initech", tc.Identifier);

        tc = await store.GetByIdentifierAsync("lol");
        Assert.Equal("lol", tc!.Id);
        Assert.Equal("lol", tc.Identifier);
    }

    [Fact]
    public void AddDelegateStrategy()
    {
        var services = new ServiceCollection();
        var builder = new MultiTenantBuilder<TenantInfo, string>(services);
        builder.WithDelegateStrategy(_ => Task.FromResult<string?>("Hi"));
        var sp = services.BuildServiceProvider();

        var strategy = sp.GetRequiredService<IMultiTenantStrategy>();
        Assert.IsType<DelegateStrategy>(strategy);
    }

    [Fact]
    public void AddTypedDelegateStrategy()
    {
        var services = new ServiceCollection();
        var builder = new MultiTenantBuilder<TenantInfo, string>(services);
        builder.WithDelegateStrategy<int, TenantInfo,string>(context => Task.FromResult(context.ToString())!);
        var sp = services.BuildServiceProvider();

        var strategy = sp.GetRequiredService<IMultiTenantStrategy>();
        Assert.IsType<DelegateStrategy>(strategy);
    }

    [Fact]
    public async Task ReturnNullForWrongTypeSendToTypedDelegateStrategy()
    {
        var services = new ServiceCollection();
        var builder = new MultiTenantBuilder<TenantInfo, string>(services);
        builder.WithDelegateStrategy<int, TenantInfo,string>(_ => Task.FromResult("Shouldn't ever get here")!);
        var sp = services.BuildServiceProvider();

        var strategy = sp.GetRequiredService<IMultiTenantStrategy>();
        var identifier = await strategy.GetIdentifierAsync(new object());
        Assert.Null(identifier);
    }

    private class BaseCtx
    {
    }

    private class DerivedCtx : BaseCtx
    {
    }

    [Fact]
    public async Task InvokeTypedDelegateWhenRuntimeContextIsDerivedType()
    {
        var services = new ServiceCollection();
        var builder = new MultiTenantBuilder<TenantInfo, string>(services);
        builder.WithDelegateStrategy<BaseCtx, TenantInfo,string>(ctx => Task.FromResult<string?>($"ok-{ctx.GetType().Name}"));
        var sp = services.BuildServiceProvider();

        var strategy = sp.GetRequiredService<IMultiTenantStrategy>();
        var identifier = await strategy.GetIdentifierAsync(new DerivedCtx());

        Assert.Equal("ok-DerivedCtx", identifier);
    }

    [Fact]
    public async Task ReturnNullWhenRuntimeContextIsBaseOfExpectedDerivedType()
    {
        var services = new ServiceCollection();
        var builder = new MultiTenantBuilder<TenantInfo, string>(services);
        builder.WithDelegateStrategy<DerivedCtx, TenantInfo, string>(ctx =>
            Task.FromResult<string?>($"ok-{ctx.GetType().Name}"));
        var sp = services.BuildServiceProvider();

        var strategy = sp.GetRequiredService<IMultiTenantStrategy>();
        var identifier = await strategy.GetIdentifierAsync(new BaseCtx());

        Assert.Null(identifier);
    }

    [Fact]
    public void AddStaticStrategy()
    {
        var services = new ServiceCollection();
        var builder = new MultiTenantBuilder<TenantInfo, string>(services);
        builder.WithStaticStrategy("initech");
        var sp = services.BuildServiceProvider();

        var strategy = sp.GetRequiredService<IMultiTenantStrategy>();
        Assert.IsType<StaticStrategy>(strategy);
    }

    [Fact]
    public void ThrowIfNullParamAddingStaticStrategy()
    {
        var services = new ServiceCollection();
        var builder = new MultiTenantBuilder<TenantInfo, string>(services);
        Assert.Throws<ArgumentNullException>(()
            => builder.WithStaticStrategy(null!));
    }
}
