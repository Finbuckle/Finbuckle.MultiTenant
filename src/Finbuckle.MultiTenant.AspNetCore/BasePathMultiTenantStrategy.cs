using System;
using System.Threading.Tasks;
using Finbuckle.MultiTenant.AspNetCore;
using Finbuckle.MultiTenant.Core;
using Finbuckle.MultiTenant.Core.Abstractions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace Finbuckle.MultiTenant.AspNetCore
{
    public class BasePathMultiTenantStrategy : IMultiTenantStrategy
    {
        private readonly ILogger<BasePathMultiTenantStrategy> logger;

        public BasePathMultiTenantStrategy(ILogger<BasePathMultiTenantStrategy> logger = null)
        {
            this.logger = logger;
        }

        public string GetIdentifier(object context)
        {
            if(!typeof(HttpContext).IsAssignableFrom(context.GetType()))
                throw new MultiTenantException(null,
                    new ArgumentException("\"context\" type must be of type HttpContext", nameof(context)));

            var path = (context as HttpContext).Request.Path;

            Utilities.TryLogInfo(logger, $"Path:  \"{path.Value ?? "<null>"}\"");

            var pathSegments =
                path.Value.Split(new char[] { '/' }, StringSplitOptions.RemoveEmptyEntries);

            if (pathSegments.Length == 0)
                return null;

            string identifier = pathSegments[0];

            Utilities.TryLogInfo(logger, $"Found identifier:  \"{identifier ?? "<null>"}\"");

            return identifier;
        }
    }
}