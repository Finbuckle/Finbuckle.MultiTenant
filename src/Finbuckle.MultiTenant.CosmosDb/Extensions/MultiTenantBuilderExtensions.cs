using Finbuckle.MultiTenant;
using Finbuckle.MultiTenant.CosmosDb.Stores;

using Microsoft.Azure.Cosmos;
using Microsoft.Azure.Cosmos.Fluent;

using System;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class FinbuckleMultiTenantBuilderExtensions
    {
        /// <summary>
        /// Adds a CosmosDb based multi-tenant store to the application. Will also create the database context service unless it's already exists.
        /// </summary>
        /// <typeparam name="TDatabaseContext"></typeparam>
        /// <typeparam name="TTenantInfo"></typeparam>
        /// <param name="builder"></param>
        /// <param name="CosmosClientBuilder"><see cref="CosmosClient"/> Builder</param>
        /// <param name="DatabaseContextBuilder"><see cref="DatabaseContext"/> Builder</param>
        /// <param name="TenantContainerSelector">Get the <see cref="ITenantInfo"/> <see cref="Container"/>.</param>
        /// <returns>The same <see cref="FinbuckleMultiTenantBuilder{TTenantInfo}"/> passed into the method.</returns>
        public static FinbuckleMultiTenantBuilder<TTenantInfo> WithCosmosDbStore<TDatabaseContext, TTenantInfo>(this FinbuckleMultiTenantBuilder<TTenantInfo> builder,
            Func<IServiceProvider, CosmosClient> CosmosClientBuilder,
            Func<CosmosClient, IServiceProvider, TDatabaseContext> DatabaseContextBuilder,
            Func<TDatabaseContext, Container> tenantContainerSelector)
            where TTenantInfo : class, ITenantInfo, new()
            where TDatabaseContext : DatabaseContext
        {
            builder.Services.AddSingleton(services => CosmosClientBuilder(services));

            builder.Services.AddSingleton(services =>
            {
                var cosmosClient = services.GetRequiredService<CosmosClient>();
                var dbContext = DatabaseContextBuilder(cosmosClient, services);
                
                // Can't think of a good way around .GetAwaiter().GetResult()
                dbContext.InitializeAsync().GetAwaiter().GetResult();

                return dbContext;
            });

            builder.Services.AddSingleton(services =>
            {
                var dbContext = services.GetRequiredService<TDatabaseContext>();
                var container = tenantContainerSelector(dbContext);
                return new CosmosDbStoreContext(container);
            });

            return builder.WithStore<CosmosDbStore<TTenantInfo>>(ServiceLifetime.Scoped);
        }
    }
}