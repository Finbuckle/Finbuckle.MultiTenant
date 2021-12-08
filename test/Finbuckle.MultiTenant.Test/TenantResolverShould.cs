// Copyright Finbuckle LLC, Andrew White, and Contributors.
// Refer to the solution LICENSE file for more inforation.

using System;
using System.Linq;
using System.Threading.Tasks;
using Finbuckle.MultiTenant.Stores;
using Finbuckle.MultiTenant.Strategies;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Finbuckle.MultiTenant.Test
{
    public class TenantResolverShould
    {
        [Fact]
        void InitializeSortedStrategiesFromDi()
        {
            var services = new ServiceCollection();
            services.
                AddMultiTenant<TenantInfo>().
                WithDelegateStrategy(_ => Task.FromResult("strategy1")).
                WithStaticStrategy("strategy2").
                WithDelegateStrategy(_ => Task.FromResult("strategy3")).
                WithInMemoryStore();
            var sp = services.BuildServiceProvider();

            var resolver = sp.GetRequiredService<ITenantResolver<TenantInfo>>();
            var strategies = resolver.Strategies.ToArray();

            Assert.Equal(3, strategies.Length);
            Assert.IsType<DelegateStrategy>(strategies[0]);
            Assert.IsType<DelegateStrategy>(strategies[1]);
            Assert.IsType<StaticStrategy>(strategies[2]); // Note the Static strategy should be last due its priority.
        }

        [Fact]
        void InitializeStoresFromDi()
        {
            var configBuilder = new ConfigurationBuilder();
            configBuilder.AddJsonFile("ConfigurationStoreTestSettings.json");
            var configuration = configBuilder.Build();

            var services = new ServiceCollection();
            services.AddSingleton<IConfiguration>(configuration);

            services.
                AddMultiTenant<TenantInfo>().
                WithStaticStrategy("strategy").
                WithInMemoryStore().
                WithConfigurationStore();
            var sp = services.BuildServiceProvider();

            var resolver = sp.GetRequiredService<ITenantResolver<TenantInfo>>();
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
                WithDelegateStrategy(_ => Task.FromResult("not-found")).
                WithStaticStrategy("initech").
                WithInMemoryStore().
                WithConfigurationStore();
            var sp = services.BuildServiceProvider();
            sp.GetServices<IMultiTenantStore<TenantInfo>>().
                Single(i => i.GetType() == typeof(InMemoryStore<TenantInfo>)).TryAddAsync(new TenantInfo { Id = "null", Identifier = "null" }).Wait();

            var resolver = sp.GetRequiredService<ITenantResolver<TenantInfo>>();
            var result = resolver.ResolveAsync(new object()).Result;

            Assert.Equal("initech", result!.TenantInfo!.Identifier);
            Assert.IsType<StaticStrategy>(result!.StrategyInfo!.Strategy);
            Assert.IsType<ConfigurationStore<TenantInfo>>(result!.StoreInfo!.Store);
        }

        [Fact]
        void ReturnNullMultiTenantContextGivenNoStrategies()
        {
            var configBuilder = new ConfigurationBuilder();
            configBuilder.AddJsonFile("ConfigurationStoreTestSettings.json");
            var configuration = configBuilder.Build();

            var services = new ServiceCollection();
            services.AddSingleton<IConfiguration>(configuration);

            services.
                AddMultiTenant<TenantInfo>().
                WithInMemoryStore().
                WithConfigurationStore();

            var sp = services.BuildServiceProvider();
            var resolver = sp.GetRequiredService<ITenantResolver<TenantInfo>>();
            var result = resolver.ResolveAsync(new object()).Result;

            Assert.Null(result);
        }

        [Fact]
        void ThrowGivenStaticStrategyWithNullIdentifierArgument()
        {
            var services = new ServiceCollection();

            Action action = () => services.
                AddMultiTenant<TenantInfo>().
                WithStaticStrategy(null!).
                WithInMemoryStore().
                WithConfigurationStore();

            Assert.Throws<ArgumentNullException>(action);
        }

        [Fact]
        void ThrowGivenDelegateStrategyWithNullArgument()
        {
            var services = new ServiceCollection();

            Action action = () => services.
                AddMultiTenant<TenantInfo>().
                WithDelegateStrategy(null!).
                WithInMemoryStore().
                WithConfigurationStore();

            Assert.Throws<ArgumentNullException>(action);
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
                WithDelegateStrategy(_ => Task.FromResult("lol")). // should be ignored
                WithStaticStrategy("initech").
                WithInMemoryStore().
                WithConfigurationStore();
            var sp = services.BuildServiceProvider();
            sp.GetServices<IMultiTenantStore<TenantInfo>>().
                Single(i => i.GetType() == typeof(InMemoryStore<TenantInfo>)).TryAddAsync(new TenantInfo { Id = "null", Identifier = "null" }).Wait();

            var resolver = sp.GetRequiredService<ITenantResolver<TenantInfo>>();
            var result = resolver.ResolveAsync(new object()).Result;

            Assert.Equal("initech", result!.TenantInfo!.Identifier);
            Assert.IsType<StaticStrategy>(result!.StrategyInfo!.Strategy);
            Assert.IsType<ConfigurationStore<TenantInfo>>(result!.StoreInfo!.Store);
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
                WithDelegateStrategy(_ => Task.FromResult<string>(null!)).
                WithInMemoryStore().
                WithConfigurationStore();
            var sp = services.BuildServiceProvider();
            sp.GetServices<IMultiTenantStore<TenantInfo>>().
                Single(i => i.GetType() == typeof(InMemoryStore<TenantInfo>)).TryAddAsync(new TenantInfo { Id = "null", Identifier = "null" }).Wait();

            var resolver = sp.GetRequiredService<ITenantResolver<TenantInfo>>();
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
                WithDelegateStrategy(_ => Task.FromResult("not-found")).
                WithStaticStrategy("also-not-found").
                WithInMemoryStore().
                WithConfigurationStore();
            var sp = services.BuildServiceProvider();
            sp.
                GetServices<IMultiTenantStore<TenantInfo>>().
                Single(i => i.GetType() == typeof(InMemoryStore<TenantInfo>)).TryAddAsync(new TenantInfo { Id = "null", Identifier = "null" }).Wait();

            var resolver = sp.GetRequiredService<ITenantResolver<TenantInfo>>();
            var result = resolver.ResolveAsync(new object()).Result;

            Assert.Null(result);
        }
    }
}