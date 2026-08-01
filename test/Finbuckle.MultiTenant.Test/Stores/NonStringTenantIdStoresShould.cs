// Copyright Finbuckle LLC, Andrew White, and Contributors.
// Refer to the solution LICENSE file for more information.

using Finbuckle.MultiTenant.Abstractions;
using Finbuckle.MultiTenant.Stores;
using Microsoft.Extensions.Configuration;
using Xunit;

namespace Finbuckle.MultiTenant.Test.Stores;

// Verifies the non-EFCore stores work with a value-type (int) tenant id.
public class NonStringTenantIdStoresShould
{
    [Fact]
    public async Task InMemoryStoreRoundTripsIntTenantId()
    {
        var store = new InMemoryStore<TenantInfo<int>, int>();
        await store.AddAsync(new TenantInfo<int> { Id = 1, Identifier = "initech" });
        await store.AddAsync(new TenantInfo<int> { Id = 2, Identifier = "lol" });

        Assert.Equal("initech", (await store.GetAsync(1))!.Identifier);
        Assert.Equal(2, (await store.GetByIdentifierAsync("lol"))!.Id);

        Assert.True(await store.RemoveAsync(1));
        Assert.Null(await store.GetAsync(1));
        Assert.NotNull(await store.GetAsync(2));
    }

    [Fact]
    public async Task ConfigurationStoreBindsIntTenantId()
    {
        const string prefix = "Finbuckle:MultiTenant:Stores:ConfigurationStore:Tenants";
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                [$"{prefix}:0:Id"] = "1",
                [$"{prefix}:0:Identifier"] = "initech",
                [$"{prefix}:1:Id"] = "2",
                [$"{prefix}:1:Identifier"] = "lol",
            })
            .Build();

        var store = new ConfigurationStore<TenantInfo<int>, int>(configuration);

        var byIdentifier = await store.GetByIdentifierAsync("initech");
        Assert.NotNull(byIdentifier);
        Assert.Equal(1, byIdentifier.Id); // config value "1" bound to int Id
        Assert.Equal("initech", byIdentifier.Identifier);

        Assert.Equal("lol", (await store.GetAsync(2))!.Identifier);
    }
}
