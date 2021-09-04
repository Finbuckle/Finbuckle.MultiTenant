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
using System.Collections.Generic;
using Microsoft.Extensions.DependencyInjection;
using Xunit;
using Finbuckle.MultiTenant;
using Finbuckle.MultiTenant.Strategies;
using Microsoft.AspNetCore.Authentication;
using Finbuckle.MultiTenant.AspNetCore;
using Microsoft.Extensions.Options;
using Microsoft.AspNetCore.Authentication.Cookies;
using System.Linq;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Http;
using Moq;
using Constants = Finbuckle.MultiTenant.Internal.Constants;

public class MultiTenantBuilderExtensionsShould
{
    public class TestTenantInfo : ITenantInfo
    {
        public string Id { get; set; }
        public string Identifier { get; set; }
        public string Name { get; set; }
        public string ConnectionString { get; set; }
        public string ChallengeScheme { get; set; }
        public string CookiePath { get; set; }
        public string CookieLoginPath { get; set; }
        public string CookieLogoutPath { get; set; }
        public string CookieAccessDeniedPath { get; set; }
        public string OpenIdConnectAuthority { get; set; }
        public string OpenIdConnectClientId { get; set; }
        public string OpenIdConnectClientSecret { get; set; }
    }
    
    [Fact]
    public void NotThrowIfOriginalPrincipalValidationNotSet()
    {
        var services = new ServiceCollection();
        services.AddLogging();
        var called = false;
        services.AddAuthentication().AddCookie();
        services.AddMultiTenant<TenantInfo>()
            .WithPerTenantAuthentication();
        var sp = services.BuildServiceProvider();

        // Fake a resolved tenant
        var mtc = new MultiTenantContext<TenantInfo>();
        mtc.TenantInfo = new TenantInfo { Identifier = "abc" };
        sp.GetRequiredService<IMultiTenantContextAccessor<TenantInfo>>().MultiTenantContext = mtc;
        
        // Trigger the ValidatePrincipal event
        var httpContextMock = new Mock<HttpContext>();
        httpContextMock.Setup(c => c.RequestServices).Returns(sp);
        httpContextMock.Setup(c => c.Items).Returns(new Dictionary<object, object>());
        var scheme = sp.GetRequiredService<IAuthenticationSchemeProvider>()
            .GetSchemeAsync(CookieAuthenticationDefaults.AuthenticationScheme).Result;
        var options = sp.GetRequiredService<IOptionsMonitor<CookieAuthenticationOptions>>().Get(CookieAuthenticationDefaults.AuthenticationScheme);
        var principal = new ClaimsPrincipal(new ClaimsIdentity());
        var authTicket = new AuthenticationTicket(principal, CookieAuthenticationDefaults.AuthenticationScheme);
        authTicket.Properties.Items[Constants.TenantToken] = "abc";
        var cookieValidationContext =
            new CookieValidatePrincipalContext(httpContextMock.Object, scheme, options, authTicket);

        options.Events.ValidatePrincipal(cookieValidationContext).Wait();

        Assert.True(true);
    }

    [Fact]
    public void CallOriginalPrincipalValidation()
    {
        var services = new ServiceCollection();
        services.AddLogging();
        var called = false;
        services.AddAuthentication().AddCookie(options =>
        {
#pragma warning disable 1998
            options.Events.OnValidatePrincipal = async context =>
#pragma warning restore 1998
            {
                called = true;
            };
        });
        services.AddMultiTenant<TenantInfo>()
            .WithPerTenantAuthentication();
        var sp = services.BuildServiceProvider();
        

        // Fake a resolved tenant
        var mtc = new MultiTenantContext<TenantInfo>();
        mtc.TenantInfo = new TenantInfo { Identifier = "abc" };
        sp.GetRequiredService<IMultiTenantContextAccessor<TenantInfo>>().MultiTenantContext = mtc;
        
        // Trigger the ValidatePrincipal event
        var httpContextMock = new Mock<HttpContext>();
        httpContextMock.Setup(c => c.RequestServices).Returns(sp);
        httpContextMock.Setup(c => c.Items).Returns(new Dictionary<object, object>());
        var scheme = sp.GetRequiredService<IAuthenticationSchemeProvider>()
            .GetSchemeAsync(CookieAuthenticationDefaults.AuthenticationScheme).Result;
        var options = sp.GetRequiredService<IOptionsMonitor<CookieAuthenticationOptions>>().Get(CookieAuthenticationDefaults.AuthenticationScheme);
        var principal = new ClaimsPrincipal(new ClaimsIdentity());
        var authTicket = new AuthenticationTicket(principal, CookieAuthenticationDefaults.AuthenticationScheme);
        authTicket.Properties.Items[Constants.TenantToken] = "abc";
        var cookieValidationContext =
            new CookieValidatePrincipalContext(httpContextMock.Object, scheme, options, authTicket);

        options.Events.ValidatePrincipal(cookieValidationContext).Wait();

        Assert.True(called);
    }
    
    [Fact]
    public void PassPrincipleValidationIfTenantMatch()
    {
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddAuthentication().AddCookie();
        services.AddMultiTenant<TenantInfo>()
            .WithPerTenantAuthentication();
        var sp = services.BuildServiceProvider();
        

        // Fake a resolved tenant
        var mtc = new MultiTenantContext<TenantInfo>();
        mtc.TenantInfo = new TenantInfo { Identifier = "abc" };
        sp.GetRequiredService<IMultiTenantContextAccessor<TenantInfo>>().MultiTenantContext = mtc;
        
        // Trigger the ValidatePrincipal event
        var httpContextMock = new Mock<HttpContext>();
        httpContextMock.Setup(c => c.RequestServices).Returns(sp);
        httpContextMock.Setup(c => c.Items).Returns(new Dictionary<object, object>());
        var scheme = sp.GetRequiredService<IAuthenticationSchemeProvider>()
            .GetSchemeAsync(CookieAuthenticationDefaults.AuthenticationScheme).Result;
        var options = sp.GetRequiredService<IOptionsMonitor<CookieAuthenticationOptions>>().Get(CookieAuthenticationDefaults.AuthenticationScheme);
        var principal = new ClaimsPrincipal(new ClaimsIdentity());
        var authTicket = new AuthenticationTicket(principal, CookieAuthenticationDefaults.AuthenticationScheme);
        authTicket.Properties.Items[Constants.TenantToken] = "abc";
        var cookieValidationContext =
            new CookieValidatePrincipalContext(httpContextMock.Object, scheme, options, authTicket);

        options.Events.ValidatePrincipal(cookieValidationContext).Wait();

        Assert.NotNull(cookieValidationContext);
    }
    
    [Fact]
    public void SkipPrincipleValidationIfBypassSet()
    {
        var services = new ServiceCollection();
        services.AddLogging();
        var called = false;
#pragma warning disable 1998
        services.AddAuthentication().AddCookie(o => o.Events.OnValidatePrincipal = async c => called = true);
#pragma warning restore 1998
        services.AddMultiTenant<TenantInfo>()
            .WithPerTenantAuthentication();
        var sp = services.BuildServiceProvider();
        
        // Fake a resolved tenant
        var mtc = new MultiTenantContext<TenantInfo>();
        mtc.TenantInfo = new TenantInfo { Identifier = "abc1" };
        sp.GetRequiredService<IMultiTenantContextAccessor<TenantInfo>>().MultiTenantContext = mtc;
        
        // Trigger the ValidatePrincipal event
        var httpContextMock = new Mock<HttpContext>();
        httpContextMock.Setup(c => c.RequestServices).Returns(sp);
        var httpContextItems = new Dictionary<object, object>();
        httpContextItems[$"{Constants.TenantToken}__bypass_validate_principle__"] = true;
        httpContextMock.Setup(c => c.Items).Returns(httpContextItems);
        var scheme = sp.GetRequiredService<IAuthenticationSchemeProvider>()
            .GetSchemeAsync(CookieAuthenticationDefaults.AuthenticationScheme).Result;
        var options = sp.GetRequiredService<IOptionsMonitor<CookieAuthenticationOptions>>().Get(CookieAuthenticationDefaults.AuthenticationScheme);
        var principal = new ClaimsPrincipal(new ClaimsIdentity());
        var authTicket = new AuthenticationTicket(principal, CookieAuthenticationDefaults.AuthenticationScheme);
        authTicket.Properties.Items[Constants.TenantToken] = "abc2";
        var cookieValidationContext =
            new CookieValidatePrincipalContext(httpContextMock.Object, scheme, options, authTicket);

        options.Events.ValidatePrincipal(cookieValidationContext).Wait();

        Assert.NotNull(cookieValidationContext.Principal);
        Assert.False(called);
    }
    
    [Fact]
    public void RejectPrincipleValidationIfTenantMatch()
    {
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddAuthentication().AddCookie();
        services.AddMultiTenant<TenantInfo>()
            .WithPerTenantAuthentication();
        var sp = services.BuildServiceProvider();
        

        // Fake a resolved tenant
        var mtc = new MultiTenantContext<TenantInfo>();
        mtc.TenantInfo = new TenantInfo { Identifier = "abc1" };
        sp.GetRequiredService<IMultiTenantContextAccessor<TenantInfo>>().MultiTenantContext = mtc;
        
        // Trigger the ValidatePrincipal event
        var httpContextMock = new Mock<HttpContext>();
        httpContextMock.Setup(c => c.RequestServices).Returns(sp);
        httpContextMock.Setup(c => c.Items).Returns(new Dictionary<object, object>());
        var scheme = sp.GetRequiredService<IAuthenticationSchemeProvider>()
            .GetSchemeAsync(CookieAuthenticationDefaults.AuthenticationScheme).Result;
        var options = sp.GetRequiredService<IOptionsMonitor<CookieAuthenticationOptions>>().Get(CookieAuthenticationDefaults.AuthenticationScheme);
        var principal = new ClaimsPrincipal(new ClaimsIdentity());
        var authTicket = new AuthenticationTicket(principal, CookieAuthenticationDefaults.AuthenticationScheme);
        authTicket.Properties.Items[Constants.TenantToken] = "abc2";
        var cookieValidationContext =
            new CookieValidatePrincipalContext(httpContextMock.Object, scheme, options, authTicket);

        options.Events.ValidatePrincipal(cookieValidationContext).Wait();

        Assert.Null(cookieValidationContext.Principal);
    }
    
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
    public void ConfigurePerTenantAuthenticationCore_RegisterServices()
    {
        var services = new ServiceCollection();
        services.AddAuthentication();
        services.AddLogging();
        services.AddMultiTenant<TestTenantInfo>()
            .WithPerTenantAuthenticationCore();

        var sp = services.BuildServiceProvider();

        var authService = sp.GetRequiredService<IAuthenticationService>(); // Throws if fail
        Assert.IsType<MultiTenantAuthenticationService<TestTenantInfo>>(authService);

        var schemeProvider = sp.GetRequiredService<IAuthenticationSchemeProvider>(); // Throws if fails
        Assert.IsType<MultiTenantAuthenticationSchemeProvider>(schemeProvider);
    }
    
    [Fact]
    public void AddRemoteAuthenticationCallbackStrategy()
    {
        var services = new ServiceCollection();
        services.AddAuthentication();
        services.AddLogging();
        services.AddMultiTenant<TestTenantInfo>()
            .WithRemoteAuthenticationCallbackStrategy();
        var sp = services.BuildServiceProvider();
        
        var strategy = sp.GetServices<IMultiTenantStrategy>().Where(s => s.GetType() == typeof(RemoteAuthenticationCallbackStrategy)).Single();
        Assert.NotNull(strategy);
    }

    [Fact]
    public void ConfigurePerTenantAuthentication_UseChallengeScheme()
    {
        var services = new ServiceCollection();
        services.AddOptions();
        services.AddAuthentication().AddCookie().AddOpenIdConnect("customScheme", null);
        services.AddMultiTenant<TestTenantInfo>()
                .WithPerTenantAuthentication();
        var sp = services.BuildServiceProvider();

        var ti1 = new TestTenantInfo
        {
            Id = "id1",
            Identifier = "identifier1",
            ChallengeScheme = "customScheme"
        };

        var accessor = sp.GetRequiredService<IMultiTenantContextAccessor<TestTenantInfo>>();
        accessor.MultiTenantContext = new MultiTenantContext<TestTenantInfo> { TenantInfo = ti1 };

        var options = sp.GetRequiredService<IAuthenticationSchemeProvider>();
        Assert.Equal(ti1.ChallengeScheme, options.GetDefaultChallengeSchemeAsync().Result.Name);
    }
    
    [Fact]
    public void ConfigurePerTenantAuthenticationConventions_UseChallengeScheme()
    {
        var services = new ServiceCollection();
        services.AddOptions();
        services.AddAuthentication().AddCookie().AddOpenIdConnect("customScheme", null);
        services.AddMultiTenant<TestTenantInfo>()
            .WithPerTenantAuthenticationConventions();
        var sp = services.BuildServiceProvider();

        var ti1 = new TestTenantInfo
        {
            Id = "id1",
            Identifier = "identifier1",
            ChallengeScheme = "customScheme"
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
    public void ConfigurePerTenantAuthenticationConventions_UseOpenIdConnectConvention()
    {
        var services = new ServiceCollection();
        services.AddOptions();
        services.AddAuthentication().AddOpenIdConnect();
        services.AddMultiTenant<TestTenantInfo>()
            .WithPerTenantAuthenticationConventions();
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
    public void ConfigurePerTenantAuthenticationConventions_UseCookieOptionsConvention()
    {
        var services = new ServiceCollection();
        services.AddOptions();
        services.AddAuthentication().AddCookie();
        services.AddMultiTenant<TestTenantInfo>()
            .WithPerTenantAuthenticationConventions();
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
    public void WithPerTenantAuthentication_ThrowIfCantDecorateIAuthenticationService()
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
    public void AddClaimStrategy()
    {
        var services = new ServiceCollection();
        var builder = new FinbuckleMultiTenantBuilder<TenantInfo>(services);
        builder.WithClaimStrategy();
        var sp = services.BuildServiceProvider();

        var strategy = sp.GetRequiredService<IMultiTenantStrategy>();
        Assert.IsType<ClaimStrategy>(strategy);
    }

    [Fact]
    public void AddHeaderStrategy()
    {
        var services = new ServiceCollection();
        var builder = new FinbuckleMultiTenantBuilder<TenantInfo>(services);
        builder.WithHeaderStrategy();
        var sp = services.BuildServiceProvider();

        var strategy = sp.GetRequiredService<IMultiTenantStrategy>();
        Assert.IsType<HeaderStrategy>(strategy);
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
}