# Configuration and Usage

## Configuration
Finbuckle.MultiTenant uses the standard builder pattern for its configuration in the `ConfigureServices` method of the app's `Startup` class. Order doesn't matter, but both a multitenant store and a multitenant strategy are required.

```cs
public void ConfigureServices(IServiceCollection services)
{
    ...
    services.AddMultiTenant<TenantInfo>()
            .WithStore(...)
            .WithStrategy(...)...
    ...
}
```

## AddMultiTenant<T>
`AddMultiTenant<T>` is an extension method on `IServiceCollection` which registers the basic dependencies needed by the library.
The `T` type paramter determines the type of the `ITenantInfo` object used throughout the library. `TenantInfo` provides a basic
immplementation of `ITenantInfo`, but a custom implementation can and should be provided. It returns a `MultiTenantBuilder<T>` instance on which the methods below can be called for further configuration. Each of these methods returns the same `MultiTenantBuilder<T>` instance allowing for chaining method calls.

## WithStore Variants
Adds and configures an IMultiTenantStore to the application. Only the last store configured will be used. See [MultiTenant Stores](Stores) for more information on each type.

- WithStore&lt;TStore&gt;
- WithInMemoryStore
- WithConfigurationStore
- WithEFCoreStore

## WithStrategy Variants
Adds and configures an IMultiTenantStore to the application. Multiple strategies can be configured and each will be used in the order registered. See [MultiTenant Strategies](Strategies) for more information on each type.

- WithStrategy&lt;TStrategy&gt;
- WithBasePathStrategy
- WithClaimsStrategy
- WithDelegateStrategy
- WithHostStrategy
- WithRouteStrategy
- WithSessionStrategy
- WithStaticStrategy

## WithPerTenantOptions<TOptions>
Adds per-tenant configuration for an options class. See [Per-Tenant Options](Options) for more details.

## WithPerTenantAuthentication
Configures support for per-tenant authentication.
See [Per-Tenant Authentication](Authentication) for more details.

## Usage
Most of the capability enabled by Finbuckle.MultiTenant is utilized through its middleware and use the [Options pattern with per-tenant options](Options). The middleware will resolve the app's current tenant on each request using the configured strategies and stores, and the per-tenant options will alter the app's behavior as dependency injection passes the options to app components.

In addition, there are a few methods available for directly accessing and settings the tenant information if needed.

## UseMultiTenant
Configures the middleware handling tenant resolution via the multitenant strategy and the multitenant store. `UseMultiTenant` should usually be called before `UseAuthentication` and `UseMvc` in the `Configure` method of the app's `Startup` class. Additionally, if any other middleware uses per-tenant options then that middleware should come after `UseMultiTenant`. In ASP.NET Core 3 or later `UseRouting` should come before `UseMultiTenant` if the route strategy is used.

```cs
public void Configure(IApplicationBuilder app)
{
    app.UseRouting(); // In ASP.NET Core 3.1 this should be before UseMultiTenant!
    ...
    app.UseMultiTenant(); // Before UseAuthentication and UseMvc!
    ...
    app.UseAuthentication();
    ...
    app.UseMvc();
}
```

## GetMultiTenantContext
Extension method of `HttpContext` that returns the `MultiTenantContext` instance for the current request. If there is no current tenant the `TenantInfo` property will be null.

```cs
var tenantInfo = HttpContext.GetMultiTenantContext().TenantInfo;

if(tenantInfo != null)
{
    var tenantId = tenantInfo.Id;
    var identifier = tenantInfo.Identifier;
    var name = tenantInfo.Name;
    var something = tenantInfo.Items["something"];
}
```

## TrySetTenantInfo

*Note: For most cases the middleware sets the `TenantInfo` and this method is not needed. Use only if explicitly overriding the `TenantInfo` set by the middleware.*

Extension method of `HttpContext` that tries to set the current tenant to the provided `TenantInfo`. Returns true if successful. Optionally it can also reset the service provider scope so that any scoped services already resolved will be resolved again under the current tenant when needed. This has no effect on singleton or transient services. Setting the `TenantInfo` with this method sets both the `StoreInfo` and `StrategyInfo` properties on the `MultiTenantContext` to null.

```cs
var newTenantInfo = new TenantInfo(...);

if(HttpContext.TrySetTenantInfo(newTenantInfo, resetServiceProvider: true))
{
    // This will be the new tenant.
    var tenant = HttpContext.GetMultiTenantContext().TenantIno;

    // This will regenerate the options class.
    var optionsProvider = HttpContext.RequestServices.GetService<IOptions<MyScopedOptions>>();
}
```