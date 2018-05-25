using System;
using System.Collections.Concurrent;
using Finbuckle.MultiTenant.Core;
using Xunit;

public class TenantResolverShould
{
    private InMemoryMultiTenantStore CreateTestStore()
    {
        var store = new InMemoryMultiTenantStore();
        store.TryAdd(new TenantContext("initech", "initech", "Initech", null, null, null));

        return store;
    }

    [Fact]
    public void GetTenantFromStore()
    {
        var store = CreateTestStore();

        var strat = new StaticMultiTenantStrategy("initech");
        var resolver = new TenantResolver(store, strat);
        var tc = resolver.ResolveAsync(null).Result;

        Assert.Equal("initech", tc.Id);
        Assert.Equal("initech", tc.Identifier);
        Assert.Equal("Initech", tc.Name);
        Assert.Equal(typeof(StaticMultiTenantStrategy), tc.MultiTenantStrategyType);
        Assert.Equal(typeof(InMemoryMultiTenantStore), tc.MultiTenantStoreType);
    }
}