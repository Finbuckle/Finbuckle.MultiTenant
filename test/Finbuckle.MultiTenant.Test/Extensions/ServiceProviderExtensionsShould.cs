// Copyright Finbuckle LLC, Andrew White, and Contributors.
// Refer to the solution LICENSE file for more information.

using Finbuckle.MultiTenant.Abstractions;
using Finbuckle.MultiTenant.Extensions;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Finbuckle.MultiTenant.Test.Extensions;

public class ServiceProviderExtensionsShould
{
    [Fact]
    public void BeginUnresolvedTenantScope()
    {
        var services = new ServiceCollection();
        services.AddMultiTenant<TenantInfo, string>();
        using var provider = services.BuildServiceProvider();

        provider.BeginTenantScope();

        var tenantContext = provider.GetRequiredService<ITenantContext<TenantInfo, string>>();
        Assert.False(tenantContext.IsResolved);
        Assert.Null(tenantContext.TenantInfo);
    }

    [Fact]
    public void BeginResolvedTenantScope()
    {
        var services = new ServiceCollection();
        services.AddMultiTenant<TenantInfo, string>();
        using var provider = services.BuildServiceProvider();
        var tenantInfo = new TenantInfo { Id = "tenant-id", Identifier = "tenant" };

        provider.BeginTenantScope(tenantInfo);

        var tenantContext = provider.GetRequiredService<ITenantContext<TenantInfo, string>>();
        Assert.True(tenantContext.IsResolved);
        Assert.Same(tenantInfo, tenantContext.TenantInfo);
    }

    [Fact]
    public void ReplaceCurrentTenantScope()
    {
        var services = new ServiceCollection();
        services.AddMultiTenant<TenantInfo, string>();
        using var provider = services.BuildServiceProvider();
        var tenantInfo = new TenantInfo { Id = "tenant-id", Identifier = "tenant" };
        provider.BeginTenantScope(tenantInfo);

        provider.BeginTenantScope();

        var tenantContext = provider.GetRequiredService<ITenantContext<TenantInfo, string>>();
        Assert.False(tenantContext.IsResolved);
        Assert.Null(tenantContext.TenantInfo);
    }

    [Fact]
    public void ThrowIfServiceProviderIsNull()
    {
        IServiceProvider services = null!;

        Assert.Throws<ArgumentNullException>(() => services.BeginTenantScope());
    }

    [Fact]
    public void ThrowIfTenantInfoIsNull()
    {
        using var provider = new ServiceCollection().BuildServiceProvider();

        Assert.Throws<ArgumentNullException>(() => provider.BeginTenantScope<string>(null!));
    }

    private sealed class TenantCapturingService(ITenantContext<TenantInfo, string> tenantContext) : IDisposable
    {
        public string? TenantIdentifier { get; } = tenantContext.TenantInfo?.Identifier;
        public bool Disposed { get; private set; }
        public void Dispose() => Disposed = true;
    }

    private static ServiceProvider BuildProvider()
    {
        var services = new ServiceCollection();
        services.AddMultiTenant<TenantInfo, string>();
        services.AddScoped<TenantCapturingService>();
        return services.BuildServiceProvider();
    }

    private static TenantInfo Tenant(string identifier) => new() { Id = $"{identifier}-id", Identifier = identifier };

    [Fact]
    public async Task RunDelegateAsTenantInScopedServices()
    {
        await using var provider = BuildProvider();

        var observed = await provider.RunInTenantScopeAsync(Tenant("initech"), sp => Task.FromResult(
            (Ambient: sp.GetRequiredService<ITenantContext<TenantInfo, string>>().TenantInfo?.Identifier,
                Scoped: sp.GetRequiredService<TenantCapturingService>().TenantIdentifier)));

        Assert.Equal("initech", observed.Ambient);
        Assert.Equal("initech", observed.Scoped);
    }

    [Fact]
    public async Task RunDelegateWithoutResult()
    {
        await using var provider = BuildProvider();
        string? observed = null;

        await provider.RunInTenantScopeAsync(Tenant("initech"), async sp =>
        {
            await Task.Yield();
            observed = sp.GetRequiredService<ITenantContext<TenantInfo, string>>().TenantInfo?.Identifier;
        });

        Assert.Equal("initech", observed);
    }

    [Fact]
    public async Task RestoreTheCallersTenantScope()
    {
        await using var provider = BuildProvider();
        var tenantContext = provider.GetRequiredService<ITenantContext<TenantInfo, string>>();

        await Task.Run(async () =>
        {
            provider.BeginTenantScope(Tenant("outer"));

            var inner = await provider.RunInTenantScopeAsync(Tenant("inner"),
                sp => Task.FromResult(sp.GetRequiredService<ITenantContext<TenantInfo, string>>().TenantInfo?.Identifier));

            Assert.Equal("inner", inner);
            Assert.Equal("outer", tenantContext.TenantInfo?.Identifier);
        });
    }

    [Fact]
    public async Task LeaveNoTenantScopeWhenTheCallerHadNone()
    {
        await using var provider = BuildProvider();
        var scopeProvider = provider.GetRequiredService<ITenantScopeProvider>();

        await Task.Run(async () =>
        {
            Assert.False(scopeProvider.HasScope);

            await provider.RunInTenantScopeAsync(Tenant("initech"), _ => Task.CompletedTask);

            Assert.False(scopeProvider.HasScope);
        });
    }

    [Fact]
    public async Task UseANewServiceScopeAndDisposeIt()
    {
        await using var provider = BuildProvider();
        await using var callerScope = provider.CreateAsyncScope();
        callerScope.ServiceProvider.BeginTenantScope(Tenant("outer"));
        var callerService = callerScope.ServiceProvider.GetRequiredService<TenantCapturingService>();

        var innerService = await callerScope.ServiceProvider.RunInTenantScopeAsync(Tenant("initech"),
            sp => Task.FromResult(sp.GetRequiredService<TenantCapturingService>()));

        Assert.NotSame(callerService, innerService);
        Assert.Equal("outer", callerService.TenantIdentifier);
        Assert.Equal("initech", innerService.TenantIdentifier);
        Assert.True(innerService.Disposed);
        Assert.False(callerService.Disposed);
    }

    [Fact]
    public async Task DisposeScopeAndRestoreCallerWhenDelegateThrows()
    {
        await using var provider = BuildProvider();
        var tenantContext = provider.GetRequiredService<ITenantContext<TenantInfo, string>>();
        TenantCapturingService? innerService = null;

        await Task.Run(async () =>
        {
            provider.BeginTenantScope(Tenant("outer"));

            await Assert.ThrowsAsync<InvalidOperationException>(() =>
                provider.RunInTenantScopeAsync(Tenant("inner"), sp =>
                {
                    innerService = sp.GetRequiredService<TenantCapturingService>();
                    throw new InvalidOperationException();
                }));

            Assert.Equal("outer", tenantContext.TenantInfo?.Identifier);
        });

        Assert.NotNull(innerService);
        Assert.True(innerService.Disposed);
    }

    [Fact]
    public async Task IsolateConcurrentRuns()
    {
        await using var provider = BuildProvider();
        var identifiers = Enumerable.Range(0, 25).Select(i => $"tenant{i}").ToArray();

        var observed = await Task.WhenAll(identifiers.Select(id =>
            provider.RunInTenantScopeAsync(Tenant(id), async sp =>
            {
                await Task.Delay(5);
                return sp.GetRequiredService<ITenantContext<TenantInfo, string>>().TenantInfo?.Identifier;
            })));

        Assert.Equal(identifiers, observed);
    }

    [Fact]
    public async Task ThrowForNullRunInTenantScopeArguments()
    {
        await using var provider = BuildProvider();
        IServiceProvider nullProvider = null!;

        await Assert.ThrowsAsync<ArgumentNullException>(() =>
            nullProvider.RunInTenantScopeAsync(Tenant("initech"), _ => Task.CompletedTask));
        await Assert.ThrowsAsync<ArgumentNullException>(() =>
            provider.RunInTenantScopeAsync<string>(null!, _ => Task.CompletedTask));
        await Assert.ThrowsAsync<ArgumentNullException>(() =>
            provider.RunInTenantScopeAsync(Tenant("initech"), (Func<IServiceProvider, Task>)null!));
        await Assert.ThrowsAsync<ArgumentNullException>(() =>
            provider.RunInTenantScopeAsync(Tenant("initech"), (Func<IServiceProvider, Task<int>>)null!));
    }
}
