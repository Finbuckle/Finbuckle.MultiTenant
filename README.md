# ![Finbuckle Logo](https://www.finbuckle.com/images/finbuckle-32x32-gh.png) Finbuckle.MultiTenant <span class="_version">9.0.0</span>

## About Finbuckle.MultiTenant

Finbuckle.MultiTenant is an open-source multitenancy middleware library for .NET. It enables tenant resolution,
per-tenant app behavior, and per-tenant data isolation.
See [https://www.finbuckle.com/multitenant](https://www.finbuckle.com/multitenant) for more details and documentation.

**This release supports .NET 9 and .NET 8.**

## Open Source Support

Table of Contents

1. [What's New in v<span class="_version">9.0.0</span>](#whats-new)
2. [Open Source Support](#open-source-support)
3. [Quick Start](#quick-start)
4. [Documentation](#documentation)
5. [Sample Projects](#sample-projects)
6. [Build and Test Status](#build-and-test-status)
7. [License](#license)
8. [.NET Foundation](#net-foundation)
9. [Code of Conduct](#code-of-conduct)
10. [Community](#community)
11. [Building from Source](#building-from-source)
12. [Running Unit Tests](#running-unit-tests)

## <a name="whats-new"></a> What's New in v<span class="_version">9.0.0</span>

> This section only lists release update details specific to v<span class="_version">9.0.0</span>. See
> the [changelog file](CHANGELOG.md) for all release update details.
<!--_release-notes-->

# [9.0.0](https://github.com/Finbuckle/Finbuckle.MultiTenant/compare/v8.0.0...v9.0.0) (2024-11-13)

### ⚠ BREAKING CHANGES

* `OnTenantResolved` and `OnTenantNotResolved` are no longer used. Use the `OnStrategyResolveCompleted`, `OnStoreResolveCompleted`, and `OnTenantResolveCompleted` events instead.
* `MultiTenantDbContext` constructors accepting ITenantInfo removed, use `MultiTenantDbContext.Create` factory method instead.
* net6.0 and net7.0 are no longer supported targets.
* dotnet runtime specific dependencies now float to the latest patch version and are locked at release time with a NuGet lock file. This is a security mitigation and may break some builds not on the latest SDKs.

### Features

* add multitenant db factory method ([#894](https://github.com/Finbuckle/Finbuckle.MultiTenant/issues/894)) (
  [ea216ff](https://github.com/Finbuckle/Finbuckle.MultiTenant/commit/ea216ffb37d241804899ab0f3cd5db1e9c6ae641))
* better tenant resolution events ([#897](https://github.com/Finbuckle/Finbuckle.MultiTenant/issues/897)) ([956ca36](https://github.com/Finbuckle/Finbuckle.MultiTenant/commit/956ca36670aa8aed38afcbbbdd78f1b79d91287c))
* dotnet 9 support ([#893](https://github.com/Finbuckle/Finbuckle.MultiTenant/issues/893)) ([4be1b88](https://github.com/Finbuckle/Finbuckle.MultiTenant/commit/4be1b88fb7103223517d2cd8a16ea62c6d6204d5))
* make builds deterministic and set latest GH actions ([#889](https://github.com/Finbuckle/Finbuckle.MultiTenant/issues/889)) ([d82f89d](https://github.com/Finbuckle/Finbuckle.MultiTenant/commit/d82f89da2f7a82bb302aaedfdb5c676cc7051273))
<!--_release-notes-->

## Open Source Support

Your support helps keep the project going and is greatly appreciated!

Finbuckle.MultiTenant is primarily supported by its [GitHub sponsors](https://github.com/sponsors/Finbuckle) and [contributors](https://github.com/Finbuckle/Finbuckle.MultiTenant/graphs/contributors).  

Additional support is provided by the following organizations:

<p><a href="https://www.digitalocean.com/">
  <img src="https://opensource.nyc3.cdn.digitaloceanspaces.com/attribution/assets/SVG/DO_Logo_horizontal_blue.svg" alt="Digital Ocean logo" height="40">
</a></p>

<p><a href="https://www.github.com/">
  <img src="https://github.githubassets.com/assets/GitHub-Logo-ee398b662d42.png" alt="GitHub logo" height="40">
</a></p>

<p><a href="https://www.jetbrains.com/">
  <img src="https://resources.jetbrains.com/storage/products/company/brand/logos/jetbrains.svg" alt="Jetbrains logo" height="40">
</a></p>

## Quick Start

Finbuckle.MultiTenant is designed to be easy to use and follows standard .NET conventions as much as possible. This
introduction assumes a standard ASP.NET Core use case, but any application using .NET dependency injection can work with
the library.

### Installation

First, install the Finbuckle.MultiTenant.AspNetCore NuGet package:

.NET Core CLI

```bash
$ dotnet add package Finbuckle.MultiTenant.AspNetCore
```

### Basic Configuration

Next, in the app's service configuration call `AddMultiTenant<TTenantInfo>` and its various builder methods and in the
middleware configuration call `UseMultiTenant()`:

```cs
builder.Services.AddMultiTenant<TenantInfo>()
    .WithHostStrategy()
    .WithConfigurationStore();

// other app code...

app.UseMultiTenant();

// other app code...

app.Run();
```

That's all that is needed to get going. Let's breakdown each line:

`builder.Services.AddMultiTenant<TenantInfo>()`

This line registers the base services and designates `TenantInfo` as the class that will hold tenant information at
runtime.

The type parameter for `AddMultiTenant<TTenantInfo>` must be an implementation of `ITenantInfo` which holds basic
information about the tenant such as its name and an identifier. `TenantInfo` is provided as a basic implementation, but
a custom implementation can be used if more properties are needed.

See [Core Concepts](https://www.finbuckle.com/MultiTenant/Docs/CoreConcepts) for more information on `ITenantInfo`.

`.WithHostStrategy()`

The line tells the app that our "strategy" to determine the request tenant will be to look at the request host, which
defaults to the extracting the subdomain as a tenant identifier.

See [MultiTenant Strategies](https://www.finbuckle.com/MultiTenant/Docs/Strategies) for more information.

`.WithConfigurationStore()`

This line tells the app that information for all tenants are in the `appsettings.json` file used for app configuration.
If a tenant in the store has the identifier found by the strategy, the tenant will be successfully resolved for the
current request.

See [MultiTenant Stores](https://www.finbuckle.com/MultiTenant/Docs/Stores) for more information.

Finbuckle.MultiTenant comes with a collection of strategies and store types that can be mixed and matched in various
ways.

`app.UseMultiTenant()`

This line configures the middleware which resolves the tenant using the registered strategies, stores, and other
settings. Be sure to call it before other middleware which will use per-tenant functionality, e.g.
`UseAuthentication()`.

### Basic Usage

With the services and middleware configured, access information for the current tenant from the `TenantInfo` property on
the `MultiTenantContext<T>` object accessed from the `GetMultiTenantContext<T>` extension method:

```cs
var tenantInfo = HttpContext.GetMultiTenantContext<TenantInfo>().TenantInfo;

if(tenantInfo != null)
{
    var tenantId = tenantInfo.Id;
    var identifier = tenantInfo.Identifier;
    var name = tenantInfo.Name;
}
```

The type of the `TenantInfo` property depends on the type passed when calling `AddMultiTenant<T>` during configuration.
If the current tenant could not be determined then `TenantInfo` will be null.

The `ITenantInfo` instance and/or the typed instance are also available directly through dependency injection.

See [Configuration and Usage](https://www.finbuckle.com/MultiTenant/Docs/ConfigurationAndUsage) for more information.

## Documentation

The library builds on this basic functionality to provide a variety of higher level features. See
the [documentation](https://www.finbuckle.com/multitenant/docs) for more details:

* [Per-tenant Options](https://www.finbuckle.com/MultiTenant/Docs/Options)
* [Per-tenant Authentication](https://www.finbuckle.com/MultiTenant/Docs/Authentication)
* [Entity Framework Core Data Isolation](https://www.finbuckle.com/MultiTenant/Docs/EFCore)
* [ASP.NET Core Identity Data Isolation](https://www.finbuckle.com/MultiTenant/Docs/Identity)

## Sample Projects

A variety of [sample projects](https://github.com/Finbuckle/Finbuckle.MultiTenant/tree/main/samples) are available in
the repository. Check older tagged release commits for samples from prior .NET versions.

## Build and Test Status

![Build and Test Status](https://github.com/Finbuckle/Finbuckle.MultiTenant/actions/workflows/ci.yml/badge.svg)

## License

This project uses the [Apache 2.0 license](https://www.apache.org/licenses/LICENSE-2.0). See [LICENSE](LICENSE) file for
license information.

## .NET Foundation

This project is supported by the [.NET Foundation](https://dotnetfoundation.org).

## Code of Conduct

This project has adopted the code of conduct defined by the Contributor Covenant to clarify expected behavior in our
community. For more information see the [.NET Foundation Code of Conduct](https://dotnetfoundation.org/code-of-conduct)
or the [CONTRIBUTING.md](CONTRIBUTING.md) file.

## Community

Check out the [GitHub repository](https://github.com/Finbuckle/Finbuckle.MultiTenant) to ask a question, make a request,
or peruse the code!

## Building from Source

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