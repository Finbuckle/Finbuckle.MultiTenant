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
using System.Collections;
using System.Collections.Concurrent;
using System.Reflection;
using System.Threading.Tasks;
using Finbuckle.MultiTenant;
using Finbuckle.MultiTenant.AspNetCore;
using Finbuckle.MultiTenant.Core;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Moq;
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
        services.Configure<CookieAuthenticationOptions>(name, o => o.Cookie.Name = $"{name}_begin");
        services.PostConfigure<CookieAuthenticationOptions>(name, o => o.Cookie.Name += "end");
        var sp = services.BuildServiceProvider();

        Action<CookieAuthenticationOptions, TenantInfo> tenantConfig = (o, _ti) => o.Cookie.Name += $"_{_ti.Id}_";
        
        var factory = ActivatorUtilities.
            CreateInstance<MultiTenantOptionsFactory<CookieAuthenticationOptions>>(sp, new [] { tenantConfig });

        var options = factory.Create(name);
        Assert.Equal($"{name}_begin_{ti.Id}_end", options.Cookie.Name);
    }

    [Fact]
    public void IgnoreNullTenantInfo()
    {
        var tca = new TestMultiTenantContextAccessor(new MultiTenantContext());

        var services = new ServiceCollection();
        services.AddTransient<IMultiTenantContextAccessor>(_sp => tca);
        services.Configure<CookieAuthenticationOptions>(o => o.Cookie.Name = "begin");
        services.PostConfigure<CookieAuthenticationOptions>(o => o.Cookie.Name += "end");
        var sp = services.BuildServiceProvider();

        Action<CookieAuthenticationOptions, TenantInfo> tenantConfig = (o, _ti) => o.Cookie.Name += $"_{_ti.Id}_";
        
        var factory = ActivatorUtilities.
            CreateInstance<MultiTenantOptionsFactory<CookieAuthenticationOptions>>(sp, new [] { tenantConfig });

        var options = factory.Create("");
        Assert.Equal($"beginend", options.Cookie.Name);
    }

    [Fact]
    public void IgnoreNullMultiTenantContext()
    {
        var tca = new TestMultiTenantContextAccessor(null);

        var services = new ServiceCollection();
        services.AddTransient<IMultiTenantContextAccessor>(_sp => tca);
        services.Configure<CookieAuthenticationOptions>(o => o.Cookie.Name = "begin");
        services.PostConfigure<CookieAuthenticationOptions>(o => o.Cookie.Name += "end");
        var sp = services.BuildServiceProvider();

        Action<CookieAuthenticationOptions, TenantInfo> tenantConfig = (o, _ti) => o.Cookie.Name += $"_{_ti.Id}_";
        
        var factory = ActivatorUtilities.
            CreateInstance<MultiTenantOptionsFactory<CookieAuthenticationOptions>>(sp, new [] { tenantConfig });

        var options = factory.Create("");
        Assert.Equal($"beginend", options.Cookie.Name);
    }
}