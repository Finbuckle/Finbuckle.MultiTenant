//    Copyright 2018-2020 Finbuckle LLC, Andrew White, and Contributors
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
using Finbuckle.MultiTenant.Stores;

public class MultiTenantBuilderExtensionsShould
{
    [Fact]
    public void AddDistributedCacheStoreDefault()
    {
        var services = new ServiceCollection();
        services.AddDistributedMemoryCache();
        var builder = new FinbuckleMultiTenantBuilder<TenantInfo>(services);
        builder.WithDistributedCacheStore();
        var sp = services.BuildServiceProvider();
        var store = sp.GetRequiredService<IMultiTenantStore<TenantInfo>>();
        Assert.IsType<DistributedCacheStore<TenantInfo>>(store);
    }

    [Fact]
    public void AddDistributedCacheStoreWithSlidingExpiration()
    {
        var services = new ServiceCollection();
        services.AddDistributedMemoryCache();
        var builder = new FinbuckleMultiTenantBuilder<TenantInfo>(services);
        builder.WithDistributedCacheStore(TimeSpan.FromMinutes(5));
        var sp = services.BuildServiceProvider();
        var store = sp.GetRequiredService<IMultiTenantStore<TenantInfo>>();
        Assert.IsType<DistributedCacheStore<TenantInfo>>(store);
    }

    [Fact]
    public void AddHttpRemoteStoreAndHttpRemoteStoreClient()
    {
        var services = new ServiceCollection();
        var builder = new FinbuckleMultiTenantBuilder<TenantInfo>(services);
        builder.WithHttpRemoteStore("http://example.com");
        var sp = services.BuildServiceProvider();
        
        sp.GetRequiredService<HttpRemoteStoreClient<TenantInfo>>();
        var store = sp.GetRequiredService<IMultiTenantStore<TenantInfo>>();
        Assert.IsType<HttpRemoteStore<TenantInfo>>(store);
    }

    [Fact]
    public void AddHttpRemoteStoreWithHttpClientBuilders()
    {
        var services = new ServiceCollection();
        var builder = new FinbuckleMultiTenantBuilder<TenantInfo>(services);
        var flag = false;
        builder.WithHttpRemoteStore("http://example.com", b => flag = true);
        var sp = services.BuildServiceProvider();
        
        sp.GetRequiredService<HttpRemoteStoreClient<TenantInfo>>();
        var store = sp.GetRequiredService<IMultiTenantStore<TenantInfo>>();
        Assert.IsType<HttpRemoteStore<TenantInfo>>(store);
        Assert.True(flag);
    }

    [Fact]
    public void AddConfigurationStoreWithDefaults()
    {
        var configBuilder = new ConfigurationBuilder();
        configBuilder.AddJsonFile("ConfigurationStoreTestSettings.json");
        var configuration = configBuilder.Build();

        var services = new ServiceCollection();
        var builder = new FinbuckleMultiTenantBuilder<TenantInfo>(services);
        builder.WithConfigurationStore();
        services.AddSingleton<IConfiguration>(configuration);
        var sp = services.BuildServiceProvider();

        var store = sp.GetRequiredService<IMultiTenantStore<TenantInfo>>();
        Assert.IsType<ConfigurationStore<TenantInfo>>(store);

        var tc = store.TryGetByIdentifierAsync("initech").Result;
        Assert.Equal("initech-id", tc.Id);
        Assert.Equal("initech", tc.Identifier);
        Assert.Equal("Initech", tc.Name);
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
        var builder = new FinbuckleMultiTenantBuilder<TenantInfo>(services);

        // Non-default section name.
        configuration = configuration.GetSection("Finbuckle");
        builder.WithConfigurationStore(configuration, "MultiTenant:Stores:ConfigurationStore");
        var sp = services.BuildServiceProvider();

        var store = sp.GetRequiredService<IMultiTenantStore<TenantInfo>>();
        Assert.IsType<ConfigurationStore<TenantInfo>>(store);

        var tc = store.TryGetByIdentifierAsync("initech").Result;
        Assert.Equal("initech-id", tc.Id);
        Assert.Equal("initech", tc.Identifier);
        Assert.Equal("Initech", tc.Name);
        // Note: connection string below loading from default in json.
        Assert.Equal("Datasource=sample.db", tc.ConnectionString);

        tc = store.TryGetByIdentifierAsync("lol").Result;
        Assert.Equal("lol-id", tc.Id);
        Assert.Equal("lol", tc.Identifier);
        Assert.Equal("LOL", tc.Name);
        Assert.Equal("Datasource=lol.db", tc.ConnectionString);
    }

    [Fact]
    public void ThrowIfNullParamAddingInMemoryStore()
    {
        var services = new ServiceCollection();
        var builder = new FinbuckleMultiTenantBuilder<TenantInfo>(services);
        Assert.Throws<ArgumentNullException>(()
            => builder.WithInMemoryStore<TenantInfo>(null));
    }

    [Fact]
    public void AddInMemoryStoreWithCaseSentivity()
    {
        var configBuilder = new ConfigurationBuilder();
        //configBuilder.AddJsonFile("testsettings.json");
        var configuration = configBuilder.Build();

        var services = new ServiceCollection();
        var builder = new FinbuckleMultiTenantBuilder<TenantInfo>(services);
        builder.WithInMemoryStore(options =>
        {
            options.IsCaseSensitive = true;
            options.Tenants.Add(new TenantInfo{ Id = "lol", Identifier = "lol", Name = "LOL", ConnectionString = "Datasource=lol.db"});
        });
        var sp = services.BuildServiceProvider();

        var store = sp.GetRequiredService<IMultiTenantStore<TenantInfo>>();
        Assert.IsType<InMemoryStore<TenantInfo>>(store);

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
    public void AddDelegateStrategy()
    {
        var services = new ServiceCollection();
        var builder = new FinbuckleMultiTenantBuilder<TenantInfo>(services);
        builder.WithDelegateStrategy(c => Task.FromResult("Hi"));
        var sp = services.BuildServiceProvider();

        var strategy = sp.GetRequiredService<IMultiTenantStrategy>();
        Assert.IsType<DelegateStrategy>(strategy);
    }

    [Fact]
    public void AddStaticStrategy()
    {
        var services = new ServiceCollection();
        var builder = new FinbuckleMultiTenantBuilder<TenantInfo>(services);
        builder.WithStaticStrategy("initech");
        var sp = services.BuildServiceProvider();

        var strategy = sp.GetRequiredService<IMultiTenantStrategy>();
        Assert.IsType<StaticStrategy>(strategy);
    }

    [Fact]
    public void ThrowIfNullParamAddingStaticStrategy()
    {
        var services = new ServiceCollection();
        var builder = new FinbuckleMultiTenantBuilder<TenantInfo>(services);
        Assert.Throws<ArgumentException>(()
            => builder.WithStaticStrategy(null));
    }
}