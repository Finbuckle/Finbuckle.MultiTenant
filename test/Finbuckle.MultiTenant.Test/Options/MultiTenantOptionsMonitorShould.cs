// Copyright Finbuckle LLC, Andrew White, and Contributors.
// Refer to the solution LICENSE file for more information.

using System.Collections.Concurrent;
using Finbuckle.MultiTenant.Abstractions;
using Finbuckle.MultiTenant.Extensions;
using Finbuckle.MultiTenant.Options;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Primitives;
using Xunit;

namespace Finbuckle.MultiTenant.Test.Options;

public class MultiTenantOptionsMonitorShould
{
    public class TestOptions
    {
        public string? Value { get; set; }
    }

    [Fact]
    public void GetOptionByDefaultNameIfNameNull()
    {
        var source = new TestChangeTokenSource<TestOptions>(Microsoft.Extensions.Options.Options.DefaultName);
        var factory = new CountingFactory<TestOptions>(name => new TestOptions { Value = name });
        var monitor = BuildMonitor(factory, new[] { source }, "tenant-1");

        var options = monitor.Get(null);

        Assert.Equal(Microsoft.Extensions.Options.Options.DefaultName, options.Value);
        Assert.Equal(1, factory.CreateCountByName[Microsoft.Extensions.Options.Options.DefaultName]);
    }

    [Fact]
    public void PartitionCacheByTenantId()
    {
        var source = new TestChangeTokenSource<TestOptions>("name");
        var factory = new CountingFactory<TestOptions>(name => new TestOptions { Value = $"{name}-{Guid.NewGuid()}" });
        var cache = new MultiTenantOptionsCache<TestOptions>();
        var tenant1Monitor = BuildMonitor(factory, new[] { source }, "tenant-1", cache);
        var tenant2Monitor = BuildMonitor(factory, Array.Empty<IOptionsChangeTokenSource<TestOptions>>(), "tenant-2", cache);

        var tenant1First = tenant1Monitor.Get("name");
        var tenant1Second = tenant1Monitor.Get("name");
        var tenant2First = tenant2Monitor.Get("name");
        var tenant2Second = tenant2Monitor.Get("name");

        Assert.Same(tenant1First, tenant1Second);
        Assert.Same(tenant2First, tenant2Second);
        Assert.NotSame(tenant1First, tenant2First);
        Assert.Equal(2, factory.TotalCreateCount);
    }

    [Fact]
    public void InvalidateChangedNameForAllTenants()
    {
        var source = new TestChangeTokenSource<TestOptions>("name");
        var factory = new CountingFactory<TestOptions>(name => new TestOptions { Value = $"{name}-{Guid.NewGuid()}" });
        var cache = new MultiTenantOptionsCache<TestOptions>();
        var tenant1Monitor = BuildMonitor(factory, new[] { source }, "tenant-1", cache);
        var tenant2Monitor = BuildMonitor(factory, Array.Empty<IOptionsChangeTokenSource<TestOptions>>(), "tenant-2", cache);

        var tenant1Before = tenant1Monitor.Get("name");
        var tenant2Before = tenant2Monitor.Get("name");

        // A source change is global for the options type, so all tenant entries are invalidated.
        source.Trigger();
        Thread.Sleep(20);

        var tenant1After = tenant1Monitor.Get("name");
        var tenant2After = tenant2Monitor.Get("name");

        Assert.NotSame(tenant1Before, tenant1After);
        Assert.NotSame(tenant2Before, tenant2After);
    }

    [Fact]
    public void InvokeOnChangeWithNewOptionsAndName()
    {
        var source = new TestChangeTokenSource<TestOptions>("changed-name");
        var factory = new CountingFactory<TestOptions>(name => new TestOptions { Value = $"{name}-{Guid.NewGuid()}" });
        var monitor = BuildMonitor(factory, new[] { source }, "tenant-1");

        var before = monitor.Get("changed-name");
        TestOptions? changedOptions = null;
        string? changedName = null;
        using var listener = monitor.OnChange((options, name) =>
        {
            changedOptions = options;
            changedName = name;
        });

        source.Trigger();

        Assert.True(SpinWait.SpinUntil(() => changedOptions is not null, TimeSpan.FromSeconds(1)));
        Assert.Equal("changed-name", changedName);
        Assert.NotSame(before, changedOptions);
    }

    [Fact]
    public void NotInvokeOnChangeAfterListenerDisposed()
    {
        var source = new TestChangeTokenSource<TestOptions>("name");
        var factory = new CountingFactory<TestOptions>(name => new TestOptions { Value = name });
        var monitor = BuildMonitor(factory, new[] { source }, "tenant-1");

        var calls = 0;
        var listener = monitor.OnChange((_, _) => calls++);
        listener.Dispose();

        source.Trigger();
        Thread.Sleep(20);

        Assert.Equal(0, calls);
    }

    [Fact]
    public void RefreshOptionsAfterChangesEvenIfMonitorDisposed()
    {
        var source = new TestChangeTokenSource<TestOptions>("name");
        var factory = new CountingFactory<TestOptions>(name => new TestOptions { Value = $"{name}-{Guid.NewGuid()}" });
        var monitor = BuildMonitor(factory, new[] { source }, "tenant-1");

        var before = monitor.Get("name");
        // Monitor disposal is a no-op; hub-owned token handling should still invalidate cache entries.
        monitor.Dispose();

        source.Trigger();
        Thread.Sleep(20);

        var after = monitor.Get("name");

        Assert.NotSame(before, after);
        Assert.Equal(2, factory.TotalCreateCount);
    }

    [Fact]
    public void KeepOnChangeRegistrationAliveAfterMonitorDisposed()
    {
        var source = new TestChangeTokenSource<TestOptions>("name");
        var factory = new CountingFactory<TestOptions>(name => new TestOptions { Value = $"{name}-{Guid.NewGuid()}" });
        var monitor = BuildMonitor(factory, new[] { source }, "tenant-1");

        TestOptions? changedOptions = null;
        using var registration = monitor.OnChange((options, _) => changedOptions = options);

        // Prime cache, then dispose monitor instance; registration should remain hub-backed.
        _ = monitor.Get("name");
        monitor.Dispose();

        source.Trigger();

        Assert.True(SpinWait.SpinUntil(() => changedOptions is not null, TimeSpan.FromSeconds(1)));
    }

    [Fact]
    public void InvokeOnChangeForEachTenantRegistrationUsingRegistrationTenantId()
    {
        var source = new TestChangeTokenSource<TestOptions>("name");
        var factory = new CountingFactory<TestOptions>(name => new TestOptions { Value = $"{name}-{Guid.NewGuid()}" });
        var cache = new MultiTenantOptionsCache<TestOptions>();
        var hub = new MultiTenantOptionsChangeTokenHub<TestOptions>(new[] { source }, cache, factory);

        var tenant1Monitor = new MultiTenantOptionsMonitor<TestOptions>(
            factory,
            hub,
            cache,
            new TenantContext<TenantInfo>(new TenantInfo { Id = "tenant-1", Identifier = "tenant-1" }));

        var tenant2Monitor = new MultiTenantOptionsMonitor<TestOptions>(
            factory,
            hub,
            cache,
            new TenantContext<TenantInfo>(new TenantInfo { Id = "tenant-2", Identifier = "tenant-2" }));

        TestOptions? tenant1Changed = null;
        TestOptions? tenant2Changed = null;

        using var tenant1Registration = tenant1Monitor.OnChange((options, _) => tenant1Changed = options);
        using var tenant2Registration = tenant2Monitor.OnChange((options, _) => tenant2Changed = options);

        _ = tenant1Monitor.Get("name");
        _ = tenant2Monitor.Get("name");

        // Each registration carries its tenant id, so callback options should be tenant-specific.
        source.Trigger();

        Assert.True(SpinWait.SpinUntil(() => tenant1Changed is not null && tenant2Changed is not null, TimeSpan.FromSeconds(1)));
        Assert.NotSame(tenant1Changed, tenant2Changed);
    }

    [Fact]
    public void AllowDisposingOnChangeRegistrationAfterMonitorDisposed()
    {
        var source = new TestChangeTokenSource<TestOptions>("name");
        var factory = new CountingFactory<TestOptions>(name => new TestOptions { Value = $"{name}-{Guid.NewGuid()}" });
        var monitor = BuildMonitor(factory, new[] { source }, "tenant-1");

        var calls = 0;
        var registration = monitor.OnChange((_, _) => calls++);

        // Disposing monitor must not prevent explicit unsubscription from working.
        monitor.Dispose();
        registration.Dispose();

        source.Trigger();
        Thread.Sleep(20);

        Assert.Equal(0, calls);
    }

    [Fact]
    public void ClearAllInvalidatesOtherOptionNamesOnAnySourceChange()
    {
        var source = new TestChangeTokenSource<TestOptions>("named");
        var factory = new CountingFactory<TestOptions>(name => new TestOptions { Value = $"{name}-{Guid.NewGuid()}" });
        var monitor = BuildMonitor(factory, new[] { source }, "tenant-1");

        var namedBefore = monitor.Get("named");
        var defaultBefore = monitor.Get(Microsoft.Extensions.Options.Options.DefaultName);

        // Current hub strategy clears all cached names when any source token fires.
        source.Trigger();
        Thread.Sleep(20);

        var namedAfter = monitor.Get("named");
        var defaultAfter = monitor.Get(Microsoft.Extensions.Options.Options.DefaultName);

        Assert.NotSame(namedBefore, namedAfter);
        Assert.NotSame(defaultBefore, defaultAfter);
    }

    [Fact]
    public void ChangeTokenHubIsSingletonAndNotificationsPersistAcrossScopes()
    {
        var services = new ServiceCollection();
        var tenantHolder = new TenantHolder();
        var source = new TestChangeTokenSource<TestOptions>(Microsoft.Extensions.Options.Options.DefaultName);

        services.AddSingleton(tenantHolder);
        services.AddScoped<ITenantContext<TenantInfo>>(sp => new TenantContext<TenantInfo>(sp.GetRequiredService<TenantHolder>().Current));
        services.AddScoped<ITenantContext>(sp => sp.GetRequiredService<ITenantContext<TenantInfo>>());
        services.AddSingleton<IOptionsFactory<TestOptions>>(new CountingFactory<TestOptions>(name => new TestOptions { Value = name }));
        services.AddSingleton<IOptionsChangeTokenSource<TestOptions>>(source);
        services.ConfigurePerTenant<TestOptions, TenantInfo>((options, tenant) => options.Value = tenant.Id);

        using var provider = services.BuildServiceProvider();
        // Hub is singleton even though monitor itself is scoped.
        var hub1 = provider.GetRequiredService<MultiTenantOptionsChangeTokenHub<TestOptions>>();
        var hub2 = provider.GetRequiredService<MultiTenantOptionsChangeTokenHub<TestOptions>>();
        Assert.Same(hub1, hub2);

        tenantHolder.Current = new TenantInfo { Id = "tenant-1", Identifier = "tenant-1" };

        TestOptions? changed = null;
        using (var scope = provider.CreateScope())
        {
            var monitor = scope.ServiceProvider.GetRequiredService<IOptionsMonitor<TestOptions>>();
            using var registration = monitor.OnChange((options, _) => changed = options);

            _ = monitor.Get(Microsoft.Extensions.Options.Options.DefaultName);
            source.Trigger();

            Assert.True(SpinWait.SpinUntil(() => changed is not null, TimeSpan.FromSeconds(1)));
        }
    }

    private static MultiTenantOptionsMonitor<TestOptions> BuildMonitor(
        IOptionsFactory<TestOptions> factory,
        IEnumerable<IOptionsChangeTokenSource<TestOptions>> sources,
        string tenantId,
        MultiTenantOptionsCache<TestOptions>? cache = null)
    {
        cache ??= new MultiTenantOptionsCache<TestOptions>();
        var hub = new MultiTenantOptionsChangeTokenHub<TestOptions>(sources, cache, factory);
        return new MultiTenantOptionsMonitor<TestOptions>(
            factory,
            hub,
            cache,
            new TenantContext<TenantInfo>(new TenantInfo { Id = tenantId, Identifier = tenantId }));
    }

    private sealed class CountingFactory<TOptions>(Func<string, TOptions> create)
        : IOptionsFactory<TOptions>
        where TOptions : class
    {
        private readonly ConcurrentDictionary<string, int> _createCountByName = new();

        public IReadOnlyDictionary<string, int> CreateCountByName => _createCountByName;

        public int TotalCreateCount => _createCountByName.Values.Sum();

        public TOptions Create(string name)
        {
            _createCountByName.AddOrUpdate(name, 1, (_, count) => count + 1);
            return create(name);
        }
    }

    private sealed class TestChangeTokenSource<TOptions>(string? name)
        : IOptionsChangeTokenSource<TOptions>
    {
        private CancellationTokenSource _cts = new();

        public string? Name { get; } = name;

        public IChangeToken GetChangeToken() => new CancellationChangeToken(_cts.Token);

        public void Trigger()
        {
            var previous = Interlocked.Exchange(ref _cts, new CancellationTokenSource());
            previous.Cancel();
            previous.Dispose();
        }
    }

    private sealed class TenantHolder
    {
        public TenantInfo? Current { get; set; }
    }
}


