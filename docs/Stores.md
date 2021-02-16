# MultiTenant Stores
A multitenant store is responsible for retrieving information about a tenant based on an identifier string determined by [MultiTenant strategies](Strategies). The retrieved information is then used to create a `TenantInfo` object which provides the current tenant information to an app.

Finbuckle.MultiTenant supports several "out-of-the-box" stores for resolving the
tenant. Custom stores can be created by implementing `IMultiTenantStore`.

## Custom ITenantInfo Support
MultiTenant stores support custom `ITenantInfo` implementations. but complex
implementations may require special handling. For best results ensure the class
works well with the underlying store approach--e.g. that it can be serialized
from JSON for the configuration store if using json file configuration sources.

The examples in this documentation use the `TenantInfo` basic implementation.

## IMultiTenantStore and Custom Stores
If the provided multitenant stores are not suitable then a custom store can be created by implementing `IMultiTenantStore<TTenantInfo>`. The library will set the type parameter`TTenantInfo` to match the type parameter passed to `AddMultiTenant<T>` at compile time. The implementation must define `TryAddAsync`, `TryUpdateAsync`, `TryRemoveAsync`, `TryGetByIdentifierAsync`, `TryGetAsync`, and `GetAllAsync` methods. `TryGetByIdentifierAsync` and `TryGetAsync` should return null if there is no suitable tenant match.

A custom implementation of `IMultiTenantStore<TTenantInfo>` can be registered by calling `WithStore<TStore>` after `AddMultiTenant<T>` in the `ConfigureServices` method of the `Startup` class. `WithStore<TStore>` uses dependency injection along with any passed parameters to construct the implementation instance. An alternative overload accepts a `Func<IServiceProvider, TStore>` factory method for even more customization. Both methods also require a service lifetime when registering. The library internally decorates any `IMultiTenantStore<TTenantInfo>` at runtime ith a wrapper providing basic logging and exception handling.

```cs
// Register a custom store with the templated method.
services.AddMultiTenant<TenantInfo>()
        .WithStore<MyStore>(ServiceLifetime.Singleton, myParam1, myParam2)...

// Or register a custom store with the non-templated method which accepts a factory method.
// Note that the type parameter on `WithStore` is inferred by the compiler.
services.AddMultiTenant<TenantInfo>()
        .WithStore(ServiceLifetime.Singleton, sp => new MyStore())...
```

## Using Multiple Stores
Multiple stores can be used, and for each strategy returning a non-null
identifier the stores are checked in the order registered until a matching
tenant is resolved. Keep in mind that if multiple strategies are used it is
possible for a store to be checked multiple times during tenant resolution.

## Accessing the Store at Runtime
MultiTenant stores are registered in the dependency injection system under the
`IMultiTenantStore<TenantInfo>` service type.

If multiple stores are registered a specific one can be retrieving an
`IEnumerable<IMultiTenantStore<TenantInfo>>` and filtering to the specific
implementation type:

```cs
// Assume we have a service provider. The IEnumerable could be injected via
// other DI means as well.
var store = serviceProvider.GetService<IEnumerable<IMultiTenantStore<TenantInfo>>>
                           .Where(s => s.ImplementationType == typeof(InMemoryStore))
                           .SingleOrDefault();

// Add some tenants...
await store.TryAddAsync(new TenantInfo{...});
```

## Getting All Tenants from Store
If implemented, `GetAllAsync` will return an `IEnumerable<TTenantInfo>` listing of all  tenants in the store.
Currently `InMemoryStore`, `ConfigurationStore`, and `EFCoreStore` implement `GetAllAsync`.

## In-Memory Store
> NuGet package: Finbuckle.MultiTenant

Uses a `ConcurrentDictionary<string, TenantInfo>` as the underlying store.

Configure by calling `WithInMemoryStore` after `AddMultiTenant<T>` in the `ConfigureServices` method of the app's `Startup` class.y By default the store is empty and the tenant identifier matching is case insensitive. An overload of `WithInMemoryStore` accepts an `Action<InMemoryStoreOptions>` delegate to configure the store further:

```cs
// Set up a case-insensitive in-memory store.
services.AddMultiTenant<TenantInfo>()
        .WithInMemoryStore()...

// Or make it case sensitive and/or add some tenants.
services.AddMultiTenant<TenantInfo>()
        .WithInMemoryStore(options =>
        {
          options.IsCaseSensitive = true;
          options.Tenants.Add(new TenantInfo{...});
          options.Tenants.Add(new TenantInfo{...});
          options.Tenants.Add(new TenantInfo{...});
        })...
```

The contents of the store can be changed at runtime with `TryAddAsync`, `TryUpdateAsync`, and `TryRemoveAsync`:
```cs
// Use service provider or dependency injection to get the InMemoryStore instance.
var store = serviceProvider.GetService<IEnumerable<IMultiTenantStore<TenantInfo>>>()
                           .Where(s => s.ImplementationType == typeof(InMemoryStore<TenantInfo>))
                           .SingleOrDefault();   

// Add a new tenant to the store.
var newTenant = new TenantInfo{...};
await store.TryAddAsync(newTenant);

// Update a tenant.
newTenant.ConnectionString = "UpdatedConnectionString";
await store.TryUpdateAsync(newTenant);

// Remove a tenant.
await store.TryRemoveAsync(newTenant.Identifier);
```

When possible prefer a case-insensitive in-memory store.

## Configuration Store
> NuGet package: Finbuckle.MultiTenant

Uses an app's [configuration](https://docs.microsoft.com/en-us/aspnet/core/fundamentals/configuration/?view=aspnetcore-3.1) as the underlying store. Most of the sample projects use this store for simplicity. This store is case insensitive when retrieving tenant information by tenant identifier.

This store is read-only and calls to `TryAddAsync`, `TryUpdateAsync`, and `TryRemoveAsync` will throw a `NotImplementedException`. However, if the app is configured to reload its configuration if the source changes, e.g. `appsettings.json` is updated, then the multitenant store will reflect the change.

Configure by calling `WithConfigurationStore` after `AddMultiTenant<T>` in the `ConfigureServices` method of the app's `Startup` class. By default it will use the root configuration object and search for a section named "Finbuckle:MultiTenant:Stores:ConfigurationStore". An overload of `WithConfigurationStore` allows for a different base configuration object or section name if needed.

```cs
// Register to use the default root configuration and section name.
services.AddMultiTenant<TenantInfo>()
        .WithConfigurationStore()...
```

The configuration section should use this JSON format shown below. Any fields in the `Defaults` section will be automatically copied into each tenant unless the tenant specifies its own value. For a custom implementation of `ITenantInfo` properties are mapped from the JSON automatically.

> Note: Finbuckle.MultiTenant versions prior to 6.0 had an `Index` collection
> property to store custom data. This was removed because custom `ITenantInfo`
> implementations can use normal properties for custom data. Older examples
> might show an `Index` sub-object in the configuration json that may not be
> applicable.

```json
{
  "Finbuckle:MultiTenant:Stores:ConfigurationStore": {
    "Defaults": {
        "ConnectionString": "default_connection_string"
      },
    "Tenants": [
      {
        "Id": "unique-id-0ff4adaf",
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

Uses an Entity Framework Core database context as the backing store. This store is usually case-sensitive when retrieving tenant information by tenant identifier, depending on the underlying database.

The database context should derive from `EFCoreStoreDbContext`. The code examples below are taken from the [EFCore Store Sample](https://github.com/Finbuckle/Finbuckle.MultiTenant/tree/master/samples/ASP.NET%20Core%203/EFCoreStoreSample).

The database context used with the EFCore store must derive from `EFCoreStoreDbContext`, but other entities can be added:

```cs
public class MultiTenantStoreDbContext : EFCoreStoreDbContext
{
  protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
  {
      // Use InMemory, but could be MsSql, Sqlite, MySql, etc...
      optionsBuilder.UseInMemoryDatabase("EfCoreStoreSampleConnectionString");
      base.OnConfiguring(optionsBuilder);
  }

  // Other stuff if needed...
}
```

This database context will have its own connection string (usually) separate from that of any tenant in the store. Additionally, this database context can be entirely separate from any others an application might use if co-mingling the multitenant store and app entity models is not desired.

Configure by calling `WithEFCoreStore<TEFCoreStoreDbContext>` after `AddMultiTenant<T>` in the `ConfigureServices` method of the app's `Startup` class and provide types for the store's database context generic parameter:

```cs
// Register to use the database context and TTenantInfo types show above.
services.AddMultiTenant<TenantInfo>()
        .WithEFCoreStore<MultiTenantStoreDbContext>()...
```

The contents of the store can be changed at runtime with the `TryAddAsync`,
`TryUpdateAsync`, and `TryRemoveAsync` methods of `IMultiTenantStore`:

```cs
// Use service provider or dependency injection to get the store instance.
// Here assuming only one store is registered in DI.
var store = serviceProvider.GetService<IMultiTenantStore>();

// Add a new tenant to the store.
var newTenant = new TenantInfo(...);
store.TryAddAsync(newTenant);

// Update a tenant.
newTenant.ConnectionString = "UpdatedConnectionString";
store.TryUpdate(newTenant);

// Remove a tenant.
store.TryRemove(newTenant.Identifier);
```

In addition the underlying db context can be used to modify data in the same way
Entity Framework Core works with any db context.

## Http Remote Store
> NuGet package: Finbuckle.MultiTenant

Sends the tenant identifier, provided by the multitenant strategy, to an http(s) endpoint to get a `TenantInfo` object in return. The [Http Remote Store Sample](https://github.com/Finbuckle/Finbuckle.MultiTenant/tree/master/samples/ASP.NET%20Core%203/HttpRemoteStoreSample) projects demonstrate this store. This store is usually case insensitive when retrieving tenant information by tenant identifier, but the remote server might be more restrictive.

Note, make sure the tenant info type will support basic JSON serialization and
deserialization.

For a successfully request, the store expects a 200 response code and a json body with properties `Id`, `Identifier`, `Name`, and `ConnectionString` and other properties which will be mapped into a `TenantInfo` object with the type passed to `AddMultiTenant<T>`.

Any non-200 response code results in a null `TenantInfo`.

This store is read-only and calls to `TryAddAsync`, `TryUpdateAsync`, and `TryRemoveAsync` will throw a `NotImplementedException`.

Configure by calling `WithHttpRemoteStore` after `AddMultiTenant<T>` in the `ConfigureServices` method of the app's `Startup` class. A uri template string must be passed to the method. At runtime the tenant identifier will replace the substring `{__tenant__}` in the uri template. If the template provided does not contain `{__tenant__}`, the identifier is appended to the template. An overload of `WithHttpRemoteStore` allows for a lambda function to further configure the internal `HttpClient`.

```cs
// This will append the identifier to the provided url.
services.AddMultiTenant<TenantInfo>()
        .WithHttpRemoteStore("https://remoteserver.com/)...
```

```cs
// This will replace {__tenant__} with the identifier.
services.AddMultiTenant<TenantInfo>()
        .WithHttpRemoteStore("https://remoteserver.com/{__tenant__}/getinfo)...
```

Use the overload of `WithHttpRemoteStore` to configure the underlying `HttpClient`:
```cs
// This will inject MyCustomHeaderHandler, a DelegatingHandler, to the request pipeline.
services.AddMultiTenant<TenantInfo>()
        .WithHttpRemoteStore("https://remoteserver.com/", httpClientBuilder =>
        {
            httpClientBuilder.ConfigureHttpClient( client =>
            {
                client.Timeout = TimeSpan.FromSeconds(10);
            });
        });
```

Use the same overload to configure delegating handlers and [customize the http request behavior](https://docs.microsoft.com/en-us/aspnet/core/fundamentals/http-requests?view=aspnetcore-3.1#outgoing-request-middleware). For example, adding custom headers for authentication:
```cs
// This will inject MyCustomHeaderHandler, a DelegatingHandler, to the request pipeline.
services.AddMultiTenant<TenantInfo>()
        .WithHttpRemoteStore("https://remoteserver.com/", httpClientBuilder =>
        {
            httpClientBuilder.AddHttpMessageHandler<MyCustomHeaderHandler>();
        });
```

Use the same overload to add resilience and transient fault handling with [Polly](https://www.hanselman.com/blog/AddingResilienceAndTransientFaultHandlingToYourNETCoreHttpClientWithPolly.aspx):
```cs
// This will retry the request if needed.
services.AddMultiTenant<TenantInfo>()
        .WithHttpRemoteStore("https://remoteserver.com/", httpClientBuilder =>
        {
            httpClientBuilder.AddTransientHttpErrorPolicy(policyBuilder => policyBuilder.RetryAsync(2));
        });
```

## Distributed Cache Store
> NuGet package: Finbuckle.MultiTenant

Uses the ASP.NET Core [distributed cache](https://docs.microsoft.com/en-us/aspnet/core/performance/caching/distributed?view=aspnetcore-3.1)
mechanism. The distributed cache can use Redis, SQl Server, NCache, or an in-memory (for
testing purposes) implementation. A sliding expiration is also supported.

Note, make sure the tenant info type will support basic JSON serialization and
deserialization.

Each tenant info instance is actually stored twice in the cache, once using the
Tenant Id as the key and another using the Tenant Identifier as the key. Calls
to `TryAddAsync`, `TryUpdateAsync`, and `TryRemoveAsync` will keep these dual
cache entries synched.

This store does not implement `GetAllAsync`.

Configure by calling `WithDistributedCacheStore` after `AddMultiTenant<T>` in
the `ConfigureServices` method of the app's `Startup` class. By default entries
do not expire, but a `TimeSpan` can be passed to be used as a sliding expiration
for all entries.

Note that the store does not interact with any other stores by default.

```cs
// This will use the default configuration with no sliding expiration.
services.AddMultiTenant<TenantInfo>()
        .WithDistributedCacheStore()...

// This will set a 5 minute sliding expiration.
services.AddMultiTenant<TenantInfo>()
        .WithDistributedCacheStore(TimeSpan.FromMinutes(5));
```