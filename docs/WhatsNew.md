# What's New in v<span class="_version">10.0.0</span>

> This page only lists release update details specific to v<span class="_version">10.0.0</span>. [Release update details for all releases are shown in the history page.](History)

<!--_release-notes-->
## [10.0.0](https://github.com/Finbuckle/Finbuckle.MultiTenant/compare/v9.4.2...v10.0.0) (2025-11-20)

### ⚠ BREAKING CHANGES

* In prior versions user provided generic types to the `MultiTenantIdentityDbContext` family of classes were not mult-tenant by default. This was confusing and to simplify all are now multi-tenant by default.
* The `RouteStrategy` will include the tenant in the ambient route values used for link generation, similar to `Controller` and `Action`. Can be disabled via the `WithRouteStrategy` overload taking a boolean for `useTenantAmbientRouteValue` set to false.
* General improvements in folder structure to reduce overnesting has caused namespace changes for certain types. Namely some stores and options types.
* BasePathStrategy default behavior is changed to rebase the aspnetcore path base. This was opt in before. Can be set via `BasePathStrategyOptions`.
* This was opt in before. Can be set via `BasePathStrategyOptions`.
* Namespaces were standardized to match folder locations.
* Making TenantInfo a record reduces risk of unintended changes to the current tenant. This change also removes `ITenantInfo` and `TenantInfo` should be used as the base for custom implementations. Note that `EFCoreStore` uses this record as an entity but takes care not to rely on tracking.
* prior extension namespaces were inconsistent, now they are all `{PackageName}.Extensions`, e.g. `Finbuckle.MultiTenant.Options.Extensions`
* Per-tenant options support was previously part of the Finbuckle.MultiTenant package. Projects will need to reference the Finbuckle.MultiTenant.Options package going forward.
* Minor changes to the `IMultiTenantStore` interface signatures.
* Removes the 64 character limit on Tenant ID. This also removes the max length constraint where Tenant ID is used in EF Core if applicable.
* `IMultiTenantContext` and its implementations are now immutable. Changes will require assigning a new instance.
* Prior to this change anonymous filters were used and required special consideration in advanced scenarios. The change to named filters removes these considerations, but named filters cannot be mixed with anonymous filters.
* net8 and net9 targets were removed
* This change better isolates dependencies. Basic interfaces and types are now in the `Finbuckle.MultiTenant.Abstractions` package and `MultiTenantIdentityDbContext` functionality is now in the `Finbuckle.MultiTenant.Identity.EntityFrameworkCore` package.

### Features

* add `IsNotMultiTenant` method to exclude entities from multi-tenancy in EF Core per-tenant data functionality. ([b160826](https://github.com/Finbuckle/Finbuckle.MultiTenant/commit/b160826d33a41957dddaa664984d1c92124fe97b))
* add Identity passkey multi-tenant support ([7f0bf73](https://github.com/Finbuckle/Finbuckle.MultiTenant/commit/7f0bf738107c0ce72d63ffc75a81a88415ab7bec))
* add MultiTenantAmbientValueLinkGenerator to promote tenant route values ([#1041](https://github.com/Finbuckle/Finbuckle.MultiTenant/issues/1041)) ([259511c](https://github.com/Finbuckle/Finbuckle.MultiTenant/commit/259511ca7100d836e17d7ce5dffabe42bb276b81))
* all projects target net10 ([#1007](https://github.com/Finbuckle/Finbuckle.MultiTenant/issues/1007)) ([1f02e8f](https://github.com/Finbuckle/Finbuckle.MultiTenant/commit/1f02e8f9be2a25048b410e312641ef7e2f12cc26))
* BasePathStrategy will rebase the aspnetcore path base by default ([bd7f0d0](https://github.com/Finbuckle/Finbuckle.MultiTenant/commit/bd7f0d064f539850309e89041db0ecf9999a87dc))
* BasePathStrategy will rebase the aspnetcore path base by default ([d107d81](https://github.com/Finbuckle/Finbuckle.MultiTenant/commit/d107d818ded597ea298108afd1e085b9f241dde8))
* expose many internal types as public and adjust namespaces ([#1030](https://github.com/Finbuckle/Finbuckle.MultiTenant/issues/1030)) ([f680843](https://github.com/Finbuckle/Finbuckle.MultiTenant/commit/f6808434e90b820fe54ff8638085bd2316153a1d))
* Identity entity types are all multi-tenant by default on `MultiTenantIdentityDbContext` variants. ([4e1bd9f](https://github.com/Finbuckle/Finbuckle.MultiTenant/commit/4e1bd9faf111344499fcf4feb0eee1636737eef7))
* immutable IMultiTenantContext ([#1018](https://github.com/Finbuckle/Finbuckle.MultiTenant/issues/1018)) ([03ddeb0](https://github.com/Finbuckle/Finbuckle.MultiTenant/commit/03ddeb067654c23e3747f7d2c90b33f7ca0ceeb9))
* improve folder structure ([#1040](https://github.com/Finbuckle/Finbuckle.MultiTenant/issues/1040)) ([d46ce8c](https://github.com/Finbuckle/Finbuckle.MultiTenant/commit/d46ce8c9fb6e6b1cc30c09f6fd4fbb05e076a0bc))
* improved depedency structure with `Finbuckle.MultiTenant.Abstractions` and `Finbuckle.MultiTenant.Identity.EntityFrameworkCore` ([#1006](https://github.com/Finbuckle/Finbuckle.MultiTenant/issues/1006)) ([e191d83](https://github.com/Finbuckle/Finbuckle.MultiTenant/commit/e191d83dfe2b161aeafcf08bcca5978d23bcd783))
* improved store interface ([#1020](https://github.com/Finbuckle/Finbuckle.MultiTenant/issues/1020)) ([c6a16c4](https://github.com/Finbuckle/Finbuckle.MultiTenant/commit/c6a16c44880ac1edcf5b7c09da9986091efe3a52))
* improved xml comments ([#1038](https://github.com/Finbuckle/Finbuckle.MultiTenant/issues/1038)) ([fdd59b9](https://github.com/Finbuckle/Finbuckle.MultiTenant/commit/fdd59b980d9209296d7e4af6a3bb73211e9aa91c))
* improved xml comments ([#1038](https://github.com/Finbuckle/Finbuckle.MultiTenant/issues/1038)) ([8ee6597](https://github.com/Finbuckle/Finbuckle.MultiTenant/commit/8ee65977088989cfb16936fbd70999789cca9d90))
* namespaces for extension methods changed ([#1026](https://github.com/Finbuckle/Finbuckle.MultiTenant/issues/1026)) ([318fcec](https://github.com/Finbuckle/Finbuckle.MultiTenant/commit/318fcecefc53fa765e5d50de730baa8b86be4b95))
* refactors per-tenant options into Finbuckle.MultiTenant.Options package ([#1024](https://github.com/Finbuckle/Finbuckle.MultiTenant/issues/1024)) ([ca4877f](https://github.com/Finbuckle/Finbuckle.MultiTenant/commit/ca4877f2ffccc3d8d686cf449dd865e05db3f6cb))
* removed max char length on TenantInfo ([#1019](https://github.com/Finbuckle/Finbuckle.MultiTenant/issues/1019)) ([37bb15b](https://github.com/Finbuckle/Finbuckle.MultiTenant/commit/37bb15b18c9af505202cc49b221f71654b23ad05))
* TenantInfo is now a record and `ITenantInfo` is removed ([#1029](https://github.com/Finbuckle/Finbuckle.MultiTenant/issues/1029)) ([21559da](https://github.com/Finbuckle/Finbuckle.MultiTenant/commit/21559dab15e20a451aae49252b128bad81549977))
* use named global query filters in EF Core ([#1016](https://github.com/Finbuckle/Finbuckle.MultiTenant/issues/1016)) ([c3ac833](https://github.com/Finbuckle/Finbuckle.MultiTenant/commit/c3ac833a561c6e122d5d618659cf2308c9a0c0c1))

### Bug Fixes

* update dependencies ([#1023](https://github.com/Finbuckle/Finbuckle.MultiTenant/issues/1023)) ([69ac561](https://github.com/Finbuckle/Finbuckle.MultiTenant/commit/69ac5613526acb3e3001bf284698853a3feb9b4e))
* update dependencies ([#1027](https://github.com/Finbuckle/Finbuckle.MultiTenant/issues/1027)) ([b185944](https://github.com/Finbuckle/Finbuckle.MultiTenant/commit/b185944a63f3027fed0f04b3aed9eb2491f16959))
* update dependencies ([#1037](https://github.com/Finbuckle/Finbuckle.MultiTenant/issues/1037)) ([b168900](https://github.com/Finbuckle/Finbuckle.MultiTenant/commit/b1689005f0ee1437762a7ac511d90931fd2364f1))

### Performance Improvements

* new Lock use ([#1022](https://github.com/Finbuckle/Finbuckle.MultiTenant/issues/1022)) ([55bde60](https://github.com/Finbuckle/Finbuckle.MultiTenant/commit/55bde60fd81c14ca6bdac6d628cd27b3f098f8eb))

<!--_release-notes-->
