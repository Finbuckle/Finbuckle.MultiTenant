// Copyright Finbuckle LLC, Andrew White, and Contributors.
// Refer to the solution LICENSE file for more information.

using System;
using System.Threading.Tasks;
using Finbuckle.MultiTenant.AspNetCore.Extensions;
using Finbuckle.MultiTenant.DependencyInjection;
using Finbuckle.MultiTenant.Strategies;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Finbuckle.MultiTenant.AspNetCore.Test.Strategies;

public class RouteStrategyShould
{
    [Theory]
    [InlineData("/initech", "initech", "initech")]
    [InlineData("/", "initech", "")]
    public async Task ReturnExpectedIdentifier(string path, string identifier, string expected)
    {
        IWebHostBuilder hostBuilder = GetTestHostBuilder(identifier, "{__tenant__=}");

        using (var server = new TestServer(hostBuilder))
        {
            var client = server.CreateClient();
            var response = await client.GetStringAsync(path);
            Assert.Equal(expected, response);
        }
    }

    [Fact]
    public async void ThrowIfContextIsNotHttpContext()
    {
        var context = new Object();
        var strategy = new RouteStrategy("__tenant__");

        await Assert.ThrowsAsync<MultiTenantException>(() => strategy.GetIdentifierAsync(context));
    }

    [Fact]
    public async Task ReturnNullIfNoRouteParamMatch()
    {
        IWebHostBuilder hostBuilder = GetTestHostBuilder("test_tenant", "{controller}");

        using (var server = new TestServer(hostBuilder))
        {
            var client = server.CreateClient();
            var response = await client.GetStringAsync("/test_tenant");
            Assert.Equal("", response);
        }
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    public void ThrowIfRouteParamIsNullOrWhitespace(string? testString)
    {
        Assert.Throws<ArgumentException>(() =>
            new RouteStrategy(testString!));
    }

    private static IWebHostBuilder GetTestHostBuilder(string identifier, string routePattern)
    {
        return new WebHostBuilder()
            .ConfigureServices(services =>
            {
                services.AddMultiTenant<TenantInfo>().WithRouteStrategy().WithInMemoryStore();
                services.AddMvc();
            })
            .Configure(app =>
            {
                app.UseRouting();
                app.UseMultiTenant();
                app.UseEndpoints(endpoints =>
                {
                    endpoints.Map(routePattern, async context =>
                    {
                        if (context.GetMultiTenantContext<TenantInfo>()?.TenantInfo != null)
                        {
                            await context.Response.WriteAsync(context.GetMultiTenantContext<TenantInfo>()!
                                .TenantInfo!.Id!);
                        }
                    });
                });

                var store = app.ApplicationServices.GetRequiredService<IMultiTenantStore<TenantInfo>>();
                store.TryAddAsync(new TenantInfo { Id = identifier, Identifier = identifier }).Wait();
            });
    }
}