# ![Finbuckle Logo](https://www.finbuckle.com/images/finbuckle-32x32-gh.png) Finbuckle.MultiTenant 6.2.0

## About Finbuckle.MultiTenant

Finbuckle.MultiTenant is open source multitenancy middleware library for .NET. It enables tenant resolution, per-tenant app behavior, and per-tenant data isolation. See [https://www.finbuckle.com/multitenant](https://www.finbuckle.com/multitenant) for more details and documentation.


## Main Build and Test Status

![Build Status Linux 3.1](https://github.com/Finbuckle/Finbuckle.MultiTenant/actions/workflows/linux-3.1.yml/badge.svg)  
![Build Status MacOS 3.1](https://github.com/Finbuckle/Finbuckle.MultiTenant/actions/workflows/macos-3.1.yml/badge.svg?)  
![Build Status Windows 3.1](https://github.com/Finbuckle/Finbuckle.MultiTenant/actions/workflows/windows-3.1.yml/badge.svg)

![Build Status Linux 5.0](https://github.com/Finbuckle/Finbuckle.MultiTenant/actions/workflows/linux-5.0.yml/badge.svg)  
![Build Status MacOS 5.0](https://github.com/Finbuckle/Finbuckle.MultiTenant/actions/workflows/macos-5.0.yml/badge.svg)  
![Build Status Windows 5.0](https://github.com/Finbuckle/Finbuckle.MultiTenant/actions/workflows/windows-5.0.yml/badge.svg)

## License

This project uses the [Apache 2.0 license](https://www.apache.org/licenses/LICENSE-2.0). See [LICENSE](LICENSE) file for license information.

## .NET Foundation

This project is supported by the [.NET Foundation](https://dotnetfoundation.org).

## Code of Conduct

This project has adopted the code of conduct defined by the Contributor Covenant to clarify expected behavior in our community.
For more information see the [.NET Foundation Code of Conduct](https://dotnetfoundation.org/code-of-conduct) or the [CONTRIBUTING.md](CONTRIBUTING.md) file.

## Quick Start

Finbuckle.MultiTenant is designed to be easy to use and follows standard .NET conventions as much as possible. This introduction assumes a standard ASP.NET Core
use case, but any application using .NET dependency injection can work with the library.

### Installation

First, install the Finbuckle.MultiTenant.AspNetCore NuGet package:

.NET Core CLI
```bash
$ dotnet add package Finbuckle.MultiTenant.AspNetCore
```

### Basic Configuration

Next, in the app's startup `ConfigureServices` method call `AddMultiTenant<T>` and its various builder methods:

```cs
public void ConfigureServices(IServiceCollection services)
{
    ...
    services.AddMultiTenant<TenantInfo>()
            .WithHostStrategy()
            .WithConfigurationStore()
    ...
}
```

Finally, in the `Configure` method call `UseMultiTenant()` to register the middleware:

```cs
public void Configure(IApplicationBuilder app)
{
    ...
    app.UseMultiTenant(); // Before UseMvc!
    ...
    //app.UseMvc(); // for .NET Core 3.1
    app.UseEndpoints(...); // for .NET 5.0+
}
```

That's all that is needed to get going. Let's breakdown each line:

`services.AddMultiTenant<TenantInfo>()`

This line registers the base services and designates `TenantInfo` as the class that will hold tenant information at runtime.

The type parameter for `AddMultiTenant<T>` must be an implementation of `ITenantInfo` and holds basic information about the tenant such as its name and an identifier. `TenantInfo` is provided as a basic implementation, but a custom implementation can be used if more properties are needed.

See [Core Concepts](https://www.finbuckle.com/MultiTenant/Docs/CoreConcepts) for more information on `ITenantInfo`.

`.WithHostStrategy()`

The line tells the app that our "strategy" to determine the request tenant will be to look at the request host, which defaults to the extracting the subdomain as a tenant identifier.

See [Strategies](https://www.finbuckle.com/MultiTenant/Docs/Strategies) for more information.

`.WithConfigurationStore()`

This line tells the app that information for all tenants are in the `appsettings.json` file used for app configuration. If a tenant in the store has the identifier found by the strategy, the tenant will be successfully resolved for the current request.

See [Stores](https://www.finbuckle.com/MultiTenant/Docs/Stores) for more information.

Finbuckle.MultiTenant comes with a collection of strategies and store types that can be mixed and matched in various ways.

`app.UseEndPoints`

This line configures the middleware which resolves the tenant using the registered strategies, stores, and other settings. Be sure to call it before calling `UseEndpoints` and other middleware which will use per-tenant functionality, e.g. `UseAuthentication`.

### Basic Usage

With the services and middleware configured, access information for the current tenant from the `TenantInfo` property on the `MultiTenantContext` object accessed from the `GetMultiTenantContext<T>` extension method:

```cs
var tenantInfo = HttpContext.GetMultiTenantContext<TenantInfo>().TenantInfo;

if(tenantInfo != null)
{
    var tenantId = tenantInfo.Id;
    var identifier = tenantInfo.Identifier;
    var name = tenantInfo.Name;
}
```

The type of the `TenantInfo` property depends on the type passed when calling `AddMultiTenant<T>` during configuration. If the current tenant could not be determined then `TenantInfo` will be null.

The `ITenantInfo` instance and/or the typed instance are also available directly through dependency injection.

See [Configuration and Usage](https://www.finbuckle.com/MultiTenant/Docs/ConfigurationAndUsage) for more information.

## Advanced Usage

The library builds on this basic functionality to provide a variety of higher level features. See the documentation for more details:

* [Per-tenant Options](https://www.finbuckle.com/MultiTenant/Docs/Options)
* [Per-tenant Authentication](https://www.finbuckle.com/MultiTenant/Docs/Authentication)
* [Entity Framework Core Data Isolation](https://www.finbuckle.com/MultiTenant/Docs/EFCore)
* [ASP.NET Core Identity Data Isolation](https://www.finbuckle.com/MultiTenant/Docs/Identity)

## Samples

A variety of sample projects are available in the `samples` directory. Be sure to read the information on the index page of each sample and the code comments in the `Startup` class.

## Compiling from Source

From the command line clone the git repository, `cd` into the new directory, and compile with `dotnet build`.

```bash
$ git clone https://github.com/Finbuckle/Finbuckle.MultiTenant.git
$ cd Finbuckle.MultiTenant
Cloning into 'Finbuckle.MultiTenant'...
<output omitted>
$ cd Finbuckle.MultiTenant
$ dotnet build
```

## Running Unit Tests

Run the unit tests from the command line with `dotnet test` from the solution directory.

```bash
$ dotnet test
```