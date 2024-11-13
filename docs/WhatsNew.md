# What's New in v<span class="_version">9.0.0</span>

> This page only lists release update details specific to v<span class="_version">9.0.0</span>. [Release update details for all releases are shown in the history page.](History)

<!--_release-notes-->
# [9.0.0](https://github.com/Finbuckle/Finbuckle.MultiTenant/compare/v8.0.0...v9.0.0) (2024-11-13)

### Features

* add multitenant db factory method ([#896](https://github.com/Finbuckle/Finbuckle.MultiTenant/issues/896)) ([8728447](https://github.com/Finbuckle/Finbuckle.MultiTenant/commit/8728447f47df8d72f394e02b18ea76ff0051b593))
* better tenant resolution events ([#897](https://github.com/Finbuckle/Finbuckle.MultiTenant/issues/897)) ([956ca36](https://github.com/Finbuckle/Finbuckle.MultiTenant/commit/956ca36670aa8aed38afcbbbdd78f1b79d91287c))
* dotnet 9 support ([#893](https://github.com/Finbuckle/Finbuckle.MultiTenant/issues/893)) ([4be1b88](https://github.com/Finbuckle/Finbuckle.MultiTenant/commit/4be1b88fb7103223517d2cd8a16ea62c6d6204d5))
* make builds deterministic and set latest GH actions ([#889](https://github.com/Finbuckle/Finbuckle.MultiTenant/issues/889)) ([d82f89d](https://github.com/Finbuckle/Finbuckle.MultiTenant/commit/d82f89da2f7a82bb302aaedfdb5c676cc7051273))


### BREAKING CHANGES

* `OnTenantResolved` and `OnTenantNotResolved` are no longer used. Use the `OnStrategyResolveCompleted`, `OnStoreResolveCompleted`, and `OnTenantResolveCompleted` events instead.
* `MultiTenantDbContext` constructors accepting ITenantInfo removed, use `MultiTenantDbContext.Create` factory method
* `MultiTenantDbContext` constructors accepting ITenantInfo removed, use `MultiTenantDbContext .Create` factory method instead
* net6.0 and net7.0 are no longer supported targets.
* dotnet runtime specific dependencies now float to the latest patch version and are locked at release time with a NuGet lock file. This is a security mitigation and may break some builds not on the latest SDKs.

<!--_release-notes-->
