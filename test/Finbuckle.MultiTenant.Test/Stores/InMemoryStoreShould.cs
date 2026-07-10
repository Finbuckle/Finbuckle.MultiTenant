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
    public async Task FailIfAddingDuplicateId()
    {
        var store = await CreateTestStore();

        Assert.False(await store.AddAsync(new TenantInfo { Id = "initech-id", Identifier = "other" }));
        Assert.Null(await store.GetByIdentifierAsync("other"));
    }

    [Theory]
    [InlineData("", "")]
    [InlineData(null, null)]
    [InlineData("", null)]
    [InlineData("a", "")]
    [InlineData("a", null)]
    [InlineData("", "a")]
    [InlineData(null, "a")]
    public async Task ThrowIfAddingTenantWithMissingIdOrIdentifier(string? id, string? identifier)
    {
        var store = new InMemoryStore<TenantInfo>();

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

    // Basic store functionality tested in MultiTenantStoresShould.cs

    protected override async Task<IMultiTenantStore<TenantInfo>> CreateTestStore()
    {
        var store = new InMemoryStore<TenantInfo>();

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
    public override async Task GetAllTenantsFromStoreAsync()
    {
        await base.GetAllTenantsFromStoreAsync();
    }
}
