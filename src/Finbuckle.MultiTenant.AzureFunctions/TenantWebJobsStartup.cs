using Finbuckle.MultiTenant.AspNetCore;
using Finbuckle.MultiTenant.AzureFunctions;
using Finbuckle.MultiTenant.AzureFunctions.Config;
using Finbuckle.MultiTenant.AzureFunctions.Host.Middleware;

using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Hosting;

using System;
using System.Collections.Generic;
using System.Text;

[assembly: WebJobsStartup(typeof(TenantWebJobsStartup))]
namespace Finbuckle.MultiTenant.AzureFunctions
{
    public class TenantWebJobsStartup : IWebJobsStartup
    {
        public void Configure(IWebJobsBuilder builder)
        {
            builder.AddTenantBindings();

            builder.Services.AddHttpMiddleware(nameof(MultiTenantMiddleware), async (context, next) =>
            {
                var multiTenantMiddleware = new MultiTenantMiddleware(next);
                await multiTenantMiddleware.Invoke(context).ConfigureAwait(false);
            });
        }
    }
}
