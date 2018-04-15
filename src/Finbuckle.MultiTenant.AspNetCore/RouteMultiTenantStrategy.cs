using System;
using System.Threading.Tasks;
using Finbuckle.MultiTenant.Core;
using Finbuckle.MultiTenant.Core.Abstractions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace Finbuckle.MultiTenant.AspNetCore
{
    public class RouteMultiTenantStrategy : IMultiTenantStrategy
    {
        private readonly string _tenantParam;

        public RouteMultiTenantStrategy(string tenantParam)
        {
            if (string.IsNullOrWhiteSpace(tenantParam))
            {
                throw new MultiTenantException(null, new ArgumentException($"\"{nameof(tenantParam)}\" must not be null or whitespace", nameof(tenantParam)));
            }

            _tenantParam = tenantParam;
        }

        public string GetIdentifier(object context)
        {
            if(!typeof(HttpContext).IsAssignableFrom(context.GetType()))
                throw new MultiTenantException(null,
                    new ArgumentException("\"context\" type must be of type HttpContext", nameof(context)));

            object identifier = null;
            (context as HttpContext).GetRouteData()?.Values.TryGetValue(_tenantParam, out identifier);

            return identifier as string;
        }
    }
}
