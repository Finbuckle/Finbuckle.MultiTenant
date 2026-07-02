// Copyright Finbuckle LLC, Andrew White, and Contributors.
// Refer to the solution LICENSE file for more information.

using Microsoft.Extensions.Options;
using Microsoft.Extensions.Primitives;

namespace Finbuckle.MultiTenant.Options;

/// <summary>
/// Singleton hub that subscribes to option change tokens and fans out change notifications to registered listeners.
/// </summary>
/// <typeparam name="TOptions">The options type.</typeparam>
public sealed class MultiTenantOptionsChangeTokenHub<TOptions> : IDisposable
    where TOptions : class
{
    private readonly MultiTenantOptionsCache<TOptions> _cache;
    private readonly IOptionsFactory<TOptions> _factory;
    private readonly List<IDisposable> _sourceRegistrations = new();
    private readonly Dictionary<Guid, ListenerRegistrationState> _listeners = new();
    private readonly object _gate = new();
    private bool _disposed;

    /// <summary>
    /// Initializes a new instance of the <see cref="MultiTenantOptionsChangeTokenHub{TOptions}"/> class.
    /// </summary>
    /// <param name="sources">The sources used to listen for changes to the options instance.</param>
    /// <param name="cache">The singleton cache used to invalidate and rebuild options.</param>
    /// <param name="factory">The options factory used to rebuild option instances on change.</param>
    public MultiTenantOptionsChangeTokenHub(
        IEnumerable<IOptionsChangeTokenSource<TOptions>> sources,
        MultiTenantOptionsCache<TOptions> cache,
        IOptionsFactory<TOptions> factory)
    {
        ArgumentNullException.ThrowIfNull(sources);

        _cache = cache;
        _factory = factory;

        foreach (var source in sources)
        {
            IDisposable registration = ChangeToken.OnChange(
                source.GetChangeToken,
                InvokeChanged,
                source.Name);

            _sourceRegistrations.Add(registration);
        }

        return;

        void InvokeChanged(string? name) => NotifyChanged(name);
    }

    /// <summary>
    /// Registers a listener that will be invoked when any option change token fires.
    /// </summary>
    /// <param name="tenantId">The tenant id for this registration.</param>
    /// <param name="listener">The callback to invoke when a change token fires.</param>
    /// <returns>An <see cref="IDisposable"/> that removes the listener when disposed.</returns>
    public IDisposable Register(string? tenantId, Action<TOptions, string> listener)
    {
        ArgumentNullException.ThrowIfNull(listener);

        var id = Guid.NewGuid();
        lock (_gate)
        {
            ThrowIfDisposed();
            _listeners.Add(id, new ListenerRegistrationState(tenantId, listener));
        }

        return new ListenerRegistration(this, id);
    }

    private void NotifyChanged(string? name)
    {
        name ??= Microsoft.Extensions.Options.Options.DefaultName;

        ListenerRegistrationState[] listeners;
        lock (_gate)
        {
            if (_disposed)
                return;

            listeners = _listeners.Values.ToArray();
        }

        // Global sources can affect any tenant option values.
        _cache.ClearAll();

        foreach (var registration in listeners)
        {
            var options = _cache.GetOrAdd(name, registration.TenantId, () => _factory.Create(name));
            registration.Listener(options, name);
        }
    }

    private void Unregister(Guid id)
    {
        lock (_gate)
        {
            if (_disposed)
                return;

            _listeners.Remove(id);
        }
    }

    private void ThrowIfDisposed()
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
    }

    /// <inheritdoc />
    public void Dispose()
    {
        lock (_gate)
        {
            if (_disposed)
                return;

            _disposed = true;
            _listeners.Clear();
        }

        foreach (var registration in _sourceRegistrations)
            registration.Dispose();

        _sourceRegistrations.Clear();
    }

    private sealed class ListenerRegistrationState(
        string? tenantId,
        Action<TOptions, string> listener)
    {
        public string? TenantId { get; } = tenantId;
        public Action<TOptions, string> Listener { get; } = listener;
    }

    private sealed class ListenerRegistration : IDisposable
    {
        private readonly MultiTenantOptionsChangeTokenHub<TOptions> _hub;
        private readonly Guid _id;
        private bool _disposed;

        public ListenerRegistration(MultiTenantOptionsChangeTokenHub<TOptions> hub, Guid id)
        {
            _hub = hub;
            _id = id;
        }

        public void Dispose()
        {
            if (_disposed)
                return;

            _disposed = true;
            _hub.Unregister(_id);
        }
    }
}

