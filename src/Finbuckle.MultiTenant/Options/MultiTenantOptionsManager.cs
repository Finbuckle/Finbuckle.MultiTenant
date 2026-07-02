// Copyright Finbuckle LLC, Andrew White, and Contributors.
// Refer to the solution LICENSE file for more information.

using Finbuckle.MultiTenant.Abstractions;
using Microsoft.Extensions.Options;

namespace Finbuckle.MultiTenant.Options;

/// <summary>
/// Implementation of <see cref="IOptions{TOptions}"/> and <see cref="IOptionsSnapshot{TOptions}"/> that uses dependency injection for its private cache.
/// </summary>
/// <typeparam name="TOptions">The type of options being configured.</typeparam>
public class MultiTenantOptionsManager<TOptions> : IOptionsSnapshot<TOptions> where TOptions : class
{
    private readonly IOptionsFactory<TOptions> _factory;
    private readonly MultiTenantOptionsCache<TOptions> _cache; // Note: this is a private cache
    private readonly ITenantContext _tenantContext;

    /// <summary>
    /// Initializes a new instance with the specified options configurations.
    /// </summary>
    /// <param name="factory">The factory to use to create options.</param>
    /// <param name="cache">The cache used for options.</param>
    /// <param name="tenantContext">The current tenant context.</param>
    public MultiTenantOptionsManager(IOptionsFactory<TOptions> factory, MultiTenantOptionsCache<TOptions> cache, ITenantContext tenantContext)
    {
        _factory = factory;
        _cache = cache;
        _tenantContext = tenantContext;
    }

    /// <inheritdoc />
    public TOptions Value => Get(Microsoft.Extensions.Options.Options.DefaultName);

    /// <inheritdoc />
    public TOptions Get(string? name)
    {
        name ??= Microsoft.Extensions.Options.Options.DefaultName;

        // Store the options in our instance cache.
        return _cache.GetOrAdd(name, _tenantContext.TenantInfo?.Id, () => _factory.Create(name));
    }

    /// <summary>
    /// Clears the options for the current tenant.
    /// </summary>
    public void Reset()
    {
        _cache.Clear(_tenantContext.TenantInfo?.Id);
    }
}