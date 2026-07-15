# Getting Started

MultiTenant is designed to be easy to use and follows standard .NET conventions as much as possible. This
introduction assumes a typical ASP.NET Core use case, but any application using .NET dependency injection can work with
the library.

## Installation

MultiTenant is split into focused NuGet packages so you only reference what you need. For a typical ASP.NET
Core app install `Finbuckle.MultiTenant.AspNetCore`:

```bash
dotnet add package Finbuckle.MultiTenant.AspNetCore
```

## Basic Configuration

MultiTenant is simple to get started with. Below is a sample app configured to use the subdomain as the tenant
identifier and the app's [configuration](https://learn.microsoft.com/en-us/aspnet/core/fundamentals/configuration/) (
most likely from an `appsettings.json` file) as the source of tenant details.

```csharp
using Finbuckle.MultiTenant;

var builder = WebApplication.CreateBuilder(args);

// add app services...

// add MultiTenant services
builder.Services.AddMultiTenant<TenantInfo>()
    .WithHostStrategy()
    .WithConfigurationStore();

var app = builder.Build();

// add the MultiTenant middleware
app.UseMultiTenant();

// add other middleware...

app.Run();
```

That's all that is needed to get going. Let's break down each line:

`builder.Services.AddMultiTenant<TenantInfo>()`

This line registers the base services and designates `TenantInfo` as the class that will hold tenant information at
runtime.

The type parameter for `AddMultiTenant<TTenantInfo>` must implement `ITenantInfo` and holds
basic information about the tenant such as its id and an identifier. `TenantInfo` is provided as a basic
implementation class, but any implementation of `ITenantInfo` can be used if more properties are needed.

See [Core Concepts](CoreConcepts) for more information on `TenantInfo`.

`.WithHostStrategy()`

The line tells the app that our "strategy" to determine the request tenant will be to look at the request host, which
defaults to extracting the subdomain as a tenant identifier.

See [Strategies](Strategies) for more information.

`.WithConfigurationStore()`

This line tells the app that information for all tenants are in the `appsettings.json` file used for app configuration.
If a tenant in the store has the identifier found by the strategy, the tenant will be successfully resolved for the
current request.

A minimal configuration entry looks like this:

```json
{
  "Finbuckle:MultiTenant:Stores:ConfigurationStore": {
    "Tenants": [
      {
        "Id": "93f330717e5d4f039cd05da312d559cc",
        "Identifier": "initech",
        "Name": "Initech"
      }
    ]
  }
}
```

See [Stores](Stores) for more information.

MultiTenant comes with a collection of strategies and store types that can be mixed and matched in various
ways.

`app.UseMultiTenant()`

This line configures the middleware which resolves the tenant using the registered strategies, stores, and other
settings. Be sure to call it before other middleware which will use per-tenant functionality, such as
`UseAuthentication()` or `UseAuthorization()` so those components see the resolved tenant.

## Basic Usage

With the services and middleware configured, access information for the current tenant from the `TenantInfo` property on
the `ITenantContext<TTenantInfo>` object accessed from the `GetTenantContext<TTenantInfo>` extension method:

```csharp
var tenantInfo = HttpContext.GetTenantContext<TenantInfo>().TenantInfo;

if(tenantInfo != null)
{
    var tenantId = tenantInfo.Id;
    var identifier = tenantInfo.Identifier;
}
```

The type of the `TenantInfo` property depends on the type passed when calling `AddMultiTenant<TTenantInfo>` during
configuration. If the current tenant could not be determined then `TenantInfo` will be null.

For non-generic access in ASP.NET Core, use the `HttpContext.TenantContext` extension property. To read only the
current tenant as `ITenantInfo`, use `HttpContext.CurrentTenant`.

The `TenantInfo` instance and the typed instance are also available using the `ITenantContext<TTenantInfo>` interface
which is available via dependency injection.

See [Configuration and Usage](ConfigurationAndUsage) for more information.

## Advanced Usage

The library builds on this basic functionality to provide a variety of higher level features. See the documentation for
more details:

* [ASP.NET Core Integration](AspNetCore) — middleware options, bypassing, short-circuiting, `HttpContext` helpers
* [Per-tenant Options](Options)
* [Per-tenant Authentication](Authentication)
* [Entity Framework Core Data Isolation](EFCore)
* [ASP.NET Core Identity Data Isolation](Identity)

## Samples

A variety of sample projects are available in
the [samples](https://github.com/Finbuckle/Finbuckle.MultiTenant/tree/main/samples) directory.

## Important Considerations

- The type parameter passed to `AddMultiTenant<TTenantInfo>()` determines the `ITenantInfo` implementation used
  throughout the app. Choose or define a class that fits your tenant data model.
- `ITenantContext<TTenantInfo>` is registered as a scoped service. In ASP.NET Core the middleware populates it
  automatically per request. In other app models you must manage scope creation and tenant resolution manually
  (see [.NET Generic Host Integration](GenericHost)).
- `TenantInfo` can only be set once per scope. The middleware handles this in web apps, but be aware of the
  constraint if you call `SetTenantInfo` manually.
- Middleware ordering is critical: `UseMultiTenant()` must come before `UseAuthentication()`, `UseAuthorization()`,
  and any middleware that reads per-tenant options or services.
- For web apps, prefer the `HttpContext` extension members (`GetTenantContext<T>()`, `GetTenantInfo<T>()`,
  `TenantContext`, `CurrentTenant`)
  over injecting `ITenantContext` directly, as they always reflect the middleware's state.

## See Also

- [Core Concepts](CoreConcepts) — `ITenantInfo`, strategies, stores
- [Configuration and Usage](ConfigurationAndUsage) — all registration options
- [ASP.NET Core Integration](AspNetCore) — middleware and `HttpContext` helpers
- [.NET Generic Host Integration](GenericHost) — non-web scenarios
