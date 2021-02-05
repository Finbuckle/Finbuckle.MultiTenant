using Microsoft.AspNetCore.Http;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Description;
using Microsoft.Azure.WebJobs.Host.Bindings;
using Microsoft.Azure.WebJobs.Host.Config;

using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Finbuckle.MultiTenant.AzureFunctions
{
    [Extension(nameof(TenantExtensionConfigProvider))]
    public class TenantExtensionConfigProvider : IExtensionConfigProvider
    {
        public void Initialize(ExtensionConfigContext context)
        {
            //context.AddOpenConverter<HttpRequest, TenantOpenType>(typeof(TenantConverter<>));

            // Creates a rule that links the attribute to the binding

            var provider = new TenantBindingProvider();
            var rule = context.AddBindingRule<TenantAttribute>().Bind(provider);
        }
    }
}