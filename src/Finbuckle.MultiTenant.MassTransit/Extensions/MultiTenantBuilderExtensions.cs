using Finbuckle.MultiTenant.Abstractions;
using Finbuckle.MultiTenant.MassTransit.Strategies;
using Finbuckle.MultiTenant.Internal;

using Microsoft.Extensions.DependencyInjection;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Finbuckle.MultiTenant.MassTransit.Internal;

namespace Finbuckle.MultiTenant
{
    public static class MultiTenantBuilderExtensions
    {
        /// <summary>
        /// Adds and configures a MassTransitHeaderStrategy to the application with the default value as defined in <see cref="Constants.TenantToken"/>.
        /// </summary>
        /// <typeparam name="TTenantInfo">The ITenantInfo implementation type.</typeparam>
        /// <param name="builder">MultiTenantBuilder instance.</param>
        /// <returns>The same MultiTenantBuilder passed into the method.</returns>
        public static MultiTenantBuilder<TTenantInfo> WithMassTransitHeaderStrategy<TTenantInfo>(
            this MultiTenantBuilder<TTenantInfo> builder)
            where TTenantInfo : class, ITenantInfo, new()
        => builder.WithMassTransitHeaderStrategy($"{Constants.TenantToken}");

        /// <summary>
        /// Adds and configures a MassTransitHeaderStrategy to the application with a header value.
        /// </summary>
        /// <typeparam name="TTenantInfo">The ITenantInfo implementation type.</typeparam>
        /// <param name="builder">MultiTenantBuilder instance.</param>
        /// <param name="headerKey">The MassTransit header key to use.</param>
        /// <returns>The same MultiTenantBuilder passed into the method.</returns>
        public static MultiTenantBuilder<TTenantInfo> WithMassTransitHeaderStrategy<TTenantInfo>(
            this MultiTenantBuilder<TTenantInfo> builder,
            string headerKey)
            where TTenantInfo : class, ITenantInfo, new()
        {
            if (string.IsNullOrWhiteSpace(headerKey))
            {
                throw new ArgumentException("Header key cannot be null, empty or whitespace.", nameof(headerKey));
            }

            TenantHeaderConfiguration? headerConfiguration = new TenantHeaderConfiguration(headerKey);

            // Add the TenantHeaderConfiguration to the services collection for Dependency Injection to be picked up in the filters.
            builder.Services.AddSingleton<ITenantHeaderConfiguration>(headerConfiguration);

            return builder.WithStrategy<MassTransitHeaderStrategy>(ServiceLifetime.Singleton, headerConfiguration);
        }
    }
}
