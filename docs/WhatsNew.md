# What's New in v<span class="_version">9.3.0</span>

> This page only lists release update details specific to v<span class="_version">9.3.0</span>. [Release update details for all releases are shown in the history page.](History)

<!--_release-notes-->
## [9.3.0](https://github.com/Finbuckle/Finbuckle.MultiTenant/compare/v9.2.2...v9.3.0) (2025-08-09)

### Features

* Add a RedirectTo Uri to ShortCircuitWhenOptions, enabling the middleware to redirect the user when short circuiting. ([64ad8e2](https://github.com/Finbuckle/Finbuckle.MultiTenant/commit/64ad8e2851b670f37f6a7d2b1310e3715484d314))
* Add ExcludeFromMultiTenantResolution() and ExcludeFromMultiTenantResolutionAttribute for endpoints. ([5cd9d3c](https://github.com/Finbuckle/Finbuckle.MultiTenant/commit/5cd9d3c37ba85aaaadad231aa1eaf807f6625e26))
* Add ShortCircuitWhen(Action<ShortCircuitWhenOptions> options) and ShortCircuitWhenTenantNotResolved() ([709b0de](https://github.com/Finbuckle/Finbuckle.MultiTenant/commit/709b0deda80ce66d8096b53d42b10b83b0bf5297))

### Bug Fixes

* support decorated multiple registered services of same type and fix activator bugs for instance and factory based services ([#994](https://github.com/Finbuckle/Finbuckle.MultiTenant/issues/994)) ([0d1b68c](https://github.com/Finbuckle/Finbuckle.MultiTenant/commit/0d1b68c135052a206e52e6f2bd68f8b813f5d6b7))
* update dependencies ([#995](https://github.com/Finbuckle/Finbuckle.MultiTenant/issues/995)) ([0e0440a](https://github.com/Finbuckle/Finbuckle.MultiTenant/commit/0e0440a9391de4be970e3a9648bcac408e841323))

<!--_release-notes-->
