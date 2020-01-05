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

using Microsoft.Extensions.DependencyInjection;
using Xunit;
using Finbuckle.MultiTenant;
using Finbuckle.MultiTenant.Strategies;
using System;
using Microsoft.Extensions.Configuration;
using System.Threading.Tasks;

public class MultiTenantBuilderExtensionsShould
{
    [Fact]
    public void AddFallbackTenantIdentifier()
    {
        var services = new ServiceCollection();
        var builder = new FinbuckleMultiTenantBuilder(services);
        builder.WithFallbackStrategy("test");
        var sp = services.BuildServiceProvider();

        var strategy = sp.GetRequiredService<FallbackStrategy>();
        Assert.Equal("test", strategy.identifier);
    }

    [Fact]
    public void ThrowIfFallbackTenantIdentifierIsNull()
    {
        var services = new ServiceCollection();
        var builder = new FinbuckleMultiTenantBuilder(services);
        Assert.Throws<ArgumentNullException>(() => builder.WithFallbackStrategy(null));
    }

    [Fact]
    public void AddConfigurationStoreWithDefaults()
    {
        var configBuilder = new ConfigurationBuilder();
        configBuilder.AddJsonFile("ConfigurationStoreTestSettings.json");
        var configuration = configBuilder.Build();

        var services = new ServiceCollection();
        var builder = new FinbuckleMultiTenantBuilder(services);
        builder.WithConfigurationStore();
        services.AddSingleton<IConfiguration>(configuration);
        var sp = services.BuildServiceProvider();

        var store = sp.GetRequiredService<IMultiTenantStore>(); ;

        var tc = store.TryGetByIdentifierAsync("initech").Result;
        Assert.Equal("initech-id", tc.Id);
        Assert.Equal("initech", tc.Identifier);
        Assert.Equal("Initech", tc.Name);
        Assert.Equal("1234", tc.Items["test_item"]);
        // Note: connection string below loading from default in json.
        Assert.Equal("Datasource=sample.db", tc.ConnectionString);

        tc = store.TryGetByIdentifierAsync("lol").Result;
        Assert.Equal("lol-id", tc.Id);
        Assert.Equal("lol", tc.Identifier);
        Assert.Equal("LOL", tc.Name);
        Assert.Equal("Datasource=lol.db", tc.ConnectionString);
    }

    [Fact]
    public void AddConfigurationStoreWithSectionName()
    {
        var configBuilder = new ConfigurationBuilder();
        configBuilder.AddJsonFile("ConfigurationStoreTestSettings.json");
        IConfiguration configuration = configBuilder.Build();

        var services = new ServiceCollection();
        var builder = new FinbuckleMultiTenantBuilder(services);

        // Non-default section name.
        configuration = configuration.GetSection("Finbuckle");
        builder.WithConfigurationStore(configuration, "MultiTenant:Stores:ConfigurationStore");
        var sp = services.BuildServiceProvider();

        var store = sp.GetRequiredService<IMultiTenantStore>(); ;

        var tc = store.TryGetByIdentifierAsync("initech").Result;
        Assert.Equal("initech-id", tc.Id);
        Assert.Equal("initech", tc.Identifier);
        Assert.Equal("Initech", tc.Name);
        Assert.Equal("1234", tc.Items["test_item"]);
        // Note: connection string below loading from default in json.
        Assert.Equal("Datasource=sample.db", tc.ConnectionString);

        tc = store.TryGetByIdentifierAsync("lol").Result;
        Assert.Equal("lol-id", tc.Id);
        Assert.Equal("lol", tc.Identifier);
        Assert.Equal("LOL", tc.Name);
        Assert.Equal("Datasource=lol.db", tc.ConnectionString);
    }

    [Fact]
    // TODO: remove and cleanup when this functioanlity is removed
    public void AddInMemoryStoreViaConfigSection()
    {
        var configBuilder = new ConfigurationBuilder();
        configBuilder.AddJsonFile("testsettings.json");
        var configuration = configBuilder.Build();

        var services = new ServiceCollection();
        var builder = new FinbuckleMultiTenantBuilder(services);
        builder.WithInMemoryStore(configuration.GetSection("Finbuckle:MultiTenant:InMemoryStore"));
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
        var builder = new FinbuckleMultiTenantBuilder(services);
        builder.
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
        var builder = new FinbuckleMultiTenantBuilder(services);
        Assert.Throws<ArgumentNullException>(()
            => builder.WithInMemoryStore(config: null));
    }

    [Fact]
    public void AddInMemoryStoreWithCaseSentivity()
    {
        var configBuilder = new ConfigurationBuilder();
        configBuilder.AddJsonFile("testsettings.json");
        var configuration = configBuilder.Build();

        var services = new ServiceCollection();
        var builder = new FinbuckleMultiTenantBuilder(services);
        builder.
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
        var builder = new FinbuckleMultiTenantBuilder(services);
        builder.
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
        var builder = new FinbuckleMultiTenantBuilder(services);
        builder.WithInMemoryStore(o => configuration.GetSection("Finbuckle:MultiTenant:InMemoryStore").Bind(o));
        var sp = services.BuildServiceProvider();

        Assert.Throws<MultiTenantException>(() => sp.GetRequiredService<IMultiTenantStore>());
    }

    [Fact]
    public void AddDelegateStrategy()
    {
        var services = new ServiceCollection();
        var builder = new FinbuckleMultiTenantBuilder(services);
        builder.WithDelegateStrategy(c => Task.FromResult("Hi"));
        var sp = services.BuildServiceProvider();

        var strategy = sp.GetRequiredService<IMultiTenantStrategy>();
        Assert.IsType<MultiTenantStrategyWrapper<DelegateStrategy>>(strategy);
    }

    [Fact]
    public void AddStaticStrategy()
    {
        var services = new ServiceCollection();
        var builder = new FinbuckleMultiTenantBuilder(services);
        builder.WithStaticStrategy("initech");
        var sp = services.BuildServiceProvider();

        var strategy = sp.GetRequiredService<IMultiTenantStrategy>();
        Assert.IsType<MultiTenantStrategyWrapper<StaticStrategy>>(strategy);
    }

    [Fact]
    public void ThrowIfNullParamAddingStaticStrategy()
    {
        var services = new ServiceCollection();
        var builder = new FinbuckleMultiTenantBuilder(services);
        Assert.Throws<ArgumentException>(()
            => builder.WithStaticStrategy(null));
    }
}