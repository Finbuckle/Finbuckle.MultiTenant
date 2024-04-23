# Per-Tenant Options

Finbuckle.MultiTenant is designed to emphasize using per-tenant options in an app to drive per-tenant behavior. This
approach allows app logic to be written having to add tenant-dependent or tenant-specific logic to the code.

By using per-tenant options, the options values used within app logic will automatically
reflect the per-tenant values as configured for the current tenant. Any code already using the Options pattern will gain
multi-tenant capability with minimal code changes.

Finbuckle.MultiTenant integrates with the
standard [.NET Options pattern](https://learn.microsoft.com/en-us/dotnet/core/extensions/options) (see also the [ASP.NET
Core Options pattern](https://docs.microsoft.com/en-us/aspnet/core/fundamentals/configuration/options) and lets apps
customize options distinctly for each tenant.

Note: For authentication options, Finbuckle.MultiTenant provides special support
for [per-tenant authentication](Authentication).

The current tenant determines which options are retrieved via
the `IOptions<TOptions>`, `IOptionsSnapshot<TOptions>`, or `IOptionsMonitor<TOptions>` instances' `Value` property and
`Get(string name)` method.

Per-tenant options will work with *any* options class when using `IOptions<TOptions>`, `IOptionsSnapshot<TOptions>`,
or `IOptionsMonitor<TOptions>` with dependency injection or service resolution. This includes an app's own code *and*
code internal to ASP.NET Core or other libraries that use the Options pattern.

A potential issue arises when code internally stores or caches options values from
an `IOptions<TOptions>`, `IOptionsSnapshot<TOptions>`, or `IOptionsMonitor<TOptions>` instance. This is usually
unnecessary because the options are already cached within the .NET options infrastructure, and in these cases the
initial instance of the options is always used, regardless of the current tenant. Finbuckle.MultiTenant works around
this for some parts of
ASP.NET Core, and recommends that in your own code to always access options values via
the `IOptions<TOptions>`, `IOptionsSnapshot<TOptions>`, or `IOptionsMonitor<TOptions>` instance. This will ensure the
correct values for the current tenant are used.

## Options Basics

Consider a typical scenario in ASP.Net Core, starting with a simple class:

```csharp
public class MyOptions
{
    public int Option1 { get; set; }
    public int Option2 { get; set; }
}
```

In the app configuration, `services.Configure<MyOptions>` is called with a delegate
or `IConfiguration` parameter to set the option values:

```csharp
var builder = WebApplication.CreateBuilder(args);

builder.Services.Configure<MyOptions>(options => options.Option1 = 1);
        
 // ...rest of app code
```

Dependency injection of `IOptions<MyOptions>` or its siblings into a class constructor, such as a controller, provides
access to the options values. A service provider instance can also provide access to the options values.

```csharp
// access options via dependency injection in a class constructor
public MyController : Controller
{
    private readonly MyOptions _myOptions;
    
    public MyController(IOptionsMonitor<MyOptions> optionsAccessor)
    {
        // same options regardless of the current tenant
        _myOptions = optionsAccessor.Value;
    }
}

// or with a service provider
httpContext.RequestServices.GetServices<IOptionsSnaption<MyOptions>();
```

With standard options each tenant would get see the same exact options.

## Customizing Options Per Tenant

This sections assumes a standard web application builder is configured and Finbuckle.MultiTenant is configured with
a `TTenantInfo` type of `TenantInfo`.
See [Getting Started](GettingStarted) for details.

To configure options per tenant, the standard `Configure` method variants on the service collection now all
have `PerTenant` equivalents which accept a `Action<TOptions, TTenantInfo>` delegate. When the options are created at
runtime the delegate will be called with the current tenant details.

```csharp
// configure options per tenant
builder.Services.ConfigurePerTenant<MyOptions, Tenantnfo>((options, tenantInfo) =>
    {
        options.MyOption1 = tenantInfo.Option1Value;
        options.MyOption2 = tenantInfo.Option2Value;
    });

// or configure named options per tenant
builder.Services.ConfigurePerTenant<MyOptions, Tenantnfo>("scheme2", (options, tenantInfo) =>
    {
        options.MyOption1 = tenantInfo.Option1Value;
        options.MyOption2 = tenantInfo.Option2Value;
    });

// ConfigureAll options variant
builder.Services.ConfigureAllPerTenant<MyOptions, Tenantnfo>((options, tenantInfo) =>
    {
        options.MyOption1 = tenantInfo.Option1Value;
        options.MyOption2 = tenantInfo.Option2Value;
    });

// can also configure post options, named post options, and all post options variants
builder.Services.PostConfigurePerTenant<MyOptions, Tenantnfo>((options, tenantInfo) =>
    {
        options.MyOption1 = tenantInfo.Option1Value;
        options.MyOption2 = tenantInfo.Option2Value;
    });

builder.Services.PostConfigurePerTenant<MyOptions, Tenantnfo>("scheme2", (options, tenantInfo) =>
    {
        options.MyOption1 = tenantInfo.Option1Value;
        options.MyOption2 = tenantInfo.Option2Value;
    });

builder.Services.PostConfigureAllPerTenant<MyOptions, Tenantnfo>((options, tenantInfo) =>
    {
        options.MyOption1 = tenantInfo.Option1Value;
        options.MyOption2 = tenantInfo.Option2Value;
    });
```

Now with the same controller example from above, the option values will be specific to the current tenant:

```csharp
public MyController : Controller
{
    private readonly MyOptions _myOptions;

    public MyController(IOptionsMonitor<MyOptions> optionsAccessor)
    {
        // _myOptions.MyOptions1 and .MyOptions2 will be specific to the current tenant.
        _myOptions = optionsAccessor.Value;
    }
}
```

## Using the OptionsBuilder API

.NET provides
the [OptionsBuilder](https://learn.microsoft.com/en-us/aspnet/core/fundamentals/configuration/options?view=aspnetcore-8.0#optionsbuilder-api)
API to provide more flexibility for configuring options. This pattern simplifies dependency injection and validation for
the standard [Options pattern](https://learn.microsoft.com/en-us/dotnet/core/extensions/options). Finbuckle.MultiTenant
extends this API to enable options configuration for per-tenant options similarly. Note that while the `OptionsBuilder`
normally supports up to five dependencies, Finbuckle.MultiTenant support only supports four.

```csharp
// use OptionsBuilder API to configure per-tenant options with dependencies
builder.Services.AddOptions<MyOptions>("optionalName")
    .ConfigurePerTenant<ExampleService, TenantInfo>(
        (options, es, tenantInfo) =>
            options.Property = DoSomethingWith(es, tenantInfo));
```

## Options and Caching

Internally .NET caches options, and Finbuckle.MultiTenant extends this to cache options per tenant. Caching
occurs when a `TOptions` instance is retrieved via `Value` or `Get` on the injected `IOptions<TOptions>` (or derived)
instance for the first time for a tenant.

`IOptions<TOptions>` instances are always regenerated when injected so any caching only lasts as long as the specific
instance.

`IOptionsSnapshot<TOptions>` instances are generated once per HTTP request and caching will last throughout the entire
request.

`IOptionsMonitor<TOptions>` instances persist across HTTP requests and caching can persist for long periods of time.

In some situations cached options may need to be cleared so that the options can be regenerated.

When using per-tenant options via `IOptions<TOptions>` and `IOptionsSnapshot<TOptions>` the injected instance is of
type `MultiTenantOptionsManager<TOptions>`. Casting to this type exposes the `Reset()` method which clears any internal
caching for the current tenant and cause the options to be regenerated when next accessed via `Value`
or `Get(string name)`.

When using per-tenant options with `IOptionsMonitor<TOptions>` each injected instance uses a shared persistent cache.
This cache can be retrieved by injecting or resolving an instance of `IOptionsMonitorCache<TOptions>` which has
a `Clear()` method that will clear the cache for the current tenant. Casting the `IOptionsMonitorCache<TOptions>`
instance to `MultiTenantOptionsCache<TOptions>` exposes the `Clear(string tenantId)` and `ClearAll()`
methods. `Clear(string tenantId)` clears cached options for a specific tenant (or the regular non per-tenant options if
the parameter is empty or null). `ClearAll()` clears all cached options (including regular non per-tenant options).