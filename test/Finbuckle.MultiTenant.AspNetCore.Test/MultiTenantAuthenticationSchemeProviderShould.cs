// Copyright Finbuckle LLC, Andrew White, and Contributors.
// Refer to the solution LICENSE file for more information.

using Finbuckle.MultiTenant.Abstractions;
using Finbuckle.MultiTenant.AspNetCore.Authentication;
using Finbuckle.MultiTenant.AspNetCore.Extensions;
using Finbuckle.MultiTenant.Extensions;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Xunit;

namespace Finbuckle.MultiTenant.AspNetCore.Test;

public class MultiTenantAuthenticationSchemeProviderShould
{
    [Fact]
    public async Task ReturnPerTenantAuthenticationOptions()
    {
        var services = new ServiceCollection();
        services.AddAuthentication()
            .AddCookie("tenant1Scheme")
            .AddCookie("tenant2Scheme");

        services.AddMultiTenant<TenantInfo, string>()
            .WithPerTenantAuthentication();

        services.ConfigureAllPerTenant<AuthenticationOptions, TenantInfo,string>((ao, ti) =>
        {
            ao.DefaultChallengeScheme = ti.Identifier + "Scheme";
        });

        // ValidateScopes ensures the authentication decorators have valid lifetimes.
        var sp = services.BuildServiceProvider(new ServiceProviderOptions { ValidateScopes = true });

        var tenant1 = new TenantInfo { Id = "tenant1", Identifier = "tenant1" };

        var tenant2 = new TenantInfo { Id = "tenant2", Identifier = "tenant2" };

        using (var scope1 = sp.CreateScope())
        {
            scope1.ServiceProvider.BeginTenantScope(tenant1);

            var schemeProvider = scope1.ServiceProvider.GetRequiredService<IAuthenticationSchemeProvider>();

            var option = await schemeProvider.GetDefaultChallengeSchemeAsync();

            Assert.NotNull(option);
            Assert.Equal("tenant1Scheme", option.Name);
        }

        using (var scope2 = sp.CreateScope())
        {
            scope2.ServiceProvider.BeginTenantScope(tenant2);

            var schemeProvider = scope2.ServiceProvider.GetRequiredService<IAuthenticationSchemeProvider>();

            var option = await schemeProvider.GetDefaultChallengeSchemeAsync();

            Assert.NotNull(option);
            Assert.Equal("tenant2Scheme", option.Name);
        }
    }

    [Fact]
    public void BeRegisteredAsSingleton()
    {
        var services = new ServiceCollection();
        services.AddAuthentication()
            .AddCookie("tenant1Scheme");

        services.AddMultiTenant<TenantInfo, string>()
            .WithPerTenantAuthentication();

        var sp = services.BuildServiceProvider(new ServiceProviderOptions { ValidateScopes = true });
        sp.BeginTenantScope();

        var provider1a = sp.GetRequiredService<IAuthenticationSchemeProvider>();
        using var scope1 = sp.CreateScope();
        using var scope2 = sp.CreateScope();
        var provider1b = scope1.ServiceProvider.GetRequiredService<IAuthenticationSchemeProvider>();
        var provider2 = scope2.ServiceProvider.GetRequiredService<IAuthenticationSchemeProvider>();

        Assert.Same(provider1a, provider1b);
        Assert.Same(provider1a, provider2);
    }

    [Fact]
    public async Task DelegateSchemeMutationsToInnerProvider()
    {
        var services = new ServiceCollection();
        services.AddAuthentication()
            .AddCookie("tenant1Scheme");

        services.AddMultiTenant<TenantInfo, string>()
            .WithPerTenantAuthentication();

        var sp = services.BuildServiceProvider(new ServiceProviderOptions { ValidateScopes = true });

        using var scope = sp.CreateScope();
        scope.ServiceProvider.BeginTenantScope();
        var schemeProvider = scope.ServiceProvider.GetRequiredService<IAuthenticationSchemeProvider>();

        var newScheme = new AuthenticationScheme("dynamicScheme", "dynamicScheme", typeof(CookieAuthenticationHandler));
        schemeProvider.AddScheme(newScheme);

        // The scheme should be visible both through this decorator and via enumeration,
        // proving AddScheme mutated the shared inner provider rather than local state.
        Assert.NotNull(await schemeProvider.GetSchemeAsync("dynamicScheme"));
        Assert.Contains((await schemeProvider.GetAllSchemesAsync()), s => s.Name == "dynamicScheme");

        schemeProvider.RemoveScheme("dynamicScheme");
        Assert.Null(await schemeProvider.GetSchemeAsync("dynamicScheme"));
    }

    private static AuthenticationScheme CreateScheme(string name) =>
        new(name, name, typeof(CookieAuthenticationHandler));

    private static MultiTenantAuthenticationSchemeProvider CreateProvider(
        AuthenticationOptions options, out Mock<IAuthenticationSchemeProvider> inner)
    {
        inner = new Mock<IAuthenticationSchemeProvider>();
        inner.Setup(i => i.GetSchemeAsync(It.IsAny<string>()))
            .Returns((string name) => Task.FromResult<AuthenticationScheme?>(CreateScheme(name)));
        return new MultiTenantAuthenticationSchemeProvider(inner.Object, Microsoft.Extensions.Options.Options.Create(options));
    }

    [Fact]
    public async Task ReturnNullDefaultSchemesWhenNoneConfigured()
    {
        var provider = CreateProvider(new AuthenticationOptions(), out _);

        Assert.Null(await provider.GetDefaultAuthenticateSchemeAsync());
        Assert.Null(await provider.GetDefaultChallengeSchemeAsync());
        Assert.Null(await provider.GetDefaultForbidSchemeAsync());
        Assert.Null(await provider.GetDefaultSignInSchemeAsync());
        Assert.Null(await provider.GetDefaultSignOutSchemeAsync());
    }

    [Fact]
    public async Task UseDefaultAuthenticateSchemeWhenSpecificOneNotSet()
    {
        var provider = CreateProvider(new AuthenticationOptions { DefaultScheme = "default" }, out var inner);

        var scheme = await provider.GetDefaultAuthenticateSchemeAsync();

        Assert.Equal("default", scheme?.Name);
        inner.Verify(i => i.GetSchemeAsync("default"), Times.Once);
    }

    [Fact]
    public async Task PreferSpecificDefaultAuthenticateSchemeOverDefaultScheme()
    {
        var provider = CreateProvider(new AuthenticationOptions
        {
            DefaultScheme = "default",
            DefaultAuthenticateScheme = "authenticate"
        }, out var inner);

        var scheme = await provider.GetDefaultAuthenticateSchemeAsync();

        Assert.Equal("authenticate", scheme?.Name);
        inner.Verify(i => i.GetSchemeAsync("default"), Times.Never);
    }

    [Fact]
    public async Task UseDefaultSchemeWhenChallengeSchemeNotSet()
    {
        var provider = CreateProvider(new AuthenticationOptions { DefaultScheme = "default" }, out _);

        var scheme = await provider.GetDefaultChallengeSchemeAsync();

        Assert.Equal("default", scheme?.Name);
    }

    [Fact]
    public async Task UseChallengeSchemeForForbidWhenForbidSchemeNotSet()
    {
        var provider = CreateProvider(new AuthenticationOptions { DefaultChallengeScheme = "challenge" }, out _);

        var scheme = await provider.GetDefaultForbidSchemeAsync();

        Assert.Equal("challenge", scheme?.Name);
    }

    [Fact]
    public async Task PreferSpecificForbidSchemeOverChallengeScheme()
    {
        var provider = CreateProvider(new AuthenticationOptions
        {
            DefaultChallengeScheme = "challenge",
            DefaultForbidScheme = "forbid"
        }, out _);

        var scheme = await provider.GetDefaultForbidSchemeAsync();

        Assert.Equal("forbid", scheme?.Name);
    }

    [Fact]
    public async Task UseDefaultSchemeForSignInWhenSignInSchemeNotSet()
    {
        var provider = CreateProvider(new AuthenticationOptions { DefaultScheme = "default" }, out _);

        var scheme = await provider.GetDefaultSignInSchemeAsync();

        Assert.Equal("default", scheme?.Name);
    }

    [Fact]
    public async Task UseSignInSchemeForSignOutWhenSignOutSchemeNotSet()
    {
        var provider = CreateProvider(new AuthenticationOptions { DefaultSignInScheme = "signIn" }, out _);

        var scheme = await provider.GetDefaultSignOutSchemeAsync();

        Assert.Equal("signIn", scheme?.Name);
    }

    [Fact]
    public async Task PreferSpecificSignOutSchemeOverSignInScheme()
    {
        var provider = CreateProvider(new AuthenticationOptions
        {
            DefaultSignInScheme = "signIn",
            DefaultSignOutScheme = "signOut"
        }, out _);

        var scheme = await provider.GetDefaultSignOutSchemeAsync();

        Assert.Equal("signOut", scheme?.Name);
    }

    [Fact]
    public async Task DelegateGetRequestHandlerSchemesAsyncToInner()
    {
        var handlerSchemes = new[] { CreateScheme("handler1") };
        var inner = new Mock<IAuthenticationSchemeProvider>();
        inner.Setup(i => i.GetRequestHandlerSchemesAsync())
            .ReturnsAsync(handlerSchemes.AsEnumerable());
        var provider = new MultiTenantAuthenticationSchemeProvider(inner.Object, Microsoft.Extensions.Options.Options.Create(new AuthenticationOptions()));

        var result = await provider.GetRequestHandlerSchemesAsync();

        Assert.Same(handlerSchemes, result);
    }

    [Fact]
    public async Task DelegateGetAllSchemesAsyncToInner()
    {
        var allSchemes = new[] { CreateScheme("a"), CreateScheme("b") };
        var inner = new Mock<IAuthenticationSchemeProvider>();
        inner.Setup(i => i.GetAllSchemesAsync()).ReturnsAsync(allSchemes.AsEnumerable());
        var provider = new MultiTenantAuthenticationSchemeProvider(inner.Object, Microsoft.Extensions.Options.Options.Create(new AuthenticationOptions()));

        var result = await provider.GetAllSchemesAsync();

        Assert.Same(allSchemes, result);
    }
}
