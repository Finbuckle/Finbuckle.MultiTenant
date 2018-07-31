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
//using System.Reflection;
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

public class MultiTenantBuilderShould
{
    // Used in some tests.
    public int TestProperty { get; set; }

    private static IWebHostBuilder GetTestHostBuilder(string identifier)
    {
        return new WebHostBuilder()
                    .ConfigureServices(services =>
                    {
                        services.AddMultiTenant().WithStaticStrategy(identifier).WithInMemoryStore();
                        services.AddMvc();
                    })
                    .Configure(app =>
                    {
                        app.UseMultiTenant();
                        app.Run(async context =>
                        {
                            await context.Response.WriteAsync(context.RequestServices.GetRequiredService<TenantInfo>().Identifier);
                        });

                        var store = app.ApplicationServices.GetRequiredService<IMultiTenantStore>();
                        store.TryAddAsync(new TenantInfo(identifier, identifier, null, null, null)).Wait();
                    });
    }

    [Fact]
    public async Task AddTenantInfoService()
    {
        IWebHostBuilder hostBuilder = GetTestHostBuilder("test_tenant");

        using (var server = new TestServer(hostBuilder))
        {
            var client = server.CreateClient();
            var response = await client.GetStringAsync("/");
            Assert.Equal("test_tenant", response);
        }
    }

    [Fact]
    public void AddCustomStoreWithDefaultCtor()
    {
        var services = new ServiceCollection();
        services.AddMultiTenant().WithStore<TestStore>();
        var sp = services.BuildServiceProvider();

        var strategy = sp.GetRequiredService<IMultiTenantStore>();
    }

    [Fact]
    public void AddCustomStoreWithFactory()
    {
        var services = new ServiceCollection();
        services.AddMultiTenant().WithStore(_sp => new TestStore());
        var sp = services.BuildServiceProvider();

        var strategy = sp.GetRequiredService<IMultiTenantStore>();
    }

    [Fact]
    public void ThrowIfNullParamAddingCustomStore()
    {
        var services = new ServiceCollection();
        Assert.Throws<ArgumentNullException>(() => services.AddMultiTenant().WithStore(null));
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

        var store = sp.GetRequiredService<IMultiTenantStore>() as InMemoryStore;

        var tc = store.GetByIdentifierAsync("initech").Result;
        Assert.Equal("initech", tc.Id);
        Assert.Equal("initech", tc.Identifier);
        Assert.Equal("Initech", tc.Name);
        Assert.Equal("1234", tc.Items["test_item"]);
        // Note: connection string below loading from default in json.
        Assert.Equal("Datasource=sample.db", tc.ConnectionString);

        // Case insensitive test.
        tc = store.GetByIdentifierAsync("LOL").Result;
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

        var store = sp.GetRequiredService<IMultiTenantStore>() as InMemoryStore;

        var tc = store.GetByIdentifierAsync("initech").Result;
        Assert.Equal("initech", tc.Id);
        Assert.Equal("initech", tc.Identifier);
        Assert.Equal("Initech", tc.Name);
        // Note: connection string below loading from default in json.
        Assert.Equal("Datasource=sample.db", tc.ConnectionString);

        // Case insensitive test.
        tc = store.GetByIdentifierAsync("LOL").Result;
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

        var store = sp.GetRequiredService<IMultiTenantStore>() as InMemoryStore;

        var tc = store.GetByIdentifierAsync("lol").Result;
        Assert.Equal("lol", tc.Id);
        Assert.Equal("lol", tc.Identifier);
        Assert.Equal("LOL", tc.Name);
        Assert.Equal("Datasource=lol.db", tc.ConnectionString);

        // Case sensitive test.
        tc = store.GetByIdentifierAsync("LOL").Result;
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
        // Note: using MultiTenansBuilderShould as our test options class.
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

        var authService = sp.GetRequiredService<IAuthenticationService>();
        var schemeProvider = sp.GetRequiredService<IAuthenticationSchemeProvider>();
    }

    [Fact]
    public void AddStaticStrategy()
    {
        var services = new ServiceCollection();
        services.AddMultiTenant().WithStaticStrategy("initech");
        var sp = services.BuildServiceProvider();

        var resolver = sp.GetRequiredService<IMultiTenantStrategy>() as StaticStrategy;
        Assert.IsType<StaticStrategy>(resolver);
        Assert.Equal("initech", resolver.identifier);
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

        var resolver = sp.GetRequiredService<IMultiTenantStrategy>();
        Assert.IsType<BasePathStrategy>(resolver);
    }

    [Fact]
    public void AddHostStrategy()
    {
        var services = new ServiceCollection();
        services.AddMultiTenant().WithHostStrategy();
        var sp = services.BuildServiceProvider();

        var resolver = sp.GetRequiredService<IMultiTenantStrategy>();
        Assert.IsType<HostStrategy>(resolver);
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

        var strategy = sp.GetRequiredService<IMultiTenantStrategy>() as RouteStrategy;
        Assert.IsType<RouteStrategy>(strategy);
        Assert.Equal("routeParam", strategy.tenantParam);
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

    [Fact]
    public void AddCustomStrategyWithDefaultCtor()
    {
        var services = new ServiceCollection();
        services.AddMultiTenant().WithStrategy<BasePathStrategy>();
        var sp = services.BuildServiceProvider();

        var strategy = sp.GetRequiredService<IMultiTenantStrategy>();
    }

    [Fact]
    public void AddCustomStrategyWithFactory()
    {
        var services = new ServiceCollection();
        services.AddMultiTenant().WithStrategy(_sp => new BasePathStrategy());
        var sp = services.BuildServiceProvider();

        var strategy = sp.GetRequiredService<IMultiTenantStrategy>();
    }

    [Fact]
    public void ThrowIfNullFactoryAddingCustomStrategy()
    {
        var services = new ServiceCollection();
        Assert.Throws<ArgumentNullException>(() => services.AddMultiTenant().WithStrategy(null));
    }
}