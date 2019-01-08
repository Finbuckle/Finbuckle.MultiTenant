# MultiTenant Stores

A multiTenant store is responsible for retrieving information about the tenant based on an identifier string produced by the [multiTenant strategy](Strategies). The retrieved information is then used to create a `TenantContext` object.

Finbuckle.MultiTenant provides a simple thread safe in-memory implementation based on `ConcurrentDictionary<string, object>` which can be configured from an `appSettings.json` file. Custom stores can be created by implementing `IMultiTenantStore`.

## Accessing the Store at Runtime

The multitenant store can be accessed at runtime to add, remote, or retreieve a `TenantContext` in addition to any startup configuration the store implementation may offer (such as the `appSettings.json` configuration supported by the In-Memory Store).

The multitenant store is registered as a singleton in the app's service collection. Access it via dependecy injection by including an `IMultiTenantStore` constructor parameter, action method parameter marked with `[FromService]`, or the `HttpContext.RequestServices` service provider instance.

## IMultiTenantStore
All multitenant stores derive from `IMultiTenantStore` and must implement `TryAdd`, `TryRemove`, and `GetByIdentifierAsync` methods. `GetByIdentifierAsync` should return null if there is no suitable tenant match.

A custom implementation of `IMultiTenantStore` can be configured by calling `WithStore<T>` or `WithStore` after `AddMultiTenant` in the `ConfigureServices` method of the `Startup` class. The templated version will use dependency injection and any passed parameters to construct the implementation instance. The non-templated version accepts a `Func<IServiceProvider, IMultiTenantStore>` factory method for even more customization.

```cs
// Register a custom store with the templated method.
services.AddMultiTenant().WithStore<MyStore>(myParam1, myParam2);

// Or register a custom store with the non-templated method which accepts a factory method.
services.AddMultiTenant().WithStore( sp => return new MyStore());
```

## In-Memory Store
Uses a `ConcurrentDictionary<string, TenantContext>` as the underlying store. By default the tenant identifier matching is case insensitive. This can be overridden by passing false to the constructor's `ignoreCase` paramater.

If using with `Finbuckle.MultiTenant.AspNetCore`, configure by calling `WithInMemoryStore` after `AddMultiTenant` in the `ConfigureServices` method of the `Startup` class.

```cs
// Set up a case insentitive in-memory store.
services.AddMultiTenant().WithInMemoryStore()...

// Or make it case sensitive.
services.AddMultiTenant().WithInMemoryStore(ignoreCase: false)...
```

A `ConfigurationSection` can also be used to configure the store:

```cs
// Register by passing a configuration section.
services.AddMultiTenant().WithInMemoryStore(Configuration.GetSection("InMemoryStoreConfig"))...
```

The configuration section should use this json format:

```json
{
  "InMemoryStoreConfig": {
    "DefaultConnectionString": "default_connection_string",
    "TenantConfigurations": [
      {
        "Id": "unique-id-0ff4adav",
        "Identifier": "tenant-1",
        "Name": "Tenant 1 Company Name"
      },
      {
        "Id": "unique-id-ao41n44",
        "Identifier": "tenant-2",
        "Name": "Name of Tenant 2",
        "ConnectionString": "tenant_specific_connection_string",
        "Items": {
            "Thing": "Some other tenant property",
            "AnotherThing": "Another property for this particular tenant"
        }
      }
    ]
  }
}
```
