# Finbuckle.MultiTenant 1.2.0-dev

Finbuckle.MultiTenant is a .NET Standard library for multitenant support designed for ASP.NET 2.0+. It provides functionality for tenant resolution, per-tenant app configuration, and per-tenant data isolation.

See [https://www.finbuckle.com](https://www.finbuckle.com) for more details and documentation.  

See [LICENSE](LICENSE) file for license information.

## Version History

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
