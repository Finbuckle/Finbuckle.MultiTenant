# MultiTenant Strategies

A multitenant strategy is responsible for defining how the tenant is determined. It ultimately produces an identifier string which is used to create a `TenantInfo` object with information from the [MultiTenant store](Stores).

Finbuckle.MultiTenant supports several "out-of-the-box" strategies for resolving the tenant. Custom strategies can be created by implementing `IMultiTenantStrategy` or using `DelegateStrategy`.

The `Strategy` property on the `StrategyInfo` member of `MultiTenantContext` instance returned by `HttpContext.GetMultiTenantContext()` returns the actual strategy used to resolve the tenant information for the current context.

## IMultiTenantStrategy and Custom Strategies
All multitenant strategies derive from `IMultiTenantStrategy` and must implement the `GetIdentifierAsync` method. 

If an identifier can't be determined, `GetIdentifierAsync` should return null which will ultimately result in a null `TenantInfo`.

Configure a custom implementation of `IMultiTenantStrategy` by calling `WithStrategy<TStrategy>` after `AddMultiTenant<T>` in the `ConfigureServices` method of the `Startup` class. There are several available overrides for configuring the strategy. The first override uses dependency injection along with any passed parameters to construct the implementation instance. The second override accepts a `Func<IServiceProvider, TStrategy>` factory method for even more customization. The library internally decorates any `IMultiTenantStrategy` with a wrapper providing basic logging and exception handling.

```cs
// Register a custom strategy with the templated method.
services.AddMultiTenant<TenantInfo>()
        .WithStrategy<MyStrategy>(myParam1, myParam2)...

// Or register a custom strategy with the overload method accepting a factory method.
// Note that the type parameter for WithStrategy is inferred by the compiler.
services.AddMultiTenant<TenantInfo>()
        .WithStrategy(sp => new MyStrategy())...
```

## Accessing the Strategies at Runtime
MultiTenant strategies are registered in the dependency injection system under the
`IMultiTenantStrategy` service type.

If multiple strategies are registered a specific one can be retrieving an
`IEnumerable<IMultiTenantStrategy>` and filtering to the specific implementation
type:

```cs
// Assume we have a service provider. The IEnumerable could be injected via
// other DI means as well.
var strategy = serviceProvider.GetService<IEnumerable<IMultiTenantStrategy>>
                              .Where(s => s.ImplementationType == typeof(StaticStrategy))
                              .SingleOrDefault();
```

## Using Multiple Strategies
Multiple strategies can be registered after `AddMultiTenant<T>` and each strategy will be tried in the order configured until a non-null identifier is returned. The remaining strategies are skipped for that request.

Note that some strategies are registered as singleton service so registering them multiple times after `AddMultiTenant<T>` is not recommended. The main use for registering multiple strategies of the same type is using several instances of the `DelegateStrategy` utilizing distinct logic.

## Static Strategy
> NuGet package: Finbuckle.MultiTenant

Always uses the same identifier to resolve the tenant. Often useful in testing or to resolve to a fallback or default tenant by registering the strategy last. This strategy is configured as a singleton.

Configure by calling `WithStaticStrategy` after `AddMultiTenant<T>` in the `ConfigureServices` method of the `Startup` class and passing in the identifier to use for tenant resolution:

```cs
services.AddMultiTenant<TenantInfo>()
        .WithStaticStrategy("MyTenant")...
```

## Delegate Strategy
> NuGet package: Finbuckle.MultiTenant

Uses a provided `Func<object, Task<string>>` to determine the tenant. For example the lambda function `async context => "initech"` would use "initech" as the identifier when resolving the tenant for every request. This strategy is good to use for testing or simple logic. This strategy is configured as transient and multiple instances can be registered.

Configure by calling `WithDelegateStrategy` after `AddMultiTenant<T>` in the `ConfigureServices` method of the `Startup` class. A `Func<object, Task<string>>`is passed in which will be used with each request to resolve the tenant. A lambda or async lambda can be used as the parameter:

```cs
// Use the request query parameter "tenant" to get the tenantId:
services.AddMultiTenant<TenantInfo>()
        .WithDelegateStrategy(async context =>
        {
            var httpContext = context as HttpContext;
            if(httpContext == null)
                return null;

            httpContext.Request.Query.TryGetValue("tenant", out StringValues tenantIdParam);
            return tenantIdParam.ToString();
        })...
```

## Base Path Strategy
> NuGet package: Finbuckle.MultiTenant.AspNetCore

Uses the base (i.e. first) path segment to determine the tenant. For example, a request to "https://www.example.com/initech" would use "initech" as the identifier when resolving the tenant. This strategy is configured as a singleton.

Configure by calling `WithBasePathStrategy` after `AddMultiTenant<T>` in the `ConfigureServices` method of the `Startup` class:

```cs
services.AddMultiTenant<TenantInfo>()
        .WithBasePathStrategy()...
```

## Claim Strategy
> NuGet package: Finbuckle.MultiTenant.AspNetCore

Uses a claim to determine the tenant identifier. By default the first claim
value with type `__tenant__` is used, but a custom type name can also be used.
This strategy uses the default authentication scheme, which is usually cookie
based, but does not go so far as to set `HttpContext.User`. Thus the ASP.NET
Core authentication middleware should still be used as normal, and in most use
cases should come after `UseMultiTenant` when using `ClaimsStrategy`. Due to how
the authentication middleware is implemented there is practically no performance
penalty when used in conjunction with the `ClaimStrategy`.

Note that this strategy is does not work well with per-tenant cookie names since
it must know the cookie name before the tenant is resolved.

Configure by calling `WithClaimStrategy` after `AddMultiTenant<T>` in the
`ConfigureServices` method of the `Startup` class. An overload to accept a
custom claim type is also available:

```cs
// This will check for a claim type __tenant__
services.AddMultiTenant<TenantInfo>()
        .WithClaimStrategy()...

// This will check for a custom claim type
services.AddMultiTenant<TenantInfo>()
        .WithClaimStrategy("MyClaimType")...
```
## Session Strategy
> NuGet package: Finbuckle.MultiTenant.AspNetCore

Uses the ASP.NET Core session to retrieve the tenant identifier. This strategy is configured as a singleton.

Configure by calling `WithSessionStrategy` after `AddMultiTenant<T>` in the `ConfigureServices` method of the `Startup` class. This will use a default session key named `__tenant__`. An overload of `WithSessionStrategy can be used to specify a different key name:

```cs
// Configure to use "__tenant__" as the session key,
services.AddMultiTenant<TenantInfo>()
        .WithSessionStrategy()...

// Or configure to use "my-tenant-session-key" as the session key,
services.AddMultiTenant<TenantInfo>()
        .WithSessionStrategy("my-tenant-session-key")...
```

Note that an app will have to [configure session state](https://docs.microsoft.com/en-us/aspnet/core/fundamentals/app-state?view=aspnetcore-3.1#session-state) accordingly and then actually set the session variable. A typical use case is to register the session strategy before a more expensive strategy. The expensive strategy can set the session value so that for subsequent requests resolve the tenant without invoking the expensive strategy.

## Route Strategy
> NuGet package: Finbuckle.MultiTenant.AspNetCore

Note: the configuration and use of this strategy differs in ASP.NET Core 2.1 and ASP.NET Core 3.1+.

Uses the `__tenant__` route parameter (or a specified route parameter) to determine the tenant. For example, a request to "https://www.example.com/initech/home/" and a route configuration of `{__tenant__}/{controller=Home}/{action=Index}` would use "initech" as the identifier when resolving the tenant. The `__tenant__` parameter can be placed anywhere in the route path configuration. This strategy is configured as a singleton.

**ASP.NET Core 3 or higher**
 The route strategy is improved in ASP.NET Core 3 due to the new endpoint routing mechanism. Configure by calling `WithRouteStrategy` after `AddMultiTenant<T>` in the `ConfigureServices` method of the `Startup` class. A different route parameter name can be specified with the overloaded version. Then in the app pipeline make sure to call `UseRouting` before `UseMultiTenant`:

```cs
public class Startup
{
    public void ConfigureServices(IServiceCollection services)
    {
        // Other services...

        // Use the default route parameter name "__tenant__":
        services.AddMultiTenant<TenantInfo>()
                .WithRouteStrategy()...
        
        // Alternatively set a different route parameter name of "MyTenantRouteParam":
        services.AddMultiTenant<TenantInfo>()
                .WithRouteStrategy("MyTenantRouteParam")...
        
        // Other services...

        services.AddMvc();

        // Other services...
    }

    public void Configure(IAppBuilder app, ...)
    {
        // Other middleware...

        app.UseRouting(); // Important!

        // Other middleware...

        app.UseMultiTenant();
        
        // Other middleware...
        
        app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllerRoute("default", "{__tenant__}/{controller=Home}/{action=Index}");
            });
    }
}
```

**ASP.NET Core 2.1**
Configure by calling `WithRouteStrategy` after `AddMultiTenant<T>` in the `ConfigureServices` method of the `Startup` class. An `Action<IRouteBuilder>` parameter which configures the routing is passed in which should always match the routes configured with MVC.  A different route parameter name can be specified with the overloaded version:

```cs
public class Startup
{
    public void ConfigureServices(IServiceCollection services)
    {
        // Other services...

        // Use the default route parameter name "__tenant__":
        services.AddMultiTenant<TenantInfo>()
                .WithRouteStrategy(configRoutes)...
        
        // Alternatively set a different route parameter name of "MyTenantRouteParam":
        services.AddMultiTenant<TenantInfo>()
                .WithRouteStrategy("MyTenantRouteParam", configRoutes)...
        
        // Other services...

        services.AddMvc();

        // Other services...
    }

    public void Configure(IAppBuilder app, ...)
    {
        // Other middleware...

        app.UseMultiTenant();
        
        // Other middleware...
        
        app.UseMvc(ConfigRoutes);
    }

    private void ConfigRoutes(IRouteBuilder routes)
    {
        routes.MapRoute("Default", "{__tenant__}/{controller=Home}/{action=Index}");
    }
}
```

## Host Strategy
> NuGet package: Finbuckle.MultiTenant.AspNetCore

Uses request's host value to determine the tenant. By default the first host segment is used. For example, a request to "https://initech.example.com/abc123" would use "initech" as the identifier when resolving the tenant. This strategy can be difficult to use in a development environment. Make sure the development system is configured properly to allow subdomains on `localhost`. This strategy is configured as a singleton.

The host strategy uses a template string which defines how the strategy will find the tenant identifier. The pattern specifies the location for the tenant identifier using "\_\_tenant\_\_" and can contain other valid domain characters. It can also use '?' and '\*' characters to represent one or "zero or more" segments. For example:
  - `__tenant__.*` is the default if no pattern is provided and selects the first domain segment for the tenant identifier.
  - `*.__tenant__.?` selects the main domain as the tenant identifier and ignores any subdomains and the top level domain.
  - `__tenant__.example.com` will always use the subdomain for the tenant identifier, but only if there are no prior subdomains and the overall host ends with "example.com".
  - `*.__tenant.__.?.?` is similar to the above example except it will select the first subdomain even if others exist and doesn't require ".com".
  - As a special case, a pattern string of just `__tenant__` will use the entire host as the tenant identifier, as opposed to a single segment.

Configure by calling `WithHostStrategy` after `AddMultiTenant<T>` in the `ConfigureServices` method of the `Startup` class. A template pattern can be specified with the overloaded version:

```cs
// Use the default template "__tenant__.*":
services.AddMultiTenant<TenantInfo>()
        .WithHostStrategy()...

// Set a template which selects the main domain segment (see 2nd example above):
services.AddMultiTenant<TenantInfo>()
        .WithHostStrategy("*.__tenant__.?")...
```

## Header Strategy
> NuGet package: Finbuckle.MultiTenant.AspNetCore

Uses an HTTP request header to determine the tenant identifier. By default the header
with key `__tenant__` is used, but a custom key can also be used.

Configure by calling `WithHeaderStrategy` after `AddMultiTenant<T>` in the
`ConfigureServices` method of the `Startup` class. An overload to accept a
custom claim type is also available:

```cs
// This will check for a claim type __tenant__
services.AddMultiTenant<TenantInfo>()
        .WithHeaderStrategy()...

// This will check for a custom claim type
services.AddMultiTenant<TenantInfo>()
        .WithHeaderStrategy("MyHeaderKey")...
```

## Remote Authentication Callback Strategy
> NuGet package: Finbuckle.MultiTenant.AspNetCore

This is a special strategy used for per-tenant authentication when remote authentication such as OpenID Connect or OAuth (e.g. Log in via Facebook) are used. This strategy is configured as a singleton.

The strategy is configured internally when `WithPerTenantAuthentication` is called
to configure [per-tenant authentication](Authentication).