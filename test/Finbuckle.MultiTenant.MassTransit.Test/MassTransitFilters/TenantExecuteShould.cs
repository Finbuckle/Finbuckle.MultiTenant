using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Finbuckle.MultiTenant.Abstractions;

using MassTransit;
using MassTransit.Courier.Contracts;
using MassTransit.Courier.Messages;

using Microsoft.Extensions.DependencyInjection;

namespace Finbuckle.MultiTenant.MassTransit.Test.MassTransitFilters
{
    public class TenantExecuteShould
    {
        [Theory]
        [InlineData("tenant-1")]
        [InlineData("tenant-2")]
        [InlineData("tenant-3")]
        public async Task ExecuteAndCompensateShouldReturnCorrectIdentifiers(string tenantIdentifier)
        {
            // Arrange
            var setup = new MultiTenantMassTransitTestSetupBusConfigurator().Setup();
            await setup.StartHarnessAsync();

            // Manually get the tenant
            var mtStore = setup.ServiceProvider.GetRequiredService<IMultiTenantStore<TenantInfo>>();

            var tenant = await mtStore.TryGetByIdentifierAsync(tenantIdentifier);

            // Set the tenant context to the tenant
            var mtcSetter = setup.ServiceProvider.GetRequiredService<IMultiTenantContextSetter>();
            mtcSetter.MultiTenantContext = new MultiTenantContext<TenantInfo>
            {
                TenantInfo = tenant
            };

            // Act
            var slip = new RoutingSlipBuilder(Guid.NewGuid());
            var endpoint = setup.Harness.GetExecuteActivityAddress<TestExecuteActivity, TestExecuteArguments>();
            var endpoint2 = setup.Harness.GetExecuteActivityAddress<TestExecuteActivityThatFails, TestExecuteArguments>();
            slip.AddActivity("TestExecuteActivity", endpoint, new TestExecuteArguments());
            slip.AddActivity("TestExecuteActivityThatFails", endpoint2, new TestExecuteArguments());
            var routingSlip = slip.Build();

            await setup.Harness.Bus.Execute(routingSlip);

            // Assert
            Assert.True(await setup.Harness.Sent.Any<RoutingSlip>());
            Assert.True(await setup.Harness.Published.Any<RoutingSlipActivityCompleted>());
            Assert.True(await setup.Harness.Published.Any<RoutingSlipActivityFaulted>());
            Assert.True(await setup.Harness.Published.Any<RoutingSlipActivityCompensated>());
            var completedMessage = setup.Harness.Published.Select<RoutingSlipActivityCompleted>().FirstOrDefault();
            Assert.NotNull(completedMessage);

            var foundIdentifierCompleted =
                ((RoutingSlipActivityCompletedMessage)completedMessage.MessageObject).Data.FirstOrDefault(x =>
                    x.Key == "identifier").Value;

            Assert.Equal(tenantIdentifier, foundIdentifierCompleted);

            var compensatedMessage = setup.Harness.Published.Select<RoutingSlipActivityCompensated>().FirstOrDefault();
            Assert.NotNull(compensatedMessage);

            var foundIdentifierCompensated =
                ((RoutingSlipActivityCompensatedMessage)compensatedMessage.MessageObject).Data.FirstOrDefault(x =>
                    x.Key == "identifier").Value;

            Assert.Equal(tenantIdentifier, foundIdentifierCompensated);
        }
    }
}
