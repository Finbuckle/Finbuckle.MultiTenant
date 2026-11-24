# ![Finbuckle Logo](https://www.finbuckle.com/images/finbuckle-32x32-gh.png) MultiTenant <span class="_version">11.0.0</span>

MultiTenant is an open source multi-tenancy library for modern .NET created and maintained by [Finbuckle LLC](https://www.finbuckle.com).
It enables tenant resolution, per-tenant app behavior, and per-tenant data isolation.

See [https://www.finbuckle.com/Products](https://www.finbuckle.com/Products) for more details and documentation.

**This release supports .NET 11.**

Beginning with MultiTenant v10, major version releases align with .NET major version releases.

New development focuses on the latest MultiTenant release version while critical security and severe bug
fixes will be released for prior versions which target .NET versions supported by Microsoft.

In general, you should target the version of MultiTenant that matches your .NET version.

## Open Source Support

Table of Contents

1. [What's New in v<span class="_version">11.0.0</span>](#whats-new)
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

## <a name="whats-new"></a> What's New in v<span class="_version">11.0.0</span>

> This section only lists release update details specific to v<span class="_version">11.0.0</span>. See
> the [changelog file](CHANGELOG.md) for all release update details.
<!--_release-notes-->
### ⚠ BREAKING CHANGES

#### abstractions

* Replace IMultiTenantContext with ITenantContext and remove IMultiTenantContextAccessor, IMultiTenantContextSetter, and the accessor implementations. Consumers must use the ambient tenant context scope APIs. ([#1212](https://github.com/Finbuckle/Finbuckle.MultiTenant/issues/1212) by [@AndrewTriesToCode](https://github.com/AndrewTriesToCode))
* ITenantResolver.ResolveAsync returns tenant information directly and exposes the primary store and cache metadata through the updated resolver contract. ([#1212](https://github.com/Finbuckle/Finbuckle.MultiTenant/issues/1212) by [@AndrewTriesToCode](https://github.com/AndrewTriesToCode))

#### aspnetcore

* UseMultiTenant<TId>() throws a MultiTenantException at startup unless AddHttpAmbientTenantScope() was called, either directly or by an ASP.NET Core builder helper. Apps that configure only general-purpose methods such as WithStaticStrategy, WithDelegateStrategy or WithStrategy must call AddHttpAmbientTenantScope() themselves. The middleware also throws for a request that has no active ambient tenant scope. ([#1238](https://github.com/Finbuckle/Finbuckle.MultiTenant/issues/1238) by [@AndrewTriesToCode](https://github.com/AndrewTriesToCode))
* A tenant set earlier in the pipeline, for example with HttpContext.SetTenantInfo in middleware before UseMultiTenant, is now kept and tenant resolution is skipped. Previously the middleware began a new scope after resolution and discarded it. ([#1238](https://github.com/Finbuckle/Finbuckle.MultiTenant/issues/1238) by [@AndrewTriesToCode](https://github.com/AndrewTriesToCode))
* ASP.NET Core integrations no longer expose or consume the removed IMultiTenantContext accessor and setter abstractions; inject ITenantContext instead. ([#1212](https://github.com/Finbuckle/Finbuckle.MultiTenant/issues/1212) by [@AndrewTriesToCode](https://github.com/AndrewTriesToCode))

#### efcore

* Querying a multi-tenant entity type through a context without a tenant set now returns no rows. It previously threw an InvalidOperationException ("An exception was thrown while attempting to evaluate a LINQ query parameter expression") wrapping a NullReferenceException. ([#1239](https://github.com/Finbuckle/Finbuckle.MultiTenant/issues/1239) by [@AndrewTriesToCode](https://github.com/AndrewTriesToCode))
* `IsMultiTenant` on entity types was a method and is now a property. Its receiver is now non-nullable, so callers that pass a nullable entity type (for example the result of `FindEntityType`) get nullable warnings and must check for null first. ([#1239](https://github.com/Finbuckle/Finbuckle.MultiTenant/issues/1239) by [@AndrewTriesToCode](https://github.com/AndrewTriesToCode))
* Multi-tenant DbContext constructors no longer accept IMultiTenantContextAccessor; resolve ITenantContext through the ambient scope. ([#1212](https://github.com/Finbuckle/Finbuckle.MultiTenant/issues/1212) by [@AndrewTriesToCode](https://github.com/AndrewTriesToCode))

#### efcoreidentity

* Multi-tenant identity DbContext constructors no longer accept IMultiTenantContextAccessor; use ITenantContext from the ambient scope. ([#1212](https://github.com/Finbuckle/Finbuckle.MultiTenant/issues/1212) by [@AndrewTriesToCode](https://github.com/AndrewTriesToCode))

#### finbuckle

* ITenantScopeProvider has two new members, EndScope() and HasScope. Custom implementations of ITenantScopeProvider must implement them. ([#1238](https://github.com/Finbuckle/Finbuckle.MultiTenant/issues/1238) by [@AndrewTriesToCode](https://github.com/AndrewTriesToCode))
* `ConfigurationStore` creates tenants with `ConfigurationBinder.Get<TTenantInfo>()` from the tenant section layered over `Defaults`, instead of binding both sections onto an uninitialized object. A type with no parameterless constructor must have a single public constructor whose parameters match configuration keys (or use the new factory overload). Collection properties present in both `Defaults` and a tenant section are overlaid by index rather than appended (for example `["a","b"]` in `Defaults` and `["c"]` in a tenant gives `["c","b"]`); dictionary properties are merged by key. Because constructors and property initializers now run, plain properties keep their initializer values when not configured, and collection initializers are added to rather than replaced. ([#1232](https://github.com/Finbuckle/Finbuckle.MultiTenant/issues/1232) by [@AndrewTriesToCode](https://github.com/AndrewTriesToCode))
* Removes the public TypeExtensions.ImplementsOrInheritsUnboundGeneric method. ([#1212](https://github.com/Finbuckle/Finbuckle.MultiTenant/issues/1212) by [@AndrewTriesToCode](https://github.com/AndrewTriesToCode))
* Remove accessor and setter registrations in favor of ITenantContext and ITenantScopeProvider. ([#1212](https://github.com/Finbuckle/Finbuckle.MultiTenant/issues/1212) by [@AndrewTriesToCode](https://github.com/AndrewTriesToCode))
* IMultiTenantStore methods accept cancellation tokens; RemoveAsync removes by tenant ID and RemoveByIdentifierAsync removes by identifier. ([#1212](https://github.com/Finbuckle/Finbuckle.MultiTenant/issues/1212) by [@AndrewTriesToCode](https://github.com/AndrewTriesToCode))
* Replace ITenantResolver.Stores with one primary Store and StoreCaches, and remove DistributedCacheStore and MultiTenantStoreWrapper. ([#1212](https://github.com/Finbuckle/Finbuckle.MultiTenant/issues/1212) by [@AndrewTriesToCode](https://github.com/AndrewTriesToCode))
* Remove InMemoryStoreOptions and its configuring overload. Tenant identifiers are case-insensitive and initial tenants are added through TenantManager. ([#1212](https://github.com/Finbuckle/Finbuckle.MultiTenant/issues/1212) by [@AndrewTriesToCode](https://github.com/AndrewTriesToCode))

#### general

* Only the net11.0 framework is targeted and runtime async is enabled. ([#1237](https://github.com/Finbuckle/Finbuckle.MultiTenant/issues/1237) by [@AndrewTriesToCode](https://github.com/AndrewTriesToCode))
* `ITenantInfo` is now `ITenantInfo<TId>` where `TId` is any `Equatable<TId>`, e.g. `int` or `GUID`. Other types and methods taking a `TTenantType` generic type parameter now also take a `TId` generic type parameter. ([#1188](https://github.com/Finbuckle/Finbuckle.MultiTenant/issues/1188) by [@mvanput](https://github.com/mvanput))
* `default(TId)` is treated as tenant not-set similar to how previously `null` was considered not-set. ([#1188](https://github.com/Finbuckle/Finbuckle.MultiTenant/issues/1188) by [@mvanput](https://github.com/mvanput))


### Features

#### aspnetcore

* require a singleton ITenantScopeProvider at startup ([#1238](https://github.com/Finbuckle/Finbuckle.MultiTenant/issues/1238) by [@AndrewTriesToCode](https://github.com/AndrewTriesToCode)) ([512544b](https://github.com/Finbuckle/Finbuckle.MultiTenant/commit/512544b38a471a1bc6d619d450573042d50337db))

#### efcore

* validate the tenant id type when the model is built ([#1239](https://github.com/Finbuckle/Finbuckle.MultiTenant/issues/1239) by [@AndrewTriesToCode](https://github.com/AndrewTriesToCode)) ([713154a](https://github.com/Finbuckle/Finbuckle.MultiTenant/commit/713154af53ef55041c648bef672adeca95519aa4))

#### finbuckle

* require OSMF EULA acceptance for official packages ([#1241](https://github.com/Finbuckle/Finbuckle.MultiTenant/issues/1241) by [@AndrewTriesToCode](https://github.com/AndrewTriesToCode)) ([b90a1ff](https://github.com/Finbuckle/Finbuckle.MultiTenant/commit/b90a1ff61c9990e5d0ef07a78f7d6dae32a73bc5))
* attest NuGet packages and assemblies before publishing ([#1242](https://github.com/Finbuckle/Finbuckle.MultiTenant/issues/1242) by [@AndrewTriesToCode](https://github.com/AndrewTriesToCode)) ([e12e081](https://github.com/Finbuckle/Finbuckle.MultiTenant/commit/e12e0810cbbf3f0141754845bf8faf6ee9b31b45))
* add RunInTenantScopeAsync to run a delegate as a tenant ([#1238](https://github.com/Finbuckle/Finbuckle.MultiTenant/issues/1238) by [@AndrewTriesToCode](https://github.com/AndrewTriesToCode)) ([8c66dd9](https://github.com/Finbuckle/Finbuckle.MultiTenant/commit/8c66dd9a1c8664c9f94132fc68942291598e441b))


### Bug Fixes

#### aspnetcore

* begin the ambient tenant scope from a startup filter ([#1240](https://github.com/Finbuckle/Finbuckle.MultiTenant/issues/1240) by [@AndrewTriesToCode](https://github.com/AndrewTriesToCode)) ([638b09e](https://github.com/Finbuckle/Finbuckle.MultiTenant/commit/638b09e25ed0629b395b74a74603cf216a71889a))

#### finbuckle

* keep decorated services owned by the container ([#1238](https://github.com/Finbuckle/Finbuckle.MultiTenant/issues/1238) by [@AndrewTriesToCode](https://github.com/AndrewTriesToCode)) ([4d4e8b5](https://github.com/Finbuckle/Finbuckle.MultiTenant/commit/4d4e8b5074763a5f4eaed3a4596a8a483c151ce5))
* key in-memory store lookups by snapshot ids ([#1232](https://github.com/Finbuckle/Finbuckle.MultiTenant/issues/1232) by [@AndrewTriesToCode](https://github.com/AndrewTriesToCode)) ([4555318](https://github.com/Finbuckle/Finbuckle.MultiTenant/commit/4555318b6770ef407558c47f86e66d1b9f7cdf81))
* propagate cancellation through remote store requests ([#1212](https://github.com/Finbuckle/Finbuckle.MultiTenant/issues/1212) by [@AndrewTriesToCode](https://github.com/AndrewTriesToCode)) ([dfd1e67](https://github.com/Finbuckle/Finbuckle.MultiTenant/commit/dfd1e67190b3758719b3b98f0f21f47d2f75212a))



<!--_release-notes-->

## Open Source Maintenance Fee

**Effective November 10th, 2026**

This project participates in the [Open Source Maintenance Fee](https://opensourcemaintenancefee.org).

The source code is freely available under the terms of the LICENSE. To
support sustainable maintenance, use of the project’s official releases
in revenue-generating activities requires adherence to the
[Open Source Maintenance Fee](./OSMFEULA.txt).

In short: if you use this project as part of generating revenue, the
Maintenance Fee applies. The fee directly supports the people maintaining
the project and helps ensure its long-term health.

To pay the Maintenance Fee, [become a Sponsor](https://github.com/sponsors/finbuckle).
See [pricing](https://www.finbuckle.com/Pricing) for details.

Release builds that reference the official packages show warning `FBMT0001` until the EULA is accepted by setting
`<AcceptFinbuckleEULA>true</AcceptFinbuckleEULA>` in your project file or `Directory.Build.props`.

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
[Getting Started](https://www.finbuckle.com/Docs/GettingStarted) documentation for more details.

## Documentation

The library builds on basic multi-tenant functionality to provide a variety of higher level features. See
the [documentation](https://www.finbuckle.com/Docs) for more details:

* [Per-tenant Options](https://www.finbuckle.com/Docs/Options)
* [Per-tenant Authentication](https://www.finbuckle.com/Docs/Authentication)
* [Entity Framework Core Data Isolation](https://www.finbuckle.com/Docs/EFCore)
* [ASP.NET Core Identity Data Isolation](https://www.finbuckle.com/Docs/Identity)

## Sample Projects

A variety of [sample projects](https://github.com/Finbuckle/Finbuckle.MultiTenant/tree/main/samples) are available in
the repository.

## Build and Test Status

![Build and Test Status](https://github.com/Finbuckle/Finbuckle.MultiTenant/actions/workflows/ci.yml/badge.svg)

## License

MultiTenant is licensed differently depending on how you get it:

- **Source code** is licensed under the [Apache 2.0 license](https://www.apache.org/licenses/LICENSE-2.0). See the
  [LICENSE](LICENSE) file. You can build and use the library from source under those terms.
- **Official NuGet packages** are licensed under the [Open Source Maintenance Fee EULA](OSMFEULA.txt). This is the
  license NuGet shows for the packages, and Visual Studio asks you to accept it when you install them.

See [Open Source Maintenance Fee](#open-source-maintenance-fee) for what the EULA requires.

The official packages and their assemblies have GitHub
[artifact attestations](https://docs.github.com/en/actions/concepts/security/artifact-attestations) that prove they
were built from this repository. See [Verifying Packages](https://www.finbuckle.com/Docs/Introduction#verifying-packages)
for how to check them.

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
