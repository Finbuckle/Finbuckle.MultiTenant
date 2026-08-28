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

> In the original v10 release `TenantInfo` was a record and `ITenantInfo` was removed to enforce immutability. This
> was reverted in 10.0.2 in favor of the interface contract described below, which the library enforces on its side
> and proves by test for both mutable and immutable implementations.

### Tenant info contract

`ITenantInfo` is the only contract the library depends on. It is **read-only from the library's point of view**: the
library never writes to a tenant info instance after it has been constructed and never forces a particular shape onto
your type. All of the following are supported and covered by the test suite:

```csharp
// plain mutable properties
public class AppTenantInfo : ITenantInfo
{
    public string Id { get; set; } = string.Empty;
    public string Identifier { get; set; } = string.Empty;
    public string? Name { get; set; }
}

// init-only properties (the shape of the built-in TenantInfo)
public class AppTenantInfo : TenantInfo
{
    public string? ConnectionString { get; init; }
}

// constructor-only, fully immutable, not derived from TenantInfo and not a record
public sealed class AppTenantInfo(string id, string identifier, string? name = null) : ITenantInfo
{
    public string Id { get; } = id;
    public string Identifier { get; } = identifier;
    public string? Name { get; } = name;
}
```

In return the library asks for one rule: **`Id` and `Identifier` must not change for the lifetime of an instance once
it has been handed to the library** (stored, cached, resolved, or assigned to a multi-tenant context). Tenant identity
is `Id` equality; built-in stores match `Identifier` case-insensitively by default. To change a tenant, create a new
instance and pass it to the store's `UpdateAsync`. Mutating other properties of your own type is your business, but keep
in mind the same instance may be shared by caches and by the current request.

The library enforces its side of the contract:

* `IMultiTenantContext` and `MultiTenantContext<TTenantInfo>` are init-only; the current tenant of a request is changed
  only by assigning a new context (for example through `HttpContext.SetTenantInfo`), never by mutating the current
  instance.
* Stores that must construct tenant instances themselves (`ConfigurationStore` and `EchoStore`) accept a factory for
  constructor-only types and throw a clear `MultiTenantException` when a type cannot be constructed. See
  [MultiTenant Stores](Stores) for details.

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
