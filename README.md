# Finbuckle.MultiTenant 2.0.0

Finbuckle.MultiTenant is a .NET Standard library for multitenant support designed for ASP.NET Core 2. It provides functionality for tenant resolution, per-tenant app configuration, and per-tenant data isolation.

See [https://www.finbuckle.com](https://www.finbuckle.com) for more details and documentation.  

See [LICENSE](LICENSE) file for license information.

## Version History

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
* Custom can be configured with custom dependenct injection lifetime (single, scoped, or transient) via `WithStore` method overloads.
* Custom stores automatically receive logging and error support via internal use of `MultiTenantStoreWrapper`.

**2.0.0 MultiTenant Strategy Enhancements**
* Use of async/await for strategy execution for improved performance.
* Custom strategies can be configured with custom dependenct injection lifetime (single, scoped, or transient) via `WithStrategy` method overloads.
* Moved route configuration for RouteStrategy from `UseMultiTenant` to `WithRouteStrategy`.

**1.2.0**
* Added variants of `MultiTenantIdentityDbContext` which allows more flexible integration with Identity (Thanks Cpcrook!)
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