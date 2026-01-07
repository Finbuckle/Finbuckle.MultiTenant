// Copyright Finbuckle LLC, Andrew White, and Contributors.
// Refer to the solution LICENSE file for more information.

using System.Security.Claims;
using Finbuckle.MultiTenant.Abstractions;
using Finbuckle.MultiTenant.AspNetCore.Authentication;
using Finbuckle.MultiTenant.AspNetCore.Extensions;
using Finbuckle.MultiTenant.AspNetCore.Options;
using Finbuckle.MultiTenant.AspNetCore.Strategies;
using Finbuckle.MultiTenant.Extensions;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Moq;
using Xunit;

namespace Finbuckle.MultiTenant.AspNetCore.Test.Extensions;

public class MultiTenantBuilderExtensionsShould
{
    [Fact]
    public async Task NotThrowIfOriginalPrincipalValidationNotSet()
    {
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddAuthentication().AddCookie();
        services.AddMultiTenant<TenantInfo>()
            .WithPerTenantAuthentication();
        var sp = services.BuildServiceProvider();

        // Fake a resolved tenant
        var mtc = new MultiTenantContext<TenantInfo>(new TenantInfo { Id = "", Identifier = "abc" });
        sp.GetRequiredService<IMultiTenantContextSetter>().MultiTenantContext = mtc;

        // Trigger the ValidatePrincipal event
        var httpContextMock = new Mock<HttpContext>();
        httpContextMock.Setup(c => c.RequestServices).Returns(sp);
        httpContextMock.Setup(c => c.Items).Returns(new Dictionary<object, object?>());
        var scheme = await sp.GetRequiredService<IAuthenticationSchemeProvider>()
            .GetSchemeAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        var options = sp.GetRequiredService<IOptionsMonitor<CookieAuthenticationOptions>>()
            .Get(CookieAuthenticationDefaults.AuthenticationScheme);
        var principal = new ClaimsPrincipal(new ClaimsIdentity());
        var authTicket = new AuthenticationTicket(principal, CookieAuthenticationDefaults.AuthenticationScheme);
        authTicket.Properties.Items[Constants.TenantToken] = "abc";
        var cookieValidationContext =
            new CookieValidatePrincipalContext(httpContextMock.Object, scheme!, options, authTicket);

        await options.Events.ValidatePrincipal(cookieValidationContext);

        Assert.True(true);
    }

    [Fact]
    public async Task CallOriginalPrincipalValidation()
    {
        var services = new ServiceCollection();
        services.AddLogging();
        var called = false;
        services.AddAuthentication().AddCookie(options =>
        {
#pragma warning disable 1998
            options.Events.OnValidatePrincipal = async _ =>
#pragma warning restore 1998
            {
                called = true;
            };
        });
        services.AddMultiTenant<TenantInfo>()
            .WithPerTenantAuthentication();
        var sp = services.BuildServiceProvider();


        // Fake a resolved tenant
        var mtc = new MultiTenantContext<TenantInfo>(new TenantInfo { Id = "", Identifier = "abc" });
        sp.GetRequiredService<IMultiTenantContextSetter>().MultiTenantContext = mtc;

        // Trigger the ValidatePrincipal event
        var httpContextMock = new Mock<HttpContext>();
        httpContextMock.Setup(c => c.RequestServices).Returns(sp);
        httpContextMock.Setup(c => c.Items).Returns(new Dictionary<object, object?>());
        var scheme = await sp.GetRequiredService<IAuthenticationSchemeProvider>()
            .GetSchemeAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        var options = sp.GetRequiredService<IOptionsMonitor<CookieAuthenticationOptions>>()
            .Get(CookieAuthenticationDefaults.AuthenticationScheme);
        var principal = new ClaimsPrincipal(new ClaimsIdentity());
        var authTicket = new AuthenticationTicket(principal, CookieAuthenticationDefaults.AuthenticationScheme);
        authTicket.Properties.Items[Constants.TenantToken] = "abc";
        var cookieValidationContext =
            new CookieValidatePrincipalContext(httpContextMock.Object, scheme!, options, authTicket);

        await options.Events.ValidatePrincipal(cookieValidationContext);

        Assert.True(called);
    }

    [Fact]
    public async Task PassPrincipalValidationIfTenantMatch()
    {
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddAuthentication().AddCookie();
        services.AddMultiTenant<TenantInfo>()
            .WithPerTenantAuthentication();
        var sp = services.BuildServiceProvider();


        // Fake a resolved tenant
        var mtc = new MultiTenantContext<TenantInfo>(new TenantInfo { Id = "", Identifier = "abc" });
        sp.GetRequiredService<IMultiTenantContextSetter>().MultiTenantContext = mtc;

        // Trigger the ValidatePrincipal event
        var httpContextMock = new Mock<HttpContext>();
        httpContextMock.Setup(c => c.RequestServices).Returns(sp);
        httpContextMock.Setup(c => c.Items).Returns(new Dictionary<object, object?>());
        var scheme = await sp.GetRequiredService<IAuthenticationSchemeProvider>()
            .GetSchemeAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        var options = sp.GetRequiredService<IOptionsMonitor<CookieAuthenticationOptions>>()
            .Get(CookieAuthenticationDefaults.AuthenticationScheme);
        var principal = new ClaimsPrincipal(new ClaimsIdentity());
        var authTicket = new AuthenticationTicket(principal, CookieAuthenticationDefaults.AuthenticationScheme);
        authTicket.Properties.Items[Constants.TenantToken] = "abc";
        var cookieValidationContext =
            new CookieValidatePrincipalContext(httpContextMock.Object, scheme!, options, authTicket);

        await options.Events.ValidatePrincipal(cookieValidationContext);

        Assert.NotNull(cookieValidationContext);
    }

    [Fact]
    public async Task SkipPrincipalValidationIfBypassSet_WithPerTenantAuthentication()
    {
        var services = new ServiceCollection();
        services.AddLogging();
        var called = false;
#pragma warning disable 1998
        services.AddAuthentication().AddCookie(o => o.Events.OnValidatePrincipal = async _ => called = true);
#pragma warning restore 1998
        services.AddMultiTenant<TenantInfo>()
            .WithPerTenantAuthentication();
        var sp = services.BuildServiceProvider();

        // Fake a resolved tenant
        var mtc = new MultiTenantContext<TenantInfo>(new TenantInfo { Id = "", Identifier = "abc1" });
        sp.GetRequiredService<IMultiTenantContextSetter>().MultiTenantContext = mtc;

        // Trigger the ValidatePrincipal event
        var httpContextMock = new Mock<HttpContext>();
        httpContextMock.Setup(c => c.RequestServices).Returns(sp);
        var httpContextItems = new Dictionary<object, object?>
        {
            [$"{Constants.TenantToken}__bypass_validate_principal__"] = true
        };
        httpContextMock.Setup(c => c.Items).Returns(httpContextItems);
        var scheme = await sp.GetRequiredService<IAuthenticationSchemeProvider>()
            .GetSchemeAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        var options = sp.GetRequiredService<IOptionsMonitor<CookieAuthenticationOptions>>()
            .Get(CookieAuthenticationDefaults.AuthenticationScheme);
        var principal = new ClaimsPrincipal(new ClaimsIdentity());
        var authTicket = new AuthenticationTicket(principal, CookieAuthenticationDefaults.AuthenticationScheme);
        authTicket.Properties.Items[Constants.TenantToken] = "abc2";
        var cookieValidationContext =
            new CookieValidatePrincipalContext(httpContextMock.Object, scheme!, options, authTicket);

        await options.Events.ValidatePrincipal(cookieValidationContext);

        Assert.NotNull(cookieValidationContext.Principal);
        Assert.False(called);
    }

    [Fact]
    public async Task SkipPrincipalValidationIfBypassSet_WithClaimStrategy()
    {
        var services = new ServiceCollection();
        services.AddLogging();
        var called = false;
#pragma warning disable 1998
        services.AddAuthentication().AddCookie(o => o.Events.OnValidatePrincipal = async _ => called = true);
#pragma warning restore 1998
        services.AddMultiTenant<TenantInfo>()
            .WithClaimStrategy();
        var sp = services.BuildServiceProvider();

        // Fake a resolved tenant
        var mtc = new MultiTenantContext<TenantInfo>(new TenantInfo { Id = "", Identifier = "abc1" });
        sp.GetRequiredService<IMultiTenantContextSetter>().MultiTenantContext = mtc;

        // Trigger the ValidatePrincipal event
        var httpContextMock = new Mock<HttpContext>();
        httpContextMock.Setup(c => c.RequestServices).Returns(sp);
        var httpContextItems = new Dictionary<object, object?>
        {
            [$"{Constants.TenantToken}__bypass_validate_principal__"] = true
        };
        httpContextMock.Setup(c => c.Items).Returns(httpContextItems);
        var scheme = await sp.GetRequiredService<IAuthenticationSchemeProvider>()
            .GetSchemeAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        var options = sp.GetRequiredService<IOptionsMonitor<CookieAuthenticationOptions>>()
            .Get(CookieAuthenticationDefaults.AuthenticationScheme);
        var principal = new ClaimsPrincipal(new ClaimsIdentity());
        var authTicket = new AuthenticationTicket(principal, CookieAuthenticationDefaults.AuthenticationScheme);
        authTicket.Properties.Items[Constants.TenantToken] = "abc2";
        var cookieValidationContext =
            new CookieValidatePrincipalContext(httpContextMock.Object, scheme!, options, authTicket);

        await options.Events.ValidatePrincipal(cookieValidationContext);

        Assert.NotNull(cookieValidationContext.Principal);
        Assert.False(called);
    }

    [Fact]
    public async Task RejectPrincipalValidationIfTenantMatch()
    {
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddAuthentication().AddCookie();
        services.AddMultiTenant<TenantInfo>()
            .WithPerTenantAuthentication();
        var sp = services.BuildServiceProvider();


        // Fake a resolved tenant
        var mtc = new MultiTenantContext<TenantInfo>(new TenantInfo { Id = "", Identifier = "abc1" });
        sp.GetRequiredService<IMultiTenantContextSetter>().MultiTenantContext = mtc;

        // Trigger the ValidatePrincipal event
        var httpContextMock = new Mock<HttpContext>();
        httpContextMock.Setup(c => c.RequestServices).Returns(sp);
        httpContextMock.Setup(c => c.Items).Returns(new Dictionary<object, object?>());
        var scheme = await sp.GetRequiredService<IAuthenticationSchemeProvider>()
            .GetSchemeAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        var options = sp.GetRequiredService<IOptionsMonitor<CookieAuthenticationOptions>>()
            .Get(CookieAuthenticationDefaults.AuthenticationScheme);
        var principal = new ClaimsPrincipal(new ClaimsIdentity());
        var authTicket = new AuthenticationTicket(principal, CookieAuthenticationDefaults.AuthenticationScheme);
        authTicket.Properties.Items[Constants.TenantToken] = "abc2";
        var cookieValidationContext =
            new CookieValidatePrincipalContext(httpContextMock.Object, scheme!, options, authTicket);

        await options.Events.ValidatePrincipal(cookieValidationContext);

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

        var strategy = sp
            .GetServices<IMultiTenantStrategy>()
            .Single(s => s.GetType() == typeof(RemoteAuthenticationCallbackStrategy));
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

        var strategy = sp
            .GetServices<IMultiTenantStrategy>()
            .Single(s => s.GetType() == typeof(RemoteAuthenticationCallbackStrategy));
        Assert.NotNull(strategy);
    }

    [Fact]
    public async Task ConfigurePerTenantAuthentication_UseChallengeScheme()
    {
        var services = new ServiceCollection();
        services.AddOptions();
        services.AddAuthentication().AddCookie().AddOpenIdConnect("customScheme", null!);
        services.AddMultiTenant<TestTenantInfo>()
            .WithPerTenantAuthentication();
        var sp = services.BuildServiceProvider();

        var ti1 = new TestTenantInfo { Id = "id1", Identifier = "identifier1",
            ChallengeScheme = "customScheme"
        };

        var setter = sp.GetRequiredService<IMultiTenantContextSetter>();
        setter.MultiTenantContext = new MultiTenantContext<TestTenantInfo>(ti1);

        var options = sp.GetRequiredService<IAuthenticationSchemeProvider>();
        var scheme = await options.GetDefaultChallengeSchemeAsync();

        Assert.NotNull(scheme);
        Assert.Equal(ti1.ChallengeScheme, scheme.Name);
    }

    [Fact]
    public async Task ConfigurePerTenantAuthenticationConventions_UseChallengeScheme()
    {
        var services = new ServiceCollection();
        services.AddOptions();
        services.AddAuthentication().AddCookie().AddOpenIdConnect("customScheme", null!);
        services.AddMultiTenant<TestTenantInfo>()
            .WithPerTenantAuthenticationConventions();
        var sp = services.BuildServiceProvider();

        var ti1 = new TestTenantInfo { Id = "id1", Identifier = "identifier1",
            ChallengeScheme = "customScheme"
        };

        var setter = sp.GetRequiredService<IMultiTenantContextSetter>();
        setter.MultiTenantContext = new MultiTenantContext<TestTenantInfo>(ti1);

        var options = sp.GetRequiredService<IAuthenticationSchemeProvider>();
        Assert.NotNull(options);

        var scheme = await options.GetDefaultChallengeSchemeAsync();
        Assert.NotNull(scheme);
        Assert.Equal(ti1.ChallengeScheme, scheme.Name);
    }

    [Fact]
    public async Task ConfigurePerTenantAuthenticationConventions_UseDefaultChallengeSchemeOptionsIfNoTenantProp()
    {
        var services = new ServiceCollection();
        services.AddOptions();
        var defaultValue = "defaultScheme";
        services.AddAuthentication(o => o.DefaultChallengeScheme = defaultValue)
            .AddCookie()
            .AddOpenIdConnect("defaultScheme", null!);
        services.AddMultiTenant<TenantInfo>()
            .WithPerTenantAuthenticationConventions();
        var sp = services.BuildServiceProvider();

        var ti1 = new TenantInfo { Id = "id1", Identifier = "identifier1" };

        var setter = sp.GetRequiredService<IMultiTenantContextSetter>();
        setter.MultiTenantContext = new MultiTenantContext<TenantInfo>(ti1);

        var options = sp.GetRequiredService<IAuthenticationSchemeProvider>();
        Assert.NotNull(options);

        var scheme = await options.GetDefaultChallengeSchemeAsync();
        Assert.NotNull(scheme);
        Assert.Equal(defaultValue, scheme.Name);
    }

    [Fact]
    public void ConfigurePerTenantAuthentication_UseOpenIdConnectConvention()
    {
        var services = new ServiceCollection();
        services.AddSingleton<IConfiguration>(new ConfigurationBuilder().Build()); // net7.0+
        services.AddOptions();
        services.AddAuthentication().AddOpenIdConnect();
        services.AddMultiTenant<TestTenantInfo>()
            .WithPerTenantAuthentication();
        var sp = services.BuildServiceProvider();

        var ti1 = new TestTenantInfo { Id = "id1", Identifier = "identifier1",
            OpenIdConnectAuthority = "https://tenant",
            OpenIdConnectClientId = "tenant",
            OpenIdConnectClientSecret = "secret"
        };

        var setter = sp.GetRequiredService<IMultiTenantContextSetter>();
        setter.MultiTenantContext = new MultiTenantContext<TestTenantInfo>(ti1);

        var options = sp.GetRequiredService<IOptionsSnapshot<OpenIdConnectOptions>>()
            .Get(OpenIdConnectDefaults.AuthenticationScheme);

        Assert.Equal(ti1.OpenIdConnectAuthority, options.Authority);
        Assert.Equal(ti1.OpenIdConnectClientId, options.ClientId);
        Assert.Equal(ti1.OpenIdConnectClientSecret, options.ClientSecret);
    }

    [Fact]
    public void ConfigurePerTenantAuthenticationConventions_UseOpenIdConnectConvention()
    {
        var services = new ServiceCollection();
        services.AddSingleton<IConfiguration>(new ConfigurationBuilder().Build()); // net7.0+
        services.AddOptions();
        services.AddAuthentication().AddOpenIdConnect();
        services.AddMultiTenant<TestTenantInfo>()
            .WithPerTenantAuthenticationConventions();
        var sp = services.BuildServiceProvider();

        var ti1 = new TestTenantInfo { Id = "id1", Identifier = "identifier1",
            OpenIdConnectAuthority = "https://tenant",
            OpenIdConnectClientId = "tenant",
            OpenIdConnectClientSecret = "secret"
        };

        var setter = sp.GetRequiredService<IMultiTenantContextSetter>();
        setter.MultiTenantContext = new MultiTenantContext<TestTenantInfo>(ti1);

        var options = sp.GetRequiredService<IOptionsSnapshot<OpenIdConnectOptions>>()
            .Get(OpenIdConnectDefaults.AuthenticationScheme);

        Assert.Equal(ti1.OpenIdConnectAuthority, options.Authority);
        Assert.Equal(ti1.OpenIdConnectClientId, options.ClientId);
        Assert.Equal(ti1.OpenIdConnectClientSecret, options.ClientSecret);
    }

    [Fact]
    public void ConfigurePerTenantAuthenticationConventions_UseDefaultOpenIdConnectOptionsIfNoTenantProp()
    {
        var services = new ServiceCollection();
        services.AddSingleton<IConfiguration>(new ConfigurationBuilder().Build()); // net7.0+
        var defaultValue = "https://defaultValue";
        services.AddOptions().AddAuthentication()
            .AddOpenIdConnect(options =>
            {
                options.Authority = defaultValue;
                options.ClientId = defaultValue;
                options.ClientSecret = defaultValue;
            });
        services.AddMultiTenant<TenantInfo>()
            .WithPerTenantAuthenticationConventions();
        var sp = services.BuildServiceProvider();

        var ti1 = new TenantInfo { Id = "id1", Identifier = "identifier1" };

        var setter = sp.GetRequiredService<IMultiTenantContextSetter>();
        setter.MultiTenantContext = new MultiTenantContext<TenantInfo>(ti1);

        var options = sp.GetRequiredService<IOptionsSnapshot<OpenIdConnectOptions>>()
            .Get(OpenIdConnectDefaults.AuthenticationScheme);

        Assert.Equal(defaultValue, options.Authority);
        Assert.Equal(defaultValue, options.ClientId);
        Assert.Equal(defaultValue, options.ClientSecret);
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

        var ti1 = new TestTenantInfo { Id = "id1", Identifier = "identifier1",
            CookieLoginPath = "/path1",
            CookieLogoutPath = "/path2",
            CookieAccessDeniedPath = "/path3"
        };

        var setter = sp.GetRequiredService<IMultiTenantContextSetter>();
        setter.MultiTenantContext = new MultiTenantContext<TestTenantInfo>(ti1);

        var options = sp.GetRequiredService<IOptionsSnapshot<CookieAuthenticationOptions>>()
            .Get(CookieAuthenticationDefaults.AuthenticationScheme);

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

        var ti1 = new TestTenantInfo { Id = "id1", Identifier = "identifier1",
            CookieLoginPath = "/path1",
            CookieLogoutPath = "/path2",
            CookieAccessDeniedPath = "/path3"
        };

        var setter = sp.GetRequiredService<IMultiTenantContextSetter>();
        setter.MultiTenantContext = new MultiTenantContext<TestTenantInfo>(ti1);

        var options = sp.GetRequiredService<IOptionsSnapshot<CookieAuthenticationOptions>>()
            .Get(CookieAuthenticationDefaults.AuthenticationScheme);

        Assert.Equal(ti1.CookieLoginPath, options.LoginPath);
        Assert.Equal(ti1.CookieLogoutPath, options.LogoutPath);
        Assert.Equal(ti1.CookieAccessDeniedPath, options.AccessDeniedPath);
    }

    [Fact]
    public void ConfigurePerTenantAuthenticationConventions_UseDefaultCookieOptionsIfNoTenantProp()
    {
        var services = new ServiceCollection();
        var defaultValue = "/defaultValue";
        services.AddOptions().AddAuthentication().AddCookie(options =>
        {
            options.LoginPath = defaultValue;
            options.LogoutPath = defaultValue;
            options.AccessDeniedPath = defaultValue;
        });
        services.AddMultiTenant<TenantInfo>()
            .WithPerTenantAuthenticationConventions();
        var sp = services.BuildServiceProvider();

        var ti1 = new TenantInfo { Id = "id1", Identifier = "identifier1" };

        var setter = sp.GetRequiredService<IMultiTenantContextSetter>();
        setter.MultiTenantContext = new MultiTenantContext<TenantInfo>(ti1);

        var options = sp.GetRequiredService<IOptionsSnapshot<CookieAuthenticationOptions>>()
            .Get(CookieAuthenticationDefaults.AuthenticationScheme);

        Assert.Equal(defaultValue, options.LoginPath);
        Assert.Equal(defaultValue, options.LogoutPath);
        Assert.Equal(defaultValue, options.AccessDeniedPath);
    }

    [Fact]
    public void WithPerTenantAuthentication_ThrowIfCantDecorateIAuthenticationService()
    {
        var services = new ServiceCollection();
        var builder = new MultiTenantBuilder<TenantInfo>(services);

        Assert.Throws<MultiTenantException>(() => builder.WithPerTenantAuthentication());
    }

    [Fact]
    public void AddBasePathStrategy()
    {
        var services = new ServiceCollection();
        var builder = new MultiTenantBuilder<TenantInfo>(services);
        builder.WithBasePathStrategy();
        var sp = services.BuildServiceProvider();

        var strategy = sp.GetRequiredService<IMultiTenantStrategy>();
        Assert.IsType<BasePathStrategy>(strategy);
    }

    [Fact]
    public void AddBasePathStrategyDefaultRebaseDefaultTrue()
    {
        var services = new ServiceCollection();
        var builder = new MultiTenantBuilder<TenantInfo>(services);
        builder.WithBasePathStrategy();
        var sp = services.BuildServiceProvider();

        var strategy = sp.GetRequiredService<IMultiTenantStrategy>();
        Assert.IsType<BasePathStrategy>(strategy);

        var options = sp.GetRequiredService<IOptions<BasePathStrategyOptions>>();
        Assert.True(options.Value.RebaseAspNetCorePathBase);
    }

    [Fact]
    public void AddBasePathStrategyWithOptions()
    {
        var services = new ServiceCollection();
        var builder = new MultiTenantBuilder<TenantInfo>(services);
        builder.WithBasePathStrategy(options => options.RebaseAspNetCorePathBase = true);
        var sp = services.BuildServiceProvider();

        var strategy = sp.GetRequiredService<IMultiTenantStrategy>();
        Assert.IsType<BasePathStrategy>(strategy);

        var options = sp.GetRequiredService<IOptions<BasePathStrategyOptions>>();
        Assert.True(options.Value.RebaseAspNetCorePathBase);
    }

    [Fact]
    public void AddClaimStrategy()
    {
        var services = new ServiceCollection();
        var builder = new MultiTenantBuilder<TenantInfo>(services);
        builder.WithClaimStrategy();
        var sp = services.BuildServiceProvider();

        var strategy = sp.GetRequiredService<IMultiTenantStrategy>();
        Assert.IsType<ClaimStrategy>(strategy);
    }

    [Fact]
    public void AddHeaderStrategy()
    {
        var services = new ServiceCollection();
        var builder = new MultiTenantBuilder<TenantInfo>(services);
        builder.WithHeaderStrategy();
        var sp = services.BuildServiceProvider();

        var strategy = sp.GetRequiredService<IMultiTenantStrategy>();
        Assert.IsType<HeaderStrategy>(strategy);
    }

    [Fact]
    public void AddHostStrategy()
    {
        var services = new ServiceCollection();
        var builder = new MultiTenantBuilder<TenantInfo>(services);
        builder.WithHostStrategy();
        var sp = services.BuildServiceProvider();

        var strategy = sp.GetRequiredService<IMultiTenantStrategy>();
        Assert.IsType<HostStrategy>(strategy);
    }

    [Fact]
    public void ThrowIfNullParamAddingHostStrategy()
    {
        var services = new ServiceCollection();
        var builder = new MultiTenantBuilder<TenantInfo>(services);
        Assert.Throws<ArgumentException>(()
            => builder.WithHostStrategy(null!));
    }

    [Fact]
    public void AddRouteStrategy()
    {
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddRouting();
        var builder = new MultiTenantBuilder<TenantInfo>(services);
        builder.WithRouteStrategy();
        var sp = services.BuildServiceProvider();

        var strategy = sp.GetRequiredService<IMultiTenantStrategy>();
        Assert.IsType<RouteStrategy>(strategy);
    }

    [Fact]
    public void ThrowIfNullParamAddingRouteStrategy()
    {
        var services = new ServiceCollection();
        var builder = new MultiTenantBuilder<TenantInfo>(services);
        Assert.Throws<ArgumentException>(()
            => builder.WithRouteStrategy(null!, false));
    }

    [Fact]
    public void AddRouteStrategyWithTenantAmbientRouteValue_DecoratesLinkGenerator()
    {
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddRouting();
        var builder = new MultiTenantBuilder<TenantInfo>(services);
        builder.WithRouteStrategy("routeParam", useTenantAmbientRouteValue: true);
        var sp = services.BuildServiceProvider();

        var strategy = sp.GetRequiredService<IMultiTenantStrategy>();
        Assert.IsType<RouteStrategy>(strategy);

        var linkGenerator = sp.GetRequiredService<LinkGenerator>();
        Assert.IsType<MultiTenantAmbientValueLinkGenerator>(linkGenerator);
    }

    [Fact]
    public void AddRouteStrategyWithoutTenantAmbientRouteValue_DoesNotDecorateLinkGenerator()
    {
        var services = new ServiceCollection();
        services.AddRouting();
        services.AddLogging();
        var builder = new MultiTenantBuilder<TenantInfo>(services);
        builder.WithRouteStrategy("routeParam", useTenantAmbientRouteValue: false);
        var sp = services.BuildServiceProvider();

        var strategy = sp.GetRequiredService<IMultiTenantStrategy>();
        Assert.IsType<RouteStrategy>(strategy);

        var linkGenerator = sp.GetRequiredService<LinkGenerator>();
        Assert.IsNotType<MultiTenantAmbientValueLinkGenerator>(linkGenerator);
    }

    [Fact]
    public void AddRouteStrategyDefault_DecoratesLinkGeneratorByDefault()
    {
        var services = new ServiceCollection();
        services.AddRouting();
        services.AddLogging();
        var builder = new MultiTenantBuilder<TenantInfo>(services);
        builder.WithRouteStrategy(); // Uses default overload which sets useTenantAmbientRouteValue to true
        var sp = services.BuildServiceProvider();

        var strategy = sp.GetRequiredService<IMultiTenantStrategy>();
        Assert.IsType<RouteStrategy>(strategy);

        var linkGenerator = sp.GetRequiredService<LinkGenerator>();
        Assert.IsType<MultiTenantAmbientValueLinkGenerator>(linkGenerator);
    }

    private class TestTenantInfo : TenantInfo
    {
        public string? ChallengeScheme { get; set; }
        public string? CookieLoginPath { get; set; }
        public string? CookieLogoutPath { get; set; }
        public string? CookieAccessDeniedPath { get; set; }
        public string? OpenIdConnectAuthority { get; set; }
        public string? OpenIdConnectClientId { get; set; }
        public string? OpenIdConnectClientSecret { get; set; }
    }
}