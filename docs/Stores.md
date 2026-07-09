# MultiTenant Stores

A MultiTenant store is responsible for retrieving tenant information after a
[MultiTenant strategy](Strategies) determines a tenant identifier. The retrieved `ITenantInfo` object becomes the
current tenant information for the app.

Tenant storage has three parts:

- A single primary store registered as `IMultiTenantStore<TTenantInfo>`.
- Zero or more store caches registered as `IMultiTenantStoreCache<TTenantInfo>`.
- `TenantManager<TTenantInfo>`, which is the runtime API that coordinates reads, writes, cache read-through, and cache
  invalidation.

During tenant resolution, each strategy can produce an identifier. For that identifier, `TenantManager<TTenantInfo>`
checks store caches in registration order before querying the primary store. If a later cache or the primary store
returns a tenant, earlier caches that missed are populated. This means faster caches can sit in front of durable stores
without each app needing to implement its own cache coordination.

Writes are deliberately simpler: they go only to the primary store. After a successful add, update, or remove,
`TenantManager<TTenantInfo>` invalidates the affected entries in every configured cache and does not immediately refill
them. The next read repopulates caches through the normal read-through path. `GetAllAsync` is also primary-store only,
so listing tenants does not populate or depend on caches.

MultiTenant includes several built-in stores and store caches. Custom primary stores implement
`IMultiTenantStore<TTenantInfo>`, while custom store caches implement `IMultiTenantStoreCache<TTenantInfo>`.

> MultiTenant stores support custom `ITenantInfo` implementations, but complex implementations may require special
> handling. For best results ensure the type works well with the underlying store approach—for example, that it can be
> serialized from JSON for the configuration store if using JSON file configuration sources. The examples in this
> documentation use the `TenantInfo` basic implementation.

## TenantManager

`TenantManager<TTenantInfo>` is the main API for reading and writing tenant information at runtime. App code should
inject `TenantManager<TTenantInfo>` rather than injecting `IMultiTenantStore<TTenantInfo>` directly. Direct store access
bypasses cache read-through, cache invalidation, and the basic validation, logging, and error handling that
`TenantManager<TTenantInfo>` provides.

```csharp
// Checks store caches first, then the primary store.
var tenant = await tenantManager.GetByIdentifierAsync("initech");

// GetAsync uses the tenant id.
tenant = await tenantManager.GetAsync("initech-id");

// Writes go to the primary store and invalidate affected cache entries.
await tenantManager.AddAsync(new TenantInfo { Id = "lol-id", Identifier = "lol" });
await tenantManager.UpdateAsync(tenant with { Identifier = "initech-new" });

// RemoveAsync uses the tenant id. Use RemoveByIdentifierAsync for identifiers.
await tenantManager.RemoveAsync("lol-id");
await tenantManager.RemoveByIdentifierAsync("initech-new");

// Always queries the primary store and does not populate caches.
var tenants = await tenantManager.GetAllAsync();
```

## Store and Cache Interfaces

If the provided MultiTenant stores are not suitable then a custom primary store can be created by
implementing `IMultiTenantStore<TTenantInfo>`. The library will set the type parameter `TTenantInfo` to match the type
parameter passed to `AddMultiTenant<TTenantInfo>` at compile time. The interface defines `AddAsync`, `UpdateAsync`,
`RemoveAsync`, `RemoveByIdentifierAsync`, `GetByIdentifierAsync`, `GetAsync`, and `GetAllAsync` methods.
`RemoveAsync` and `GetAsync` use the tenant id. `RemoveByIdentifierAsync` and `GetByIdentifierAsync` use the tenant
identifier. `GetByIdentifierAsync` and `GetAsync` should return null if there is no suitable tenant match.

A custom implementation of `IMultiTenantStore<TTenantInfo>` can be registered by calling `WithStore<TStore>`
after `AddMultiTenant<TTenantInfo>` in the `ConfigureServices` method of the `Startup` class. `WithStore<TStore>` uses
dependency injection along with any passed parameters to construct the implementation instance. Alternative overloads
accept a service lifetime, a factory method, and/or other parameters for more customization. Only one primary store can
be configured.

> Custom store and cache implementations can keep basic validation, logging, and exception handling minimal.
> `TenantManager<TTenantInfo>` handles those concerns consistently at runtime, along with cache read-through and
> invalidation.

```csharp
// register a custom store with the templated method
builder.Services.AddMultiTenant<TenantInfo>()
    .WithStore<MyStore>(ServiceLifetime.Singleton, myParam1, myParam2)...

// or register a custom store with the non-templated method which accepts a factory method
builder.Services.AddMultiTenant<TenantInfo>()
    .WithStore(ServiceLifetime.Singleton, sp => new MyStore())...
```

Custom store caches implement `IMultiTenantStoreCache<TTenantInfo>` and are registered with `WithStoreCache<TCache>`.
Caches are checked in registration order before the primary store. Caches are maintained by `TenantManager` through
read-through population and invalidation after successful primary-store writes. Cache implementations should be treated
as read-only by app code; `TenantManager<TTenantInfo>` is responsible for calling `SetAsync`, `RemoveAsync`, and
`RemoveByIdentifierAsync`.

## Using Store Caches

Multiple store caches can be used, and for each strategy returning a non-null identifier the caches are checked in the
order registered before the primary store. Keep in mind that if multiple strategies are used it is possible for a cache
or primary store to be checked multiple times during tenant resolution.

When a later cache or the primary store returns a tenant, any earlier caches that missed are populated with the tenant.
Each cache uses its own configured cache entry options when storing the tenant. `GetAllAsync` always queries the primary
store and does not populate caches.

Writes are performed against the primary store only. After a successful `AddAsync`, `UpdateAsync`, `RemoveAsync`, or
`RemoveByIdentifierAsync`, `TenantManager<TTenantInfo>` invalidates affected entries from every configured cache and
does not immediately refill them. `UpdateAsync` invalidates both the previous tenant keys and the new tenant keys, so an
identifier change does not leave stale cache entries behind.

## Getting All Tenants from Store

If implemented, `GetAllAsync` will return an `IEnumerable<TTenantInfo>` listing of all tenants in the store.
Currently `InMemoryStore`, `ConfigurationStore`, and `EFCoreStore` implement `GetAllAsync`.

### Pagination of GetAllAsync

An overload to `GetAllAsync(int take, int skip)` exists to optionally allow take and skip parameters for pagination
support if needed when iterating through a large number of tenants or retrieving from a remote source.

## In-Memory Store

> NuGet package: Finbuckle.MultiTenant

Uses a `ConcurrentDictionary<string, TenantInfo>` as the underlying store. See the
[web api sample project](https://github.com/Finbuckle/Finbuckle.MultiTenant/tree/main/samples) for an example of 
using the in-memory store.

Configure by calling `WithInMemoryStore` after `AddMultiTenant<TTenantInfo>`. By default, the store is empty and the
tenant identifier matching is case-insensitive. Case-insensitive is generally preferred. An overload
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
the underlying store. See
the [multi-tenant Identity sample project](https://github.com/Finbuckle/Finbuckle.MultiTenant/tree/main/samples) for
an example of using this store with `appsettings.json`.

This store is case-insensitive when retrieving tenant information by tenant identifier.

This store is read-only and calls to `AddAsync`, `UpdateAsync`, `RemoveAsync`, and `RemoveByIdentifierAsync` will throw
a `NotImplementedException`. However, if the app is configured to reload its configuration if the source changes,
e.g. `appsettings.json` is updated, then the MultiTenant store will reflect the change.

Configure by calling `WithConfigurationStore` after `AddMultiTenant<TTenantInfo>`. By default, it will use the root
configuration object and search for a section named "Finbuckle:MultiTenant:Stores:ConfigurationStore". An overload
of `WithConfigurationStore` allows for a different base
configuration object or section name if needed.

```csharp
// register to use the default root configuration and section name.
builder.Services.AddMultiTenant<TenantInfo>()
    .WithConfigurationStore()...
    
// or use a different configuration path key
builder.Services.AddMultiTenant<TenantInfo>()
    .WithConfigurationStore("customConfigurationPathKey")...
```

The configuration section should use this JSON format shown below. Any fields in the `Defaults` section will be
automatically copied into each tenant unless the tenant specifies its own value. For a custom implementation
of `TenantInfo` properties are mapped from the JSON automatically.

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

Case sensitivity is determined by the underlying EF Core database provider.

The database context must derive from `EFCoreStoreDbContext`. The `EFCoreStore` carefully avoids tracking issues by 
using no-tracking queries and detaching entities after store operations. If your app uses the
`EFCoreStoreDbContext` directly it should be aware of these issues.

This database context is not itself multi-tenant, but rather contains the details of all tenants.
It will often be a standalone database separate from any tenant database(s) and will have its own connection string.

Configure by calling `WithEFCoreStore<TEFCoreStoreDbContext,TenantInfo>` after `AddMultiTenant<TTenantInfo>` and
provide types for the store's database context generic parameter:

```csharp
// configure dbcontext `MultiTenantStoreDbContext`, which derives from `EFCoreStoreDbContext`
builder.Services.AddMultiTenant<TenantInfo>()
    .WithEFCoreStore<MultiTenantStoreDbContext,TenantInfo>()...
```

## Http Remote Store

> NuGet package: Finbuckle.MultiTenant

Sends the tenant identifier, provided by the multi-tenant strategy, to an http(s) endpoint to get a `TenantInfo` object
in return.

This store is usually case-insensitive when retrieving tenant information by tenant identifier, but the remote server
might be more restrictive.

Make sure the tenant info type will support basic JSON serialization and deserialization via `System.Text.Json`.
This store will attempt to deserialize the tenant using
the [System.Text.Json web defaults](https://docs.microsoft.com/en-us/dotnet/standard/serialization/system-text-json-configure-options?pivots=dotnet-6-0#web-defaults-for-jsonserializeroptions).

For a successful request, the store expects a 200 response code and a json body with properties `Id` and `Identifier`.
Any additional properties supported by the type passed to `AddMultiTenant<TTenantInfo>` will also be mapped.

Any non-200 response code results in a null `TenantInfo`.

This store is read-only and calls to `AddAsync`, `UpdateAsync`, `RemoveAsync`, and `RemoveByIdentifierAsync` will throw
a `NotImplementedException`.

Configure by calling `WithHttpRemoteStore` after `AddMultiTenant<TTenantInfo>` uri template string must be passed to the
method. At runtime the tenant identifier will replace the substring `{__tenant__}` in the uri template. If the template
provided does not contain `{__tenant__}`, the identifier is appended to the template. An overload
of `WithHttpRemoteStore` allows for a lambda function to further configure the internal `HttpClient`:

```csharp
// append the identifier to the provided url
builder.Services.AddMultiTenant<TenantInfo>()
    .WithHttpRemoteStore("https://remoteserver.com/")...

// or template the identifier into a custom location
builder.Services.AddMultiTenant<TenantInfo>()
    .WithHttpRemoteStore("https://remoteserver.com/{__tenant__}/getinfo")...

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

## Distributed Cache Store Cache

> NuGet package: Finbuckle.MultiTenant

Uses the [distributed cache](https://docs.microsoft.com/en-us/aspnet/core/performance/caching/distributed)
mechanism. The distributed cache can use Redis, SQL Server, NCache, or an in-memory (for testing purposes)
implementation. Configure expiration with `DistributedCacheEntryOptions`.
Make sure the tenant info type will support basic JSON serialization and deserialization via `System.Text.Json`.
This store will attempt to deserialize the tenant using
the [System.Text.Json web defaults](https://docs.microsoft.com/en-us/dotnet/standard/serialization/system-text-json-configure-options?pivots=dotnet-6-0#web-defaults-for-jsonserializeroptions).

Each tenant info instance is actually cached twice, once using the Tenant ID as the key and another using the Tenant
Identifier as the key. `TenantManager` keeps these dual cache entries synced through read-through population and
invalidation.

Configure by calling `WithDistributedCacheStoreCache` after `AddMultiTenant<TTenantInfo>`.

```csharp
// use the default cache entry configuration.
services.AddMultiTenant<TenantInfo>()
        .WithDistributedCacheStoreCache()
        .WithConfigurationStore()...

// or set a 5 minute sliding expiration.
services.AddMultiTenant<TenantInfo>()
        .WithDistributedCacheStoreCache(options => options.SlidingExpiration = TimeSpan.FromMinutes(5))
        .WithConfigurationStore();
```

## Memory Cache Store Cache

> NuGet package: Finbuckle.MultiTenant

Uses the standard .NET `IMemoryCache` as a tenant store cache. Configure expiration with
`MemoryCacheEntryOptions`.

```csharp
services.AddMultiTenant<TenantInfo>()
    .WithMemoryCacheStoreCache(options => options.SlidingExpiration = TimeSpan.FromMinutes(5))
    .WithConfigurationStore();
```

## Echo Store

> NuGet package: Finbuckle.MultiTenant

The Echo Store serves as a simple, read-only store that directly returns a new tenant instance based on the given
identifier
without any additional settings. It's particularly suited for applications that require a simple, immediate method for
tenant identification without the need for persistence, such as during testing phases or in environments where tenant
information is static and predefined elsewhere.

This store is read-only and calls to `AddAsync`, `UpdateAsync`, `RemoveAsync`, and `RemoveByIdentifierAsync` will throw
a `NotImplementedException`. Because no stores are saved, a call to `GetAllAsync` will also throw an Exception.

Configure by calling `WithEchoStore` after `AddMultiTenant<TTenantInfo>`.

```csharp
services.AddMultiTenant<TenantInfo>()
    .WithEchoStore();
```

## Important Considerations

- Store caches are queried in registration order for each strategy before the primary store. The first source to return
  a match wins.
- `ConfigurationStore`, `HttpRemoteStore`, and `EchoStore` are read-only. `AddAsync`, `UpdateAsync`, `RemoveAsync`,
  and `RemoveByIdentifierAsync` throw `NotImplementedException`.
- Store caches store each tenant twice (by `Id` and by `Identifier`). `TenantManager<TTenantInfo>` keeps both entries
  in sync automatically when resolving tenants or invalidating caches after writes.
- `RemoveAsync` removes by tenant id. `RemoveByIdentifierAsync` removes by tenant identifier.
- `GetAllAsync` is not implemented by all stores. Check individual store documentation before relying on it.
- Custom stores implementing `IMultiTenantStore<TTenantInfo>` should avoid extensive logging or validation —
  `TenantManager<TTenantInfo>` handles these consistently at runtime.

## See Also

- [Configuration and Usage](ConfigurationAndUsage) — store registration
- [MultiTenant Strategies](Strategies) — identifiers to query stores with
- [Getting Started](GettingStarted) — quick start configuration
- [.NET Generic Host Integration](GenericHost) — using stores in non-web apps
