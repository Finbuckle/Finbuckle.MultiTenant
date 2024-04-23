// Copyright Finbuckle LLC, Andrew White, and Contributors.
// Refer to the solution LICENSE file for more information.

using Finbuckle.MultiTenant.Abstractions;
using Microsoft.Extensions.Logging;

namespace Finbuckle.MultiTenant.Strategies;

/// <summary>
/// Wraps an IMultiTenantStrategy and logs exceptions.
/// </summary>
public class MultiTenantStrategyWrapper : IMultiTenantStrategy
{
    /// <summary>
    /// Gets the IMultiTenantStrategy instance that this wrapper is wrapping.
    /// </summary>
    public IMultiTenantStrategy Strategy { get; }

    private readonly ILogger logger;

    /// <summary>
    /// Initializes a new instance of the MultiTenantStrategyWrapper class.
    /// </summary>
    /// <param name="strategy">The IMultiTenantStrategy instance to wrap.</param>
    /// <param name="logger">An instance of ILogger.</param>
    /// <exception cref="ArgumentNullException">Thrown when strategy or logger is null.</exception>
    public MultiTenantStrategyWrapper(IMultiTenantStrategy strategy, ILogger logger)
    {
        this.Strategy = strategy ?? throw new ArgumentNullException(nameof(strategy));
        this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Asynchronously determines the tenant identifier using the wrapped strategy.
    /// </summary>
    /// <param name="context">The context used by the strategy to determine the tenant identifier.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the tenant identifier.</returns>
    /// <exception cref="ArgumentNullException">Thrown when context is null.</exception>
    /// <exception cref="MultiTenantException">Thrown when an exception occurs in the wrapped strategy's GetIdentifierAsync method.</exception>
    public async Task<string?> GetIdentifierAsync(object context)
    {
        if (context == null)
        {
            throw new ArgumentNullException(nameof(context));
        }

        string? identifier = null;

        try
        {
            identifier = await Strategy.GetIdentifierAsync(context);
        }
        catch (Exception e)
        {
            logger.LogError(e, "Exception in GetIdentifierAsync");
            throw new MultiTenantException($"Exception in {Strategy.GetType()}.GetIdentifierAsync.", e);
        }

        if(identifier != null)
        {
            if (logger.IsEnabled(LogLevel.Debug))
            {
                logger.LogDebug("GetIdentifierAsync: Found identifier: \"{Identifier}\"", identifier);
            }
        }
        else
        {
            if (logger.IsEnabled(LogLevel.Debug))
            {
                logger.LogDebug("GetIdentifierAsync: No identifier found");
            }
        }

        return identifier;
    }
}