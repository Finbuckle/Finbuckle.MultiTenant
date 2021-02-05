using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

using Finbuckle.MultiTenant;

using Microsoft.AspNetCore.Http;
using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Primitives;

[assembly: FunctionsStartup(typeof(FunctionsDelegateStrategySample.Startup))]
namespace FunctionsDelegateStrategySample
{
    public class Startup : FunctionsStartup
    {
        public override void Configure(IFunctionsHostBuilder builder)
        {
            builder.Services.AddMultiTenant<TenantInfo>()
#if Release
                //.WithConfigurationStore()
#else
                .WithInMemoryStore(config =>
                {
                    config.Tenants.Add(new TenantInfo()
                    {
                        Id = "tenant-finbuckle-d043favoiaw",
                        Identifier = "finbuckle",
                        Name = "Finbuckle"
                    });
                    config.Tenants.Add(new TenantInfo()
                    {
                        Id = "tenant-initech-341ojadsfa",
                        Identifier = "initech",
                        Name = "Initech LLC"
                    });
                })
#endif
                .WithDelegateStrategy(context =>
                {
                    ((HttpContext)context).Request.Query.TryGetValue("tenant", out StringValues tenantId);
                    return Task.FromResult(tenantId.ToString());
                });

            builder.UseMultiTenant();
        }
    }
}
