using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace Finbuckle.MultiTenant.Stores
{
    public class HttpRemoteStore : IMultiTenantStore
    {
        private readonly HttpRemoteStoreClient client;

        public HttpRemoteStore(HttpRemoteStoreClient client)
        {
            this.client = client ?? throw new ArgumentNullException(nameof(client));
        }

        public Task<bool> TryAddAsync(TenantInfo tenantInfo)
        {
            throw new System.NotImplementedException();
        }

        public Task<TenantInfo> TryGetAsync(string id)
        {
            throw new System.NotImplementedException();
        }

        public async Task<TenantInfo> TryGetByIdentifierAsync(string identifier)
        {
            var result = await client.TryGetByIdentifierAsync(identifier);
            return result;
        }

        public Task<bool> TryRemoveAsync(string id)
        {
            throw new System.NotImplementedException();
        }

        public Task<bool> TryUpdateAsync(TenantInfo tenantInfo)
        {
            throw new System.NotImplementedException();
        }
    }
}