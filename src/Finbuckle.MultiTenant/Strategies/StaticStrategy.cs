// Copyright Finbuckle LLC, Andrew White, and Contributors.
// Refer to the solution LICENSE file for more information.

using Finbuckle.MultiTenant.Abstractions;

namespace Finbuckle.MultiTenant.Strategies;

/// <summary>
/// A strategy that always returns a pre-configured tenant identifier.
/// </summary>
public class StaticStrategy : IMultiTenantStrategy
{
    // internal for testing
    // ReSharper disable once MemberCanBePrivate.Global
    internal readonly string Identifier;

    /// <inheritdoc />
    public int Priority => -1000;

    /// <summary>
    /// Initializes a new instance of StaticStrategy.
    /// </summary>
    /// <param name="identifier">The tenant identifier to return.</param>
    public StaticStrategy(string identifier)
    {
        Identifier = identifier;
    }

    /// <inheritdoc />
    public async Task<string?> GetIdentifierAsync(object context)
    {
        return await Task.FromResult(Identifier).ConfigureAwait(false);
    }
}