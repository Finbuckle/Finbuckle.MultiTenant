using System;
using System.Threading.Tasks;
using Finbuckle.MultiTenant.Core.Abstractions;
using Microsoft.Extensions.Logging;

namespace Finbuckle.MultiTenant.Core
{
    /// <summary>
    /// <c>IMultiTenantResolverStrategy</c> implementation that always resolves the same identifier.
    /// </summary>
    public class StaticMultiTenantStrategy : IMultiTenantStrategy
    {
        private readonly string identifier;
        private readonly ILogger<StaticMultiTenantStrategy> logger;

        public StaticMultiTenantStrategy(string identifier, ILogger<StaticMultiTenantStrategy> logger = null)
        {
            if (string.IsNullOrWhiteSpace(identifier))
            {
                throw new MultiTenantException(null, new ArgumentException("\"identifier\" must not be null or whitespace", nameof(identifier)));
            }

            this.identifier = identifier;
            this.logger = logger;
        }

        public string GetIdentifier(object context)
        {
            Utilities.TryLogInfo(logger, $"Found identifier:  \"{identifier}\"");

            return identifier;
        }
    }
}