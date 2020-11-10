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

#if NETCOREAPP2_1

using System;
using System.Threading.Tasks;
using Finbuckle.MultiTenant;
using Finbuckle.MultiTenant.Strategies;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Xunit;

public class RouteStrategyShould
{
    internal void configTestRoute(Microsoft.AspNetCore.Routing.IRouteBuilder routes)
    {
        routes.MapRoute("Defaut", "{__tenant__=}/{controller=Home}/{action=Index}");
    }

    [Theory]
    [InlineData("/initech", "initech", "initech")]
    [InlineData("/", "initech", "")]
    public async Task ReturnExpectedIdentifier(string path, string identifier, string expected)
    {
        Action<IRouteBuilder> configRoutes = (IRouteBuilder rb) => rb.MapRoute("testRoute", "{__tenant__}");
        IWebHostBuilder hostBuilder = GetTestHostBuilder(identifier, configRoutes);

        using (var server = new TestServer(hostBuilder))
        {
            var client = server.CreateClient();
            var response = await client.GetStringAsync(path);
            Assert.Equal(expected, response);
        }
    }

    [Fact]
    public void ThrowIfContextIsNotHttpContext()
    {
        var context = new Object();
        var adcp = new Mock<IActionDescriptorCollectionProvider>().Object;
        var strategy = new RouteStrategy("__tenant__", configTestRoute, adcp);

        Assert.Throws<AggregateException>(() => strategy.GetIdentifierAsync(context).Result);
    }

    [Fact]
    public async Task ReturnNullIfNoRouteParamMatch()
    {
        Action<IRouteBuilder> configRoutes = (IRouteBuilder rb) => rb.MapRoute("testRoute", "{controller}");
        IWebHostBuilder hostBuilder = GetTestHostBuilder("test_tenant", configRoutes);

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
    public void ThrowIfRouteParamIsNullOrWhitespace(string testString)
    {
        var adcp = new Mock<IActionDescriptorCollectionProvider>().Object;
        Assert.Throws<ArgumentException>(() => new RouteStrategy(testString, configTestRoute, adcp));
    }

    private static IWebHostBuilder GetTestHostBuilder(string identifier, Action<IRouteBuilder> configRoutes)
    {
        return new WebHostBuilder()
                    .ConfigureServices(services =>
                    {
                        services.AddMultiTenant<TenantInfo>().WithRouteStrategy(configRoutes).WithInMemoryStore();
                        services.AddMvc();
                    })
                    .Configure(app =>
                    {
                        app.UseMultiTenant();
                        app.Run(async context =>
                        {
                            if (context.GetMultiTenantContext<TenantInfo>()?.TenantInfo != null)
                            {
                                await context.Response.WriteAsync(context.GetMultiTenantContext<TenantInfo>()?.TenantInfo.Id);
                            }
                        });

                        var store = app.ApplicationServices.GetRequiredService<IMultiTenantStore<TenantInfo>>();
                        store.TryAddAsync(new TenantInfo { Id = identifier, Identifier = identifier }).Wait();
                    });
    }
}

#else

using System;
using System.Threading.Tasks;
using Finbuckle.MultiTenant;
using Finbuckle.MultiTenant.Strategies;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

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
    public void ThrowIfContextIsNotHttpContext()
    {
        var context = new Object();
        var strategy = new RouteStrategy("__tenant__");

        Assert.Throws<AggregateException>(() => strategy.GetIdentifierAsync(context).Result);
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
    public void ThrowIfRouteParamIsNullOrWhitespace(string testString)
    {
        Assert.Throws<ArgumentException>(() => new RouteStrategy(testString));
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
                                    await context.Response.WriteAsync(context.GetMultiTenantContext<TenantInfo>().TenantInfo.Id);
                                }
                            });
                        });

                        var store = app.ApplicationServices.GetRequiredService<IMultiTenantStore<TenantInfo>>();
                        store.TryAddAsync(new TenantInfo { Id = identifier, Identifier = identifier }).Wait();
                    });
    }
}

#endif