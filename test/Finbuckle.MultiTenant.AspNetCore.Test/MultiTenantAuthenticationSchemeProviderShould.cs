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
using Microsoft.Extensions.DependencyInjection;
using Xunit;
using Microsoft.AspNetCore.Authentication;
using Finbuckle.MultiTenant;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Routing;

public class MultiTenantAuthenticationSchemeProviderShould
{
    private static IWebHostBuilder GetTestHostBuilder()
    {
        return new WebHostBuilder()
                    .ConfigureServices(services =>
                    {
                        services.AddAuthentication()
                            .AddCookie("tenant1Scheme")
                            .AddCookie("tenant2Scheme");
                            
                        services.AddMultiTenant<TenantInfo>()
                            .WithBasePathStrategy()
                            .WithPerTenantAuthentication()
                            .WithInMemoryStore()
                            .WithPerTenantOptions<AuthenticationOptions>((ao, ti) =>
                            {
                                ao.DefaultChallengeScheme = ti.Identifier + "Scheme";
                            });

                        services.AddMvc();
                    })
                    .Configure(app =>
                    {
                        app.UseMultiTenant();
                        app.Run(async context =>
                        {
                            if (context.GetMultiTenantContext<TenantInfo>().TenantInfo != null)
                            {
                                var schemeProvider = context.RequestServices.GetRequiredService<IAuthenticationSchemeProvider>();
                                await context.Response.WriteAsync((await schemeProvider.GetDefaultChallengeSchemeAsync()).Name);
                            }
                        });

                        var store = app.ApplicationServices.GetRequiredService<IMultiTenantStore<TenantInfo>>();
                        store.TryAddAsync(new TenantInfo { Id = "tenant1", Identifier = "tenant1" }).Wait();
                        store.TryAddAsync(new TenantInfo { Id = "tenant2", Identifier = "tenant2" }).Wait();
                    });
    }

    [Fact]
    public async Task ReturnPerTenantAuthenticationOptions()
    {
        Action<IRouteBuilder> configRoutes = (IRouteBuilder rb) => rb.MapRoute("testRoute", "{controller}");
        IWebHostBuilder hostBuilder = GetTestHostBuilder();

        using (var server = new TestServer(hostBuilder))
        {
            var client = server.CreateClient();
            var response = await client.GetStringAsync("/tenant1");
            Assert.Equal("tenant1Scheme", response);

            response = await client.GetStringAsync("/tenant2");
            Assert.Equal("tenant2Scheme", response);
        }
    }
}