# Per-Tenant Options

> Add the `Finbuckle.MultiTenant.Options` package to your project to use this functionality.

MultiTenant is designed to emphasize using per-tenant options in your app to drive per-tenant behavior. This
approach allows your app logic to be written without having to add tenant-dependent or tenant-specific logic directly to
the code.

By using per-tenant options, the options values used within app logic will automatically
reflect the per-tenant values as configured for the current tenant. Any code already using the Options pattern will gain
multi-tenant capability with minimal code changes.

MultiTenant integrates with the
standard [.NET Options pattern](https://learn.microsoft.com/en-us/dotnet/core/extensions/options) (see also the [ASP.NET
Core Options pattern](https://docs.microsoft.com/en-us/aspnet/core/fundamentals/configuration/options)) and lets apps
customize options distinctly for each tenant.

Note: For authentication options, MultiTenant provides special support
for [per-tenant authentication](Authentication).

The current tenant determines which options are retrieved via
the `IOptions<TOptions>`, `IOptionsSnapshot<TOptions>`, or `IOptionsMonitor<TOptions>` instances' `Value` property and
`Get(string name)` method.

Per-tenant options will work with *any* options class when using `IOptions<TOptions>`, `IOptionsSnapshot<TOptions>`,
or `IOptionsMonitor<TOptions>` with dependency injection or service resolution. This includes your own code *and*
code internal to ASP.NET Core or other libraries that use the Options pattern.

> ⚠️ Avoid storing an options instance in a static field or singleton. Always resolve options from the provided accessor
> so the tenant-specific cache can refresh correctly.

A potential issue arises when code internally stores or caches options values from
an `IOptions<TOptions>`, `IOptionsSnapshot<TOptions>`, or `IOptionsMonitor<TOptions>` instance. This is usually
unnecessary because the options are already cached within the .NET options infrastructure, and in these cases the
initial instance of the options is always used, regardless of the current tenant. MultiTenant works around
this for some parts of
ASP.NET Core, and recommends that in your own code to always access options values via
the `IOptions<TOptions>`, `IOptionsSnapshot<TOptions>`, or `IOptionsMonitor<TOptions>` instance. This will ensure the
correct values for the current tenant are used.

## Options Basics

Consider a typical scenario in ASP.NET Core, starting with a simple class:

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
httpContext.RequestServices.GetServices<IOptionsSnapshot<MyOptions>>();
```

With standard options each tenant would see the same exact options.

## Customizing Options Per Tenant

This section assumes a standard web application builder is configured and MultiTenant is configured with
a `TTenantInfo` type of `TenantInfo`.
See [Getting Started](GettingStarted) for details.

Make sure to add the `Finbuckle.MultiTenant.Options` package to your project.

To configure options per tenant, the standard `Configure` method variants on the service collection now all
have `PerTenant` equivalents which accept a `Action<TOptions, TTenantInfo>` delegate. When the options are created at
runtime the delegate will be called with the current tenant details.

```csharp
using Finbuckle.MultiTenant.Options.OptionsBuilderExtensions;
using Finbuckle.MultiTenant.Options.ServiceCollectionExtensions;

// configure options per tenant
builder.Services.ConfigurePerTenant<MyOptions, TenantInfo>((options, tenantInfo) =>
    {
        options.MyOption1 = tenantInfo.Option1Value;
        options.MyOption2 = tenantInfo.Option2Value;
    });

// or configure named options per tenant
builder.Services.ConfigurePerTenant<MyOptions, TenantInfo>("scheme2", (options, tenantInfo) =>
    {
        options.MyOption1 = tenantInfo.Option1Value;
        options.MyOption2 = tenantInfo.Option2Value;
    });

// ConfigureAll options variant
builder.Services.ConfigureAllPerTenant<MyOptions, TenantInfo>((options, tenantInfo) =>
    {
        options.MyOption1 = tenantInfo.Option1Value;
        options.MyOption2 = tenantInfo.Option2Value;
    });

// can also configure post options, named post options, and all post options variants
builder.Services.PostConfigurePerTenant<MyOptions, TenantInfo>((options, tenantInfo) =>
    {
        options.MyOption1 = tenantInfo.Option1Value;
        options.MyOption2 = tenantInfo.Option2Value;
    });

builder.Services.PostConfigurePerTenant<MyOptions, TenantInfo>("scheme2", (options, tenantInfo) =>
    {
        options.MyOption1 = tenantInfo.Option1Value;
        options.MyOption2 = tenantInfo.Option2Value;
    });

builder.Services.PostConfigureAllPerTenant<MyOptions, TenantInfo>((options, tenantInfo) =>
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
the standard [Options pattern](https://learn.microsoft.com/en-us/dotnet/core/extensions/options). MultiTenant
extends this API to enable options configuration for per-tenant options similarly. Note that while the `OptionsBuilder`
normally supports up to five dependencies, MultiTenant support only supports four.

```csharp
// use OptionsBuilder API to configure per-tenant options with dependencies
builder.Services.AddOptions<MyOptions>("optionalName")
    .ConfigurePerTenant<ExampleService, TenantInfo>(
        (options, exampleService, tenantInfo) =>
            options.Property = DoSomethingWith(exampleService, tenantInfo));```

## Options and Caching

Internally .NET caches options, and MultiTenant extends this to cache options per tenant. Caching
occurs when a `TOptions` instance is retrieved via `Value` or `Get` on the injected `IOptions<TOptions>` (or derived)
instance for the first time for a tenant.

`IOptions<TOptions>` instances are scoped, but use a shared singleton per-tenant cache similar to how the standard .NET
`IOptions<TOptions>` work. Options are generated when first requested and cached until cleared.

`IOptionsSnapshot<TOptions>` instances are scoped and options are generated and cached for each scope lifetime. In
ASP.NET Core this means that options will be generated and cached for the duration of a request.

`IOptionsMonitor<TOptions>` accessor instances are scoped unlike in standard .NET where they are singletons. However,
the caching and source change tracking behavior are preserved. Options are generated at first request and cached in a
singleton. If a source change triggers all impacted options are cleared from the cache and any registered change
listeners are notified with updated options values.

In some situations cached options may need to be cleared so that the options can be regenerated.

When using per-tenant options with `IOptions<TOptions>` and `IOptionsSnapshot<TOptions>` the injected instance is of
type `MultiTenantOptionsManager<TOptions>`. Casting to this type exposes the `Reset()` method which clears any internal
caching for the current tenant and cause the options to be regenerated when next accessed via `Value`
or `Get(string name)`.

When using per-tenant options with `IOptionsMonitor<TOptions>`, the shared monitor cache is
`MultiTenantOptionsCache<TOptions>`. Its `Clear(string tenantId)` and `ClearAll()` methods can be used to invalidate
cached entries manually. `Clear(string tenantId)` clears cached options for a specific tenant (or regular non
per-tenant options if the parameter is empty or null). `ClearAll()` clears all cached options (including regular
non per-tenant options).
