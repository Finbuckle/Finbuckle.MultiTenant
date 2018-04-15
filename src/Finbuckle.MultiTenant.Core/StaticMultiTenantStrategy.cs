using System;
using System.Threading.Tasks;
using Finbuckle.MultiTenant.Core.Abstractions;

namespace Finbuckle.MultiTenant.Core
{
    /// <summary>
    /// <c>IMultiTenantResolverStrategy</c> implementation that always resolves the same identifier.
    /// </summary>
    public class StaticMultiTenantStrategy : IMultiTenantStrategy
    {
        private readonly string _identifier;

        public StaticMultiTenantStrategy(string identifier)
        {
            if (string.IsNullOrWhiteSpace(identifier))
            {
                throw new MultiTenantException(null, new ArgumentException("\"identifier\" must not be null or whitespace", nameof(identifier)));
            }
            
            _identifier = identifier;
        }

        public string GetIdentifier(object context) => _identifier;
    }
}