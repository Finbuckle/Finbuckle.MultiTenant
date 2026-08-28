// Copyright Finbuckle LLC, Andrew White, and Contributors.
// Refer to the solution LICENSE file for more information.

using Finbuckle.MultiTenant.Abstractions;
using Xunit;

#pragma warning disable xUnit1013 // Public method should be marked as test

namespace Finbuckle.MultiTenant.Test.Stores;

/// <summary>
/// Store test base parameterized on the tenant info type so the same behavior can be proven for the library's
/// mutable <see cref="TenantInfo"/> and for a constructor-only immutable implementation.
/// </summary>
public abstract class MultiTenantStoreTestBase<TTenantInfo> where TTenantInfo : ITenantInfo<string>
{
    protected abstract TTenantInfo NewTenant(string id, string identifier);

    protected abstract Task<IMultiTenantStore<TTenantInfo, string>> CreateTestStore();

    protected virtual async Task<IMultiTenantStore<TTenantInfo, string>> PopulateTestStore(IMultiTenantStore<TTenantInfo, string> store)
    {
        await store.AddAsync(NewTenant("initech-id", "initech"));
        await store.AddAsync(NewTenant("lol-id", "lol"));

        return store;
    }

    //[Fact]
    public virtual async Task GetTenantInfoFromStoreById()
    {
        var store = await CreateTestStore();

        Assert.Equal("initech", (await store.GetAsync("initech-id"))!.Identifier);
    }

    //[Fact]
    public virtual async Task ReturnNullWhenGettingByIdIfTenantInfoNotFound()
    {
        var store = await CreateTestStore();

        Assert.Null(await store.GetAsync("fake123"));
    }

    //[Fact]
    public virtual async Task GetTenantInfoFromStoreByIdentifier()
    {
        var store = await CreateTestStore();

        Assert.Equal("initech", (await store.GetByIdentifierAsync("initech"))!.Identifier);
    }

    //[Fact]
    public virtual async Task ReturnNullWhenGettingByIdentifierIfTenantInfoNotFound()
    {
        var store = await CreateTestStore();
        Assert.Null(await store.GetByIdentifierAsync("fake123"));
    }

    //[Fact]
    public virtual async Task AddTenantInfoToStore()
    {
        var store = await CreateTestStore();

        Assert.Null(await store.GetByIdentifierAsync("identifier"));
        Assert.True(await store.AddAsync(NewTenant("id", "identifier")));
        Assert.NotNull(await store.GetByIdentifierAsync("identifier"));
    }

    //[Fact]
    public virtual async Task UpdateTenantInfoInStore()
    {
        var store = await CreateTestStore();

        var result = await store.UpdateAsync(NewTenant("initech-id", "initech2"));
        Assert.True(result);
        Assert.Null(await store.GetByIdentifierAsync("initech"));
        Assert.Equal("initech2", (await store.GetByIdentifierAsync("initech2"))?.Identifier);
        Assert.Equal("initech2", (await store.GetAsync("initech-id"))?.Identifier);
    }

    //[Fact]
    public virtual async Task RemoveTenantInfoFromStore()
    {
        var store = await CreateTestStore();
        Assert.NotNull(await store.GetByIdentifierAsync("initech"));
        Assert.True(await store.RemoveAsync("initech-id"));
        Assert.Null(await store.GetByIdentifierAsync("initech"));
    }

    //[Fact]
    public virtual async Task RemoveTenantInfoFromStoreByIdentifier()
    {
        var store = await CreateTestStore();
        Assert.NotNull(await store.GetByIdentifierAsync("initech"));
        Assert.True(await store.RemoveByIdentifierAsync("initech"));
        Assert.Null(await store.GetByIdentifierAsync("initech"));
    }

    //[Fact]
    public virtual async Task GetAllTenantsFromStoreAsync()
    {
        var store = await CreateTestStore();
        Assert.Equal(2, (await store.GetAllAsync()).Count());
    }

    //[Fact]
    public virtual async Task GetAllTenantsFromStoreAsyncSkip1Take1()
    {
        var store = await CreateTestStore();
        var tenants = (await store.GetAllAsync(1, 1)).ToList();
        Assert.Single(tenants);

        var tenant = tenants.FirstOrDefault();
        Assert.NotNull(tenant);
        Assert.Equal("lol", tenant.Identifier);
    }
}

/// <summary>
/// Store test base using the library's <see cref="TenantInfo"/> implementation.
/// </summary>
public abstract class MultiTenantStoreTestBase : MultiTenantStoreTestBase<TenantInfo>
{
    protected override TenantInfo NewTenant(string id, string identifier) => new() { Id = id, Identifier = identifier };
}

/// <summary>
/// Store test base using the constructor-only <see cref="ImmutableTenantInfo"/> implementation.
/// </summary>
public abstract class ImmutableTenantStoreTestBase : MultiTenantStoreTestBase<ImmutableTenantInfo>
{
    protected override ImmutableTenantInfo NewTenant(string id, string identifier) => new(id, identifier);
}
