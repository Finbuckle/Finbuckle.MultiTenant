# Core Concepts

The library uses standard ASP.Net Core conventions and most of the internal details are abstracted away from app code. However, there are a few important specifics to be aware of.

## MultiTenantContext
Contains information about a the current multitenant environment. The `MultiTenantContext` for the current request can be obtained by calling the `GetMultiTenantContext()` method on the `HttpContext` object.

* Includes `TenantInfo`, `StrategyInfo`, and `StoreInfo` properties with details on the current tenant, how it was determined, and where its information came from.

## TenantInfo
Contains information about a tenant. Usually an app will get the current `TenantInfo` object from the `MultiTenantContext` instance for that request. Instances of `TenantInfo` can also be passed to multitenant stores for adding, removing, updating the store.

Includes properties for `Id`, `Identifier`, `Name`, `ConnectionString`, and `Items`.

* `Id` is a unique id for a tenant in the app and should never change.
* `Identifier` is the value used to actually resolve a tenant and should have a syntax compatible for the app (i.e. no crazy symbols in a web app where the identifier will be part of the URL). Unlike `Id`, `Identifier` can be changed if necessary.
* `Name` is a display name for the tenant.
* `ConnectionString` is a connection string that should be used for database operations for this tenant. It might connect to a shared database or a dedicated database for the single tenant.
* The `Items` object is a general purpose `IDictionary<string, object>` container.

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
