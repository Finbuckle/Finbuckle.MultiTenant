// Copyright Finbuckle LLC, Andrew White, and Contributors.
// Refer to the solution LICENSE file for more information.

using Finbuckle.MultiTenant.Abstractions;

namespace Finbuckle.MultiTenant.Strategies;

/// <summary>
/// A strategy that uses a delegate function to determine the tenant identifier.
/// </summary>
public class DelegateStrategy : IMultiTenantStrategy
{
    private readonly Func<object, Task<string?>> _doStrategy;

    /// <summary>
    /// Initializes a new instance of DelegateStrategy.
    /// </summary>
    /// <param name="doStrategy">The delegate function that returns the tenant identifier.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="doStrategy"/> is null.</exception>
    public DelegateStrategy(Func<object, Task<string?>> doStrategy)
    {
        _doStrategy = doStrategy ?? throw new ArgumentNullException(nameof(doStrategy));
    }

    /// <inheritdoc />
    public async Task<string?> GetIdentifierAsync(object context)
    {
        var identifier = await _doStrategy(context).ConfigureAwait(false);
        return await Task.FromResult(identifier).ConfigureAwait(false);
    }
}