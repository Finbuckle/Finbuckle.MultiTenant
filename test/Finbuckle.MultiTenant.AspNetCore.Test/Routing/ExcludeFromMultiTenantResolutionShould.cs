// Copyright Finbuckle LLC, Andrew White, and Contributors.
// Refer to the solution LICENSE file for more information.

using Finbuckle.MultiTenant.Abstractions;
using Finbuckle.MultiTenant.AspNetCore.Extensions;
using Finbuckle.MultiTenant.Extensions;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Xunit;

namespace Finbuckle.MultiTenant.AspNetCore.Test.Routing;

public class ExcludeFromMultiTenantResolutionShould
{
    private const string NoTenantResponse = "No tenant available.";

    private static async Task<IHost> CreateHostAsync(string identifier)
    {
        var hostBuilder = new HostBuilder()
            .ConfigureWebHost(webBuilder => webBuilder
                .UseTestServer()
                .ConfigureServices(services =>
                {
                    services.AddRouting();
                    services.AddMultiTenant<TenantInfo>()
                        .WithStaticStrategy(identifier)
                        .WithInMemoryStore();
                })
                .Configure(app =>
                {
                    app.UseRouting();
                    app.UseMultiTenant();

                    app.UseEndpoints(endpoints =>
                    {
                        endpoints.Map("/excluded", WriteResponseAsync).ExcludeFromMultiTenantResolution();
                        endpoints.Map("/included", WriteResponseAsync);
                    });
                }));

        var host = await hostBuilder.StartAsync();
        using var scope = host.Services.CreateScope();
        await scope.ServiceProvider.GetRequiredService<TenantManager<TenantInfo>>()
            .AddAsync(new TenantInfo { Id = identifier, Identifier = identifier });
        return host;
    }

    private static Task WriteResponseAsync(HttpContext context)
    {
        var tenantContext = context.GetTenantContext<TenantInfo>();
        return context.Response.WriteAsync(
            tenantContext.TenantInfo is null ? NoTenantResponse : tenantContext.TenantInfo.Id);
    }

    [Fact]
    public async Task SkipTenantResolutionForExcludedEndpoint()
    {
        using var host = await CreateHostAsync("initech");
        var client = host.GetTestClient();

        var response = await client.GetStringAsync("/excluded");

        Assert.Equal(NoTenantResponse, response);
    }

    [Fact]
    public async Task ResolveTenantForNonExcludedEndpoint()
    {
        using var host = await CreateHostAsync("initech");
        var client = host.GetTestClient();

        var response = await client.GetStringAsync("/included");

        Assert.Equal("initech", response);
    }
}
