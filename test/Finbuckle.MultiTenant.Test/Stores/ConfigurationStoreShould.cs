// Copyright Finbuckle LLC, Andrew White, and Contributors.
// Refer to the solution LICENSE file for more information.

using Finbuckle.MultiTenant.Abstractions;
using Finbuckle.MultiTenant.Stores;
using Microsoft.Extensions.Configuration;
using Xunit;

namespace Finbuckle.MultiTenant.Test.Stores;

public class ConfigurationStoreShould : MultiTenantStoreTestBase
{
    [Fact]
    public void NotThrowIfNoDefaultSection()
    {
        // See https://github.com/Finbuckle/Finbuckle.MultiTenant/issues/426
        var configBuilder = new ConfigurationBuilder();
        configBuilder.AddJsonFile("ConfigurationStoreTestSettings_NoDefaults.json");
        IConfiguration configuration = configBuilder.Build();

        // ReSharper disable once ObjectCreationAsStatement
        // Will throw if fail
        new ConfigurationStore<TenantInfo>(configuration);
    }

    [Fact]
    public void ThrowIfNullConfiguration()
    {
        Assert.Throws<ArgumentNullException>(() => new ConfigurationStore<TenantInfo>(null!));
    }

    [Fact]
    public void ThrowIfEmptyOrNullSection()
    {
        var configBuilder = new ConfigurationBuilder();
        configBuilder.AddJsonFile("ConfigurationStoreTestSettings.json");
        IConfiguration configuration = configBuilder.Build();

        Assert.Throws<ArgumentException>(() => new ConfigurationStore<TenantInfo>(configuration, ""));
        Assert.Throws<ArgumentException>(() => new ConfigurationStore<TenantInfo>(configuration, null!));
    }

    [Fact]
    public void ThrowIfInvalidSection()
    {
        var configBuilder = new ConfigurationBuilder();
        configBuilder.AddJsonFile("ConfigurationStoreTestSettings.json");
        IConfiguration configuration = configBuilder.Build();

        Assert.Throws<MultiTenantException>(() => new ConfigurationStore<TenantInfo>(configuration, "invalid"));
    }

    [Fact]
    public async Task IgnoreCaseWhenGettingTenantInfoFromStoreByIdentifier()
    {
        var store = await CreateTestStore();

        var tenant = await store.GetByIdentifierAsync("INITECH");

        Assert.NotNull(tenant);
        Assert.Equal("initech", tenant.Identifier);
    }

    [Fact]
    public async Task ThrowWhenTryingToGetIdentifierGivenNullIdentifier()
    {
        var store = await CreateTestStore();

        await Assert.ThrowsAsync<ArgumentNullException>(async () => await store.GetByIdentifierAsync(null!));
    }

    // Basic store functionality tested in MultiTenantStoresShould.cs

    protected override Task<IMultiTenantStore<TenantInfo>> CreateTestStore()
    {
        var configBuilder = new ConfigurationBuilder();
        configBuilder.AddJsonFile("ConfigurationStoreTestSettings.json");
        var configuration = configBuilder.Build();

        return Task.FromResult<IMultiTenantStore<TenantInfo>>(new ConfigurationStore<TenantInfo>(configuration));
    }

    protected override Task<IMultiTenantStore<TenantInfo>> PopulateTestStore(IMultiTenantStore<TenantInfo> store)
    {
        throw new NotImplementedException();
    }

    [Fact]
    public override async Task GetTenantInfoFromStoreById()
    {
        await base.GetTenantInfoFromStoreById();
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
    public override async Task ReturnNullWhenGettingByIdIfTenantInfoNotFound()
    {
        await base.ReturnNullWhenGettingByIdIfTenantInfoNotFound();
    }

    // [Fact(Skip = "Not valid for this store.")]
    public override Task AddTenantInfoToStore()
    {
        return Task.CompletedTask;
    }

    // [Fact(Skip = "Not valid for this store.")]
    public override Task RemoveTenantInfoFromStore()
    {
        return Task.CompletedTask;
    }

    // [Fact(Skip = "Not valid for this store.")]
    public override Task UpdateTenantInfoInStore()
    {
        return Task.CompletedTask;
    }

    [Fact]
    public override async Task GetAllTenantsFromStoreAsync()
    {
        await base.GetAllTenantsFromStoreAsync();
    }
    
    [Fact]
    public override async Task GetAllTenantsFromStoreAsyncSkip1Take1()
    {
        await base.GetAllTenantsFromStoreAsyncSkip1Take1();
    }
}