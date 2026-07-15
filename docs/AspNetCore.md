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

When `UseMultiTenant()` is added to the pipeline, the middleware automatically handles tenant resolution
for each HTTP request:

1. For every request, the middleware calls `ITenantResolver.ResolveAsync(HttpContext)`.
2. The resolver iterates through registered strategies (which receive the `HttpContext`) to find a tenant
   identifier, then queries stores to find a matching `TenantInfo`.
3. If a tenant is found, the middleware sets `TenantInfo` on the scoped `ITenantContext<TTenantInfo>` for
   that request.
4. All services resolved within the request's DI scope — including `IOptions<T>`, `IOptionsSnapshot<T>`,
   `IOptionsMonitor<T>`, and `IMultiTenantDbContext` — automatically see the resolved tenant.

This means you never need to manually call `ITenantResolver` or populate `ITenantContext` in an ASP.NET Core
app — the middleware wires everything together.

## Configuring the Middleware

### Service Registration

Register MultiTenant services in `Program.cs` using `AddMultiTenant<TTenantInfo>` along with at least one
[strategy](Strategies) and one [store](Stores):

```csharp
using Finbuckle.MultiTenant;

var builder = WebApplication.CreateBuilder(args);

// ...add other app services

builder.Services.AddMultiTenant<TenantInfo>()
    .WithHostStrategy()
    .WithConfigurationStore();

var app = builder.Build();
```

See [Configuration and Usage](ConfigurationAndUsage) for more details on `AddMultiTenant` and its builder methods.

### Adding the Middleware

Call `UseMultiTenant()` to add the middleware to the request pipeline.

```csharp
// add the MultiTenant middleware
app.UseMultiTenant();

// ...add other middleware

app.Run();
```

> **Middleware ordering is important.** Place `UseMultiTenant()` **before** any middleware that requires
> per-tenant behavior, such as `UseAuthentication()`, `UseAuthorization()`, and any other components that
> read per-tenant options or services.

If `UseRouting()` is explicitly called in the pipeline it must come **before** `UseMultiTenant()` when
using the [Route Strategy](#route-strategy).

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

#### `GetTenantContext<TTenantInfo>`

Returns the `ITenantContext<TTenantInfo>` instance for the current request. This is the preferred way to
access the current tenant in ASP.NET Core because it always reflects the state set by the middleware, even
in post-endpoint processing.

```csharp
var tenantInfo = HttpContext.GetTenantContext<TenantInfo>().TenantInfo;

if (tenantInfo != null)
{
    var tenantId = tenantInfo.Id;
    var identifier = tenantInfo.Identifier;
}
```

#### `TenantContext`

Returns the non-generic `ITenantContext` instance for the current request. This is useful when code does not need
the concrete tenant info type, or when working with shared infrastructure that only depends on `ITenantInfo`.

```csharp
var tenantContext = HttpContext.TenantContext;

if (tenantContext.IsResolved)
{
    var tenantId = tenantContext.TenantInfo?.Id;
}
```

#### `GetTenantInfo<TTenantInfo>`

A convenience shorthand for `GetTenantContext<TTenantInfo>().TenantInfo`. Returns the current `TTenantInfo`
instance, or null if no tenant was resolved.

```csharp
var tenantInfo = HttpContext.GetTenantInfo<TenantInfo>();

if (tenantInfo != null)
{
    Console.WriteLine($"Current tenant: {tenantInfo.Identifier}");
}
```

#### `CurrentTenant`

A convenience shorthand for `TenantContext.TenantInfo`. Returns the current `ITenantInfo` instance, or null if
no tenant was resolved.

```csharp
var tenantInfo = HttpContext.CurrentTenant;

if (tenantInfo != null)
{
    Console.WriteLine($"Current tenant: {tenantInfo.Identifier}");
}
```

#### `SetTenantInfo<TTenantInfo>`

For most cases the middleware sets the `TenantInfo` automatically and this method is not needed. Use only if
explicitly overriding the `TenantInfo` set by the middleware.

Sets the current tenant to the provided `TenantInfo` on the request's `ITenantContext<TTenantInfo>`.

> **Important:** `TenantInfo` can only be set once. Attempting to call `SetTenantInfo` after the tenant has
> already been resolved will throw a `MultiTenantException`. Use `TrySetTenantInfo` if you need to conditionally
> set the tenant only when none has been resolved yet.

```csharp
var newTenantInfo = new TenantInfo { Id = "new-id", Identifier = "new-identifier" };

HttpContext.SetTenantInfo(newTenantInfo);

// This will be the new tenant.
var tenant = HttpContext.GetTenantContext<TenantInfo>().TenantInfo;
```

#### `TrySetTenantInfo<TTenantInfo>`

Sets the current tenant only if one has not already been resolved. This is useful when your code may be called
from multiple paths and you want to avoid the exception thrown by `SetTenantInfo` when a tenant already exists.

```csharp
var fallbackTenant = new TenantInfo { Id = "default", Identifier = "default" };

// Only takes effect if no tenant was resolved by the middleware.
HttpContext.TrySetTenantInfo(fallbackTenant);
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

Call `BypassWhenEndpointNotResolved()` after `AddMultiTenant<TTenantInfo>` to skip tenant resolution
entirely when the current request has no matched endpoint. This is especially useful when requests arrive
that do not match any route.

> This bypass does **not** require the route strategy.

```csharp
builder.Services.AddMultiTenant<TenantInfo>()
    .WithRouteStrategy()
    .WithConfigurationStore()
    .BypassWhenEndpointNotResolved();
```

### Bypass When a Custom Condition Is Met

Use `BypassWhen()` to bypass resolution based on any condition derived from `HttpContext`:

```csharp
// Bypass resolution for health check requests.
builder.Services.AddMultiTenant<TenantInfo>()
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

Call `ShortCircuitWhenTenantNotResolved()` after `AddMultiTenant<TTenantInfo>` to halt further processing
when no tenant can be found. An overload accepts a URI to redirect to when no tenant is found.

> If you short circuit when tenant not resolved, and you have endpoints that do not require a tenant,
> then [excluding those endpoints](#exclude-specific-endpoints) becomes a necessity; otherwise they would
> never be reached.

```csharp
// Simply short circuit the request, ending request handling.
builder.Services.AddMultiTenant<TenantInfo>()
    .WithHostStrategy()
    .WithConfigurationStore()
    .ShortCircuitWhenTenantNotResolved();

// Short circuit and redirect to a specific Uri.
builder.Services.AddMultiTenant<TenantInfo>()
    .WithHostStrategy()
    .WithConfigurationStore()
    .ShortCircuitWhenTenantNotResolved(new Uri("/tenant/notfound", UriKind.Relative));
```

### Short Circuit When a Custom Condition Is Met

Use the `ShortCircuitWhen()` extension on `MultiTenantBuilder<TTenantInfo>` for advanced short-circuiting:

```csharp
// Advanced short circuiting: if tenant not resolved.
builder.Services.AddMultiTenant<TenantInfo>()
    .WithHostStrategy()
    .WithConfigurationStore()
    .ShortCircuitWhen(config =>
    {
        config.Predicate = context => !context.IsResolved;
    });

// Including a redirect.
builder.Services.AddMultiTenant<TenantInfo>()
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

Authentication is configured by calling `WithPerTenantAuthentication()` after `AddMultiTenant<TTenantInfo>()`:

```csharp
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie()
    .AddOpenIdConnect();

builder.Services.AddMultiTenant<TenantInfo>()
    .WithRouteStrategy()
    .WithConfigurationStore()
    .WithPerTenantAuthentication();
```

> Place `UseMultiTenant()` **before** `UseAuthentication()` so the tenant is resolved before authentication
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
the need to add `WHERE TenantId = ...` clauses throughout your app. Use `AddMultiTenantDbContext<T>()` for
scoped contexts or `AddPooledMultiTenantDbContext<T>()` for high-throughput pooled contexts:

```csharp
builder.Services.AddMultiTenantDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));
```

See [Data Isolation with Entity Framework Core](EFCore) for full details.

### ASP.NET Core Identity

MultiTenant has dedicated support for data isolation when using ASP.NET Core Identity with EF Core,
including multi-tenant identity context base classes and support for passkeys (WebAuthn).

See [Data Isolation with ASP.NET Core Identity](Identity) for full details.

## Important Considerations

- **Middleware ordering matters.** Place `UseMultiTenant()` before `UseAuthentication()`, `UseAuthorization()`,
  and any middleware that reads per-tenant options or services.
- **`TenantInfo` can only be set once** per request. The middleware sets it early in the pipeline. If you
  need to override it, use `HttpContext.SetTenantInfo()` or `TrySetTenantInfo()` before any tenant-aware
  services are resolved.
- **`ITenantContext` is scoped.** Each HTTP request gets its own instance. A new scope is created
  per request by the ASP.NET Core framework, so this happens automatically.
- **`IOptionsMonitor<T>` is scoped** in MultiTenant. Do not capture it in a singleton service.
- **Not all strategies work for all scenarios.** The [Claim Strategy](Strategies#claim-strategy) needs
  authentication middleware to run first. The [Route Strategy](Strategies#route-strategy) requires
  `UseRouting()` before `UseMultiTenant()`.
- **Per-tenant options require `ITenantContext.TenantInfo` to be set.** If tenant resolution fails (no
  strategy finds an identifier or no store matches), the `TenantInfo` will be null and default (non-tenant)
  options will be used.

## See Also

- [Configuration and Usage](ConfigurationAndUsage) — registration and resolver details
- [Core Concepts](CoreConcepts) — `ITenantContext`, `TenantContext`, and scoped lifetime
- [.NET Generic Host Integration](GenericHost) — using MultiTenant in non-web apps
- [Per-Tenant Authentication](Authentication) — full authentication setup
- [Per-Tenant Options](Options) — options customization
- [EF Core Data Isolation](EFCore) — shared and separate database patterns
