using System;
using System.Threading.Tasks;
using Finbuckle.MultiTenant.AspNetCore;
using Microsoft.AspNetCore.Http;
using Finbuckle.MultiTenant.Core;
using Finbuckle.MultiTenant.Core.Abstractions;

namespace Finbuckle.MultiTenant.AspNetCore
{
    public class HostMultiTenantStrategy : IMultiTenantStrategy
    {
        public string GetIdentifier(object context)
        {
            if(!typeof(HttpContext).IsAssignableFrom(context.GetType()))
                throw new MultiTenantException(null,
                    new ArgumentException("\"context\" type must be of type HttpContext", nameof(context)));

            var host = (context as HttpContext).Request.Host;
            if (host.HasValue == false)
                return null;
            
            var hostSegments = host.Host.Split('.');

            return hostSegments[0];
        }
    }
}