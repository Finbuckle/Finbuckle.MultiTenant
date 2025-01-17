using Finbuckle.MultiTenant.Abstractions;
using Finbuckle.MultiTenant.Events;

using MassTransit;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Finbuckle.MultiTenant.MassTransit.MassTransitFilters
{
    /// <summary>
    /// <see href="https://masstransit.io/">MassTransit</see> <see href="https://masstransit.io/documentation/configuration/middleware/scoped">Scoped Filter</see> to resolve the 
    /// <see cref="ITenantInfo.Identifier"/> from a header within the MassTransit message.
    /// This is scoped to the CompensateContext.
    /// </summary>
    /// <typeparam name="T">The Type of the Compensate</typeparam>
    /// <param name="tenantResolver">Injected via Dependency Injection. <see cref="ITenantResolver"/>.</param>
    /// <param name="mtcSetter">Injected via Dependency Injection. <see cref="IMultiTenantContextSetter"/></param>
    /// <example> 
    /// <code>
    /// <![CDATA[ builder.Services.AddMassTransit(x =>
    ///        {
    ///            x.AddConsumer=<GettingStartedConsumer=></GettingStartedConsumer>();
    ///            x.UsingInMemory((context, cfg) =>
    ///            {
    ///                cfg.UseCompensateActivityFilter(typeof(TenantCompensateFilter<>), context);
    ///                cfg.ConfigureEndpoints(context);
    ///            });
    ///        });]]>
    /// </code>
    /// </example>
    public class TenantCompensateFilter<T> : IFilter<CompensateContext<T>>
        where T : class
    {

        ITenantResolver _tenantResolver;
        IMultiTenantContextSetter _mtcSetter;

        public TenantCompensateFilter(ITenantResolver tenantResolver, IMultiTenantContextSetter mtcSetter)
        {
            _tenantResolver = tenantResolver;
            _mtcSetter = mtcSetter;
        }

        public void Probe(ProbeContext context)
        {
            context.CreateFilterScope("tenantConsumeFilter");
        }

        /// <summary>
        /// If configured, called by MassTransit during the message processing pipeline and resolves the tenant in Finbukcle.MultiTenant. Loosely based on Finbuckle.MultiTenant.AspNetCore.Internal.MultiTenantMiddleware.
        /// </summary>
        /// <param name="context">Current MassTransit Consumer Context.</param>
        /// <param name="next">The next step in the MassTransit pipeline.</param>
        /// <returns></returns>
        /// <remarks>The idea here is that MassTransit calls this as part of its own middleware so we in effect embed Finbuckle Tenant Resolving capabilities into the MassTransit middleware.</remarks>
        public async Task Send(CompensateContext<T> context, IPipe<CompensateContext<T>> next)
        {
            IMultiTenantContext? multiTenantContext = await _tenantResolver.ResolveAsync(context);
            _mtcSetter.MultiTenantContext = multiTenantContext;

            await next.Send(context);
        }
    }
}
