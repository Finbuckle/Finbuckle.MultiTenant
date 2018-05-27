using System;
using System.Threading.Tasks;
using Finbuckle.MultiTenant.Core;
using Finbuckle.MultiTenant.Core.Abstractions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Logging;

namespace Finbuckle.MultiTenant.AspNetCore
{
    public class RouteMultiTenantStrategy : IMultiTenantStrategy
    {
        private readonly string tenantParam;
        private readonly ILogger<RouteMultiTenantStrategy> logger;

        public RouteMultiTenantStrategy(string tenantParam, ILogger<RouteMultiTenantStrategy> logger = null)
        {
            if (string.IsNullOrWhiteSpace(tenantParam))
            {
                throw new MultiTenantException(null, new ArgumentException($"\"{nameof(tenantParam)}\" must not be null or whitespace", nameof(tenantParam)));
            }

            this.tenantParam = tenantParam;
            this.logger = logger;
        }

        public string GetIdentifier(object context)
        {
            if(!typeof(HttpContext).IsAssignableFrom(context.GetType()))
                throw new MultiTenantException(null,
                    new ArgumentException("\"context\" type must be of type HttpContext", nameof(context)));

            object identifier = null;
            (context as HttpContext).GetRouteData()?.Values.TryGetValue(tenantParam, out identifier);

            Utilities.TryLogInfo(logger, $"Found identifier:  \"{identifier ?? "<null>"}\"");
            
            return identifier as string;
        }
    }
}
