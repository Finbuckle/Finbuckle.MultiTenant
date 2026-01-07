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

### WithStore Variants

Adds and configures an IMultiTenantStore for your app. Only the last store configured will be used.
See [MultiTenant Stores](Stores) for more information on each type.

- `WithStore<TStore>`
- `WithInMemoryStore<TTenantStore>`
- `WithConfigurationStore<TTenantStore>`
- `WithEFCoreStore<TTenantStore>`
- `WithDistributedCacheStore<TTenantStore>`
- `WithHttpRemoteStore<TTenantStore>`

### WithStrategy Variants

Adds and configures an IMultiTenantStrategy for your app. Multiple strategies can be configured and each will be used
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

> Need fallbacks? Chain several strategies and more than one store; the resolver will keep trying strategies in order and run through the configured stores until a `TenantInfo` is found.

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

Tenant resolution is performed by the `TenantResolver` class. The class requires a list of strategies and a list of
stores as well as some options. The class will try each strategy generally in the order added, but static and per-tenant
authentication strategies will run at a lower priority. If a strategy returns a tenant identifier then each store will
be queried in the order they were added. The first store to return a `TenantInfo`
object will determine the resolved tenant. If no store returns a `TenantInfo` object then the next strategy will be
tried and so on. The `UseMultiTenant` middleware for ASP.NET Core uses `TenantResolver`
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

## ASP.NET Core Features

Some additional features are available to tailor the middleware to your specific needs.

### Exclude Endpoints From Tenant Resolution

If you have scenarios where you do not want Tenant resolution to be performed, you can exclude specific endpoints.

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

Using the `ExcludeFromMultiTenantResolutionAttribute` attribute to configure the behavior of controllers and action
methods.

```csharp
// Here --> [ExcludeFromMultiTenantResolution]
public class DashboardController : Controller
{
    // Or here --> [ExcludeFromMultiTenantResolution]
    public ActionResult Index()
    {
        return View();
    }
}
```

### Short Circuiting

The `MultiTenantMiddleware` can be configured to short circuit a request pipeline when no tenant is found or when some
custom condition is met. `ShortCircuitWhenTenantNotResolved<TTenantInfo()` and `ShortCircuitWhen<TTenantInfo>()` as
shown below will configure this behavior as necessary.

#### Short Circuit When Tenant Not resolved

Call `ShortCircuitWhenTenantNotResolved<TTenantInfo()` after `AddMultiTenant<TTenantInfo>` to halt further processing of
the request pipeline when no tenant can be found. An overload accepts a URI where the user will be redirected if no
tenant was found.

> If you short circuit when tenant not resolved, and you have endpoints that do not require a tenant,
> then `ExcludeFromMultiTenantResolution` becomes a necessity, otherwise, these endpoints would never be reached.

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

#### Short Circuit When a Custom Condition is Met

If you find that you need to short circuit for other, more advanced reasons,
use the `MultiTenantBuilder<TTenantInfo>` extension `ShortCircuitWhen()`.

```csharp
// Advanced short circuiting, if an obsolete strategy was used or when tenant not resolved.
builder.Services.AddMultiTenant<TenantInfo>()
    .WithHostStrategy()
    .WithConfigurationStore()
    .ShortCircuitWhen(config =>
	{
		config.Predicate = context => context.StrategyInfo is IMyCustomObsoleteStrategy || !context.IsResolved;
	});
	
// Including a redirect.
builder.Services.AddMultiTenant<TenantInfo>()
    .WithHostStrategy()
    .WithConfigurationStore()
	.ShortCircuitWhen(config =>
	{
		config.Predicate = context => context.StrategyInfo is IMyCustomObsoleteStrategy || !context.IsResolved;
		config.RedirectTo = new Uri("/tenant/notfound", UriKind.Relative)
	});
```

## Getting the Current Tenant

There are several ways your app can see the current tenant:

### Dependency Injection

* `IMultiTenantContextAccessor` and `IMultiTenantContextAccessor<TTenantInfo>` are available via dependency injection
  and behave similar to `IHttpContextAccessor`. Internally an `AsyncLocal<T>` is used to track state and in parent async
  contexts any changes in tenant will not be reflected. For example, the accessor will not reflect a tenant in the
  post-endpoint processing in ASP.NET Core middleware registered prior to `UseMultiTenant`. Use the `HttpContext`
  extension `GetMultiTenantContext<TTenantInfo>` to avoid this caveat.

* `IMultiTenantContextSetter` is available via dependency injection and can be used to set the current tenant. This is
  useful in advanced scenarios and should be used with caution. Prefer using the `HttpContext` extension method
  `SetTenantInfo<TTenantInfo>` in use cases where `HttpContext` is available.

> Prior versions of MultiTenant also exposed `IMultiTenantContext`, `TenantInfo`, and their implementations
> via dependency injection. This was removed as these are not actual services, similar to
> how [HttpContext is not a service](https://github.com/dotnet/aspnetcore/issues/47996#issuecomment-1529364233) and not
> available directly via dependency injection.

### `HttpContext` Extension Methods

For web apps these convenience methods are also available:

* `GetMultiTenantContext<TTenantInfo>`

  Use this `HttpContext` extension method to get the `MultiTenantContext<TTenantInfo>` instance for the current request.
  This should be preferred to `IMultiTenantContextAccessor` or `IMultiTenantContextAccessor<TTenantInfo>` when possible.

  ```csharp
  var tenantInfo = HttpContext.GetMultiTenantContext<TenantInfo>().TenantInfo;
  
  if(tenantInfo != null)
  {
    var tenantId = tenantInfo.Id;
    var identifier = tenantInfo.Identifier;
    var name = tenantInfo.Name;
  }
  ```

* `SetTenantInfo<TTenantInfo>`

  For most cases the middleware sets the `TenantInfo` and this method is not needed. Use only if explicitly overriding
  the `TenantInfo` set by the middleware.

  Use this 'HttpContext' extension method to set the current tenant to the provided `TenantInfo`.
  Optionally it can also reset the service provider scope so that any scoped services already resolved will
  be resolved again under the current tenant when needed. This has no effect on singleton or transient services. Setting
  the `TenantInfo` with this method sets both the `StoreInfo` and `StrategyInfo` properties on the
  `MultiTenantContext<TTenantInfo>` to `null`.

  ```csharp
  var newTenantInfo = new TenantInfo { Id = "new-id", Identifier = "new-identifier" };
  
  HttpContext.SetTenantInfo(newTenantInfo, resetServiceProviderScope: true);
  
  // This will be the new tenant.
  var tenant = HttpContext.GetMultiTenantContext<TenantInfo>().TenantInfo;

  // This will regenerate the options class.
  var optionsProvider = HttpContext.RequestServices.GetService<IOptions<MyScopedOptions>>();
  ```
