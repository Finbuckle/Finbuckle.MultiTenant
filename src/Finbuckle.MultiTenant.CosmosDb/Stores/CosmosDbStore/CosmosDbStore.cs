using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Microsoft.Azure.Cosmos.Linq;

namespace Finbuckle.MultiTenant.CosmosDb.Stores
{
    public class CosmosDbStore<TTenantInfo> : IMultiTenantStore<TTenantInfo>
        where TTenantInfo : class, ITenantInfo, new()
    {
        private readonly CosmosDbStoreContext _dbContext;

        public CosmosDbStore(CosmosDbStoreContext dbContext)
        {
            _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
        }

        public async Task<TTenantInfo> TryGetAsync(string id)
        {
            using (var iterator = _dbContext.Container.GetItemLinqQueryable<TTenantInfo>()
                .Where(x => x.Id == id).ToFeedIterator())
            {
                while (iterator.HasMoreResults)
                {
                    var document = await iterator.ReadNextAsync();
                    return document.Resource.SingleOrDefault();
                }
            }
            return default;
        }

        public async Task<IEnumerable<TTenantInfo>> GetAllAsync()
        {
            var tenantInfo = new List<TTenantInfo>();

            using (var iterator = _dbContext.Container.GetItemLinqQueryable<TTenantInfo>()
                .ToFeedIterator())
            {
                while (iterator.HasMoreResults)
                {
                    foreach (var tenant in await iterator.ReadNextAsync())
                    {
                        tenantInfo.Add(tenant);
                    }
                }
            }
            return tenantInfo;
        }

        public async Task<TTenantInfo> TryGetByIdentifierAsync(string identifier)
        {
            // Note: Identifier returns the organisationId, use that instead.
            //       Also the Id is better for lookup than the identifier.
            using (var iterator = _dbContext.Container.GetItemLinqQueryable<TTenantInfo>()
                //.Where(x => x.Identifier == identifier).ToFeedIterator())
                .Where(x => x.Id == identifier).ToFeedIterator())
            {
                while (iterator.HasMoreResults)
                {
                    var document = await iterator.ReadNextAsync();
                    return document.Resource.SingleOrDefault();
                }
            }
            return default;
        }

        public async Task<bool> TryAddAsync(TTenantInfo tenantInfo)
        {
            return (await _dbContext.Container.CreateItemAsync(tenantInfo)).StatusCode == System.Net.HttpStatusCode.OK;
        }

        public async Task<bool> TryRemoveAsync(string id)
        {
            return (await _dbContext.Container.DeleteItemAsync<TTenantInfo>(id, new Microsoft.Azure.Cosmos.PartitionKey(id))).StatusCode == System.Net.HttpStatusCode.OK;
        }

        public async Task<bool> TryUpdateAsync(TTenantInfo tenantInfo)
        {
            return (await _dbContext.Container.UpsertItemAsync(tenantInfo)).StatusCode == System.Net.HttpStatusCode.OK;
        }
    }
}
