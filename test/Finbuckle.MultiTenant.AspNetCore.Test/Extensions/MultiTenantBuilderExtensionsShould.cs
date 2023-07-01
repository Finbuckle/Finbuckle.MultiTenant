// Copyright Finbuckle LLC, Andrew White, and Contributors.
// Refer to the solution LICENSE file for more information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using Finbuckle.MultiTenant.AspNetCore.Options;
using Finbuckle.MultiTenant.Strategies;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Moq;
using Xunit;
using Constants = Finbuckle.MultiTenant.Internal.Constants;

namespace Finbuckle.MultiTenant.AspNetCore.Test.Extensions
{
    public class MultiTenantBuilderExtensionsShould
    {
        private class TestTenantInfo : ITenantInfo
        {
            public string? Id { get; set; }
            public string? Identifier { get; set; }
            public string? Name { get; set; }
            public string? ConnectionString { get; set; }

            public string? ChallengeScheme { get; set; }

            // public string? CookiePath { get; set; }
            public string? CookieLoginPath { get; set; }
            public string? CookieLogoutPath { get; set; }
            public string? CookieAccessDeniedPath { get; set; }
            public string? OpenIdConnectAuthority { get; set; }
            public string? OpenIdConnectClientId { get; set; }
            public string? OpenIdConnectClientSecret { get; set; }
        }

        [Fact]
        public void NotThrowIfOriginalPrincipalValidationNotSet()
        {
            var services = new ServiceCollection();
            services.AddLogging();
            services.AddAuthentication().AddCookie();
            services.AddMultiTenant<TenantInfo>()
                .WithPerTenantAuthentication();
            var sp = services.BuildServiceProvider();

            // Fake a resolved tenant
            var mtc = new MultiTenantContext<TenantInfo>
            {
                TenantInfo = new TenantInfo { Identifier = "abc" }
            };
            sp.GetRequiredService<IMultiTenantContextAccessor<TenantInfo>>().MultiTenantContext = mtc;

            // Trigger the ValidatePrincipal event
            var httpContextMock = new Mock<HttpContext>();
            httpContextMock.Setup(c => c.RequestServices).Returns(sp);
            httpContextMock.Setup(c => c.Items).Returns(new Dictionary<object, object?>());
            var scheme = sp.GetRequiredService<IAuthenticationSchemeProvider>()
                .GetSchemeAsync(CookieAuthenticationDefaults.AuthenticationScheme).Result;
            var options = sp.GetRequiredService<IOptionsMonitor<CookieAuthenticationOptions>>()
                .Get(CookieAuthenticationDefaults.AuthenticationScheme);
            var principal = new ClaimsPrincipal(new ClaimsIdentity());
            var authTicket = new AuthenticationTicket(principal, CookieAuthenticationDefaults.AuthenticationScheme);
            authTicket.Properties.Items[Constants.TenantToken] = "abc";
            var cookieValidationContext =
                new CookieValidatePrincipalContext(httpContextMock.Object, scheme!, options, authTicket);

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
            var mtc = new MultiTenantContext<TenantInfo>
            {
                TenantInfo = new TenantInfo { Identifier = "abc" }
            };
            sp.GetRequiredService<IMultiTenantContextAccessor<TenantInfo>>().MultiTenantContext = mtc;

            // Trigger the ValidatePrincipal event
            var httpContextMock = new Mock<HttpContext>();
            httpContextMock.Setup(c => c.RequestServices).Returns(sp);
            httpContextMock.Setup(c => c.Items).Returns(new Dictionary<object, object?>());
            var scheme = sp.GetRequiredService<IAuthenticationSchemeProvider>()
                .GetSchemeAsync(CookieAuthenticationDefaults.AuthenticationScheme).Result;
            var options = sp.GetRequiredService<IOptionsMonitor<CookieAuthenticationOptions>>()
                .Get(CookieAuthenticationDefaults.AuthenticationScheme);
            var principal = new ClaimsPrincipal(new ClaimsIdentity());
            var authTicket = new AuthenticationTicket(principal, CookieAuthenticationDefaults.AuthenticationScheme);
            authTicket.Properties.Items[Constants.TenantToken] = "abc";
            var cookieValidationContext =
                new CookieValidatePrincipalContext(httpContextMock.Object, scheme!, options, authTicket);

            options.Events.ValidatePrincipal(cookieValidationContext).Wait();

            Assert.True(called);
        }

        [Fact]
        public void PassPrincipalValidationIfTenantMatch()
        {
            var services = new ServiceCollection();
            services.AddLogging();
            services.AddAuthentication().AddCookie();
            services.AddMultiTenant<TenantInfo>()
                .WithPerTenantAuthentication();
            var sp = services.BuildServiceProvider();


            // Fake a resolved tenant
            var mtc = new MultiTenantContext<TenantInfo>
            {
                TenantInfo = new TenantInfo { Identifier = "abc" }
            };
            sp.GetRequiredService<IMultiTenantContextAccessor<TenantInfo>>().MultiTenantContext = mtc;

            // Trigger the ValidatePrincipal event
            var httpContextMock = new Mock<HttpContext>();
            httpContextMock.Setup(c => c.RequestServices).Returns(sp);
            httpContextMock.Setup(c => c.Items).Returns(new Dictionary<object, object?>());
            var scheme = sp.GetRequiredService<IAuthenticationSchemeProvider>()
                .GetSchemeAsync(CookieAuthenticationDefaults.AuthenticationScheme).Result;
            var options = sp.GetRequiredService<IOptionsMonitor<CookieAuthenticationOptions>>()
                .Get(CookieAuthenticationDefaults.AuthenticationScheme);
            var principal = new ClaimsPrincipal(new ClaimsIdentity());
            var authTicket = new AuthenticationTicket(principal, CookieAuthenticationDefaults.AuthenticationScheme);
            authTicket.Properties.Items[Constants.TenantToken] = "abc";
            var cookieValidationContext =
                new CookieValidatePrincipalContext(httpContextMock.Object, scheme!, options, authTicket);

            options.Events.ValidatePrincipal(cookieValidationContext).Wait();

            Assert.NotNull(cookieValidationContext);
        }

        [Fact]
        public void SkipPrincipalValidationIfBypassSet_WithPerTenantAuthentication()
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
            var mtc = new MultiTenantContext<TenantInfo>
            {
                TenantInfo = new TenantInfo { Identifier = "abc1" }
            };
            sp.GetRequiredService<IMultiTenantContextAccessor<TenantInfo>>().MultiTenantContext = mtc;

            // Trigger the ValidatePrincipal event
            var httpContextMock = new Mock<HttpContext>();
            httpContextMock.Setup(c => c.RequestServices).Returns(sp);
            var httpContextItems = new Dictionary<object, object?>
            {
                [$"{Constants.TenantToken}__bypass_validate_principal__"] = true
            };
            httpContextMock.Setup(c => c.Items).Returns(httpContextItems);
            var scheme = sp.GetRequiredService<IAuthenticationSchemeProvider>()
                .GetSchemeAsync(CookieAuthenticationDefaults.AuthenticationScheme).Result;
            var options = sp.GetRequiredService<IOptionsMonitor<CookieAuthenticationOptions>>()
                .Get(CookieAuthenticationDefaults.AuthenticationScheme);
            var principal = new ClaimsPrincipal(new ClaimsIdentity());
            var authTicket = new AuthenticationTicket(principal, CookieAuthenticationDefaults.AuthenticationScheme);
            authTicket.Properties.Items[Constants.TenantToken] = "abc2";
            var cookieValidationContext =
                new CookieValidatePrincipalContext(httpContextMock.Object, scheme!, options, authTicket);

            options.Events.ValidatePrincipal(cookieValidationContext).Wait();

            Assert.NotNull(cookieValidationContext.Principal);
            Assert.False(called);
        }

        [Fact]
        public void SkipPrincipalValidationIfBypassSet_WithClaimStrategy()
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
            var mtc = new MultiTenantContext<TenantInfo>
            {
                TenantInfo = new TenantInfo { Identifier = "abc1" }
            };
            sp.GetRequiredService<IMultiTenantContextAccessor<TenantInfo>>().MultiTenantContext = mtc;

            // Trigger the ValidatePrincipal event
            var httpContextMock = new Mock<HttpContext>();
            httpContextMock.Setup(c => c.RequestServices).Returns(sp);
            var httpContextItems = new Dictionary<object, object?>
            {
                [$"{Constants.TenantToken}__bypass_validate_principal__"] = true
            };
            httpContextMock.Setup(c => c.Items).Returns(httpContextItems);
            var scheme = sp.GetRequiredService<IAuthenticationSchemeProvider>()
                .GetSchemeAsync(CookieAuthenticationDefaults.AuthenticationScheme).Result;
            var options = sp.GetRequiredService<IOptionsMonitor<CookieAuthenticationOptions>>()
                .Get(CookieAuthenticationDefaults.AuthenticationScheme);
            var principal = new ClaimsPrincipal(new ClaimsIdentity());
            var authTicket = new AuthenticationTicket(principal, CookieAuthenticationDefaults.AuthenticationScheme);
            authTicket.Properties.Items[Constants.TenantToken] = "abc2";
            var cookieValidationContext =
                new CookieValidatePrincipalContext(httpContextMock.Object, scheme!, options, authTicket);

            options.Events.ValidatePrincipal(cookieValidationContext).Wait();

            Assert.NotNull(cookieValidationContext.Principal);
            Assert.False(called);
        }

        [Fact]
        public void RejectPrincipalValidationIfTenantMatch()
        {
            var services = new ServiceCollection();
            services.AddLogging();
            services.AddAuthentication().AddCookie();
            services.AddMultiTenant<TenantInfo>()
                .WithPerTenantAuthentication();
            var sp = services.BuildServiceProvider();


            // Fake a resolved tenant
            var mtc = new MultiTenantContext<TenantInfo>
            {
                TenantInfo = new TenantInfo { Identifier = "abc1" }
            };
            sp.GetRequiredService<IMultiTenantContextAccessor<TenantInfo>>().MultiTenantContext = mtc;

            // Trigger the ValidatePrincipal event
            var httpContextMock = new Mock<HttpContext>();
            httpContextMock.Setup(c => c.RequestServices).Returns(sp);
            httpContextMock.Setup(c => c.Items).Returns(new Dictionary<object, object?>());
            var scheme = sp.GetRequiredService<IAuthenticationSchemeProvider>()
                .GetSchemeAsync(CookieAuthenticationDefaults.AuthenticationScheme).Result;
            var options = sp.GetRequiredService<IOptionsMonitor<CookieAuthenticationOptions>>()
                .Get(CookieAuthenticationDefaults.AuthenticationScheme);
            var principal = new ClaimsPrincipal(new ClaimsIdentity());
            var authTicket = new AuthenticationTicket(principal, CookieAuthenticationDefaults.AuthenticationScheme);
            authTicket.Properties.Items[Constants.TenantToken] = "abc2";
            var cookieValidationContext =
                new CookieValidatePrincipalContext(httpContextMock.Object, scheme!, options, authTicket);

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
        public void ConfigurePerTenantAuthentication_UseChallengeScheme()
        {
            var services = new ServiceCollection();
            services.AddOptions();
            services.AddAuthentication().AddCookie().AddOpenIdConnect("customScheme", null!);
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
            Assert.Equal(ti1.ChallengeScheme, options.GetDefaultChallengeSchemeAsync()!.Result!.Name);
        }

        [Fact]
        public void ConfigurePerTenantAuthenticationConventions_UseChallengeScheme()
        {
            var services = new ServiceCollection();
            services.AddOptions();
            services.AddAuthentication().AddCookie().AddOpenIdConnect("customScheme", null!);
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
            Assert.Equal(ti1.ChallengeScheme, options.GetDefaultChallengeSchemeAsync()!.Result!.Name);
        }

        [Fact]
        public void ConfigurePerTenantAuthenticationConventions_UseDefaultChallengeSchemeOptionsIfNoTenantProp()
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

            var ti1 = new TenantInfo
            {
                Id = "id1",
                Identifier = "identifier1"
            };

            var accessor = sp.GetRequiredService<IMultiTenantContextAccessor<TenantInfo>>();
            accessor.MultiTenantContext = new MultiTenantContext<TenantInfo> { TenantInfo = ti1 };

            var options = sp.GetRequiredService<IAuthenticationSchemeProvider>();
            Assert.Equal(defaultValue, options.GetDefaultChallengeSchemeAsync()!.Result!.Name);
        }

        [Fact]
        public void ConfigurePerTenantAuthentication_UseOpenIdConnectConvention()
        {
            var services = new ServiceCollection();
            services.AddSingleton<IConfiguration>((new ConfigurationBuilder()).Build()); // net7.0+
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
            services.AddSingleton<IConfiguration>((new ConfigurationBuilder()).Build()); // net7.0+
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
            services.AddSingleton<IConfiguration>((new ConfigurationBuilder()).Build()); // net7.0+
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

            var ti1 = new TenantInfo
            {
                Id = "id1",
                Identifier = "identifier1"
            };

            var accessor = sp.GetRequiredService<IMultiTenantContextAccessor<TenantInfo>>();
            accessor.MultiTenantContext = new MultiTenantContext<TenantInfo> { TenantInfo = ti1 };

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

            var ti1 = new TenantInfo
            {
                Id = "id1",
                Identifier = "identifier1"
            };

            var accessor = sp.GetRequiredService<IMultiTenantContextAccessor<TenantInfo>>();
            accessor.MultiTenantContext = new MultiTenantContext<TenantInfo> { TenantInfo = ti1 };

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
        public void AddBasePathStrategyDefaultRebaseFalse()
        {
            var services = new ServiceCollection();
            var builder = new FinbuckleMultiTenantBuilder<TenantInfo>(services);
            builder.WithBasePathStrategy();
            var sp = services.BuildServiceProvider();

            var strategy = sp.GetRequiredService<IMultiTenantStrategy>();
            Assert.IsType<BasePathStrategy>(strategy);

            var options = sp.GetRequiredService<IOptions<BasePathStrategyOptions>>();
            Assert.False(options.Value.RebaseAspNetCorePathBase);
        }

        [Fact]
        public void AddBasePathStrategyWithOptions()
        {
            var services = new ServiceCollection();
            var builder = new FinbuckleMultiTenantBuilder<TenantInfo>(services);
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
                => builder.WithHostStrategy(null!));
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
                => builder.WithRouteStrategy(null!));
        }
    }
}