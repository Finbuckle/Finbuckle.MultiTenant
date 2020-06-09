//    Copyright 2018-2020 Andrew White
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
using Finbuckle.MultiTenant.Core;
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
        var ti = new TenantInfo{ Id = "test-id-123" };
        var tc = new MultiTenantContext<TenantInfo>();
        tc.TenantInfo = ti;
        var tca = new MultiTenantContextAccessor<TenantInfo>();
        tca.MultiTenantContext = tc;

        var services = new ServiceCollection();
        services.AddTransient<IMultiTenantContextAccessor<TenantInfo>>(_sp => tca);
        services.Configure<TestOptions>(name, o => o.DefaultConnectionString = $"{name}_begin");
        services.PostConfigure<TestOptions>(name, o => o.DefaultConnectionString += "end");
        var sp = services.BuildServiceProvider();

        Action<TestOptions, TenantInfo> tenantConfig = (o, _ti) => o.DefaultConnectionString += $"_{_ti.Id}_";
        
        var factory = ActivatorUtilities.
            CreateInstance<MultiTenantOptionsFactory<TestOptions, TenantInfo>>(sp, new [] { tenantConfig });

        var options = factory.Create(name);
        Assert.Equal($"{name}_begin_{ti.Id}_end", options.DefaultConnectionString);
    }

    [Fact]
    public void IgnoreNullTenantInfo()
    {
        var tca = new MultiTenantContextAccessor<TenantInfo>();
        tca.MultiTenantContext = new MultiTenantContext<TenantInfo>();

        var services = new ServiceCollection();
        services.AddTransient<IMultiTenantContextAccessor<TenantInfo>>(_sp => tca);
        services.Configure<TestOptions>(o => o.DefaultConnectionString = "begin");
        services.PostConfigure<TestOptions>(o => o.DefaultConnectionString += "end");
        var sp = services.BuildServiceProvider();

        Action<TestOptions, TenantInfo> tenantConfig = (o, _ti) => o.DefaultConnectionString += $"_{_ti.Id}_";
        
        var factory = ActivatorUtilities.
            CreateInstance<MultiTenantOptionsFactory<TestOptions, TenantInfo>>(sp, new [] { tenantConfig });

        var options = factory.Create("");
        Assert.Equal($"beginend", options.DefaultConnectionString);
    }

    [Fact]
    public void IgnoreNullMultiTenantContext()
    {
        var tca = new MultiTenantContextAccessor<TenantInfo>();

        var services = new ServiceCollection();
        services.AddTransient<IMultiTenantContextAccessor<TenantInfo>>(_sp => tca);
        services.Configure<TestOptions>(o => o.DefaultConnectionString = "begin");
        services.PostConfigure<TestOptions>(o => o.DefaultConnectionString += "end");
        var sp = services.BuildServiceProvider();

        Action<TestOptions, TenantInfo> tenantConfig = (o, _ti) => o.DefaultConnectionString += $"_{_ti.Id}_";
        
        var factory = ActivatorUtilities.
            CreateInstance<MultiTenantOptionsFactory<TestOptions, TenantInfo>>(sp, new [] { tenantConfig });

        var options = factory.Create("");
        Assert.Equal($"beginend", options.DefaultConnectionString);
    }
}