# Configuration and Usage

## Configuration

MultiTenant uses the standard application builder pattern for its configuration. In addition to adding the
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
further configuration. Each of these methods returns the same `MultiTenantBuilder<TTenantInfo>` instance allowing for
chaining method calls.

## Configuring the Service

### WithStore and WithStoreCache Variants

Adds and configures an `IMultiTenantStore` primary store for your app. Exactly one primary store can be configured.
Store caches can also be configured and each cache will be checked in the order registered before the primary store.
See [MultiTenant Stores](Stores) for more information on each type.

- `WithStore<TStore>`
- `WithInMemoryStore`
- `WithConfigurationStore`
- `WithEFCoreStore<TEFCoreStoreDbContext, TTenantInfo>`
- `WithHttpRemoteStore`
- `WithEchoStore`
- `WithStoreCache<TStoreCache>`
- `WithMemoryCacheStoreCache`
- `WithDistributedCacheStoreCache`

### WithStrategy Variants

Adds and configures an `IMultiTenantStrategy` for your app. Multiple strategies can be configured and each will be used
in the order registered. See [MultiTenant Strategies](Strategies) for more information on each type.

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

> Need fallbacks? Chain several strategies and configure store caches; the resolver will keep trying strategies in order
> and query configured caches before the primary store until a `TenantInfo` is found.

### WithPerTenantAuthentication

Configures support for per-tenant authentication. See [Per-Tenant Authentication](Authentication) for more details.

## Per-Tenant Options

MultiTenant is designed to integrate with the
standard [.NET Options pattern](https://learn.microsoft.com/en-us/dotnet/core/extensions/options) (see also
the [ASP.NET Core Options pattern](https://docs.microsoft.com/en-us/aspnet/core/fundamentals/configuration/options)) and
lets apps customize options distinctly for each tenant. See [Per-Tenant Options](Options) for more details.

## Tenant Resolution and Usage

MultiTenant will perform tenant resolution using the context, strategies, and stores as configured.

The context will depend on the type of app. For an ASP.NET Core web app the context is the `HttpContext` for each
request and a tenant will be resolved for each request. For other types of apps the context will be different. For
example, a console app might resolve the tenant once at startup or a background service monitoring a queue might resolve
the tenant for each message it receives.

Tenant resolution is performed by the `TenantResolver` class. The class requires a list of strategies and a
`TenantManager` as well as some options. The class will try each strategy generally in the order added, but static and
per-tenant authentication strategies will run at a lower priority. If a strategy returns a tenant identifier then the
tenant manager will query configured store caches in order before the primary store. The first source to return a
`TenantInfo` object will determine the resolved tenant. If no source returns a `TenantInfo` object then the next
strategy will be tried and so on. The `UseMultiTenant` middleware for ASP.NET Core uses `TenantResolver`
internally.

The `TenantResolver` options are configured in the `AddMultiTenant<TTenantInfo>` method with the following properties:

- `IgnoredIdentifiers` - A list of tenant identifiers that should be ignored by the resolver.
- `Events` - A set of events that can be used to hook into the resolution process:
    - `OnStrategyResolveCompleted` - Called after each strategy has attempted to resolve a tenant identifier. The
      `IdentifierFound` property will be `true` if the strategy resolved a tenant identifier. The `Identifier` property
      contains the resolved tenant identifier and can be changed by the event handler to override the strategy's result.
    - `OnStoreCacheResolveCompleted` - Called after each store cache has attempted to resolve a tenant. The
      `TenantFound` property will be `true` if the cache resolved a tenant. The `TenantInfo` property contains the
      resolved tenant and can be changed by the event handler to override the cache result. Setting `TenantInfo` to
      null causes resolution to continue to the next cache or the primary store. A non-null `TenantInfo` object will
      stop the resolver from trying additional strategies and sources.
    - `OnStoreResolveCompleted` - Called after the primary store has attempted to resolve a tenant. The `TenantFound`
      property will be `true` if the store resolved a tenant. The `TenantInfo` property contains the resolved tenant
      and can be changed by the event handler to override the store result. Setting `TenantInfo` to null causes
      resolution to continue to the next strategy. A non-null `TenantInfo` object will stop the resolver from trying
      additional strategies.
    - `OnTenantResolveCompleted` - Called once after a tenant has been resolved. The `TenantContext` property
      contains the resolved tenant context and can be changed by the event handler to override the resolver's
      result. The `Store` or `Cache` property references the source that resolved the tenant, `Strategy` references the
      strategy that was used, and `Context` holds the runtime context object passed to the resolver. If no tenant was
      resolved then `Store`, `Cache`, and `Strategy` are null. The `IsResolved` property indicates whether a tenant was
      found.

## Getting the Current Tenant

There are several ways your app can read the current tenant:

### Via Dependency Injection

`ITenantContext<TTenantInfo>` (and its non-generic variant `ITenantContext`) are available via dependency injection
with a **scoped lifetime** (`AddScoped`). Each DI scope (e.g. each HTTP request in ASP.NET Core) gets its own
`TenantContext<TTenantInfo>` instance. The middleware resolves the tenant and sets `TenantInfo` on this scoped
instance early in the request pipeline, so all services resolved within the same scope see the same tenant.

In ASP.NET Core, prefer the `HttpContext` extension members such as `GetTenantContext<TTenantInfo>` or
`TenantContext` since they always reflect the state set by the middleware, even in post-endpoint processing.

### Via `HttpContext` (ASP.NET Core)

For ASP.NET Core web apps the `GetTenantContext<TTenantInfo>`, `GetTenantInfo<TTenantInfo>`, `TenantContext`, and
`CurrentTenant` extension members are available directly on `HttpContext`. See
[ASP.NET Core Integration](AspNetCore#getting-the-current-tenant-in-aspnet-core) for details and examples.

## Setting the Current Tenant

In most cases the middleware resolves and sets the tenant automatically. When manual override is needed there are two
options:

### Via Dependency Injection

The injected `ITenantContext<TTenantInfo>` instance's `TenantInfo` property can be set directly to change the
current tenant. **`TenantInfo` can only be set once** — attempting to set it again throws a
`MultiTenantException`. This is useful in advanced scenarios and should be used with caution. Prefer the
`HttpContext` extension method `SetTenantInfo<TTenantInfo>` or `TrySetTenantInfo<TTenantInfo>` when
`HttpContext` is available.

### Via `HttpContext` (ASP.NET Core)

For ASP.NET Core web apps the `SetTenantInfo<TTenantInfo>` extension method is available directly on `HttpContext`.
See [ASP.NET Core Integration](AspNetCore#getting-the-current-tenant-in-aspnet-core) for details and examples.

## Important Considerations

- `ITenantContext<TTenantInfo>` is registered as a **scoped** service. Its lifetime is tied to the DI scope,
  not to a specific tenant. All services within the same scope see the same tenant.
- `TenantInfo` can only be set once per scope. Attempting to set it a second time throws
  `MultiTenantException`. Use the `HttpContext.TrySetTenantInfo<T>()` extension or check
  `ITenantContext.IsResolved` to avoid this.
- The `TenantResolver` tries strategies in order, then stores in order for each strategy. Resolution stops
  at the first store returning a match. Plan your ordering accordingly.
- Strategies from `Finbuckle.MultiTenant.AspNetCore` (Host, Route, Base Path, etc.) require `HttpContext`
  and only work in web apps. Use [Delegate Strategy](Strategies#delegate-strategy) or
  [Static Strategy](Strategies#static-strategy) in non-web scenarios.
- `ITenantInfo` instances should be kept lightweight. Store heavy data externally and load it by tenant `Id`
  when needed.

## See Also

- [ASP.NET Core Integration](AspNetCore) — middleware and `HttpContext` helpers
- [.NET Generic Host Integration](GenericHost) — using MultiTenant in non-web apps
- [Core Concepts](CoreConcepts) — `ITenantContext`, strategies, stores
- [Per-Tenant Options](Options) — per-tenant configuration
- [MultiTenant Strategies](Strategies) — all built-in strategies
