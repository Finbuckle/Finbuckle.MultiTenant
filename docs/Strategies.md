# MultiTenant Strategies

A multitenant strategy is responsible for defining how the tenant is determined. It ultimately produces an identifier string which is used to create a `TenantInfo` object with information from the [MultiTenant store](Stores).

Finbuckle.MultiTenant supports several "out-of-the-box" strategies for resolving the tenant. Custom strategies can be created by implementing `IMultiTenantStrategy` or using `DelegateStrategy`.

Multiple strategies can be registered after `AddMultiTenant` and each strategy will be tried in the order configured until a non-null identifier is returned. The remaining strategies are skipped for that request. Note that some strategies are registered as singleton service so registering them multiple times after `AddMultiTenant` is not recommended. The main use for registering multiple strategies of the same type is using several instances of the `DelegateStrategy` utilizing distinct logic.

The `Strategy` property on the `StrategyInfo` member of `MultiTenantContext` instance returned by `HttpContext.GetMultiTenantContext()` returns the actual strategy used to resolve the tenant information for the current context.

## IMultiTenantStrategy and Custom Strategies
All multitenant strategies derive from `IMultiTenantStrategy` and must implement the `GetIdentifierAsync` method. 

If an identifier can't be determined, `GetIdentifierAsync` should return null which will ultimately result in a null `TenantInfo`.

Configure a custom implementation of `IMultiTenantStrategy` by calling `WithStrategy<TStrategy>` after `AddMultiTenant` in the `ConfigureServices` method of the `Startup` class. There are several available overrides for configurign the strategy. The first override uses dependency injection along with any passed parameters to construct the implementation instance. The second override accepts a `Func<IServiceProvider, TStrategy>` factory method for even more customization. The library internally decorates any `IMultiTenantStrategy` with a wrapper providing basic logging and exception handling.

```cs
// Register a custom strategy with the templated method.
// Make sure to include a multitenant store!
services.AddMultiTenant().WithStrategy<MyStrat>(myParam1, myParam2)...

// Or register a custom strategy with the non-templated method which accepts a factory method.
// Make sure to include a multitenant store!
services.AddMultiTenant().WithStrategy( sp => return new MyStrat())...
```

## Static Strategy
Always uses the same identifier to resolve the tenant. This strategy is configured as a singleton.

Configure by calling `WithStaticStrategy` after `AddMultiTenant` in the `ConfigureServices` method of the `Startup` class and passing in the identifier to use for tenant resolution:

```cs
// Make sure to include a multitenant store!
services.AddMultiTenant().WithStaticStrategy("MyTenant")...
```

## Base Path Strategy 
Uses the base (i.e. first) path segment to determine the tenant. For example, a request to "https://www.example.com/contoso" would use "contoso" as the identifier when resolving the tenant. This strategy is configured as a singleton.

Configure by calling `WithBasePathStrategy` after `AddMultiTenant` in the `ConfigureServices` method of the `Startup` class:

```cs
// Make sure to include a multitenant store!
services.AddMultiTenant().WithBasePathStrategy()...
```

## Session Strategy
Uses the ASP.NET Core session to retrieve the tenant identifier. This strategy is configured as a singleton.

Configure by calling `WithSessionStrategy` after `AddMultiTenant` in the `ConfigureServices` method of the `Startup` class. This will use a default session key named `__tenant__`. An overload of `WithSessionStrategy can be used to specify a different key name:

```cs
// Configure to use "__tenant__" as the session key,
services.AddMultiTenant().WithSessionStrategy()...

// Or configure to use "my-tenant-session-key" as the session key,
services.AddMultiTenant().WithSessionStrategy("my-tenant-session-key")...
```

Note that an app will have to [configure session state](https://docs.microsoft.com/en-us/aspnet/core/fundamentals/app-state?view=aspnetcore-3.1#session-state) accordingly and then actually set the session variable. A typical use case is to register the session strategy before a more expensive strategy. The expensive strategy can set the session value so that for subsequent requests resolve the tenant without invoking the expensive strategy.

## Route Strategy
Note: the configuration and use of this strategy differs in ASP.NET Core 2.1 and ASP.NET Core 3.1+.

Uses the `__tenant__` route parameter (or a specified route parameter) to determine the tenant. For example, a request to "https://www.example.com/contoso/home/" and a route configuration of `{__tenant__}/{controller=Home}/{action=Index}` would use "contoso" as the identifier when resolving the tenant. The `__tenant__` parameter can be placed anywhere in the route path configuration. This strategy is configured as a singleton.

**ASP.NET Core 3 or higher**
 The route strategy is made improved in ASP.NET Core 3 due to the new endpoint routing mechanism. Configure by calling `WithRouteStrategy` after `AddMultiTenant` in the `ConfigureServices` method of the `Startup` class. A different route parameter name can be specified with the overloaded version. Then in the app pipeline make sure to call `UseRouting` before `UseMultiTenant`:

```cs
public class Startup
{
    public void ConfigureServices(IServiceCollection services)
    {
        // Other services...

        // Use the default route parameter name "__tenant__":
        // Make sure to include a multitenant store!
        services.AddMultiTenant().WithRouteStrategy()...
        
        // Alternatively set a different route parameter name of "MyTenantRouteParam":
        // services.AddMultiTenant().WithRouteStrategy("MyTenantRouteParam")...
        
        // Other services...

        services.AddMvc();

        // Other services...
    }

    public void Configure(IappBuilder app, ...)
    {
        // Other middlware...

        app.UseRouting(); // Important!

        // Other middlware...

        app.UseMultiTenant();
        
        // Other middlware...
        
        app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllerRoute("default", "{__tenant__}/{controller=Home}/{action=Index}");
            });
    }
}
```

**ASP.NET Core 2.1**
Configure by calling `WithRouteStrategy` after `AddMultiTenant` in the `ConfigureServices` method of the `Startup` class. An `Action<IRouteBuilder>` parameter which configures the routing is passed in which should always match the routes configured with MVC.  A different route parameter name can be specified with the overloaded version:

```cs
public class Startup
{
    public void ConfigureServices(IServiceCollection services)
    {
        // Other services...

        // Use the default route parameter name "__tenant__":
        // Make sure to include a multitenant store!
        services.AddMultiTenant().WithRouteStrategy(configRoutes)...
        
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
Uses request's host value to determine the tenant. By default the first host segment is used. For example, a request to "https://contoso.example.com/abc123" would use "contoso" as the identifier when resolving the tenant. This strategy can be difficult to use in a development environment. Make sure the development system is configured properly to allow subdomains on `localhost`. This strategy is configured as a singleton.

The host strategy uses a template string which defines how the strategy will find the tenant identifier. The pattern specifies the location for the tenant identifier using "\_\_tenant\_\_" and can contain other valid domain characters. It can also use '?' and '\*' characters to represent one or "zero or more" segments. For example:
  - `__tenant__.*` is the default if no pattern is provided and selects the first domain segment for the tenant identifier.
  - `*.__tenant__.?` selects the main domain as the tenant identifier and ignores any subdomains and the top level domain.
  - `__tenant__.example.com` will always use the subdomain for the tenant identifier, but only if there are no prior subdomains and the overall host ends with "example.com".
  - `*.__tenant.__.?.?` is similar to the above example except it will select the first subdomain even if others exist and doesn't require ".com".
  - As a special case, a pattern string of just `__tenant__` will use the entire host as the tenant identifier, as opposed to a single segment.

Configure by calling `WithHostStrategy` after `AddMultiTenant` in the `ConfigureServices` method of the `Startup` class. A template pattern can be specified with the overloaded version:

```cs
// Use the default template "__tenant__.*":
// Make sure to include a multitenant store!
services.AddMultiTenant().WithHostStrategy()...

// Set a template which selects the main domain segment (see 2nd example above):
// Make sure to include a multitenant store!
services.AddMultiTenant().WithHostStrategy("*.__tenant__.?")...
```

## Delegate Strategy

Uses a provided `Func<object, Task<string>>` to determine the tenant. For example the lambda function `context => Task.FromResult("contoso")` would use "contoso" as the identifier when resolving the tenant for every request. This strategy is good to use for testing or simple logic. This strategy is configured as transient and multiple instances can be registered.

Configure by calling `WithDelegateStrategy` after `AddMultiTenant` in the `ConfigureServices` method of the `Startup` class. A `Func<object, Task<string>>`is passed in which will be used with each request to resolve the tenant. A lambda or async lambda can be used as the parameter:

```cs
// Use the request query parameter "tenant" to get the tenantId:
// Make sure to include a multitenant store!
services.AddMultiTenant().
    WithDelegateStrategy(context =>
    {
        ((HttpContext)context).Request.Query.TryGetValue("tenant", out StringValues tenantId);
        return Task.FromResult(tenantId.ToString());
    })...
```

## Fallback Strategy

Returns a static tenant identifier string if the main strategy (and the remove authentication strategy, if applicable) fails to resolve a tenant with the tenant store. If the store does not have an entry for the fallback tenant identifier then this strategy has no effect. This strategy is configured as a singleton.

This strategy is intended be used in conjunction with other strategies and is always called last.

Configure by calling `WithFallbackStrategy` after `AddMultiTenant` in the `ConfigureServices` method of the `Startup` class and passing in the identifier to use for tenant resolution.

```cs
// If the called with e.g. "not_a_tenant.mysite.com" and the identifer "not_a_tenant"
// is not in the store, then the identifier "defaultTenant" will be used as a fallback.
// Make sure to include a multitenant store!
services.AddMultiTenant().WithHostStrategy().WithFallbackStrategy("defaultTenant");
```

## Remote Authentication Callback Strategy

This is a special strategy used for per-tenant authentication when remote authentication such as Open
ID Connect or OAuth (e.g. Log in via Facebook) are used. This strategy is configured as a singleton.

Ideally remote authentication callback paths can be configured per-tenant using `WithPerTenantOptions`
so that the callback paths work in conjunction with another strategy such as the host or route strategies.

However, in scenarios where this is not possible and multiple tenants must use the same callback url this strategy
will append the tenant identifier to state parameter which the remote authentication passes back to the app after
performing authentication. The strategy will parse the tenant identifier out of the callback resonse state paramater
and use it to set the current tenant for the request.