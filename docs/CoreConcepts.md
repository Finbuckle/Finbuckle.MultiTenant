# Core Concepts

The library uses standard .NET Core conventions and most of the internal details are abstracted away from app code.
However, there are a few important specifics to be aware of. The items below make up the foundation of the library.

## `ITenantInfo` and `TenantInfo`

A `TenantInfo` instance contains information about a tenant. Often this will be the "current" tenant in the context an
app. These instances' type must implement `ITenantInfo` which defines properties
for `Id` and `Identifier`. `TenantInfo` is a basic implementation provided by the library which also includes a `Name` property.

When calling `AddMultiTenant<TTenantInfo>` the type passed into the
type parameter defines the `ITenantInfo` implementation used throughout the library and app.

* `Id` is a unique id for a tenant in your app and should never change.
* `Identifier` is the value used to actually resolve a tenant and should have a syntax compatible for your app (i.e. no
  crazy symbols in a web app where the identifier will be part of the URL). Unlike `Id`, `Identifier` can be changed if
  necessary.

The library provides `TenantInfo` as a base implementation. Your app can and should define a custom class implementing `ITenantInfo` (or inheriting from `TenantInfo`) and add custom 
properties as needed. It is recommended to keep these
classes lightweight since they are often queried. Keep heavier associated data in an external area that can be pulled in
when needed via the tenant `Id`.

> Previous versions of `TenantInfo` included a connection string property. If needed simply add it to your custom
> `TenantInfo` derived class.

## `MultiTenantContext<TTenantInfo>`

The `MultiTenantContext<TTenantInfo>` contains information about the current tenant.

* Implements `IMultiTenantContext` and `IMultiTenantContext<TTenantInfo>` which can be obtained from dependency injection.
* Includes `TenantInfo`, `StrategyInfo`, and `StoreInfo` properties with details on the current tenant, how it was
  determined, and from where its information was retrieved.
* The `IsResolved` property indicates whether a tenant was successfully resolved for the current context.
* Can be obtained in ASP.NET Core by calling the `GetMultiTenantContext()` method on the current request's `HttpContext`
  object.
* The `HttpContext` extension method `SetTenantInfo` can be used to manually set the current tenant, but normally the middleware handles this.
* A custom implementation can be defined for advanced use cases.

> In the original v10 release the `TenantInfo` property was immutable, but this change was reverted. It is 
> recommended that you only mutate the `TenantInfo` property with extreme care.

## MultiTenant Strategies

Responsible for determining and returning a tenant identifier string for the current request.

* Several strategies are provided based on host, route, etc. See [MultiTenant Strategies](Strategies) for more
  information.
* Custom strategies implementing `IMultiTenantStrategy` can be used as well.

## MultiTenant Stores

Responsible for returning a `TenantInfo` object based on a tenant string identifier (which is usually provided by a
strategy).

* Has methods for adding, removing, updating, and retrieving `TenantInfo` objects.
* Two implementations are provided: a basic `InMemoryTenantStore` based on `ConcurrentDictionary<string, TenantInfo>`
  and a more advanced Entity Framework Core based implementation.
* Custom stores implementing `IMultiTenantStore` can be used as well.

## MultiTenantException

Exception type thrown when a serious problem occurs within MultiTenant.

* Usually wraps an underlying exception.
