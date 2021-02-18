using Finbuckle.MultiTenant;

using FunctionsEFCoreStoreSample.Data;

using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;

using System;

[assembly: FunctionsStartup(typeof(FunctionsBasePathStrategySample.Startup))]
namespace FunctionsBasePathStrategySample
{
    public class Startup : FunctionsStartup
    {
        public override void Configure(IFunctionsHostBuilder builder)
        {
            builder.Services.AddMultiTenant<TenantInfo>()
                .WithEFCoreStore<MultiTenantStoreDbContext, TenantInfo>()
                .WithBasePathStrategy();

            SetupStore(builder.Services.BuildServiceProvider());
        }

        private void SetupStore(IServiceProvider sp)
        {

            var scopeServices = sp.CreateScope().ServiceProvider;
            var store = scopeServices.GetRequiredService<IMultiTenantStore<TenantInfo>>();

            store.TryAddAsync(new TenantInfo { Id = "tenant-finbuckle-d043favoiaw", Identifier = "finbuckle", Name = "Finbuckle", ConnectionString = "finbuckle_conn_string" }).Wait();
            store.TryAddAsync(new TenantInfo { Id = "tenant-initech-341ojadsfa", Identifier = "initech", Name = "Initech LLC", ConnectionString = "initech_conn_string" }).Wait();
        }
    }
}
