// Copyright Finbuckle LLC, Andrew White, and Contributors.
// Refer to the solution LICENSE file for more information.

using Finbuckle.MultiTenant.Abstractions;

namespace Finbuckle.MultiTenant.Strategies;

/// <summary>
/// The StaticStrategy class implements the IMultiTenantStrategy interface and provides a static identifier to determine the tenant.
/// </summary>
public class StaticStrategy : IMultiTenantStrategy
{
    internal readonly string Identifier;

    /// <summary>
    /// Gets the priority of the strategy. Strategies with higher priority are evaluated first.
    /// </summary>
    public int Priority { get => -1000; }
    
    /// <summary>
    /// Initializes a new instance of the StaticStrategy class.
    /// </summary>
    /// <param name="identifier">The static identifier used to determine the tenant.</param>
    public StaticStrategy(string identifier)
    {
        this.Identifier = identifier;
    }

    /// <summary>
    /// Asynchronously determines the tenant identifier using the static identifier.
    /// </summary>
    /// <param name="context">The context used by the strategy to determine the tenant identifier. In this case, it's not used.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the tenant identifier.</returns>
    public async Task<string?> GetIdentifierAsync(object context)
    {
        return await Task.FromResult(Identifier);
    }
}