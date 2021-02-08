using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Logging;

using System;
using System.Threading;
using System.Threading.Tasks;

namespace Finbuckle.MultiTenant.CosmosDb.Stores
{
    public abstract class DbContext
    {
        private readonly ILogger<DbContext> _logger;

        public CosmosClient Client { get; private set; }

        public DbContext(CosmosClient client)
        {
            Client = client;
        }

        public DbContext(CosmosClient client, ILogger<DbContext> logger)
        {
            Client = client;
            _logger = logger;
        }

        public abstract Task InitializeAsync(CancellationToken cancellationToken = default);

        internal protected async Task InitializeDbContextAsync(string databaseName, ThroughputProperties databaseThroughput, params DatabaseCollection[] collections)
        {
            // Database
            var databaseResponse = await Client.CreateDatabaseIfNotExistsAsync(databaseName, databaseThroughput).ConfigureAwait(false);
            if(!(databaseResponse.StatusCode == System.Net.HttpStatusCode.OK || databaseResponse.StatusCode == System.Net.HttpStatusCode.Created))
            {
                _logger.LogError($"Failed to Open or Create database: {databaseResponse.Diagnostics}");
                throw new ApplicationException($"Failed to Open or Create database: {databaseResponse.Diagnostics}");
            }
            _logger.LogInformation($"Created/Connected Database: {databaseResponse.Database.Id}");
            var database = Client.GetDatabase(databaseName);

            // Enumerate collections
            foreach (var collection in collections)
            {
                var colletionResponse = await database.CreateContainerIfNotExistsAsync(
                    new ContainerProperties(collection.CollectionName, collection.PartitionKey), collection.Throughput)
                    .ConfigureAwait(false);
                if (!(colletionResponse.StatusCode == System.Net.HttpStatusCode.OK || colletionResponse.StatusCode == System.Net.HttpStatusCode.Created))
                {
                    _logger.LogError($"Failed to Open or Create container: {colletionResponse.Diagnostics}");
                    throw new ApplicationException($"Failed to Open or Create container: {colletionResponse.Diagnostics}");
                }
                _logger.LogInformation($"Created/Connected Container: {colletionResponse.Container.Id}");
                collection.Container = colletionResponse.Container;
            }
        }
    }
}
