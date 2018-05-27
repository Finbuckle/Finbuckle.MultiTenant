using System;
using System.Collections.Concurrent;
using System.Reflection;
using Finbuckle.MultiTenant.AspNetCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Xunit;
using Finbuckle.MultiTenant.Core;
using Finbuckle.MultiTenant.Core.Abstractions;
using Microsoft.Extensions.Options;

public class MultiTenansBuilderShould
{
    // Used in some tests.
    public int TestProp { get; set; }

    [Fact]
    public void AddStaticStrategy()
    {
        var services = new ServiceCollection();
        services.AddMultiTenant().WithStaticStrategy("initech");
        var sp = services.BuildServiceProvider();

        var resolver = sp.GetRequiredService<IMultiTenantStrategy>();
        Assert.IsType<StaticMultiTenantStrategy>(resolver);

        var tenantId = (string)resolver.GetType().
            GetField("identifier", BindingFlags.Instance | BindingFlags.NonPublic).
            GetValue(resolver);
        Assert.Equal("initech", tenantId);
    }

    [Fact]
    public void AddBasePathStrategy()
    {
        var services = new ServiceCollection();
        services.AddMultiTenant().WithBasePathStrategy();
        var sp = services.BuildServiceProvider();

        var resolver = sp.GetRequiredService<IMultiTenantStrategy>();
        Assert.IsType<BasePathMultiTenantStrategy>(resolver);
    }

    [Fact]
    public void AddHostStrategy()
    {
        var services = new ServiceCollection();
        services.AddMultiTenant().WithHostStrategy();
        var sp = services.BuildServiceProvider();

        var resolver = sp.GetRequiredService<IMultiTenantStrategy>();
        Assert.IsType<HostMultiTenantStrategy>(resolver);
    }

    [Fact]
    public void AddRouteStrategy()
    {
        var services = new ServiceCollection();
        services.AddMultiTenant().WithRouteStrategy("routeParam");
        var sp = services.BuildServiceProvider();

        var strategy = sp.GetRequiredService<IMultiTenantStrategy>();
        Assert.IsType<RouteMultiTenantStrategy>(strategy);

        var tenantParam = (string)strategy.GetType().
            GetField("tenantParam", BindingFlags.Instance | BindingFlags.NonPublic).
            GetValue(strategy);
        Assert.Equal("routeParam", tenantParam);
    }

    [Fact]
    public void AddInMemoryStoreViaConfigSection()
    {
        var configBuilder = new ConfigurationBuilder();
        configBuilder.AddJsonFile("testsettings.json");
        var configuration = configBuilder.Build();

        var services = new ServiceCollection();
        services.AddMultiTenant().
                WithInMemoryStore(configuration.GetSection("Finbuckle:MultiTenant:InMemoryMultiTenantStore"));
        var sp = services.BuildServiceProvider();

        var store = sp.GetRequiredService<IMultiTenantStore>() as InMemoryMultiTenantStore;

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
    public void AddInMemoryStoreViaConfigAction()
    {
        var configBuilder = new ConfigurationBuilder();
        configBuilder.AddJsonFile("testsettings.json");
        var configuration = configBuilder.Build();

        var services = new ServiceCollection();
        services.AddMultiTenant().
                WithInMemoryStore(o => configuration.GetSection("Finbuckle:MultiTenant:InMemoryMultiTenantStore").Bind(o));
        var sp = services.BuildServiceProvider();

        var store = sp.GetRequiredService<IMultiTenantStore>() as InMemoryMultiTenantStore;

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
    public void AddInMemoryStoreWithCaseSentivity()
    {
        var configBuilder = new ConfigurationBuilder();
        configBuilder.AddJsonFile("testsettings.json");
        var configuration = configBuilder.Build();

        var services = new ServiceCollection();
        services.AddMultiTenant().
                WithInMemoryStore(o => configuration.GetSection("Finbuckle:MultiTenant:InMemoryMultiTenantStore").Bind(o), false);
        var sp = services.BuildServiceProvider();

        var store = sp.GetRequiredService<IMultiTenantStore>() as InMemoryMultiTenantStore;

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
                WithInMemoryStore(o => configuration.GetSection("Finbuckle:MultiTenant:InMemoryMultiTenantStore").Bind(o));
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
                WithInMemoryStore(o => configuration.GetSection("Finbuckle:MultiTenant:InMemoryMultiTenantStore").Bind(o));
        var sp = services.BuildServiceProvider();

        Assert.Throws<MultiTenantException>(() => sp.GetRequiredService<IMultiTenantStore>());
    }

    [Fact]
    public void AddPerTenantConfigOptions()
    {
        var services = new ServiceCollection();
        // Note: using MultiTenansBuilderShould as our test options class.
        services.AddMultiTenant().WithPerTenantOptionsConfig<MultiTenansBuilderShould>((o, tc) => o.TestProp = 1);
        var sp = services.BuildServiceProvider();

        var cache = sp.GetRequiredService<IOptionsMonitorCache<MultiTenansBuilderShould>>();
    }
}