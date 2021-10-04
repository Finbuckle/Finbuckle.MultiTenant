// Copyright Finbuckle LLC, Andrew White, and Contributors.
// Refer to the solution LICENSE file for more inforation.

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Finbuckle.MultiTenant.Strategies;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Moq;
using Xunit;

namespace Finbuckle.MultiTenant.Test.DependencyInjection
{
    public class MultiTenantBuilderShould
    {
        // Used in some tests.
        // ReSharper disable once UnusedAutoPropertyAccessor.Local
        private int TestProperty { get; set; }

        [Theory]
        [InlineData(ServiceLifetime.Singleton)]
        [InlineData(ServiceLifetime.Scoped)]
        [InlineData(ServiceLifetime.Transient)]
        public void AddCustomStoreWithDefaultCtorAndLifetime(ServiceLifetime lifetime)
        {
            var services = new ServiceCollection();
            var builder = new FinbuckleMultiTenantBuilder<TenantInfo>(services);
            builder.WithStore<TestStore<TenantInfo>>(lifetime);

            var sp = services.BuildServiceProvider();

            var store = sp.GetRequiredService<IMultiTenantStore<TenantInfo>>();
            var scope = sp.CreateScope();
            var store2 = scope.ServiceProvider.GetRequiredService<IMultiTenantStore<TenantInfo>>();

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
                    store = scope.ServiceProvider.GetRequiredService<IMultiTenantStore<TenantInfo>>();
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
            var builder = new FinbuckleMultiTenantBuilder<TenantInfo>(services);
            builder.WithStore<TestStore<TenantInfo>>(lifetime, true);

            var sp = services.BuildServiceProvider();

            var store = sp.GetRequiredService<IMultiTenantStore<TenantInfo>>();
            var scope = sp.CreateScope();
            var store2 = scope.ServiceProvider.GetRequiredService<IMultiTenantStore<TenantInfo>>();

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
                    store = scope.ServiceProvider.GetRequiredService<IMultiTenantStore<TenantInfo>>();
                    Assert.NotSame(store, store2);
                    break;
                
                default:
                    throw new ArgumentOutOfRangeException(nameof(lifetime), lifetime, null);
            }
        }

        [Theory]
        [InlineData(ServiceLifetime.Singleton)]
        [InlineData(ServiceLifetime.Scoped)]
        [InlineData(ServiceLifetime.Transient)]
        public void AddCustomStoreWithFactoryAndLifetime(ServiceLifetime lifetime)
        {
            var services = new ServiceCollection();
            var builder = new FinbuckleMultiTenantBuilder<TenantInfo>(services);
            builder.WithStore(lifetime, _ => new TestStore<TenantInfo>());

            var sp = services.BuildServiceProvider();

            var store = sp.GetRequiredService<IMultiTenantStore<TenantInfo>>();
            var scope = sp.CreateScope();
            var store2 = scope.ServiceProvider.GetRequiredService<IMultiTenantStore<TenantInfo>>();

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
                    store = scope.ServiceProvider.GetRequiredService<IMultiTenantStore<TenantInfo>>();
                    Assert.NotSame(store, store2);
                    break;
            }
        }

        [Fact]
        public void ThrowIfNullFactoryAddingCustomStore()
        {
            var services = new ServiceCollection();
            var builder = new FinbuckleMultiTenantBuilder<TenantInfo>(services);
            Assert.Throws<ArgumentNullException>(() => builder.WithStore<TestStore<TenantInfo>>(ServiceLifetime.Singleton, factory: null));
        }

        [Fact]
        public void AddPerTenantOptions()
        {
            var services = new ServiceCollection();
            var accessor = new Mock<IMultiTenantContextAccessor<TenantInfo>>();
            accessor.Setup(a => a.MultiTenantContext).Returns((IMultiTenantContext<TenantInfo>)null);
            services.AddSingleton(accessor.Object);
            var builder = new FinbuckleMultiTenantBuilder<TenantInfo>(services);
            // Note: using MultiTenantBuilderShould as our test options class.
            builder.WithPerTenantOptions<MultiTenantBuilderShould>((o, _) => o.TestProperty = 1);
            var sp = services.BuildServiceProvider();

            sp.GetRequiredService<IOptionsMonitorCache<MultiTenantBuilderShould>>();
        }

        [Fact]
        public void ThrowIfNullParamAddingPerTenantOptions()
        {
            var services = new ServiceCollection();
            var builder = new FinbuckleMultiTenantBuilder<TenantInfo>(services);
            Assert.Throws<ArgumentNullException>(() => builder.WithPerTenantOptions<MultiTenantBuilderShould>(null));
        }

        [Theory]
        [InlineData(ServiceLifetime.Singleton)]
        [InlineData(ServiceLifetime.Scoped)]
        [InlineData(ServiceLifetime.Transient)]
        public void AddCustomStrategyWithDefaultCtorAndLifetime(ServiceLifetime lifetime)
        {
            var services = new ServiceCollection();
            var builder = new FinbuckleMultiTenantBuilder<TenantInfo>(services);
            builder.WithStrategy<StaticStrategy>(lifetime, "initech");

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
            var builder = new FinbuckleMultiTenantBuilder<TenantInfo>(services);
            builder.WithStrategy<StaticStrategy>(lifetime, "id");

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
            var builder = new FinbuckleMultiTenantBuilder<TenantInfo>(services);
            builder.WithStrategy(lifetime, _ => new StaticStrategy("id"));

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
            var builder = new FinbuckleMultiTenantBuilder<TenantInfo>(services);
            Assert.Throws<ArgumentNullException>(() => builder.WithStrategy<StaticStrategy>(ServiceLifetime.Singleton, factory: null));
        }

        private class TestStore<TTenant> : IMultiTenantStore<TTenant>
            where TTenant : class, ITenantInfo, new()
        {
            // ReSharper disable once NotAccessedField.Local
            private readonly bool _testParam;

            public TestStore()
            {
            }

            // ReSharper disable once UnusedMember.Local
            // Needed to test param injection
            public TestStore(bool testParam)
            {
                this._testParam = testParam;
            }

            public Task<bool> TryAddAsync(TTenant tenantInfo)
            {
                throw new NotImplementedException();
            }

            public Task<TTenant> TryGetAsync(string id)
            {
                throw new NotImplementedException();
            }

            public Task<IEnumerable<TTenant>> GetAllAsync()
            {
                throw new NotImplementedException();
            }

            public Task<TTenant> TryGetByIdentifierAsync(string identifier)
            {
                throw new NotImplementedException();
            }

            public Task<bool> TryRemoveAsync(string id)
            {
                throw new NotImplementedException();
            }

            public Task<bool> TryUpdateAsync(TTenant tenantInfo)
            {
                throw new NotImplementedException();
            }
        }
    }
}