// Copyright Finbuckle LLC, Andrew White, and Contributors.
// Refer to the solution LICENSE file for more information.

using System.Text;
using Finbuckle.MultiTenant.Abstractions;
using Finbuckle.MultiTenant.AspNetCore.Strategies;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using Xunit;

namespace Finbuckle.MultiTenant.AspNetCore.Test.Strategies;

public class RemoteAuthenticationCallbackStrategyShould
{
    public sealed class FakeRemoteOptions : RemoteAuthenticationOptions
    {
        public PathString SignedOutCallbackPath { get; set; }
        public required ISecureDataFormat<AuthenticationProperties> StateDataFormat { get; set; }
    }

    public sealed class FakeRemoteHandler : IAuthenticationRequestHandler
    {
        public FakeRemoteOptions Options { get; } = null!;

        public Task<AuthenticateResult> AuthenticateAsync() => throw new NotImplementedException();
        public Task ChallengeAsync(AuthenticationProperties? properties) => throw new NotImplementedException();
        public Task ForbidAsync(AuthenticationProperties? properties) => throw new NotImplementedException();
        public Task InitializeAsync(AuthenticationScheme scheme, HttpContext context) => throw new NotImplementedException();
        public Task<bool> HandleRequestAsync() => throw new NotImplementedException();
    }

    public sealed class RequestHandlerWithoutOptions : IAuthenticationRequestHandler
    {
        public Task<AuthenticateResult> AuthenticateAsync() => throw new NotImplementedException();
        public Task ChallengeAsync(AuthenticationProperties? properties) => throw new NotImplementedException();
        public Task ForbidAsync(AuthenticationProperties? properties) => throw new NotImplementedException();
        public Task InitializeAsync(AuthenticationScheme scheme, HttpContext context) => throw new NotImplementedException();
        public Task<bool> HandleRequestAsync() => throw new NotImplementedException();
    }

    private static (RemoteAuthenticationCallbackStrategy Strategy, DefaultHttpContext Context,
        Mock<ISecureDataFormat<AuthenticationProperties>> StateDataFormat) CreateStrategy(
        string requestPath = "/signin-remote", string method = "GET", bool includeSecondScheme = false)
    {
        var stateDataFormat = new Mock<ISecureDataFormat<AuthenticationProperties>>();
        var options = new FakeRemoteOptions
        {
            CallbackPath = "/signin-remote",
            SignedOutCallbackPath = "/signout-remote",
            StateDataFormat = stateDataFormat.Object
        };
        var optionsMonitor = new Mock<IOptionsMonitor<FakeRemoteOptions>>();
        optionsMonitor.Setup(o => o.Get(It.IsAny<string>())).Returns(options);

        var schemes = new List<AuthenticationScheme>();
        if (includeSecondScheme)
            schemes.Add(new AuthenticationScheme("first", null, typeof(RequestHandlerWithoutOptions)));
        schemes.Add(new AuthenticationScheme("remote", null, typeof(FakeRemoteHandler)));

        var schemeProvider = new Mock<IAuthenticationSchemeProvider>();
        schemeProvider.Setup(s => s.GetRequestHandlerSchemesAsync()).ReturnsAsync(schemes);

        var services = new ServiceCollection();
        services.AddSingleton(schemeProvider.Object);
        services.AddSingleton(optionsMonitor.Object);
        var context = new DefaultHttpContext
        {
            RequestServices = services.BuildServiceProvider()
        };
        context.Request.Path = requestPath;
        context.Request.Method = method;

        return (new RemoteAuthenticationCallbackStrategy(
            Mock.Of<ILogger<RemoteAuthenticationCallbackStrategy>>()), context, stateDataFormat);
    }

    private static AuthenticationProperties CreateProperties(string? identifier = "initech")
    {
        var properties = new AuthenticationProperties();
        if (identifier is not null)
            properties.Items[Constants.TenantToken] = identifier;
        return properties;
    }

    [Fact]
    public void HavePriorityNeg900()
    {
        var strategy = new RemoteAuthenticationCallbackStrategy(null!);
        Assert.Equal(-900, strategy.Priority);
    }

    [Fact]
    public async Task ReturnNullIfContextIsNotHttpContext()
    {
        var context = new object();
        var strategy = new RemoteAuthenticationCallbackStrategy(null!);

        Assert.Null(await strategy.GetIdentifierAsync(context));
    }

    [Fact]
    public async Task ResolveIdentifierFromGetCallbackState()
    {
        var (strategy, context, stateDataFormat) = CreateStrategy();
        context.Request.QueryString = new QueryString("?state=protected-state");
        stateDataFormat.Setup(f => f.Unprotect("protected-state")).Returns(CreateProperties());

        Assert.Equal("initech", await strategy.GetIdentifierAsync(context));
    }

    [Fact]
    public async Task ResolveIdentifierFromPostCallbackState()
    {
        var (strategy, context, stateDataFormat) = CreateStrategy(method: "POST");
        context.Request.ContentType = "application/x-www-form-urlencoded";
        context.Request.Body = new MemoryStream(Encoding.UTF8.GetBytes("state=protected-state"));
        stateDataFormat.Setup(f => f.Unprotect("protected-state")).Returns(CreateProperties());

        Assert.Equal("initech", await strategy.GetIdentifierAsync(context));
    }

    [Fact]
    public async Task ResolveIdentifierFromSignedOutCallbackState()
    {
        var (strategy, context, stateDataFormat) = CreateStrategy("/signout-remote");
        context.Request.QueryString = new QueryString("?state=protected-state");
        stateDataFormat.Setup(f => f.Unprotect("protected-state")).Returns(CreateProperties());

        Assert.Equal("initech", await strategy.GetIdentifierAsync(context));
    }

    [Fact]
    public async Task ContinueToSupportedRemoteScheme()
    {
        var (strategy, context, stateDataFormat) = CreateStrategy(includeSecondScheme: true);
        context.Request.QueryString = new QueryString("?state=protected-state");
        stateDataFormat.Setup(f => f.Unprotect("protected-state")).Returns(CreateProperties());

        Assert.Equal("initech", await strategy.GetIdentifierAsync(context));
    }

    [Theory]
    [InlineData("GET")]
    [InlineData("POST")]
    public async Task ReturnNullIfCallbackStateMissing(string method)
    {
        var (strategy, context, stateDataFormat) = CreateStrategy(method: method);
        if (method == "POST")
        {
            context.Request.ContentType = "application/x-www-form-urlencoded";
            context.Request.Body = new MemoryStream(Encoding.UTF8.GetBytes("other=value"));
        }

        Assert.Null(await strategy.GetIdentifierAsync(context));
        stateDataFormat.Verify(f => f.Unprotect(It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task ReturnNullForUnrelatedPath()
    {
        var (strategy, context, stateDataFormat) = CreateStrategy("/unrelated");
        context.Request.QueryString = new QueryString("?state=protected-state");

        Assert.Null(await strategy.GetIdentifierAsync(context));
        stateDataFormat.Verify(f => f.Unprotect(It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task ReturnNullIfStateDoesNotContainAuthenticationProperties()
    {
        var (strategy, context, stateDataFormat) = CreateStrategy();
        context.Request.QueryString = new QueryString("?state=protected-state");
        stateDataFormat.Setup(f => f.Unprotect("protected-state")).Returns((AuthenticationProperties?)null);

        Assert.Null(await strategy.GetIdentifierAsync(context));
    }

    [Fact]
    public async Task ReturnNullIfAuthenticationPropertiesDoNotContainTenant()
    {
        var (strategy, context, stateDataFormat) = CreateStrategy();
        context.Request.QueryString = new QueryString("?state=protected-state");
        stateDataFormat.Setup(f => f.Unprotect("protected-state")).Returns(CreateProperties(null));

        Assert.Null(await strategy.GetIdentifierAsync(context));
    }

    [Fact]
    public async Task WrapInvalidProtectedStateException()
    {
        var (strategy, context, stateDataFormat) = CreateStrategy();
        context.Request.QueryString = new QueryString("?state=invalid");
        var innerException = new InvalidOperationException("invalid state");
        stateDataFormat.Setup(f => f.Unprotect("invalid")).Throws(innerException);

        var exception = await Assert.ThrowsAsync<MultiTenantException>(() => strategy.GetIdentifierAsync(context));

        Assert.Same(innerException, exception.InnerException);
    }
}
