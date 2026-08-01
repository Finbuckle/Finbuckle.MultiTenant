# Core Concepts

The library uses standard .NET Core conventions and most of the internal details are abstracted away from app code.
However, there are a few important specifics to be aware of. The items below make up the foundation of the library.

## `ITenantInfo` and `TenantInfo`

A `TenantInfo` instance contains information about a tenant. Often this will be the "current" tenant in the context of an
app. The type of these instances must implement `ITenantInfo<TId>` which defines properties for `Id` (of type `TId`) and
`Identifier` (a `string`). `TenantInfo<TId>` is a basic implementation provided by the library with those two properties.

`AddMultiTenant<TTenantInfo, TId>` takes two type parameters: `TTenantInfo` (the `ITenantInfo<TId>` implementation used
throughout the library and app) and `TId` (the type of the tenant `Id`). `TId` must be `IEquatable<TId>`, so common
choices are `string`, `int`, and `Guid`. For example, `AddMultiTenant<TenantInfo, string>()` uses string ids while
`AddMultiTenant<AppTenantInfo, Guid>()` uses `Guid` ids.

* `Id` is a unique id for a tenant in your app and should never change. It must be non-default: `default(TId)` — `0`,
  `Guid.Empty`, or a null/whitespace `string` — is treated as *no tenant* and is rejected (a `MultiTenantException` is
  thrown when adding, updating, resolving, or assigning such a tenant, and an `ArgumentException` when looking one up by
  a default id).
* `Identifier` is the value used to actually resolve a tenant and should have a syntax compatible for your app (i.e. no
  crazy symbols in a web app where the identifier will be part of the URL). It must be non-empty. Unlike `Id`,
  `Identifier` can be changed if necessary.

The library provides `TenantInfo` as a base implementation. Your app can and should define a custom class implementing
`ITenantInfo` (or inheriting from `TenantInfo`) and add custom properties as needed. It is recommended to keep these
classes lightweight since they are often queried. Keep heavier associated data in an external area that can be pulled in
when needed via the tenant `Id`.

> Previous versions of `TenantInfo` included a connection string property. If needed simply add it to your custom
> `TenantInfo` derived class.

## `ITenantContext` and the ambient tenant scope

`ITenantContext<TTenantInfo, TId>` (and its non-generic base `ITenantContext<TId>`) holds the current tenant. The
default implementation is `AmbientTenantContext<TTenantInfo, TId>`, obtained from dependency injection.

* The context is registered as a **singleton** but is **ambient** — the current tenant is stored in an
  `AsyncLocal`, so it flows with the current asynchronous execution context rather than with the DI scope. Each
  logical unit of work (an HTTP request, a dequeued message, a console run) gets its own ambient tenant by first
  calling `BeginScope()` / `BeginTenantScope()`.
* You establish a scope with `ITenantScopeProvider.BeginScope()` or, more commonly, the
  `IServiceProvider.BeginTenantScope(tenantInfo)` extension, which begins a scope and sets the tenant in one call.
  In ASP.NET Core the middleware does this automatically for each request; in other hosts you call it yourself (see
  [.NET Generic Host Integration](GenericHost)).
* Accessing `TenantInfo` before a scope has been established throws a `MultiTenantException`
  (`"No ambient tenant scope has been established."`).
* The `TenantInfo` property holds the current tenant info. Within a scope it **can only be set once** — attempting to
  set it a second time throws a `MultiTenantException`. A tenant assigned to the context must be valid: its `Id` must be
  non-default and its `Identifier` non-empty (see [`ITenantInfo` and `TenantInfo`](#itenantinfo-and-tenantinfo)).
* The `IsResolved` property indicates whether a tenant has been resolved (`TenantInfo` is not null).
* Can be obtained in ASP.NET Core from the current request's `HttpContext` with
  `GetTenantContext<TTenantInfo, TId>()` or the non-generic `TenantContext<TId>()` extension. See
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
