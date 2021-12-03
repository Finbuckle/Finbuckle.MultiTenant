// Copyright Finbuckle LLC, Andrew White, and Contributors.
// Refer to the solution LICENSE file for more inforation.

using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace Finbuckle.MultiTenant.Strategies
{
    public class MultiTenantStrategyWrapper : IMultiTenantStrategy
    {
        public IMultiTenantStrategy Strategy { get; }

        private readonly ILogger logger;

        public MultiTenantStrategyWrapper(IMultiTenantStrategy strategy, ILogger logger)
        {
            this.Strategy = strategy ?? throw new ArgumentNullException(nameof(strategy));
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

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
}
