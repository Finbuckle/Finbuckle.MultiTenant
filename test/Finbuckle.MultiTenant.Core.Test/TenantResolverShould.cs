//    Copyright 2018 Andrew White
// 
//    Licensed under the Apache License, Version 2.0 (the "License");
//    you may not use this file except in compliance with the License.
//    You may obtain a copy of the License at
// 
//        http://www.apache.org/licenses/LICENSE-2.0
// 
//    Unless required by applicable law or agreed to in writing, software
//    distributed under the License is distributed on an "AS IS" BASIS,
//    WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//    See the License for the specific language governing permissions and
//    limitations under the License.

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