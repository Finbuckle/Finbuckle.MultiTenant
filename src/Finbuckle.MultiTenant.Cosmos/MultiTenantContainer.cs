// // Copyright Finbuckle LLC, Andrew White, and Contributors.
// // Refer to the solution LICENSE file for more inforation.
//
// using System.ComponentModel;
// using System.Runtime.CompilerServices;
// using Microsoft.Azure.Cosmos;
// using Microsoft.Azure.Cosmos.Scripts;
// using Container = Microsoft.Azure.Cosmos.Container;
//
// namespace Finbuckle.CosmosIdentity.MultiTenantCosmos;
//
// public static class ContainerExtensions
// {
//     public static MultiTenantContainer GetMultiTenantContainer(this Database database, string containerId, string tenantId)
//     {
//         return new MultiTenantContainer(database.GetContainer(containerId), tenantId);
//     }
// }
//
// public class MultiTenantContainer : Container
// {
//     private Container _containerImplementation;
//     private string _tenantId;
//
//     public MultiTenantContainer(Container containerImplementation, string tenantId)
//     {
//         _containerImplementation = containerImplementation;
//         _tenantId = tenantId;
//     }
//
//     public override Task<ContainerResponse> ReadContainerAsync(ContainerRequestOptions requestOptions = null,
//         CancellationToken cancellationToken = new CancellationToken())
//     {
//         return _containerImplementation.ReadContainerAsync(requestOptions, cancellationToken);
//     }
//
//     public override Task<ResponseMessage> ReadContainerStreamAsync(ContainerRequestOptions requestOptions = null,
//         CancellationToken cancellationToken = new CancellationToken())
//     {
//         return _containerImplementation.ReadContainerStreamAsync(requestOptions, cancellationToken);
//     }
//
//     public override Task<ContainerResponse> ReplaceContainerAsync(ContainerProperties containerProperties, ContainerRequestOptions requestOptions = null,
//         CancellationToken cancellationToken = new CancellationToken())
//     {
//         return _containerImplementation.ReplaceContainerAsync(containerProperties, requestOptions, cancellationToken);
//     }
//
//     public override Task<ResponseMessage> ReplaceContainerStreamAsync(ContainerProperties containerProperties, ContainerRequestOptions requestOptions = null,
//         CancellationToken cancellationToken = new CancellationToken())
//     {
//         return _containerImplementation.ReplaceContainerStreamAsync(containerProperties, requestOptions, cancellationToken);
//     }
//
//     public override Task<ContainerResponse> DeleteContainerAsync(ContainerRequestOptions requestOptions = null,
//         CancellationToken cancellationToken = new CancellationToken())
//     {
//         return _containerImplementation.DeleteContainerAsync(requestOptions, cancellationToken);
//     }
//
//     public override Task<ResponseMessage> DeleteContainerStreamAsync(ContainerRequestOptions requestOptions = null,
//         CancellationToken cancellationToken = new CancellationToken())
//     {
//         return _containerImplementation.DeleteContainerStreamAsync(requestOptions, cancellationToken);
//     }
//
//     public override Task<int?> ReadThroughputAsync(CancellationToken cancellationToken = new CancellationToken())
//     {
//         return _containerImplementation.ReadThroughputAsync(cancellationToken);
//     }
//
//     public override Task<ThroughputResponse> ReadThroughputAsync(RequestOptions requestOptions, CancellationToken cancellationToken = new CancellationToken())
//     {
//         return _containerImplementation.ReadThroughputAsync(requestOptions, cancellationToken);
//     }
//
//     public override Task<ThroughputResponse> ReplaceThroughputAsync(int throughput, RequestOptions requestOptions = null,
//         CancellationToken cancellationToken = new CancellationToken())
//     {
//         return _containerImplementation.ReplaceThroughputAsync(throughput, requestOptions, cancellationToken);
//     }
//
//     public override Task<ThroughputResponse> ReplaceThroughputAsync(ThroughputProperties throughputProperties, RequestOptions requestOptions = null,
//         CancellationToken cancellationToken = new CancellationToken())
//     {
//         return _containerImplementation.ReplaceThroughputAsync(throughputProperties, requestOptions, cancellationToken);
//     }
//
//     public override Task<ResponseMessage> CreateItemStreamAsync(Stream streamPayload, PartitionKey partitionKey, ItemRequestOptions requestOptions = null,
//         CancellationToken cancellationToken = new CancellationToken())
//     {
//         return _containerImplementation.CreateItemStreamAsync(streamPayload, new PartitionKey(_tenantId), requestOptions, cancellationToken);
//     }
//
//     public override Task<ItemResponse<T>> CreateItemAsync<T>(T item, PartitionKey? partitionKey = null, ItemRequestOptions requestOptions = null,
//         CancellationToken cancellationToken = new CancellationToken())
//     {
//         return _containerImplementation.CreateItemAsync(item, new PartitionKey(_tenantId), requestOptions, cancellationToken);
//     }
//
//     public override Task<ResponseMessage> ReadItemStreamAsync(string id, PartitionKey partitionKey, ItemRequestOptions requestOptions = null,
//         CancellationToken cancellationToken = new CancellationToken())
//     {
//         return _containerImplementation.ReadItemStreamAsync(id, new PartitionKey(_tenantId), requestOptions, cancellationToken);
//     }
//
//     public override Task<ItemResponse<T>> ReadItemAsync<T>(string id, PartitionKey partitionKey, ItemRequestOptions requestOptions = null,
//         CancellationToken cancellationToken = new CancellationToken())
//     {
//         return _containerImplementation.ReadItemAsync<T>(id, new PartitionKey(_tenantId), requestOptions, cancellationToken);
//     }
//
//     public override Task<ResponseMessage> UpsertItemStreamAsync(Stream streamPayload, PartitionKey partitionKey, ItemRequestOptions requestOptions = null,
//         CancellationToken cancellationToken = new CancellationToken())
//     {
//         return _containerImplementation.UpsertItemStreamAsync(streamPayload, new PartitionKey(_tenantId), requestOptions, cancellationToken);
//     }
//
//     public override Task<ItemResponse<T>> UpsertItemAsync<T>(T item, PartitionKey? partitionKey = null, ItemRequestOptions requestOptions = null,
//         CancellationToken cancellationToken = new CancellationToken())
//     {
//         return _containerImplementation.UpsertItemAsync(item, new PartitionKey(_tenantId), requestOptions, cancellationToken);
//     }
//
//     public override Task<ResponseMessage> ReplaceItemStreamAsync(Stream streamPayload, string id, PartitionKey partitionKey,
//         ItemRequestOptions requestOptions = null, CancellationToken cancellationToken = new CancellationToken())
//     {
//         return _containerImplementation.ReplaceItemStreamAsync(streamPayload, id, new PartitionKey(_tenantId), requestOptions, cancellationToken);
//     }
//
//     public override Task<ItemResponse<T>> ReplaceItemAsync<T>(T item, string id, PartitionKey? partitionKey = null, ItemRequestOptions requestOptions = null,
//         CancellationToken cancellationToken = new CancellationToken())
//     {
//         return _containerImplementation.ReplaceItemAsync(item, id, new PartitionKey(_tenantId), requestOptions, cancellationToken);
//     }
//
//     public override Task<ResponseMessage> ReadManyItemsStreamAsync(IReadOnlyList<(string id, PartitionKey partitionKey)> items, ReadManyRequestOptions readManyRequestOptions = null,
//         CancellationToken cancellationToken = new CancellationToken())
//     {
//         return _containerImplementation.ReadManyItemsStreamAsync(items, readManyRequestOptions, cancellationToken);
//     }
//
//     public override Task<FeedResponse<T>> ReadManyItemsAsync<T>(IReadOnlyList<(string id, PartitionKey partitionKey)> items, ReadManyRequestOptions readManyRequestOptions = null,
//         CancellationToken cancellationToken = new CancellationToken())
//     {
//         return _containerImplementation.ReadManyItemsAsync<T>(items, readManyRequestOptions, cancellationToken);
//     }
//
//     public override Task<ItemResponse<T>> PatchItemAsync<T>(string id, PartitionKey partitionKey, IReadOnlyList<PatchOperation> patchOperations,
//         PatchItemRequestOptions requestOptions = null, CancellationToken cancellationToken = new CancellationToken())
//     {
//         return _containerImplementation.PatchItemAsync<T>(id, new PartitionKey(_tenantId), patchOperations, requestOptions, cancellationToken);
//     }
//
//     public override Task<ResponseMessage> PatchItemStreamAsync(string id, PartitionKey partitionKey, IReadOnlyList<PatchOperation> patchOperations,
//         PatchItemRequestOptions requestOptions = null, CancellationToken cancellationToken = new CancellationToken())
//     {
//         return _containerImplementation.PatchItemStreamAsync(id, new PartitionKey(_tenantId), patchOperations, requestOptions, cancellationToken);
//     }
//
//     public override Task<ResponseMessage> DeleteItemStreamAsync(string id, PartitionKey partitionKey, ItemRequestOptions requestOptions = null,
//         CancellationToken cancellationToken = new CancellationToken())
//     {
//         return _containerImplementation.DeleteItemStreamAsync(id, new PartitionKey(_tenantId), requestOptions, cancellationToken);
//     }
//
//     public override Task<ItemResponse<T>> DeleteItemAsync<T>(string id, PartitionKey partitionKey, ItemRequestOptions requestOptions = null,
//         CancellationToken cancellationToken = new CancellationToken())
//     {
//         return _containerImplementation.DeleteItemAsync<T>(id, new PartitionKey(_tenantId), requestOptions, cancellationToken);
//     }
//
//     public override FeedIterator GetItemQueryStreamIterator(QueryDefinition queryDefinition, string continuationToken = null,
//         QueryRequestOptions requestOptions = null)
//     {
//         return _containerImplementation.GetItemQueryStreamIterator(queryDefinition, continuationToken, requestOptions);
//     }
//
//     public override FeedIterator<T> GetItemQueryIterator<T>(QueryDefinition queryDefinition, string continuationToken = null,
//         QueryRequestOptions requestOptions = null)
//     {
//         return _containerImplementation.GetItemQueryIterator<T>(queryDefinition, continuationToken, requestOptions);
//     }
//
//     public override FeedIterator GetItemQueryStreamIterator(string queryText = null, string continuationToken = null,
//         QueryRequestOptions requestOptions = null)
//     {
//         return _containerImplementation.GetItemQueryStreamIterator(queryText, continuationToken, requestOptions);
//     }
//
//     public override FeedIterator<T> GetItemQueryIterator<T>(string queryText = null, string continuationToken = null,
//         QueryRequestOptions requestOptions = null)
//     {
//         return _containerImplementation.GetItemQueryIterator<T>(queryText, continuationToken, requestOptions);
//     }
//
//     public override IOrderedQueryable<T> GetItemLinqQueryable<T>(bool allowSynchronousQueryExecution = false, string continuationToken = null,
//         QueryRequestOptions requestOptions = null, CosmosLinqSerializerOptions linqSerializerOptions = null)
//     {
//         return _containerImplementation.GetItemLinqQueryable<T>(allowSynchronousQueryExecution, continuationToken, requestOptions, linqSerializerOptions);
//     }
//
//     public override ChangeFeedProcessorBuilder GetChangeFeedProcessorBuilder<T>(string processorName, ChangesHandler<T> onChangesDelegate)
//     {
//         return _containerImplementation.GetChangeFeedProcessorBuilder(processorName, onChangesDelegate);
//     }
//
//     public override ChangeFeedProcessorBuilder GetChangeFeedEstimatorBuilder(string processorName,
//         ChangesEstimationHandler estimationDelegate, TimeSpan? estimationPeriod = null)
//     {
//         return _containerImplementation.GetChangeFeedEstimatorBuilder(processorName, estimationDelegate, estimationPeriod);
//     }
//
//     public override ChangeFeedEstimator GetChangeFeedEstimator(string processorName, Container leaseContainer)
//     {
//         return _containerImplementation.GetChangeFeedEstimator(processorName, leaseContainer);
//     }
//
//     public override TransactionalBatch CreateTransactionalBatch(PartitionKey partitionKey)
//     {
//         return _containerImplementation.CreateTransactionalBatch(partitionKey);
//     }
//
//     public override Task<IReadOnlyList<FeedRange>> GetFeedRangesAsync(CancellationToken cancellationToken = new CancellationToken())
//     {
//         return _containerImplementation.GetFeedRangesAsync(cancellationToken);
//     }
//
//     public override FeedIterator GetChangeFeedStreamIterator(ChangeFeedStartFrom changeFeedStartFrom, ChangeFeedMode changeFeedMode,
//         ChangeFeedRequestOptions changeFeedRequestOptions = null)
//     {
//         return _containerImplementation.GetChangeFeedStreamIterator(changeFeedStartFrom, changeFeedMode, changeFeedRequestOptions);
//     }
//
//     public override FeedIterator<T> GetChangeFeedIterator<T>(ChangeFeedStartFrom changeFeedStartFrom, ChangeFeedMode changeFeedMode,
//         ChangeFeedRequestOptions changeFeedRequestOptions = null)
//     {
//         return _containerImplementation.GetChangeFeedIterator<T>(changeFeedStartFrom, changeFeedMode, changeFeedRequestOptions);
//     }
//
//     public override ChangeFeedProcessorBuilder GetChangeFeedProcessorBuilder<T>(string processorName, ChangeFeedHandler<T> onChangesDelegate)
//     {
//         return _containerImplementation.GetChangeFeedProcessorBuilder(processorName, onChangesDelegate);
//     }
//
//     public override ChangeFeedProcessorBuilder GetChangeFeedProcessorBuilderWithManualCheckpoint<T>(string processorName,
//         ChangeFeedHandlerWithManualCheckpoint<T> onChangesDelegate)
//     {
//         return _containerImplementation.GetChangeFeedProcessorBuilderWithManualCheckpoint(processorName, onChangesDelegate);
//     }
//
//     public override ChangeFeedProcessorBuilder GetChangeFeedProcessorBuilder(string processorName,
//         ChangeFeedStreamHandler onChangesDelegate)
//     {
//         return _containerImplementation.GetChangeFeedProcessorBuilder(processorName, onChangesDelegate);
//     }
//
//     public override ChangeFeedProcessorBuilder GetChangeFeedProcessorBuilderWithManualCheckpoint(string processorName,
//         ChangeFeedStreamHandlerWithManualCheckpoint onChangesDelegate)
//     {
//         return _containerImplementation.GetChangeFeedProcessorBuilderWithManualCheckpoint(processorName, onChangesDelegate);
//     }
//
//     public override string Id => _containerImplementation.Id;
//
//     public override Database Database => _containerImplementation.Database;
//
//     public override Conflicts Conflicts => _containerImplementation.Conflicts;
//
//     public override Scripts Scripts => _containerImplementation.Scripts;
// }