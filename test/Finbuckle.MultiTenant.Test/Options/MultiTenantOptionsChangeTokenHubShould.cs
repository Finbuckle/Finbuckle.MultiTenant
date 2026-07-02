// Copyright Finbuckle LLC, Andrew White, and Contributors.
// Refer to the solution LICENSE file for more information.

using System.Collections.Concurrent;
using Finbuckle.MultiTenant.Options;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Primitives;
using Xunit;

namespace Finbuckle.MultiTenant.Test.Options;

public class MultiTenantOptionsChangeTokenHubShould
{
    public class TestOptions
    {
        public string? Value;
    }

    [Fact]
    public void NotifyEachTenantRegistrationWithTenantSpecificOptions()
    {
        var source = new TestChangeTokenSource<TestOptions>("name");
        var cache = new MultiTenantOptionsCache<TestOptions>();
        var factory = new CountingFactory<TestOptions>(name => new TestOptions { Value = $"{name}-{Guid.NewGuid()}" });
        using var hub = new MultiTenantOptionsChangeTokenHub<TestOptions>(new[] { source }, cache, factory);

        TestOptions? tenant1Options = null;
        TestOptions? tenant2Options = null;

        // Each registration carries a tenant id and should receive an option instance for that tenant.
        using var registration1 = hub.Register("tenant-1", (options, _) => tenant1Options = options);
        using var registration2 = hub.Register("tenant-2", (options, _) => tenant2Options = options);

        source.Trigger();

        Assert.True(SpinWait.SpinUntil(() => tenant1Options is not null && tenant2Options is not null, TimeSpan.FromSeconds(1)));
        Assert.NotSame(tenant1Options, tenant2Options);
        Assert.Equal(2, factory.CreateCountByName["name"]);
    }

    [Fact]
    public void StopNotifyingWhenRegistrationDisposed()
    {
        var source = new TestChangeTokenSource<TestOptions>("name");
        var cache = new MultiTenantOptionsCache<TestOptions>();
        var factory = new CountingFactory<TestOptions>(name => new TestOptions { Value = name });
        using var hub = new MultiTenantOptionsChangeTokenHub<TestOptions>(new[] { source }, cache, factory);

        var calls = 0;
        var registration = hub.Register("tenant-1", (_, _) => calls++);

        registration.Dispose();
        source.Trigger();
        Thread.Sleep(20);

        Assert.Equal(0, calls);
    }

    [Fact]
    public void ClearCacheOnChangeEvenWithNoListeners()
    {
        var source = new TestChangeTokenSource<TestOptions>("named");
        var cache = new MultiTenantOptionsCache<TestOptions>();
        var factory = new CountingFactory<TestOptions>(name => new TestOptions { Value = $"{name}-{Guid.NewGuid()}" });
        using var hub = new MultiTenantOptionsChangeTokenHub<TestOptions>(new[] { source }, cache, factory);

        var defaultBefore = cache.GetOrAdd(Microsoft.Extensions.Options.Options.DefaultName, "tenant-1", () => factory.Create(Microsoft.Extensions.Options.Options.DefaultName));
        var namedBefore = cache.GetOrAdd("named", "tenant-1", () => factory.Create("named"));

        // There are no registrations, but source changes should still invalidate cached options.
        source.Trigger();
        Thread.Sleep(20);

        var defaultAfter = cache.GetOrAdd(Microsoft.Extensions.Options.Options.DefaultName, "tenant-1", () => factory.Create(Microsoft.Extensions.Options.Options.DefaultName));
        var namedAfter = cache.GetOrAdd("named", "tenant-1", () => factory.Create("named"));

        Assert.NotSame(defaultBefore, defaultAfter);
        Assert.NotSame(namedBefore, namedAfter);
    }

    private sealed class CountingFactory<TOptions>(Func<string, TOptions> create)
        : IOptionsFactory<TOptions>
        where TOptions : class
    {
        private readonly ConcurrentDictionary<string, int> _createCountByName = new();

        public IReadOnlyDictionary<string, int> CreateCountByName => _createCountByName;

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
}


