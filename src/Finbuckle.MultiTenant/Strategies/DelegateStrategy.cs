// Copyright Finbuckle LLC, Andrew White, and Contributors.
// Refer to the solution LICENSE file for more information.

using Finbuckle.MultiTenant.Abstractions;

namespace Finbuckle.MultiTenant.Strategies;

/// <summary>
/// The DelegateStrategy class implements the IMultiTenantStrategy interface and uses a delegate to determine the tenant identifier.
/// </summary>
public class DelegateStrategy : IMultiTenantStrategy
{
    private readonly Func<object, Task<string?>> _doStrategy;

    /// <summary>
    /// Initializes a new instance of the DelegateStrategy class.
    /// </summary>
    /// <param name="doStrategy">A delegate that encapsulates a method to determine the tenant identifier.</param>
    /// <exception cref="ArgumentNullException">Thrown when doStrategy is null.</exception>
    public DelegateStrategy(Func<object, Task<string?>> doStrategy)
    {
        _doStrategy = doStrategy ?? throw new ArgumentNullException(nameof(doStrategy));
    }

    /// <summary>
    /// Asynchronously determines the tenant identifier using the encapsulated delegate.
    /// </summary>
    /// <param name="context">The context used by the delegate to determine the tenant identifier.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the tenant identifier or null if not found.</returns>
    public async Task<string?> GetIdentifierAsync(object context)
    {
        var identifier = await _doStrategy(context);
        return await Task.FromResult(identifier);
    }
}