# Per-Tenant Options

Finbuckle.MultiTenant integrates with the standard ASP.NET Core [Options pattern](https://docs.microsoft.com/en-us/aspnet/core/fundamentals/configuration/options) and lets apps customize options distinctly for each tenant. A specialized variation of this is [per-tenant authentication](Authentication).

Per-tenant options will work with *any* options class when using `IOptions<TOptions>`, `IOptionsSnapshot<TOptions>`, or `IOptionsMonitor<TOptions>` for dependency injection or service resolution. This includes an app's own code *and* code internal to ASP.NET Core or other libraries that use the Options pattern. Use with caution: ASP.NET Core and other libraries may internally cache options or exhibit other unexpected behavior resulting in the wrong option values!

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

Call `WithPerTenantOptions<TOptions>` after `AddMultiTenant` in the `ConfigureServices` method:

```cs
services.AddMultiTenant()...
    .WithPerTenantOptions<MyOptions>((options, tenantContext) =>
    {
        options.MyOption1 = (int)tenantContext.Items["someValue"];
        options.MyOption2 = (int)tenantContext.Items["anotherValue"];
    });
```

The type parameter `TOptions` is the options type being customized per-tenant. The method parameter is an `Action<TOptions, TenantContext>`. This action will modify the options instance *after* the options normal configuration and *before* its [post configuration](https://docs.microsoft.com/en-us/aspnet/core/fundamentals/configuration/options?#ipostconfigureoptions).


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
