using Finbuckle.MultiTenant;
using Finbuckle.MultiTenant.CosmosDb.Stores;

using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Logging;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace CosmosStoreSample.Data
{
    public class MultiTenantStoreDbContext : DatabaseContext
    {
        public DatabaseCollection Tenants { get; set; }

        public MultiTenantStoreDbContext(CosmosClient client, string databaseName, ILogger<MultiTenantStoreDbContext> logger)
            : base(client, databaseName, ThroughputProperties.CreateManualThroughput(400), logger)
        {
            Tenants = new DatabaseCollection(nameof(Tenants), $"/{nameof(TenantInfo.Id).ToLower()}", ThroughputProperties.CreateManualThroughput(400));
        }

        public override async Task InitializeAsync(CancellationToken cancellationToken = default)
        {
            var createContainerTasks = new List<Task>();

            createContainerTasks.Add(RegisterContainerAsync(Tenants));

            await Task.WhenAll(createContainerTasks);
        }
    }
}
