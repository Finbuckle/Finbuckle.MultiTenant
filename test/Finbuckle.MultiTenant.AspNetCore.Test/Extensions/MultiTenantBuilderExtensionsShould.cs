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
using Finbuckle.MultiTenant.AspNetCore;
using Microsoft.Extensions.Options;
using Microsoft.AspNetCore.Authentication.Cookies;
using System.Linq;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;

public partial class MultiTenantBuilderExtensionsShould
{
    [Fact]
    public void ConfigurePerTenantAuthentication_RegisterServices()
    {
        var services = new ServiceCollection();
        services.AddAuthentication();
        services.AddLogging();
        services.AddMultiTenant<TestTenantInfo>()
                .WithPerTenantAuthentication();

        var sp = services.BuildServiceProvider();

        var authService = sp.GetRequiredService<IAuthenticationService>(); // Throws if fail
        Assert.IsType<MultiTenantAuthenticationService<TestTenantInfo>>(authService);

        var schemeProvider = sp.GetRequiredService<IAuthenticationSchemeProvider>(); // Throws if fails
        Assert.IsType<MultiTenantAuthenticationSchemeProvider>(schemeProvider);

        var strategy = sp.GetServices<IMultiTenantStrategy>().Where(s => s.GetType() == typeof(RemoteAuthenticationCallbackStrategy)).Single();
        Assert.NotNull(strategy);
    }

    [Fact]
    public void ConfigurePerTenantAuthentication_UseChallengeScheme()
    {
        var services = new ServiceCollection();
        services.AddOptions();
        services.AddAuthentication().AddCookie().AddOpenIdConnect();
        services.AddMultiTenant<TestTenantInfo>()
                .WithPerTenantAuthentication();
        var sp = services.BuildServiceProvider();

        var ti1 = new TestTenantInfo
        {
            Id = "id1",
            Identifier = "identifier1",
            ChallengeScheme = OpenIdConnectDefaults.AuthenticationScheme
        };

        var accessor = sp.GetRequiredService<IMultiTenantContextAccessor<TestTenantInfo>>();
        accessor.MultiTenantContext = new MultiTenantContext<TestTenantInfo> { TenantInfo = ti1 };

        var options = sp.GetRequiredService<IAuthenticationSchemeProvider>();
        Assert.Equal(ti1.ChallengeScheme, options.GetDefaultChallengeSchemeAsync().Result.Name);
    }

    [Fact]
    public void ConfigurePerTenantAuthentication_UseOpenIdConnectConvention()
    {
        var services = new ServiceCollection();
        services.AddOptions();
        services.AddAuthentication().AddOpenIdConnect();
        services.AddMultiTenant<TestTenantInfo>()
                .WithPerTenantAuthentication();
        var sp = services.BuildServiceProvider();

        var ti1 = new TestTenantInfo
        {
            Id = "id1",
            Identifier = "identifier1",
            OpenIdConnectAuthority = "https://tenant",
            OpenIdConnectClientId = "tenant",
            OpenIdConnectClientSecret = "secret"
        };

        var accessor = sp.GetRequiredService<IMultiTenantContextAccessor<TestTenantInfo>>();
        accessor.MultiTenantContext = new MultiTenantContext<TestTenantInfo> { TenantInfo = ti1 };

        var options = sp.GetRequiredService<IOptionsSnapshot<OpenIdConnectOptions>>().Get(OpenIdConnectDefaults.AuthenticationScheme);

        Assert.Equal(ti1.OpenIdConnectAuthority, options.Authority);
        Assert.Equal(ti1.OpenIdConnectClientId, options.ClientId);
        Assert.Equal(ti1.OpenIdConnectClientSecret, options.ClientSecret);
    }

    [Fact]
    public void ConfigurePerTenantAuthentication_UseCookieOptionsConvention()
    {
        var services = new ServiceCollection();
        services.AddOptions();
        services.AddAuthentication().AddCookie();
        services.AddMultiTenant<TestTenantInfo>()
                .WithPerTenantAuthentication();
        var sp = services.BuildServiceProvider();

        var ti1 = new TestTenantInfo
        {
            Id = "id1",
            Identifier = "identifier1",
            CookieLoginPath = "/path1",
            CookieLogoutPath = "/path2",
            CookieAccessDeniedPath = "/path3"
        };

        var accessor = sp.GetRequiredService<IMultiTenantContextAccessor<TestTenantInfo>>();
        accessor.MultiTenantContext = new MultiTenantContext<TestTenantInfo> { TenantInfo = ti1 };

        var options = sp.GetRequiredService<IOptionsSnapshot<CookieAuthenticationOptions>>().Get(CookieAuthenticationDefaults.AuthenticationScheme);

        Assert.Equal(ti1.CookieLoginPath, options.LoginPath);
        Assert.Equal(ti1.CookieLogoutPath, options.LogoutPath);
        Assert.Equal(ti1.CookieAccessDeniedPath, options.AccessDeniedPath);
    }

    [Fact]
    public void ThrowIfCantDecorateIAuthenticationService()
    {
        var services = new ServiceCollection();
        var builder = new FinbuckleMultiTenantBuilder<TenantInfo>(services);

        Assert.Throws<MultiTenantException>(() => builder.WithPerTenantAuthentication());
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