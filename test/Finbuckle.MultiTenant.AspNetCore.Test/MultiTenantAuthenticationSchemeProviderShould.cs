// Copyright Finbuckle LLC, Andrew White, and Contributors.
// Refer to the solution LICENSE file for more inforation.

using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Finbuckle.MultiTenant.AspNetCore.Test
{
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
                        if (context.GetMultiTenantContext<TenantInfo>()?.TenantInfo != null)
                        {
                            var schemeProvider = context.RequestServices.GetRequiredService<IAuthenticationSchemeProvider>();
                            await context.Response.WriteAsync((await schemeProvider.GetDefaultChallengeSchemeAsync())!.Name);
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
            var hostBuilder = GetTestHostBuilder();

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
}