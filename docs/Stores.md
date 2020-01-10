# MultiTenant Stores

A multitenant store is responsible for retrieving information about a tenant based on an identifier string determined by [MultiTenant strategies](Strategies). The retrieved information is then used to create a `TenantInfo` object which provides the current tenant information to an app.

Finbuckle.MultiTenant provides three basic multitenant stores
- `InMemoryStore` - a simple, thread safe in-memory implementation based on `ConcurrentDictionary<string, object>`.
- `ConfigurationStore` - a read-only store that is back by app configuration (e.g. appsettings.json).
- `EFCoreStore` - an Entity Framework Core based implementation to query tenant information from a database.
- `HttpRemoteStore` - a read-only store that sends the tenant identifier to an http(s) endpoint to get the tenant information.

## IMultiTenantStore and Custom Stores
If the provided multitenant stores are not suitable then a custom store can easily be created by implementing `IMultiTenantStore`. The implementation must define the`TryAdd`, `TryRemove`, and `GetByIdentifierAsync` methods. `GetByIdentifierAsync` should return null if there is no suitable tenant match.

A custom implementation of `IMultiTenantStore` can be configured by calling `WithStore<TSore>`after `AddMultiTenant` in the `ConfigureServices` method of the `Startup` class. The first overload uses dependency injection along with any passed parameters to construct the implementation instance. The second overload accepts a `Func<IServiceProvider, TStore>` factory method for even more customization. The library internally decorates any `IMultiTenantStore` with a wrapper providing basic logging and exception handling.

```cs
// Register a custom store with the templated method.
services.AddMultiTenant().WithStore<MyStore>(myParam1, myParam2);

// Or register a custom store with the non-templated method which accepts a factory method.
services.AddMultiTenant().WithStore( sp => return new MyStore());
```

## Accessing the Store at Runtime

The multitenant store can be accessed at runtime to add, remove, or retrieve a `TenantInfo` in addition to any startup configuration the store implementation may offer (such as the `appsettings.json` configuration supported by the In-Memory Store).

There are two ways to access the store. First, via the `Store` property on the `StoreInfo` member of `MultiTenantContext` instance returned by `HttpContext.GetMultiTenantContext()`. This property returns the actual store used to retrieve the tenant information for the current context.

Second, the multitenant store is registered in the app's service collection. Access it via dependency injection by including an `IMultiTenantStore` constructor parameter, action method parameter marked with `[FromService]`, or the `HttpContext.RequestServices` service provider instance.

## In-Memory Store
Uses a `ConcurrentDictionary<string, TenantInfo>` as the underlying store. By default the tenant identifier matching is case insensitive. This can be overridden by passing false to the constructor's `ignoreCase` parameter.

Configure by calling `WithInMemoryStore` after `AddMultiTenant` in the `ConfigureServices` method of the app's `Startup` class:

```cs
// Set up a case-insentitive in-memory store.
services.AddMultiTenant().WithInMemoryStore()...

// Or make it case sensitive.
services.AddMultiTenant().WithInMemoryStore(ignoreCase: false)...
```

The contents of the store can be changed at runtime with `TryAdd`, `TryUpdate`, and `TryRemove`:
```cs
// Use service provider or dependenct injection to get the InMemoryStore instance.
var store = serviceProvider.GetService<IMultiTenantStore>();

// Add a new tenant to the store.
var newTenant = new TenantInfo(...);
store.TryAdd(newTenant);

// Update a tenant.
newTenant.ConnectionString = "UpdatedConnectionString";
store.TryUpdate(newTenant);

// Remove a tenant.
store.TryRemove(newTenant.Identifier);
```

A `ConfigurationSection` can also be used to configure the store:

***This behavior is deprecated and the `ConfigurationStore` is recommended for using app configuration as a multitenant store.***

```cs
// Register by passing a configuration section.
services.AddMultiTenant().WithInMemoryStore(Configuration.GetSection("InMemoryStoreConfig"))...
```

The configuration section should use this JSON format:

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

## Configuration Store
Uses an app's [configuration](https://docs.microsoft.com/en-us/aspnet/core/fundamentals/configuration/?view=aspnetcore-3.1) as the underlying store. Most of the sample projects use this store for simplicity. This store is case insensitive when retrieving tenant information by tenant identifier.

This store is read-only and calls to `TryAdd`, `TryUpdate`, and `TryRemove` will throw a `NotImplementedException`. However, if the app is configured to reload its configuration if the source changes, e.g. `appsettings.json` is updated, then the multitenant store will reflect the change.

Configure by calling `WithConfigurationStore` after `AddMultiTenant` in the `ConfigureServices` method of the app's `Startup` class. By default it will use the root configuration object and search for a section named "Finbuckle:MultiTenant:Stores:ConfigurationStore". An overload of `WithConfigurationStore` allows for a different base configuration object or section name if needed.

```cs
// Register to use the default root configuaration and section name.
services.AddMultiTenant().WithConfigurationStore()...
```

The configuration section should use this JSON format shown below. Any fields in the `Defaults` section will be automatically copied into each tenant unless the tenant specifies its own value.

```json
{
  "Finbuckle:MultiTenant:Stores:ConfigurationStore": {
    "Defaults": {
        "ConnectionString": "default_connection_string"
      },
    "Tenants": [
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

## EFCore Store
Uses an Entity Framework Core database context as the backing store. This store does not support storing and retrieving the `Items` collection property on `TenantInfo`, although it could be modified to do so. Addtionally, this store is usually case-sensitive when retrieving tenant information by tenant identifier, depending on the underlying database.

The database context should derive from `EFCoreStoreDbContext`. The code examples below are taken from the [EFCore Store Sample](https://github.com/Finbuckle/Finbuckle.MultiTenant/tree/master/samples/ASP.NET%20Core%203/EFCoreStoreSample).

The database context can contain any settings or entities, but to be used as a multitenant store it is only necessary to derive from `EFCoreStoreDbContext`:

```cs
public class AppDbContext : EFCoreStoreDbContext
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

Note, this database context wil have its own connection string (usually) separate from that of any tenant in the store. Addtionally, this database context can be entirely separate from any others an application might use if comingling the multitenant store and app entity models is not desired.

Configure by calling `WithEFCoreStore<TEFCoreStoreDbContext>` after `AddMultiTenant` in the `ConfigureServices` method of the app's `Startup` class and provide types for the store's database context generic parameter:

```cs
// Register to use the database context and TTenantInfo types show above.
services.AddMultiTenant().WithEFCoreStore<AppDbContext>()...
```

The contents of the store can be changed at runtime with `TryAdd`, `TryUpdate`, and `TryRemove` which result in updates to the underlying database:

```cs
// Use service provider or dependenct injection to get the InMemoryStore instance.
var store = serviceProvider.GetService<IMultiTenantStore>();

// Add a new tenant to the store.
var newTenant = new TenantInfo(...);
store.TryAdd(newTenant);

// Update a tenant.
newTenant.ConnectionString = "UpdatedConnectionString";
store.TryUpdate(newTenant);

// Remove a tenant.
store.TryRemove(newTenant.Identifier);
```

## Http Remote Store
Sends the tenant identifier, provided by the multitenant strategy, to an http(s) endpoint to get a `TenantInfo` object in return. The [Http Remote Store Sample](https://github.com/Finbuckle/Finbuckle.MultiTenant/tree/master/samples/ASP.NET%20Core%203/HttpRemoteStoreSample) projects demonstrate this store. This store is usually case insensitive when retrieving tenant information by tenant identifier, but the remote server might be more restrictive.

For a successfully request, the store expects a 200 response code and a json body with properties `Id`, `Identifier`, `Name`, and `ConnectionString` which will be mapped into a `TenantInfo` object.

Any non-200 response code results in a null `TenantInfo`.

This store is read-only and calls to `TryAdd`, `TryUpdate`, and `TryRemove` will throw a `NotImplementedException`.

Configure by calling `WithHttpRemoteStore` after `AddMultiTenant` in the `ConfigureServices` method of the app's `Startup` class. A uri template string must be passed to the method. At runtime the tenant identifier will replace the substring `{__tenant__}` in the uri. If the template provided does not contain `{__tenant__}` it is appended to the template. An overload of `WithHttpRemoteStore` allows for a lambda function to further configure the internal `HttpClient`.

```cs
// This will append the identifier to the provided url.
services.AddMultiTenant()
        .WithHttpRemoteStore("https://remoteserver.com/)...
```

```cs
// This will replace {__tenant__} with the identifier.
services.AddMultiTenant()
        .WithHttpRemoteStore("https://remoteserver.com/{__tenant__}/getinfo)...
```

Use the overload of `WithHttpRemoteStore` to configure the underlying `HttpClient`:
```cs
// This will inject MyCustomHeaderHandler, a DelegatingHandler, to the request pipeline.
services.AddMultiTenant()
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
services.AddMultiTenant()
        .WithHttpRemoteStore("https://remoteserver.com/", httpClientBuilder =>
        {
            httpClientBuilder.AddHttpMessageHander<MyCustomHeaderHandler>();
        });
```

Use the same overload to add resilience and transient fault handling with [Polly](https://www.hanselman.com/blog/AddingResilienceAndTransientFaultHandlingToYourNETCoreHttpClientWithPolly.aspx):
```cs
// This will retry the request if needed.
services.AddMultiTenant()
        .WithHttpRemoteStore("https://remoteserver.com/", httpClientBuilder =>
        {
            httpClientBuilder.AddTransientHttpErrorPolicy(policyBuilder => policyBuilder.RetryAsync(2));
        });
```