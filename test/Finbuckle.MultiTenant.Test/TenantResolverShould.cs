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

namespace Finbuckle.MultiTenant.Test;

public class TenantResolverShould
{
    [Fact]
    public async Task ResolveTenantWithIntTenantId()
    {
        var services = new ServiceCollection();
        services.AddMultiTenant<TenantInfo<int>, int>()
            .WithStaticStrategy("initech")
            .WithInMemoryStore();
        var sp = services.BuildServiceProvider();

        await sp.GetRequiredService<TenantManager<TenantInfo<int>, int>>()
            .AddAsync(new TenantInfo<int> { Id = 42, Identifier = "initech" });

        var result = await sp.GetRequiredService<ITenantResolver<TenantInfo<int>, int>>().ResolveAsync(new object());

        Assert.NotNull(result);
        Assert.Equal(42, result.Id);
        Assert.Equal("initech", result.Identifier);
    }

    [Fact]
    public void InitializeSortedStrategiesFromDi()
    {
        var services = new ServiceCollection();
        services.AddMultiTenant<TenantInfo,string>().WithDelegateStrategy(_ => Task.FromResult<string?>("strategy1"))
            .WithStaticStrategy("strategy2").WithDelegateStrategy(_ => Task.FromResult<string?>("strategy3"))
            .WithInMemoryStore();
        var sp = services.BuildServiceProvider();

        var resolver = sp.GetRequiredService<ITenantResolver<TenantInfo,string>>();
        var strategies = resolver.Strategies.ToArray();

        Assert.Equal(3, strategies.Length);
        Assert.IsType<DelegateStrategy>(strategies[0]);
        Assert.IsType<DelegateStrategy>(strategies[1]);
        Assert.IsType<StaticStrategy>(strategies[2]); // Note the Static strategy should be last due its priority.
    }

    [Fact]
    public void InitializeStoreAndCachesFromDi()
    {
        var configBuilder = new ConfigurationBuilder();
        configBuilder.AddJsonFile("ConfigurationStoreTestSettings.json");
        var configuration = configBuilder.Build();

        var services = new ServiceCollection();
        services.AddSingleton<IConfiguration>(configuration);

        services.AddMultiTenant<TenantInfo,string>().WithStaticStrategy("strategy")
            .WithMemoryCacheStoreCache()
            .WithConfigurationStore();
        var sp = services.BuildServiceProvider();

        var resolver = sp.GetRequiredService<ITenantResolver<TenantInfo,string>>();
        var caches = resolver.StoreCaches.ToArray();

        Assert.IsType<ConfigurationStore<TenantInfo,string>>(resolver.Store);
        Assert.Single(caches);
        Assert.IsType<MemoryCacheStoreCache<TenantInfo,string>>(caches[0]);
    }

    [Fact]
    public async Task ReturnTenantContext()
    {
        var configBuilder = new ConfigurationBuilder();
        configBuilder.AddJsonFile("ConfigurationStoreTestSettings.json");
        var configuration = configBuilder.Build();

        var services = new ServiceCollection();
        services.AddSingleton<IConfiguration>(configuration);

        services.AddMultiTenant<TenantInfo,string>().WithDelegateStrategy(_ => Task.FromResult<string?>("not-found"))
            .WithStaticStrategy("initech").WithConfigurationStore();
        var sp = services.BuildServiceProvider();

        var resolver = sp.GetRequiredService<ITenantResolver<TenantInfo,string>>();
        var result = await resolver.ResolveAsync(new object());

        Assert.Equal("initech", result!.Identifier);
    }

    [Fact]
    public async Task NonGenericResolverReturnsTenantInfoWithoutAmbientScope()
    {
        var services = new ServiceCollection();
        services.AddMultiTenant<TenantInfo,string>()
            .WithStaticStrategy("initech")
            .WithInMemoryStore();
        var provider = services.BuildServiceProvider();
        await provider.GetRequiredService<TenantManager<TenantInfo,string>>()
            .AddAsync(new TenantInfo { Id = "initech", Identifier = "initech" });

        var result = await provider.GetRequiredService<ITenantResolver<string>>().ResolveAsync(new object());

        Assert.NotNull(result);
        Assert.Equal("initech", result.Identifier);
    }

    [Fact]
    public void ThrowGivenStaticStrategyWithNullIdentifierArgument()
    {
        var services = new ServiceCollection();

        Assert.Throws<ArgumentNullException>(() =>
            services.AddMultiTenant<TenantInfo,string>().WithStaticStrategy(null!).WithInMemoryStore());
    }

    [Fact]
    public void ThrowGivenDelegateStrategyWithNullArgument()
    {
        var services = new ServiceCollection();

        Assert.Throws<ArgumentNullException>(() =>
            services.AddMultiTenant<TenantInfo,string>().WithDelegateStrategy(null!).WithInMemoryStore());
    }

    [Fact]
    public async Task IgnoreSomeIdentifiersFromOptions()
    {
        var configBuilder = new ConfigurationBuilder();
        configBuilder.AddJsonFile("ConfigurationStoreTestSettings.json");
        var configuration = configBuilder.Build();

        var services = new ServiceCollection();
        services.AddSingleton<IConfiguration>(configuration);

        services.AddMultiTenant<TenantInfo,string>(options => options.IgnoredIdentifiers.Add("lol"))
            .WithDelegateStrategy(_ => Task.FromResult<string?>("lol")). // should be ignored
            WithStaticStrategy("initech").WithConfigurationStore();
        var sp = services.BuildServiceProvider();

        var resolver = sp.GetRequiredService<ITenantResolver<TenantInfo,string>>();
        var result = await resolver.ResolveAsync(new object());

        Assert.Equal("initech", result!.Identifier);
    }

    [Fact]
    public async Task CallOnTenantResolveCompletedIfSuccess()
    {
        TenantResolveCompletedContext<TenantInfo,string>? resolvedContext = null;

        var configuration = new ConfigurationBuilder()
            .AddJsonFile("ConfigurationStoreTestSettings.json")
            .Build();

        var services = new ServiceCollection();
        services.AddSingleton<IConfiguration>(configuration);
        services.Configure<MultiTenantOptions<TenantInfo,string>>(options =>
            options.Events.OnTenantResolveCompleted = context => Task.FromResult(resolvedContext = context));
        services.AddMultiTenant<TenantInfo,string>()
            .WithStaticStrategy("initech")
            .WithInMemoryStore();
        var sp = services.BuildServiceProvider();
        await sp.GetRequiredService<TenantManager<TenantInfo,string>>()
            .AddAsync(new TenantInfo { Id = "initech", Identifier = "initech" });
        var resolver = sp.GetRequiredService<ITenantResolver<TenantInfo,string>>();

        await resolver.ResolveAsync(new object());

        Assert.NotNull(resolvedContext);
        Assert.Equal("initech", resolvedContext.TenantInfo!.Identifier);
        Assert.IsType<InMemoryStore<TenantInfo,string>>(resolvedContext.Store);
        Assert.IsType<StaticStrategy>(resolvedContext.Strategy);
    }

    [Fact]
    public async Task CallOnTenantResolveCompletedIfFailure()
    {
        TenantResolveCompletedContext<TenantInfo,string>? resolvedContext = null;

        var configuration = new ConfigurationBuilder()
            .AddJsonFile("ConfigurationStoreTestSettings.json")
            .Build();

        var services = new ServiceCollection();
        services.AddSingleton<IConfiguration>(configuration);
        services.Configure<MultiTenantOptions<TenantInfo,string>>(options =>
            options.Events.OnTenantResolveCompleted = context => Task.FromResult(resolvedContext = context));
        services.AddMultiTenant<TenantInfo,string>()
            .WithDelegateStrategy(_ => Task.FromResult<string?>("not-found"))
            .WithConfigurationStore();
        var sp = services.BuildServiceProvider();
        var resolver = sp.GetRequiredService<ITenantResolver<TenantInfo,string>>();

        await resolver.ResolveAsync(new object());

        Assert.NotNull(resolvedContext);
        Assert.False(resolvedContext.IsResolved);
        Assert.Null(resolvedContext.TenantInfo);
        Assert.Null(resolvedContext.Store);
        Assert.Null(resolvedContext.Strategy);
    }

    [Fact]
    public async Task CompletionEventCanReplaceTenantInfoAndReturnedValue()
    {
        var replacement = new TenantInfo { Id = "replacement", Identifier = "replacement" };
        var services = new ServiceCollection();
        services.Configure<MultiTenantOptions<TenantInfo,string>>(options =>
            options.Events.OnTenantResolveCompleted = context =>
            {
                context.TenantInfo = replacement;
                return Task.CompletedTask;
            });
        services.AddMultiTenant<TenantInfo,string>()
            .WithStaticStrategy("initech")
            .WithInMemoryStore();
        var provider = services.BuildServiceProvider();
        await provider.GetRequiredService<TenantManager<TenantInfo,string>>()
            .AddAsync(new TenantInfo { Id = "initech", Identifier = "initech" });

        var result = await provider.GetRequiredService<ITenantResolver<TenantInfo,string>>().ResolveAsync(new object());

        Assert.Same(replacement, result);
    }

    [Fact]
    public async Task RejectInvalidTenantInfoSetByCompletionEvent()
    {
        // Events may replace the tenant before it is committed, but the committed tenant is always validated.
        var services = new ServiceCollection();
        services.Configure<MultiTenantOptions<TenantInfo,string>>(options =>
            options.Events.OnTenantResolveCompleted = context =>
            {
                context.TenantInfo = new TenantInfo { Id = "", Identifier = "invalid" };
                return Task.CompletedTask;
            });
        services.AddMultiTenant<TenantInfo,string>()
            .WithStaticStrategy("initech")
            .WithInMemoryStore();
        var provider = services.BuildServiceProvider();
        await provider.GetRequiredService<TenantManager<TenantInfo,string>>()
            .AddAsync(new TenantInfo { Id = "initech", Identifier = "initech" });

        await Assert.ThrowsAsync<MultiTenantException>(() =>
            provider.GetRequiredService<ITenantResolver<TenantInfo,string>>().ResolveAsync(new object()));
    }

    [Fact]
    public async Task CompletionEventCanClearTenantInfoAndReturnNull()
    {
        var services = new ServiceCollection();
        services.Configure<MultiTenantOptions<TenantInfo,string>>(options =>
            options.Events.OnTenantResolveCompleted = context =>
            {
                context.TenantInfo = null;
                return Task.CompletedTask;
            });
        services.AddMultiTenant<TenantInfo,string>()
            .WithStaticStrategy("initech")
            .WithInMemoryStore();
        var provider = services.BuildServiceProvider();
        await provider.GetRequiredService<TenantManager<TenantInfo,string>>()
            .AddAsync(new TenantInfo { Id = "initech", Identifier = "initech" });

        var result = await provider.GetRequiredService<ITenantResolver<TenantInfo,string>>().ResolveAsync(new object());

        Assert.Null(result);
    }

    [Fact]
    public async Task CallOnStrategyResolveCompletedPerStrategy()
    {
        var numCalls = 0;

        var configuration = new ConfigurationBuilder()
            .AddJsonFile("ConfigurationStoreTestSettings.json")
            .Build();

        var services = new ServiceCollection();
        services.AddSingleton<IConfiguration>(configuration);
        services.Configure<MultiTenantOptions<TenantInfo,string>>(options =>
            options.Events.OnStrategyResolveCompleted = _ => Task.FromResult(numCalls++));
        services.AddMultiTenant<TenantInfo,string>()
            .WithDelegateStrategy(_ => Task.FromResult<string?>("not-found"))
            .WithStaticStrategy("initech")
            .WithConfigurationStore();
        var sp = services.BuildServiceProvider();
        var resolver = sp.GetRequiredService<ITenantResolver<TenantInfo,string>>();

        await resolver.ResolveAsync(new object());

        Assert.Equal(2, numCalls);
    }

    [Fact]
    public async Task CallOnStoreResolveCompletedForPrimaryStore()
    {
        var numCalls = 0;

        var configuration = new ConfigurationBuilder()
            .AddJsonFile("ConfigurationStoreTestSettings.json")
            .Build();

        var services = new ServiceCollection();
        services.AddSingleton<IConfiguration>(configuration);
        services.Configure<MultiTenantOptions<TenantInfo,string>>(options =>
            options.Events.OnStoreResolveCompleted = _ => Task.FromResult(numCalls++));
        services.AddMultiTenant<TenantInfo,string>()
            .WithStaticStrategy("initech")
            .WithMemoryCacheStoreCache()
            .WithConfigurationStore();
        var sp = services.BuildServiceProvider();
        var resolver = sp.GetRequiredService<ITenantResolver<TenantInfo,string>>();

        await resolver.ResolveAsync(new object());

        Assert.Equal(1, numCalls);
    }

    [Fact]
    public async Task CallOnStoreCacheResolveCompletedForStoreCaches()
    {
        var cacheCalls = 0;
        var storeCalls = 0;

        var configuration = new ConfigurationBuilder()
            .AddJsonFile("ConfigurationStoreTestSettings.json")
            .Build();

        var services = new ServiceCollection();
        services.AddSingleton<IConfiguration>(configuration);
        services.Configure<MultiTenantOptions<TenantInfo, string>>(options =>
        {
            options.Events.OnStoreCacheResolveCompleted = context =>
            {
                Assert.NotNull(context.Cache);
                cacheCalls++;
                return Task.CompletedTask;
            };
            options.Events.OnStoreResolveCompleted = context =>
            {
                Assert.NotNull(context.Store);
                storeCalls++;
                return Task.CompletedTask;
            };
        });
        services.AddMultiTenant<TenantInfo,string>()
            .WithStaticStrategy("initech")
            .WithMemoryCacheStoreCache()
            .WithConfigurationStore();
        var sp = services.BuildServiceProvider();
        var resolver = sp.GetRequiredService<ITenantResolver<TenantInfo,string>>();

        await resolver.ResolveAsync(new object());

        Assert.Equal(1, cacheCalls);
        Assert.Equal(1, storeCalls);
    }

    [Fact]
    public async Task AllowStoreCacheResolveCompletedToNullTenantInfo()
    {
        TenantResolveCompletedContext<TenantInfo, string>? resolvedContext = null;

        var configuration = new ConfigurationBuilder()
            .AddJsonFile("ConfigurationStoreTestSettings.json")
            .Build();

        var services = new ServiceCollection();
        services.AddSingleton<IConfiguration>(configuration);
        services.Configure<MultiTenantOptions<TenantInfo, string>>(options =>
        {
            options.Events.OnStoreCacheResolveCompleted = context =>
            {
                context.TenantInfo = null;
                return Task.CompletedTask;
            };
            options.Events.OnTenantResolveCompleted = context =>
            {
                resolvedContext = context;
                return Task.CompletedTask;
            };
        });
        services.AddMultiTenant<TenantInfo, string>()
            .WithStaticStrategy("initech")
            .WithMemoryCacheStoreCache()
            .WithConfigurationStore();
        var sp = services.BuildServiceProvider();
        var manager = sp.GetRequiredService<TenantManager<TenantInfo, string>>();
        await manager.GetByIdentifierAsync("initech");
        var resolver = sp.GetRequiredService<ITenantResolver<TenantInfo, string>>();

        await resolver.ResolveAsync(new object());

        Assert.NotNull(resolvedContext);
        Assert.NotNull(resolvedContext.Store);
        Assert.Null(resolvedContext.Cache);
    }

    [Fact]
    public async Task IncludeCacheSourceInTenantResolveCompletedWhenCacheResolves()
    {
        TenantResolveCompletedContext<TenantInfo, string>? resolvedContext = null;

        var configuration = new ConfigurationBuilder()
            .AddJsonFile("ConfigurationStoreTestSettings.json")
            .Build();

        var services = new ServiceCollection();
        services.AddSingleton<IConfiguration>(configuration);
        services.Configure<MultiTenantOptions<TenantInfo, string>>(options =>
            options.Events.OnTenantResolveCompleted = context =>
            {
                resolvedContext = context;
                return Task.CompletedTask;
            });
        services.AddMultiTenant<TenantInfo, string>()
            .WithStaticStrategy("initech")
            .WithMemoryCacheStoreCache()
            .WithConfigurationStore();
        var sp = services.BuildServiceProvider();
        var manager = sp.GetRequiredService<TenantManager<TenantInfo, string>>();
        await manager.GetByIdentifierAsync("initech");
        var resolver = sp.GetRequiredService<ITenantResolver<TenantInfo, string>>();

        await resolver.ResolveAsync(new object());

        Assert.NotNull(resolvedContext);
        Assert.NotNull(resolvedContext.Cache);
        Assert.Null(resolvedContext.Store);
    }

    [Fact]
    public async Task IncludeStoreSourceInTenantResolveCompletedWhenPrimaryStoreResolves()
    {
        TenantResolveCompletedContext<TenantInfo, string>? resolvedContext = null;

        var configuration = new ConfigurationBuilder()
            .AddJsonFile("ConfigurationStoreTestSettings.json")
            .Build();

        var services = new ServiceCollection();
        services.AddSingleton<IConfiguration>(configuration);
        services.Configure<MultiTenantOptions<TenantInfo, string>>(options =>
            options.Events.OnTenantResolveCompleted = context =>
            {
                resolvedContext = context;
                return Task.CompletedTask;
            });
        services.AddMultiTenant<TenantInfo, string>()
            .WithStaticStrategy("initech")
            .WithMemoryCacheStoreCache()
            .WithConfigurationStore();
        var sp = services.BuildServiceProvider();
        var resolver = sp.GetRequiredService<ITenantResolver<TenantInfo, string>>();

        await resolver.ResolveAsync(new object());

        Assert.NotNull(resolvedContext);
        Assert.NotNull(resolvedContext.Store);
        Assert.Null(resolvedContext.Cache);
    }
}
