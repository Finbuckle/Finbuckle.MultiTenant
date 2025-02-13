using Finbuckle.MultiTenant.Abstractions;
using Finbuckle.MultiTenant.MassTransit.MassTransitFilters;

using Microsoft.Extensions.DependencyInjection;

using MassTransit;

using Moq;

using System.Threading.Tasks;

using Xunit;
using MassTransit.Testing;

namespace Finbuckle.MultiTenant.MassTransit.Test.MassTransitFilters
{
    public class TenantSendAndConsumeFilterShould
    {
        [Theory]
        [InlineData("tenant-1", "tenant-1")]
        [InlineData("tenant-2", "tenant-2")]
        [InlineData("tenant-3", "tenant-3")]
        public async Task UsingNamedFiltersSendMessageWithTenantIdentifierHeaderCreated(string? tenantIdentifier, string? expectedTenant)
        {
            // Arrange
            MultiTenantMassTransitTestSetupNamedFilters? setup = new MultiTenantMassTransitTestSetupNamedFilters().Setup();
            await setup.StartHarnessAsync();

            // Manually get the tenant
            IMultiTenantStore<TenantInfo>? mtStore = setup.ServiceProvider.GetRequiredService<IMultiTenantStore<TenantInfo>>();

            TenantInfo tenant = await mtStore.TryGetByIdentifierAsync(tenantIdentifier);

            // Set the tenant context to the tenant
            IMultiTenantContextSetter? mtcSetter = setup.ServiceProvider.GetRequiredService<IMultiTenantContextSetter>();
            mtcSetter.MultiTenantContext = new MultiTenantContext<TenantInfo>
            {
                TenantInfo = tenant
            };
            
            // Act

            ISendEndpoint? endpoint = await setup.Harness.GetConsumerEndpoint<TestMessageConsumer>();
            await endpoint.Send(new TestMessage("Hello, World!"));

            // Assert
            
            //Check sent message
            Assert.True(await setup.Harness.Sent.Any<TestMessage>());
            ISentMessage<TestMessage>? sentMessage = setup.Harness.Sent.Select<TestMessage>().FirstOrDefault();
            Assert.NotNull(sentMessage); // Ensure a message was sent

            bool headerExists = sentMessage.Context.Headers.TryGetHeader("__tenant__", out var headerValueSent);
            Assert.True(headerExists); // Verify the header exists
            Assert.Equal(expectedTenant, headerValueSent); // Verify the header value

            //Check consumed message
            Assert.True(await setup.Harness.Consumed.Any<TestMessage>());
            IReceivedMessage<TestMessage>? consumedMessage = setup.Harness.Consumed.Select<TestMessage>().FirstOrDefault();
            Assert.NotNull(consumedMessage); // Ensure a message was consumed

            headerExists = consumedMessage.Context.Headers.TryGetHeader("__tenant__", out var headerValueConsumed);
            Assert.True(headerExists); // Verify the header exists
            Assert.Equal(expectedTenant, headerValueConsumed); // Verify the header value

            await setup.StopHarnessAsync();
        }

        [Theory]
        [InlineData("", null)]
        [InlineData("tenant-4", null)]
        public async Task UsingNamedFiltersSendMessageWithNoTenantIdentifierHeader(string? tenantIdentifier, string? expectedTenant)
        {
            // Arrange
            MultiTenantMassTransitTestSetupNamedFilters? setup = new MultiTenantMassTransitTestSetupNamedFilters().Setup();
            await setup.StartHarnessAsync();

            // Manually get the tenant
            IMultiTenantStore<TenantInfo>? mtStore = setup.ServiceProvider.GetRequiredService<IMultiTenantStore<TenantInfo>>();

            TenantInfo tenant = await mtStore.TryGetByIdentifierAsync(tenantIdentifier);

            // Set the tenant context to the tenant
            IMultiTenantContextSetter? mtcSetter = setup.ServiceProvider.GetRequiredService<IMultiTenantContextSetter>();
            mtcSetter.MultiTenantContext = new MultiTenantContext<TenantInfo>
            {
                TenantInfo = tenant
            };

            // Act
            ISendEndpoint? endpoint = await setup.Harness.GetConsumerEndpoint<TestMessageConsumer>();
            await endpoint.Send(new TestMessage("Hello, World!"));

            // Assert

            //Check sent message
            Assert.True(await setup.Harness.Sent.Any<TestMessage>());
            ISentMessage<TestMessage>? sentMessage = setup.Harness.Sent.Select<TestMessage>().FirstOrDefault();
            Assert.NotNull(sentMessage); // Ensure a message was sent

            bool headerExists = sentMessage.Context.Headers.TryGetHeader("__tenant__", out var headerValueSent);
            Assert.False(headerExists); // Verify the header exists
            Assert.Equal(expectedTenant, headerValueSent); // Verify the header value

            //Check consumed message
            Assert.True(await setup.Harness.Consumed.Any<TestMessage>());
            var consumedMessage = setup.Harness.Consumed.Select<TestMessage>().FirstOrDefault();
            Assert.NotNull(consumedMessage); // Ensure a message was consumed

            headerExists = consumedMessage.Context.Headers.TryGetHeader("__tenant__", out var headerValueConsumed);
            Assert.False(headerExists); // Verify the header exists
            Assert.Equal(expectedTenant, headerValueConsumed); // Verify the header value

            await setup.StopHarnessAsync();
        }

        [Theory]
        [InlineData("tenant-1", "tenant-1")]
        [InlineData("tenant-2", "tenant-2")]
        [InlineData("tenant-3", "tenant-3")]
        public async Task UsingBusConfiguratorSendMessageWithTenantIdentifierHeaderCreated(string? tenantIdentifier, string? expectedTenant)
        {
            // Arrange
            MultiTenantMassTransitTestSetupBusConfigurator? setup = new MultiTenantMassTransitTestSetupBusConfigurator().Setup();
            await setup.StartHarnessAsync();

            // Manually get the tenant
            IMultiTenantStore<TenantInfo>? mtStore = setup.ServiceProvider.GetRequiredService<IMultiTenantStore<TenantInfo>>();

            TenantInfo tenant = await mtStore.TryGetByIdentifierAsync(tenantIdentifier);

            // Set the tenant context to the tenant
            IMultiTenantContextSetter? mtcSetter = setup.ServiceProvider.GetRequiredService<IMultiTenantContextSetter>();
            mtcSetter.MultiTenantContext = new MultiTenantContext<TenantInfo>
            {
                TenantInfo = tenant
            };

            // Act

            ISendEndpoint? endpoint = await setup.Harness.GetConsumerEndpoint<TestMessageConsumer>();
            await endpoint.Send(new TestMessage("Hello, World!"));

            // Assert

            //Check sent message
            Assert.True(await setup.Harness.Sent.Any<TestMessage>());
            ISentMessage<TestMessage>? sentMessage = setup.Harness.Sent.Select<TestMessage>().FirstOrDefault();
            Assert.NotNull(sentMessage); // Ensure a message was sent

            bool headerExists = sentMessage.Context.Headers.TryGetHeader("__tenant__", out var headerValueSent);
            Assert.True(headerExists); // Verify the header exists
            Assert.Equal(expectedTenant, headerValueSent); // Verify the header value

            //Check consumed message
            Assert.True(await setup.Harness.Consumed.Any<TestMessage>());
            IReceivedMessage<TestMessage>? consumedMessage = setup.Harness.Consumed.Select<TestMessage>().FirstOrDefault();
            Assert.NotNull(consumedMessage); // Ensure a message was consumed

            headerExists = consumedMessage.Context.Headers.TryGetHeader("__tenant__", out var headerValueConsumed);
            Assert.True(headerExists); // Verify the header exists
            Assert.Equal(expectedTenant, headerValueConsumed); // Verify the header value

            await setup.StopHarnessAsync();
        }

        [Theory]
        [InlineData("", null)]
        [InlineData("tenant-4", null)]
        public async Task UsingBusConfiguratorSendMessageWithNoTenantIdentifierHeader(string? tenantIdentifier, string? expectedTenant)
        {
            // Arrange
            MultiTenantMassTransitTestSetupBusConfigurator? setup = new MultiTenantMassTransitTestSetupBusConfigurator().Setup();
            await setup.StartHarnessAsync();

            // Manually get the tenant
            IMultiTenantStore<TenantInfo>? mtStore = setup.ServiceProvider.GetRequiredService<IMultiTenantStore<TenantInfo>>();

            TenantInfo tenant = await mtStore.TryGetByIdentifierAsync(tenantIdentifier);

            // Set the tenant context to the tenant
            IMultiTenantContextSetter? mtcSetter = setup.ServiceProvider.GetRequiredService<IMultiTenantContextSetter>();
            mtcSetter.MultiTenantContext = new MultiTenantContext<TenantInfo>
            {
                TenantInfo = tenant
            };

            // Act
            ISendEndpoint? endpoint = await setup.Harness.GetConsumerEndpoint<TestMessageConsumer>();
            await endpoint.Send(new TestMessage("Hello, World!"));

            // Assert

            //Check sent message
            Assert.True(await setup.Harness.Sent.Any<TestMessage>());
            ISentMessage<TestMessage>? sentMessage = setup.Harness.Sent.Select<TestMessage>().FirstOrDefault();
            Assert.NotNull(sentMessage); // Ensure a message was sent

            bool headerExists = sentMessage.Context.Headers.TryGetHeader("__tenant__", out var headerValueSent);
            Assert.False(headerExists); // Verify the header exists
            Assert.Equal(expectedTenant, headerValueSent); // Verify the header value

            //Check consumed message
            Assert.True(await setup.Harness.Consumed.Any<TestMessage>());
            var consumedMessage = setup.Harness.Consumed.Select<TestMessage>().FirstOrDefault();
            Assert.NotNull(consumedMessage); // Ensure a message was consumed

            headerExists = consumedMessage.Context.Headers.TryGetHeader("__tenant__", out var headerValueConsumed);
            Assert.False(headerExists); // Verify the header exists
            Assert.Equal(expectedTenant, headerValueConsumed); // Verify the header value

            await setup.StopHarnessAsync();
        }
    }

   
}
