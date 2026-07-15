// Copyright Finbuckle LLC, Andrew White, and Contributors.
// Refer to the solution LICENSE file for more information.

using Finbuckle.MultiTenant.Abstractions;
using Finbuckle.MultiTenant.Strategies;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Finbuckle.MultiTenant.Test.Extensions;

public class MultiTenantBuilderShould
{
    // Used in some tests.
    // ReSharper disable once UnusedAutoPropertyAccessor.Local
    private string? TestProperty { get; set; }

    [Theory]
    [InlineData(ServiceLifetime.Singleton)]
    [InlineData(ServiceLifetime.Scoped)]
    [InlineData(ServiceLifetime.Transient)]
    public void AddCustomStoreWithDefaultCtorAndLifetime(ServiceLifetime lifetime)
    {
        var services = new ServiceCollection();
        var builder = new MultiTenantBuilder<TenantInfo>(services);
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
        var builder = new MultiTenantBuilder<TenantInfo>(services);
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
        var builder = new MultiTenantBuilder<TenantInfo>(services);
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
        var builder = new MultiTenantBuilder<TenantInfo>(services);
        Assert.Throws<ArgumentNullException>(() =>
            builder.WithStore<TestStore<TenantInfo>>(ServiceLifetime.Singleton, factory: null!));
    }

    [Fact]
    public void ThrowIfAddingMultiplePrimaryStores()
    {
        var services = new ServiceCollection();
        var builder = new MultiTenantBuilder<TenantInfo>(services);
        builder.WithStore<TestStore<TenantInfo>>(ServiceLifetime.Singleton);

        Assert.Throws<InvalidOperationException>(() =>
            builder.WithStore<TestStore<TenantInfo>>(ServiceLifetime.Singleton));
    }

    [Fact]
    public void AddMultipleStoreCachesInOrder()
    {
        var services = new ServiceCollection();
        var builder = new MultiTenantBuilder<TenantInfo>(services);
        builder.WithStoreCache<TestStoreCache<TenantInfo>>(ServiceLifetime.Singleton, "first");
        builder.WithStoreCache<TestStoreCache<TenantInfo>>(ServiceLifetime.Singleton, "second");

        var sp = services.BuildServiceProvider();
        var caches = sp.GetRequiredService<IEnumerable<IMultiTenantStoreCache<TenantInfo>>>()
            .Cast<TestStoreCache<TenantInfo>>()
            .ToArray();

        Assert.Equal(2, caches.Length);
        Assert.Equal("first", caches[0].Name);
        Assert.Equal("second", caches[1].Name);
    }

    [Theory]
    [InlineData(ServiceLifetime.Singleton)]
    [InlineData(ServiceLifetime.Scoped)]
    [InlineData(ServiceLifetime.Transient)]
    public void AddCustomStrategyWithDefaultCtorAndLifetime(ServiceLifetime lifetime)
    {
        var services = new ServiceCollection();
        var builder = new MultiTenantBuilder<TenantInfo>(services);
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
            default:
                throw new ArgumentOutOfRangeException(nameof(lifetime), lifetime, null);
        }
    }

    [Theory]
    [InlineData(ServiceLifetime.Singleton)]
    [InlineData(ServiceLifetime.Scoped)]
    [InlineData(ServiceLifetime.Transient)]
    public void AddCustomStrategyWithParamsAndLifetime(ServiceLifetime lifetime)
    {
        var services = new ServiceCollection();
        var builder = new MultiTenantBuilder<TenantInfo>(services);
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
        var builder = new MultiTenantBuilder<TenantInfo>(services);
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
        var builder = new MultiTenantBuilder<TenantInfo>(services);
        Assert.Throws<ArgumentNullException>(() =>
            builder.WithStrategy<StaticStrategy>(ServiceLifetime.Singleton, factory: null!));
    }

    private class TestStore<TTenant> : IMultiTenantStore<TTenant>
        where TTenant : TenantInfo
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
            _testParam = testParam;
        }

        public Task<bool> AddAsync(TTenant tenantInfo, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<TTenant?> GetAsync(string id, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<IEnumerable<TTenant>> GetAllAsync(CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<IEnumerable<TTenant>> GetAllAsync(int take, int skip, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<TTenant?> GetByIdentifierAsync(string identifier, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<bool> RemoveAsync(string id, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<bool> RemoveByIdentifierAsync(string identifier, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<bool> UpdateAsync(TTenant tenantInfo, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }
    }

    private class TestStoreCache<TTenant> : IMultiTenantStoreCache<TTenant>
        where TTenant : TenantInfo
    {
        public TestStoreCache(string name)
        {
            Name = name;
        }

        public string Name { get; }

        public Task<TTenant?> GetAsync(string id, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<TTenant?> GetByIdentifierAsync(string identifier, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task SetAsync(TTenant tenantInfo, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task RemoveAsync(string id, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task RemoveByIdentifierAsync(string identifier, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }
    }
}
