using Microsoft.Azure.Cosmos;

namespace Finbuckle.MultiTenant.CosmosDb.Stores
{
    public class DatabaseCollection
    {
        public string CollectionName { get; set; }
        public string PartitionKey { get; set; }
        public ThroughputProperties Throughput { get; set; }
        public Container Container { get; internal set; }

        public DatabaseCollection(string collectionName, string partitionKey, ThroughputProperties throughput = null)
        {
            CollectionName = collectionName;
            PartitionKey = partitionKey;
            Throughput = throughput ?? ThroughputProperties.CreateManualThroughput(400);
        }
    }
}