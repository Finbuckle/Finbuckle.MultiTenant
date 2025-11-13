// Copyright Finbuckle LLC, Andrew White, and Contributors.
// Refer to the solution LICENSE file for more information.

//    Portions of this file are derived from the .NET Foundation source file located at:
//    https://github.com/aspnet/Options/blob/dev/src/Microsoft.Extensions.Options/OptionsManager.cs

using Microsoft.Extensions.Options;

namespace Finbuckle.MultiTenant.Options;

/// <summary>
/// Implementation of <see cref="IOptions{TOptions}"/> and <see cref="IOptionsSnapshot{TOptions}"/> that uses dependency injection for its private cache.
/// </summary>
/// <typeparam name="TOptions">The type of options being configured.</typeparam>
public class MultiTenantOptionsManager<TOptions> : IOptionsSnapshot<TOptions> where TOptions : class
{
    private readonly IOptionsFactory<TOptions> _factory;
    private readonly IOptionsMonitorCache<TOptions> _cache; // Note: this is a private cache

    /// <summary>
    /// Initializes a new instance with the specified options configurations.
    /// </summary>
    /// <param name="factory">The factory to use to create options.</param>
    /// <param name="cache">The cache used for options.</param>
    public MultiTenantOptionsManager(IOptionsFactory<TOptions> factory, IOptionsMonitorCache<TOptions> cache)
    {
        _factory = factory;
        _cache = cache;
    }

    /// <inheritdoc />
    public TOptions Value => Get(Microsoft.Extensions.Options.Options.DefaultName);

    /// <inheritdoc />
    public TOptions Get(string? name)
    {
        name ??= Microsoft.Extensions.Options.Options.DefaultName;

        // Store the options in our instance cache.
        return _cache.GetOrAdd(name, () => _factory.Create(name));
    }

    /// <summary>
    /// Clears the options stored in the internal cache.
    /// </summary>
    public void Reset()
    {
        _cache.Clear();
    }
}