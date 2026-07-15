# Core Concepts

The library uses standard .NET Core conventions and most of the internal details are abstracted away from app code.
However, there are a few important specifics to be aware of. The items below make up the foundation of the library.

## `ITenantInfo` and `TenantInfo`

A `TenantInfo` instance contains information about a tenant. Often this will be the "current" tenant in the context of an
app. The type of these instances must implement `ITenantInfo` which defines properties
for `Id` and `Identifier`. `TenantInfo` is a basic implementation provided by the library with those two properties.

When calling `AddMultiTenant<TTenantInfo>` the type passed into the
type parameter defines the `ITenantInfo` implementation used throughout the library and app.

* `Id` is a unique id for a tenant in your app and should never change.
* `Identifier` is the value used to actually resolve a tenant and should have a syntax compatible for your app (i.e. no
  crazy symbols in a web app where the identifier will be part of the URL). Unlike `Id`, `Identifier` can be changed if
  necessary.

The library provides `TenantInfo` as a base implementation. Your app can and should define a custom class implementing
`ITenantInfo` (or inheriting from `TenantInfo`) and add custom properties as needed. It is recommended to keep these
classes lightweight since they are often queried. Keep heavier associated data in an external area that can be pulled in
when needed via the tenant `Id`.

> Previous versions of `TenantInfo` included a connection string property. If needed simply add it to your custom
> `TenantInfo` derived class.

## `ITenantContext` and `TenantContext<TTenantInfo>`

The `TenantContext<TTenantInfo>` contains information about the current tenant.

* Implements `ITenantContext` and `ITenantContext<TTenantInfo>` which can be obtained from dependency injection.
* Registered as a **scoped service** (`AddScoped`), so each DI scope (e.g. each HTTP request in ASP.NET Core)
  gets its own `TenantContext<TTenantInfo>` instance. All services resolved within the same scope see the
  same tenant.
* What constitutes a scope is application-specific: in ASP.NET Core it corresponds to a web request; in a
  .NET Generic Host worker service it may correspond to a single message dequeued and processed; in a
  console app it could be a manually created scope. The `TenantContext` lifecycle always matches the DI
  scope it was resolved from.
* The DI scope itself is **not** per-tenant — a single scope serves one request for whichever tenant was
  resolved. The `TenantContext` is simply a scoped service within that scope, not a separate tenant-bound
  container.
* The `TenantInfo` property holds the current tenant info. It **can only be set once** — attempting to set it
  a second time throws a `MultiTenantException`. Setting `TenantInfo` also clears the `Items` dictionary.
* The `Items` property is a `Dictionary<object, object>` for storing arbitrary data scoped to the current
  tenant context. Use it for per-request middleware data, tokens, or other transient state. Items are lost
  when the scope ends or when `TenantInfo` is set.
* The `IsResolved` property indicates whether a tenant was successfully resolved for the current context.
* Can be obtained in ASP.NET Core from the current request's `HttpContext` object with
  `GetTenantContext<TTenantInfo>()` or the non-generic `TenantContext` extension property. See
  [ASP.NET Core Integration](AspNetCore#getting-the-current-tenant-in-aspnet-core) for details.
* The `HttpContext` extension method `SetTenantInfo` can be used to manually set the current tenant, but normally the
  middleware handles this. Use `TrySetTenantInfo` if you need to set only when no tenant has been resolved yet.
* A custom implementation can be defined for advanced use cases.

## MultiTenant Strategies

Responsible for determining and returning a tenant identifier string for the current request.

* Several strategies are provided based on host, route, etc. See [MultiTenant Strategies](Strategies) for more
  information.
* Custom strategies implementing `IMultiTenantStrategy` can be used as well.

## MultiTenant Stores

Responsible for returning a `TenantInfo` object based on a tenant string identifier (which is usually provided by a
strategy).

* Has methods for adding, removing, updating, and retrieving `TenantInfo` objects.
* Several implementations are provided, including a basic lock-protected `InMemoryStore` and a more advanced Entity
  Framework Core based implementation.
* Custom stores implementing `IMultiTenantStore` can be used as well.

## MultiTenantException

Exception type thrown when a serious problem occurs within MultiTenant.

* Usually wraps an underlying exception.

## See Also

- [Configuration and Usage](ConfigurationAndUsage) — service registration and resolution
- [Getting Started](GettingStarted) — quick start walkthrough
- [ASP.NET Core Integration](AspNetCore) — ASP.NET Core-specific details
- [.NET Generic Host Integration](GenericHost) — non-web scenarios
- [MultiTenant Strategies](Strategies) — all built-in strategies
- [MultiTenant Stores](Stores) — all built-in stores
