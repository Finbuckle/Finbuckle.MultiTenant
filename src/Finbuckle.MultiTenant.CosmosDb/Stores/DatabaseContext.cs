using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Logging;

using System;
using System.Threading;
using System.Threading.Tasks;

namespace Finbuckle.MultiTenant.CosmosDb.Stores
{
    public abstract class DatabaseContext
    {
        private readonly ILogger<DatabaseContext> _logger;
        private readonly string _databaseName;
        private readonly ThroughputProperties _databaseThroughput;

        private Database _database;

        public CosmosClient Client { get; private set; }

        public DatabaseContext(CosmosClient client, string databaseName, ThroughputProperties databaseThroughput, ILogger<DatabaseContext> logger)
        {
            Client = client;
            _databaseName = databaseName;
            _databaseThroughput = databaseThroughput ?? ThroughputProperties.CreateManualThroughput(400);
            _logger = logger;
        }

        public abstract Task InitializeAsync(CancellationToken cancellationToken = default);

        internal async Task InitializeDatabaseAsync()
        {
            if (_database is null)
            {
                // Database
                var databaseResponse = await Client.CreateDatabaseIfNotExistsAsync(_databaseName, _databaseThroughput).ConfigureAwait(false);
                if (!(databaseResponse.StatusCode == System.Net.HttpStatusCode.OK || databaseResponse.StatusCode == System.Net.HttpStatusCode.Created))
                {
                    _logger.LogError($"Failed to Open or Create database: {databaseResponse.Diagnostics}");
                    throw new ApplicationException($"Failed to Open or Create database: {databaseResponse.Diagnostics}");
                }
                _logger.LogInformation($"Created/Connected Database: {databaseResponse.Database.Id}");
                _database = Client.GetDatabase(_databaseName);
            }
        }

        internal protected async Task RegisterContainerAsync(DatabaseCollection collection)
        {
            var colletionResponse = await _database.CreateContainerIfNotExistsAsync(
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
