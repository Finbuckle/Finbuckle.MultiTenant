using System;
using System.Threading.Tasks;
using Finbuckle.MultiTenant.Core.Abstractions;
using Microsoft.Extensions.Logging;

namespace Finbuckle.MultiTenant.Core
{
    /// <summary>
    /// Resolves the <c>TenantContext</c> from the configured <c>ITenantStore</c> using the configured <c>ITenantResolverStrategy</c>.
    /// </summary>
    public class TenantResolver
    {
        public readonly IMultiTenantStore _multiTenantStore;
        private readonly IMultiTenantStrategy _multiTenantStrategy;
        private readonly ILogger<TenantResolver> _logger;

        public TenantResolver(IMultiTenantStore multiTenantStore, IMultiTenantStrategy multiTenantStrategy, ILogger<TenantResolver> logger = null)
        {
            _multiTenantStore = multiTenantStore ??
                throw new MultiTenantException(null, new ArgumentNullException(nameof(multiTenantStore)));
            
            _multiTenantStrategy = multiTenantStrategy ??
                throw new ArgumentNullException(nameof(_multiTenantStrategy));
            
            _logger = logger;
        }

        /// <summary>
        /// Resolves the <c>TenantContext</c> from the configured <c>ITenantStore</c> using the configured <c>ITenantResolverStrategy</c>.
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        public async Task<TenantContext> ResolveAsync(object context)
        {
            TryLogInfo($"Resolving tenant using \"{_multiTenantStrategy.GetType()}\".");

            string identifier = _multiTenantStrategy.GetIdentifier(context);

            TryLogInfo($"Tenant identifier \"{identifier ?? "<null>"}\" detected.");

            if (string.IsNullOrWhiteSpace(identifier))
                return null;

            TryLogInfo($"Retrieving TenantContext using \"{_multiTenantStore.GetType()}\".");

            var storeResult = await _multiTenantStore.GetByIdentifierAsync(identifier).ConfigureAwait(false);
            
            TryLogInfo($"TenantContext for Tenant Id \"{storeResult?.Id ?? "<null>"}\" was retrieved.");

            if (storeResult == null)
                return null;

            var result = new TenantContext(storeResult);
            result.MultiTenantStrategyType = _multiTenantStrategy.GetType();
            result.MultiTenantStoreType = _multiTenantStore.GetType();

            return result;
        }

        private void TryLogInfo(string message)
        {
            if (_logger != null)
            {
                _logger.LogInformation(message);
            }
        }
    }
}