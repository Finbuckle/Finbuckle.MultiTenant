// Copyright Finbuckle LLC, Andrew White, and Contributors.
// Refer to the solution LICENSE file for more information.

using Finbuckle.MultiTenant.Abstractions;
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
                                                   s.ServiceType ==
                                                   typeof(IConfigureOptions<MultiTenantOptions<TenantInfo>>));

        Assert.NotNull(service);
        Assert.Equal(ServiceLifetime.Singleton, service.Lifetime);
    }

    [Fact]
    public void DecorateService_WithImplementationType_WrapsService()
    {
        var services = new ServiceCollection();
        services.AddSingleton<ITestService, TestService>();
        bool result = services.DecorateService<ITestService, DecoratedTestService>();
        Assert.True(result);
        var provider = services.BuildServiceProvider();
        var service = provider.GetRequiredService<ITestService>();
        Assert.IsType<DecoratedTestService>(service);
        Assert.IsType<TestService>(((DecoratedTestService)service).Inner);
    }

    [Fact]
    public void DecorateService_WithImplementationInstance_WrapsService()
    {
        var instance = new TestService();
        var services = new ServiceCollection();
        services.AddSingleton<ITestService>(instance);
        bool result = services.DecorateService<ITestService, DecoratedTestService>();
        Assert.True(result);
        var provider = services.BuildServiceProvider();
        var service = provider.GetRequiredService<ITestService>();
        Assert.IsType<DecoratedTestService>(service);
        Assert.Same(instance, ((DecoratedTestService)service).Inner);
    }

    [Fact]
    public void DecorateService_WithImplementationFactory_WrapsService()
    {
        var services = new ServiceCollection();
        services.AddSingleton<ITestService>(sp => new TestService());
        bool result = services.DecorateService<ITestService, DecoratedTestService>();
        Assert.True(result);
        var provider = services.BuildServiceProvider();
        var service = provider.GetRequiredService<ITestService>();
        Assert.IsType<DecoratedTestService>(service);
        Assert.IsType<TestService>(((DecoratedTestService)service).Inner);
    }

    [Fact]
    public void DecorateService_ThrowsIfNoServiceFound()
    {
        var services = new ServiceCollection();
        Assert.Throws<ArgumentException>(() => services.DecorateService<ITestService, DecoratedTestService>());
    }

    [Fact]
    public void DecorateService_WrapsAllServicesOfSameType()
    {
        var services = new ServiceCollection();
        var instance1 = new TestService();
        var instance2 = new TestService();
        services.AddSingleton<ITestService>(instance1);
        services.AddSingleton<ITestService>(instance2);
        bool result = services.DecorateService<ITestService, DecoratedTestService>();
        Assert.True(result);
        var provider = services.BuildServiceProvider();
        var all = provider.GetServices<ITestService>().ToList();
        Assert.Equal(2, all.Count);
        Assert.All(all, s => Assert.IsType<DecoratedTestService>(s));
        var inners = all.Cast<DecoratedTestService>().Select(d => d.Inner).ToList();
        Assert.Contains(instance1, inners);
        Assert.Contains(instance2, inners);
    }

    public interface ITestService
    {
    }

    public class TestService : ITestService
    {
    }

    public class DecoratedTestService : ITestService
    {
        public ITestService Inner { get; }
        public DecoratedTestService(ITestService inner) => Inner = inner;
    }
}