// Copyright 2018-2020 Finbuckle LLC, Andrew White, and Contributors
// 
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
// 
//     http://www.apache.org/licenses/LICENSE-2.0
// 
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

using System.Linq;
using System.Threading.Tasks;
using Finbuckle.MultiTenant;
using Finbuckle.MultiTenant.Stores;
using Finbuckle.MultiTenant.Strategies;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

public class TenantResolverShould
{
    [Fact]
    void InitializeSortedStrategiesFromDI()
    {
        var services = new ServiceCollection();
        services.
            AddMultiTenant<TenantInfo>().
            WithDelegateStrategy(c => Task.FromResult("strat1")).
            WithStaticStrategy("strat2").
            WithDelegateStrategy(c => Task.FromResult("strat3")).
            WithInMemoryStore();
        var sp = services.BuildServiceProvider();

        var resolver = sp.GetService<ITenantResolver<TenantInfo>>();
        var strategies = resolver.Strategies.ToArray();

        Assert.Equal(3, strategies.Length);
        Assert.IsType<DelegateStrategy>(strategies[0]);
        Assert.IsType<DelegateStrategy>(strategies[1]);
        Assert.IsType<StaticStrategy>(strategies[2]); // Note the Static stragy should be last due its priority.
    }

    [Fact]
    void InitializeStoresFromDI()
    {
        var configBuilder = new ConfigurationBuilder();
        configBuilder.AddJsonFile("ConfigurationStoreTestSettings.json");
        var configuration = configBuilder.Build();

        var services = new ServiceCollection();
        services.AddSingleton<IConfiguration>(configuration);

        services.
            AddMultiTenant<TenantInfo>().
            WithStaticStrategy("strat").
            WithInMemoryStore().
            WithConfigurationStore();
        var sp = services.BuildServiceProvider();

        var resolver = sp.GetService<ITenantResolver<TenantInfo>>();
        var stores = resolver.Stores.ToArray();

        Assert.Equal(2, stores.Length);
        Assert.IsType<InMemoryStore<TenantInfo>>(stores[0]);
        Assert.IsType<ConfigurationStore<TenantInfo>>(stores[1]);
    }

    [Fact]
    void ReturnMultiTenantContext()
    {
        var configBuilder = new ConfigurationBuilder();
        configBuilder.AddJsonFile("ConfigurationStoreTestSettings.json");
        var configuration = configBuilder.Build();

        var services = new ServiceCollection();
        services.AddSingleton<IConfiguration>(configuration);

        services.
            AddMultiTenant<TenantInfo>().
            WithDelegateStrategy(c => Task.FromResult("not-found")).
            WithStaticStrategy("initech").
            WithInMemoryStore().
            WithConfigurationStore();
        var sp = services.BuildServiceProvider();
        sp.GetServices<IMultiTenantStore<TenantInfo>>().
            Where(i => i.GetType() == typeof(InMemoryStore<TenantInfo>)).
            Single().TryAddAsync(new TenantInfo { Id = "null", Identifier = "null" }).Wait();

        var resolver = sp.GetService<ITenantResolver<TenantInfo>>();
        var result = resolver.ResolveAsync(new object()).Result;
        
        Assert.Equal("initech", result.TenantInfo.Identifier);
        Assert.IsType<StaticStrategy>(result.StrategyInfo.Strategy);
        Assert.IsType<ConfigurationStore<TenantInfo>>(result.StoreInfo.Store);
    }

    [Fact]
    void IgnoreSomeIdentifiersFromOptions()
    {
        var configBuilder = new ConfigurationBuilder();
        configBuilder.AddJsonFile("ConfigurationStoreTestSettings.json");
        var configuration = configBuilder.Build();

        var services = new ServiceCollection();
        services.AddSingleton<IConfiguration>(configuration);

        services.
            AddMultiTenant<TenantInfo>(options => options.IgnoredIdentifiers.Add("lol")).
            WithDelegateStrategy(c => Task.FromResult("lol")). // should be ignored
            WithStaticStrategy("initech").
            WithInMemoryStore().
            WithConfigurationStore();
        var sp = services.BuildServiceProvider();
        sp.GetServices<IMultiTenantStore<TenantInfo>>().
            Where(i => i.GetType() == typeof(InMemoryStore<TenantInfo>)).
            Single().TryAddAsync(new TenantInfo { Id = "null", Identifier = "null" }).Wait();

        var resolver = sp.GetService<ITenantResolver<TenantInfo>>();
        var result = resolver.ResolveAsync(new object()).Result;
        
        Assert.Equal("initech", result.TenantInfo.Identifier);
        Assert.IsType<StaticStrategy>(result.StrategyInfo.Strategy);
        Assert.IsType<ConfigurationStore<TenantInfo>>(result.StoreInfo.Store);
    }

    [Fact]
    void ReturnNullIfNoStrategySuccess()
    {
        var configBuilder = new ConfigurationBuilder();
        configBuilder.AddJsonFile("ConfigurationStoreTestSettings.json");
        var configuration = configBuilder.Build();

        var services = new ServiceCollection();
        services.AddSingleton<IConfiguration>(configuration);

        services.
            AddMultiTenant<TenantInfo>().
            WithDelegateStrategy(c => Task.FromResult<string>(null)).
            WithInMemoryStore().
            WithConfigurationStore();
        var sp = services.BuildServiceProvider();
        sp.GetServices<IMultiTenantStore<TenantInfo>>().
            Where(i => i.GetType() == typeof(InMemoryStore<TenantInfo>)).
            Single().TryAddAsync(new TenantInfo { Id = "null", Identifier = "null" }).Wait();

        var resolver = sp.GetService<ITenantResolver<TenantInfo>>();
        var result = resolver.ResolveAsync(new object()).Result;

        Assert.Null(result);
    }

    [Fact]
    void ReturnNullIfNoStoreSuccess()
    {
        var configBuilder = new ConfigurationBuilder();
        configBuilder.AddJsonFile("ConfigurationStoreTestSettings.json");
        var configuration = configBuilder.Build();

        var services = new ServiceCollection();
        services.AddSingleton<IConfiguration>(configuration);

        services.AddLogging().
            AddMultiTenant<TenantInfo>().
            WithDelegateStrategy(c => Task.FromResult("not-found")).
            WithStaticStrategy("also-not-found").
            WithInMemoryStore().
            WithConfigurationStore();
        var sp = services.BuildServiceProvider();
        sp.GetServices<IMultiTenantStore<TenantInfo>>().
            Where(i => i.GetType() == typeof(InMemoryStore<TenantInfo>)).
            Single().TryAddAsync(new TenantInfo { Id = "null", Identifier = "null" }).Wait();

        var resolver = sp.GetService<ITenantResolver<TenantInfo>>();
        var result = resolver.ResolveAsync(new object()).Result;

        Assert.Null(result);
    }
}