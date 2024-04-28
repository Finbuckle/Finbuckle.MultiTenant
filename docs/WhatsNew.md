# What's New in v<span class="_version">7.0.0</span>

> This page only lists release update details specific to v<span class="_version">7.0.0</span>. [Release update details for all releases are shown in the history page.](History)

<!--_release-notes-->
## [7.0.0](https://github.com/Finbuckle/Finbuckle.MultiTenant/compare/v6.13.1...v7.0.0) (2024-04-21)


### ⚠ BREAKING CHANGES

* (I)MultiTenantContext and (I)TenantInfo are no longer available via dependency injection. Use
  IMultiTenantContextAccessor instead. MultiTenantDbContext and MultiTenantIdentityDbContext will require a new
  constructor that injects IMultiTenantContextAccessor or IMultiTenantContext<TTenantInfo>.
* Many namespaces have been updated for consistency. Most code will only need to use the Finbuckle.MultiTenant or
  Finbuckle.MultiTenant.Abstractions namespace.
* Connection string is removed from ITenantInfo and the default TenantInfo implementation.
* Added support for OptionsBuilder API and more efficient per-tenant options overall.
* WithPerTenantOptions replaced by ConfigurePerTenant service collection extensions methods.
* Unique indexes and the UserLogin primary key in the standard Identity models adjusted to include tenant id.
* IMultiTenantContext nullability reworked and should never be null.

### Features

* better options support ([#681](https://github.com/Finbuckle/Finbuckle.MultiTenant/issues/681)) ([1859017](https://github.com/Finbuckle/Finbuckle.MultiTenant/commit/185901786f2225eb38b3609770c60b98fdcbc122))
* change default MultiTenantIdentityDbContext default index and key behavior ([81f5612](https://github.com/Finbuckle/Finbuckle.MultiTenant/commit/81f5612008c3335192d9b26beb22be9a28beca8b))
* MultiTenantDbContext and MultiTenantIdentityDbContext support for IMultiTenantContextAccessor DI ([9015085](https://github.com/Finbuckle/Finbuckle.MultiTenant/commit/901508563af4fa872a0dc3930ff3b8315777b912))
* namespace cleaned up ([b354838](https://github.com/Finbuckle/Finbuckle.MultiTenant/commit/b354838a90741626c47ea4f109c49f7fe2ca5b3d))
* refactor DI and improve nullability ([eca24bf](https://github.com/Finbuckle/Finbuckle.MultiTenant/commit/eca24bfa0c314f95794b235141cff42059cf3fcf))
* remove ConnectionString from ITenantInfo and TenantInfo ([f4e20db](https://github.com/Finbuckle/Finbuckle.MultiTenant/commit/f4e20db35fe9e31e1cfb37a667b1ba4b64ce6f3f))


### Bug Fixes

* AdjustKey correctly adding TenantId to primary and foreign keys ([613b4a8](https://github.com/Finbuckle/Finbuckle.MultiTenant/commit/613b4a855e919e02910c42f9f534cddba40339c9))


<!--_release-notes-->
