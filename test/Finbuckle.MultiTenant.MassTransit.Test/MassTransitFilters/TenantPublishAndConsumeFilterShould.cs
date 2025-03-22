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
    public class TenantPublishAndConsumeFilterShould
    {
        [Theory]
        [InlineData("tenant-1", "tenant-1")]
        [InlineData("tenant-2", "tenant-2")]
        [InlineData("tenant-3", "tenant-3")]
        public async Task UsingNamedFiltersPublishMessageWithTenantIdentifierHeaderCreated(string? tenantIdentifier, string? expectedTenant)
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
            setup.Harness.Bus.Publish<TestMessage>(new TestMessage("Hello, World!"));

            // Assert
            
            //Check published message
            Assert.True(await setup.Harness.Published.Any<TestMessage>());
            IPublishedMessage<TestMessage>? publishedMessage = setup.Harness.Published.Select<TestMessage>().FirstOrDefault();
            Assert.NotNull(publishedMessage); // Ensure a message was published

            bool headerExists = publishedMessage.Context.Headers.TryGetHeader("__tenant__", out var headerValuePublished);
            Assert.True(headerExists); // Verify the header exists
            Assert.Equal(expectedTenant, headerValuePublished); // Verify the header value

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
        public async Task UsingNamedFiltersPublishMessageWithNoTenantIdentifierHeader(string? tenantIdentifier, string? expectedTenant)
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
            setup.Harness.Bus.Publish<TestMessage>(new TestMessage("Hello, World!"));

            // Assert
            
            //Check published message
            Assert.True(await setup.Harness.Published.Any<TestMessage>());
            IPublishedMessage<TestMessage>? publishedMessage = setup.Harness.Published.Select<TestMessage>().FirstOrDefault();
            Assert.NotNull(publishedMessage); // Ensure a message was published

            bool headerExists = publishedMessage.Context.Headers.TryGetHeader("__tenant__", out var headerValuePublished);
            Assert.False(headerExists); // Verify the header exists
            Assert.Equal(expectedTenant, headerValuePublished); // Verify the header value

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
        public async Task UsingBusConfiguratorPublishMessageWithTenantIdentifierHeaderCreated(string? tenantIdentifier, string? expectedTenant)
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
            setup.Harness.Bus.Publish<TestMessage>(new TestMessage("Hello, World!"));

            // Assert

            //Check published message
            Assert.True(await setup.Harness.Published.Any<TestMessage>());
            IPublishedMessage<TestMessage>? publishedMessage = setup.Harness.Published.Select<TestMessage>().FirstOrDefault();
            Assert.NotNull(publishedMessage); // Ensure a message was published

            bool headerExists = publishedMessage.Context.Headers.TryGetHeader("__tenant__", out var headerValuePublished);
            Assert.True(headerExists); // Verify the header exists
            Assert.Equal(expectedTenant, headerValuePublished); // Verify the header value

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
        public async Task UsingBusConfiguratorPublishMessageWithNoTenantIdentifierHeader(string? tenantIdentifier, string? expectedTenant)
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
            setup.Harness.Bus.Publish<TestMessage>(new TestMessage("Hello, World!"));

            // Assert

            //Check published message
            Assert.True(await setup.Harness.Published.Any<TestMessage>());
            IPublishedMessage<TestMessage>? publishedMessage = setup.Harness.Published.Select<TestMessage>().FirstOrDefault();
            Assert.NotNull(publishedMessage); // Ensure a message was published

            bool headerExists = publishedMessage.Context.Headers.TryGetHeader("__tenant__", out var headerValuePublished);
            Assert.False(headerExists); // Verify the header exists
            Assert.Equal(expectedTenant, headerValuePublished); // Verify the header value

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
