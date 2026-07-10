// Copyright Finbuckle LLC, Andrew White, and Contributors.
// Refer to the solution LICENSE file for more information.

using System.Security.Claims;
using Finbuckle.MultiTenant.Abstractions;
using Finbuckle.MultiTenant.AspNetCore.Authentication;
using Finbuckle.MultiTenant.Extensions;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Moq;
using Xunit;

namespace Finbuckle.MultiTenant.AspNetCore.Test;

public class MultiTenantAuthenticationServiceShould
{
    private static (MultiTenantAuthenticationService<TenantInfo> Service, Mock<IAuthenticationService> Inner,
        DefaultHttpContext Context) CreateService(bool resolved = true, bool skipUnresolvedChallenge = false)
    {
        var services = new ServiceCollection();
        services.AddMultiTenant<TenantInfo>();
        var serviceProvider = services.BuildServiceProvider();
        if (resolved)
        {
            serviceProvider.GetRequiredService<ITenantContext<TenantInfo>>().TenantInfo =
                new TenantInfo { Id = "initech-id", Identifier = "initech" };
        }

        var options = new Mock<IOptionsMonitor<MultiTenantAuthenticationOptions>>();
        options.Setup(o => o.CurrentValue).Returns(new MultiTenantAuthenticationOptions
        {
            SkipChallengeIfTenantNotResolved = skipUnresolvedChallenge
        });
        var inner = new Mock<IAuthenticationService>();
        var context = new DefaultHttpContext { RequestServices = serviceProvider };
        return (new MultiTenantAuthenticationService<TenantInfo>(inner.Object, options.Object), inner, context);
    }

    [Fact]
    public void ThrowIfInnerServiceIsNull()
    {
        Assert.Throws<ArgumentNullException>(() =>
            new MultiTenantAuthenticationService<TenantInfo>(null!,
                Mock.Of<IOptionsMonitor<MultiTenantAuthenticationOptions>>()));
    }

    [Fact]
    public async Task DelegateAuthenticateUnchanged()
    {
        var (service, inner, context) = CreateService();
        var expected = AuthenticateResult.NoResult();
        inner.Setup(i => i.AuthenticateAsync(context, "scheme")).ReturnsAsync(expected);

        var result = await service.AuthenticateAsync(context, "scheme");

        Assert.Same(expected, result);
        inner.Verify(i => i.AuthenticateAsync(context, "scheme"), Times.Once);
    }

    [Theory]
    [InlineData("challenge")]
    [InlineData("forbid")]
    [InlineData("sign-in")]
    [InlineData("sign-out")]
    public async Task AddTenantIdentifierToAuthenticationOperations(string operation)
    {
        var (service, inner, context) = CreateService();
        AuthenticationProperties? captured = null;
        var principal = new ClaimsPrincipal(new ClaimsIdentity());

        switch (operation)
        {
            case "challenge":
                inner.Setup(i => i.ChallengeAsync(context, "scheme", It.IsAny<AuthenticationProperties>()))
                    .Callback<HttpContext, string?, AuthenticationProperties?>((_, _, p) => captured = p);
                await service.ChallengeAsync(context, "scheme", null);
                break;
            case "forbid":
                inner.Setup(i => i.ForbidAsync(context, "scheme", It.IsAny<AuthenticationProperties>()))
                    .Callback<HttpContext, string?, AuthenticationProperties?>((_, _, p) => captured = p);
                await service.ForbidAsync(context, "scheme", null);
                break;
            case "sign-in":
                inner.Setup(i => i.SignInAsync(context, "scheme", principal, It.IsAny<AuthenticationProperties>()))
                    .Callback<HttpContext, string?, ClaimsPrincipal, AuthenticationProperties?>((_, _, _, p) =>
                        captured = p);
                await service.SignInAsync(context, "scheme", principal, null);
                break;
            case "sign-out":
                inner.Setup(i => i.SignOutAsync(context, "scheme", It.IsAny<AuthenticationProperties>()))
                    .Callback<HttpContext, string?, AuthenticationProperties?>((_, _, p) => captured = p);
                await service.SignOutAsync(context, "scheme", null);
                break;
        }

        Assert.NotNull(captured);
        Assert.Equal("initech", captured.Items[Constants.TenantToken]);
    }

    [Fact]
    public async Task PreserveExistingTenantIdentifier()
    {
        var (service, inner, context) = CreateService();
        var properties = new AuthenticationProperties();
        properties.Items[Constants.TenantToken] = "existing";
        AuthenticationProperties? captured = null;
        inner.Setup(i => i.ChallengeAsync(context, null, properties))
            .Callback<HttpContext, string?, AuthenticationProperties?>((_, _, p) => captured = p);

        await service.ChallengeAsync(context, null, properties);

        Assert.Same(properties, captured);
        Assert.Equal("existing", properties.Items[Constants.TenantToken]);
    }

    [Fact]
    public async Task DelegateWithoutPropertiesIfTenantIsUnresolved()
    {
        var (service, inner, context) = CreateService(resolved: false);

        await service.SignOutAsync(context, "scheme", null);

        inner.Verify(i => i.SignOutAsync(context, "scheme", null), Times.Once);
    }

    [Fact]
    public async Task SkipChallengeIfConfiguredAndTenantIsUnresolved()
    {
        var (service, inner, context) = CreateService(resolved: false, skipUnresolvedChallenge: true);

        await service.ChallengeAsync(context, "scheme", null);

        inner.Verify(i => i.ChallengeAsync(It.IsAny<HttpContext>(), It.IsAny<string?>(),
            It.IsAny<AuthenticationProperties?>()), Times.Never);
    }
}
