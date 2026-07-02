// Copyright Finbuckle LLC, Andrew White, and Contributors.
// Refer to the solution LICENSE file for more information.

// Notice: This class is based on the OptionsMonitor implementation in Microsoft.Extensions.Options, but modified to use
// a cache that is tenant-aware and centralizes change notifications. The original licensed source code can be found here:
// https://github.com/dotnet/dotnet/blob/main/src/runtime/src/libraries/Microsoft.Extensions.Options/src/OptionsMonitor.cs

using Finbuckle.MultiTenant.Abstractions;
using Microsoft.Extensions.Options;

namespace Finbuckle.MultiTenant.Options;

/// <summary>
/// Implements <see cref="IOptionsMonitor{TOptions}"/>.
/// </summary>
/// <remarks>
/// This class is designed to be used as a scoped service, with singleton backed cache and source change handling.
/// <see cref="OnChange"/> registrations capture the current tenant id from <see cref="ITenantContext"/> at registration time and are invoked with the updated options instance for that tenant when a source token fires.
/// </remarks>
/// <typeparam name="TOptions">The options type.</typeparam>
public class MultiTenantOptionsMonitor<TOptions> :
    IOptionsMonitor<TOptions>,
    IDisposable
    where TOptions : class
{
    private readonly MultiTenantOptionsCache<TOptions> _cache;
    private readonly MultiTenantOptionsChangeTokenHub<TOptions> _changeTokenHub;
    private readonly ITenantContext _tenantContext;
    private readonly IOptionsFactory<TOptions> _factory;

    /// <summary>
    /// Initializes a new instance of <see cref="OptionsMonitor{TOptions}"/> with the specified factory, change-token hub, and cache.
    /// </summary>
    /// <param name="factory">The factory to use to create options.</param>
    /// <param name="changeTokenHub">The singleton hub used to listen for changes to the options instance.</param>
    /// <param name="cache">The <see cref="MultiTenantOptionsCache{TOptions}"/> used to store options.</param>
    /// <param name="tenantContext">The tenant context used to determine the current tenant.</param>
    public MultiTenantOptionsMonitor(
        IOptionsFactory<TOptions> factory,
        MultiTenantOptionsChangeTokenHub<TOptions> changeTokenHub,
        MultiTenantOptionsCache<TOptions> cache,
        ITenantContext tenantContext)
    {
        _factory = factory;
        _cache = cache;
        _changeTokenHub = changeTokenHub;
        _tenantContext = tenantContext;
    }

    /// <inheritdoc />
    public TOptions CurrentValue => Get(Microsoft.Extensions.Options.Options.DefaultName);

    /// <inheritdoc />
    public TOptions Get(string? name)
    {
        name ??= Microsoft.Extensions.Options.Options.DefaultName;
        return _cache.GetOrAdd(name, _tenantContext.TenantInfo?.Id, () => _factory.Create(name));
    }

    /// <summary>
    /// Registers a listener to be called whenever <typeparamref name="TOptions"/> changes.
    /// </summary>
    /// <remarks>
    /// The registration captures the current tenant id from <see cref="ITenantContext"/> at registration time.
    /// When a source token fires, the updated options for that tenant are retrieved and the listener is invoked.
    /// Dispose the returned token to unregister the callback.
    /// </remarks>
    /// <param name="listener">The action to be invoked when <typeparamref name="TOptions"/> has changed.</param>
    /// <returns>An <see cref="IDisposable"/> that should be disposed to stop listening for changes.</returns>
    public IDisposable OnChange(Action<TOptions, string> listener)
    {
        var tenantId = _tenantContext.TenantInfo?.Id;
        return _changeTokenHub.Register(tenantId, listener);
    }

#pragma warning disable CA1816
    /// <summary>
    /// Disposes the monitor instance. Change-token subscriptions are owned by the singleton hub.
    /// </summary>
    public void Dispose()
#pragma warning restore CA1816
    {
        // Intentionally no-op.
    }
}