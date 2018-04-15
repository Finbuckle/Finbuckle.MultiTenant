using System;
using System.Threading.Tasks;
using Finbuckle.MultiTenant.Core.Abstractions;

namespace Finbuckle.MultiTenant.Core
{
    /// <summary>
    /// Resolves the <c>TenantContext</c> from the configured <c>ITenantStore</c> using the configured <c>ITenantResolverStrategy</c>.
    /// </summary>
    public class TenantResolver
    {
        public readonly IMultiTenantStore _multiTenantStore;
        private readonly IMultiTenantStrategy _multiTenantStrategy;

        public TenantResolver(IMultiTenantStore multiTenantStore, IMultiTenantStrategy _multiTenantStrategy)
        {
            _multiTenantStore = multiTenantStore ??
                throw new MultiTenantException(null, new ArgumentNullException(nameof(multiTenantStore)));
            
            this._multiTenantStrategy = _multiTenantStrategy ??
                throw new ArgumentNullException(nameof(_multiTenantStrategy));
        }

        /// <summary>
        /// Resolves the <c>TenantContext</c> from the configured <c>ITenantStore</c> using the configured <c>ITenantResolverStrategy</c>.
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        public async Task<TenantContext> ResolveAsync(object context)
        {
            string identifier = _multiTenantStrategy.GetIdentifier(context);
            if(string.IsNullOrWhiteSpace(identifier))
                return null;

            var storeResult = await _multiTenantStore.GetByIdentifierAsync(identifier).ConfigureAwait(false);
            if (storeResult == null)
                return null;

            var result = new TenantContext(storeResult);
            result.MultiTenantStrategyType = _multiTenantStrategy.GetType();
            result.MultiTenantStoreType = _multiTenantStore.GetType();

            return result;
        }
    }
}