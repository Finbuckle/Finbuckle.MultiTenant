// Copyright Finbuckle LLC, Andrew White, and Contributors.
// Refer to the solution LICENSE file for more inforation.

using System;
using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Configuration;

namespace Finbuckle.MultiTenant.Cosmos.Test;

public class CosmosClientFixture : IDisposable
{
    public CosmosClientFixture()
    {
        var configurationBuilder = new ConfigurationBuilder();
        configurationBuilder.AddUserSecrets<CosmosClientFixture>();
        var configuration = configurationBuilder.Build();
        ConnectionString = configuration.GetConnectionString("DefaultConnection");
        DatabaseId = $"CosmosStore Test Data ({Environment.Version.Major}.{Environment.Version.Minor})";
        ContainerId = "CosmosStore";
        var options = new CosmosClientOptions
        {
            SerializerOptions = new CosmosSerializationOptions
            {
                PropertyNamingPolicy =
                    CosmosPropertyNamingPolicy.CamelCase
            }
        };
        CosmosClient = new CosmosClient(ConnectionString, options);

        CosmosClient.GetDatabase(DatabaseId).DeleteStreamAsync().Wait();
        var database = CosmosClient.CreateDatabaseAsync(DatabaseId).Result;
        database.Database.CreateContainerAsync(ContainerId, "/PartitionKey").Wait();
    }

    public CosmosClient CosmosClient { get; set; }
    public string ConnectionString { get; set; }
    public string DatabaseId { get; set; }
    public string ContainerId { get; set; }

    public void Dispose()
    {
        // Keep data for checking, overwritten on each run.
        //CosmosClient.GetDatabase(DatabaseId).DeleteAsync().Wait();
        CosmosClient.Dispose();
    }
}