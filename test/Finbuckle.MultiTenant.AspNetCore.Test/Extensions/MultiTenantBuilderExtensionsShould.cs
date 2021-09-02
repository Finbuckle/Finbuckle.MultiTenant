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
using System.Net.Mime;
using System.Security.Claims;
using System.Threading.Tasks;
using Castle.DynamicProxy.Internal;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.VisualBasic;
using Moq;
using Constants = Finbuckle.MultiTenant.Internal.Constants;

public partial class MultiTenantBuilderExtensionsShould
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
    public void PreserveTenantClaimAfterAuthenticationPrincipalValidation()
    {
        // see https://github.com/Finbuckle/Finbuckle.MultiTenant/issues/415
        
        var services = new ServiceCollection();
        services.AddAuthentication();
        services.AddLogging();
        services.AddAuthentication().AddCookie(options =>
        {
#pragma warning disable 1998
            options.Events.OnValidatePrincipal = async context =>
#pragma warning restore 1998
            {
                // This is a simple validator that will remove the tenant claim; meant to simulate Identity's security stamp validator
                if (context.Principal != null)
                {
                    var newPrincipal = new ClaimsPrincipal(new ClaimsIdentity());
                    if (newPrincipal.Identity is ClaimsIdentity identity)
                        foreach(var claim in context.Principal.Claims.Where(c => c.Type != Constants.TenantToken))
                            identity.AddClaim(claim);

                    context.ReplacePrincipal(newPrincipal);
                }
            };
        });
        services.AddMultiTenant<TenantInfo>()
            .WithPerTenantAuthentication();
        
        var sp = services.BuildServiceProvider();
        
        // Fake HttpContext
        var httpContextMock = new Mock<HttpContext>();
        httpContextMock.Setup(c => c.RequestServices).Returns(sp);
        httpContextMock.Setup(c => c.Items).Returns(new Dictionary<object, object>());

        // Fake a resolved tenant
        var mtc = new MultiTenantContext<TenantInfo>();
        mtc.TenantInfo = new TenantInfo { Identifier = "abc" };
        sp.GetRequiredService<IMultiTenantContextAccessor<TenantInfo>>().MultiTenantContext = mtc;
        
        // Trigger the ValidatePrincipal event
        var options = sp.GetRequiredService<IOptionsMonitor<CookieAuthenticationOptions>>().Get(CookieAuthenticationDefaults.AuthenticationScheme);

        var principal = new ClaimsPrincipal(new ClaimsIdentity());
        if (principal.Identity is ClaimsIdentity identity)
            identity.AddClaim(new Claim(Constants.TenantToken, "abc"));
        var authTicket = new AuthenticationTicket(principal, CookieAuthenticationDefaults.AuthenticationScheme);
        var scheme = sp.GetRequiredService<IAuthenticationSchemeProvider>()
            .GetSchemeAsync(CookieAuthenticationDefaults.AuthenticationScheme).Result;
        var cookieValidationContext =
            new CookieValidatePrincipalContext(httpContextMock.Object, scheme, options, authTicket);

        options.Events.ValidatePrincipal(cookieValidationContext).Wait();

        Assert.Equal("abc", cookieValidationContext.Principal?.Claims.SingleOrDefault(c => c.Type == Constants.TenantToken)?.Value);
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