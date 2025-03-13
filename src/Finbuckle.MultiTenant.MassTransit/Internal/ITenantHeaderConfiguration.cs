using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Finbuckle.MultiTenant.MassTransit.MassTransitFilters;
using Finbuckle.MultiTenant.MassTransit.Strategies;

using static System.Net.WebRequestMethods;

namespace Finbuckle.MultiTenant.MassTransit.Internal
{
    /// <summary>
    /// Used in <see href="https://masstransit.io/">MassTransit</see> <see cref="TenantConsumeFilter{T}"/> filter to configure the header keys used to identify tenants.
    /// Also used in the <see cref="MassTransitHeaderStrategy"/> to get the tenant identifier from the header. Supported filters are:
    /// <list type="bullet">
    /// <item><see cref="TenantConsumeFilter{T}"/></item>
    /// <item><see cref="TenantPublishFilter{T}"/></item>
    /// <item><see cref="TenantSendFilter{T}"/></item>
    /// </list>
    /// </summary>
    public interface ITenantHeaderConfiguration
    {
        string TenantIdentifierHeaderKey { get; }
    }
}
