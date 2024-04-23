# MultiTenant Stores

A MultiTenant store is responsible for retrieving information about a tenant based on an identifier string determined
by [MultiTenant strategies](Strategies). The retrieved information is then used to create a `TenantInfo` object which
provides the current tenant information to an app.

Finbuckle.MultiTenant supports several "out-of-the-box" stores for resolving the tenant. Custom stores can be created by
implementing `IMultiTenantStore`.

## Custom ITenantInfo Support

MultiTenant stores support custom `ITenantInfo` implementations. but complex implementations may require special
handling. For best results ensure the class works well with the underlying store approach--e.g. that it can be
serialized from JSON for the configuration store if using JSON file configuration sources.

The examples in this documentation use the `TenantInfo` basic implementation.

## IMultiTenantStore and Custom Stores

If the provided MultiTenant stores are not suitable then a custom store can be created by
implementing `IMultiTenantStore<TTenantInfo>`. The library will set the type parameter`TTenantInfo` to match the type
parameter passed to `AddMultiTenant<TTenantInfo>` at compile time. The implementation must
define `TryAddAsync`, `TryUpdateAsync`
, `TryRemoveAsync`, `TryGetByIdentifierAsync`, `TryGetAsync`, and `GetAllAsync` methods. `TryGetByIdentifierAsync`
and `TryGetAsync` should return null if there is no suitable tenant match.

A custom implementation of `IMultiTenantStore<TTenantInfo>` can be registered by calling `WithStore<TStore>`
after `AddMultiTenant<TTenantInfo>` in the `ConfigureServices` method of the `Startup` class. `WithStore<TStore>` uses
dependency
injection along with any passed parameters to construct the implementation instance. An alternative overload accepts
a `Func<IServiceProvider, TStore>` factory method for even more customization. Both methods also require a service
lifetime when registering. The library internally decorates any `IMultiTenantStore<TTenantInfo>` at runtime ith a
wrapper providing basic logging and exception handling.

```csharp
// register a custom store with the templated method
builder.Services.AddMultiTenant<TenantInfo>()
    .WithStore<MyStore>(ServiceLifetime.Singleton, myParam1, myParam2)...

// or register a custom store with the non-templated method which accepts a factory method
builder.Services.AddMultiTenant<TenantInfo>()
    .WithStore(ServiceLifetime.Singleton, sp => new MyStore())...
```

## Using Multiple Stores

Multiple stores can be used, and for each strategy returning a non-null identifier the stores are checked in the order
registered until a matching tenant is resolved. Keep in mind that if multiple strategies are used it is possible for a
store to be checked multiple times during tenant resolution.

## Accessing the Store at Runtime

MultiTenant stores are registered in the dependency injection system under the
`IMultiTenantStore<TenantInfo>` service type.

If multiple stores are registered a specific one can be retrieving an
`IEnumerable<IMultiTenantStore<TenantInfo>>` and filtering to the specific implementation type:

## Getting All Tenants from Store

If implemented, `GetAllAsync` will return an `IEnumerable<TTenantInfo>` listing of all tenants in the store.
Currently `InMemoryStore`, `ConfigurationStore`, and `EFCoreStore` implement `GetAllAsync`.

## In-Memory Store

> NuGet package: Finbuckle.MultiTenant

Uses a `ConcurrentDictionary<string, TenantInfo>` as the underlying store.

Configure by calling `WithInMemoryStore` after `AddMultiTenant<TTenantInfo>`. By default the store is empty and the
tenant identifier matching is case insensitive. Case insensitive is generally preferred. An overload
of `WithInMemoryStore` accepts an `Action<InMemoryStoreOptions>` delegate to configure the store further:

```csharp
// set up a case-insensitive in-memory store.
builder.Services.AddMultiTenant<TenantInfo>()
    .WithInMemoryStore()...

// or make it case sensitive and/or add some tenants.
builder.Services.AddMultiTenant<TenantInfo>()
    .WithInMemoryStore(options =>
    {
        options.IsCaseSensitive = true;
        options.Tenants.Add(new TenantInfo{...});
        options.Tenants.Add(new TenantInfo{...});
        options.Tenants.Add(new TenantInfo{...});
    })...
```

## Configuration Store

> NuGet package: Finbuckle.MultiTenant

Uses an
app's [configuration](https://docs.microsoft.com/en-us/aspnet/core/fundamentals/configuration/) as
the underlying store. Most of the sample projects use this store for simplicity. This store is case insensitive when
retrieving tenant information by tenant identifier.

This store is read-only and calls to `TryAddAsync`, `TryUpdateAsync`, and `TryRemoveAsync` will throw
a `NotImplementedException`. However, if the app is configured to reload its configuration if the source changes,
e.g. `appsettings.json` is updated, then the MultiTenant store will reflect the change.

Configure by calling `WithConfigurationStore` after `AddMultiTenant<TTenantInfo>`. By default it will use the root
configuration object and search for a section named "Finbuckle:MultiTenant:Stores:ConfigurationStore". An overload
of `WithConfigurationStore` allows for a different base
configuration object or section name if needed.

```csharp
// register to use the default root configuration and section name.
builder.Services.AddMultiTenant<TenantInfo>()
    .WithConfigurationStore()...
    
// or use a different configuration path key
builder.Services.AddMultiTenant<TenantInfo>()
    .WithConfigurationStore("customConfigurationPathKey)...
```

The configuration section should use this JSON format shown below. Any fields in the `Defaults` section will be
automatically copied into each tenant unless the tenant specifies its own value. For a custom implementation
of `ITenantInfo` properties are mapped from the JSON automatically.

```json
{
  "Finbuckle:MultiTenant:Stores:ConfigurationStore": {
    "Defaults": {
      "ConnectionString": "default_connection_string"
    },
    "Tenants": [
      {
        "Id": "unique-id-0ff4daf",
        "Identifier": "tenant-1",
        "Name": "Tenant 1 Company Name",
        "ACustomProperty": "VIP Customer"
      },
      {
        "Id": "unique-id-ao41n44",
        "Identifier": "tenant-2",
        "Name": "Name of Tenant 2",
        "ConnectionString": "tenant_specific_connection_string"
      }
    ]
  }
}
```

## EFCore Store

> NuGet package: Finbuckle.MultiTenant.EntityFrameworkCore

Uses an Entity Framework Core database context as the backing store.

This store is usually case-sensitive when retrieving tenant information by tenant identifier, depending on the underlying database.

The database context should derive from `EFCoreStoreDbContext`. The code examples below are taken from
the [EFCore Store Sample](https://github.com/Finbuckle/Finbuckle.MultiTenant/tree/v6.9.1/samples/ASP.NET%20Core%203/EFCoreStoreSample)
.

The database context used with the EFCore store must derive from `EFCoreStoreDbContext`, but other entities can be
added:

```csharp
public class MultiTenantStoreDbContext : EFCoreStoreDbContext<TenantInfo>
{
  protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
  {
      // Use InMemory, but could be MsSql, Sqlite, MySql, etc...
      optionsBuilder.UseInMemoryDatabase("EfCoreStoreSampleConnectionString");
      base.OnConfiguring(optionsBuilder);
  }
}
```

This database context is not itself multi-tenant, but rather it globally contains the details of each tenant.
It will often be a standalone database separate from any tenant database(s) and will have its own connection string.

Configure by calling `WithEFCoreStore<TEFCoreStoreDbContext,ITenantInfo>` after `AddMultiTenant<TTenantInfo>` and
provide types for the store's database context generic parameter:

```csharp
// configure dbcontext `MultiTenantStoreDbContext`, which derives from `EFCoreStoreDbContext`
builder.Services.AddMultiTenant<TenantInfo>()
    .WithEFCoreStore<MultiTenantStoreDbContext,TenantInfo>()...
```

In addition the `IMultiTenantStore` interface methods, the database context can be used to modify data in the same way
Entity Framework Core works with any database context which can offer richer functionality.

## Http Remote Store

> NuGet package: Finbuckle.MultiTenant

Sends the tenant identifier, provided by the multitenant strategy, to an http(s) endpoint to get a `TenantInfo` object
in return.

The [Http Remote Store Sample](https://github.com/Finbuckle/Finbuckle.MultiTenant/tree/v6.9.1/samples/ASP.NET%20Core%203/HttpRemoteStoreSample)
projects demonstrate this store. This store is usually case insensitive when retrieving tenant information by tenant identifier, but the remote server might be more restrictive.

Make sure the tenant info type will support basic JSON serialization and deserialization via `System.Text.Json`.
This strategy will attempt to deserialize the tenant using the [System.Text.Json web defaults](https://docs.microsoft.com/en-us/dotnet/standard/serialization/system-text-json-configure-options?pivots=dotnet-6-0#web-defaults-for-jsonserializeroptions).

For a successfully request, the store expects a 200 response code and a json body with properties `Id`, `Identifier`
, `Name`, and other properties which will be mapped into a `TenantInfo` object with the type
passed to `AddMultiTenant<TTenantInfo>`.

Any non-200 response code results in a null `TenantInfo`.

This store is read-only and calls to `TryAddAsync`, `TryUpdateAsync`, and `TryRemoveAsync` will throw
a `NotImplementedException`.

Configure by calling `WithHttpRemoteStore` after `AddMultiTenant<TTenantInfo>` uri template string must be passed to the
method. At runtime the tenant identifier will replace the substring `{__tenant__}` in the uri template. If the template
provided does not contain `{__tenant__}`, the identifier is appended to the template. An overload
of `WithHttpRemoteStore` allows for a lambda function to further configure the internal `HttpClient`:

```csharp
// append the identifier to the provided url
builder.Services.AddMultiTenant<TenantInfo>()
    .WithHttpRemoteStore("https://remoteserver.com/)...

// or template the identifier into a custom location
builder.Services.AddMultiTenant<TenantInfo>()
    .WithHttpRemoteStore("https://remoteserver.com/{__tenant__}/getinfo)...

// or modify the underlying `HttpClient` with a custom message handler and settings
builder.Services.AddMultiTenant<TenantInfo>()
    .WithHttpRemoteStore("https://remoteserver.com/", httpClientBuilder =>
    {
        httpClientBuilder.AddHttpMessageHandler<MyCustomHeaderHandler>();
        
        httpClientBuilder.ConfigureHttpClient( client =>
        {
            client.Timeout = TimeSpan.FromSeconds(10);
        });
    });
 
// or add Polly support
// via https://www.hanselman.com/blog/AddingResilienceAndTransientFaultHandlingToYourNETCoreHttpClientWithPolly.aspx
builder.Services.AddMultiTenant<TenantInfo>()
    .WithHttpRemoteStore("https://remoteserver.com/", httpClientBuilder =>
    {
        httpClientBuilder.AddTransientHttpErrorPolicy(policyBuilder => policyBuilder.RetryAsync(2));
    });
```

## Distributed Cache Store

> NuGet package: Finbuckle.MultiTenant

Uses the ASP.NET
Core [distributed cache](https://docs.microsoft.com/en-us/aspnet/core/performance/caching/distributed)
mechanism. The distributed cache can use Redis, SQl Server, NCache, or an in-memory (for testing purposes)
implementation. A sliding expiration is also supported. The store does not interact with any other stores by default.
Make sure the tenant info type will support basic JSON serialization and deserialization via `System.Text.Json`.
This strategy will attempt to deserialize the tenant using the [System.Text.Json web defaults](https://docs.microsoft.com/en-us/dotnet/standard/serialization/system-text-json-configure-options?pivots=dotnet-6-0#web-defaults-for-jsonserializeroptions).

Each tenant info instance is actually stored twice in the cache, once using the Tenant Id as the key and another using
the Tenant Identifier as the key. Calls to `TryAddAsync`, `TryUpdateAsync`, and `TryRemoveAsync` will keep these dual
cache entries synced.

This store does not implement `GetAllAsync`.

Configure by calling `WithDistributedCacheStore` after `AddMultiTenant<TTenantInfo>`. By default entries do not expire,
but a `TimeSpan` can be passed to be used as a sliding
expiration:

```csharp
// use the default configuration with no sliding expiration.
services.AddMultiTenant<TenantInfo>()
        .WithDistributedCacheStore()...

// or set a 5 minute sliding expiration.
services.AddMultiTenant<TenantInfo>()
        .WithDistributedCacheStore(TimeSpan.FromMinutes(5));
```
