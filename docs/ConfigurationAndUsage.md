# Configuration and Usage

## Configuration

MultiTenant uses the standard application builder pattern. In addition to registering the
services, configuration for one or more [MultiTenant Stores](Stores) and [MultiTenant Strategies](Strategies) are
required. A typical configuration for your ASP.NET Core app might look like this:

```csharp
using Finbuckle.MultiTenant;

var builder = WebApplication.CreateBuilder(args);

// ...add app services

// add MultiTenant services
builder.Services.AddMultiTenant<TenantInfo>()
    .WithHostStrategy()
    .WithConfigurationStore();

var app = builder.Build();

// add the MultiTenant middleware
app.UseMultiTenant();

// ...add other middleware

app.Run();
```

## Adding the MultiTenant Service

Use the `AddMultiTenant<TTenantInfo>` extension method on `IServiceCollection` to register the basic dependencies needed
by the library. It returns a `MultiTenantBuilder<TTenantInfo>` instance on which the methods below can be called for
further configuration. Each method returns the same `MultiTenantBuilder<TTenantInfo>` instance, so calls can be
chained.

## Configuring the Service

### WithStore Variants

Adds and configures an `IMultiTenantStore` for your app. Multiple stores can be configured, and they will be consulted
in the order registered until a tenant is resolved.
See [MultiTenant Stores](Stores) for more information on each type.

- `WithStore<TStore>`
- `WithInMemoryStore`
- `WithConfigurationStore`
- `WithEFCoreStore<TEFCoreStoreDbContext, TTenantInfo>`
- `WithDistributedCacheStore`
- `WithHttpRemoteStore`

### WithStrategy Variants

Adds and configures an `IMultiTenantStrategy` for your app. Multiple strategies can be configured and are tried in
registration order. See [MultiTenant Strategies](Strategies) for more information on each type.

- `WithStrategy<TStrategy>`
- `WithBasePathStrategy`
- `WithClaimStrategy`
- `WithDelegateStrategy`
- `WithDelegateStrategy<TContext, TTenantInfo>`
- `WithHeaderStrategy`
- `WithHostStrategy`
- `WithHttpContextStrategy`
- `WithRouteStrategy`
- `WithSessionStrategy`
- `WithStaticStrategy`

> Need fallbacks? Chain several strategies and more than one store; the resolver will keep trying strategies in order
> and run through the configured stores until a `TenantInfo` is found.

### WithPerTenantAuthentication

Configures support for per-tenant authentication. See [Per-Tenant Authentication](Authentication) for more details.

## Per-Tenant Options

MultiTenant is designed to integrate with the
standard [.NET Options pattern](https://learn.microsoft.com/en-us/dotnet/core/extensions/options) (see also
the [ASP.NET Core Options pattern](https://docs.microsoft.com/en-us/aspnet/core/fundamentals/configuration/options)) and
lets apps customize options distinctly for each tenant. See [Per-Tenant Options](Options) for more details.

## Tenant Resolution and Usage

MultiTenant will perform tenant resolution using the context, strategies, and stores as configured.

The context depends on the type of app. For an ASP.NET Core web app, the context is the `HttpContext` and a tenant is
resolved for each request. For other app types, the context will differ. For
example, a console app might resolve the tenant once at startup or a background service monitoring a queue might resolve
the tenant for each message it receives.

Tenant resolution is performed by the `TenantResolver` class. It requires a list of strategies, a list of stores, and
options. It generally tries strategies in the order added, although static and per-tenant authentication strategies run
at a lower priority. When a strategy returns a tenant identifier, each store is queried in registration order. The first
store to return a `TenantInfo` determines the resolved tenant. If no store returns a `TenantInfo`, the resolver tries
the next strategy. The `UseMultiTenant` middleware for ASP.NET Core uses `TenantResolver`
internally.

The `TenantResolver` options are configured in the `AddMultiTenant<TTenantInfo>` method with the following properties:

- `IgnoredIdentifiers` - A list of tenant identifiers that should be ignored by the resolver.
- `Events` - A set of events that can be used to hook into the resolution process:
    - `OnStrategyResolveCompleted` - Called after each strategy has attempted to resolve a tenant identifier. The
      `IdentifierFound` property will be `true` if the strategy resolved a tenant identifier. The `Identifier` property
      contains the resolved tenant identifier and can be changed by the event handler to override the strategy's result.
    - `OnStoreResolveCompleted` - Called after each store has attempted to resolve a tenant. The `TenantFound` property
      will be `true` if the store resolved a tenant. The `TenantInfo` property contains the resolved tenant and can be
      changed by the event handler to override the store's result. A non-null `TenantInfo` object will stop the resolver
      from trying additional strategies and stores.
    - `OnTenantResolveCompleted` - Called once after a tenant has been resolved. The `MultiTenantContext` property
      contains the resolved multi-tenant context and can be changed by the event handler to override the resolver's
      result.

## Getting the Current Tenant

There are several ways your app can read the current tenant:

### Via Dependency Injection

`IMultiTenantContextAccessor<TTenantInfo>` (and its non-generic variant `IMultiTenantContextAccessor`) are available
via dependency injection and behave similarly to `IHttpContextAccessor`. Internally an `AsyncLocal<T>` is used to track
state. Changes made in a child async context do not flow back to its parent. For example, the accessor will not reflect
a tenant during post-endpoint processing in ASP.NET Core middleware registered before `UseMultiTenant`.
Use the `HttpContext` extension `GetMultiTenantContext<TTenantInfo>` to avoid this caveat.

> Prior versions of MultiTenant also exposed `IMultiTenantContext`, `TenantInfo`, and their implementations
> via dependency injection. This was removed as these are not actual services, similar to
> how [HttpContext is not a service](https://github.com/dotnet/aspnetcore/issues/47996#issuecomment-1529364233) and not
> available directly via dependency injection.

### Via `HttpContext` (ASP.NET Core)

For ASP.NET Core web apps the `GetMultiTenantContext<TTenantInfo>` extension method is available directly on
`HttpContext` and is the preferred approach. See
[ASP.NET Core Integration](AspNetCore#getting-the-current-tenant-in-aspnet-core) for details and examples.

## Setting the Current Tenant

In most cases the middleware resolves and sets the tenant automatically. When manual override is needed there are two
options:

### Via Dependency Injection

`IMultiTenantContextSetter` is available via dependency injection and can be used to set the current tenant. This is
useful in advanced scenarios and should be used with caution. Prefer the `HttpContext` extension method
`SetTenantInfo<TTenantInfo>` when `HttpContext` is available.

### Via `HttpContext` (ASP.NET Core)

For ASP.NET Core web apps the `SetTenantInfo<TTenantInfo>` extension method is available directly on `HttpContext`.
See [ASP.NET Core Integration](AspNetCore#getting-the-current-tenant-in-aspnet-core) for details and examples.
