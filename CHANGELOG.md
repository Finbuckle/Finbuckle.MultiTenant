**6.2.0**
* Added a new events system. See PR #359 Thanks to **@natelaff**!
* Some internal refactoring.
* Various documentation fixes.
* Added sourcelink to allow debugging into remote source code.
* Added a security policy.

**6.1.0**
* .NET 5.0 support.
* New `DistributedCacheStore` uses the ASP.NET Core distributed cache for tenant resolution.
* New `HeaderStrategy` uses HTTP headers for tenant resolution. Thanks to **@natelaff**!
* Support for inheritance in multitenant Entity Framework Core entity. Thanks to **@rchamorro**!
* Fixed a conflict between ClaimStrategy and per-tenant authentication.
* Updated docs, samples, and unit tests.

**6.0.0**
* Customizable `TenantInfo`. Implement `ITenantInfo` as needed or use the basic `TenantInfo` implementation. Should work with most strategies and stores. This was a major overhaul to the library. See docs for more information.
* Changed NuGet structure: use `Finbuckle.MultiTenant.AspNetCore` for web apps and if needed add `Finbuckle.MultiTenant.EntityFrameworkCore`.
* `WithPerTenantAuthentication` - Adds support for common per-tenant authentication scenarios. See docs for full details.
* Multiple strategies and stores can be registe red. They will run in the order registered and the first tenant returned by a strategy/store combination is used.
* New `ClaimStrategy` checks for a tenant claim to resolve the tenant.
* New `SessionStrategy` uses a session variable to resolve the tenant.
* Refactored `InMemoryStore`, removed deprecated configuration functionality.
* Improved Blazor support.
* Improved support for non ASP.NET Core use cases.
* Removed support for ASP.NET 3.0.
* Removed `FallbackStrategy`, `StaticStrategy` is a better alternative.
* Bug fixes, refactors, and tweaks.
* Improved unit tests.
* Updated and improved documentation.
* Updated sample. Removed some older ASP.NET Core 2.1 samples.

**5.0.4**
* Fixed a conflicting assembly and NuGet versions.
* Minor documentation fix.

**5.0.3**
* Fixed a bug where documented static methods were internal rather than public.
* Minor documentation fix.

**5.0.1**
* Updated for [.NET Core January 2020 Updates](https://devblogs.microsoft.com/dotnet/net-core-january-2020/) adding support for .NET Core 2.1.15, 3.0.2, and 3.1.1.
* Updated dependencies as recommended in security notices for [.NET Core January 2020 Updates](https://devblogs.microsoft.com/dotnet/net-core-january-2020/).
* *Finbuckle.MultiTenant.AspNetCore* targets `netcoreapp3.1`, `netcoreapp3.0`, and `netcoreapp2.1`.
* *Finbuckle.MultiTenant.Core* targets `netstandard2.1` and `netstandard2.0`.
* *Finbuckle.MultiTenant.EntityFrameworkCore* targets `netstandard2.1` and `netstandard2.0`.

**5.0.0**
* Added support for ASP.NET Core 3.1.
* Major refactor of how Entity Framework multitenant data isolation works. No longer need to derive from `MultiTenantDbContext` greatly improving flexibility. `IdentityMultiTenantDbContext` reworked under this new model and no longer requires or recommends use of multitenant support classes, e.g. `MultiTenantIdentityUser`. Attempted to minimize impact, but if using `IdentityMultiTenantDbContext` **this may be a breaking change!** Thanks **@GordonBlahut**!
* Simplified `EFCoreStore` to use `TenantInfo` directly. **This is a breaking change!**
* Fixed a bug with user id not being set correctly in legacy 'IdentityMultiTenantDbContext'.
* Added `ConfigurationStore` to load tenant information from app configuration. The store is read-only in code, but changes in configuration (e.g. appsettings.json) are picked up at runtime. Updated most sample projects to use this store.
* Deprecated `InMemoryStore` functionality that reads from configuration.
* Added `HttpRemoteStore` which will make an http request to get a `TenantInfo` object. It can be extended with `DelegatingHandler`s (i.e. to add authentication headers). Added sample projects for this store. Thanks to **@colindekker**!
* Fixed an exception with OpenIdConnect remote authentication if "state" is not returned from the identity provider. The new behavior will result in no tenant found for the request.
* Updated samples.
* Updated documentation.
* Updated unit tests.

**4.0.0**
* Added support for ASP.NET Core 3! Valid project targets are `netcoreapp3.0`, `netcoreapp2.0`, and `netcoreapp2.1`.
* Added a sample app for ASP.NET 3 highlighting the route strategy improvements due to the endpoint routing mechanism.
* Fixed a bug where route strategy could throw an exception when used with Razor Pages. Thanks @stardocs-services!
* Support for configuring multiple multitenant strategies. Each will be tried in the order configured until a non-null tenant identifier is returned. The exception is the fallback strategy which always goes last.
* Refactored component assemblies for better dependency control. EFCore can be excluded by referencing `Finbuckle.MultiTenant.AspNetCore` instead of `Finbuckle.MultiTenant`.
* Updated documentation.
* Updated unit tests to check against all valid project targets.
* Symbols package included for debugging.

**3.2.0**
* Added support for any preexisting global query filters in `MultiTenantDbContext` and `MultiTenantIdentityDbContext`. Thanks @nbarbettini!
* Exposed the inner stores and strategies as a property on the respective `StoreInfo` and `StrategyInfo` properties of `MultiTenantContext`. Previously you could only access the wrapper object for each. Thanks @WalternativE!
* Fixed certain methods on `MultiTenantOptionsCache` to be external as originally intended. Thanks @chernihiv!
* Fix a bug with `TryUpdateAsync` in the wrapper store. Thanks @steebwba!
* Updated documentation and fixed typos. Thanks @MesfinMo!

**3.1.0**
* Added a strategy wrapper that handles validation and logging for the active strategy. When implementing `IMultiTenantStrategy` basic validation and logging are automatically provided.
* Added the delegate strategy that accepts a lambda to return the tenant identifier. Configure by calling `WithDelegateStrategy(...)`.
* Added the fallback strategy that provides a tenant identifier if the normal strategy (or remote authentication, if applicable) fails to resolve a tenant. Configure by calling `WithFallbackStrategy(...)`.
* Added `TrySetTenantInfo` as an extension method to `HttpContext`. This will set the `TenantInfo` provided as the current tenant for the request and can optionally reset the service providers so that scoped services are regenerated under the new tenant.
* Updated and improved documentation and sample projects.
* Miscellaneous bug fixes, code improvement, and unit tests.
* Thanks to @nbarbettini for contributing to this release.

**3.0.1**
* Refactored  the global query filter used in `MultiTenantDbContext` and `MultiTenantIdentityDbContext` (Thanks @GordonBlahut!) for better performance and code quality.
* Removed custom `IModelCacheKeyFactory` as it is no longer needed due to the global query filter changes.
* Updated documentation and samples.

**3.0.0**
* Allow resetting option cache per-tenant. This is a breaking change.
* Host strategy can match entire domain as a special case (prior it only matched a single host segment).
* Added a sample project demonstrating a common login page shared by all tenants.
* Overhauled documentation.
* Updated unit and integration tests.

**2.0.2**
* Fixed bug in Identity where `UserLogins` primary key was not adjusted for multitenant usage.
* Updated and Fixed the IdentityDataIsolation sample project.
* General code and test cleanup.

**2.0.1**
* Fixed bug where the `TenantInfo` constructor did not save the passed `Items` collection.
* Tested for compatibility with ASP.NET Core 2.2.
* Updated samples for ASP.NET Core 2.2.
* Cleaned up library dependencies to target ASP.NET Core 2.1 or greater.

**2.0.0 General Changes**
* Changed `TenantContext` to `MultiTenantContext` which includes `TenantInfo`, `StrategyInfo`, and `StoreInfo` properties.
* Namespace changes (e.g. use of `Microsoft.Extensions.DependencyInjection` namespace for `Configure` and `ConfigureServices` methods).
* Additional and improved unit tests.
* Updated sample project dependencies.
* Various other internal improvements to code and bug fixes.

**2.0.0 MultiTenant Store Enhancements**
* `TryUpdate` method added to `IMultiTenantStore` interface.
* Added `EFCoreStore` which allows an Entity Framework Core database context as the tenant store.
* Added sample project demonstrating use of `EFCoreStore`.
* Custom can be configured with custom dependency injection lifetime (single, scoped, or transient) via `WithStore` method overloads.
* Custom stores automatically receive logging and error support via internal use of `MultiTenantStoreWrapper`.

**2.0.0 MultiTenant Strategy Enhancements**
* Use of async/await for strategy execution for improved performance.
* Custom strategies can be configured with custom dependency injection lifetime (single, scoped, or transient) via `WithStrategy` method overloads.
* Moved route configuration for RouteStrategy from `UseMultiTenant` to `WithRouteStrategy`.

**1.2.0**
* Added variants of `MultiTenantIdentityDbContext` which allows more flexible integration with Identity (Thanks @Cpcrook!)
* Added sample project for data isolation with Identity
* Minor refactoring and more unit tests
* Various bug fixes

**1.1.1**
* Fixed bug affecting per-tenant data isolation using a shared database
* Added sample project for data isolation
* Added new constructors for `MultiTenantDbContext` and `MultiTenantIdentityDbContext`

**1.1.0**
* Remote authentication support
* Strategy improvements
* Store improvements
* Per-tenant options improvements
* Logging support
* Updated samples
* Improved unit and integration tests
* Switch to Apache 2.0 license

**1.0.0**
* Initial release