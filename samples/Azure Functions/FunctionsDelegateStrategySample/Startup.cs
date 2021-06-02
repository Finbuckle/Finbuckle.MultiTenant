using Finbuckle.MultiTenant;

using Microsoft.AspNetCore.Http;
using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Primitives;

using System.Threading.Tasks;

[assembly: FunctionsStartup(typeof(FunctionsDelegateStrategySample.Startup))]
namespace FunctionsDelegateStrategySample
{
    public class Startup : FunctionsStartup
    {
        public override void Configure(IFunctionsHostBuilder builder)
        {
            builder.Services.AddMultiTenant<TenantInfo>()
                .WithConfigurationStore()
                .WithDelegateStrategy(async context =>
                {
                    ((HttpContext)context).Request.Query.TryGetValue("tenant", out StringValues tenantId);
                    return await Task.FromResult(tenantId.ToString()); // ignore await warning or use await Task.FromResult(...)
                });
        }
    }
}
