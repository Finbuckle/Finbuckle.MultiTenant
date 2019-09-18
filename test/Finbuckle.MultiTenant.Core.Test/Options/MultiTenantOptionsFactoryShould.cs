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
using Finbuckle.MultiTenant;
using Finbuckle.MultiTenant.Options;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

public class MultiTenantOptionsFactoryShould
{
    [Theory]
    [InlineData("")]
    [InlineData(null)]
    [InlineData("name")]
    public void CreateOptionsWithTenantAction(string name)
    {
        var ti = new TenantInfo("test-id-123", null, null, null, null);
        var tc = new MultiTenantContext();
        tc.TenantInfo = ti;
        var tca = new TestMultiTenantContextAccessor(tc);

        var services = new ServiceCollection();
        services.AddTransient<IMultiTenantContextAccessor>(_sp => tca);
        services.Configure<InMemoryStoreOptions>(name, o => o.DefaultConnectionString = $"{name}_begin");
        services.PostConfigure<InMemoryStoreOptions>(name, o => o.DefaultConnectionString += "end");
        var sp = services.BuildServiceProvider();

        Action<InMemoryStoreOptions, TenantInfo> tenantConfig = (o, _ti) => o.DefaultConnectionString += $"_{_ti.Id}_";
        
        var factory = ActivatorUtilities.
            CreateInstance<MultiTenantOptionsFactory<InMemoryStoreOptions>>(sp, new [] { tenantConfig });

        var options = factory.Create(name);
        Assert.Equal($"{name}_begin_{ti.Id}_end", options.DefaultConnectionString);
    }

    [Fact]
    public void IgnoreNullTenantInfo()
    {
        var tca = new TestMultiTenantContextAccessor(new MultiTenantContext());

        var services = new ServiceCollection();
        services.AddTransient<IMultiTenantContextAccessor>(_sp => tca);
        services.Configure<InMemoryStoreOptions>(o => o.DefaultConnectionString = "begin");
        services.PostConfigure<InMemoryStoreOptions>(o => o.DefaultConnectionString += "end");
        var sp = services.BuildServiceProvider();

        Action<InMemoryStoreOptions, TenantInfo> tenantConfig = (o, _ti) => o.DefaultConnectionString += $"_{_ti.Id}_";
        
        var factory = ActivatorUtilities.
            CreateInstance<MultiTenantOptionsFactory<InMemoryStoreOptions>>(sp, new [] { tenantConfig });

        var options = factory.Create("");
        Assert.Equal($"beginend", options.DefaultConnectionString);
    }

    [Fact]
    public void IgnoreNullMultiTenantContext()
    {
        var tca = new TestMultiTenantContextAccessor(null);

        var services = new ServiceCollection();
        services.AddTransient<IMultiTenantContextAccessor>(_sp => tca);
        services.Configure<InMemoryStoreOptions>(o => o.DefaultConnectionString = "begin");
        services.PostConfigure<InMemoryStoreOptions>(o => o.DefaultConnectionString += "end");
        var sp = services.BuildServiceProvider();

        Action<InMemoryStoreOptions, TenantInfo> tenantConfig = (o, _ti) => o.DefaultConnectionString += $"_{_ti.Id}_";
        
        var factory = ActivatorUtilities.
            CreateInstance<MultiTenantOptionsFactory<InMemoryStoreOptions>>(sp, new [] { tenantConfig });

        var options = factory.Create("");
        Assert.Equal($"beginend", options.DefaultConnectionString);
    }
}