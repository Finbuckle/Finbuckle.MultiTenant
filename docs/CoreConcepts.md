# Core Concepts

The library uses standard .NET conventions, and most internal details are abstracted away from app code.
However, there are a few important specifics to be aware of. The items below make up the foundation of the library.

## `ITenantInfo` and `TenantInfo`

A `TenantInfo` instance contains information about a tenant, often the current tenant for an app. The type must implement
`ITenantInfo`, which defines `Id` and `Identifier`. The library provides `TenantInfo`, a basic implementation that also
includes `Name`.

When calling `AddMultiTenant<TTenantInfo>`, the type parameter defines the `ITenantInfo` implementation used throughout
the library and app.

* `Id` is a unique id for a tenant in your app and should never change.
* `Identifier` is the value used to resolve a tenant and should use a syntax appropriate for your app (for example, URL-safe
  characters when it is part of a web address). Unlike `Id`, `Identifier` can be changed if
  necessary.

The library provides `TenantInfo` as a base implementation. Your app can define a custom class that implements
`ITenantInfo` or inherits from `TenantInfo`, adding properties as needed. Keep these
classes lightweight since they are often queried. Keep heavier associated data in an external area that can be pulled in
when needed via the tenant `Id`.

> Previous versions of `TenantInfo` included a connection string property. If needed simply add it to your custom
> `TenantInfo` derived class.

## `MultiTenantContext<TTenantInfo>`

The `MultiTenantContext<TTenantInfo>` contains information about the current tenant.

* Implements `IMultiTenantContext` and `IMultiTenantContext<TTenantInfo>` and can be accessed through
  `IMultiTenantContextAccessor` from dependency injection.
* Includes `TenantInfo`, `StrategyInfo`, and `StoreInfo` properties with details on the current tenant, how it was
  determined, and from where its information was retrieved.
* The `IsResolved` property indicates whether a tenant was successfully resolved for the current context.
* Can be obtained in ASP.NET Core by calling the `GetMultiTenantContext<TTenantInfo>()` method on the current request's `HttpContext`
  object. See [ASP.NET Core Integration](AspNetCore#getting-the-current-tenant-in-aspnet-core) for details.
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
* Several implementations are provided, including in-memory, configuration, EF Core, distributed cache, HTTP remote,
  and echo stores. See [MultiTenant Stores](Stores) for more information.
* Custom stores implementing `IMultiTenantStore` can be used as well.

## MultiTenantException

Exception type thrown when a serious problem occurs within MultiTenant.

* Usually wraps an underlying exception.
