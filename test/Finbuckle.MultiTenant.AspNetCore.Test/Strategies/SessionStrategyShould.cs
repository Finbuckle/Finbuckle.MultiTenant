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

using System;
using System.Threading.Tasks;
using Finbuckle.MultiTenant;
using Finbuckle.MultiTenant.Strategies;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

public class SessionStrategyShould
{
    private static IWebHostBuilder GetTestHostBuilder(string identifier, string sessionKey)
    {
        return new WebHostBuilder()
                    .ConfigureServices(services =>
                    {
                        services.AddDistributedMemoryCache();
                        services.AddSession(options =>
                        {
                            options.IdleTimeout = TimeSpan.FromSeconds(10);
                            options.Cookie.HttpOnly = true;
                            options.Cookie.IsEssential = true;
                        });

                        services.AddMultiTenant<TenantInfo>()
                                .WithStrategy<SessionStrategy>(ServiceLifetime.Singleton, sessionKey)
                                .WithInMemoryStore();
                        services.AddMvc();
                    })
                    .Configure(app =>
                    {
                        app.UseSession();
                        app.UseMultiTenant();
                        app.Run(async context =>
                        {
                            context.Session.SetString(sessionKey, identifier);
                            if (context.GetMultiTenantContext<TenantInfo>()?.TenantInfo != null)
                            {
                                await context.Response.WriteAsync(context.GetMultiTenantContext<TenantInfo>()?.TenantInfo.Id);
                            }
                        });

                        var store = app.ApplicationServices.GetRequiredService<IMultiTenantStore<TenantInfo>>();
                        store.TryAddAsync(new TenantInfo { Id = identifier, Identifier = identifier }).Wait();
                    });
    }

    [Fact]
    public void ThrowIfContextIsNotHttpContext()
    {
        var context = new Object();
        var strategy = new SessionStrategy("__tenant__");

        Assert.Throws<AggregateException>(() => strategy.GetIdentifierAsync(context).Result);
    }

    [Fact]
    public async Task ReturnNullIfNoSessionValue()
    {
        Action<IRouteBuilder> configRoutes = (IRouteBuilder rb) => rb.MapRoute("testRoute", "{controller}");
        IWebHostBuilder hostBuilder = GetTestHostBuilder("test_tenant", "__tenant__");

        using (var server = new TestServer(hostBuilder))
        {
            var client = server.CreateClient();
            var response = await client.GetStringAsync("/test_tenant");
            Assert.Equal("", response);
        }
    }

    // TO DO: Figure out how to test this
    // public async Task ReturnIdentifierIfSessionValue()
    // {
    // }
}
