using System;
using System.Collections.Generic;
using System.Text;

using Finbuckle.MultiTenant;

using Microsoft.AspNetCore.Http;
using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;

[assembly: FunctionsStartup(typeof(FunctionsBasePathStrategySample.Startup))]
namespace FunctionsBasePathStrategySample
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
                })
#endif
                .WithStaticStrategy("finbuckle");

            builder.UseMultiTenant();
        }
    }
}
