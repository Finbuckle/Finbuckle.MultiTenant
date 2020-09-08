# Core Concepts
The library uses standard .NET Core conventions and most of the internal details are abstracted away from app code. However, there are a few important specifics to be aware of.

## IMultiTenantContext
Interface for a type containing information about the current multitenant environment.

* Includes `TenantInfo`, `StrategyInfo`, and `StoreInfo` properties with details on the current tenant, how it was determined, and from where its information was retrieved.
* Can be obtained in ASP.NET Core by calling the `GetMultiTenantContext()` method on the current request's `HttpContext` object. The implementation used with ASP.NET Core middleware has read only properties. The `HttpContext` extension method `TrySetTenantInfo` can be used to manually set the current tenant, but normally the middleware handles this. 
* A custom implementation can be defined for more advanced use cases.

## ITenantInfo and TenantInfo
Contains information about a tenant. Usually an app will get the current `TenantInfo` object from the `MultiTenantContext` instance for that request. Instances of `TenantInfo` can also be passed to multitenant stores for adding, removing, updating the store.

`ITenantInfo` defines properties for `Id`, `Identifier`, `Name`, `ConnectionString`.

* `Id` is a unique id for a tenant in the app and should never change.
* `Identifier` is the value used to actually resolve a tenant and should have a syntax compatible for the app (i.e. no crazy symbols in a web app where the identifier will be part of the URL). Unlike `Id`, `Identifier` can be changed if necessary.
* `Name` is a display name for the tenant.
* `ConnectionString` is a connection string that should be used for database operations for this tenant. It might connect to a shared database or a dedicated database for the single tenant.

`TenantInfo` is a basic implementation of `ITenantInfo` with only the required properties.
An app can define a custom `ITenantInfo` and add any other needed properties.
When calling `AddMultiTenant<T>` the type passed into the type parameter defines the
`ITenantInfo` use through the library and thus the app.

## StrategyInfo
Contains information about the multitenant strategy used to create the `MultiTenantContext`. Accessible as a property on `MultiTenantContext`.

## StoreInfo
Contains information about the multitenant store used to create the `MultiTenantContext`. Accessible as a property on `MultiTenantContext`.

## MultiTenant Strategies
Responsible for determining and returning a tenant identifier string for the current request.
* Several strategies are provided based on host, route, etc. See [MultiTenant Strategies](Strategies) for more information.
* Custom strategies implementing `IMultiTenantStrategy` can be used as well.

## MultiTenant Stores
Responsible for returning a `TenantInfo` object based on a tenant string identifier (which is usually provided by a strategy).
* Has methods for adding, removing, updating, and retrieving `TenantInfo` objects.
* Two implementations are provided: a basic `InMemoryTenantStore` based on `ConcurrentDictionary<string, TenantInfo>` and a more advanced Entity Framework Core based implementation.
* Custom stores implementing `IMultiTenantStore` can be used as well.

## MultiTenantException
Exception type thrown when a serious problem occurs within Finbuckle.MultiTenant.
* Usually wraps an underlying exception.
