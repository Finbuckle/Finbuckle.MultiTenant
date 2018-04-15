using System;
using System.Threading.Tasks;
using Finbuckle.MultiTenant.AspNetCore;
using Finbuckle.MultiTenant.Core;
using Finbuckle.MultiTenant.Core.Abstractions;
using Microsoft.AspNetCore.Http;

namespace Finbuckle.MultiTenant.AspNetCore
{
    public class BasePathMultiTenantStrategy : IMultiTenantStrategy
    {
        public string GetIdentifier(object context)
        {
            if(!typeof(HttpContext).IsAssignableFrom(context.GetType()))
                throw new MultiTenantException(null,
                    new ArgumentException("\"context\" type must be of type HttpContext", nameof(context)));

            var path = (context as HttpContext).Request.Path;
            var pathSegments =
                path.Value.Split(new char[] { '/' },
                                  StringSplitOptions.RemoveEmptyEntries);

            if (pathSegments.Length == 0)
                return null;

            return pathSegments[0];
        }
    }
}