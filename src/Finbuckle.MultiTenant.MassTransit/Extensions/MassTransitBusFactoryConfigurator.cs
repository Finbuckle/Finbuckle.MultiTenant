using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Finbuckle.MultiTenant;
using Finbuckle.MultiTenant.Abstractions;
using Finbuckle.MultiTenant.MassTransit.MassTransitFilters;

namespace MassTransit
{
    public static class MassTransitBusFactoryConfigurator 
    {
        public static void AddTenantFilters(this IBusFactoryConfigurator configurator, IRegistrationContext context)
        {
            configurator.UseConsumeFilter(typeof(TenantConsumeFilter<>), context);
            configurator.UseSendFilter(typeof(TenantSendFilter<>), context);
            configurator.UsePublishFilter(typeof(TenantPublishFilter<>), context);
            configurator.UseExecuteActivityFilter(typeof(TenantExecuteFilter<>), context);
            configurator.UseCompensateActivityFilter(typeof(TenantCompensateFilter<>), context);
        }
    }
}
