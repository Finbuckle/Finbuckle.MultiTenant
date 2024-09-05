# MultiTenant Strategies

A multi-tenant strategy is responsible for defining how the tenant is determined. It ultimately produces an identifier
string which is used to create a `TenantInfo` object with information from the [MultiTenant store](Stores).

Finbuckle.MultiTenant supports several "out-of-the-box" strategies for resolving the tenant. Custom strategies can be
created by implementing `IMultiTenantStrategy` or using `DelegateStrategy`.

## IMultiTenantStrategy and Custom Strategies

All MultiTenant strategies derive from `IMultiTenantStrategy` and must implement the `GetIdentifierAsync` method.

If an identifier can't be determined, `GetIdentifierAsync` should return null which will ultimately result in a
null `TenantInfo`.

Configure a custom implementation of `IMultiTenantStrategy` by calling `WithStrategy<TStrategy>`
after `AddMultiTenant<TTenantInfo>` in the `ConfigureServices` method of the `Startup` class. There are several
available
overrides for configuring the strategy. The first override uses dependency injection along with any passed parameters to
construct the implementation instance. The second override accepts a `Func<IServiceProvider, TStrategy>` factory method
for even more customization. The library internally decorates any `IMultiTenantStrategy` with a wrapper providing basic
logging and exception handling.

```csharp
// configure a strategy with a given type
builder.Services.AddMultiTenant<TenantInfo>()
    .WithStrategy<MyStrategy>(myParam1, myParam2)...

// or configure a strategy with a factory method
builder.Services.AddMultiTenant<TenantInfo>()
    .WithStrategy(sp => new MyStrategy())...
```

## Using Multiple Strategies

Multiple strategies can be registered after `AddMultiTenant<TTenantInfo>` and each strategy will be tried in the order
configured until a non-null identifier is returned and any remaining strategies are skipped.

Most out-of-the-box strategies are registered as singleton services so configuring them multiple times
after `AddMultiTenant<TTenantInfo>` is not recommended. The main use for configuring multiple strategies of the same
type is for several instances of `DelegateStrategy` utilizing distinct logic or other advanced scenarios.

## Static Strategy

> NuGet package: Finbuckle.MultiTenant

Always uses the same identifier to resolve the tenant. Often useful in testing or to resolve to a fallback or default
tenant by registering the strategy last.

Configure by calling `WithStaticStrategy` after `AddMultiTenant<TTenantInfo>` and passing in the identifier to use for
tenant resolution:

```csharp
builder.Services.AddMultiTenant<TenantInfo>()
    .WithStaticStrategy("MyTenant")
```

## Delegate Strategy

> NuGet package: Finbuckle.MultiTenant

Uses a provided `Func<object, Task<string>>` to determine the tenant. For example the lambda
function `async context => "initech"` would use "initech" as the identifier when resolving the tenant for every request.
This strategy is good to use for testing or simple logic. This strategy is configured multiple times and will run in the
order configured.

Configure by calling `WithDelegateStrategy` after `AddMultiTenant<TTenantInfo>` A `Func<object, Task<string?>>`is passed
in which will be used with each request to resolve the tenant. A lambda or async lambda can be used as the parameter:

```csharp
// use async logic to get the tenant identifier
builder.Services.AddMultiTenant<TenantInfo>()
    .WithDelegateStrategy(async context =>
    {
        string? tenantIdentifier = await DoSomethingAsync(context);
        return tenantIdentifier
    })...
    
 // or do it without async
builder.Services.AddMultiTenant<TenantInfo>()
    .WithDelegateStrategy(context =>
    {
        var httpContext = context as HttpContext;
        if (httpContext == null)
            return null;
        
        httpContext.Request.Query.TryGetValue("tenant", out StringValues tenantIdentifier);
        
        if (tenantIdentifier is null)
            return Task.FromValue<string?>(null);
        
        return Task.FromValue(tenantIdentifier.ToString());
    })...
```

## Base Path Strategy

> NuGet package: Finbuckle.MultiTenant.AspNetCore

Uses the base (i.e. first) path segment to determine the tenant. For example, a request
to "https://www.example.com/initech" would use "initech" as the identifier when resolving the tenant.

Configure by calling `WithBasePathStrategy` after `AddMultiTenant<TTenantInfo>`:

```csharp
builder.Services.AddMultiTenant<TenantInfo>()
    .WithBasePathStrategy()...
```

This strategy can also adjust the ASP.NET Core `Request.PathBase` and `Request.Path` variables so that subsequent
middleware checking `Request.Path` do not see the tenant identifier segment and generated relative urls include the
tenant base path automatically. This can be useful in some scenarios where an application makes certain assumptions
about paths that you otherwise cannot work around.

For example, a request to `https://mydomain.com/mytenant/mypath` by default has a `Request.PathBase` of `/` and
a `Request.Path` of `/mytenant/mypath`. Setting this option will adjust these values to `/mytenant` and `/mypath`
respectively when a tenant is successfully resolved with the `BasePathStrategy`.

```csharp
builder.Services.AddMultiTenant<TenantInfo>()
    .WithBasePathStrategy(options =>
    {
          options.RebaseAspNetCorePathBase = true;
    })...
```

Be aware that relative links to static files will be impacted so css files and other static resources may need to
be referenced using absolute urls. Alternatively, you can place the `UseStaticFiles` middleware after
the `UseMultiTenant` middware in the app pipeline configuration.

## Claim Strategy

> NuGet package: Finbuckle.MultiTenant.AspNetCore

Uses a claim to determine the tenant identifier. By default the first claim value with type `__tenant__` is used, but a
custom type name can also be used. This strategy uses the default authentication scheme, which is usually cookie based,
but does not go so far as to set `HttpContext.User`. Thus the ASP.NET Core authentication middleware should still be
used as normal, and in most use cases should come after `UseMultiTenant`.

Note that this strategy is does not work well with per-tenant cookie names since it must know the cookie name before the
tenant is resolved.

Configure by calling `WithClaimStrategy` after `AddMultiTenant<TTenantInfo>`. An overload to accept a custom claim type
is also available:

```csharp
// check for a claim type __tenant__
builder.Services.AddMultiTenant<TenantInfo>()
    .WithClaimStrategy()...

// check for a custom claim type
builder.Services.AddMultiTenant<TenantInfo>()
    .WithClaimStrategy("MyClaimType")...
```

## Session Strategy

> NuGet package: Finbuckle.MultiTenant.AspNetCore

Uses the ASP.NET Core session to retrieve the tenant identifier. This strategy is configured as a singleton.

Configure by calling `WithSessionStrategy` after `AddMultiTenant<TTenantInfo>`. Uses a default session key
named `__tenant__`. An overload of `WithSessionStrategy can be used to specify a different key name:

```csharp
// check for default "__tenant__" as the session key
builder.Services.AddMultiTenant<TenantInfo>()
    .WithSessionStrategy()...

// or check for a custom session key
builder.Services.AddMultiTenant<TenantInfo>()
    .WithSessionStrategy("my-tenant-session-key")...
```

Note that an app will have
to [configure session state](https://docs.microsoft.com/en-us/aspnet/core/fundamentals/app-state#session-state)
accordingly and then actually set the session variable. A typical use case is to register the session strategy before a
more expensive strategy. The expensive strategy can set the session value so that for subsequent requests resolve the
tenant without invoking the expensive strategy.

## Route Strategy

> NuGet package: Finbuckle.MultiTenant.AspNetCore

Uses the `__tenant__` route parameter (or a specified route parameter) to determine the tenant. For example, a request
to "https://www.example.com/initech/home/" and a route configuration of `{__tenant__}/{controller=Home}/{action=Index}`
would use "initech" as the identifier when resolving the tenant. The `__tenant__` parameter can be placed anywhere in
the route path configuration. If explicity calling `UseRouting` in your app pipline make sure to place it
before `WithRouteStrategy`.

Configure by calling `WithRouteStrategy` after `AddMultiTenant<TTenantInfo>`. A custom route parameter can also be
configured:

```csharp
// use the default route parameter name "__tenant__"
builder.Services.AddMultiTenant<TenantInfo>()
  .WithRouteStrategy()...
    
// or set a different route parameter name of "MyTenantRouteParam"
builder.Services.AddMultiTenant<TenantInfo>()
  .WithRouteStrategy("MyTenantRouteParam")...

// UseRouting is optional in ASP.NET Core, but if needed place before UseMultiTenant when the route strategy used
app.UseRouting();
app.UseMultiTenant();
```

## Host Strategy

> NuGet package: Finbuckle.MultiTenant.AspNetCore

Uses request's host value to determine the tenant. By default the first host segment is used. For example, a request
to "https://initech.example.com/abc123" would use "initech" as the identifier when resolving the tenant. This strategy
can be difficult to use in a development environment. Make sure the development system is configured properly to allow
subdomains on `localhost`. This strategy is configured as a singleton.

The host strategy uses a template string which defines how the strategy will find the tenant identifier. The pattern
specifies the location for the tenant identifier using "\_\_tenant\_\_" and can contain other valid domain characters.
It can also use '?' and '\*' characters to represent one or "zero or more" segments. For example:

- `__tenant__.*` is the default if no pattern is provided and selects the first (or only) domain segment as the tenant
  identifier.
- `*.__tenant__.?` selects the main domain (or second to last) as the tenant identifier.
- `__tenant__.example.com` will always use the subdomain for the tenant identifier, but only if there are no prior
  subdomains and the overall host ends with "example.com".
- `*.__tenant__.?.?` is similar to the above example except it will select the first subdomain even if others exist and
  doesn't require ".com".
- As a special case, a pattern string of just `__tenant__` will use the entire host as the tenant identifier, as opposed
  to a single segment.

Configure by calling `WithHostStrategy` after `AddMultiTenant<TTenantInfo>` in the `ConfigureServices` method of
the `Startup`
class. A template pattern can be specified with the overloaded version:

```csharp
// check the first domain segment (e.g. subdomain)
builder.Services.AddMultiTenant<TenantInfo>()
    .WithHostStrategy()...

// check the second level domain segment (see 2nd example above)
builder.Services.AddMultiTenant<TenantInfo>()
    .WithHostStrategy("*.__tenant__.?")...
```

## Header Strategy

> NuGet package: Finbuckle.MultiTenant.AspNetCore

Uses an HTTP request header to determine the tenant identifier. By default the header with key `__tenant__` is used, but
a custom key can also be used.

Configure by calling `WithHeaderStrategy` after `AddMultiTenant<TTenantInfo>`. An overload to accept a custom claim type
is also available:

```csharp
// check for header "__tenant__" value
builder.Services.AddMultiTenant<TenantInfo>()
    .WithHeaderStrategy()...

// or check for custom header value
builder.Services.AddMultiTenant<TenantInfo>()
    .WithHeaderStrategy("MyHeaderKey")...
```

## Remote Authentication Callback Strategy

> NuGet package: Finbuckle.MultiTenant.AspNetCore

This is a special strategy used for per-tenant authentication when remote authentication such as OpenID Connect or
OAuth (e.g. Log in via Facebook) are used.

The strategy is configured internally when `WithPerTenantAuthentication` is called to
configure [per-tenant authentication](Authentication).