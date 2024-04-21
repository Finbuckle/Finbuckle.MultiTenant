# What's New in Finbuckle.MultiTenant <span class="_version">7.0.0</span>

<!--_release-notes-->
## [7.0.0](https://github.com/Finbuckle/Finbuckle.MultiTenant/compare/v6.13.1...v7.0.0) (2024-04-21)


### ⚠ BREAKING CHANGES

* Many namespaces have been updated for consistency. Most code will only need to reference the  namespace.
* Connection string is removed because in many cases it is not needed. Closes #624
* all unique indexes and the UserLogin primary key in the standard Identity models are adjusted to include the tenant id
* (I)MultiTenantContext and (I)TenantInfo are no longer available via DI. Use IMultiTenantContextAccessor instead. Also IMultiTenantContext nullability reworked and should never be null.
* WithPerTenantOptions replaced by ConfigurePerTenant service collection extensions methods.

Added support for  API and more efficient per-tenant options overall.

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
