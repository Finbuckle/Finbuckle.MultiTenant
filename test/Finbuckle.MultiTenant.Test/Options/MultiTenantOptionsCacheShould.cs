// Copyright Finbuckle LLC, Andrew White, and Contributors.
// Refer to the solution LICENSE file for more information.

using Finbuckle.MultiTenant.Abstractions;
using Finbuckle.MultiTenant.Options;
using Xunit;

namespace Finbuckle.MultiTenant.Test.Options;

public class MultiTenantOptionsCacheShould
{
    public class TestOptions
    {
        public string? Value { get; init; }
    }

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    [InlineData("name")]
    public void CacheNamedOptionsForCurrentTenant(string? name)
    {
        var (cache, context) = CreateCache();
        SetTenant(context, "tenant-1");
        var tenant1 = new TestOptions();

        Assert.True(cache.TryAdd(name, tenant1));
        Assert.False(cache.TryAdd(name, new TestOptions()));

        SetTenant(context, "tenant-2");
        var tenant2 = new TestOptions();
        Assert.True(cache.TryAdd(name, tenant2));
        Assert.Same(tenant2, cache.GetOrAdd(name, () => new TestOptions()));

        SetTenant(context, "tenant-1");
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
        var tenantContext = new AmbientTenantContext<TenantInfo,string>();
        var cache = new MultiTenantOptionsCache<TestOptions, string>(tenantContext);

        Assert.Throws<ArgumentNullException>(() => new MultiTenantOptionsCache<TestOptions, string>(null!));
        Assert.Throws<ArgumentNullException>(() => cache.GetOrAdd("name", null!));
        Assert.Throws<ArgumentNullException>(() => cache.TryAdd("name", null!));
    }

    [Fact]
    public void ClearOptionsForCurrentTenantOnly()
    {
        var (cache, context) = CreateCache();
        var tenant1 = Add(cache, context, "tenant-1", "name");
        var tenant2 = Add(cache, context, "tenant-2", "name");

        SetTenant(context, "tenant-1");
        cache.Clear();

        var replacement = new TestOptions();
        Assert.Same(replacement, cache.GetOrAdd("name", () => replacement));
        SetTenant(context, "tenant-2");
        Assert.Same(tenant2, cache.GetOrAdd("name", () => new TestOptions()));
        Assert.NotSame(tenant1, replacement);
    }

    [Fact]
    public void ClearOptionsForSpecifiedTenantOnly()
    {
        var (cache, context) = CreateCache();
        Add(cache, context, "tenant-1", "name");
        var tenant2 = Add(cache, context, "tenant-2", "name");

        cache.Clear("tenant-1");

        SetTenant(context, "tenant-1");
        var replacement = new TestOptions();
        Assert.Same(replacement, cache.GetOrAdd("name", () => replacement));
        SetTenant(context, "tenant-2");
        Assert.Same(tenant2, cache.GetOrAdd("name", () => new TestOptions()));
    }

    [Fact]
    public void TreatNullTenantIdAsNoTenantWhenClearing()
    {
        var (cache, _) = CreateCache();
        var original = new TestOptions();
        cache.TryAdd("name", original);

        cache.Clear(null);

        var replacement = new TestOptions();
        Assert.Same(replacement, cache.GetOrAdd("name", () => replacement));
        Assert.NotSame(original, replacement);
    }

    [Fact]
    public void ClearAllOptionsForAllTenantsAndNames()
    {
        var (cache, context) = CreateCache();
        Add(cache, context, "tenant-1", "name-1");
        Add(cache, context, "tenant-1", "name-2");
        Add(cache, context, "tenant-2", "name-1");

        cache.ClearAll();

        foreach (var tenantId in new[] { "tenant-1", "tenant-2" })
        {
            SetTenant(context, tenantId);
            var replacement = new TestOptions();
            Assert.Same(replacement, cache.GetOrAdd("name-1", () => replacement));
        }
    }

    [Theory]
    [InlineData(null)]
    [InlineData("target")]
    public void RemoveNamedOptionsForAllTenantsButPreserveOtherNames(string? name)
    {
        var normalizedName = name ?? Microsoft.Extensions.Options.Options.DefaultName;
        var (cache, context) = CreateCache();
        var originals = new Dictionary<string, TestOptions>();
        var others = new Dictionary<string, TestOptions>();

        foreach (var tenantId in new[] { "tenant-1", "tenant-2" })
        {
            originals[tenantId] = Add(cache, context, tenantId, normalizedName);
            others[tenantId] = Add(cache, context, tenantId, "other");
        }

        SetNoTenant(context);
        var noTenantOriginal = new TestOptions();
        var noTenantOther = new TestOptions();
        cache.TryAdd(name, noTenantOriginal);
        cache.TryAdd("other", noTenantOther);

        Assert.True(cache.TryRemove(name));
        Assert.False(cache.TryRemove(name));

        foreach (var tenantId in new[] { "tenant-1", "tenant-2" })
        {
            SetTenant(context, tenantId);
            var replacement = new TestOptions();
            Assert.Same(replacement, cache.GetOrAdd(name, () => replacement));
            Assert.NotSame(originals[tenantId], replacement);
            Assert.Same(others[tenantId], cache.GetOrAdd("other", () => new TestOptions()));
        }

        SetNoTenant(context);
        var noTenantReplacement = new TestOptions();
        Assert.Same(noTenantReplacement, cache.GetOrAdd(name, () => noTenantReplacement));
        Assert.NotSame(noTenantOriginal, noTenantReplacement);
        Assert.Same(noTenantOther, cache.GetOrAdd("other", () => new TestOptions()));
    }

    [Fact]
    public async Task InvokeFactoryOnceForConcurrentAccessToSameTenantAndName()
    {
        var (cache, context) = CreateCache();
        var factoryCalls = 0;

        var results = await Task.WhenAll(Enumerable.Range(0, 32).Select(_ => Task.Run(() =>
        {
            SetTenant(context, "tenant-1");
            return cache.GetOrAdd("name", () =>
            {
                Interlocked.Increment(ref factoryCalls);
                Thread.Sleep(10);
                return new TestOptions();
            });
        })));

        Assert.Equal(1, factoryCalls);
        Assert.All(results, result => Assert.Same(results[0], result));
    }

    [Fact]
    public async Task IsolateParallelTenantContextsAndInvalidateNameAcrossThem()
    {
        var (cache, context) = CreateCache();
        var tenantIds = Enumerable.Range(1, 20).Select(i => $"tenant-{i}").ToArray();

        var originals = await Task.WhenAll(tenantIds.Select(tenantId => Task.Run(() =>
        {
            SetTenant(context, tenantId);
            return cache.GetOrAdd("name", () => new TestOptions { Value = tenantId });
        })));

        Assert.Equal(tenantIds.Order(), originals.Select(o => o.Value).Order());
        Assert.True(cache.TryRemove("name"));

        var replacements = await Task.WhenAll(tenantIds.Select(tenantId => Task.Run(() =>
        {
            SetTenant(context, tenantId);
            return cache.GetOrAdd("name", () => new TestOptions { Value = $"new-{tenantId}" });
        })));

        Assert.All(replacements, replacement => Assert.StartsWith("new-tenant-", replacement.Value));
    }

    [Fact]
    public async Task RebuildNameAfterInvalidationOverlapsCreation()
    {
        var (cache, context) = CreateCache();
        using var creationStarted = new ManualResetEventSlim();
        using var releaseCreation = new ManualResetEventSlim();

        var getTask = Task.Run(() =>
        {
            SetTenant(context, "tenant-1");
            return cache.GetOrAdd("name", () =>
            {
                creationStarted.Set();
                releaseCreation.Wait();
                return new TestOptions { Value = "old" };
            });
        });

        Assert.True(creationStarted.Wait(TimeSpan.FromSeconds(5)));
        var removeTask = Task.Run(() => cache.TryRemove("name"));

        releaseCreation.Set();
        Assert.Equal("old", (await getTask).Value);
        Assert.True(await removeTask);

        SetTenant(context, "tenant-1");
        Assert.Equal("new", cache.GetOrAdd("name", () => new TestOptions { Value = "new" }).Value);
    }

    [Fact]
    public void CacheOptionsSeparatelyPerIntTenant()
    {
        var context = new AmbientTenantContext<TenantInfo<int>, int>();
        context.BeginScope();
        var cache = new MultiTenantOptionsCache<TestOptions, int>(context);

        // tenant 1
        context.BeginScope();
        context.TenantInfo = new TenantInfo<int> { Id = 1, Identifier = "t1" };
        var opt1 = new TestOptions();
        Assert.True(cache.TryAdd(null, opt1));

        // tenant 2 gets its own cache slot (int id 2 keys separately from int id 1)
        context.BeginScope();
        context.TenantInfo = new TenantInfo<int> { Id = 2, Identifier = "t2" };
        var opt2 = new TestOptions();
        Assert.True(cache.TryAdd(null, opt2));
        Assert.Same(opt2, cache.GetOrAdd(null, () => new TestOptions()));

        // back to tenant 1 -> original options still cached
        context.BeginScope();
        context.TenantInfo = new TenantInfo<int> { Id = 1, Identifier = "t1" };
        Assert.Same(opt1, cache.GetOrAdd(null, () => new TestOptions()));
    }

    private static (MultiTenantOptionsCache<TestOptions,string> Cache, AmbientTenantContext<TenantInfo,string> Context) CreateCache()
    {
        var context = new AmbientTenantContext<TenantInfo,string>();
        SetNoTenant(context);
        return (new MultiTenantOptionsCache<TestOptions, string>(context), context);
    }

    private static TestOptions Add(MultiTenantOptionsCache<TestOptions, string> cache,
        AmbientTenantContext<TenantInfo,string> context, string tenantId, string? name)
    {
        SetTenant(context, tenantId);
        var options = new TestOptions();
        Assert.True(cache.TryAdd(name, options));
        return options;
    }

    private static void SetTenant(AmbientTenantContext<TenantInfo, string> context, string tenantId)
    {
        context.BeginScope();
        context.TenantInfo = new TenantInfo { Id = tenantId, Identifier = tenantId };
    }

    private static void SetNoTenant(AmbientTenantContext<TenantInfo, string> context) => context.BeginScope();
}
