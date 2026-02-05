// Copyright Finbuckle LLC, Andrew White, and Contributors.
// Refer to the solution LICENSE file for more information.

using Finbuckle.MultiTenant.Abstractions;
using Microsoft.Extensions.Logging;

namespace Finbuckle.MultiTenant.Strategies;

/// <summary>
/// Multi-tenant strategy decorator that handles exception handling and logging.
/// </summary>
public class MultiTenantStrategyWrapper : IMultiTenantStrategy
{
    /// <summary>
    /// Gets the internal <see cref="IMultiTenantStrategy"/> instance.
    /// </summary>
    public IMultiTenantStrategy Strategy { get; }

    private readonly ILogger logger;

    /// <summary>
    /// Initializes a new instance of MultiTenantStrategyWrapper.
    /// </summary>
    /// <param name="strategy">The <see cref="IMultiTenantStrategy"/> instance to wrap.</param>
    /// <param name="logger">The logger instance.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="strategy"/> or <paramref name="logger"/> is null.</exception>
    public MultiTenantStrategyWrapper(IMultiTenantStrategy strategy, ILogger logger)
    {
        Strategy = strategy ?? throw new ArgumentNullException(nameof(strategy));
        this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <inheritdoc />
    public async Task<string?> GetIdentifierAsync(object context)
    {
        string? identifier = null;

        try
        {
            identifier = await Strategy.GetIdentifierAsync(context).ConfigureAwait(false);
        }
        catch (Exception e)
        {
            logger.LogError(e, "Exception in GetIdentifierAsync");
            throw new MultiTenantException($"Exception in {Strategy.GetType()}.GetIdentifierAsync.", e);
        }

        if (identifier != null)
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