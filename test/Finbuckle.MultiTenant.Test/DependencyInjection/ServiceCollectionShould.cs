// Copyright Finbuckle LLC, Andrew White, and Co ntributors.
// Refer to the solution LICENSE file for more information.

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Xunit;

namespace Finbuckle.MultiTenant.Test.DependencyInjection
{
    public class ServiceCollectionShould
    {
        // Used in some tests.
        // ReSharper disable once UnusedAutoPropertyAccessor.Local
        private string? TestProperty { get; set; }
        
        [Fact]
        public void AddPerTenantOptions()
        {
            var services = new ServiceCollection();
            _ = services.AddMultiTenant<TenantInfo>();

            services.AddPerTenantOptions<ServiceCollectionShould>()
                .Configure<TenantInfo>((o, ti) => o.TestProperty = ti.Id);
            
            var sp = services.BuildServiceProvider();
            var multiTenantContextAccessor = sp.GetRequiredService<IMultiTenantContextAccessor<TenantInfo>>();
            multiTenantContextAccessor.MultiTenantContext = new MultiTenantContext<TenantInfo>{ TenantInfo = new TenantInfo { Id = "initech" } };

            using var scope = sp.CreateScope();
            var options = scope.ServiceProvider.GetRequiredService<IOptions<ServiceCollectionShould>>();

            Assert.Equal("initech", options.Value.TestProperty);
        }
    }
}