// Copyright Finbuckle LLC, Andrew White, and Contributors.
// Refer to the solution LICENSE file for more information.

using Finbuckle.MultiTenant.Abstractions;
using Finbuckle.MultiTenant.Stores;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Moq;
using Xunit;

namespace Finbuckle.MultiTenant.Test.Stores;

public class MultiTenantStoreWrapperShould : MultiTenantStoreTestBase
{
    // Basic store functionality tested in MultiTenantStoresShould.cs

    protected override async Task<IMultiTenantStore<TenantInfo>> CreateTestStore()
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

        return await PopulateTestStore(store);
    }

    [Fact]
    public override async Task GetTenantInfoFromStoreById()
    {
        await base.GetTenantInfoFromStoreById();
    }

    [Fact]
    public override async Task ReturnNullWhenGettingByIdIfTenantInfoNotFound()
    {
        await base.ReturnNullWhenGettingByIdIfTenantInfoNotFound();
    }

    [Fact]
    public async Task ThrowWhenGettingByIdIfTenantIdIsNull()
    {
        var store = await CreateTestStore();

        await Assert.ThrowsAsync<ArgumentNullException>(async () => await store.GetAsync(null!));
    }

    [Fact]
    public override async Task GetTenantInfoFromStoreByIdentifier()
    {
        await base.GetTenantInfoFromStoreByIdentifier();
    }

    [Fact]
    public override async Task ReturnNullWhenGettingByIdentifierIfTenantInfoNotFound()
    {
        await base.ReturnNullWhenGettingByIdentifierIfTenantInfoNotFound();
    }

    [Fact]
    public async Task ThrowWhenGettingByIdentifierIfTenantIdentifierIsNull()
    {
        var store = await CreateTestStore();

        await Assert.ThrowsAsync<ArgumentNullException>(async () => await store.GetByIdentifierAsync(null!));
    }

    [Fact]
    public override async Task AddTenantInfoToStore()
    {
        await base.AddTenantInfoToStore();
    }

    [Fact]
    public async Task ThrowWhenAddingIfTenantInfoIsNull()
    {
        var store = await CreateTestStore();
        await Assert.ThrowsAsync<ArgumentNullException>(async () => await store.AddAsync(null!));
    }

    [Fact]
    public async Task ThrowWhenAddingIfTenantInfoIdIsNull()
    {
        var store = await CreateTestStore();
        await Assert.ThrowsAsync<ArgumentNullException>(async () => await store.AddAsync(new TenantInfo { Id = null!, Identifier = "" }));
    }

    [Fact]
    public async Task ThrowWhenAddingIfTenantInfoIdentifierIsNull()
    {
        var store = await CreateTestStore();
        await Assert.ThrowsAsync<ArgumentNullException>(async () =>
            await store.AddAsync(new TenantInfo { Id = "initech-id", Identifier = null! }));
    }

    [Fact]
    public async Task ReturnFalseWhenAddingIfDuplicateId()
    {
        var store = await CreateTestStore();
        // Try to add with duplicate identifier.
        Assert.False(await store.AddAsync(new TenantInfo { Id = "initech-id", Identifier = "initech2" }));
    }

    [Fact]
    public async Task ReturnFalseWhenAddingIfDuplicateIdentifier()
    {
        var store = await CreateTestStore();
        // Try to add with duplicate identifier.
        Assert.False(await store.AddAsync(new TenantInfo { Id = "initech-id2", Identifier = "initech" }));
    }

    [Fact]
    public async Task ThrowWhenUpdatingIfTenantInfoIsNull()
    {
        var store = await CreateTestStore();

        await Assert.ThrowsAsync<ArgumentNullException>(async () => await store.UpdateAsync(null!));
    }

    [Fact]
    public async Task ThrowWhenUpdatingIfTenantInfoIdIsNull()
    {
        var store = await CreateTestStore();

        await Assert.ThrowsAsync<ArgumentNullException>(async () => await store.UpdateAsync(new TenantInfo { Id = null!, Identifier = "" }));
    }

    [Fact]
    public async Task ReturnFalseWhenUpdatingIfTenantIdIsNotFound()
    {
        var store = await CreateTestStore();

        var result = await store.UpdateAsync(new TenantInfo { Id = "not-found", Identifier = "" });
        Assert.False(result);
    }

    [Fact]
    public override async Task RemoveTenantInfoFromStore()
    {
        await base.RemoveTenantInfoFromStore();
    }

    [Fact]
    public async Task ThrowWhenRemovingIfTenantIdentifierIsNull()
    {
        var store = await CreateTestStore();
        await Assert.ThrowsAsync<ArgumentNullException>(async () => await store.RemoveAsync(null!));
    }

    [Fact]
    public async Task ReturnFalseWhenRemovingIfTenantInfoNotFound()
    {
        var store = await CreateTestStore();
        Assert.False(await store.RemoveAsync("not-there-identifier"));
    }

    [Fact]
    public override async Task UpdateTenantInfoInStore()
    {
        await base.UpdateTenantInfoInStore();
    }
}