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

using System;
using Finbuckle.MultiTenant;
using Finbuckle.MultiTenant.Core;
using Finbuckle.MultiTenant.Options;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Xunit;

public class MultiTenantOptionsFactoryShould
{
    [Theory]
    [InlineData("")]
    [InlineData(null)]
    [InlineData("name")]
    public void CreateOptionsWithTenantAction(string name)
    {
        var services = new ServiceCollection();
        services.AddOptions();
        services.Configure<TestOptions>(name, o => o.DefaultConnectionString = $"{name}_begin");
        services.PostConfigure<TestOptions>(name, o => o.DefaultConnectionString += "end");
        services.AddMultiTenant<TenantInfo>().WithPerTenantOptions<TestOptions>((o, _ti) => o.DefaultConnectionString += $"_{_ti.Id}_");
        var sp = services.BuildServiceProvider();
        var accessor = sp.GetRequiredService<IMultiTenantContextAccessor<TenantInfo>>();
        accessor.MultiTenantContext = new MultiTenantContext<TenantInfo> { TenantInfo = new TenantInfo { Id = "test-id-123" } };

        var options = sp.GetRequiredService<IOptionsSnapshot<TestOptions>>().Get(name);
        Assert.Equal($"{name}_begin_{accessor.MultiTenantContext.TenantInfo.Id}_end", options.DefaultConnectionString);
    }

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    [InlineData("name")]
    public void CreateMultipelOptionsWithTenantAction(string name)
    {
        var services = new ServiceCollection();
        services.AddOptions();
        services.Configure<TestOptions>(name, o => o.DefaultConnectionString = $"{name}_begin");
        services.PostConfigure<TestOptions>(name, o => o.DefaultConnectionString += "end");
        services.AddMultiTenant<TenantInfo>()
                .WithPerTenantOptions<TestOptions>((o, _ti) => o.DefaultConnectionString += $"_{_ti.Id}")
                .WithPerTenantOptions<TestOptions>((o, _ti) => o.DefaultConnectionString += $"_{_ti.Identifier}_");
        var sp = services.BuildServiceProvider();
        var accessor = sp.GetRequiredService<IMultiTenantContextAccessor<TenantInfo>>();
        accessor.MultiTenantContext = new MultiTenantContext<TenantInfo> { TenantInfo = new TenantInfo { Id = "id", Identifier = "identifier" } };

        var options = sp.GetRequiredService<IOptionsSnapshot<TestOptions>>().Get(name);
        Assert.Equal($"{name}_begin_{accessor.MultiTenantContext.TenantInfo.Id}_{accessor.MultiTenantContext.TenantInfo.Identifier}_end", options.DefaultConnectionString);
    }

    [Fact]
    public void IgnoreNullTenantInfo()
    {
        var services = new ServiceCollection();
        services.Configure<TestOptions>(o => o.DefaultConnectionString = "begin");
        services.PostConfigure<TestOptions>(o => o.DefaultConnectionString += "end");
        services.AddMultiTenant<TenantInfo>().WithPerTenantOptions<TestOptions>((o, _ti) => o.DefaultConnectionString += $"_{_ti.Id}_");
        var sp = services.BuildServiceProvider();
        var accessor = sp.GetRequiredService<IMultiTenantContextAccessor<TenantInfo>>();
        accessor.MultiTenantContext = new MultiTenantContext<TenantInfo>();

        var options = sp.GetRequiredService<IOptionsSnapshot<TestOptions>>().Value;
        Assert.Equal($"beginend", options.DefaultConnectionString);
    }

    [Fact]
    public void IgnoreNullMultiTenantContext()
    {
        var services = new ServiceCollection();
        services.Configure<TestOptions>(o => o.DefaultConnectionString = "begin");
        services.PostConfigure<TestOptions>(o => o.DefaultConnectionString += "end");
        services.AddMultiTenant<TenantInfo>().WithPerTenantOptions<TestOptions>((o, _ti) => o.DefaultConnectionString += $"_{_ti.Id}_");
        var sp = services.BuildServiceProvider();
        var accessor = sp.GetRequiredService<IMultiTenantContextAccessor<TenantInfo>>();
        accessor.MultiTenantContext = null;

        var options = sp.GetRequiredService<IOptionsSnapshot<TestOptions>>().Value;
        Assert.Equal($"beginend", options.DefaultConnectionString);
    }
}