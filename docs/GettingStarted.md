# Getting Started

## Installation

Install the Finbuckle.MultiTenant NuGet package with your method of choice.

.NET CLI
```bash
$ dotnet add package Finbuckle.MultiTenant
```

Package Manager
```bash
> Install-Package Finbuckle.MultiTenant
```

## Usage

Configure the services by calling `AddMultiTenant` and its builder methods in your app's `ConfigureServices` method. Here we are usign the host strategy and in-memory store, but Finbuckle.MultiTenant comes with several other multitenant [strategies]() and [stores]().

```cs
public void ConfigureServices(IServiceCollection services)
{
    ...
    services.AddMultiTenant().WithHostStrategy().WithInMemoryStore();
    ...
}
```

Configure the middleware by calling `UseMultiTenant` in your app's `Configure` method. Be sure to call it before calling `UseMvc` if using ASP.NET Core MVC.

```cs
public void Configure(IApplicationBuilder app)
{
    ...
    app.UseMultiTenant(); // Before UseMvc!
    ...
    app.UseMvc();
}
```

With the services and middleware configured, access information for the current tenant from the `TenantInfo` object. Here we are accessing it from the `GetMultiTenantContext` extension method, but there are [several ways] to obtain it. If the current tenant could not be determined then `TenantInfo` will be null.

```cs
using Finbuckle.MultiTenant;
...

var tenantInfo = HttpContext.GetMultiTenantContext().TenantInfo;

if(tenantInfo != null)
{
    var tenantId = tenantInfo.Id;
    var identifier = tenantinfo.Identifier;
    var name = tenantInfo.Name;
    var something = tenantInfo.Items["something"];
}
```

The `TenantInfo` object holds [basic details]() about a tenant, and its `Items` property provides extensibility. This enables you to customize your app on a on a per-tenant basis in any way you want.

Finbuckle.MultiTenant provides built-in functionality for [per-tenant options](), [per-tenant authentication](), and [per-tenant data isolation]().

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

## Running Tests

Run the xUnit tests from the command line with `dotnet test` from the solution directory.

```bash
$ dotnet test
```

For cleaner output filter the command to run on only the test projects.
```bash
$ find . -name '*Test.csproj' -print0 | xargs -0 -I{} dotnet test {}
```
