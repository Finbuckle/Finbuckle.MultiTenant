# Per-Tenant Options
Finbuckle.MultiTenant integrates with the standard ASP.NET Core [Options pattern](https://docs.microsoft.com/en-us/aspnet/core/fundamentals/configuration/options) and lets apps customize options distinctly for each tenant. The current tenant determines which options are retrieved via the `IOptions<TOptions>` (or derived) instance's `Value` property and `Get(string name)` method.

 A specialized variation of this is [per-tenant authentication](Authentication).

Per-tenant options will work with *any* options class when using `IOptions<TOptions>`, `IOptionsSnapshot<TOptions>`, or `IOptionsMonitor<TOptions>` with dependency injection or service resolution. This includes an app's own code *and* code internal to ASP.NET Core or other libraries that use the Options pattern. There is one potential caveat: ASP.NET Core and other libraries may internally cache options or exhibit other unexpected behavior resulting in the wrong option values!

Consider a typical scenario in ASP.Net Core, starting with a simple class:

```cs
public class MyOptions
{
    public int Option1 { get; set; }
    public int Option2 { get; set; }
}
```

In the `ConfigureServices` method of the startup class, `services.Configure<MyOptions>` is called with a delegate or `IConfiguration` parameter to set the option values:

```cs
public class Startup
{
    public void ConfigureServices(IServiceCollection services)
    {
        services.Configure<MyOptions>(options => options.Option1 = 1);
        
        // Other services configured here...
    }
}
```

Dependency injection of `IOptions<MyOptions>` into a controller (or anywhere DI can be used) provides access to the options values, which are the same for every tenant at this point:

```cs
public MyController : Controller
{
    private readonly MyOptions _myOptions;
    
    public MyController(IOptionsMonitor<MyOptions> optionsAccessor)
    {
        // Same options regardless of the current tenant.
        _myOptions = optionsAccessor.Value;
    }
}
```

## Customizing Options Per Tenant
This sections assumes Finbuckle.MultiTenant is installed and configured. See [Getting Started](GettingStarted) for details.

Call `WithPerTenantOptions<TOptions>` after `AddMultiTenant<T>` in the `ConfigureServices` method:

```cs
services.AddMultiTenant<TenantInfo>()...
        .WithPerTenantOptions<MyOptions>((options, tenantInfo) =>
        {
            options.MyOption1 = (int)tenantInfo.Items["someValue"];
            options.MyOption2 = (int)tenantInfo.Items["anotherValue"];
        });
```

The type parameter `TOptions` is the options type being customized per-tenant. The method parameter is an `Action<TOptions, TenantInfo>`. This action will modify the options instance *after* the options normal configuration and *before* its [post configuration](https://docs.microsoft.com/en-us/aspnet/core/fundamentals/configuration/options?#ipostconfigureoptions).

`WithPerTenantOptions<TOptions>` can be called multiple times on the same `TOptions`
type and the configuration will run in the respective order.

Now with the same controller example from above, the option values will be specific to the current tenant:

```cs
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

## Named Options
Both named and unnamed options are modified per-tenant. The same delegate passed to `WithPerTenantOptions<TOptions>` is applied to all options generated of type `TOptions` regardless of the option name.

## Options Caching
Internally ASP.NET Core caches options, and Finbuckle.MultiTenant extends this to cache options per tenant. Caching occurs when a `TOptions` instance is retrieved via `Value` or `Get` on the injected `IOptions<TOptions>` (or derived) instance for the first time for a tenant.

`IOptions<TOptions>` instances are always regenerated when injected so any caching only lasts as long as the specific instance.

`IOptionsSnapshot<TOptions>` instances are generated once per HTTP request and caching will last throughout the entire request.

`IOptionsMonitor<TOptions>` instances persist across HTTP requests and caching can persist for long periods of time.

In some situations cached options may need to be cleared so that the options can be regenerated.

When using per-tenant options via `IOptions<TOptions>` and `IOptionsSnapshot<TOptions>` the injected instance is of type `MultiTenantOptionsManager<TOptions>`. Casting to this type exposes the `Reset()` method which clears any internal caching for the current tenant and cause the options to be regenerated when next accessed via `Value` or `Get(string name)`.

When using per-tenant options with `IOptionsMonitor<TOptions>` each injected instance uses a shared persistent cache. This cache can be retrieved by injecting or resolving an instance of `IOptionsMonitorCache<TOptions>` which has a `Clear()` method that will clear the cache for the current tenant. Casting the `IOptionsMonitorCache<TOptions>` instance to `MultiTenantOptionsCache<TOptions>` exposes the `Clear(string tenantId)` and `ClearAll()` methods. `Clear(string tenantId)` clears cached options for a specific tenant (or the regular non per-tenant options if the parameter is empty or null). `ClearAll()` clears all cached options (including regular non per-tenant options).