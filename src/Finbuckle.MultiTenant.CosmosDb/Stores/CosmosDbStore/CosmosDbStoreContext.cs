using Microsoft.Azure.Cosmos;

namespace Finbuckle.MultiTenant.CosmosDb.Stores
{
    public class CosmosDbStoreContext
    {
        public CosmosDbStoreContext(Container container)
        {
            Container = container;
        }

        public Container Container { get; private set; }
    }
}
