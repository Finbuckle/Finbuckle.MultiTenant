# ![Finbuckle Logo](https://www.finbuckle.com/images/finbuckle-32x32-gh.png) MultiTenant <span class="_version">10.0.2</span>

MultiTenant is an open source multi-tenancy library for modern .NET created and maintained by [Finbuckle LLC](https://www.finbuckle.com).
It enables tenant resolution, per-tenant app behavior, and per-tenant data isolation.

See [https://www.finbuckle.com/MultiTenant](https://www.finbuckle.com/MultiTenant) for more details and documentation.

**This release supports .NET 10.**

Beginning with MultiTenant v10, major version releases align with .NET major version releases.

New development focuses on the latest MultiTenant release version while critical security and severe bug
fixes will be released for prior versions which target .NET versions supported by Microsoft.

In general, you should target the version of MultiTenant that matches your .NET version.

## Open Source Support

Table of Contents

1. [What's New in v<span class="_version">10.0.2</span>](#whats-new)
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

## <a name="whats-new"></a> What's New in v<span class="_version">10.0.2</span>

> This section only lists release update details specific to v<span class="_version">10.0.2</span>. See
> the [changelog file](CHANGELOG.md) for all release update details.
<!--_release-notes-->

### Bug Fixes

* correct store skip take order bug ([#1076](https://github.com/Finbuckle/Finbuckle.MultiTenant/issues/1076)) ([42a6139](https://github.com/Finbuckle/Finbuckle.MultiTenant/commit/42a6139d05f45ed21700b1c71f70b3a0362c3708))
* re-add ITenantInfo interface ([#1075](https://github.com/Finbuckle/Finbuckle.MultiTenant/issues/1075)) ([4b4db14](https://github.com/Finbuckle/Finbuckle.MultiTenant/commit/4b4db1487f012961671dd7969c94ef143dfc7c17))
* remove the shadow `TenantId` property when calling `IsNotMultiTenant()` ([#1079](https://github.com/Finbuckle/Finbuckle.MultiTenant/issues/1079)) ([d258b62](https://github.com/Finbuckle/Finbuckle.MultiTenant/commit/d258b622600e96da160638a7fee7b53d615a7e0e))
* update dependencies for .NET 10.0.2 ([#1084](https://github.com/Finbuckle/Finbuckle.MultiTenant/issues/1084)) ([def5e59](https://github.com/Finbuckle/Finbuckle.MultiTenant/commit/def5e59d5a577d51b000a7df55c58fef018c1205))
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

MultiTenant is designed to be easy to use and follows standard .NET conventions as much as possible. See the 
[Getting Started](https://www.finbuckle.com/MultiTenant/Docs/GettingStarted) documentation for more details.

## Documentation

The library builds on on basic multi-tenant functionality to provide a variety of higher level features. See
the [documentation](https://www.finbuckle.com/multitenant/docs) for more details:

* [Per-tenant Options](https://www.finbuckle.com/MultiTenant/Docs/Options)
* [Per-tenant Authentication](https://www.finbuckle.com/MultiTenant/Docs/Authentication)
* [Entity Framework Core Data Isolation](https://www.finbuckle.com/MultiTenant/Docs/EFCore)
* [ASP.NET Core Identity Data Isolation](https://www.finbuckle.com/MultiTenant/Docs/Identity)

## Sample Projects

A variety of [sample projects](https://github.com/Finbuckle/Finbuckle.MultiTenant/tree/main/samples) are available in
the repository.

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
git clone https://github.com/Finbuckle/Finbuckle.MultiTenant.git
cd Finbuckle.MultiTenant
cd Finbuckle.MultiTenant
dotnet build
```

## Running Unit Tests

Run the unit tests from the command line with `dotnet test` from the solution directory.

```bash
dotnet test
```
