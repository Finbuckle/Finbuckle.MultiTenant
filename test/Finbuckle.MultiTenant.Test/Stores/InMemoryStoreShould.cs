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

        var ti1 = new TenantInfo { Id = "initech", Identifier = "initech", Name = "initech" };
        var ti2 = new TenantInfo { Id = "lol", Identifier = "lol", Name = "lol" };
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
        var ti1 = new TenantInfo { Id = "initech", Identifier = "initech", Name = "initech" };
        var ti2 = new TenantInfo { Id = "iNiTEch", Identifier = "iNiTEch", Name = "Initech" };
        Assert.False(await store.AddAsync(ti1));
        Assert.True(await store.AddAsync(ti2));
    }

    [Fact]
    public void ThrowIfDuplicateIdentifierInOptionsTenants()
    {
        var services = new ServiceCollection();
        services.AddOptions().Configure<InMemoryStoreOptions<TenantInfo>>(options =>
        {
            options.Tenants.Add(new TenantInfo { Id = "lol", Identifier = "lol", Name = "LOL" });
            options.Tenants.Add(new TenantInfo { Id = "lol", Identifier = "lol", Name = "LOL" });
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
            options.Tenants.Add(new TenantInfo { Id = id!, Identifier = identifier!, Name = "LOL" });
        });
        var sp = services.BuildServiceProvider();

        Assert.Throws<MultiTenantException>(() =>
            new InMemoryStore<TenantInfo>(sp.GetRequiredService<IOptions<InMemoryStoreOptions<TenantInfo>>>()));
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