// Copyright Finbuckle LLC, Andrew White, and Contributors.
// Refer to the solution LICENSE file for more information.

using Finbuckle.MultiTenant.Internal;
using Finbuckle.MultiTenant.Options;
using Xunit;

namespace Finbuckle.MultiTenant.Test.Options;

public class MultiTenantOptionsCacheShould
{
    internal class TestOptions
    {
        public string? Value { get; set; }
    }

    private static (MultiTenantOptionsCache<TestOptions> Cache,
        AsyncLocalMultiTenantContextAccessor<TenantInfo> Accessor) CreateCache()
    {
        var accessor = new AsyncLocalMultiTenantContextAccessor<TenantInfo>();
        return (new MultiTenantOptionsCache<TestOptions>(accessor), accessor);
    }

    private static void SetTenant(AsyncLocalMultiTenantContextAccessor<TenantInfo> accessor, string tenantId)
    {
        accessor.MultiTenantContext = new MultiTenantContext<TenantInfo>
        {
            TenantInfo = new TenantInfo { Id = tenantId }
        };
    }

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    [InlineData("name")]
    public void CacheNamedOptionsForCurrentTenant(string? name)
    {
        var (cache, accessor) = CreateCache();
        SetTenant(accessor, "tenant-1");
        var tenant1 = new TestOptions();

        Assert.True(cache.TryAdd(name, tenant1));
        Assert.False(cache.TryAdd(name, new TestOptions()));

        SetTenant(accessor, "tenant-2");
        var tenant2 = new TestOptions();
        Assert.True(cache.TryAdd(name, tenant2));
        Assert.Same(tenant2, cache.GetOrAdd(name, () => new TestOptions()));

        SetTenant(accessor, "tenant-1");
        Assert.Same(tenant1, cache.GetOrAdd(name, () => new TestOptions()));
    }

    [Fact]
    public void CacheOptionsWithoutResolvedTenant()
    {
        var (cache, _) = CreateCache();
        var options = new TestOptions();

        Assert.True(cache.TryAdd(null, options));
        Assert.Same(options, cache.GetOrAdd(null, () => new TestOptions()));
    }

    [Fact]
    public void ThrowForNullConstructorParameterFactoryAndOptions()
    {
        var (cache, _) = CreateCache();

        Assert.Throws<ArgumentNullException>(() => new MultiTenantOptionsCache<TestOptions>(null!));
        Assert.Throws<ArgumentNullException>(() => cache.GetOrAdd("name", null!));
        Assert.Throws<ArgumentNullException>(() => cache.TryAdd("name", null!));
    }

    [Fact]
    public void ClearOptionsForCurrentTenantOnly()
    {
        var (cache, accessor) = CreateCache();
        SetTenant(accessor, "tenant-1");
        var tenant1 = new TestOptions();
        cache.TryAdd("name", tenant1);
        SetTenant(accessor, "tenant-2");
        var tenant2 = new TestOptions();
        cache.TryAdd("name", tenant2);

        SetTenant(accessor, "tenant-1");
        cache.Clear();

        var replacement = new TestOptions();
        Assert.Same(replacement, cache.GetOrAdd("name", () => replacement));
        SetTenant(accessor, "tenant-2");
        Assert.Same(tenant2, cache.GetOrAdd("name", () => new TestOptions()));
        Assert.NotSame(tenant1, replacement);
    }

    [Fact]
    public void ClearOptionsForSpecifiedTenantOnly()
    {
        var (cache, accessor) = CreateCache();
        SetTenant(accessor, "tenant-1");
        cache.TryAdd("name", new TestOptions());
        SetTenant(accessor, "tenant-2");
        var tenant2 = new TestOptions();
        cache.TryAdd("name", tenant2);

        cache.Clear("tenant-1");

        SetTenant(accessor, "tenant-1");
        var replacement = new TestOptions();
        Assert.Same(replacement, cache.GetOrAdd("name", () => replacement));
        SetTenant(accessor, "tenant-2");
        Assert.Same(tenant2, cache.GetOrAdd("name", () => new TestOptions()));
    }

    [Fact]
    public void ClearAllOptionsForAllTenants()
    {
        var (cache, accessor) = CreateCache();
        foreach (var tenantId in new[] { "tenant-1", "tenant-2" })
        {
            SetTenant(accessor, tenantId);
            cache.TryAdd("name", new TestOptions());
        }

        cache.ClearAll();

        foreach (var tenantId in new[] { "tenant-1", "tenant-2" })
        {
            SetTenant(accessor, tenantId);
            var replacement = new TestOptions();
            Assert.Same(replacement, cache.GetOrAdd("name", () => replacement));
        }
    }

    [Theory]
    [InlineData(null)]
    [InlineData("target")]
    public void RemoveNamedOptionsForAllTenantsButPreserveOtherNames(string? name)
    {
        var (cache, accessor) = CreateCache();
        var originals = new Dictionary<string, TestOptions>();
        var others = new Dictionary<string, TestOptions>();

        foreach (var tenantId in new[] { "tenant-1", "tenant-2" })
        {
            SetTenant(accessor, tenantId);
            originals[tenantId] = new TestOptions();
            others[tenantId] = new TestOptions();
            cache.TryAdd(name, originals[tenantId]);
            cache.TryAdd("other", others[tenantId]);
        }

        Assert.True(cache.TryRemove(name));
        Assert.False(cache.TryRemove(name));

        foreach (var tenantId in new[] { "tenant-1", "tenant-2" })
        {
            SetTenant(accessor, tenantId);
            var replacement = new TestOptions();
            Assert.Same(replacement, cache.GetOrAdd(name, () => replacement));
            Assert.NotSame(originals[tenantId], replacement);
            Assert.Same(others[tenantId], cache.GetOrAdd("other", () => new TestOptions()));
        }
    }
}
