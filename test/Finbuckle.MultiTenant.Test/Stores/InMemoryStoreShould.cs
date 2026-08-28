// Copyright Finbuckle LLC, Andrew White, and Contributors.
// Refer to the solution LICENSE file for more information.

using Finbuckle.MultiTenant.Abstractions;
using Finbuckle.MultiTenant.Stores;
using Xunit;

namespace Finbuckle.MultiTenant.Test.Stores;

public class InMemoryStoreShould : MultiTenantStoreTestBase
{
    [Fact]
    public async Task GetTenantInfoFromStoreCaseInsensitive()
    {
        var store = await CreateTestStore();
        Assert.Equal("initech", (await store.GetByIdentifierAsync("iNitEch"))?.Identifier);
    }

    [Fact]
    public async Task FailIfAddingDuplicateIdentifierIgnoringCase()
    {
        var store = await CreateTestStore();

        Assert.False(await store.AddAsync(new TenantInfo { Id = "other-id", Identifier = "INITECH" }));
        Assert.Equal("initech-id", (await store.GetByIdentifierAsync("initech"))?.Id);
    }

    [Fact]
    public async Task MoveIdentifierWhenUpdatingTenant()
    {
        var store = await CreateTestStore();

        Assert.True(await store.UpdateAsync(new TenantInfo { Id = "initech-id", Identifier = "initech2" }));
        Assert.Null(await store.GetByIdentifierAsync("initech"));
        Assert.Equal("initech2", (await store.GetByIdentifierAsync("initech2"))?.Identifier);
        Assert.Equal("initech2", (await store.GetAsync("initech-id"))?.Identifier);
    }

    [Fact]
    public async Task FailIfAddingDuplicateId()
    {
        var store = await CreateTestStore();

        Assert.False(await store.AddAsync(new TenantInfo { Id = "initech-id", Identifier = "other" }));
        Assert.Null(await store.GetByIdentifierAsync("other"));
    }

    [Theory]
    [InlineData(null, null)]
    [InlineData("a", "")]
    [InlineData("a", null)]
    [InlineData(null, "a")]
    public async Task ThrowIfAddingTenantWithMissingIdOrIdentifier(string? id, string? identifier)
    {
        var store = new InMemoryStore<TenantInfo,string>();

        await Assert.ThrowsAsync<MultiTenantException>(() =>
            store.AddAsync(new TenantInfo { Id = id!, Identifier = identifier! }));
    }

    [Fact]
    public async Task NotUpdateIfNewIdentifierAlreadyExists()
    {
        var store = await CreateTestStore();

        Assert.False(await store.UpdateAsync(new TenantInfo { Id = "initech-id", Identifier = "lol" }));
        Assert.Equal("initech", (await store.GetAsync("initech-id"))?.Identifier);
        Assert.Equal("lol-id", (await store.GetByIdentifierAsync("lol"))?.Id);
    }

    [Fact]
    public async Task ReturnFalseIfUpdatingMissingTenant()
    {
        var store = await CreateTestStore();

        Assert.False(await store.UpdateAsync(new TenantInfo { Id = "missing", Identifier = "missing" }));
    }

    [Fact]
    public async Task AllowCaseOnlyIdentifierUpdateWhenCaseInsensitive()
    {
        var store = await CreateTestStore();

        Assert.True(await store.UpdateAsync(new TenantInfo { Id = "initech-id", Identifier = "INITECH" }));
        Assert.Equal("INITECH", (await store.GetByIdentifierAsync("initech"))?.Identifier);
    }

    [Fact]
    public async Task KeepIdLookupAvailableDuringConcurrentIdentifierUpdates()
    {
        var store = await CreateTestStore();
        var updates = Enumerable.Range(0, 100)
            .Select(i => Task.Run(async () =>
            {
                var identifier = i % 2 == 0 ? "initech" : "initech2";
                await store.UpdateAsync(new TenantInfo { Id = "initech-id", Identifier = identifier });
                Assert.NotNull(await store.GetAsync("initech-id"));
            }));

        await Task.WhenAll(updates);
    }

    [Fact]
    public async Task RemainConsistentWhenStoredInstanceIsMutated()
    {
        // The store snapshots Id/Identifier on add; mutating the handed-out instance afterwards (a contract
        // violation by the caller) must not corrupt the store's own indexes.
        var store = new InMemoryStore<MutableTenantInfo, string>();
        var tenant = new MutableTenantInfo { Id = "initech-id", Identifier = "initech" };
        Assert.True(await store.AddAsync(tenant));

        var stored = await store.GetByIdentifierAsync("initech");
        stored!.Id = "mutated-id";
        stored.Identifier = "mutated";

        Assert.Same(tenant, await store.GetAsync("initech-id"));
        Assert.Same(tenant, await store.GetByIdentifierAsync("initech"));
        Assert.Null(await store.GetAsync("mutated-id"));
        Assert.Null(await store.GetByIdentifierAsync("mutated"));

        // a new tenant using the original keys is still a duplicate
        Assert.False(await store.AddAsync(new MutableTenantInfo { Id = "initech-id", Identifier = "other" }));
        Assert.False(await store.AddAsync(new MutableTenantInfo { Id = "other", Identifier = "initech" }));

        Assert.True(await store.UpdateAsync(new MutableTenantInfo { Id = "initech-id", Identifier = "initech2" }));
        Assert.Null(await store.GetByIdentifierAsync("initech"));
        Assert.Equal("initech2", (await store.GetAsync("initech-id"))!.Identifier);

        Assert.True(await store.RemoveAsync("initech-id"));
        Assert.Empty(await store.GetAllAsync());
    }

    [Fact]
    public async Task RemoveByIdentifierClearsIdLookup()
    {
        var store = await CreateTestStore();

        Assert.True(await store.RemoveByIdentifierAsync("INITECH"));
        Assert.Null(await store.GetAsync("initech-id"));
        Assert.False(await store.RemoveByIdentifierAsync("initech"));
        Assert.True(await store.AddAsync(new TenantInfo { Id = "initech-id", Identifier = "initech" }));
    }

    // Basic store functionality tested in MultiTenantStoresShould.cs

    protected override async Task<IMultiTenantStore<TenantInfo,string>> CreateTestStore()
    {
        var store = new InMemoryStore<TenantInfo,string>();

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
    public override async Task AddTenantInfoToStore()
    {
        await base.AddTenantInfoToStore();
    }

    [Fact]
    public override async Task RemoveTenantInfoFromStore()
    {
        await base.RemoveTenantInfoFromStore();
    }

    [Fact]
    public override async Task UpdateTenantInfoInStore()
    {
        await base.UpdateTenantInfoInStore();
    }

    [Fact]
    public override async Task RemoveTenantInfoFromStoreByIdentifier()
    {
        await base.RemoveTenantInfoFromStoreByIdentifier();
    }

    [Fact]
    public override async Task GetAllTenantsFromStoreAsync()
    {
        await base.GetAllTenantsFromStoreAsync();
    }
}
