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
        /// <param name="connectionString">CosmosDb Connection String.</param>
        /// <param name="serializationOptions">Default CosmosDb Serialization options.</param>
        /// <param name="cosmosDbStoreContext">Function to select the container to use when looking up the <see cref="ITenantInfo"/>.</param>
        /// <returns>The same <see cref="FinbuckleMultiTenantBuilder{TTenantInfo}"/> passed into the method.</returns>
        public static FinbuckleMultiTenantBuilder<TTenantInfo> WithCosmosDbStore<TDatabaseContext, TTenantInfo>(this FinbuckleMultiTenantBuilder<TTenantInfo> builder,
            string connectionString,
            CosmosSerializationOptions serializationOptions,
            Func<TDatabaseContext, Container> cosmosDbStoreContext)
            where TTenantInfo : class, ITenantInfo, new()
            where TDatabaseContext : DatabaseContext
        {
            var cosmosClient = new CosmosClientBuilder(connectionString)
                .WithSerializerOptions(serializationOptions)
                .Build();

            builder.Services.AddSingleton(services =>
            {
                var dbContext = ActivatorUtilities.CreateInstance<TDatabaseContext>(services, cosmosClient);
                
                // Can't think of a good way around .GetAwaiter().GetResult()
                dbContext.InitializeAsync().GetAwaiter().GetResult();

                return dbContext;
            });

            builder.Services.AddSingleton(services =>
            {
                var dbContext = services.GetRequiredService<TDatabaseContext>();
                var container = cosmosDbStoreContext(dbContext);
                return new CosmosDbStoreContext(container);
            });

            return builder.WithStore<CosmosDbStore<TTenantInfo>>(ServiceLifetime.Scoped);
        }

        /// <summary>
        /// Adds a CosmosDb based multi-tenant store to the application. Will also create the database context service unless it's already exists.
        /// </summary>
        /// <typeparam name="TDatabaseContext"></typeparam>
        /// <typeparam name="TTenantInfo"></typeparam>
        /// <param name="builder"></param>
        /// <param name="connectionString">CosmosDb Connection String.</param>
        /// <param name="cosmosDbStoreContext">Function to select the container to use when looking up the <see cref="ITenantInfo"/>.</param>
        /// <returns>The same <see cref="FinbuckleMultiTenantBuilder{TTenantInfo}"/> passed into the method.</returns>
        public static FinbuckleMultiTenantBuilder<TTenantInfo> WithCosmosDbStore<TDatabaseContext, TTenantInfo>(this FinbuckleMultiTenantBuilder<TTenantInfo> builder,
            string connectionString,
            Func<TDatabaseContext, Container> cosmosDbStoreContext)
            where TTenantInfo : class, ITenantInfo, new()
            where TDatabaseContext : DatabaseContext
        {
            return WithCosmosDbStore(builder, connectionString, new CosmosSerializationOptions(), cosmosDbStoreContext);
        }
    }
}