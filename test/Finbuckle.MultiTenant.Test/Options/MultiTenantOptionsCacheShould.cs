// Copyright Finbuckle LLC, Andrew White, and Contributors.
// Refer to the solution LICENSE file for more information.

using System.Collections.Concurrent;
using System.ComponentModel.DataAnnotations;
using System.Reflection;
using Finbuckle.MultiTenant.Options;
using Microsoft.Extensions.Options;
using Xunit;

namespace Finbuckle.MultiTenant.Test.Options;

public class MultiTenantOptionsCacheShould
{
    public class TestOptions
    {
        [Required]
        public string? DefaultConnectionString { get; set; }
    }

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    [InlineData("name")]
    public void AddNamedOptionsForCurrentTenantOnlyOnAdd(string? name)
    {
        var cache = new MultiTenantOptionsCache<TestOptions>();

        var options = new TestOptions();
        const string tenantId = "test-id-123";

        // Add new options.
        var result = cache.TryAdd(name, tenantId, options);
        Assert.True(result);

        // Fail adding options under same name.
        result = cache.TryAdd(name, tenantId, options);
        Assert.False(result);

        // Change the tenant id and confirm options can be added again.
        result = cache.TryAdd(name, "diff", options);
        Assert.True(result);
    }

    [Fact]
    public void HandleNullTenantIdOnAdd()
    {
        var cache = new MultiTenantOptionsCache<TestOptions>();

        var options = new TestOptions();

        // Add new options for the default (null) tenant id bucket.
        var result = cache.TryAdd("", null, options);
        Assert.True(result);
    }

    [Fact]
    public void HandleNullTenantIdOnGetOrAdd()
    {
        var cache = new MultiTenantOptionsCache<TestOptions>();

        var options = new TestOptions();

        // Add new options for the default (null) tenant id bucket.
        var result = cache.GetOrAdd("", null, () => options);
        Assert.NotNull(result);
    }

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    [InlineData("name")]
    public void GetOrAddNamedOptionForCurrentTenantOnly(string? name)
    {
        var cache = new MultiTenantOptionsCache<TestOptions>();

        var options = new TestOptions();
        var options2 = new TestOptions();
        const string tenantId = "test-id-123";

        // Add new options.
        var result = cache.GetOrAdd(name, tenantId, () => options);
        Assert.Same(options, result);

        // Get the existing options if exists.
        result = cache.GetOrAdd(name, tenantId, () => options2);
        Assert.NotSame(options2, result);

        // Confirm different tenant on same object is an add (ie it didn't exist there).
        result = cache.GetOrAdd(name, "diff_id", () => options2);
        Assert.Same(options2, result);
    }

    [Fact]
    public void ThrowsIfGetOrAddFactoryIsNull()
    {
        var cache = new MultiTenantOptionsCache<TestOptions>();

        Assert.Throws<ArgumentNullException>(() => { cache.GetOrAdd("", "tenant", null!); });
    }

    [Fact]
    public void AllowParameterlessConstructor()
    {
        var cache = new MultiTenantOptionsCache<TestOptions>();

        Assert.NotNull(cache);
    }

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    [InlineData("name")]
    public void RemoveNamedOptionsForCurrentTenantOnly(string? name)
    {
        var cache = new MultiTenantOptionsCache<TestOptions>();

        var options = new TestOptions();
        const string tenant1Id = "test-id-123";
        const string tenant2Id = "diff_id";

        // Add new options.
        var result = cache.TryAdd(name, tenant1Id, options);
        Assert.True(result);

        // Add under a different tenant.
        result = cache.TryAdd(name, tenant2Id, options);
        Assert.True(result);
        result = cache.TryAdd("diffName", tenant2Id, options);
        Assert.True(result);

        // Remove named options for tenant2.
        result = cache.TryRemove(name, tenant2Id);
        Assert.True(result);
        var tenantCache = (ConcurrentDictionary<string, IOptionsMonitorCache<TestOptions>>?)cache.GetType()
            .GetField("map", BindingFlags.NonPublic | BindingFlags.Instance)?.GetValue(cache);

        dynamic? tenantInternalCache = tenantCache?[tenant2Id].GetType()
            .GetField("_cache", BindingFlags.NonPublic | BindingFlags.Instance)?
            .GetValue(tenantCache[tenant2Id]);

        // Assert named options removed and other options on tenant2 left as-is.
        Assert.False(tenantInternalCache!.Keys.Contains(name));
        Assert.True(tenantInternalCache.Keys.Contains("diffName"));

        // Assert tenant1 not affected.
        tenantInternalCache = tenantCache?[tenant1Id].GetType()
            .GetField("_cache", BindingFlags.NonPublic | BindingFlags.Instance)?
            .GetValue(tenantCache[tenant1Id]);
        Assert.True(tenantInternalCache!.ContainsKey(name ?? Microsoft.Extensions.Options.Options.DefaultName));
    }

    [Fact]
    public void ClearOptionsForNullTenantIdOnly()
    {
        var cache = new MultiTenantOptionsCache<TestOptions>();

        var options = new TestOptions();

        // Add new options.
        var result = cache.TryAdd("", null, options);
        Assert.True(result);

        // Add under a different tenant.
        result = cache.TryAdd("", "diff_id", options);
        Assert.True(result);

        // Clear options on null tenant id bucket only.
        cache.Clear(null);

        // Assert options cleared on null tenant id bucket.
        var tenantCache = (ConcurrentDictionary<string, IOptionsMonitorCache<TestOptions>>?)cache.GetType()
            .GetField("map", BindingFlags.NonPublic | BindingFlags.Instance)?.GetValue(cache);

        dynamic? tenantInternalCache = tenantCache?[""].GetType()
            .GetField("_cache", BindingFlags.NonPublic | BindingFlags.Instance)?
            .GetValue(tenantCache[""]);
        Assert.True(tenantInternalCache!.IsEmpty);

        // Assert options still exist on other tenant.
        tenantInternalCache = tenantCache?["diff_id"].GetType()
            .GetField("_cache", BindingFlags.NonPublic | BindingFlags.Instance)?
            .GetValue(tenantCache["diff_id"]);
        Assert.False(tenantInternalCache!.IsEmpty);
    }

    [Fact]
    public void ClearOptionsForTenantIdOnly()
    {
        var cache = new MultiTenantOptionsCache<TestOptions>();

        var options = new TestOptions();

        // Add new options.
        var result = cache.TryAdd("", "test-id-123", options);
        Assert.True(result);

        // Add under a different tenant.
        result = cache.TryAdd("", "diff_id", options);
        Assert.True(result);

        // Clear options on first tenant.
        cache.Clear("test-id-123");

        // Assert options cleared on this tenant.
        var tenantCache = (ConcurrentDictionary<string, IOptionsMonitorCache<TestOptions>>?)cache.GetType()
            .GetField("map", BindingFlags.NonPublic | BindingFlags.Instance)?.GetValue(cache);

        dynamic? tenantInternalCache = tenantCache?["test-id-123"].GetType()
            .GetField("_cache", BindingFlags.NonPublic | BindingFlags.Instance)?
            .GetValue(tenantCache["test-id-123"]);
        Assert.True(tenantInternalCache!.IsEmpty);

        // Assert options still exist on other tenant.
        tenantInternalCache = tenantCache?["diff_id"].GetType()
            .GetField("_cache", BindingFlags.NonPublic | BindingFlags.Instance)?
            .GetValue(tenantCache["diff_id"]);
        Assert.False(tenantInternalCache!.IsEmpty);
    }

    [Fact]
    public void ClearAllOptionsForClearAll()
    {
        var cache = new MultiTenantOptionsCache<TestOptions>();

        var options = new TestOptions();

        // Add new options.
        var result = cache.TryAdd("", "test-id-123", options);
        Assert.True(result);

        // Add under a different tenant.
        result = cache.TryAdd("", "diff_id", options);
        Assert.True(result);

        // Clear all options.
        cache.ClearAll();

        // Assert options cleared on this tenant.
        var tenantCache = (ConcurrentDictionary<string, IOptionsMonitorCache<TestOptions>>?)cache.GetType()
            .GetField("map", BindingFlags.NonPublic | BindingFlags.Instance)?.GetValue(cache);

        dynamic? tenantInternalCache = tenantCache?["test-id-123"].GetType()
            .GetField("_cache", BindingFlags.NonPublic | BindingFlags.Instance)?
            .GetValue(tenantCache["test-id-123"]);
        Assert.True(tenantInternalCache!.IsEmpty);

        // Assert options cleared on other tenant.
        tenantInternalCache = tenantCache?["diff_id"].GetType()
            .GetField("_cache", BindingFlags.NonPublic | BindingFlags.Instance)?
            .GetValue(tenantCache["diff_id"]);
        Assert.True(tenantInternalCache!.IsEmpty);
    }
}