using Microsoft.Azure.Cosmos;
using Microsoft.Azure.Cosmos.Linq;
using Newtonsoft.Json;

namespace Finbuckle.MultiTenant.Cosmos;

public class CosmosStore<T> : IMultiTenantStore<T> where T : class, ITenantInfo, new()
{
    private Container _container;

    public CosmosStore(Container container)
    {
        _container = container;
    }

    public async Task<bool> TryAddAsync(T tenantInfo)
    {
        try
        {
            await _container.CreateItemAsync(tenantInfo, PartitionKey.None);
        }
        catch
        {
            return false;
        }

        return true;
    }

    public async Task<bool> TryUpdateAsync(T tenantInfo)
    {
        try
        {
            await _container.ReplaceItemAsync(tenantInfo, tenantInfo.Id, PartitionKey.None);
        }
        catch
        {
            return false;
        }

        return true;
    }

    public async Task<bool> TryRemoveAsync(string identifier)
    {
        var tenant = await TryGetByIdentifierAsync(identifier);
        if (tenant is null)
            return false;

        var response = await _container.DeleteItemStreamAsync(tenant.Id, PartitionKey.None);

        return response.IsSuccessStatusCode;
    }

    public async Task<T?> TryGetByIdentifierAsync(string identifier)
    {
        var queryable = _container.GetItemLinqQueryable<T>().Where(t => t.Identifier == identifier);
        return (await GetEnumerableAsync(queryable)).FirstOrDefault();
    }

    public async Task<T?> TryGetAsync(string id)
    {
        try
        {
            var response = await _container.ReadItemAsync<T>(id, PartitionKey.None);
            return response.Resource;
        }
        catch
        {
            return null;
        }
    }

    public async Task<IEnumerable<T>> GetAllAsync()
    {
        var queryable = _container.GetItemLinqQueryable<T>();
        return await GetEnumerableAsync(queryable);
    }

    private static async Task<IEnumerable<T>> GetEnumerableAsync(IQueryable<T> queryable,
        CancellationToken cancellationToken = default)
    {
        var result = new List<T>();
        var feed = queryable.ToFeedIterator();
        while (feed.HasMoreResults)
            result.AddRange(await feed.ReadNextAsync(cancellationToken));

        return result;
    }
}