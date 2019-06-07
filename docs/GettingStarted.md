# Getting Started

## Installation

Install the Finbuckle.MultiTenant NuGet package with your method of choice.

.NET Core CLI
```bash
$ dotnet add package Finbuckle.MultiTenant
```

Package Manager
```bash
> Install-Package Finbuckle.MultiTenant
```

## Usage

Configure the services by calling `AddMultiTenant` followed by its builder methods in your app's `ConfigureServices` method. Here we are using the host strategy and in-memory store, but Finbuckle.MultiTenant comes with several other multitenant [strategies](Strategies) and [stores](Stores).

```cs
public void ConfigureServices(IServiceCollection services)
{
    ...
    services.AddMultiTenant().WithHostStrategy().WithInMemoryStore();
    ...
}
```

Configure the middleware by calling `UseMultiTenant` in your app's `Configure` method. Be sure to call it before calling `UseMvc` and other middleware which will use per-tenant funtionality.

```cs
public void Configure(IApplicationBuilder app)
{
    ...
    app.UseMultiTenant(); // Before UseMvc!
    ...
    app.UseMvc();
}
```

With the services and middleware configured, access information for the current tenant from the `TenantInfo` property on the `MultiTenantContext` object accessed from the `GetMultiTenantContext` extension method. If the current tenant could not be determined then `TenantInfo` will be null.

```cs
using Finbuckle.MultiTenant;
...

var tenantInfo = HttpContext.GetMultiTenantContext().TenantInfo;

if(tenantInfo != null)
{
    var tenantId = tenantInfo.Id;
    var identifier = tenantInfo.Identifier;
    var name = tenantInfo.Name;
    var something = tenantInfo.Items["something"];
}
```

The `TenantInfo` property holds basic details about a tenant and enables customization of your app on a on a per-tenant basis in any way you want.

Finbuckle.MultiTenant uses `TenantInfo` internally to provide built-in functionality such as [per-tenant options](Options), [per-tenant authentication](Authentication), and [per-tenant data isolation](EFCore).

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
