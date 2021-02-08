using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.Azure.Cosmos;
using Microsoft.Azure.Cosmos.Scripts;

using static Microsoft.Azure.Cosmos.Container;

namespace Finbuckle.MultiTenant.CosmosDb
{
    public class MultiTenantContainer
    {
        private readonly Container _container;
        private readonly ITenantInfo _tenantInfo;
        private readonly PartitionKey _partitionKey;

        public MultiTenantContainer(Container container, ITenantInfo tenantInfo)
        {
            _container = container;
            _tenantInfo = tenantInfo;
            _partitionKey = new PartitionKey(_tenantInfo.Id);
        }

        public string Id => _container.Id;

        public Database Database => _container.Database;

        public Conflicts Conflicts => _container.Conflicts;

        public Scripts Scripts => _container.Scripts;

        public Task<ItemResponse<T>> CreateItemAsync<T>(T item, ItemRequestOptions requestOptions = null, CancellationToken cancellationToken = default)
        {
            return _container.CreateItemAsync(item, _partitionKey, requestOptions, cancellationToken);
        }

        public Task<ResponseMessage> CreateItemStreamAsync(Stream streamPayload, ItemRequestOptions requestOptions = null, CancellationToken cancellationToken = default)
        {
            return _container.CreateItemStreamAsync(streamPayload, _partitionKey, requestOptions, cancellationToken);
        }

        public TransactionalBatch CreateTransactionalBatch()
        {
            return _container.CreateTransactionalBatch(_partitionKey);
        }

        public Task<ContainerResponse> DeleteContainerAsync(ContainerRequestOptions requestOptions = null, CancellationToken cancellationToken = default)
        {
            return _container.DeleteContainerAsync(requestOptions, cancellationToken);
        }

        public Task<ResponseMessage> DeleteContainerStreamAsync(ContainerRequestOptions requestOptions = null, CancellationToken cancellationToken = default)
        {
            return _container.DeleteContainerStreamAsync(requestOptions, cancellationToken);
        }

        public Task<ItemResponse<T>> DeleteItemAsync<T>(string id, ItemRequestOptions requestOptions = null, CancellationToken cancellationToken = default)
        {
            return _container.DeleteItemAsync<T>(id, _partitionKey, requestOptions, cancellationToken);
        }

        public Task<ResponseMessage> DeleteItemStreamAsync(string id, ItemRequestOptions requestOptions = null, CancellationToken cancellationToken = default)
        {
            return _container.DeleteItemStreamAsync(id, _partitionKey, requestOptions, cancellationToken);
        }

        public ChangeFeedProcessorBuilder GetChangeFeedEstimatorBuilder(string processorName, ChangesEstimationHandler estimationDelegate, TimeSpan? estimationPeriod = null)
        {
            return _container.GetChangeFeedEstimatorBuilder(processorName, estimationDelegate, estimationPeriod);
        }

        public IOrderedQueryable<T> GetItemLinqQueryable<T>(bool allowSynchronousQueryExecution = false, string continuationToken = null, QueryRequestOptions requestOptions = null)
        {
            return _container.GetItemLinqQueryable<T>(allowSynchronousQueryExecution, continuationToken, requestOptions);
        }

        public FeedIterator<T> GetItemQueryIterator<T>(QueryDefinition queryDefinition, string continuationToken = null, QueryRequestOptions requestOptions = null)
        {
            return _container.GetItemQueryIterator<T>(queryDefinition, continuationToken, requestOptions);
        }

        public FeedIterator<T> GetItemQueryIterator<T>(string queryText = null, string continuationToken = null, QueryRequestOptions requestOptions = null)
        {
            return _container.GetItemQueryIterator<T>(queryText, continuationToken, requestOptions);
        }

        public FeedIterator GetItemQueryStreamIterator(QueryDefinition queryDefinition, string continuationToken = null, QueryRequestOptions requestOptions = null)
        {
            return _container.GetItemQueryStreamIterator(queryDefinition, continuationToken, requestOptions);
        }

        public FeedIterator GetItemQueryStreamIterator(string queryText = null, string continuationToken = null, QueryRequestOptions requestOptions = null)
        {
            return _container.GetItemQueryStreamIterator(queryText, continuationToken, requestOptions);
        }

        public Task<ContainerResponse> ReadContainerAsync(ContainerRequestOptions requestOptions = null, CancellationToken cancellationToken = default)
        {
            return _container.ReadContainerAsync(requestOptions, cancellationToken);
        }

        public Task<ResponseMessage> ReadContainerStreamAsync(ContainerRequestOptions requestOptions = null, CancellationToken cancellationToken = default)
        {
            return _container.ReadContainerStreamAsync(requestOptions, cancellationToken);
        }

        public Task<ItemResponse<T>> ReadItemAsync<T>(string id, ItemRequestOptions requestOptions = null, CancellationToken cancellationToken = default)
        {
            return _container.ReadItemAsync<T>(id, _partitionKey, requestOptions, cancellationToken);
        }

        public Task<ResponseMessage> ReadItemStreamAsync(string id, ItemRequestOptions requestOptions = null, CancellationToken cancellationToken = default)
        {
            return _container.ReadItemStreamAsync(id, _partitionKey, requestOptions, cancellationToken);
        }

        public Task<int?> ReadThroughputAsync(CancellationToken cancellationToken = default)
        {
            return _container.ReadThroughputAsync(cancellationToken);
        }

        public Task<ThroughputResponse> ReadThroughputAsync(RequestOptions requestOptions, CancellationToken cancellationToken = default)
        {
            return _container.ReadThroughputAsync(requestOptions, cancellationToken);
        }

        public Task<ContainerResponse> ReplaceContainerAsync(ContainerProperties containerProperties, ContainerRequestOptions requestOptions = null, CancellationToken cancellationToken = default)
        {
            return _container.ReplaceContainerAsync(containerProperties, requestOptions, cancellationToken);
        }

        public Task<ResponseMessage> ReplaceContainerStreamAsync(ContainerProperties containerProperties, ContainerRequestOptions requestOptions = null, CancellationToken cancellationToken = default)
        {
            return _container.ReplaceContainerStreamAsync(containerProperties, requestOptions, cancellationToken);
        }

        public Task<ItemResponse<T>> ReplaceItemAsync<T>(T item, string id, ItemRequestOptions requestOptions = null, CancellationToken cancellationToken = default)
        {
            return _container.ReplaceItemAsync(item, id, _partitionKey, requestOptions, cancellationToken);
        }

        public Task<ResponseMessage> ReplaceItemStreamAsync(Stream streamPayload, string id, ItemRequestOptions requestOptions = null, CancellationToken cancellationToken = default)
        {
            return _container.ReplaceItemStreamAsync(streamPayload, id, _partitionKey, requestOptions, cancellationToken);
        }

        public Task<ThroughputResponse> ReplaceThroughputAsync(int throughput, RequestOptions requestOptions = null, CancellationToken cancellationToken = default)
        {
            return _container.ReplaceThroughputAsync(throughput, requestOptions, cancellationToken);
        }

        public Task<ThroughputResponse> ReplaceThroughputAsync(ThroughputProperties throughputProperties, RequestOptions requestOptions = null, CancellationToken cancellationToken = default)
        {
            return _container.ReplaceThroughputAsync(throughputProperties, requestOptions, cancellationToken);
        }

        public Task<ItemResponse<T>> UpsertItemAsync<T>(T item, ItemRequestOptions requestOptions = null, CancellationToken cancellationToken = default)
        {
            return _container.UpsertItemAsync(item, _partitionKey, requestOptions, cancellationToken);
        }

        public Task<ResponseMessage> UpsertItemStreamAsync(Stream streamPayload, ItemRequestOptions requestOptions = null, CancellationToken cancellationToken = default)
        {
            return _container.UpsertItemStreamAsync(streamPayload, _partitionKey, requestOptions, cancellationToken);
        }
    }
}
