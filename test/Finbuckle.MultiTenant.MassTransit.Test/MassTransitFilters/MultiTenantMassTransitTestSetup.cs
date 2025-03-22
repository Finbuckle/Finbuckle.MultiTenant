using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Finbuckle.MultiTenant.Abstractions;
using Finbuckle.MultiTenant.MassTransit.MassTransitFilters;

using MassTransit;
using MassTransit.Configuration;
using MassTransit.Testing;

using Microsoft.Extensions.DependencyInjection;

namespace Finbuckle.MultiTenant.MassTransit.Test.MassTransitFilters
{
    /// <summary>
    /// Setup for a test harness with named filters for multi-tenant MassTransit testing.
    /// </summary>
    internal class MultiTenantMassTransitTestSetupNamedFilters
    {
        public ServiceProvider ServiceProvider { get; private set; }
        public ITestHarness Harness { get; private set; }

        public MultiTenantMassTransitTestSetupNamedFilters Setup()
        {
            var services = new ServiceCollection();

            // Configure multi-tenant services, tenant resolver, and stores as needed
            services.AddMultiTenant<TenantInfo>()
                .WithMassTransitHeaderStrategy()
                .WithInMemoryStore(options =>
                {
                    options.Tenants.Add(new TenantInfo { Id = "tenant-1", Identifier = "tenant-1", Name = "Tenant 1" });
                    options.Tenants.Add(new TenantInfo { Id = "tenant-2", Identifier = "tenant-2", Name = "Tenant 2" });
                    options.Tenants.Add(new TenantInfo { Id = "tenant-3", Identifier = "tenant-3", Name = "Tenant 3" });
                });

            // Setup MassTransit with the test harness and apply tenant filters
            services.AddMassTransitTestHarness(cfg =>
            {
                cfg.AddConsumer<TestMessageConsumer>();
                cfg.UsingInMemory((context, cfg) =>
                {
                    // Apply the tenant filters
                    cfg.UseConsumeFilter(typeof(TenantConsumeFilter<>), context);
                    cfg.UseCompensateActivityFilter(typeof(TenantCompensateFilter<>), context);
                    cfg.UseExecuteActivityFilter(typeof(TenantExecuteFilter<>), context);
                    cfg.UsePublishFilter(typeof(TenantPublishFilter<>), context);
                    cfg.UseSendFilter(typeof(TenantSendFilter<>), context);
                    cfg.ConfigureEndpoints(context);
                }); 

            });

            ServiceProvider = services.BuildServiceProvider();
            Harness = ServiceProvider.GetRequiredService<ITestHarness>();

            return this;
        }

        public async Task StartHarnessAsync()
        {
            await Harness.Start();
        }

        public async Task StopHarnessAsync()
        {
            await Harness.Stop();
        }
    }

    /// <summary>
    /// setup for a test harness using the Bus Configurator for multi-tenant MassTransit testing.
    /// </summary>
    internal class MultiTenantMassTransitTestSetupBusConfigurator
    {
        public ServiceProvider ServiceProvider { get; private set; }
        public ITestHarness Harness { get; private set; }

        public MultiTenantMassTransitTestSetupBusConfigurator Setup()
        {
            var services = new ServiceCollection();

            // Configure multi-tenant services, tenant resolver, and stores as needed
            services.AddMultiTenant<TenantInfo>()
                .WithMassTransitHeaderStrategy()
                .WithInMemoryStore(options =>
                {
                    options.Tenants.Add(new TenantInfo { Id = "tenant-1", Identifier = "tenant-1", Name = "Tenant 1" });
                    options.Tenants.Add(new TenantInfo { Id = "tenant-2", Identifier = "tenant-2", Name = "Tenant 2" });
                    options.Tenants.Add(new TenantInfo { Id = "tenant-3", Identifier = "tenant-3", Name = "Tenant 3" });
                });

            // Setup MassTransit with the test harness and apply tenant filters
            services.AddMassTransitTestHarness(cfg =>
            {
                cfg.AddConsumer<TestMessageConsumer>();
                cfg.AddActivity<TestExecuteActivity, TestExecuteArguments, TestExecuteLog>();
                cfg.AddActivity<TestExecuteActivityThatFails, TestExecuteArguments, TestExecuteLog>();
                cfg.UsingInMemory((context, cfg) =>
                {
                    cfg.AddTenantFilters(context);
                    cfg.ConfigureEndpoints(context);
                });

            });

            ServiceProvider = services.BuildServiceProvider();
            Harness = ServiceProvider.GetRequiredService<ITestHarness>();

            return this;
        }

        public async Task StartHarnessAsync()
        {
            await Harness.Start();
        }

        public async Task StopHarnessAsync()
        {
            await Harness.Stop();
        }
    }

    public class TestMessageConsumer : IConsumer<TestMessage>
    {
        public Task Consume(ConsumeContext<TestMessage> context)
        {
            return Task.CompletedTask;
        }
    }

    public class TestMessage
    {
        public string Content { get; set; }

        public TestMessage(string content)
        {
            Content = content;
        }
    }

    public class TestExecuteActivity : IActivity<TestExecuteArguments, TestExecuteLog>
    {
        private readonly IMultiTenantContextAccessor _multiTenantContextAccessor;

        public TestExecuteActivity(IMultiTenantContextAccessor multiTenantContextAccessor)
        {
            _multiTenantContextAccessor = multiTenantContextAccessor;
        }

        public Task<ExecutionResult> Execute(ExecuteContext<TestExecuteArguments> context)
        {
            return Task.FromResult(context.Completed(new TestExecuteLog(_multiTenantContextAccessor.MultiTenantContext.TenantInfo?.Identifier)));
        }

        public Task<CompensationResult> Compensate(CompensateContext<TestExecuteLog> context)
        {
            return Task.FromResult(context.Compensated(new TestExecuteLog(_multiTenantContextAccessor.MultiTenantContext.TenantInfo?.Identifier)));
        }
    }

    public class TestExecuteActivityThatFails : IActivity<TestExecuteArguments, TestExecuteLog>
    {
        private readonly IMultiTenantContextAccessor _multiTenantContextAccessor;

        public TestExecuteActivityThatFails(IMultiTenantContextAccessor multiTenantContextAccessor)
        {
            _multiTenantContextAccessor = multiTenantContextAccessor;
        }

        public Task<ExecutionResult> Execute(ExecuteContext<TestExecuteArguments> context)
        {
            return Task.FromResult(context.FaultedWithVariables(new Exception("Faulted"), new TestExecuteLog(_multiTenantContextAccessor.MultiTenantContext.TenantInfo?.Identifier)));
        }

        public Task<CompensationResult> Compensate(CompensateContext<TestExecuteLog> context)
        {
            return Task.FromResult(context.Compensated());
        }
    }

    public record TestExecuteArguments();

    public record TestExecuteLog(string? Identifier);
}
