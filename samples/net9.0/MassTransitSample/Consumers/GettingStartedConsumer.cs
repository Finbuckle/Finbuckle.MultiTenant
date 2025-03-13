using Finbuckle.MultiTenant.Abstractions;

using MassTransit;

using MassTransitSample.Contracts;

using Microsoft.Extensions.Logging;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MassTransitSample.Consumers
{
    public class GettingStartedConsumer : IConsumer<HelloMessage>
    {
        readonly ILogger<GettingStartedConsumer> _logger;
        readonly IMultiTenantContextAccessor _mtca;

        public GettingStartedConsumer(ILogger<GettingStartedConsumer> logger, IMultiTenantContextAccessor mtca)
        {
            _logger = logger;
            _mtca = mtca;
        }

        //public GettingStartedConsumer(ILogger<GettingStartedConsumer> logger, ITenantInfo tenantInfo)
        //{
        //    _logger = logger;
        //    _tenantInfo = tenantInfo;
        //}

        public Task Consume(ConsumeContext<HelloMessage> context)
        {
            _logger.LogInformation("Service Bus Message received for tenant: {Tenant} Received: {Text}", _mtca.MultiTenantContext.TenantInfo.Identifier, context.Message.Text);

            //Console.Out.WriteLineAsync($"Received: {context.Message.Text}");

            return Task.CompletedTask;
        }
    }
}
