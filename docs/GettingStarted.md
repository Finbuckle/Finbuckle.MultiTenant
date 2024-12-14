# Getting Started

Finbuckle.MultiTenant is designed to be easy to use and follows standard .NET conventions as much as possible. This
introduction assumes a typical ASP.NET Core use case, but any application using .NET dependency injection can work with
the library.

## Installation

First, install the Finbuckle.MultiTenant.AspNetCore NuGet package:

.NET Core CLI

```bash
$ dotnet add package Finbuckle.MultiTenant.AspNetCore
```

## Basic Configuration

Finbuckle.MultiTenant is simple to get started with. Below is a sample app configured to use the subdomain as the tenant
identifier and the app's [configuration](https://learn.microsoft.com/en-us/aspnet/core/fundamentals/configuration/) (
most likely from an `appsettings.json` file) as the source of tenant details.

```csharp
using Finbuckle.MultiTenant;

var builder = WebApplication.CreateBuilder(args);

// add app services...

// add Finbuckle.MultiTenant services
builder.Services.AddMultiTenant<TenantInfo>()
    .WithHostStrategy()
    .WithConfigurationStore();

var app = builder.Build();

// add the Finbuckle.MultiTenant middleware
app.UseMultiTenant();

// add other middleware...

app.Run();
```

That's all that is needed to get going. Let's break down each line:

`builder.Services.AddMultiTenant<TenantInfo>()`

This line registers the base services and designates `TenantInfo` as the class that will hold tenant information at
runtime.

The type parameter for `AddMultiTenant<TTenantInfo>` must be an implementation of `ITenantInfo` and holds basic
information about the tenant such as its name and an identifier. `TenantInfo` is provided as a basic implementation, but
a custom implementation can be used if more properties are needed.

See [Core Concepts](CoreConcepts) for more information on `ITenantInfo`.

`.WithHostStrategy()`

The line tells the app that our "strategy" to determine the request tenant will be to look at the request host, which
defaults to the extracting the subdomain as a tenant identifier.

See [Strategies](Strategies) for more information.

`.WithConfigurationStore()`

This line tells the app that information for all tenants are in the `appsettings.json` file used for app configuration.
If a tenant in the store has the identifier found by the strategy, the tenant will be successfully resolved for the
current request.

See [Stores](Stores) for more information.

Finbuckle.MultiTenant comes with a collection of strategies and store types that can be mixed and matched in various
ways.

`app.UseMultiTenant()`

This line configures the middleware which resolves the tenant using the registered strategies, stores, and other
settings. Be sure to call it before other middleware which will use per-tenant functionality, such as
`UseAuthentication()`.

## Basic Usage

With the services and middleware configured, access information for the current tenant from the `TenantInfo` property on
the `MultiTenantContext<TTenantInfo>` object accessed from the `GetMultiTenantContext<TTenantInfo>` extension method:

```csharp
var tenantInfo = HttpContext.GetMultiTenantContext<TenantInfo>().TenantInfo;

if(tenantInfo != null)
{
    var tenantId = tenantInfo.Id;
    var identifier = tenantInfo.Identifier;
    var name = tenantInfo.Name;
}
```

The type of the `TenantInfo` property depends on the type passed when calling `AddMultiTenant<TTenantInfo>` during
configuration. If the current tenant could not be determined then `TenantInfo` will be null.

The `ITenantInfo` instance and the typed instance are also available using the
`IMultiTenantContextAccessor<TTenantinfo>` interface which is available via dependency injection.

See [Configuration and Usage](ConfigurationAndUsage) for more information.

## Advanced Usage

The library builds on this basic functionality to provide a variety of higher level features. See the documentation for
more details:

* [Per-tenant Options](Options)
* [Per-tenant Authentication](Authentication)
* [Entity Framework Core Data Isolation](EFCore)
* [ASP.NET Core Identity Data Isolation](Identity)

## Samples

A variety of sample projects are available in the `samples` directory. Be sure to read the information on the index page
of each sample and the code comments in the `Startup` class. Note that may older samples have been removed as .NET
versions are end-of-lifed by Microsoft, such as .NET Core 3.0 and .NET 5. These samples are still available in earlier
tagged release commits if needed.

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
