//    Copyright 2018 Andrew White
// 
//    Licensed under the Apache License, Version 2.0 (the "License");
//    you may not use this file except in compliance with the License.
//    You may obtain a copy of the License at
// 
//        http://www.apache.org/licenses/LICENSE-2.0
// 
//    Unless required by applicable law or agreed to in writing, software
//    distributed under the License is distributed on an "AS IS" BASIS,
//    WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//    See the License for the specific language governing permissions and
//    limitations under the License.

using System;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Xunit;
using Microsoft.Extensions.Options;
using Microsoft.AspNetCore.Authentication;
using Finbuckle.MultiTenant;
using Finbuckle.MultiTenant.Stores;
using Finbuckle.MultiTenant.Strategies;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Routing;
using Finbuckle.MultiTenant.AspNetCore;

public class MultiTenantBuilderShould
{
    // Used in some tests.
    public int TestProperty { get; set; }

    [Fact]
    public void AddFallbackTenantIdentifier()
    {
        var services = new ServiceCollection();
        services.AddMultiTenant().WithFallbackStrategy("test");
        var sp = services.BuildServiceProvider();

        var strategy = sp.GetRequiredService<FallbackStrategy>();
        Assert.Equal("test", strategy.identifier);
    }

    [Fact]
    public void ThrowIfFallbackTenantIdentifierIsNull()
    {
        var services = new ServiceCollection();
        Assert.Throws<ArgumentNullException>(() => services.AddMultiTenant().WithFallbackStrategy(null));
    }

    [Theory]
    [InlineData(ServiceLifetime.Singleton)]
    [InlineData(ServiceLifetime.Scoped)]
    [InlineData(ServiceLifetime.Transient)]
    public void AddCustomStoreWithDefaultCtorAndLifetime(ServiceLifetime lifetime)
    {
        var services = new ServiceCollection();
        services.AddMultiTenant().WithStore<TestNullStore>(lifetime);

        var sp = services.BuildServiceProvider();

        var store = sp.GetRequiredService<IMultiTenantStore>();
        var scope = sp.CreateScope();
        var store2 = scope.ServiceProvider.GetRequiredService<IMultiTenantStore>();

        switch (lifetime)
        {
            case ServiceLifetime.Singleton:
                Assert.Same(store, store2);
                break;

            case ServiceLifetime.Scoped:
                Assert.NotSame(store, store2);
                break;

            case ServiceLifetime.Transient:
                Assert.NotSame(store, store2);
                store = scope.ServiceProvider.GetRequiredService<IMultiTenantStore>();
                Assert.NotSame(store, store2);
                break;
        }
    }

    [Theory]
    [InlineData(ServiceLifetime.Singleton)]
    [InlineData(ServiceLifetime.Scoped)]
    [InlineData(ServiceLifetime.Transient)]
    public void AddCustomStoreWithParamsAndLifetime(ServiceLifetime lifetime)
    {
        var services = new ServiceCollection();
        services.AddMultiTenant().WithStore<TestNullStore2>(lifetime, new[] { new object() });

        var sp = services.BuildServiceProvider();

        var store = sp.GetRequiredService<IMultiTenantStore>();
        var scope = sp.CreateScope();
        var store2 = scope.ServiceProvider.GetRequiredService<IMultiTenantStore>();

        switch (lifetime)
        {
            case ServiceLifetime.Singleton:
                Assert.Same(store, store2);
                break;

            case ServiceLifetime.Scoped:
                Assert.NotSame(store, store2);
                break;

            case ServiceLifetime.Transient:
                Assert.NotSame(store, store2);
                store = scope.ServiceProvider.GetRequiredService<IMultiTenantStore>();
                Assert.NotSame(store, store2);
                break;
        }
    }

    [Theory]
    [InlineData(ServiceLifetime.Singleton)]
    [InlineData(ServiceLifetime.Scoped)]
    [InlineData(ServiceLifetime.Transient)]
    public void AddCustomStoreWithFactoryAndLifetime(ServiceLifetime lifetime)
    {
        var services = new ServiceCollection();
        services.AddMultiTenant().WithStore(lifetime, _sp => new TestNullStore());

        var sp = services.BuildServiceProvider();

        var store = sp.GetRequiredService<IMultiTenantStore>();
        var scope = sp.CreateScope();
        var store2 = scope.ServiceProvider.GetRequiredService<IMultiTenantStore>();

        switch (lifetime)
        {
            case ServiceLifetime.Singleton:
                Assert.Same(store, store2);
                break;

            case ServiceLifetime.Scoped:
                Assert.NotSame(store, store2);
                break;

            case ServiceLifetime.Transient:
                Assert.NotSame(store, store2);
                store = scope.ServiceProvider.GetRequiredService<IMultiTenantStore>();
                Assert.NotSame(store, store2);
                break;
        }
    }

    [Fact]
    public void ThrowIfNullFactoryAddingCustomStore()
    {
        var services = new ServiceCollection();
        Assert.Throws<ArgumentNullException>(() => services.AddMultiTenant().WithStore<InMemoryStore>(ServiceLifetime.Singleton, factory: null));
    }

    [Fact]
    public void AddEFCoreStore()
    {
        var services = new ServiceCollection();
        services.AddMultiTenant().WithStaticStrategy("initech").WithEFCoreStore<TestEFCoreStoreDbContext, TestTenantInfoEntity>();
        var sp = services.BuildServiceProvider();

        var resolver = sp.CreateScope().ServiceProvider.GetRequiredService<IMultiTenantStore>();
        Assert.IsType<MultiTenantStoreWrapper<EFCoreStore<TestEFCoreStoreDbContext, TestTenantInfoEntity>>>(resolver);
    }

    [Fact]
    public void AddEFCoreStoreWithExistingDbContext()
    {
        var services = new ServiceCollection();
        services.AddDbContext<TestEFCoreStoreDbContext>(o => o.UseInMemoryDatabase(nameof(AddEFCoreStoreWithExistingDbContext)));
        services.AddMultiTenant().WithStaticStrategy("initech").WithEFCoreStore<TestEFCoreStoreDbContext, TestTenantInfoEntity>();
        var sp = services.BuildServiceProvider().CreateScope().ServiceProvider;

        var resolver = sp.GetRequiredService<IMultiTenantStore>();
        Assert.IsType<MultiTenantStoreWrapper<EFCoreStore<TestEFCoreStoreDbContext, TestTenantInfoEntity>>>(resolver);
    }

    [Fact]
    public void AddInMemoryStoreViaConfigSection()
    {
        var configBuilder = new ConfigurationBuilder();
        configBuilder.AddJsonFile("testsettings.json");
        var configuration = configBuilder.Build();

        var services = new ServiceCollection();
        services.AddMultiTenant().
                WithInMemoryStore(configuration.GetSection("Finbuckle:MultiTenant:InMemoryStore"));
        var sp = services.BuildServiceProvider();

        var store = sp.GetRequiredService<IMultiTenantStore>(); ;

        var tc = store.TryGetByIdentifierAsync("initech").Result;
        Assert.Equal("initech", tc.Id);
        Assert.Equal("initech", tc.Identifier);
        Assert.Equal("Initech", tc.Name);
        Assert.Equal("1234", tc.Items["test_item"]);
        // Note: connection string below loading from default in json.
        Assert.Equal("Datasource=sample.db", tc.ConnectionString);

        // Case insensitive test.
        tc = store.TryGetByIdentifierAsync("LOL").Result;
        Assert.Equal("lol", tc.Id);
        Assert.Equal("lol", tc.Identifier);
        Assert.Equal("LOL", tc.Name);
        Assert.Equal("Datasource=lol.db", tc.ConnectionString);
    }

    [Fact]
    public void AddInMemoryStoreViaConfigAction()
    {
        var configBuilder = new ConfigurationBuilder();
        configBuilder.AddJsonFile("testsettings.json");
        var configuration = configBuilder.Build();

        var services = new ServiceCollection();
        services.AddMultiTenant().
                WithInMemoryStore(o => configuration.GetSection("Finbuckle:MultiTenant:InMemoryStore").Bind(o));
        var sp = services.BuildServiceProvider();

        var store = sp.GetRequiredService<IMultiTenantStore>();

        var tc = store.TryGetByIdentifierAsync("initech").Result;
        Assert.Equal("initech", tc.Id);
        Assert.Equal("initech", tc.Identifier);
        Assert.Equal("Initech", tc.Name);
        // Note: connection string below loading from default in json.
        Assert.Equal("Datasource=sample.db", tc.ConnectionString);

        // Case insensitive test.
        tc = store.TryGetByIdentifierAsync("LOL").Result;
        Assert.Equal("lol", tc.Id);
        Assert.Equal("lol", tc.Identifier);
        Assert.Equal("LOL", tc.Name);
        Assert.Equal("Datasource=lol.db", tc.ConnectionString);
    }

    [Fact]
    public void ThrowIfNullParamAddingInMemoryStore()
    {
        var services = new ServiceCollection();
        Assert.Throws<ArgumentNullException>(()
            => services.AddMultiTenant().WithInMemoryStore(config: null));
    }

    [Fact]
    public void AddInMemoryStoreWithCaseSentivity()
    {
        var configBuilder = new ConfigurationBuilder();
        configBuilder.AddJsonFile("testsettings.json");
        var configuration = configBuilder.Build();

        var services = new ServiceCollection();
        services.AddMultiTenant().
                WithInMemoryStore(o => configuration.GetSection("Finbuckle:MultiTenant:InMemoryStore").Bind(o), false);
        var sp = services.BuildServiceProvider();

        var store = sp.GetRequiredService<IMultiTenantStore>();

        var tc = store.TryGetByIdentifierAsync("lol").Result;
        Assert.Equal("lol", tc.Id);
        Assert.Equal("lol", tc.Identifier);
        Assert.Equal("LOL", tc.Name);
        Assert.Equal("Datasource=lol.db", tc.ConnectionString);

        // Case sensitive test.
        tc = store.TryGetByIdentifierAsync("LOL").Result;
        Assert.Null(tc);
    }

    [Fact]
    public void ThrowIfDuplicateIdentifierInTenantConfig()
    {
        var configBuilder = new ConfigurationBuilder();
        configBuilder.AddJsonFile("testsettings_duplicates.json");
        var configuration = configBuilder.Build();

        var services = new ServiceCollection();
        services.AddMultiTenant().
                WithInMemoryStore(o => configuration.GetSection("Finbuckle:MultiTenant:InMemoryStore").Bind(o));
        var sp = services.BuildServiceProvider();

        Assert.Throws<MultiTenantException>(() => sp.GetRequiredService<IMultiTenantStore>());
    }

    [Theory]
    [InlineData("testsettings_missing_id.json")]
    [InlineData("testsettings_missing_identifier.json")]
    [InlineData("testsettings_empty_id.json")]
    [InlineData("testsettings_empty_identifier.json")]
    public void ThrowIfMissingIdOrIdentifierInTenantConfig(string jsonFile)
    {
        var configBuilder = new ConfigurationBuilder();
        configBuilder.AddJsonFile(jsonFile);
        var configuration = configBuilder.Build();

        var services = new ServiceCollection();
        services.AddMultiTenant().
                WithInMemoryStore(o => configuration.GetSection("Finbuckle:MultiTenant:InMemoryStore").Bind(o));
        var sp = services.BuildServiceProvider();

        Assert.Throws<MultiTenantException>(() => sp.GetRequiredService<IMultiTenantStore>());
    }

    [Fact]
    public void AddPerTenantOptions()
    {
        var services = new ServiceCollection();
        // Note: using MultiTenantBuilderShould as our test options class.
        services.AddMultiTenant().WithPerTenantOptions<MultiTenantBuilderShould>((o, tc) => o.TestProperty = 1);
        var sp = services.BuildServiceProvider();

        var cache = sp.GetRequiredService<IOptionsMonitorCache<MultiTenantBuilderShould>>();
    }

    [Fact]
    public void ThrowIfNullParamAddingPerTenantOptions()
    {
        var services = new ServiceCollection();
        Assert.Throws<ArgumentNullException>(() => services.AddMultiTenant().WithPerTenantOptions<MultiTenantBuilderShould>(null));
    }

    [Fact]
    public void AddRemoteAuthenticationServices()
    {
        var services = new ServiceCollection();
        services.AddAuthentication();
        services.AddMultiTenant().WithRemoteAuthentication();
        var sp = services.BuildServiceProvider();

        var authService = sp.GetRequiredService<IAuthenticationService>(); // Throw fails
        var schemeProvider = sp.GetRequiredService<IAuthenticationSchemeProvider>(); // Throw fails
    }

    [Fact]
    public void AddDelegateStrategy()
    {
        var services = new ServiceCollection();
        services.AddMultiTenant().WithDelegateStrategy(c => Task.FromResult("Hi"));
        var sp = services.BuildServiceProvider();

        var strategy = sp.GetRequiredService<IMultiTenantStrategy>();
        Assert.IsType<MultiTenantStrategyWrapper<DelegateStrategy>>(strategy);
    }

    [Fact]
    public void AddStaticStrategy()
    {
        var services = new ServiceCollection();
        services.AddMultiTenant().WithStaticStrategy("initech");
        var sp = services.BuildServiceProvider();

        var strategy = sp.GetRequiredService<IMultiTenantStrategy>();
        Assert.IsType<MultiTenantStrategyWrapper<StaticStrategy>>(strategy);
    }

    [Fact]
    public void ThrowIfNullParamAddingStaticStrategy()
    {
        var services = new ServiceCollection();
        Assert.Throws<ArgumentException>(()
            => services.AddMultiTenant().WithStaticStrategy(null));
    }

    [Fact]
    public void AddBasePathStrategy()
    {
        var services = new ServiceCollection();
        services.AddMultiTenant().WithBasePathStrategy();
        var sp = services.BuildServiceProvider();

        var strategy = sp.GetRequiredService<IMultiTenantStrategy>();
        Assert.IsType<MultiTenantStrategyWrapper<BasePathStrategy>>(strategy);
    }

    [Fact]
    public void AddHostStrategy()
    {
        var services = new ServiceCollection();
        services.AddMultiTenant().WithHostStrategy();
        var sp = services.BuildServiceProvider();

        var strategy = sp.GetRequiredService<IMultiTenantStrategy>();
        Assert.IsType<MultiTenantStrategyWrapper<HostStrategy>>(strategy);
    }

    [Fact]
    public void ThrowIfNullParamAddingHostStrategy()
    {
        var services = new ServiceCollection();
        Assert.Throws<ArgumentException>(()
            => services.AddMultiTenant().WithHostStrategy(null));
    }

    [Fact]
    public void AddRouteStrategy()
    {
        var services = new ServiceCollection();
        services.AddMultiTenant().WithRouteStrategy("routeParam", cr => cr.MapRoute("test", "test"));
        var sp = services.BuildServiceProvider();

        var strategy = sp.GetRequiredService<IMultiTenantStrategy>();
        Assert.IsType<MultiTenantStrategyWrapper<RouteStrategy>>(strategy);
    }

    [Fact]
    public void ThrowIfNullParamAddingRouteStrategy()
    {
        var services = new ServiceCollection();
        Assert.Throws<ArgumentException>(()
            => services.AddMultiTenant().WithRouteStrategy(null, rb => rb.GetType()));
    }

    [Fact]
    public void ThrowIfNullRouteConfigAddingRouteStrategy()
    {
        var services = new ServiceCollection();
        Assert.Throws<ArgumentNullException>(()
            => services.AddMultiTenant().WithRouteStrategy(null));
        Assert.Throws<ArgumentNullException>(()
            => services.AddMultiTenant().WithRouteStrategy("param", null));
    }

    [Theory]
    [InlineData(ServiceLifetime.Singleton)]
    [InlineData(ServiceLifetime.Scoped)]
    [InlineData(ServiceLifetime.Transient)]
    public void AddCustomStrategyWithDefaultCtorAndLifetime(ServiceLifetime lifetime)
    {
        var services = new ServiceCollection();
        services.AddMultiTenant().WithStrategy<BasePathStrategy>(lifetime);

        var sp = services.BuildServiceProvider();

        var strategy = sp.GetRequiredService<IMultiTenantStrategy>();
        var scope = sp.CreateScope();
        var strategy2 = scope.ServiceProvider.GetRequiredService<IMultiTenantStrategy>();

        switch (lifetime)
        {
            case ServiceLifetime.Singleton:
                Assert.Same(strategy, strategy2);
                break;

            case ServiceLifetime.Scoped:
                Assert.NotSame(strategy, strategy2);
                break;

            case ServiceLifetime.Transient:
                Assert.NotSame(strategy, strategy2);
                strategy = scope.ServiceProvider.GetRequiredService<IMultiTenantStrategy>();
                Assert.NotSame(strategy, strategy2);
                break;
        }
    }

    [Theory]
    [InlineData(ServiceLifetime.Singleton)]
    [InlineData(ServiceLifetime.Scoped)]
    [InlineData(ServiceLifetime.Transient)]
    public void AddCustomStrategyWithParamsAndLifetime(ServiceLifetime lifetime)
    {
        var services = new ServiceCollection();
        services.AddMultiTenant().WithStrategy<RouteStrategy>(lifetime, new object[] { "__tenant__", new Action<IRouteBuilder>((rb) => {}) });

        var sp = services.BuildServiceProvider();

        var strategy = sp.GetRequiredService<IMultiTenantStrategy>();
        var scope = sp.CreateScope();
        var strategy2 = scope.ServiceProvider.GetRequiredService<IMultiTenantStrategy>();

        switch (lifetime)
        {
            case ServiceLifetime.Singleton:
                Assert.Same(strategy, strategy2);
                break;

            case ServiceLifetime.Scoped:
                Assert.NotSame(strategy, strategy2);
                break;

            case ServiceLifetime.Transient:
                Assert.NotSame(strategy, strategy2);
                strategy = scope.ServiceProvider.GetRequiredService<IMultiTenantStrategy>();
                Assert.NotSame(strategy, strategy2);
                break;
        }
    }

    [Theory]
    [InlineData(ServiceLifetime.Singleton)]
    [InlineData(ServiceLifetime.Scoped)]
    [InlineData(ServiceLifetime.Transient)]
    public void AddCustomStrategyWithFactoryAndLifetime(ServiceLifetime lifetime)
    {
        var services = new ServiceCollection();
        services.AddMultiTenant().WithStrategy(lifetime, _sp => new BasePathStrategy());

        var sp = services.BuildServiceProvider();

        var strategy = sp.GetRequiredService<IMultiTenantStrategy>();
        var scope = sp.CreateScope();
        var strategy2 = scope.ServiceProvider.GetRequiredService<IMultiTenantStrategy>();

        switch (lifetime)
        {
            case ServiceLifetime.Singleton:
                Assert.Same(strategy, strategy2);
                break;

            case ServiceLifetime.Scoped:
                Assert.NotSame(strategy, strategy2);
                break;

            case ServiceLifetime.Transient:
                Assert.NotSame(strategy, strategy2);
                strategy = scope.ServiceProvider.GetRequiredService<IMultiTenantStrategy>();
                Assert.NotSame(strategy, strategy2);
                break;
        }
    }

    [Fact]
    public void ThrowIfNullFactoryAddingCustomStrategy()
    {
        var services = new ServiceCollection();
        Assert.Throws<ArgumentNullException>(() => services.AddMultiTenant().WithStrategy<StaticStrategy>(ServiceLifetime.Singleton, factory: null));
    }
}