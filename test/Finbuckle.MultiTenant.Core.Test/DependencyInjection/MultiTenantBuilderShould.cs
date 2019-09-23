//    Copyright 2018 Andrew White
// 
//    Licensed under the Apache License, Version 2.0 (the "License");
//    you may not use this file except in compliance with the License.
//    You may obtain a copy of the License at
// 
//        http://www.apache.org/licenses/LICENSE-2.0
// 
//    Unless required by applicable law or agreed to in writing, software
//    distributed under the License is distributed on an "AS IS" BASIS,
//    WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//    See the License for the specific language governing permissions and
//    limitations under the License.

using System;
using Microsoft.Extensions.DependencyInjection;
using Xunit;
using Microsoft.Extensions.Options;
using Finbuckle.MultiTenant;
using Finbuckle.MultiTenant.Stores;
using Finbuckle.MultiTenant.Strategies;
using System.Threading.Tasks;
using Moq;

internal class NullStrategy : IMultiTenantStrategy
{
    public Task<string> GetIdentifierAsync(object context)
    {
        throw new NotImplementedException();
    }
}

public class MultiTenantBuilderShould
{
    // Used in some tests.
    public int TestProperty { get; set; }

    [Theory]
    [InlineData(ServiceLifetime.Singleton)]
    [InlineData(ServiceLifetime.Scoped)]
    [InlineData(ServiceLifetime.Transient)]
    public void AddCustomStoreWithDefaultCtorAndLifetime(ServiceLifetime lifetime)
    {
        var services = new ServiceCollection();
        var builder = new FinbuckleMultiTenantBuilder(services);
        builder.WithStore<InMemoryStore>(lifetime);

        var sp = services.BuildServiceProvider();

        var store = sp.GetRequiredService<IMultiTenantStore>();
        var scope = sp.CreateScope();
        var store2 = scope.ServiceProvider.GetRequiredService<IMultiTenantStore>();

        switch (lifetime)
        {
            case ServiceLifetime.Singleton:
                Assert.Same(store, store2);
                break;

            case ServiceLifetime.Scoped:
                Assert.NotSame(store, store2);
                break;

            case ServiceLifetime.Transient:
                Assert.NotSame(store, store2);
                store = scope.ServiceProvider.GetRequiredService<IMultiTenantStore>();
                Assert.NotSame(store, store2);
                break;
        }
    }

    [Theory]
    [InlineData(ServiceLifetime.Singleton)]
    [InlineData(ServiceLifetime.Scoped)]
    [InlineData(ServiceLifetime.Transient)]
    public void AddCustomStoreWithParamsAndLifetime(ServiceLifetime lifetime)
    {
        var services = new ServiceCollection();
        var builder = new FinbuckleMultiTenantBuilder(services);
        builder.WithStore<InMemoryStore>(lifetime, true);

        var sp = services.BuildServiceProvider();

        var store = sp.GetRequiredService<IMultiTenantStore>();
        var scope = sp.CreateScope();
        var store2 = scope.ServiceProvider.GetRequiredService<IMultiTenantStore>();

        switch (lifetime)
        {
            case ServiceLifetime.Singleton:
                Assert.Same(store, store2);
                break;

            case ServiceLifetime.Scoped:
                Assert.NotSame(store, store2);
                break;

            case ServiceLifetime.Transient:
                Assert.NotSame(store, store2);
                store = scope.ServiceProvider.GetRequiredService<IMultiTenantStore>();
                Assert.NotSame(store, store2);
                break;
        }
    }

    [Theory]
    [InlineData(ServiceLifetime.Singleton)]
    [InlineData(ServiceLifetime.Scoped)]
    [InlineData(ServiceLifetime.Transient)]
    public void AddCustomStoreWithFactoryAndLifetime(ServiceLifetime lifetime)
    {
        var services = new ServiceCollection();
        var builder = new FinbuckleMultiTenantBuilder(services);
        builder.WithStore(lifetime, _sp => new InMemoryStore());

        var sp = services.BuildServiceProvider();

        var store = sp.GetRequiredService<IMultiTenantStore>();
        var scope = sp.CreateScope();
        var store2 = scope.ServiceProvider.GetRequiredService<IMultiTenantStore>();

        switch (lifetime)
        {
            case ServiceLifetime.Singleton:
                Assert.Same(store, store2);
                break;

            case ServiceLifetime.Scoped:
                Assert.NotSame(store, store2);
                break;

            case ServiceLifetime.Transient:
                Assert.NotSame(store, store2);
                store = scope.ServiceProvider.GetRequiredService<IMultiTenantStore>();
                Assert.NotSame(store, store2);
                break;
        }
    }

    [Fact]
    public void ThrowIfNullFactoryAddingCustomStore()
    {
        var services = new ServiceCollection();
        var builder = new FinbuckleMultiTenantBuilder(services);
        Assert.Throws<ArgumentNullException>(() => builder.WithStore<InMemoryStore>(ServiceLifetime.Singleton, factory: null));
    }

    [Fact]
    public void AddPerTenantOptions()
    {
        var services = new ServiceCollection();
        var accessor = new Mock<IMultiTenantContextAccessor>();
        accessor.Setup(a => a.MultiTenantContext).Returns((MultiTenantContext)null);
        services.AddSingleton<IMultiTenantContextAccessor>(accessor.Object);
        var builder = new FinbuckleMultiTenantBuilder(services);
        // Note: using MultiTenantBuilderShould as our test options class.
        builder.WithPerTenantOptions<MultiTenantBuilderShould>((o, tc) => o.TestProperty = 1);
        var sp = services.BuildServiceProvider();

        var cache = sp.GetRequiredService<IOptionsMonitorCache<MultiTenantBuilderShould>>();
    }

    [Fact]
    public void ThrowIfNullParamAddingPerTenantOptions()
    {
        var services = new ServiceCollection();
        var builder = new FinbuckleMultiTenantBuilder(services);
        Assert.Throws<ArgumentNullException>(() => builder.WithPerTenantOptions<MultiTenantBuilderShould>(null));
    }

    [Theory]
    [InlineData(ServiceLifetime.Singleton)]
    [InlineData(ServiceLifetime.Scoped)]
    [InlineData(ServiceLifetime.Transient)]
    public void AddCustomStrategyWithDefaultCtorAndLifetime(ServiceLifetime lifetime)
    {
        var services = new ServiceCollection();
        var builder = new FinbuckleMultiTenantBuilder(services);
        builder.WithStrategy<NullStrategy>(lifetime);

        var sp = services.BuildServiceProvider();

        var strategy = sp.GetRequiredService<IMultiTenantStrategy>();
        var scope = sp.CreateScope();
        var strategy2 = scope.ServiceProvider.GetRequiredService<IMultiTenantStrategy>();

        switch (lifetime)
        {
            case ServiceLifetime.Singleton:
                Assert.Same(strategy, strategy2);
                break;

            case ServiceLifetime.Scoped:
                Assert.NotSame(strategy, strategy2);
                break;

            case ServiceLifetime.Transient:
                Assert.NotSame(strategy, strategy2);
                strategy = scope.ServiceProvider.GetRequiredService<IMultiTenantStrategy>();
                Assert.NotSame(strategy, strategy2);
                break;
        }
    }

    [Theory]
    [InlineData(ServiceLifetime.Singleton)]
    [InlineData(ServiceLifetime.Scoped)]
    [InlineData(ServiceLifetime.Transient)]
    public void AddCustomStrategyWithParamsAndLifetime(ServiceLifetime lifetime)
    {
        var services = new ServiceCollection();
        var builder = new FinbuckleMultiTenantBuilder(services);
        builder.WithStrategy<StaticStrategy>(lifetime, new object[] { "id" });

        var sp = services.BuildServiceProvider();

        var strategy = sp.GetRequiredService<IMultiTenantStrategy>();
        var scope = sp.CreateScope();
        var strategy2 = scope.ServiceProvider.GetRequiredService<IMultiTenantStrategy>();

        switch (lifetime)
        {
            case ServiceLifetime.Singleton:
                Assert.Same(strategy, strategy2);
                break;

            case ServiceLifetime.Scoped:
                Assert.NotSame(strategy, strategy2);
                break;

            case ServiceLifetime.Transient:
                Assert.NotSame(strategy, strategy2);
                strategy = scope.ServiceProvider.GetRequiredService<IMultiTenantStrategy>();
                Assert.NotSame(strategy, strategy2);
                break;
        }
    }

    [Theory]
    [InlineData(ServiceLifetime.Singleton)]
    [InlineData(ServiceLifetime.Scoped)]
    [InlineData(ServiceLifetime.Transient)]
    public void AddCustomStrategyWithFactoryAndLifetime(ServiceLifetime lifetime)
    {
        var services = new ServiceCollection();
        var builder = new FinbuckleMultiTenantBuilder(services);
        builder.WithStrategy(lifetime, _sp => new StaticStrategy("id"));

        var sp = services.BuildServiceProvider();

        var strategy = sp.GetRequiredService<IMultiTenantStrategy>();
        var scope = sp.CreateScope();
        var strategy2 = scope.ServiceProvider.GetRequiredService<IMultiTenantStrategy>();

        switch (lifetime)
        {
            case ServiceLifetime.Singleton:
                Assert.Same(strategy, strategy2);
                break;

            case ServiceLifetime.Scoped:
                Assert.NotSame(strategy, strategy2);
                break;

            case ServiceLifetime.Transient:
                Assert.NotSame(strategy, strategy2);
                strategy = scope.ServiceProvider.GetRequiredService<IMultiTenantStrategy>();
                Assert.NotSame(strategy, strategy2);
                break;
        }
    }

    [Fact]
    public void ThrowIfNullFactoryAddingCustomStrategy()
    {
        var services = new ServiceCollection();
        var builder = new FinbuckleMultiTenantBuilder(services);
        Assert.Throws<ArgumentNullException>(() => builder.WithStrategy<StaticStrategy>(ServiceLifetime.Singleton, factory: null));
    }
}