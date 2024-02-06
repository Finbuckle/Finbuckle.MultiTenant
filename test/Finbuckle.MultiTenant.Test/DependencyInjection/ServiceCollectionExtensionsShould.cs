// Copyright Finbuckle LLC, Andrew White, and Contributors.
// Refer to the solution LICENSE file for more information.

using System.Linq;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Xunit;

namespace Finbuckle.MultiTenant.Test.DependencyInjection
{
    public class ServiceCollectionExtensionsShould
    {
        [Fact]
        public void RegisterITenantResolverInDi()
        {
            var services = new ServiceCollection();
            services.AddMultiTenant<TenantInfo>();

            var service = services.SingleOrDefault(s => s.ServiceType == typeof(ITenantResolver));

            Assert.NotNull(service);
            Assert.Equal(ServiceLifetime.Scoped, service!.Lifetime);
        }

        [Fact]
        public void RegisterITenantResolverGenericInDi()
        {
            var services = new ServiceCollection();
            services.AddMultiTenant<TenantInfo>();

            var service = services.SingleOrDefault(s => s.ServiceType == typeof(ITenantResolver<TenantInfo>));

            Assert.NotNull(service);
            Assert.Equal(ServiceLifetime.Scoped, service!.Lifetime);
        }

        [Fact]
        public void RegisterIMultiTenantContextInDi()
        {
            var services = new ServiceCollection();
            services.AddMultiTenant<TenantInfo>();

            var service = services.SingleOrDefault(s => s.Lifetime == ServiceLifetime.Scoped &&
                                                        s.ServiceType == typeof(IMultiTenantContext<TenantInfo>));

            Assert.NotNull(service);
            Assert.Equal(ServiceLifetime.Scoped, service!.Lifetime);
        }

        [Fact]
        public void RegisterTenantInfoInDi()
        {
            var services = new ServiceCollection();
            services.AddMultiTenant<TenantInfo>();

            var service = services.SingleOrDefault(s => s.Lifetime == ServiceLifetime.Scoped &&
                                                        s.ServiceType == typeof(TenantInfo));

            Assert.NotNull(service);
            Assert.Equal(ServiceLifetime.Scoped, service!.Lifetime);
        }

        [Fact]
        public void RegisterITenantInfoInDi()
        {
            var services = new ServiceCollection();
            services.AddMultiTenant<TenantInfo>();

            var service = services.SingleOrDefault(s => s.Lifetime == ServiceLifetime.Scoped &&
                                                        s.ServiceType == typeof(ITenantInfo));

            Assert.NotNull(service);
            Assert.Equal(ServiceLifetime.Scoped, service!.Lifetime);
        }

        [Fact]
        public void RegisterIMultiTenantContextAccessorInDi()
        {
            var services = new ServiceCollection();
            services.AddMultiTenant<TenantInfo>();

            var service = services.SingleOrDefault(s => s.Lifetime == ServiceLifetime.Singleton &&
                                                        s.ServiceType == typeof(IMultiTenantContextAccessor));

            Assert.NotNull(service);
            Assert.Equal(ServiceLifetime.Singleton, service!.Lifetime);
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
            Assert.Equal(ServiceLifetime.Singleton, service!.Lifetime);
        }

        [Fact]
        public void RegisterMultiTenantOptionsInDi()
        {
            var services = new ServiceCollection();
            services.AddMultiTenant<TenantInfo>();

            var service = services.SingleOrDefault(s => s.Lifetime == ServiceLifetime.Singleton &&
                                                        s.ServiceType == typeof(IConfigureOptions<MultiTenantOptions>));

            Assert.NotNull(service);
            Assert.Equal(ServiceLifetime.Singleton, service!.Lifetime);
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
    }
}