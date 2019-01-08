# MultiTenant Strategies

A multitenant strategy is responsible for defining how the tenant is determined. It ultimately produces an identifier string which is used to create a `TenantContext` object with information from the [multiTenant store](Stores).

Finbuckle.MultiTenant supports several "out-of-the-box" strategies for resolving the tenant. Custom strategies can be created by implementing `IMultiTenantStrategy`. Internally strategies are registered as singleton services.

## IMultiTenantStrategy
All multitenant strategies derive from `IMultiTenantStrategy` and must implement the `GetIdentifier` method. 

If an identifier can't be determined, `GetIdentifierAsync` should return null which will ultimately result in a null `TenantContext`.

Configure a custom implementation of `IMultiTenantStrategy` by calling `WithStrategy<T>` or `WithStrategy` after `AddMultiTenant` in the `ConfigureServices` method of the `Startup` class. The templated version will use dependency injection and any passed parameters to construct the implementation instance. The non-templated version accepts a `Func<IServiceProvider, IMultiTenantStrategy>` factory method for even more customization.

```cs
// Register a custom strategy with the templated method.
services.AddMultiTenant().WithStrategy<MyStrat>(myParam1, myParam2)...

// Or register a custom strategy with the non-templated method which accepts a factory method.
services.AddMultiTenant().WithStrategy( sp => return new MyStrat())...
```

## Static Strategy
Always uses the same identifier to resolve the tenant.

If using with ASP.NET Core, configure by calling `WithStaticStrategy` after `AddMultiTenant` in the `ConfigureServices` method of the `Startup` class and passing in the identifier to use for tenant resolution:

```cs
services.AddMultiTenant().WithStaticStrategy("MyTenant")...
```

## Base Path Strategy 
Uses the base (i.e. first) path segment to determine the tenant. For example, a request to "https://www.example.com/contoso" would use "contoso" as the identifier when retreiving the `TenantContext`.

Configure by calling `WithBasePathStrategy` after `AddMultiTenant` in the `ConfigureServices` method of the `Startup` class:

```cs
services.AddMultiTenant().WithBasePathStrategy()...
```

## Route Strategy
Uses the `__tenant__` route parameter (or a specified route parameter) to determine the tenant. For example, a request to "https://www.example.com/contoso/home/" and a route configuration of `{__tenant__}/{controller=Home}/{action=Index}` would use "contoso" as the identifier when resolving the tenant. The `__tenant__` parameter can be placed anywhere in the route path configuration.

Configure by calling `WithRouteStrategy` after `AddMultiTenant` in the `ConfigureServices` method of the `Startup` class. An `Action<IRouteBuilder>` parameter which configures the routing is passed in which should always match the routes configured with MVC.  A different route parameter name can be specified with the overloaded version:

```cs
public class Startup
{
    public void ConfigureServices(IServiceCollection services)
    {
        // Other services...

        // Use the default route parameter name "__tenant__":
        services.AddMultiTenant().WithRouteStrategy(configRoutes)... // Make sure to include a multitenant store!
        
        // Alternatively set a different route parameter name of "MyTenantRouteParam":
        // services.AddMultiTenant().WithRouteStrategy("MyTenantRouteParam", configRoutes)...
        
        // Other services...

        services.AddMvc();

        // Other services...
    }

    public void Configure(IappBuilder app, ...)
    {
        // Other middlware...

        app.UseMultiTenant();
        
        // Other middlware...
        
        app.UseMvc(ConfigRoutes);
    }

    private void ConfigRoutes(IRouteBuilder routes)
    {
        routes.MapRoute("Defaut", "{__tenant__}/{controller=Home}/{action=Index}");
    }
}
``` 

## Host Strategy
Uses request's host value to determine the tenant. By default the first host segment is used. For example, a request to "https://contoso.example.com/abc123" would use "contoso" as the identifier when retrieving the `TenantContext`. This strategy is included in `Finbuckle.MultiTenant.AspNetCore`. This strategy can be difficult to use in a development environment. Make sure the development system is configured properly to allow subdomains on `localhost`.

The host strategy uses a template string which defines how the strategy will find the tenant identifier. The pattern specifies the location for the tenant identifer using "\_\_tenant\_\_" and can contain other valid domain characters. It can also use '?' and '\*' characters to represent one or "zero or more" segments. For example:
  - `__tenant__.*` is the default if no pattern is provided and selects the first domain segment for the tenant identifier.
  - `*.__tenant__.?` selects the main domain as the tenant identifier and ignores any subdomains and the top level domain.
  - `__tenant__.example.com` will always use the subdomain for the tenant identifier, but only if there are no prior subdomains and the overall host ends with "example.com".
  - `*.__tenant.__.?.?` is similar to the above example except it will select the first subdomain even if others exist and doesn't require ".com".

Configure by calling `WithHostStrategy` after `AddMultiTenant` in the `ConfigureServices` method of the `Startup` class. A template pattern can be specified with the overloaded version:

```cs
// Use the default template "__tenant__.*":
services.AddMultiTenant().WithHostStrategy()...

// Set a template which selects the main domain segment (see 2nd example above):
services.AddMultiTenant().WithHostStrategy("*.__tenant__.?")...
```
