// Copyright Finbuckle LLC, Andrew White, and Contributors.
// Refer to the solution LICENSE file for more information.

using Finbuckle.MultiTenant.Abstractions;
using Finbuckle.MultiTenant.Stores;
using Finbuckle.MultiTenant.Stores.InMemoryStore;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Moq;
using Xunit;

namespace Finbuckle.MultiTenant.Test.Stores;

public class MultiTenantStoreWrapperShould : MultiTenantStoreTestBase
{
    // Basic store functionality tested in MultiTenantStoresShould.cs

    protected override IMultiTenantStore<TenantInfo> CreateTestStore()
    {
        var optionsMock = new Mock<IOptions<InMemoryStoreOptions<TenantInfo>>>();
        var options = new InMemoryStoreOptions<TenantInfo>
        {
            IsCaseSensitive = false,
            Tenants = new List<TenantInfo>()
        };
        optionsMock.Setup(o => o.Value).Returns(options);
        var store = new MultiTenantStoreWrapper<TenantInfo>(
            new InMemoryStore<TenantInfo>(optionsMock.Object), NullLogger.Instance);

        return PopulateTestStore(store);
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
    public void ThrowWhenGettingByIdIfTenantIdIsNull()
    {
        var store = CreateTestStore();

        var e = Assert.Throws<AggregateException>(() => store.TryGetAsync(null!).Result);
        Assert.IsType<ArgumentNullException>(e.InnerException);
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
    public void ThrowWhenGettingByIdentifierIfTenantIdentifierIsNull()
    {
        var store = CreateTestStore();

        var e = Assert.Throws<AggregateException>(() => store.TryGetByIdentifierAsync(null!).Result);
        Assert.IsType<ArgumentNullException>(e.InnerException);
    }

    [Fact]
    public override void AddTenantInfoToStore()
    {
        base.AddTenantInfoToStore();
    }

    [Fact]
    public void ThrowWhenAddingIfTenantInfoIsNull()
    {
        var store = CreateTestStore();
        var e = Assert.Throws<AggregateException>(() => store.TryAddAsync(null!).Result);
        Assert.IsType<ArgumentNullException>(e.InnerException);
    }

    [Fact]
    public void ThrowWhenAddingIfTenantInfoIdIsNull()
    {
        var store = CreateTestStore();
        var e = Assert.Throws<AggregateException>(() => store.TryAddAsync(new TenantInfo()).Result);
        Assert.IsType<ArgumentNullException>(e.InnerException);
    }

    [Fact]
    public void ThrowWhenAddingIfTenantInfoIdentifierIsNull()
    {
        var store = CreateTestStore();
        var e = Assert.Throws<AggregateException>(() =>
            store.TryAddAsync(new TenantInfo() { Id = "initech-id" }).Result);
        Assert.IsType<ArgumentNullException>(e.InnerException);
    }

    [Fact]
    public async Task ReturnFalseWhenAddingIfDuplicateId()
    {
        var store = CreateTestStore();
        // Try to add with duplicate identifier.
        Assert.False(await store.TryAddAsync(new TenantInfo { Id = "initech-id", Identifier = "initech2" }));
    }

    [Fact]
    public async Task ReturnFalseWhenAddingIfDuplicateIdentifier()
    {
        var store = CreateTestStore();
        // Try to add with duplicate identifier.
        Assert.False(await store.TryAddAsync(new TenantInfo { Id = "initech-id2", Identifier = "initech" }));
    }

    [Fact]
    public async Task ThrowWhenUpdatingIfTenantInfoIsNull()
    {
        var store = CreateTestStore();

        var e = await Assert.ThrowsAsync<ArgumentNullException>(async () => await store.TryUpdateAsync(null!));
    }

    [Fact]
    public async Task ThrowWhenUpdatingIfTenantInfoIdIsNull()
    {
        var store = CreateTestStore();

        var e = await Assert.ThrowsAsync<ArgumentNullException>(async () => await store.TryUpdateAsync(new TenantInfo()));
    }

    [Fact]
    public async Task ReturnFalseWhenUpdatingIfTenantIdIsNotFound()
    {
        var store = CreateTestStore();

        var result = await store.TryUpdateAsync(new TenantInfo { Id = "not-found" });
        Assert.False(result);
    }

    [Fact]
    public override void RemoveTenantInfoFromStore()
    {
        base.RemoveTenantInfoFromStore();
    }

    [Fact]
    public void ThrowWhenRemovingIfTenantIdentifierIsNull()
    {
        var store = CreateTestStore();
        var e = Assert.Throws<AggregateException>(() => store.TryRemoveAsync(null!).Result);
        Assert.IsType<ArgumentNullException>(e.InnerException);
    }

    [Fact]
    public async Task ReturnFalseWhenRemovingIfTenantInfoNotFound()
    {
        var store = CreateTestStore();
        Assert.False(await store.TryRemoveAsync("not-there-identifier"));
    }

    [Fact]
    public override void UpdateTenantInfoInStore()
    {
        base.UpdateTenantInfoInStore();
    }
}