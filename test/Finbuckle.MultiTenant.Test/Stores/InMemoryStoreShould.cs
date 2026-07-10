// Copyright Finbuckle LLC, Andrew White, and Contributors.
// Refer to the solution LICENSE file for more information.

using Finbuckle.MultiTenant.Abstractions;
using Finbuckle.MultiTenant.Stores;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Moq;
using Xunit;

namespace Finbuckle.MultiTenant.Test.Stores;

public class InMemoryStoreShould : MultiTenantStoreTestBase
{
    private async Task<IMultiTenantStore<TenantInfo>> CreateCaseSensitiveTestStore()
    {
        var services = new ServiceCollection();
        services.AddOptions().Configure<InMemoryStoreOptions<TenantInfo>>(o => o.IsCaseSensitive = true);
        var sp = services.BuildServiceProvider();

        var store = new InMemoryStore<TenantInfo>(sp.GetRequiredService<IOptions<InMemoryStoreOptions<TenantInfo>>>());

        var ti1 = new TenantInfo { Id = "initech", Identifier = "initech" };
        var ti2 = new TenantInfo { Id = "lol", Identifier = "lol" };
        await store.AddAsync(ti1);
        await store.AddAsync(ti2);

        return store;
    }

    [Fact]
    public async Task GetTenantInfoFromStoreCaseInsensitiveByDefault()
    {
        var store = await CreateTestStore();
        Assert.Equal("initech", (await store.GetByIdentifierAsync("iNitEch"))?.Identifier);
    }

    [Fact]
    public async Task GetTenantInfoFromStoreCaseSensitive()
    {
        var store = await CreateCaseSensitiveTestStore();
        Assert.Equal("initech", (await store.GetByIdentifierAsync("initech"))?.Identifier);
        Assert.Null(await store.GetByIdentifierAsync("iNitEch"));
    }

    [Fact]
    public async Task FailIfAddingDuplicateCaseSensitive()
    {
        var store = await CreateCaseSensitiveTestStore();
        var ti1 = new TenantInfo { Id = "initech", Identifier = "initech" };
        var ti2 = new TenantInfo { Id = "iNiTEch", Identifier = "iNiTEch" };
        Assert.False(await store.AddAsync(ti1));
        Assert.True(await store.AddAsync(ti2));
    }

    [Fact]
    public void ThrowIfDuplicateIdentifierInOptionsTenants()
    {
        var services = new ServiceCollection();
        services.AddOptions().Configure<InMemoryStoreOptions<TenantInfo>>(options =>
        {
            options.Tenants.Add(new TenantInfo { Id = "lol", Identifier = "lol" });
            options.Tenants.Add(new TenantInfo { Id = "lol", Identifier = "lol" });
        });
        var sp = services.BuildServiceProvider();

        Assert.Throws<MultiTenantException>(() =>
            new InMemoryStore<TenantInfo>(sp.GetRequiredService<IOptions<InMemoryStoreOptions<TenantInfo>>>()));
    }

    [Theory]
    [InlineData("", "")]
    [InlineData(null, null)]
    [InlineData("", null)]
    [InlineData("a", "")]
    [InlineData("a", null)]
    [InlineData("", "a")]
    [InlineData(null, "a")]
    public void ThrowIfMissingIdOrIdentifierInOptionsTenants(string? id, string? identifier)
    {
        var services = new ServiceCollection();
        services.AddOptions().Configure<InMemoryStoreOptions<TenantInfo>>(options =>
        {
            options.Tenants.Add(new TenantInfo { Id = id!, Identifier = identifier! });
        });
        var sp = services.BuildServiceProvider();

        Assert.Throws<MultiTenantException>(() =>
            new InMemoryStore<TenantInfo>(sp.GetRequiredService<IOptions<InMemoryStoreOptions<TenantInfo>>>()));
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
    public async Task MoveCaseOnlyIdentifierWhenCaseSensitive()
    {
        var store = await CreateCaseSensitiveTestStore();

        Assert.True(await store.UpdateAsync(new TenantInfo { Id = "initech", Identifier = "INITECH" }));
        Assert.Null(await store.GetByIdentifierAsync("initech"));
        Assert.Equal("INITECH", (await store.GetByIdentifierAsync("INITECH"))?.Identifier);
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
        var optionsMock = new Mock<IOptions<InMemoryStoreOptions<TenantInfo>>>();
        var options = new InMemoryStoreOptions<TenantInfo>
        {
            IsCaseSensitive = false,
            Tenants = new List<TenantInfo>()
        };
        optionsMock.Setup(o => o.Value).Returns(options);
        var store = new InMemoryStore<TenantInfo>(optionsMock.Object);

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
