using System.Threading.Tasks;
using Finbuckle.MultiTenant.Test.Stores;
using Xunit;

namespace Finbuckle.MultiTenant.Cosmos.Test;

public class CosmosStoreShould : MultiTenantStoreTestBase, IClassFixture<CosmosClientFixture>
{
    private CosmosClientFixture _cosmosClientFixture;

    public CosmosStoreShould(CosmosClientFixture cosmosClientFixture)
    {
        _cosmosClientFixture = cosmosClientFixture;
    }

    protected override IMultiTenantStore<TenantInfo> CreateTestStore()
    {
        var container =
            _cosmosClientFixture.CosmosClient.GetContainer(_cosmosClientFixture.DatabaseId,
                _cosmosClientFixture.ContainerId);
        var store = new CosmosStore<TenantInfo>(container);
        return PopulateTestStore(store);
    }

    protected override IMultiTenantStore<TenantInfo> PopulateTestStore(IMultiTenantStore<TenantInfo> store)
    {
        // Clear out data for each test.
        var tenants = store.GetAllAsync().Result;
        foreach (var tenant in tenants)
            store.TryRemoveAsync(tenant.Identifier).Wait();

        base.PopulateTestStore(store);
        return store;
    }

    [Fact]
    public override void GetTenantInfoFromStoreById()
    {
        base.GetTenantInfoFromStoreById();
    }

    [Fact]
    public override void ReturnNullWhenGettingByIdIfTenantInfoNotFound()
    {
        base.ReturnNullWhenGettingByIdIfTenantInfoNotFound();
    }

    [Fact]
    public override void GetTenantInfoFromStoreByIdentifier()
    {
        base.GetTenantInfoFromStoreByIdentifier();
    }

    [Fact]
    public override void ReturnNullWhenGettingByIdentifierIfTenantInfoNotFound()
    {
        base.ReturnNullWhenGettingByIdentifierIfTenantInfoNotFound();
    }

    [Fact]
    public override void AddTenantInfoToStore()
    {
        base.AddTenantInfoToStore();
    }

    [Fact]
    public override void UpdateTenantInfoInStore()
    {
        base.UpdateTenantInfoInStore();
    }

    [Fact]
    public override void RemoveTenantInfoFromStore()
    {
        base.RemoveTenantInfoFromStore();
    }

    [Fact]
    public override void GetAllTenantsFromStoreAsync()
    {
        base.GetAllTenantsFromStoreAsync();
    }
}