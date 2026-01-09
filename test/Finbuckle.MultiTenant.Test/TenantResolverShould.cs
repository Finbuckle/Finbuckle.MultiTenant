// Copyright Finbuckle LLC, Andrew White, and Contributors.
// Refer to the solution LICENSE file for more information.

using Finbuckle.MultiTenant.Abstractions;
using Finbuckle.MultiTenant.Extensions;
using Finbuckle.MultiTenant.Stores;
using Finbuckle.MultiTenant.Strategies;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Finbuckle.MultiTenant.Test;

public class TenantResolverShould
{
    [Fact]
    public void InitializeSortedStrategiesFromDi()
    {
        var services = new ServiceCollection();
        services.AddMultiTenant<TenantInfo>().WithDelegateStrategy(_ => Task.FromResult<string?>("strategy1"))
            .WithStaticStrategy("strategy2").WithDelegateStrategy(_ => Task.FromResult<string?>("strategy3"))
            .WithInMemoryStore();
        var sp = services.BuildServiceProvider();

        var resolver = sp.GetRequiredService<ITenantResolver<TenantInfo>>();
        var strategies = resolver.Strategies.ToArray();

        Assert.Equal(3, strategies.Length);
        Assert.IsType<DelegateStrategy>(strategies[0]);
        Assert.IsType<DelegateStrategy>(strategies[1]);
        Assert.IsType<StaticStrategy>(strategies[2]); // Note the Static strategy should be last due its priority.
    }

    [Fact]
    public void InitializeStoresFromDi()
    {
        var configBuilder = new ConfigurationBuilder();
        configBuilder.AddJsonFile("ConfigurationStoreTestSettings.json");
        var configuration = configBuilder.Build();

        var services = new ServiceCollection();
        services.AddSingleton<IConfiguration>(configuration);

        services.AddMultiTenant<TenantInfo>().WithStaticStrategy("strategy").WithInMemoryStore()
            .WithConfigurationStore();
        var sp = services.BuildServiceProvider();

        var resolver = sp.GetRequiredService<ITenantResolver<TenantInfo>>();
        var stores = resolver.Stores.ToArray();

        Assert.Equal(2, stores.Length);
        Assert.IsType<InMemoryStore<TenantInfo>>(stores[0]);
        Assert.IsType<ConfigurationStore<TenantInfo>>(stores[1]);
    }

    [Fact]
    public async Task ReturnMultiTenantContext()
    {
        var configBuilder = new ConfigurationBuilder();
        configBuilder.AddJsonFile("ConfigurationStoreTestSettings.json");
        var configuration = configBuilder.Build();

        var services = new ServiceCollection();
        services.AddSingleton<IConfiguration>(configuration);

        services.AddMultiTenant<TenantInfo>().WithDelegateStrategy(_ => Task.FromResult<string?>("not-found"))
            .WithStaticStrategy("initech").WithInMemoryStore().WithConfigurationStore();
        var sp = services.BuildServiceProvider();
        await sp.GetServices<IMultiTenantStore<TenantInfo>>()
            .Single(i => i.GetType() == typeof(InMemoryStore<TenantInfo>))
            .AddAsync(new TenantInfo { Id = "null", Identifier = "null" });

        var resolver = sp.GetRequiredService<ITenantResolver<TenantInfo>>();
        var result = await resolver.ResolveAsync(new object());

        Assert.Equal("initech", result.TenantInfo!.Identifier);
        Assert.IsType<StaticStrategy>(result.StrategyInfo!.Strategy);
        Assert.IsType<ConfigurationStore<TenantInfo>>(result.StoreInfo!.Store);
    }

    [Fact]
    public void ThrowGivenStaticStrategyWithNullIdentifierArgument()
    {
        var services = new ServiceCollection();

        Assert.Throws<ArgumentNullException>(() =>
            services.AddMultiTenant<TenantInfo>().WithStaticStrategy(null!).WithInMemoryStore()
                .WithConfigurationStore());
    }

    [Fact]
    public void ThrowGivenDelegateStrategyWithNullArgument()
    {
        var services = new ServiceCollection();

        Assert.Throws<ArgumentNullException>(() =>
            services.AddMultiTenant<TenantInfo>().WithDelegateStrategy(null!).WithInMemoryStore()
                .WithConfigurationStore());
    }

    [Fact]
    public async Task IgnoreSomeIdentifiersFromOptions()
    {
        var configBuilder = new ConfigurationBuilder();
        configBuilder.AddJsonFile("ConfigurationStoreTestSettings.json");
        var configuration = configBuilder.Build();

        var services = new ServiceCollection();
        services.AddSingleton<IConfiguration>(configuration);

        services.AddMultiTenant<TenantInfo>(options => options.IgnoredIdentifiers.Add("lol"))
            .WithDelegateStrategy(_ => Task.FromResult<string?>("lol")). // should be ignored
            WithStaticStrategy("initech").WithInMemoryStore().WithConfigurationStore();
        var sp = services.BuildServiceProvider();
        await sp.GetServices<IMultiTenantStore<TenantInfo>>()
            .Single(i => i.GetType() == typeof(InMemoryStore<TenantInfo>))
            .AddAsync(new TenantInfo { Id = "null", Identifier = "null" });

        var resolver = sp.GetRequiredService<ITenantResolver<TenantInfo>>();
        var result = await resolver.ResolveAsync(new object());

        Assert.Equal("initech", result.TenantInfo!.Identifier);
        Assert.IsType<StaticStrategy>(result.StrategyInfo!.Strategy);
        Assert.IsType<ConfigurationStore<TenantInfo>>(result.StoreInfo!.Store);
    }

    [Fact]
    public async Task CallOnTenantResolveCompletedIfSuccess()
    {
        TenantResolveCompletedContext<TenantInfo>? resolvedContext = null;

        var configuration = new ConfigurationBuilder()
            .AddJsonFile("ConfigurationStoreTestSettings.json")
            .Build();

        var services = new ServiceCollection();
        services.AddSingleton<IConfiguration>(configuration);
        services.Configure<MultiTenantOptions<TenantInfo>>(options =>
            options.Events.OnTenantResolveCompleted = context => Task.FromResult(resolvedContext = context));
        services.AddMultiTenant<TenantInfo>()
            .WithStaticStrategy("initech")
            .WithConfigurationStore();
        var sp = services.BuildServiceProvider();
        var resolver = sp.GetRequiredService<ITenantResolver<TenantInfo>>();

        await resolver.ResolveAsync(new object());

        Assert.NotNull(resolvedContext);
        Assert.Equal("initech", resolvedContext.MultiTenantContext.TenantInfo!.Identifier);
        Assert.Equal(typeof(StaticStrategy), resolvedContext.MultiTenantContext.StrategyInfo!.StrategyType);
        Assert.Equal(typeof(ConfigurationStore<TenantInfo>), resolvedContext.MultiTenantContext.StoreInfo!.StoreType);
    }

    [Fact]
    public async Task CallOnTenantResolveCompletedIfFailure()
    {
        TenantResolveCompletedContext<TenantInfo>? resolvedContext = null;

        var configuration = new ConfigurationBuilder()
            .AddJsonFile("ConfigurationStoreTestSettings.json")
            .Build();

        var services = new ServiceCollection();
        services.AddSingleton<IConfiguration>(configuration);
        services.Configure<MultiTenantOptions<TenantInfo>>(options =>
            options.Events.OnTenantResolveCompleted = context => Task.FromResult(resolvedContext = context));
        services.AddMultiTenant<TenantInfo>()
            .WithDelegateStrategy(_ => Task.FromResult<string?>("not-found"))
            .WithConfigurationStore();
        var sp = services.BuildServiceProvider();
        var resolver = sp.GetRequiredService<ITenantResolver<TenantInfo>>();

        await resolver.ResolveAsync(new object());

        Assert.NotNull(resolvedContext);
        Assert.False(resolvedContext.IsResolved);
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
        services.Configure<MultiTenantOptions<TenantInfo>>(options =>
            options.Events.OnStrategyResolveCompleted = _ => Task.FromResult(numCalls++));
        services.AddMultiTenant<TenantInfo>()
            .WithDelegateStrategy(_ => Task.FromResult<string?>("not-found"))
            .WithStaticStrategy("initech")
            .WithConfigurationStore();
        var sp = services.BuildServiceProvider();
        var resolver = sp.GetRequiredService<ITenantResolver<TenantInfo>>();

        await resolver.ResolveAsync(new object());

        Assert.Equal(2, numCalls);
    }

    [Fact]
    public async Task CallOnStoreResolveCompletedPerStore()
    {
        var numCalls = 0;

        var configuration = new ConfigurationBuilder()
            .AddJsonFile("ConfigurationStoreTestSettings.json")
            .Build();

        var services = new ServiceCollection();
        services.AddSingleton<IConfiguration>(configuration);
        services.Configure<MultiTenantOptions<TenantInfo>>(options =>
            options.Events.OnStoreResolveCompleted = _ => Task.FromResult(numCalls++));
        services.AddMultiTenant<TenantInfo>()
            .WithStaticStrategy("initech")
            .WithInMemoryStore()
            .WithConfigurationStore();
        var sp = services.BuildServiceProvider();
        var resolver = sp.GetRequiredService<ITenantResolver<TenantInfo>>();

        await resolver.ResolveAsync(new object());

        Assert.Equal(2, numCalls);
    }
}