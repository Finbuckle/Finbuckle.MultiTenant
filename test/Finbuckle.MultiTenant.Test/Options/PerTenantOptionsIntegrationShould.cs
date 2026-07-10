// Copyright Finbuckle LLC, Andrew White, and Contributors.
// Refer to the solution LICENSE file for more information.

using Finbuckle.MultiTenant.Abstractions;
using Finbuckle.MultiTenant.Extensions;
using Finbuckle.MultiTenant.Options;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Primitives;
using Xunit;

namespace Finbuckle.MultiTenant.Test.Options;

public class PerTenantOptionsIntegrationShould
{
    [Fact]
    public void ResolveConfiguredAndValidatedOptionsThroughAllAccessors()
    {
        var services = new ServiceCollection();
        services.AddMultiTenant<TenantInfo>();
        services.Configure<TestOptions>(options => options.Value = "base");
        services.ConfigurePerTenant<TestOptions, TenantInfo>((options, tenant) =>
            options.Value += $":{tenant.Id}");
        services.PostConfigurePerTenant<TestOptions, TenantInfo>((options, tenant) =>
            options.Value += $":post-{tenant.Id}");
        services.AddOptions<TestOptions>().Validate(options =>
            options.Value == "base:tenant-1:post-tenant-1");

        using var provider = services.BuildServiceProvider();
        SetTenant(provider, "tenant-1");

        Assert.Equal("base:tenant-1:post-tenant-1",
            provider.GetRequiredService<IOptions<TestOptions>>().Value.Value);
        using var scope = provider.CreateScope();
        Assert.Equal("base:tenant-1:post-tenant-1",
            scope.ServiceProvider.GetRequiredService<IOptionsSnapshot<TestOptions>>().Value.Value);
        Assert.Equal("base:tenant-1:post-tenant-1",
            provider.GetRequiredService<IOptionsMonitor<TestOptions>>().CurrentValue.Value);
    }

    [Fact]
    public void ConfigureNamedOptionsThroughOptionsBuilderDependencies()
    {
        var services = new ServiceCollection();
        services.AddMultiTenant<TenantInfo>();
        services.AddSingleton(new TestDependency("dependency"));
        services.AddOptions<TestOptions>("builder")
            .ConfigurePerTenant<TestOptions, TestDependency, TenantInfo>((options, dependency, tenant) =>
                options.Value = $"{dependency.Value}:{tenant.Id}")
            .PostConfigurePerTenant<TestOptions, TestDependency, TenantInfo>((options, dependency, _) =>
                options.Value += $":post-{dependency.Value}");

        using var provider = services.BuildServiceProvider();
        SetTenant(provider, "tenant-1");

        Assert.Equal("dependency:tenant-1:post-dependency",
            provider.GetRequiredService<IOptionsMonitor<TestOptions>>().Get("builder").Value);
    }

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

        string? notificationName = null;
        TestOptions? notificationValue = null;
        using var registration = monitor.OnChange((options, name) =>
        {
            notificationName = name;
            notificationValue = options;
        });

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

        Assert.Equal("changed", notificationName);
        Assert.Equal("tenant-2:2", notificationValue?.Value);
    }

    private static void SetTenant(IServiceProvider provider, string tenantId)
    {
        var setter = provider.GetRequiredService<IMultiTenantContextSetter>();
        setter.MultiTenantContext = new MultiTenantContext<TenantInfo>(
            new TenantInfo { Id = tenantId, Identifier = tenantId });
    }

    public class TestOptions
    {
        public string? Value { get; set; }
    }

    public record TestDependency(string Value);

    private sealed class ReloadState
    {
        public int Version { get; set; } = 1;
    }

    private sealed class ManualChangeTokenSource<TOptions>(string? name) : IOptionsChangeTokenSource<TOptions>
    {
        private CancellationTokenSource source = new();

        public string? Name { get; } = name;

        public IChangeToken GetChangeToken() => new CancellationChangeToken(source.Token);

        public void Trigger()
        {
            var previous = Interlocked.Exchange(ref source, new CancellationTokenSource());
            previous.Cancel();
            previous.Dispose();
        }
    }

    private sealed class CustomOptionsSnapshot : IOptionsSnapshot<TestOptions>
    {
        public TestOptions Value { get; } = new();

        public TestOptions Get(string? name) => Value;
    }
}
