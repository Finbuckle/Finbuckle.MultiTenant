# What's New in v<span class="_version">9.0.0</span>

> This page only lists release update details specific to v<span class="_version">9.0.0</span>. [Release update details for all releases are shown in the history page.](History)

<!--_release-notes-->
# [9.0.0](https://github.com/Finbuckle/Finbuckle.MultiTenant/compare/v8.0.0...v9.0.0) (2024-11-13)


* multitenant db factory ([#894](https://github.com/Finbuckle/Finbuckle.MultiTenant/issues/894)) ([ea216ff](https://github.com/Finbuckle/Finbuckle.MultiTenant/commit/ea216ffb37d241804899ab0f3cd5db1e9c6ae641))


### Bug Fixes

* remove deprecated dotnet ([#891](https://github.com/Finbuckle/Finbuckle.MultiTenant/issues/891)) ([1429cbf](https://github.com/Finbuckle/Finbuckle.MultiTenant/commit/1429cbf0bb054fee9d39d24d6b7d34c24fc0074e))


### Features

* add multitenant db factory method ([#896](https://github.com/Finbuckle/Finbuckle.MultiTenant/issues/896)) ([8728447](https://github.com/Finbuckle/Finbuckle.MultiTenant/commit/8728447f47df8d72f394e02b18ea76ff0051b593))
* better tenant resolution events ([#897](https://github.com/Finbuckle/Finbuckle.MultiTenant/issues/897)) ([956ca36](https://github.com/Finbuckle/Finbuckle.MultiTenant/commit/956ca36670aa8aed38afcbbbdd78f1b79d91287c))
* dotnet 9 support ([#893](https://github.com/Finbuckle/Finbuckle.MultiTenant/issues/893)) ([4be1b88](https://github.com/Finbuckle/Finbuckle.MultiTenant/commit/4be1b88fb7103223517d2cd8a16ea62c6d6204d5))
* Make builds deterministic and set latest GH actions ([#889](https://github.com/Finbuckle/Finbuckle.MultiTenant/issues/889)) ([d82f89d](https://github.com/Finbuckle/Finbuckle.MultiTenant/commit/d82f89da2f7a82bb302aaedfdb5c676cc7051273))


### Reverts

* Revert "multitenant db factory ([#894](https://github.com/Finbuckle/Finbuckle.MultiTenant/issues/894))" ([#895](https://github.com/Finbuckle/Finbuckle.MultiTenant/issues/895)) ([0e164a8](https://github.com/Finbuckle/Finbuckle.MultiTenant/commit/0e164a8fec637c7e1112e43c2cc8c4e6f8ca4d77))


### BREAKING CHANGES

* `OnTenantResolved` and `OnTenantNotResolved` are no longer used. Use the `OnStrategyResolveCompleted`, `OnStoreResolveCompleted`, and `OnTenantResolveCompleted` events instead.
* `MultiTenantDbContext` constructors accepting ITenantInfo removed, use `MultiTenantDbContext.Create` factory method
* `MultiTenantDbContext` constructors accepting ITenantInfo removed, use `MultiTenantDbContext .Create` factory method instead
* net6.0 and net7.0 are no longer supported targets.
* Dotnet runtime specific dependencies now float to the latest patch version and are locked at release time with a NuGet lock file. This is a security mitigation and may break some builds not on the latest SDKs.




<!--_release-notes-->
