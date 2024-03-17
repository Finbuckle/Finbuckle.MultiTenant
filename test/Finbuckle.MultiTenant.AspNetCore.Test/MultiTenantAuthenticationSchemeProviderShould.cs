// Copyright Finbuckle LLC, Andrew White, and Contributors.
// Refer to the solution LICENSE file for more information.

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
        [Fact]
        public async Task ReturnPerTenantAuthenticationOptions()
        {
            // var hostBuilder = GetTestHostBuilder();
            //
            // using (var server = new TestServer(hostBuilder))
            // {
            //     var client = server.CreateClient();
            //     var response = await client.GetStringAsync("/tenant1");
            //     Assert.Equal("tenant1Scheme", response);
            //
            //     response = await client.GetStringAsync("/tenant2");
            //     Assert.Equal("tenant2Scheme", response);
            // }

            var services = new ServiceCollection();
            services.AddAuthentication()
                .AddCookie("tenant1Scheme")
                .AddCookie("tenant2Scheme");

            services.AddMultiTenant<TenantInfo>()
                .WithPerTenantAuthentication();

            services.ConfigureAllPerTenant<AuthenticationOptions, TenantInfo>((ao, ti) =>
            {
                ao.DefaultChallengeScheme = ti.Identifier + "Scheme";
            });

            var sp = services.BuildServiceProvider();

            var tenant1 = new TenantInfo{
                Id = "tenant1",
                Identifier = "tenant1"
            };
            
            var tenant2 = new TenantInfo{
                Id = "tenant2",
                Identifier = "tenant2"
            };
            
            var mtc = new MultiTenantContext<TenantInfo>();
            var multiTenantContextAccessor = sp.GetRequiredService<IMultiTenantContextAccessor<TenantInfo>>();
            multiTenantContextAccessor.MultiTenantContext = mtc;

            mtc.TenantInfo = tenant1;
            var schemeProvider = sp.GetRequiredService<IAuthenticationSchemeProvider>();
            
            var option = schemeProvider.GetDefaultChallengeSchemeAsync().Result;
            Assert.Equal("tenant1Scheme", option?.Name);

            mtc.TenantInfo = tenant2;
            option = schemeProvider.GetDefaultChallengeSchemeAsync().Result;
            Assert.Equal("tenant2Scheme", option?.Name);
        }
    }
}