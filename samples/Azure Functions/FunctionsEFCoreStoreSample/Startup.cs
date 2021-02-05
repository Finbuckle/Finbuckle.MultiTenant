using System;
using System.Collections.Generic;
using System.Text;

using Finbuckle.MultiTenant;

using FunctionsEFCoreStoreSample.Data;

using Microsoft.AspNetCore.Http;
using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;

[assembly: FunctionsStartup(typeof(FunctionsBasePathStrategySample.Startup))]
namespace FunctionsBasePathStrategySample
{
    public class Startup : FunctionsStartup
    {
        public override void Configure(IFunctionsHostBuilder builder)
        {
            builder.Services.AddMultiTenant<TenantInfo>()
                .WithEFCoreStore<MultiTenantStoreDbContext, TenantInfo>()
                .WithBasePathStrategy(routePrefix: PathString.FromUriComponent("/api"));

            builder.UseMultiTenant();

            throw new NotImplementedException("Still need to finish the Seeding function. <see cref=\"Startup\"/>");
            // SetupStore( ??? ); // Not sure if I can resolve the service at this point (I know docs say you shouldn't but for testing).
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
