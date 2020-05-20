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
using Microsoft.Extensions.DependencyInjection;
using Xunit;
using Finbuckle.MultiTenant;
using Finbuckle.MultiTenant.Strategies;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Builder;
using Moq;
using Microsoft.AspNetCore.Mvc.Infrastructure;

public class MultiTenantBuilderExtensionsShould
{
    [Fact]
    public void AddRemoteAuthenticationServices()
    {
        var services = new ServiceCollection();
        var builder = new FinbuckleMultiTenantBuilder<TenantInfo>(services);
        services.AddAuthentication();
        builder.WithRemoteAuthenticationCallbackStrategy();
        var sp = services.BuildServiceProvider();

        var authService = sp.GetRequiredService<IAuthenticationService>(); // Throws if fail
        var schemeProvider = sp.GetRequiredService<IAuthenticationSchemeProvider>(); // Throws if fails
    }

    [Fact]
    public void ThrowIfCantDecorateIAuthenticationService()
    {
        var services = new ServiceCollection();
        var builder = new FinbuckleMultiTenantBuilder<TenantInfo>(services);

        Assert.Throws<MultiTenantException>(() => builder.WithRemoteAuthenticationCallbackStrategy());
    }

    [Fact]
    public void AddBasePathStrategy()
    {
        var services = new ServiceCollection();
        var builder = new FinbuckleMultiTenantBuilder<TenantInfo>(services);
        builder.WithBasePathStrategy();
        var sp = services.BuildServiceProvider();

        var strategy = sp.GetRequiredService<IMultiTenantStrategy>();
        Assert.IsType<BasePathStrategy>(strategy);
    }

    [Fact]
    public void AddHostStrategy()
    {
        var services = new ServiceCollection();
        var builder = new FinbuckleMultiTenantBuilder<TenantInfo>(services);
        builder.WithHostStrategy();
        var sp = services.BuildServiceProvider();

        var strategy = sp.GetRequiredService<IMultiTenantStrategy>();
        Assert.IsType<HostStrategy>(strategy);
    }

    [Fact]
    public void ThrowIfNullParamAddingHostStrategy()
    {
        var services = new ServiceCollection();
        var builder = new FinbuckleMultiTenantBuilder<TenantInfo>(services);
        Assert.Throws<ArgumentException>(()
            => builder.WithHostStrategy(null));
    }

#if NETCOREAPP2_1
    [Fact]
    public void AddRouteStrategy()
    {
        var services = new ServiceCollection();
        var adcp = new Mock<IActionDescriptorCollectionProvider>().Object;
        services.AddSingleton<IActionDescriptorCollectionProvider>(adcp);
        var builder = new FinbuckleMultiTenantBuilder<TenantInfo>(services);
        builder.WithRouteStrategy("routeParam", cr => cr.MapRoute("test", "test"));
        var sp = services.BuildServiceProvider();

        var strategy = sp.GetRequiredService<IMultiTenantStrategy>();
        Assert.IsType<RouteStrategy>(strategy);
    }

    [Fact]
    public void ThrowIfNullParamAddingRouteStrategy()
    {
        var services = new ServiceCollection();
        var builder = new FinbuckleMultiTenantBuilder<TenantInfo>(services);
        Assert.Throws<ArgumentException>(()
            => builder.WithRouteStrategy(null, rb => rb.GetType()));
    }

    [Fact]
    public void ThrowIfNullRouteConfigAddingRouteStrategy()
    {
        var services = new ServiceCollection();
        var builder = new FinbuckleMultiTenantBuilder<TenantInfo>(services);
        Assert.Throws<ArgumentNullException>(()
            => builder.WithRouteStrategy(null));
        Assert.Throws<ArgumentNullException>(()
            => builder.WithRouteStrategy("param", null));
    }

#else
    [Fact]
    public void AddRouteStrategy()
    {
        var services = new ServiceCollection();
        var builder = new FinbuckleMultiTenantBuilder<TenantInfo>(services);
        builder.WithRouteStrategy("routeParam");
        var sp = services.BuildServiceProvider();

        var strategy = sp.GetRequiredService<IMultiTenantStrategy>();
        Assert.IsType<RouteStrategy>(strategy);
    }

    [Fact]
    public void ThrowIfNullParamAddingRouteStrategy()
    {
        var services = new ServiceCollection();
        var builder = new FinbuckleMultiTenantBuilder<TenantInfo>(services);
        Assert.Throws<ArgumentException>(()
            => builder.WithRouteStrategy(null));
    }
#endif
}