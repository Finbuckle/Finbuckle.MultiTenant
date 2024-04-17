// Copyright Finbuckle LLC, Andrew White, and Contributors.
// Refer to the solution LICENSE file for more information.

using System;
using System.Threading.Tasks;
using Finbuckle.MultiTenant.DependencyInjection;
using Finbuckle.MultiTenant.Strategies;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Finbuckle.MultiTenant.AspNetCore.Test.Strategies
{
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
                            await context.Response.WriteAsync(context.GetMultiTenantContext<TenantInfo>()!.TenantInfo!.Id!);
                        }
                    });

                    var store = app.ApplicationServices.GetRequiredService<IMultiTenantStore<TenantInfo>>();
                    store.TryAddAsync(new TenantInfo { Id = identifier, Identifier = identifier }).Wait();
                });
        }

        [Fact]
        public async void ThrowIfContextIsNotHttpContext()
        {
            var context = new Object();
            var strategy = new SessionStrategy("__tenant__");

            await Assert.ThrowsAsync<MultiTenantException>(() => strategy.GetIdentifierAsync(context));
        }

        [Fact]
        public async Task ReturnNullIfNoSessionValue()
        {
            var hostBuilder = GetTestHostBuilder("test_tenant", "__tenant__");

            using (var server = new TestServer(hostBuilder))
            {
                var client = server.CreateClient();
                var response = await client.GetStringAsync("/test_tenant");
                Assert.Equal("", response);
            }
        }

        // TODO: Figure out how to test this
        // public async Task ReturnIdentifierIfSessionValue()
        // {
        // }
    }
}
