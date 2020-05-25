//    Copyright 2018-2020 Andrew White
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
using System.Reflection;
using Finbuckle.MultiTenant;
using Finbuckle.MultiTenant.Core;
using Finbuckle.MultiTenant.Options;
using Microsoft.Extensions.Options;
using Xunit;

public partial class MultiTenantOptionsCacheShould
{
    [Theory]
    [InlineData("")]
    [InlineData(null)]
    [InlineData("name")]
    public void AddNamedOptionsForCurrentTenantOnlyOnAdd(string name)
    {
        var ti = new TenantInfo { Id = "test-id-123" };
        var tc = new MultiTenantContext<TenantInfo>();
        tc.TenantInfo = ti;
        var tca = new MultiTenantContextAccessor<TenantInfo>();
        tca.MultiTenantContext = tc;
        var cache = new MultiTenantOptionsCache<TestOptions, TenantInfo>(tca);

        var options = new TestOptions();

        // Add new options.
        var result = cache.TryAdd(name, options);
        Assert.True(result);

        // Fail adding options under same name.
        result = cache.TryAdd(name, options);
        Assert.False(result);

        // Change the tenant id and confirm options can be added again.
        ti.Id = "diff_id";
        result = cache.TryAdd(name, options);
        Assert.True(result);
    }

    [Fact]
    public void HandleNullMultiTenantContextOnAdd()
    {
        var tca = new MultiTenantContextAccessor<TenantInfo>();
        var cache = new MultiTenantOptionsCache<TestOptions, TenantInfo>(tca);

        var options = new TestOptions();

        // Add new options, ensure no exception caused by null MultiTenantContext.
        var result = cache.TryAdd("", options);
        Assert.True(result);
    }

    [Fact]
    public void HandleNullMultiTenantContextOnGetOrAdd()
    {
        var tca = new MultiTenantContextAccessor<TenantInfo>();
        var cache = new MultiTenantOptionsCache<TestOptions, TenantInfo>(tca);

        var options = new TestOptions();

        // Add new options, ensure no exception caused by null MultiTenantContext.
        var result = cache.GetOrAdd("", () => options);
        Assert.NotNull(result);
    }

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    [InlineData("name")]
    public void GetOrAddNamedOptionForCurrentTenantOnly(string name)
    {
        var ti = new TenantInfo { Id = "test-id-123"};
        var tc = new MultiTenantContext<TenantInfo>();
        tc.TenantInfo = ti;
        var tca = new MultiTenantContextAccessor<TenantInfo>();
        tca.MultiTenantContext = tc;
        var cache = new MultiTenantOptionsCache<TestOptions, TenantInfo>(tca);

        var options = new TestOptions();
        var options2 = new TestOptions();

        // Add new options.
        var result = cache.GetOrAdd(name, () => options);
        Assert.Same(options, result);

        // Get the existing options if exists.
        result = cache.GetOrAdd(name, () => options2);
        Assert.NotSame(options2, result);

        // Confirm different tenant on same object is an add (ie it didn't exist there).
        ti.Id = "diff_id";
        result = cache.GetOrAdd(name, () => options2);
        Assert.Same(options2, result);
    }

    [Fact]
    public void ThrowsIfGetOrAddFactoryIsNull()
    {
        var tc = new MultiTenantContext<TenantInfo>();
        var tca = new MultiTenantContextAccessor<TenantInfo>();
        tca.MultiTenantContext = tc;
        var cache = new MultiTenantOptionsCache<TestOptions, TenantInfo>(tca);

        Assert.Throws<ArgumentNullException>(() => cache.GetOrAdd("", null));
    }

    [Fact]
    public void ThrowIfContructorParamIsNull()
    {
        var tc = new MultiTenantContext<TenantInfo>();
        var tca = new MultiTenantContextAccessor<TenantInfo>();
        tca.MultiTenantContext = tc;

        Assert.Throws<ArgumentNullException>(() => new MultiTenantOptionsCache<TestOptions, TenantInfo>(null));
    }

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    [InlineData("name")]
    public void RemoveNamedOptionsForCurrentTenantOnly(string name)
    {
        var ti = new TenantInfo { Id = "test-id-123" };
        var tc = new MultiTenantContext<TenantInfo>();
        tc.TenantInfo = ti;
        var tca = new MultiTenantContextAccessor<TenantInfo>();
        tca.MultiTenantContext = tc;
        var cache = new MultiTenantOptionsCache<TestOptions, TenantInfo>(tca);

        var options = new TestOptions();

        // Add new options.
        var result = cache.TryAdd(name, options);
        Assert.True(result);

        // Add under a different tenant.
        ti.Id = "diff_id";
        result = cache.TryAdd(name, options);
        Assert.True(result);
        result = cache.TryAdd("diffname", options);
        Assert.True(result);

        // Remove named options for current tenant.
        result = cache.TryRemove(name);
        Assert.True(result);
        var tenantCache = (ConcurrentDictionary<string, IOptionsMonitorCache<TestOptions>>)cache.GetType().
            GetField("map", BindingFlags.NonPublic | BindingFlags.Instance).
            GetValue(cache);

        dynamic tenantInternalCache = tenantCache[ti.Id].GetType().GetField("_cache", BindingFlags.NonPublic | BindingFlags.Instance)
            .GetValue(tenantCache[ti.Id]);

        // Assert named options removed and other options on tenant left as-is.
        Assert.False(tenantInternalCache.Keys.Contains(name ?? ""));
        Assert.True(tenantInternalCache.Keys.Contains("diffname"));

        // Assert other tenant not affected.
        ti.Id = "test-id-123";
        tenantInternalCache = tenantCache[ti.Id].GetType().GetField("_cache", BindingFlags.NonPublic | BindingFlags.Instance)
            .GetValue(tenantCache[ti.Id]);
        Assert.True(tenantInternalCache.ContainsKey(name ?? ""));
    }

    [Fact]
    public void ClearOptionsForCurrentTenantOnly()
    {
        var ti = new TenantInfo { Id = "test-id-123" };
        var tc = new MultiTenantContext<TenantInfo>();
        tc.TenantInfo = ti;
        var tca = new MultiTenantContextAccessor<TenantInfo>();
        tca.MultiTenantContext = tc;
        var cache = new MultiTenantOptionsCache<TestOptions, TenantInfo>(tca);

        var options = new TestOptions();

        // Add new options.
        var result = cache.TryAdd("", options);
        Assert.True(result);

        // Add under a different tenant.
        ti.Id = "diff_id";
        result = cache.TryAdd("", options);
        Assert.True(result);

        // Clear options on first tenant.
        ti.Id = "test-id-123";
        cache.Clear();

        // Assert options cleared on this tenant.
        var tenantCache = (ConcurrentDictionary<string, IOptionsMonitorCache<TestOptions>>)cache.GetType().
            GetField("map", BindingFlags.NonPublic | BindingFlags.Instance).
            GetValue(cache);

        dynamic tenantInternalCache = tenantCache[ti.Id].GetType().GetField("_cache", BindingFlags.NonPublic | BindingFlags.Instance)
            .GetValue(tenantCache[ti.Id]);
        Assert.True(tenantInternalCache.IsEmpty);

        // Assert options still exist on other tenant.
        ti.Id = "diff_id";
        tenantInternalCache = tenantCache[ti.Id].GetType().GetField("_cache", BindingFlags.NonPublic | BindingFlags.Instance)
            .GetValue(tenantCache[ti.Id]);
        Assert.False(tenantInternalCache.IsEmpty);
    }

    [Fact]
    public void ClearOptionsForTenantIdOnly()
    {
        var ti = new TenantInfo { Id = "test-id-123" };
        var tc = new MultiTenantContext<TenantInfo>();
        tc.TenantInfo = ti;
        var tca = new MultiTenantContextAccessor<TenantInfo>();
        tca.MultiTenantContext = tc;
        var cache = new MultiTenantOptionsCache<TestOptions, TenantInfo>(tca);

        var options = new TestOptions();

        // Add new options.
        var result = cache.TryAdd("", options);
        Assert.True(result);

        // Add under a different tenant.
        ti.Id = "diff_id";
        result = cache.TryAdd("", options);
        Assert.True(result);

        // Clear options on first tenant.
        cache.Clear("test-id-123");

        // Assert options cleared on this tenant.
        var tenantCache = (ConcurrentDictionary<string, IOptionsMonitorCache<TestOptions>>)cache.GetType().
            GetField("map", BindingFlags.NonPublic | BindingFlags.Instance).
            GetValue(cache);

        dynamic tenantInternalCache = tenantCache[ti.Id].GetType().GetField("_cache", BindingFlags.NonPublic | BindingFlags.Instance)
            .GetValue(tenantCache["test-id-123"]);
        Assert.True(tenantInternalCache.IsEmpty);

        // Assert options still exist on other tenant.
        tenantInternalCache = tenantCache["diff_id"].GetType().GetField("_cache", BindingFlags.NonPublic | BindingFlags.Instance)
            .GetValue(tenantCache["diff_id"]);
        Assert.False(tenantInternalCache.IsEmpty);
    }

    [Fact]
    public void ClearAllOptionsForClearAll()
    {
        var ti = new TenantInfo { Id = "test-id-123" };
        var tc = new MultiTenantContext<TenantInfo>();
        tc.TenantInfo = ti;
        var tca = new MultiTenantContextAccessor<TenantInfo>();
        tca.MultiTenantContext = tc;
        var cache = new MultiTenantOptionsCache<TestOptions, TenantInfo>(tca);

        var options = new TestOptions();

        // Add new options.
        var result = cache.TryAdd("", options);
        Assert.True(result);

        // Add under a different tenant.
        ti.Id = "diff_id";
        result = cache.TryAdd("", options);
        Assert.True(result);

        // Clear all options.
        cache.ClearAll();

        // Assert options cleared on this tenant.
        var tenantCache = (ConcurrentDictionary<string, IOptionsMonitorCache<TestOptions>>)cache.GetType().
            GetField("map", BindingFlags.NonPublic | BindingFlags.Instance).
            GetValue(cache);

        dynamic tenantInternalCache = tenantCache[ti.Id].GetType().GetField("_cache", BindingFlags.NonPublic | BindingFlags.Instance)
            .GetValue(tenantCache[ti.Id]);
        Assert.True(tenantInternalCache.IsEmpty);

        // Assert options cleared on other tenant.
        ti.Id = "diff_id";
        tenantInternalCache = tenantCache[ti.Id].GetType().GetField("_cache", BindingFlags.NonPublic | BindingFlags.Instance)
            .GetValue(tenantCache[ti.Id]);
        Assert.True(tenantInternalCache.IsEmpty);
    }
}