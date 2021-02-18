using System;
using System.Collections.Generic;
using System.Text;

using Finbuckle.MultiTenant;

using Microsoft.AspNetCore.Http;
using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;

[assembly: FunctionsStartup(typeof(FunctionsHostStrategySample.Startup))]
namespace FunctionsHostStrategySample
{
    public class Startup : FunctionsStartup
    {
        public override void Configure(IFunctionsHostBuilder builder)
        {
            builder.Services.AddMultiTenant<TenantInfo>()
                .WithConfigurationStore()
                .WithHostStrategy();
        }
    }
}
