## Version 
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