// Copyright Finbuckle LLC, Andrew White, and Contributors.
// Refer to the solution LICENSE file for more information.

using System.Text.Encodings.Web;
using Finbuckle.MultiTenant.Abstractions;
using Finbuckle.MultiTenant.AspNetCore.Extensions;
using Finbuckle.MultiTenant.AspNetCore.Strategies;
using Finbuckle.MultiTenant.Extensions;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using Xunit;
using RemoteAuthenticationCallbackStrategyShould =
    Finbuckle.MultiTenant.AspNetCore.Test.Strategies.RemoteAuthenticationCallbackStrategyShould;

namespace Finbuckle.MultiTenant.AspNetCore.Test;

public class AuthenticationPipelineShould
{
    public sealed class CaptureChallengeHandler(
        IOptionsMonitor<AuthenticationSchemeOptions> options,
        ILoggerFactory logger,
        UrlEncoder encoder)
        : AuthenticationHandler<AuthenticationSchemeOptions>(options, logger, encoder)
    {
        protected override Task<AuthenticateResult> HandleAuthenticateAsync() =>
            Task.FromResult(AuthenticateResult.NoResult());

        protected override Task HandleChallengeAsync(AuthenticationProperties properties)
        {
            properties.Items.TryGetValue(Constants.TenantToken, out var identifier);
            return Response.WriteAsync(identifier ?? "missing");
        }
    }

    [Fact]
    public async Task AddTenantIdentifierToChallengeProperties()
    {
        using var host = await new HostBuilder()
            .ConfigureWebHost(webBuilder => webBuilder
                .UseTestServer()
                .ConfigureServices(services =>
                {
                    services.AddRouting();
                    services.AddAuthentication("capture")
                        .AddScheme<AuthenticationSchemeOptions, CaptureChallengeHandler>("capture", _ => { });
                    services.AddMultiTenant<TenantInfo>()
                        .WithStaticStrategy("initech")
                        .WithInMemoryStore()
                        .WithPerTenantAuthenticationCore();
                })
                .Configure(app =>
                {
                    app.UseRouting();
                    app.UseMultiTenant();
                    app.UseAuthentication();
                    app.UseEndpoints(endpoints =>
                        endpoints.Map("/challenge", context => context.ChallengeAsync("capture")));
                }))
            .StartAsync();
        using (var scope = host.Services.CreateScope())
        {
            await scope.ServiceProvider.GetRequiredService<TenantManager<TenantInfo>>()
                .AddAsync(new TenantInfo { Id = "initech-id", Identifier = "initech" });
        }

        var response = await host.GetTestClient().GetStringAsync("/challenge");

        Assert.Equal("initech", response);
    }

    [Fact]
    public async Task ResolveTenantFromRemoteCallbackState()
    {
        var stateDataFormat = new Mock<ISecureDataFormat<AuthenticationProperties>>();
        var properties = new AuthenticationProperties();
        properties.Items[Constants.TenantToken] = "initech";
        stateDataFormat.Setup(f => f.Unprotect("protected-state")).Returns(properties);

        var options = new RemoteAuthenticationCallbackStrategyShould.FakeRemoteOptions
        {
            CallbackPath = "/signin-remote",
            SignedOutCallbackPath = "/signout-remote",
            StateDataFormat = stateDataFormat.Object
        };
        var optionsMonitor = new Mock<IOptionsMonitor<RemoteAuthenticationCallbackStrategyShould.FakeRemoteOptions>>();
        optionsMonitor.Setup(o => o.Get("remote")).Returns(options);
        var schemeProvider = new Mock<IAuthenticationSchemeProvider>();
        schemeProvider.Setup(s => s.GetRequestHandlerSchemesAsync()).ReturnsAsync([
            new AuthenticationScheme("remote", null,
                typeof(RemoteAuthenticationCallbackStrategyShould.FakeRemoteHandler))
        ]);

        using var host = await new HostBuilder()
            .ConfigureWebHost(webBuilder => webBuilder
                .UseTestServer()
                .ConfigureServices(services =>
                {
                    services.AddLogging();
                    services.AddSingleton<IAuthenticationSchemeProvider>(schemeProvider.Object);
                    services.AddSingleton<IOptionsMonitor<RemoteAuthenticationCallbackStrategyShould.FakeRemoteOptions>>(
                        optionsMonitor.Object);
                    services.AddMultiTenant<TenantInfo>()
                        .WithRemoteAuthenticationCallbackStrategy()
                        .WithInMemoryStore();
                })
                .Configure(app =>
                {
                    app.UseMultiTenant();
                    app.Run(context => context.Response.WriteAsync(
                        context.GetTenantInfo<TenantInfo>()?.Identifier ?? "unresolved"));
                }))
            .StartAsync();
        using (var scope = host.Services.CreateScope())
        {
            await scope.ServiceProvider.GetRequiredService<TenantManager<TenantInfo>>()
                .AddAsync(new TenantInfo { Id = "initech-id", Identifier = "initech" });
        }

        var response = await host.GetTestClient().GetStringAsync("/signin-remote?state=protected-state");

        Assert.Equal("initech", response);
    }
}
