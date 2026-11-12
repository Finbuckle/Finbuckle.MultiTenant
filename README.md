# ![Finbuckle Logo](https://www.finbuckle.com/images/finbuckle-32x32-gh.png) MultiTenant <span class="_version">10.2.0</span>

MultiTenant is an open source multi-tenancy library for modern .NET created and maintained by [Finbuckle LLC](https://www.finbuckle.com).
It enables tenant resolution, per-tenant app behavior, and per-tenant data isolation.

See [https://www.finbuckle.com/MultiTenant](https://www.finbuckle.com/MultiTenant) for more details and documentation.

**This release supports .NET 10.**

Beginning with MultiTenant v10, major version releases align with .NET major version releases.

New development focuses on the latest MultiTenant release version while critical security and severe bug
fixes will be released for prior versions which target .NET versions supported by Microsoft.

In general, you should target the version of MultiTenant that matches your .NET version.

Table of Contents

1. [What's New in v<span class="_version">10.2.0</span>](#whats-new)
2. [Sponsorship](#sponsorship)
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

## <a name="whats-new"></a> What's New in v<span class="_version">10.2.0</span>

> This section only lists release update details specific to v<span class="_version">10.2.0</span>. See
> the [changelog file](CHANGELOG.md) for all release update details.
<!--_release-notes-->
### Features

#### finbuckle

* support constructor-only tenant info in EchoStore and ConfigurationStore ([#1205](https://github.com/Finbuckle/Finbuckle.MultiTenant/issues/1205) by [@fdcastel](https://github.com/fdcastel)) ([1217d16](https://github.com/Finbuckle/Finbuckle.MultiTenant/commit/1217d168198a4261c54f2962eecc1b59c9cef9b8))


### Bug Fixes

#### deps

* update source dependencies ([#1225](https://github.com/Finbuckle/Finbuckle.MultiTenant/issues/1225) by [@github-actions[bot]](https://github.com/github-actions[bot])) ([c25b301](https://github.com/Finbuckle/Finbuckle.MultiTenant/commit/c25b301a84bf0351159be40b4c78fa05ba4ce042))

#### finbuckle

* dispose temporary tenant configurations ([#1205](https://github.com/Finbuckle/Finbuckle.MultiTenant/issues/1205) by [@fdcastel](https://github.com/fdcastel)) ([ce7851f](https://github.com/Finbuckle/Finbuckle.MultiTenant/commit/ce7851f29f8d515d45931aac4914649e4adef8f7))



<!--_release-notes-->

## Sponsorship

If MultiTenant is useful to you, please consider [becoming a Sponsor](https://github.com/sponsors/finbuckle).
Sponsorship directly supports the people maintaining the project and helps ensure its long-term health.

As of November 10, 2026, with the release of .NET 11, MultiTenant v11 and later require the
[Open Source Maintenance Fee](https://opensourcemaintenancefee.org) for use in revenue-generating activities.
MultiTenant v10 does not.

**Additional support is provided by the following organizations:**

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

The library builds on basic multi-tenant functionality to provide a variety of higher level features. See
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
dotnet build
```

## Running Unit Tests

Run the unit tests from the command line with `dotnet test` from the solution directory.

```bash
dotnet test
```
