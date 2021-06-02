
using Finbuckle.MultiTenant;

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
                .WithConfigurationStore()
                .WithStaticStrategy("finbuckle");
        }
    }
}
