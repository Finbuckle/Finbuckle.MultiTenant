using System;
using System.Threading.Tasks;
using Finbuckle.MultiTenant.AspNetCore;
using Microsoft.AspNetCore.Http;
using Finbuckle.MultiTenant.Core;
using Finbuckle.MultiTenant.Core.Abstractions;
using Microsoft.Extensions.Logging;

namespace Finbuckle.MultiTenant.AspNetCore
{
    public class HostMultiTenantStrategy : IMultiTenantStrategy
    {
        private readonly ILogger<HostMultiTenantStrategy> logger;

        public HostMultiTenantStrategy(ILogger<HostMultiTenantStrategy> logger = null)
        {
            this.logger = logger;
        }

        public string GetIdentifier(object context)
        {
            if(!typeof(HttpContext).IsAssignableFrom(context.GetType()))
                throw new MultiTenantException(null,
                    new ArgumentException("\"context\" type must be of type HttpContext", nameof(context)));

            var host = (context as HttpContext).Request.Host;

            if (logger != null)
            {
                logger.LogInformation($"Host:  \"{host.Host ?? "<null>"}\"");
            }

            if (host.HasValue == false)
                return null;
            
            var hostSegments = host.Host.Split(new[] {'.'}, StringSplitOptions.RemoveEmptyEntries);
            string identifier = hostSegments[0];

            if (logger != null)
            {
                logger.LogInformation($"Found identifier:  \"{identifier}\"");
            }

            return identifier;
        }
    }
}