// Copyright Finbuckle LLC, Andrew White, and Contributors.
// Refer to the solution LICENSE file for more information.

using Finbuckle.MultiTenant.Abstractions;
using Finbuckle.MultiTenant.Extensions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Xunit;

namespace Finbuckle.MultiTenant.Test.Extensions;

public class ServiceCollectionExtensionsShould
{
    [Fact]
    public void RegisterITenantResolverInDi()
    {
        var services = new ServiceCollection();
        services.AddMultiTenant<TenantInfo, string>();

        var service = services.SingleOrDefault(s => s.ServiceType == typeof(ITenantResolver<string>));

        Assert.NotNull(service);
        Assert.Equal(ServiceLifetime.Scoped, service.Lifetime);
    }

    [Fact]
    public void RegisterITenantResolverGenericInDi()
    {
        var services = new ServiceCollection();
        services.AddMultiTenant<TenantInfo, string>();

        var service = services.SingleOrDefault(s => s.ServiceType == typeof(ITenantResolver<TenantInfo, string>));

        Assert.NotNull(service);
        Assert.Equal(ServiceLifetime.Scoped, service.Lifetime);
    }

    [Fact]
    public void RegisterITenantContextInDi()
    {
        var services = new ServiceCollection();
        services.AddMultiTenant<TenantInfo, string>();

        var service = services.SingleOrDefault(s => s.Lifetime == ServiceLifetime.Singleton &&
                                                    s.ServiceType == typeof(ITenantContext<string>));

        Assert.NotNull(service);
        Assert.Equal(ServiceLifetime.Singleton, service.Lifetime);
    }

    [Fact]
    public void RegisterITenantContextGenericInDi()
    {
        var services = new ServiceCollection();
        services.AddMultiTenant<TenantInfo, string>();

        var service = services.SingleOrDefault(s => s.Lifetime == ServiceLifetime.Singleton &&
                                                    s.ServiceType ==
                                                    typeof(ITenantContext<TenantInfo, string>));

        Assert.NotNull(service);
        Assert.Equal(ServiceLifetime.Singleton, service.Lifetime);
    }

    [Fact]
    public void RegisterTenantScopeProviderAndContextAliasesAsSameSingleton()
    {
        var services = new ServiceCollection();
        services.AddMultiTenant<TenantInfo, string>();
        using var provider = services.BuildServiceProvider();

        var ambient = provider.GetRequiredService<AmbientTenantContext<TenantInfo, string>>();
        Assert.Same(ambient, provider.GetRequiredService<ITenantScopeProvider>());
        Assert.Same(ambient, provider.GetRequiredService<ITenantContext<TenantInfo, string>>());
        Assert.Same(ambient, provider.GetRequiredService<ITenantContext<string>>());
        Assert.NotNull(provider.GetService<ITenantContext<string>>());
    }

    [Fact]
    public void SingletonConsumerObservesCurrentAmbientTenant()
    {
        var services = new ServiceCollection();
        services.AddMultiTenant<TenantInfo, string>();
        services.AddSingleton<TenantReader>();
        using var provider = services.BuildServiceProvider();
        var reader = provider.GetRequiredService<TenantReader>();

        var first = new TenantInfo { Id = "first", Identifier = "first" };
        provider.BeginTenantScope(first);
        Assert.Same(first, reader.TenantInfo);

        var second = new TenantInfo { Id = "second", Identifier = "second" };
        provider.BeginTenantScope(second);
        Assert.Same(second, reader.TenantInfo);
    }

    [Fact]
    public void RegisterMultiTenantOptionsInDi()
    {
        var services = new ServiceCollection();
        services.AddMultiTenant<TenantInfo, string>();

        var service = services.FirstOrDefault(s => s.Lifetime == ServiceLifetime.Singleton &&
                                                   s.ServiceType ==
                                                   typeof(IConfigureOptions<MultiTenantOptions<TenantInfo, string>>));

        Assert.NotNull(service);
        Assert.Equal(ServiceLifetime.Singleton, service.Lifetime);
    }

    public class TestOptions
    {
        public string? Prop1 { get; set; }
    }

    private sealed class TenantReader(ITenantContext<TenantInfo, string> context)
    {
        public TenantInfo? TenantInfo => context.TenantInfo;
    }

    [Fact]
    public void RegisterNamedOptionsPerTenant()
    {
        var services = new ServiceCollection();
        services.AddMultiTenant<TenantInfo, string>();
        services.ConfigurePerTenant<TestOptions, TenantInfo,string>("name1",
            (option, tenant) => option.Prop1 = tenant.Id);
        var sp = services.BuildServiceProvider();

        var configs = sp.GetRequiredService<IEnumerable<IConfigureOptions<TestOptions>>>();
        var config = configs.Where(config =>
            config is ConfigureNamedOptions<TestOptions, ITenantContext<TenantInfo, string>> options).ToList();

        Assert.Single(config);
        Assert.Equal("name1",
            config.Select(c => (ConfigureNamedOptions<TestOptions, ITenantContext<TenantInfo, string>>)c).Single()
                .Name);
    }

    [Fact]
    public void RegisterUnnamedOptionsPerTenant()
    {
        var services = new ServiceCollection();
        services.AddMultiTenant<TenantInfo, string>();
        services.ConfigurePerTenant<TestOptions, TenantInfo, string>((option, tenant) => option.Prop1 = tenant.Id);
        var sp = services.BuildServiceProvider();

        var configs = sp.GetRequiredService<IEnumerable<IConfigureOptions<TestOptions>>>();
        var config = configs.Where(config =>
            config is ConfigureNamedOptions<TestOptions, ITenantContext<TenantInfo, string>> options).ToList();

        Assert.Single(config);
        Assert.Equal(Microsoft.Extensions.Options.Options.DefaultName,
            config.Select(c => (ConfigureNamedOptions<TestOptions, ITenantContext<TenantInfo, string>>)c).Single()
                .Name);
    }

    [Fact]
    public void RegisterAllOptionsPerTenant()
    {
        var services = new ServiceCollection();
        services.AddMultiTenant<TenantInfo, string>();
        services.ConfigureAllPerTenant<TestOptions, TenantInfo, string>((option, tenant) => option.Prop1 = tenant.Id);
        var sp = services.BuildServiceProvider();

        var configs = sp.GetRequiredService<IEnumerable<IConfigureOptions<TestOptions>>>();
        var config = configs.Where(config =>
            config is ConfigureNamedOptions<TestOptions, ITenantContext<TenantInfo, string>> options).ToList();

        Assert.Single(config);
        Assert.Null(config.Select(c => (ConfigureNamedOptions<TestOptions, ITenantContext<TenantInfo, string>>)c)
            .Single().Name);
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

    [Fact]
    public void DecorateService_PassesParametersAndResolvesDecoratorDependencies()
    {
        var services = new ServiceCollection();
        services.AddSingleton<DecoratorDependency>();
        services.AddSingleton<ITestService, TestService>();
        var parameters = new object[] { "original" };

        services.DecorateService<ITestService, ParameterizedDecoratedTestService>(parameters);
        parameters[0] = "mutated";

        using var provider = services.BuildServiceProvider();
        var service = Assert.IsType<ParameterizedDecoratedTestService>(
            provider.GetRequiredService<ITestService>());

        Assert.Equal("original", service.Label);
        Assert.IsType<TestService>(service.Inner);
        Assert.NotNull(service.Dependency);
    }

    [Theory]
    [InlineData(ServiceLifetime.Singleton)]
    [InlineData(ServiceLifetime.Scoped)]
    [InlineData(ServiceLifetime.Transient)]
    public void DecorateService_PreservesLifetimeAndRuntimeSemantics(ServiceLifetime lifetime)
    {
        var services = new ServiceCollection();
        ((IServiceCollection)services).Add(new ServiceDescriptor(typeof(ITestService), typeof(TestService), lifetime));

        services.DecorateService<ITestService, DecoratedTestService>();

        Assert.Equal(lifetime, services.Single(s => s.ServiceType == typeof(ITestService)).Lifetime);

        using var provider = services.BuildServiceProvider();
        using var firstScope = provider.CreateScope();
        using var secondScope = provider.CreateScope();
        var first = firstScope.ServiceProvider.GetRequiredService<ITestService>();
        var firstAgain = firstScope.ServiceProvider.GetRequiredService<ITestService>();
        var second = secondScope.ServiceProvider.GetRequiredService<ITestService>();

        if (lifetime == ServiceLifetime.Singleton)
        {
            Assert.Same(first, firstAgain);
            Assert.Same(first, second);
        }
        else if (lifetime == ServiceLifetime.Scoped)
        {
            Assert.Same(first, firstAgain);
            Assert.NotSame(first, second);
        }
        else
        {
            Assert.NotSame(first, firstAgain);
            Assert.NotSame(first, second);
        }
    }

    [Fact]
    public void DecorateService_PassesTheActiveProviderToTheOriginalFactory()
    {
        var services = new ServiceCollection();
        services.AddScoped<FactoryDependency>();
        services.AddScoped<ITestService>(sp =>
            new FactoryTestService(sp.GetRequiredService<FactoryDependency>()));
        services.DecorateService<ITestService, DecoratedTestService>();

        using var provider = services.BuildServiceProvider();
        using var scope = provider.CreateScope();
        var service = Assert.IsType<DecoratedTestService>(
            scope.ServiceProvider.GetRequiredService<ITestService>());

        var inner = Assert.IsType<FactoryTestService>(service.Inner);
        Assert.NotNull(inner.Dependency);
    }

    [Fact]
    public void DecorateService_PreservesRegistrationOrderAndDefaultResolution()
    {
        var services = new ServiceCollection();
        var first = new TestService();
        var second = new TestService();
        services.AddSingleton<IOtherService, FirstOtherService>();
        services.AddSingleton<ITestService>(first);
        services.AddSingleton<IOtherService, SecondOtherService>();
        services.AddSingleton<ITestService>(second);
        var serviceTypesBefore = services.Select(s => s.ServiceType).ToArray();

        services.DecorateService<ITestService, DecoratedTestService>();

        Assert.Equal(serviceTypesBefore, services.Select(s => s.ServiceType).ToArray());
        using var provider = services.BuildServiceProvider();
        var service = Assert.IsType<DecoratedTestService>(provider.GetRequiredService<ITestService>());
        Assert.Same(second, service.Inner);
        Assert.Equal(2, provider.GetServices<IOtherService>().Count());
    }

    [Fact]
    public void DecorateService_StacksDecoratorsWhenCalledRepeatedly()
    {
        var services = new ServiceCollection();
        services.AddSingleton<ITestService, TestService>();

        services.DecorateService<ITestService, DecoratedTestService>();
        services.DecorateService<ITestService, SecondDecoratedTestService>();

        using var provider = services.BuildServiceProvider();
        var outer = Assert.IsType<SecondDecoratedTestService>(provider.GetRequiredService<ITestService>());
        var inner = Assert.IsType<DecoratedTestService>(outer.Inner);
        Assert.IsType<TestService>(inner.Inner);
    }

    [Fact]
    public void DecorateService_LeavesKeyedRegistrationsUntouched()
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

    [Fact]
    public void DecorateService_ThrowsWhenOnlyKeyedRegistrationExists()
    {
        var services = new ServiceCollection();
        services.AddKeyedSingleton<ITestService, TestService>("key");

        var exception = Assert.Throws<ArgumentException>(() =>
            services.DecorateService<ITestService, DecoratedTestService>());

        Assert.Contains(nameof(ITestService), exception.Message);
    }

    [Fact]
    public void DecorateService_ThrowsForNullServiceCollection()
    {
        IServiceCollection? services = null;

        Assert.Throws<ArgumentNullException>(() =>
            ServiceCollectionExtensions.DecorateService<ITestService, DecoratedTestService>(services!));
    }

    [Fact]
    public void DecorateService_ThrowsForNullParameters()
    {
        var services = new ServiceCollection();
        services.AddSingleton<ITestService, TestService>();

        Assert.Throws<ArgumentNullException>(() =>
            ServiceCollectionExtensions.DecorateService<ITestService, DecoratedTestService>(
                services, (object[])null!));
    }

    [Fact]
    public void DecorateService_ThrowsForIncompatibleDecoratorTypeBeforeMutation()
    {
        var services = new ServiceCollection();
        services.AddSingleton<ITestService, TestService>();
        var serviceCount = services.Count;

        var exception = Assert.Throws<ArgumentException>(() =>
            services.DecorateService<ITestService, IncompatibleDecorator>());

        Assert.Contains(nameof(IncompatibleDecorator), exception.Message);
        Assert.Equal(serviceCount, services.Count);
        Assert.Equal(typeof(TestService), services.Single().ImplementationType);
    }

    [Fact]
    public void DecorateService_ThrowsWhenFactoryReturnsNull()
    {
        var services = new ServiceCollection();
        services.AddSingleton<ITestService>(_ => null!);
        services.DecorateService<ITestService, DecoratedTestService>();
        using var provider = services.BuildServiceProvider();

        var exception = Assert.Throws<ArgumentException>(() =>
            provider.GetRequiredService<ITestService>());

        Assert.Contains("Unable to instantiate decorated service", exception.Message);
    }

    [Fact]
    public void DecorateService_PropagatesDecoratorActivationFailure()
    {
        var services = new ServiceCollection();
        services.AddSingleton<ITestService, TestService>();
        services.DecorateService<ITestService, UnconstructableDecoratedTestService>();
        using var provider = services.BuildServiceProvider();

        Assert.Throws<InvalidOperationException>(() => provider.GetRequiredService<ITestService>());
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

    public sealed class ParameterizedDecoratedTestService : ITestService
    {
        public ITestService Inner { get; }
        public string Label { get; }
        public DecoratorDependency Dependency { get; }

        public ParameterizedDecoratedTestService(ITestService inner, string label, DecoratorDependency dependency)
        {
            Inner = inner;
            Label = label;
            Dependency = dependency;
        }
    }

    public sealed class SecondDecoratedTestService : ITestService
    {
        public ITestService Inner { get; }
        public SecondDecoratedTestService(ITestService inner) => Inner = inner;
    }

    public sealed class FactoryTestService : ITestService
    {
        public FactoryDependency Dependency { get; }
        public FactoryTestService(FactoryDependency dependency) => Dependency = dependency;
    }

    public sealed class DecoratorDependency
    {
    }

    public sealed class FactoryDependency
    {
    }

    public interface IOtherService
    {
    }

    public sealed class FirstOtherService : IOtherService
    {
    }

    public sealed class SecondOtherService : IOtherService
    {
    }

    public sealed class IncompatibleDecorator
    {
        public IncompatibleDecorator(ITestService inner)
        {
        }
    }

    public sealed class UnconstructableDecoratedTestService : ITestService
    {
        public UnconstructableDecoratedTestService(ITestService inner, MissingDecoratorDependency dependency)
        {
        }
    }

    public sealed class MissingDecoratorDependency
    {
    }
}
