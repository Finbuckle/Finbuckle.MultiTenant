# ASP.NET Core Integration

MultiTenant provides first-class support for ASP.NET Core through a dedicated package that adds middleware,
`HttpContext` helpers, and several ASP.NET Core-specific strategies.

## Package Installation

For most ASP.NET Core apps install the `Finbuckle.MultiTenant.AspNetCore` package:

```bash
dotnet add package Finbuckle.MultiTenant.AspNetCore
```

This package depends on `Finbuckle.MultiTenant` and transitively brings in everything needed for
ASP.NET Core integration, including [ASP.NET Core-specific strategies](#asp-net-core-strategies).

## How the Middleware Works

When `UseMultiTenant<TId>()` is added to the pipeline, the middleware automatically handles tenant resolution
for each HTTP request:

1. When the host creates the `HttpContext` for a request, a fresh ambient tenant scope begins (see
   [The Per-Request Ambient Tenant Scope](#the-per-request-ambient-tenant-scope)). The scope is in effect for the
   whole pipeline, including middleware that runs before `UseMultiTenant<TId>()`, and ends when the `HttpContext`
   is disposed.
2. The middleware calls `ITenantResolver.ResolveAsync(HttpContext)`.
3. The resolver iterates through registered strategies (which receive the `HttpContext`) to find a tenant
   identifier, then queries stores to find a matching `TenantInfo`.
4. If a tenant is found, the middleware sets `TenantInfo` on the `ITenantContext<TTenantInfo, TId>` for the request's
   scope. If earlier middleware already set a tenant, resolution is skipped and that tenant is kept.
5. All services resolved during the request — including `IOptions<T>`, `IOptionsSnapshot<T>`,
   `IOptionsMonitor<T>`, and `IMultiTenantDbContext<TId>` — automatically see the resolved tenant.

This means you never need to manually call `ITenantResolver<TId>` or populate `ITenantContext<TId>` in an ASP.NET Core
app — the middleware wires everything together.

## Configuring the Middleware

### Service Registration

Register MultiTenant services in `Program.cs` using `AddMultiTenant<TTenantInfo, TId>` along with at least one
[strategy](Strategies) and one [store](Stores):

```csharp
using Finbuckle.MultiTenant;

var builder = WebApplication.CreateBuilder(args);

// ...add other app services

builder.Services.AddMultiTenant<TenantInfo, string>()
    .WithHostStrategy()
    .WithConfigurationStore();

var app = builder.Build();
```

See [Configuration and Usage](ConfigurationAndUsage) for more details on `AddMultiTenant` and its builder methods.

### Adding the Middleware

Call `UseMultiTenant<TId>()` to add the middleware to the request pipeline.

```csharp
// add the MultiTenant middleware
app.UseMultiTenant<string>();

// ...add other middleware

app.Run();
```

> **Middleware ordering is important.** Place `UseMultiTenant<TId>()` **before** any middleware that requires
> per-tenant behavior, such as `UseAuthentication()`, `UseAuthorization()`, and any other components that
> read per-tenant options or services.

If `UseRouting()` is explicitly called in the pipeline it must come **before** `UseMultiTenant<TId>()` when
using the [Route Strategy](#route-strategy).

### The Per-Request Ambient Tenant Scope

`AddHttpAmbientTenantScope()` wraps the registered `IHttpContextFactory` with `TenantScopeHttpContextFactory`. The
wrapper begins a new ambient tenant scope when each `HttpContext` is created and ends it when the context is
disposed, so every request starts with its own unresolved tenant scope. This is the same mechanism ASP.NET Core uses
to make `IHttpContextAccessor` available for the whole request.

Every ASP.NET Core builder method (all the `With...Strategy()` methods in this package,
`WithPerTenantAuthentication()`, `BypassWhen()`, `ShortCircuitWhen()`, and their variants) calls
`AddHttpAmbientTenantScope()` for you. Call it yourself only if you configure MultiTenant for ASP.NET Core with
general-purpose methods alone, such as `WithStaticStrategy()` or `WithDelegateStrategy()`:

```csharp
builder.Services.AddMultiTenant<TenantInfo, string>()
    .WithDelegateStrategy(ctx => Task.FromResult<string?>("initech"))
    .WithConfigurationStore()
    .AddHttpAmbientTenantScope();
```

Calling it more than once has no further effect. If it was never called, `UseMultiTenant<TId>()` throws a
`MultiTenantException` at startup. If the middleware runs for a request with no active scope (for example, because
`IHttpContextFactory` was replaced after `AddHttpAmbientTenantScope()`), it throws a `MultiTenantException` for that
request.

### Long-Lived Connections

Some requests stay open for a long time, such as WebSockets (including SignalR over WebSockets), server-sent events,
and streaming gRPC calls. For these, the request's tenant scope lasts as long as the connection:

- **The tenant is resolved once, when the connection opens.** It applies to everything that runs as part of that
  request for the life of the connection, such as each SignalR hub method call or each message on a stream.
- **The tenant can't change on an open connection.** `TenantInfo` can only be set once per scope, so to switch
  tenants the client must open a new connection.
- **The connection request must resolve the tenant.** Make sure the route the connection uses goes through
  `UseMultiTenant<TId>()` and isn't excluded from resolution. Choose a strategy that works for that request: browsers
  can't add custom headers when opening a WebSocket, so the [Header Strategy](Strategies#header-strategy) usually
  won't work there. Route, host, base path, or claim strategies work, as does a query string read with
  `WithHttpContextStrategy()`.
- **Transports that use several requests need the same tenant on each.** Some transports, such as SignalR's long
  polling fallback, spread one logical connection across several HTTP requests, each with its own tenant scope.
  Make sure every one of those requests resolves to the same tenant.
- **Work outside the request doesn't see the scope.** Code that runs outside the connection's request, such as a
  hosted service sending messages through an `IHubContext`, has no ambient tenant scope. Begin one yourself with
  `IServiceProvider.BeginTenantScope(tenantInfo)` before using tenant-aware services there (see
  [.NET Generic Host Integration](GenericHost)).

## ASP.NET Core Strategies

The following strategies are provided by `Finbuckle.MultiTenant.AspNetCore` and work with the ASP.NET Core
request pipeline. See [MultiTenant Strategies](Strategies) for full documentation of all strategies, including
general-purpose strategies available in the base package.

| Strategy | Description |
|---|---|
| [Host Strategy](Strategies#host-strategy) | Uses subdomains or other host segments to identify the tenant. |
| [Route Strategy](Strategies#route-strategy) | Uses a route parameter (e.g. `{__tenant__}`) in the URL path. |
| [Base Path Strategy](Strategies#base-path-strategy) | Uses the first URL path segment as the tenant identifier. |
| [Header Strategy](Strategies#header-strategy) | Uses an HTTP request header to identify the tenant. |
| [Claim Strategy](Strategies#claim-strategy) | Uses a claim on the authenticated user's principal. |
| [Session Strategy](Strategies#session-strategy) | Uses the ASP.NET Core session to store the resolved identifier. |
| [HttpContext Strategy](Strategies#httpcontext-strategy) | Uses a delegate accepting `HttpContext` for custom logic. |
| [Remote Authentication Callback Strategy](Strategies#remote-authentication-callback-strategy) | Used internally by [per-tenant authentication](Authentication) for remote auth flows. |

## Getting the Current Tenant in ASP.NET Core

### `HttpContext` Extension Members

The following extension members are available on `HttpContext` for web apps:

#### `GetTenantContext<TTenantInfo, TId>`

Returns the `ITenantContext<TTenantInfo, TId>` instance for the current request. This is the preferred way to
access the current tenant in ASP.NET Core because it always reflects the state set by the middleware, even
in post-endpoint processing.

```csharp
var tenantInfo = HttpContext.GetTenantContext<TenantInfo, string>().TenantInfo;

if (tenantInfo != null)
{
    var tenantId = tenantInfo.Id;
    var identifier = tenantInfo.Identifier;
}
```

#### `TenantContext<TId>()`

Returns the `ITenantContext<TId>` instance for the current request. This is useful when code does not need
the concrete tenant info type, or when working with shared infrastructure that only depends on `ITenantInfo<TId>`.

```csharp
var tenantContext = HttpContext.TenantContext<string>();

if (tenantContext.IsResolved)
{
    var tenantId = tenantContext.TenantInfo?.Id;
}
```

#### `GetTenantInfo<TTenantInfo, TId>`

A convenience shorthand for `GetTenantContext<TTenantInfo, TId>().TenantInfo`. Returns the current `TTenantInfo`
instance, or null if no tenant was resolved.

```csharp
var tenantInfo = HttpContext.GetTenantInfo<TenantInfo, string>();

if (tenantInfo != null)
{
    Console.WriteLine($"Current tenant: {tenantInfo.Identifier}");
}
```

#### `TenantInfo<TId>()`

A convenience shorthand for `TenantContext<TId>().TenantInfo`. Returns the current `ITenantInfo<TId>` instance, or null if
no tenant was resolved.

```csharp
var tenantInfo = HttpContext.TenantInfo<string>();

if (tenantInfo != null)
{
    Console.WriteLine($"Current tenant: {tenantInfo.Identifier}");
}
```

#### `SetTenantInfo<TTenantInfo, TId>`

For most cases the middleware sets the `TenantInfo` automatically and this method is not needed. Use only if
explicitly overriding the `TenantInfo` set by the middleware.

Sets the current tenant to the provided `TenantInfo` on the request's `ITenantContext<TTenantInfo, TId>`.

> **Important:** `TenantInfo` can only be set once. Attempting to call `SetTenantInfo` after the tenant has
> already been resolved will throw a `MultiTenantException`. Use `TrySetTenantInfo` if you need to conditionally
> set the tenant only when none has been resolved yet.

```csharp
var newTenantInfo = new TenantInfo { Id = "new-id", Identifier = "new-identifier" };

HttpContext.SetTenantInfo(newTenantInfo);

// This will be the new tenant.
var tenant = HttpContext.GetTenantContext<TenantInfo, string>().TenantInfo;
```

#### `TrySetTenantInfo<TTenantInfo, TId>`

Sets the current tenant only if one has not already been resolved. This is useful when your code may be called
from multiple paths and you want to avoid the exception thrown by `SetTenantInfo` when a tenant already exists.

```csharp
var fallbackTenant = new TenantInfo { Id = "default", Identifier = "default" };

// Only takes effect if no tenant was resolved by the middleware.
HttpContext.TrySetTenantInfo<TenantInfo, string>(fallbackTenant);
```

> For dependency injection-based access to the current tenant outside of a controller or middleware, see
> `ITenantContext` in [Configuration and Usage](ConfigurationAndUsage#getting-the-current-tenant).

## Bypassing Tenant Resolution

Bypassing prevents the middleware from performing tenant resolution for a request entirely and passes it
directly to the next middleware in the pipeline.

> Bypassing runs **before** tenant resolution. [Short circuiting](#short-circuiting) runs **after** resolution.

### Exclude Specific Endpoints

For scenarios where individual, known endpoints should never trigger tenant resolution, annotate them at
registration time.

Using the `IEndpointConventionBuilder` extension `ExcludeFromMultiTenantResolution`:

```csharp
var app = builder.Build();

// Exclude OpenApi endpoints.
app.MapOpenApi()
    .ExcludeFromMultiTenantResolution();

// Exclude a specific endpoint.
app.MapGet("/oops", () => "Oops! An error happened.")
    .ExcludeFromMultiTenantResolution();

// Exclude a group of endpoints.
app.MapGroup("api/v{version:apiVersion}/dashboard")
    .ExcludeFromMultiTenantResolution();

// Exclude static asset endpoints.
app.MapStaticAssets()
    .ExcludeFromMultiTenantResolution();

app.Run();
```

Using the `ExcludeFromMultiTenantResolutionAttribute` attribute on controllers and action methods:

```csharp
// Exclude an entire controller.
[ExcludeFromMultiTenantResolution]
public class DashboardController : Controller
{
    // Or exclude just a specific action.
    [ExcludeFromMultiTenantResolution]
    public ActionResult Index()
    {
        return View();
    }
}
```

### Bypass When No Endpoint Is Matched

Call `BypassWhenEndpointNotResolved()` after `AddMultiTenant<TTenantInfo, TId>` to skip tenant resolution
entirely when the current request has no matched endpoint. This is especially useful when requests arrive
that do not match any route.

> This bypass does **not** require the route strategy.

```csharp
builder.Services.AddMultiTenant<TenantInfo, string>()
    .WithRouteStrategy()
    .WithConfigurationStore()
    .BypassWhenEndpointNotResolved();
```

### Bypass When a Custom Condition Is Met

Use `BypassWhen()` to bypass resolution based on any condition derived from `HttpContext`:

```csharp
// Bypass resolution for health check requests.
builder.Services.AddMultiTenant<TenantInfo,string>()
    .WithRouteStrategy()
    .WithConfigurationStore()
    .BypassWhen(options =>
    {
        options.Predicate = ctx => ctx.Request.Path.StartsWithSegments("/health");
    });
```

## Short Circuiting

The `MultiTenantMiddleware` can be configured to short circuit a request pipeline when no tenant is found
or when some custom condition is met.

> Short-circuiting the request pipeline does not return an HTTP error code — it simply stops calling further middleware.

### Short Circuit When Tenant Not Resolved

Call `ShortCircuitWhenTenantNotResolved()` after `AddMultiTenant<TTenantInfo, TId>` to halt further processing
when no tenant can be found. An overload accepts a URI to redirect to when no tenant is found.

> If you short circuit when tenant not resolved, and you have endpoints that do not require a tenant,
> then [excluding those endpoints](#exclude-specific-endpoints) becomes a necessity; otherwise they would
> never be reached.

```csharp
// Simply short circuit the request, ending request handling.
builder.Services.AddMultiTenant<TenantInfo, string>()
    .WithHostStrategy()
    .WithConfigurationStore()
    .ShortCircuitWhenTenantNotResolved();

// Short circuit and redirect to a specific Uri.
builder.Services.AddMultiTenant<TenantInfo, string>()
    .WithHostStrategy()
    .WithConfigurationStore()
    .ShortCircuitWhenTenantNotResolved(new Uri("/tenant/notfound", UriKind.Relative));
```

### Short Circuit When a Custom Condition Is Met

Use the `ShortCircuitWhen()` extension on `MultiTenantBuilder<TTenantInfo, TId>` for advanced short-circuiting:

```csharp
// Advanced short circuiting: if tenant not resolved.
builder.Services.AddMultiTenant<TenantInfo, string>()
    .WithHostStrategy()
    .WithConfigurationStore()
    .ShortCircuitWhen(config =>
    {
        config.Predicate = context => !context.IsResolved;
    });

// Including a redirect.
builder.Services.AddMultiTenant<TenantInfo, string>()
    .WithHostStrategy()
    .WithConfigurationStore()
    .ShortCircuitWhen(config =>
    {
        config.Predicate = context => !context.IsResolved;
        config.RedirectTo = new Uri("/tenant/notfound", UriKind.Relative);
    });
```

## Per-Tenant Authentication

MultiTenant provides built-in support for isolating tenant authentication so that login sessions are
scoped to the current tenant. This includes per-tenant cookie validation, challenge schemes, login/logout
paths, and OpenID Connect settings.

Authentication is configured by calling `WithPerTenantAuthentication()` after `AddMultiTenant<TTenantInfo, TId>()`:

```csharp
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie()
    .AddOpenIdConnect();

builder.Services.AddMultiTenant<TenantInfo, string>()
    .WithRouteStrategy()
    .WithConfigurationStore()
    .WithPerTenantAuthentication();
```

> Place `UseMultiTenant<TId>()` **before** `UseAuthentication()` so the tenant is resolved before authentication
> runs.

See [Per-Tenant Authentication](Authentication) for full details, including OpenID Connect configuration,
per-tenant cookie names, and JWT bearer customization.

## Per-Tenant Options

MultiTenant integrates with the standard ASP.NET Core Options pattern so that options can be configured
differently for each tenant. Once the middleware sets the tenant for a request, `IOptions<T>`,
`IOptionsSnapshot<T>`, and `IOptionsMonitor<T>` all return the correct per-tenant values automatically.
Any options class used by ASP.NET Core or your own code can be customized per tenant with minimal changes.

> `IOptionsMonitor<T>` is registered as a **scoped** service in MultiTenant (unlike standard .NET where it
> is a singleton), but it preserves the same change-notification and cache-invalidation behavior.

```csharp
builder.Services.ConfigurePerTenant<MyOptions, TenantInfo>((options, tenantInfo) =>
{
    options.Setting = tenantInfo.Identifier;
});
```

See [Per-Tenant Options](Options) for full details.

## Data Isolation

### Entity Framework Core

MultiTenant can automatically filter EF Core queries by tenant using a global query filter. This removes
the need to add `WHERE TenantId = ...` clauses throughout your app. Use `AddMultiTenantDbContext<T, TId>()` for
scoped contexts or `AddPooledMultiTenantDbContext<T, TId>()` for high-throughput pooled contexts:

```csharp
builder.Services.AddMultiTenantDbContext<AppDbContext, string>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));
```

See [Data Isolation with Entity Framework Core](EFCore) for full details.

### ASP.NET Core Identity

MultiTenant has dedicated support for data isolation when using ASP.NET Core Identity with EF Core,
including multi-tenant identity context base classes and support for passkeys (WebAuthn).

See [Data Isolation with ASP.NET Core Identity](Identity) for full details.

## Important Considerations

- **Middleware ordering matters.** Place `UseMultiTenant<TId>()` before `UseAuthentication()`, `UseAuthorization()`,
  and any middleware that reads per-tenant options or services.
- **`TenantInfo` can only be set once** per request. The middleware sets it early in the pipeline. If you
  need to choose the tenant yourself, call `HttpContext.SetTenantInfo<TTenantInfo, TId>()` or
  `TrySetTenantInfo<TTenantInfo, TId>()` in middleware placed before `UseMultiTenant<TId>()`; the middleware then
  skips resolution and keeps that tenant.
- **`ITenantContext` is ambient.** The current tenant is held in an `AsyncLocal`, not a scoped DI service. A fresh
  ambient tenant scope begins when each request's `HttpContext` is created (see
  [The Per-Request Ambient Tenant Scope](#the-per-request-ambient-tenant-scope)) and the middleware sets the resolved
  tenant on it, so each request sees its own tenant automatically.
- **Long-lived connections keep the tenant they opened with.** WebSockets, SignalR, server-sent events, and
  streaming gRPC calls keep their request's tenant for the life of the connection. See
  [Long-Lived Connections](#long-lived-connections).
- **`IOptionsMonitor<T>` is scoped** in MultiTenant. Do not capture it in a singleton service.
- **Not all strategies work for all scenarios.** The [Claim Strategy](Strategies#claim-strategy) needs
  authentication middleware to run first. The [Route Strategy](Strategies#route-strategy) requires
  `UseRouting()` before `UseMultiTenant<TId>()`.
- **Per-tenant options require `ITenantContext.TenantInfo` to be set.** If tenant resolution fails (no
  strategy finds an identifier or no store matches), the `TenantInfo` will be null and default (non-tenant)
  options will be used.

## See Also

- [Configuration and Usage](ConfigurationAndUsage) — registration and resolver details
- [Core Concepts](CoreConcepts) — `ITenantContext<TId>` and the ambient tenant scope
- [.NET Generic Host Integration](GenericHost) — using MultiTenant in non-web apps
- [Per-Tenant Authentication](Authentication) — full authentication setup
- [Per-Tenant Options](Options) — options customization
- [EF Core Data Isolation](EFCore) — shared and separate database patterns
