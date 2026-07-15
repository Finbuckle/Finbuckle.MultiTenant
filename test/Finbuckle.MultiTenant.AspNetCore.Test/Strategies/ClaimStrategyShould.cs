// Copyright Finbuckle LLC, Andrew White, and Contributors.
// Refer to the solution LICENSE file for more information.

using System.Security.Claims;
using Finbuckle.MultiTenant.Abstractions;
using Finbuckle.MultiTenant.AspNetCore.Strategies;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Xunit;

namespace Finbuckle.MultiTenant.AspNetCore.Test.Strategies;

public class ClaimStrategyShould
{
    private static HttpContext CreateAuthenticatedHttpContextMock(params Claim[] claims)
    {
        var identity = new ClaimsIdentity(claims, "TestAuthType");
        var mock = new Mock<HttpContext>();
        mock.Setup(c => c.User).Returns(new ClaimsPrincipal(identity));

        return mock.Object;
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    public void ThrowIfTemplateIsNullOrWhitespace(string? template)
    {
        // ArgumentException.ThrowIfNullOrWhiteSpace throws ArgumentNullException for null input,
        // which derives from ArgumentException.
        Assert.ThrowsAny<ArgumentException>(() => new ClaimStrategy(template!));
    }

    [Fact]
    public async Task ReturnClaimValueIfUserAuthenticatedAndClaimPresent()
    {
        var httpContext = CreateAuthenticatedHttpContextMock(new Claim("tenant", "initech"));
        var strategy = new ClaimStrategy("tenant");

        var identifier = await strategy.GetIdentifierAsync(httpContext);

        Assert.Equal("initech", identifier);
    }

    [Fact]
    public async Task ReturnNullIfUserAuthenticatedButClaimMissing()
    {
        var httpContext = CreateAuthenticatedHttpContextMock(new Claim("other", "value"));
        var strategy = new ClaimStrategy("tenant");

        var identifier = await strategy.GetIdentifierAsync(httpContext);

        Assert.Null(identifier);
    }

    [Fact]
    public async Task ReturnNullIfContextIsNotHttpContext()
    {
        var context = new object();
        var strategy = new ClaimStrategy("tenant");

        Assert.Null(await strategy.GetIdentifierAsync(context));
    }

    [Fact]
    public async Task ReturnNullIfUnauthenticatedAndNoDefaultAuthenticationScheme()
    {
        var schemeProvider = new Mock<IAuthenticationSchemeProvider>();
        schemeProvider.Setup(s => s.GetDefaultAuthenticateSchemeAsync())
            .ReturnsAsync((AuthenticationScheme?)null);

        var services = new ServiceCollection();
        services.AddSingleton(schemeProvider.Object);
        var sp = services.BuildServiceProvider();

        var mock = new Mock<HttpContext>();
        mock.Setup(c => c.User).Returns(new ClaimsPrincipal(new ClaimsIdentity()));
        mock.Setup(c => c.RequestServices).Returns(sp);

        var strategy = new ClaimStrategy("tenant");

        var identifier = await strategy.GetIdentifierAsync(mock.Object);

        Assert.Null(identifier);
        schemeProvider.Verify(s => s.GetDefaultAuthenticateSchemeAsync(), Times.Once);
    }

    [Fact]
    public async Task ReturnNullIfUnauthenticatedAndSpecifiedAuthenticationSchemeNotFound()
    {
        var schemeProvider = new Mock<IAuthenticationSchemeProvider>();
        schemeProvider.Setup(s => s.GetAllSchemesAsync())
            .ReturnsAsync(Enumerable.Empty<AuthenticationScheme>());

        var services = new ServiceCollection();
        services.AddSingleton(schemeProvider.Object);
        var sp = services.BuildServiceProvider();

        var mock = new Mock<HttpContext>();
        mock.Setup(c => c.User).Returns(new ClaimsPrincipal(new ClaimsIdentity()));
        mock.Setup(c => c.RequestServices).Returns(sp);

        var strategy = new ClaimStrategy("tenant", "customScheme");

        var identifier = await strategy.GetIdentifierAsync(mock.Object);

        Assert.Null(identifier);
        schemeProvider.Verify(s => s.GetDefaultAuthenticateSchemeAsync(), Times.Never);
    }

    [Fact]
    public async Task ReturnClaimValueFromAuthenticatedHandlerWhenUnauthenticated()
    {
        var schemeProvider = new Mock<IAuthenticationSchemeProvider>();
        var scheme = new AuthenticationScheme("customScheme", "customScheme", typeof(FakeAuthHandler));
        schemeProvider.Setup(s => s.GetAllSchemesAsync())
            .ReturnsAsync(new[] { scheme }.AsEnumerable());

        var services = new ServiceCollection();
        services.AddSingleton(schemeProvider.Object);
        services.AddSingleton(new FakeClaimsProvider { Claims = [new Claim("tenant", "initech")] });
        var sp = services.BuildServiceProvider();

        var itemsDict = new Dictionary<object, object?>();
        var mock = new Mock<HttpContext>();
        mock.Setup(c => c.User).Returns(new ClaimsPrincipal(new ClaimsIdentity()));
        mock.Setup(c => c.RequestServices).Returns(sp);
        mock.Setup(c => c.Items).Returns(itemsDict);

        var strategy = new ClaimStrategy("tenant", "customScheme");

        var identifier = await strategy.GetIdentifierAsync(mock.Object);

        Assert.Equal("initech", identifier);
        // The bypass-validate-principal marker should be cleaned up after use.
        Assert.False(itemsDict.ContainsKey($"{Constants.TenantToken}__bypass_validate_principal__"));
    }

    [Fact]
    public async Task ReturnNullFromAuthenticatedHandlerWhenNoMatchingClaim()
    {
        var schemeProvider = new Mock<IAuthenticationSchemeProvider>();
        var scheme = new AuthenticationScheme("customScheme", "customScheme", typeof(FakeAuthHandler));
        schemeProvider.Setup(s => s.GetDefaultAuthenticateSchemeAsync()).ReturnsAsync(scheme);

        var services = new ServiceCollection();
        services.AddSingleton(schemeProvider.Object);
        services.AddSingleton(new FakeClaimsProvider { Claims = [new Claim("other", "value")] });
        var sp = services.BuildServiceProvider();

        var itemsDict = new Dictionary<object, object?>();
        var mock = new Mock<HttpContext>();
        mock.Setup(c => c.User).Returns(new ClaimsPrincipal(new ClaimsIdentity()));
        mock.Setup(c => c.RequestServices).Returns(sp);
        mock.Setup(c => c.Items).Returns(itemsDict);

        // No explicit scheme name, so the default authenticate scheme is used.
        var strategy = new ClaimStrategy("tenant");

        var identifier = await strategy.GetIdentifierAsync(mock.Object);

        Assert.Null(identifier);
    }

    private class FakeClaimsProvider
    {
        public Claim[] Claims { get; set; } = [];
    }

    private class FakeAuthHandler(FakeClaimsProvider claimsProvider) : IAuthenticationHandler
    {
        public Task InitializeAsync(AuthenticationScheme scheme, HttpContext context) => Task.CompletedTask;

        public Task<AuthenticateResult> AuthenticateAsync()
        {
            var identity = new ClaimsIdentity(claimsProvider.Claims, "FakeAuthType");
            var principal = new ClaimsPrincipal(identity);
            var ticket = new AuthenticationTicket(principal, "customScheme");
            return Task.FromResult(AuthenticateResult.Success(ticket));
        }

        public Task ChallengeAsync(AuthenticationProperties? properties) => Task.CompletedTask;

        public Task ForbidAsync(AuthenticationProperties? properties) => Task.CompletedTask;
    }
}
