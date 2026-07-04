# ASP.NET Core Integration

MultiTenant provides first-class support for ASP.NET Core through a dedicated package that adds middleware,
`HttpContext` helpers, and several ASP.NET Core-specific strategies.

## Package Installation

For most ASP.NET Core apps install the `Finbuckle.MultiTenant.AspNetCore` package:

```bash
dotnet add package Finbuckle.MultiTenant.AspNetCore
```

This package depends on `Finbuckle.MultiTenant` and transitively brings in everything needed for
ASP.NET Core integration, including [ASP.NET Core-specific strategies](#aspnet-core-strategies).

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

### `HttpContext` Extension Methods

The following extension methods are available on `HttpContext` for web apps:

#### `GetTenantContext<TTenantInfo>`

Returns the `ITenantContext<TTenantInfo>` instance for the current request. This is the preferred way to
access the current tenant in ASP.NET Core because it always reflects the state set by the middleware, even
in post-endpoint processing.

Internally `ITenantContext<TTenantInfo>` is registered as a **scoped service**, so each HTTP request gets its own
instance. Once the middleware resolves the tenant, `TenantInfo` is set on that instance and remains available
throughout the request's DI scope.

```csharp
var tenantInfo = HttpContext.GetTenantContext<TenantInfo>().TenantInfo;

if (tenantInfo != null)
{
    var tenantId = tenantInfo.Id;
    var identifier = tenantInfo.Identifier;
    var name = tenantInfo.Name;
}
```

#### `SetTenantInfo<TTenantInfo>`

For most cases the middleware sets the `TenantInfo` automatically and this method is not needed. Use only if
explicitly overriding the `TenantInfo` set by the middleware.

Sets the current tenant to the provided `TenantInfo` on the request's `ITenantContext<TTenantInfo>`.

```csharp
var newTenantInfo = new TenantInfo { Id = "new-id", Identifier = "new-identifier" };

HttpContext.SetTenantInfo(newTenantInfo);

// This will be the new tenant.
var tenant = HttpContext.GetTenantContext<TenantInfo>().TenantInfo;
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

> Bypassing runs **before** tenant resolution. [Short circuiting](#short-circuiting) runs **after** resolution.

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

See [Per-Tenant Authentication](Authentication) for full details.

## Per-Tenant Options

MultiTenant integrates with the standard ASP.NET Core Options pattern so that options can be configured
differently for each tenant. Any options class used by ASP.NET Core or your own code can be customized
per tenant with minimal changes.

See [Per-Tenant Options](Options) for full details.

## Data Isolation

### Entity Framework Core

MultiTenant can automatically filter EF Core queries by tenant using a global query filter. This removes
the need to add `WHERE TenantId = ...` clauses throughout your app.

See [Data Isolation with Entity Framework Core](EFCore) for full details.

### ASP.NET Core Identity

MultiTenant has dedicated support for data isolation when using ASP.NET Core Identity with EF Core,
including multi-tenant identity context base classes and support for passkeys (WebAuthn).

See [Data Isolation with ASP.NET Core Identity](Identity) for full details.

