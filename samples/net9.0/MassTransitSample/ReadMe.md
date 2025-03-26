# MassTransitSample Application

## Overview

This sample application demonstrates how to use MassTransit with Finbuckle.MultiTenant to handle multi-tenant scenarios. The application is configured to use tenant-specific headers to manage tenant context in HTTP requests and MassTransit messages.

The headers do not need to be the same header name for HTTP or MassTransit, but it is required that the same header name is used consistently where end services rely on each other.

For example, MassTansit can use `__tenant__` header internally within the service bus, and externally you might want to surface the tenant context in HTTP requests using a header like `X-Tenant`.

## Features

*	Multi-tenant support using Finbuckle.MultiTenant.  
*	MassTransit integration to handle tenant context in messages.
*	Configuration of tenants in appsettings.json.
*	HTTP header strategy to determine the tenant.
*	In-memory transport for MassTransit.

## Prerequisites
*	.NET 9 SDK
*	Visual Studio 2022 or any other compatible IDE

## Configuration

The tenant configuration is stored in appsettings.json:

```Json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  },
  "AllowedHosts": "*",
  "Finbuckle:MultiTenant:Stores:ConfigurationStore": {
    "Defaults": {
    },
    "Tenants": [
      {
        "Id": "unique-id-1",
        "Identifier": "tenant-1",
        "Name": "T1"
      },
      {
        "Id": "unique-id-2",
        "Identifier": "tenant-2",
        "Name": "T2"
      },
      {
        "Id": "unique-id-3",
        "Identifier": "tenant-3",
        "Name": "T3"
      }
    ]
  }
}

```

## Usage

The `program.cs` should contain the following code to configure the tenant middleware:

Note this is the minimum configuration required to get the sample working. You can add more configuration as needed.

```C#
public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            builder.Services.AddMultiTenant<TenantInfo>()
                .WithConfigurationStore() // Store is in appsettings.json
                .WithMassTransitHeaderStrategy(); // Adds the MassTransit header strategy to the MultiTenant middleware.
                                                  

            builder.Services.AddMassTransit(x =>
            {
                x.AddConsumer<GettingStartedConsumer>(); // The MassTransit Consumer that will be used to consume messages.

                x.UsingInMemory((IBusRegistrationContext context, IInMemoryBusFactoryConfigurator cfg) => //using in memory for simplicity. Please replace with your preferred transport method.
                {
                    cfg.AddTenantFilters(context); // Required if wanting to have a MassTransit Consumer and maintain tenant context. To use this filter, .WithMassTransitHeaderStrategy() must be called in the MultiTenantBuilder.
                    cfg.ConfigureEndpoints(context);
                });
            });

            builder.Services.AddControllers();            

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
               
            }

            app.UseMultiTenant(); // Adds the MultiTenant middleware to the request pipeline.

            app.UseHttpsRedirection();

            app.UseAuthorization();

            app.MapControllers();

            app.Run();
        }
```

## Testing this sample

* The API docs should open automatically when you run the application. If not, navigate to `<baseUri>/scalar/v1` to view the API documentation.
* you can then run requests to the API which sends the Service Bus message to the consumer. Note the tenant identifier in the request header.

Alternatively, you can use a tool like Postman to send requests to the API.

The MassTransitSample.http file can be ran against a working instance of the application to test the API.