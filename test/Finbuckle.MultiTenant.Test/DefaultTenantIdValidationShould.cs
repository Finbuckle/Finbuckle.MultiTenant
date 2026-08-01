// Copyright Finbuckle LLC, Andrew White, and Contributors.
// Refer to the solution LICENSE file for more information.

using Finbuckle.MultiTenant.Abstractions;
using Finbuckle.MultiTenant.StoreCaches;
using Finbuckle.MultiTenant.Stores;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Xunit;

namespace Finbuckle.MultiTenant.Test;

// Verifies default(TId) is uniformly treated as an unset/invalid tenant across the core components.
public class DefaultTenantIdValidationShould
{
    // ---- EnsureValid extension ----

    [Fact]
    public void EnsureValidThrowsForNull()
    {
        ITenantInfo<int>? tenantInfo = null;
        Assert.Throws<ArgumentNullException>(() => tenantInfo.EnsureValid());
    }

    [Theory]
    [InlineData(0)]      // default(int)
    public void EnsureValidThrowsForDefaultValueTypeId(int id)
    {
        var tenantInfo = new TenantInfo<int> { Id = id, Identifier = "ok" };
        Assert.Throws<MultiTenantException>(() => tenantInfo.EnsureValid());
    }

    [Fact]
    public void EnsureValidThrowsForDefaultGuidId()
    {
        var tenantInfo = new TenantInfo<Guid> { Id = Guid.Empty, Identifier = "ok" };
        Assert.Throws<MultiTenantException>(() => tenantInfo.EnsureValid());
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void EnsureValidThrowsForNullOrWhitespaceStringId(string? id)
    {
        var tenantInfo = new TenantInfo<string> { Id = id!, Identifier = "ok" };
        Assert.Throws<MultiTenantException>(() => tenantInfo.EnsureValid());
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void EnsureValidThrowsForMissingIdentifier(string? identifier)
    {
        var tenantInfo = new TenantInfo<int> { Id = 1, Identifier = identifier! };
        Assert.Throws<MultiTenantException>(() => tenantInfo.EnsureValid());
    }

    [Fact]
    public void EnsureValidPassesForValidTenant()
    {
        new TenantInfo<int> { Id = 1, Identifier = "ok" }.EnsureValid();
        new TenantInfo<Guid> { Id = Guid.NewGuid(), Identifier = "ok" }.EnsureValid();
        new TenantInfo<string> { Id = "id", Identifier = "ok" }.EnsureValid();
    }

    // ---- InMemoryStore ----

    [Fact]
    public async Task InMemoryStoreRejectsDefaultId()
    {
        var store = new InMemoryStore<TenantInfo<int>, int>();

        await Assert.ThrowsAsync<MultiTenantException>(() =>
            store.AddAsync(new TenantInfo<int> { Id = 0, Identifier = "x" }));
        await Assert.ThrowsAsync<ArgumentException>(() => store.GetAsync(0));
        await Assert.ThrowsAsync<ArgumentException>(() => store.RemoveAsync(0));
    }

    // ---- TenantManager ----

    [Fact]
    public async Task TenantManagerRejectsDefaultId()
    {
        var manager = new TenantManager<TenantInfo<int>, int>(new InMemoryStore<TenantInfo<int>, int>(), []);

        await Assert.ThrowsAsync<MultiTenantException>(() =>
            manager.AddAsync(new TenantInfo<int> { Id = 0, Identifier = "x" }));
        await Assert.ThrowsAsync<ArgumentException>(() => manager.GetAsync(0));
        await Assert.ThrowsAsync<ArgumentException>(() => manager.RemoveAsync(0));
    }

    // ---- Caches ----

    [Fact]
    public async Task MemoryCacheRejectsDefaultId()
    {
        var cache = new MemoryCacheStoreCache<TenantInfo<int>, int>(new MemoryCache(new MemoryCacheOptions()),
            Constants.TenantToken, new MemoryCacheEntryOptions());

        await Assert.ThrowsAsync<MultiTenantException>(() =>
            cache.SetAsync(new TenantInfo<int> { Id = 0, Identifier = "x" }));
        await Assert.ThrowsAsync<ArgumentException>(() => cache.GetAsync(0));
        await Assert.ThrowsAsync<ArgumentException>(() => cache.RemoveAsync(0));
    }

    // ---- ConfigurationStore ----

    [Fact]
    public void ConfigurationStoreRejectsDefaultIdOnLoad()
    {
        const string prefix = "Finbuckle:MultiTenant:Stores:ConfigurationStore:Tenants";
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                [$"{prefix}:0:Id"] = "0", // default(int) -> unset
                [$"{prefix}:0:Identifier"] = "initech",
            })
            .Build();

        Assert.Throws<MultiTenantException>(() => new ConfigurationStore<TenantInfo<int>, int>(configuration));
    }

    // ---- Ambient context ----

    [Fact]
    public void AmbientContextRejectsDefaultIdTenant()
    {
        var context = new AmbientTenantContext<TenantInfo<int>, int>();
        context.BeginScope();

        Assert.Throws<MultiTenantException>(() =>
            context.TenantInfo = new TenantInfo<int> { Id = 0, Identifier = "x" });
    }
}
