# Getting Started
Finbuckle.MultiTenant is designed to be easy to use and follows standard .NET
Core conventions as much as possible. This guide assumes a standard ASP.NET Core
use case

## Installation

Install the Finbuckle.MultiTenant.AspNetCore NuGet package.

.NET Core CLI
```bash
$ dotnet add package Finbuckle.MultiTenant
```

Package Manager
```bash
> Install-Package Finbuckle.MultiTenant.AspNetCore
```

## Basics

Configure the services by calling `AddMultiTenant<T>` followed by its builder methods in the app's `ConfigureServices` method. Here we are using the basic `TenantInfo` implementation, the host strategy and the configuration store.
Finbuckle.MultiTenant comes with several other multitenant [strategies](Strategies) and [stores](Stores).

The `TenantInfo` class holds basic details about a tenant and is used throughout the library. See [Core Concepts](CoreConcepts) for more information.

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

Configure the middleware by calling `UseMultiTenant` in the app's `Configure` method. Be sure to call it before calling `UseMvc` and other middleware which will use per-tenant functionality.

```cs
public void Configure(IApplicationBuilder app)
{
    ...
    app.UseMultiTenant(); // Before UseMvc!
    ...
    app.UseMvc();
}
```

With the services and middleware configured, access information for the current tenant from the `TenantInfo` property on the `MultiTenantContext` object accessed from the `GetMultiTenantContext` extension method. If the current tenant could not be determined then `TenantInfo` will be null. The type of the `TenantInfo` property depends on the type passed when calling
`AddMultiTenant<T>` during configuration.

```cs
var tenantInfo = HttpContext.GetMultiTenantContext().TenantInfo;

if(tenantInfo != null)
{
    var tenantId = tenantInfo.Id;
    var identifier = tenantInfo.Identifier;
    var name = tenantInfo.Name;
}
```

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

Run the unit tests from the command line with `dotnet test` from the solution directory.

```bash
$ dotnet test
```
