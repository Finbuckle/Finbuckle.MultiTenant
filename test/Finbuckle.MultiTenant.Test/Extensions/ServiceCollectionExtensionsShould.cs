// Copyright Finbuckle LLC, Andrew White, and Contributors.
// Refer to the solution LICENSE file for more inforation.

using System.Linq;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Xunit;

namespace Finbuckle.MultiTenant.Test.Extensions
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
                                                        s.ServiceType == typeof(IMultiTenantContextAccessor<TenantInfo>));

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
    }
}