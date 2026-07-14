// Copyright Finbuckle LLC, Andrew White, and Contributors.
// Refer to the solution LICENSE file for more information.

using Finbuckle.MultiTenant.Abstractions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Primitives;
using Xunit;

namespace Finbuckle.MultiTenant.Test.Options;

public class PerTenantOptionsIntegrationShould
{
    [Fact]
    public void ReloadChangedNameForEveryTenantAndPreserveOtherNames()
    {
        var services = new ServiceCollection();
        var state = new ReloadState();
        var source = new ManualChangeTokenSource<TestOptions>("changed");
        services.AddMultiTenant<TenantInfo>();
        services.AddSingleton<IOptionsChangeTokenSource<TestOptions>>(source);
        services.ConfigurePerTenant<TestOptions, TenantInfo>("changed", (options, tenant) =>
            options.Value = $"{tenant.Id}:{state.Version}");
        services.ConfigurePerTenant<TestOptions, TenantInfo>("unchanged", (options, tenant) =>
            options.Value = $"unchanged:{tenant.Id}");

        using var provider = services.BuildServiceProvider();
        var monitor = provider.GetRequiredService<IOptionsMonitor<TestOptions>>();

        SetTenant(provider, "tenant-1");
        var tenant1Changed = monitor.Get("changed");
        var tenant1Unchanged = monitor.Get("unchanged");
        SetTenant(provider, "tenant-2");
        var tenant2Changed = monitor.Get("changed");
        var tenant2Unchanged = monitor.Get("unchanged");

        state.Version = 2;
        source.Trigger();

        SetTenant(provider, "tenant-1");
        var tenant1Reloaded = monitor.Get("changed");
        Assert.Equal("tenant-1:2", tenant1Reloaded.Value);
        Assert.NotSame(tenant1Changed, tenant1Reloaded);
        Assert.Same(tenant1Unchanged, monitor.Get("unchanged"));

        SetTenant(provider, "tenant-2");
        var tenant2Reloaded = monitor.Get("changed");
        Assert.Equal("tenant-2:2", tenant2Reloaded.Value);
        Assert.NotSame(tenant2Changed, tenant2Reloaded);
        Assert.Same(tenant2Unchanged, monitor.Get("unchanged"));
    }

    private static void SetTenant(IServiceProvider provider, string tenantId)
    {
        var setter = provider.GetRequiredService<IMultiTenantContextSetter>();
        setter.MultiTenantContext = new MultiTenantContext<TenantInfo>
        {
            TenantInfo = new TenantInfo { Id = tenantId, Identifier = tenantId }
        };
    }

    public class TestOptions
    {
        public string? Value { get; set; }
    }

    private sealed class ReloadState
    {
        public int Version { get; set; } = 1;
    }

    private sealed class ManualChangeTokenSource<TOptions> : IOptionsChangeTokenSource<TOptions>
    {
        private CancellationTokenSource source = new();

        public ManualChangeTokenSource(string? name)
        {
            Name = name;
        }

        public string? Name { get; }

        public IChangeToken GetChangeToken() => new CancellationChangeToken(source.Token);

        public void Trigger()
        {
            var previous = Interlocked.Exchange(ref source, new CancellationTokenSource());
            previous.Cancel();
            previous.Dispose();
        }
    }
}
