// Copyright Finbuckle LLC, Andrew White, and Contributors.
// Refer to the solution LICENSE file for more information.

using Finbuckle.MultiTenant.Abstractions;
using Finbuckle.MultiTenant.Options;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Xunit;

namespace Finbuckle.MultiTenant.Test.DependencyInjection;

public class ServiceCollectionExtensionsShould
{
    [Fact]
    public void RegisterITenantResolverInDi()
    {
            var services = new ServiceCollection();
            services.AddMultiTenant<TenantInfo>();

            var service = services.SingleOrDefault(s => s.ServiceType == typeof(ITenantResolver));

            Assert.NotNull(service);
            Assert.Equal(ServiceLifetime.Scoped, service.Lifetime);
        }

    [Fact]
    public void RegisterITenantResolverGenericInDi()
    {
            var services = new ServiceCollection();
            services.AddMultiTenant<TenantInfo>();

            var service = services.SingleOrDefault(s => s.ServiceType == typeof(ITenantResolver<TenantInfo>));

            Assert.NotNull(service);
            Assert.Equal(ServiceLifetime.Scoped, service.Lifetime);
        }

    [Fact]
    public void RegisterIMultiTenantContextAccessorInDi()
    {
            var services = new ServiceCollection();
            services.AddMultiTenant<TenantInfo>();

            var service = services.SingleOrDefault(s => s.Lifetime == ServiceLifetime.Singleton &&
                                                        s.ServiceType == typeof(IMultiTenantContextAccessor));

            Assert.NotNull(service);
            Assert.Equal(ServiceLifetime.Singleton, service.Lifetime);
        }

    [Fact]
    public void RegisterIMultiTenantContextAccessorGenericInDi()
    {
            var services = new ServiceCollection();
            services.AddMultiTenant<TenantInfo>();

            var service = services.SingleOrDefault(s => s.Lifetime == ServiceLifetime.Singleton &&
                                                        s.ServiceType ==
                                                        typeof(IMultiTenantContextAccessor<TenantInfo>));

            Assert.NotNull(service);
            Assert.Equal(ServiceLifetime.Singleton, service.Lifetime);
        }

    [Fact]
    public void RegisterMultiTenantOptionsInDi()
    {
            var services = new ServiceCollection();
            services.AddMultiTenant<TenantInfo>();

            var service = services.FirstOrDefault(s => s.Lifetime == ServiceLifetime.Singleton &&
                                                        s.ServiceType == typeof(IConfigureOptions<MultiTenantOptions<TenantInfo>>));

            Assert.NotNull(service);
            Assert.Equal(ServiceLifetime.Singleton, service.Lifetime);
        }

    public class TestOptions
    {
        public string? Prop1 { get; set; }
    }

    [Fact]
    public void RegisterNamedOptionsPerTenant()
    {
            var services = new ServiceCollection();
            services.AddMultiTenant<TenantInfo>();
            services.ConfigurePerTenant<TestOptions, TenantInfo>("name1",
                (option, tenant) => option.Prop1 = tenant.Id);
            var sp = services.BuildServiceProvider();

            var configs = sp.GetRequiredService<IEnumerable<IConfigureOptions<TestOptions>>>();
            var config = configs.Where(config => config is ConfigureNamedOptions<TestOptions, IMultiTenantContextAccessor<TenantInfo>> options).ToList();

            Assert.Single(config);
            Assert.Equal("name1", config.Select(c => (ConfigureNamedOptions<TestOptions, IMultiTenantContextAccessor<TenantInfo>>)c).Single().Name);
        }

    [Fact]
    public void RegisterUnnamedOptionsPerTenant()
    {
            var services = new ServiceCollection();
            services.AddMultiTenant<TenantInfo>();
            services.ConfigurePerTenant<TestOptions, TenantInfo>((option, tenant) => option.Prop1 = tenant.Id);
            var sp = services.BuildServiceProvider();

            var configs = sp.GetRequiredService<IEnumerable<IConfigureOptions<TestOptions>>>();
            var config = configs.Where(config => config is ConfigureNamedOptions<TestOptions, IMultiTenantContextAccessor<TenantInfo>> options).ToList();

            Assert.Single(config);
            Assert.Equal(Microsoft.Extensions.Options.Options.DefaultName,
                config.Select(c => (ConfigureNamedOptions<TestOptions, IMultiTenantContextAccessor<TenantInfo>>)c).Single().Name);
        }
        
    [Fact]
    public void RegisterAllOptionsPerTenant()
    {
            var services = new ServiceCollection();
            services.AddMultiTenant<TenantInfo>();
            services.ConfigureAllPerTenant<TestOptions, TenantInfo>((option, tenant) => option.Prop1 = tenant.Id);
            var sp = services.BuildServiceProvider();

            var configs = sp.GetRequiredService<IEnumerable<IConfigureOptions<TestOptions>>>();
            var config = configs.Where(config => config is ConfigureNamedOptions<TestOptions, IMultiTenantContextAccessor<TenantInfo>> options).ToList();

            Assert.Single(config);
            Assert.Null(config.Select(c => (ConfigureNamedOptions<TestOptions, IMultiTenantContextAccessor<TenantInfo>>)c).Single().Name);
        }
    [Fact]
    public void ReplaceExactClosedOptionsRegistrationsAndKeepOpenGenericDefaults()
    {
        var services = new ServiceCollection();
        services.AddMultiTenant<TenantInfo>();
        services.AddOptions();
        services.AddSingleton<IOptionsMonitorCache<TestOptions>, OptionsCache<TestOptions>>();
        services.AddSingleton<IOptions<TestOptions>>(Microsoft.Extensions.Options.Options.Create(new TestOptions()));
        services.AddScoped<IOptionsSnapshot<TestOptions>, CustomOptionsSnapshot>();

        services.ConfigurePerTenant<TestOptions, TenantInfo>((_, _) => { });

        Assert.Contains(services, descriptor => descriptor.ServiceType == typeof(IOptions<>));
        Assert.Contains(services, descriptor => descriptor.ServiceType == typeof(IOptionsSnapshot<>));
        Assert.Contains(services, descriptor => descriptor.ServiceType == typeof(IOptionsMonitorCache<>));
        Assert.Single(services, descriptor => descriptor.ServiceType == typeof(IOptions<TestOptions>));
        Assert.Single(services, descriptor => descriptor.ServiceType == typeof(IOptionsSnapshot<TestOptions>));
        var cacheDescriptor = Assert.Single(services,
            descriptor => descriptor.ServiceType == typeof(IOptionsMonitorCache<TestOptions>));
        Assert.Equal(typeof(MultiTenantOptionsCache<TestOptions>), cacheDescriptor.ImplementationType);

        using var provider = services.BuildServiceProvider();
        Assert.IsType<MultiTenantOptionsManager<TestOptions>>(provider.GetRequiredService<IOptions<TestOptions>>());
        using var scope = provider.CreateScope();
        Assert.IsType<MultiTenantOptionsManager<TestOptions>>(
            scope.ServiceProvider.GetRequiredService<IOptionsSnapshot<TestOptions>>());
    }

    [Fact]
    public void LeaveOneClosedRegistrationAfterRepeatedPerTenantConfiguration()
    {
        var services = new ServiceCollection();
        services.AddMultiTenant<TenantInfo>();
        services.ConfigurePerTenant<TestOptions, TenantInfo>((_, _) => { });
        services.AddSingleton<IOptionsMonitorCache<TestOptions>, OptionsCache<TestOptions>>();

        services.PostConfigurePerTenant<TestOptions, TenantInfo>((_, _) => { });

        Assert.Single(services, descriptor => descriptor.ServiceType == typeof(IOptions<TestOptions>));
        Assert.Single(services, descriptor => descriptor.ServiceType == typeof(IOptionsSnapshot<TestOptions>));
        var cacheDescriptor = Assert.Single(services,
            descriptor => descriptor.ServiceType == typeof(IOptionsMonitorCache<TestOptions>));
        Assert.Equal(typeof(MultiTenantOptionsCache<TestOptions>), cacheDescriptor.ImplementationType);
    }

    [Fact]
    public void DecorateImplementationTypeInstanceAndFactoryRegistrations()
    {
        var services = new ServiceCollection();
        var instance = new TestService();
        services.AddSingleton<ITestService, TestService>();
        services.AddSingleton<ITestService>(instance);
        services.AddSingleton<ITestService>(_ => new TestService());

        Assert.True(services.DecorateService<ITestService, DecoratedTestService>());

        using var provider = services.BuildServiceProvider();
        var decorated = provider.GetServices<ITestService>().Cast<DecoratedTestService>().ToList();
        Assert.Equal(3, decorated.Count);
        Assert.All(decorated, service => Assert.IsType<TestService>(service.Inner));
        Assert.Same(instance, decorated[1].Inner);
    }

    [Fact]
    public void PreserveRegistrationOrderAndStackDecorators()
    {
        var services = new ServiceCollection();
        var first = new TestService();
        var second = new TestService();
        services.AddSingleton<IOtherService, OtherService>();
        services.AddSingleton<ITestService>(first);
        services.AddSingleton<IOtherService, OtherService>();
        services.AddSingleton<ITestService>(second);
        var serviceTypesBefore = services.Select(s => s.ServiceType).ToArray();

        services.DecorateService<ITestService, DecoratedTestService>();
        services.DecorateService<ITestService, SecondDecoratedTestService>();

        Assert.Equal(serviceTypesBefore, services.Select(s => s.ServiceType).ToArray());
        using var provider = services.BuildServiceProvider();
        var outer = Assert.IsType<SecondDecoratedTestService>(provider.GetRequiredService<ITestService>());
        var inner = Assert.IsType<DecoratedTestService>(outer.Inner);
        Assert.Same(second, inner.Inner);
    }

#if NET8_0_OR_GREATER
    [Fact]
    public void LeaveKeyedRegistrationsUntouched()
    {
        var services = new ServiceCollection();
        var unkeyed = new TestService();
        services.AddKeyedSingleton<ITestService, TestService>("key");
        services.AddSingleton<ITestService>(unkeyed);

        services.DecorateService<ITestService, DecoratedTestService>();

        using var provider = services.BuildServiceProvider();
        var decorated = Assert.IsType<DecoratedTestService>(provider.GetRequiredService<ITestService>());
        Assert.Same(unkeyed, decorated.Inner);
        Assert.IsType<TestService>(provider.GetKeyedService<ITestService>("key"));
    }
#endif

    [Fact]
    public void RejectInvalidDecoratorArgumentsBeforeChangingRegistrations()
    {
        var services = new ServiceCollection();
        services.AddSingleton<ITestService, TestService>();
        var serviceCount = services.Count;

        Assert.Throws<ArgumentException>(() => services.DecorateService<ITestService, IncompatibleDecorator>());
        Assert.Equal(serviceCount, services.Count);
        Assert.Equal(typeof(TestService), services.Single().ImplementationType);
        Assert.Throws<ArgumentNullException>(() =>
            FinbuckleServiceCollectionExtensions.DecorateService<ITestService, DecoratedTestService>(
                null!, Array.Empty<object>()));
        Assert.Throws<ArgumentNullException>(() =>
            FinbuckleServiceCollectionExtensions.DecorateService<ITestService, DecoratedTestService>(
                services, (object[])null!));
    }

    private sealed class CustomOptionsSnapshot : IOptionsSnapshot<TestOptions>
    {
        public TestOptions Value { get; } = new();
        public TestOptions Get(string? name) => Value;
    }

    public interface ITestService
    {
    }

    public sealed class TestService : ITestService
    {
    }

    public sealed class DecoratedTestService : ITestService
    {
        public ITestService Inner { get; }
        public DecoratedTestService(ITestService inner) => Inner = inner;
    }
    public sealed class SecondDecoratedTestService : ITestService
    {
        public ITestService Inner { get; }
        public SecondDecoratedTestService(ITestService inner) => Inner = inner;
    }

    public sealed class IncompatibleDecorator
    {
        public IncompatibleDecorator(ITestService inner)
        {
        }
    }

    public interface IOtherService
    {
    }

    public sealed class OtherService : IOtherService
    {
    }
}
