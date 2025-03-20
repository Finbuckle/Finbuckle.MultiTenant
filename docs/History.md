# Version History

<!--_history-->
## [9.1.2](https://github.com/Finbuckle/Finbuckle.MultiTenant/compare/v9.1.1...v9.1.2) (2025-03-20)

### Bug Fixes

* reenable deterministic builds ([#954](https://github.com/Finbuckle/Finbuckle.MultiTenant/issues/954)) ([0cd78b3](https://github.com/Finbuckle/Finbuckle.MultiTenant/commit/0cd78b3ac917917dd58280733014a7589c05423a))

## [9.1.1](https://github.com/Finbuckle/Finbuckle.MultiTenant/compare/v9.1.0...v9.1.1) (2025-03-18)

### Bug Fixes

* fix cache store leaving orphan tenant on some update scenarios ([#946](https://github.com/Finbuckle/Finbuckle.MultiTenant/issues/946)) ([5edd9fe](https://github.com/Finbuckle/Finbuckle.MultiTenant/commit/5edd9fee3e59207b67884514322c8f115bf4a543))
* update dependency versions ([#953](https://github.com/Finbuckle/Finbuckle.MultiTenant/issues/953)) ([67598c1](https://github.com/Finbuckle/Finbuckle.MultiTenant/commit/67598c1489a38e83be1ca0779addb334319d8e7a))

## [9.1.0](https://github.com/Finbuckle/Finbuckle.MultiTenant/compare/v9.0.0...v9.1.0) (2025-03-12)

### Features

* add WithDelegateStrategy<TContext, TTenantInfo> ([#932](https://github.com/Finbuckle/Finbuckle.MultiTenant/issues/932)) ([a18a935](https://github.com/Finbuckle/Finbuckle.MultiTenant/commit/a18a935ea397b4cbd9e52fdcee589212db9d0999))
* add WithHttpContextStrategy ([#934](https://github.com/Finbuckle/Finbuckle.MultiTenant/issues/934)) ([e6aeb7c](https://github.com/Finbuckle/Finbuckle.MultiTenant/commit/e6aeb7c9c265d39eb4bec0d2be526f582f41f886))
* better async pattern for class libraries ([#942](https://github.com/Finbuckle/Finbuckle.MultiTenant/issues/942)) ([8235462](https://github.com/Finbuckle/Finbuckle.MultiTenant/commit/82354625d24ced67228246885f9f326389209e9c))
* improved perf for HostStrategy ([#936](https://github.com/Finbuckle/Finbuckle.MultiTenant/issues/936)) ([a70f7e0](https://github.com/Finbuckle/Finbuckle.MultiTenant/commit/a70f7e0f47eaf180e5f64f1bba61cae59086251a))

### Bug Fixes

* added new analyzers and multiple fixes  ([#937](https://github.com/Finbuckle/Finbuckle.MultiTenant/issues/937)) ([9d9b1e4](https://github.com/Finbuckle/Finbuckle.MultiTenant/commit/9d9b1e416929ebbd795c0eef3361cca7d6d238ce))
* MultiTenantDbContext.Create should have non nullable TenantInfo ([#916](https://github.com/Finbuckle/Finbuckle.MultiTenant/issues/916)) ([9df0527](https://github.com/Finbuckle/Finbuckle.MultiTenant/commit/9df0527841549e0dd6e2017cfff3ebc6f03b7e16))

## [9.0.0](https://github.com/Finbuckle/Finbuckle.MultiTenant/compare/v8.0.0...v9.0.0) (2024-11-13)


### ⚠ BREAKING CHANGES

* `OnTenantResolved` and `OnTenantNotResolved` are no longer used. Use the `OnStrategyResolveCompleted`, `OnStoreResolveCompleted`, and `OnTenantResolveCompleted` events instead.
* `MultiTenantDbContext` constructors accepting `ITenantInfo` removed, use `MultiTenantDbContext.Create` factory method instead.
* net6.0 and net7.0 are no longer supported targets.
* dotnet runtime specific dependencies now float to the latest patch version and are locked at release time with a NuGet lock file. This is a security mitigation and may break some builds not on the latest SDKs.

### Features

* add multitenant db factory method ([#894](https://github.com/Finbuckle/Finbuckle.MultiTenant/issues/894)) (
  [ea216ff](https://github.com/Finbuckle/Finbuckle.MultiTenant/commit/ea216ffb37d241804899ab0f3cd5db1e9c6ae641))
* better tenant resolution events ([#897](https://github.com/Finbuckle/Finbuckle.MultiTenant/issues/897)) ([956ca36](https://github.com/Finbuckle/Finbuckle.MultiTenant/commit/956ca36670aa8aed38afcbbbdd78f1b79d91287c))
* dotnet 9 support ([#893](https://github.com/Finbuckle/Finbuckle.MultiTenant/issues/893)) ([4be1b88](https://github.com/Finbuckle/Finbuckle.MultiTenant/commit/4be1b88fb7103223517d2cd8a16ea62c6d6204d5))
* make builds deterministic and set latest GH actions ([#889](https://github.com/Finbuckle/Finbuckle.MultiTenant/issues/889)) ([d82f89d](https://github.com/Finbuckle/Finbuckle.MultiTenant/commit/d82f89da2f7a82bb302aaedfdb5c676cc7051273))

## [8.1.0](https://github.com/Finbuckle/Finbuckle.MultiTenant/compare/v8.0.0...v8.1.0) (2025-03-18)

### Bug Fixes

* fix cache store leaving orphan tenant on some update scenarios ([#949](https://github.com/Finbuckle/Finbuckle.MultiTenant/issues/949)) ([7929f00](https://github.com/Finbuckle/Finbuckle.MultiTenant/commit/7929f00bc2eb8ef82ee01e431bc04ef3b63d6f0f))


### Features

* better async backported ([#951](https://github.com/Finbuckle/Finbuckle.MultiTenant/issues/951)) ([a204403](https://github.com/Finbuckle/Finbuckle.MultiTenant/commit/a204403666794ea42c850ff5bfb736f1ced30e45))

## [8.0.0](https://github.com/Finbuckle/Finbuckle.MultiTenant/compare/v7.0.2...v8.0.0) (2024-10-12)

### ⚠ BREAKING CHANGES

* This commit brings the release into alignment with the new version policy. See #887 for details.
* Included strategies for ASP.NET Core would throw an exception if the passed context was not an `HttpContext` type. Now they will return null indicating no identifier was found.

### Features

* add GetAllAsync() support for HttpRemoteStore ([#848](https://github.com/Finbuckle/Finbuckle.MultiTenant/issues/848)) ([4208149](https://github.com/Finbuckle/Finbuckle.MultiTenant/commit/4208149282eaee99e2c02a788a2653faaa24ef7a))
* added the Echo Store. ([#807](https://github.com/Finbuckle/Finbuckle.MultiTenant/issues/807)) ([a3e5eee](https://github.com/Finbuckle/Finbuckle.MultiTenant/commit/a3e5eee64f0581c5f3d6abca7bb77cc56ef1d75c))
* strategies return null on invalid context type ([#885](https://github.com/Finbuckle/Finbuckle.MultiTenant/issues/885)) ([9834575](https://github.com/Finbuckle/Finbuckle.MultiTenant/commit/9834575c957fb5bd314cf0970e54a29384026d02))

### Bug Fixes

* BasePathStrategy no longer breaks the strategy chain ([#884](https://github.com/Finbuckle/Finbuckle.MultiTenant/issues/884)) ([3263eff](https://github.com/Finbuckle/Finbuckle.MultiTenant/commit/3263effae4638656aab827b24094a8e575ae19a0))
* prevent duplicate key annotation in AdjustKey() ([#883](https://github.com/Finbuckle/Finbuckle.MultiTenant/issues/883)) ([f75ba2c](https://github.com/Finbuckle/Finbuckle.MultiTenant/commit/f75ba2c678079d2e956bf7d32b8e5aee0159e72b))
* removed unused parameter from WithPerTenantRemoteAuthenticationConvention ([#886](https://github.com/Finbuckle/Finbuckle.MultiTenant/issues/886)) ([dd17ab5](https://github.com/Finbuckle/Finbuckle.MultiTenant/commit/dd17ab51825ec94f4ecfe704f42c6b0457562d98))


## [7.0.2](https://github.com/Finbuckle/Finbuckle.MultiTenant/compare/v7.0.1...v7.0.2) (2024-08-03)


### Bug Fixes

* Preserve annotations when adjusting index ([#832](https://github.com/Finbuckle/Finbuckle.MultiTenant/issues/832)) ([e765340](https://github.com/Finbuckle/Finbuckle.MultiTenant/commit/e765340a3c74268cadf191a55e9a5c082894c2bd))
* strategy wrapper no longer throws on a null context, instead passing it to the actual strategy ([#863](https://github.com/Finbuckle/Finbuckle.MultiTenant/issues/863)) ([2b165c7](https://github.com/Finbuckle/Finbuckle.MultiTenant/commit/2b165c77db4f82244e33fe1823e865f30b2a3ea2))

## [7.0.1](https://github.com/Finbuckle/Finbuckle.MultiTenant/compare/v7.0.0...v7.0.1) (2024-04-28)


### Bug Fixes

* only throw exception in EnforceMultiTenant for null tenant if there are entity changes. ([#819](https://github.com/Finbuckle/Finbuckle.MultiTenant/issues/819)) ([ca9e9fd](https://github.com/Finbuckle/Finbuckle.MultiTenant/commit/ca9e9fd9a55789d790d31f82756e5ecdac03a28f))

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

## [6.13.1](https://github.com/Finbuckle/Finbuckle.MultiTenant/compare/v6.13.0...v6.13.1) (2024-01-24)


### Bug Fixes

* update dependency to protect against CVE-2024-21319 ([#781](https://github.com/Finbuckle/Finbuckle.MultiTenant/issues/781)) ([c5e0c8a](https://github.com/Finbuckle/Finbuckle.MultiTenant/commit/c5e0c8a8e3f60033f97993b7feaf4ff87150a0f8))

## [6.13.0](https://github.com/Finbuckle/Finbuckle.MultiTenant/compare/v6.12.0...v6.13.0) (2023-12-21)


### Features

* .net8.0 LTS release support ([#770](https://github.com/Finbuckle/Finbuckle.MultiTenant/issues/770)) ([d7f08f9](https://github.com/Finbuckle/Finbuckle.MultiTenant/commit/d7f08f94896b8efa8ca1877bcb0c4920b98ba049))


### Bug Fixes

* OnTenantNotResolved not called correctly ([#729](https://github.com/Finbuckle/Finbuckle.MultiTenant/issues/729)) ([a26081c](https://github.com/Finbuckle/Finbuckle.MultiTenant/commit/a26081c960786c3eb33f9b2173feed6c33427a74)), closes [#628](https://github.com/Finbuckle/Finbuckle.MultiTenant/issues/628)

## [6.12.0](https://github.com/Finbuckle/Finbuckle.MultiTenant/compare/v6.11.1...v6.12.0) (2023-08-25)


### Features

* AdjustIndex preserves existing filter ([#711](https://github.com/Finbuckle/Finbuckle.MultiTenant/issues/711)) ([affb66f](https://github.com/Finbuckle/Finbuckle.MultiTenant/commit/affb66f268a133916c17c7797138cd16dc67e728))
* net8.0 ([#712](https://github.com/Finbuckle/Finbuckle.MultiTenant/issues/712)) ([a137dae](https://github.com/Finbuckle/Finbuckle.MultiTenant/commit/a137dae907b2f9b465bee735e9e9eddec64bddf5))

## [6.11.1](https://github.com/Finbuckle/Finbuckle.MultiTenant/compare/v6.11.0...v6.11.1) (2023-07-06)


### Bug Fixes

* make DecorateService public ([#671](https://github.com/Finbuckle/Finbuckle.MultiTenant/issues/671)) ([c9746d6](https://github.com/Finbuckle/Finbuckle.MultiTenant/commit/c9746d6655f2fa0130031885ebf9b4980a93c531)), closes [#668](https://github.com/Finbuckle/Finbuckle.MultiTenant/issues/668)

## [6.11.0](https://github.com/Finbuckle/Finbuckle.MultiTenant/compare/v6.10.0...v6.11.0) (2023-07-01)


### Features

* add HasResolvedTenant to IMultiTenantContext ([#650](https://github.com/Finbuckle/Finbuckle.MultiTenant/issues/650)) ([375add5](https://github.com/Finbuckle/Finbuckle.MultiTenant/commit/375add51e7317b03652556d4d1d0eb7ef20b8caf))
* perf improvements in BasePathStrategy and  RemoteAuthenticationCallbackStrategy ([#654](https://github.com/Finbuckle/Finbuckle.MultiTenant/issues/654)) ([ac1c58a](https://github.com/Finbuckle/Finbuckle.MultiTenant/commit/ac1c58aed8ef0f053bfc26adb95078b481c26a58))


### Bug Fixes

* internal refactoring and improved XML comments for intellisense ([c42c53d](https://github.com/Finbuckle/Finbuckle.MultiTenant/commit/c42c53d6c52bfab340327e40d44060f2bb550010))
* xml docs corrections ([#639](https://github.com/Finbuckle/Finbuckle.MultiTenant/issues/639)) ([265d26d](https://github.com/Finbuckle/Finbuckle.MultiTenant/commit/265d26d338b96813d061cb1b16ed1b575ef48469))

## [6.10.0](https://github.com/Finbuckle/Finbuckle.MultiTenant/compare/v6.9.1...v6.10.0) (2023-01-30)


### Features

* add nongeneric IMultiTenantContext for flexibility ([b3a198f](https://github.com/Finbuckle/Finbuckle.MultiTenant/commit/b3a198f46aba9370f3671b62c35ae06b829a7d73))


### Bug Fixes

* fixes undesired context tracking across EFCoreStore methods ([#633](https://github.com/Finbuckle/Finbuckle.MultiTenant/issues/633)) ([3605a75](https://github.com/Finbuckle/Finbuckle.MultiTenant/commit/3605a75482a4f585dc1115559a40a81eac437154))
* remove netcore3.1 ([#632](https://github.com/Finbuckle/Finbuckle.MultiTenant/issues/632)) ([6c21fe9](https://github.com/Finbuckle/Finbuckle.MultiTenant/commit/6c21fe999c9d15e50cd0e2fcf480b5d442f7f2f3))

## [6.9.1](https://github.com/Finbuckle/Finbuckle.MultiTenant/compare/v6.9.0...v6.9.1) (2022-11-10)


### Bug Fixes

* update for final .NET 7 release ([#610](https://github.com/Finbuckle/Finbuckle.MultiTenant/issues/610)) ([ac32e7d](https://github.com/Finbuckle/Finbuckle.MultiTenant/commit/ac32e7dbb9b2bb7315e4787234677e1643ef0118))

## [6.9.0](https://github.com/Finbuckle/Finbuckle.MultiTenant/compare/v6.8.1...v6.9.0) (2022-10-23)


### Features

* .net7.0 support ([#604](https://github.com/Finbuckle/Finbuckle.MultiTenant/issues/604)) ([4d7d54d](https://github.com/Finbuckle/Finbuckle.MultiTenant/commit/4d7d54d928ecc22b7cc7d7de50223400b00c9f10))


### Bug Fixes

* BasePathStrategy combine path bug ([0628b0f](https://github.com/Finbuckle/Finbuckle.MultiTenant/commit/0628b0f3d27d4a975cf862b8477cec73a29080b2))

## [6.8.1](https://github.com/Finbuckle/Finbuckle.MultiTenant/compare/v6.8.0...v6.8.1) (2022-09-17)


### Bug Fixes

* XML comment and generation fix ([#588](https://github.com/Finbuckle/Finbuckle.MultiTenant/issues/588)) ([c1de82d](https://github.com/Finbuckle/Finbuckle.MultiTenant/commit/c1de82dc9405830ae92ec331b81048a4b485e17b))

## [6.8.0](https://github.com/Finbuckle/Finbuckle.MultiTenant/compare/v6.7.3...v6.8.0) (2022-08-28)


### Features

* opened efcorestore to allow overriding methods ([#577](https://github.com/Finbuckle/Finbuckle.MultiTenant/issues/577)) ([7dac251](https://github.com/Finbuckle/Finbuckle.MultiTenant/commit/7dac251b39cbaf62a5329f71b920fac2288c1ec6)), closes [#574](https://github.com/Finbuckle/Finbuckle.MultiTenant/issues/574)


### Bug Fixes

* add missing using statement to samples ([#581](https://github.com/Finbuckle/Finbuckle.MultiTenant/issues/581)) ([ec8e08e](https://github.com/Finbuckle/Finbuckle.MultiTenant/commit/ec8e08e2bc99a85d28fc8be4aa34070f8eae4437))
* adjust logic in per-tenant-authentication conventions ([e78a26f](https://github.com/Finbuckle/Finbuckle.MultiTenant/commit/e78a26fe1d3fa89b5ece8ecabcb2bce2f7a749ab))
* environment configuration in samples ([#579](https://github.com/Finbuckle/Finbuckle.MultiTenant/issues/579)) ([6df8827](https://github.com/Finbuckle/Finbuckle.MultiTenant/commit/6df882786c656108ffd4f0450c7c4fcb45cfe3fb))
* update authentication.md ([#573](https://github.com/Finbuckle/Finbuckle.MultiTenant/issues/573)) ([df55b24](https://github.com/Finbuckle/Finbuckle.MultiTenant/commit/df55b24668642dfca235071abdbf7b369c2b3a85))

## [6.7.3](https://github.com/Finbuckle/Finbuckle.MultiTenant/compare/v6.7.2...v6.7.3) (2022-07-17)


### Bug Fixes

* drop .net 5.0 target ([#569](https://github.com/Finbuckle/Finbuckle.MultiTenant/issues/569)) ([38fa9e1](https://github.com/Finbuckle/Finbuckle.MultiTenant/commit/38fa9e1cac660822f091c3e71b1746803394308f))
* remove reliance on uncaught exceptions ([#563](https://github.com/Finbuckle/Finbuckle.MultiTenant/issues/563)) ([a675684](https://github.com/Finbuckle/Finbuckle.MultiTenant/commit/a6756842b0558b19d620f9dcd049e30841841406))


### Performance Improvements

* corrected various async/await code ([#557](https://github.com/Finbuckle/Finbuckle.MultiTenant/issues/557)) ([fe7c01b](https://github.com/Finbuckle/Finbuckle.MultiTenant/commit/fe7c01b66336e83ef5f1f108f9c3a92861135d54))

### [6.7.2](https://github.com/Finbuckle/Finbuckle.MultiTenant/compare/v6.7.1...v6.7.2) (2022-04-05)


### Bug Fixes

* update scheme provider to support decorator pattern ([#551](https://github.com/Finbuckle/Finbuckle.MultiTenant/issues/551)) ([ead052a](https://github.com/Finbuckle/Finbuckle.MultiTenant/commit/ead052a45bdb414b26c0373262e9eff0b472e305))

### [6.7.1](https://github.com/Finbuckle/Finbuckle.MultiTenant/compare/v6.7.0...v6.7.1) (2022-03-10)


### Bug Fixes

* use web System.Text.Json setttings and update samples ([#544](https://github.com/Finbuckle/Finbuckle.MultiTenant/issues/544)) ([266e806](https://github.com/Finbuckle/Finbuckle.MultiTenant/commit/266e806964af9b70daa7d1ed93b6b5a96c50ae5d))

## [6.7.0](https://github.com/Finbuckle/Finbuckle.MultiTenant/compare/v6.6.1...v6.7.0) (2022-03-06)


### Features

* Added support for named options ([#478](https://github.com/Finbuckle/Finbuckle.MultiTenant/issues/478)) ([#534](https://github.com/Finbuckle/Finbuckle.MultiTenant/issues/534)) ([6f9528d](https://github.com/Finbuckle/Finbuckle.MultiTenant/commit/6f9528d737f2803cf60f4d66112e53b5b1cb81c6))

### [6.6.1](https://github.com/Finbuckle/Finbuckle.MultiTenant/compare/v6.6.0...v6.6.1) (2022-02-19)


### Bug Fixes

* change delegate strategy func return type as nullabe and adds unit test ([#525](https://github.com/Finbuckle/Finbuckle.MultiTenant/issues/525)) ([80c7104](https://github.com/Finbuckle/Finbuckle.MultiTenant/commit/80c71041ad164da9ae8fb93a3ea0c68998b4e247))
* remove tenant id value generator ([#524](https://github.com/Finbuckle/Finbuckle.MultiTenant/issues/524)) ([0d3dcd8](https://github.com/Finbuckle/Finbuckle.MultiTenant/commit/0d3dcd891d23124c1589b736a0b2274d4fda060f))

## [6.6.0](https://github.com/Finbuckle/Finbuckle.MultiTenant/compare/v6.5.1...v6.6.0) (2022-02-13)


### Features

* add BasePathStrategy option to rebase the AspNetCore Request.PathBase ([#510](https://github.com/Finbuckle/Finbuckle.MultiTenant/issues/510)) ([dccf414](https://github.com/Finbuckle/Finbuckle.MultiTenant/commit/dccf414c1fbb65d8a02b709460679e86c317451a))
* add nullable reference types to all projects ([#514](https://github.com/Finbuckle/Finbuckle.MultiTenant/issues/514)) ([e6141fe](https://github.com/Finbuckle/Finbuckle.MultiTenant/commit/e6141fec807025f8be51e86a82f91b4650a08aa4))
* add strategy type and store type to TenantResolvedContext ([#508](https://github.com/Finbuckle/Finbuckle.MultiTenant/issues/508)) ([ef52fc2](https://github.com/Finbuckle/Finbuckle.MultiTenant/commit/ef52fc21d05508bf4150bcfb7993aac953cd2202))


### Bug Fixes

* actually set the strategy and store types on OnTenantResolvedContext ([#509](https://github.com/Finbuckle/Finbuckle.MultiTenant/issues/509)) ([fd9029e](https://github.com/Finbuckle/Finbuckle.MultiTenant/commit/fd9029e112b1be7b2e029e56a65b88ab7ae618d6))
* remove dependency on NewtonSoft.Json ([#505](https://github.com/Finbuckle/Finbuckle.MultiTenant/issues/505)) ([f83f0b1](https://github.com/Finbuckle/Finbuckle.MultiTenant/commit/f83f0b1bf0278e91f9f4455f080fd00a2e644167))

### [6.5.1](https://github.com/Finbuckle/Finbuckle.MultiTenant/compare/v6.5.0...v6.5.1) (2021-11-17)


### Bug Fixes

* ClaimStrategy validation bypass type principle changed to principal ([#493](https://github.com/Finbuckle/Finbuckle.MultiTenant/issues/493)) ([fbfd022](https://github.com/Finbuckle/Finbuckle.MultiTenant/commit/fbfd0228c8b30a5f663fd2dfade0ae1b5bda09da))

## [6.5.0](https://github.com/Finbuckle/Finbuckle.MultiTenant/compare/v6.4.2...v6.5.0) (2021-11-08)


### Features

* add .NET 6 support ([#489](https://github.com/Finbuckle/Finbuckle.MultiTenant/issues/489)) ([a2d0416](https://github.com/Finbuckle/Finbuckle.MultiTenant/commit/a2d041670bf7efb198b06a864bad0a4cfc490a0c))

### [6.4.2](https://github.com/Finbuckle/Finbuckle.MultiTenant/compare/v6.4.1...v6.4.2) (2021-10-25)


### Bug Fixes

* change Options types from internal to public ([#483](https://github.com/Finbuckle/Finbuckle.MultiTenant/issues/483)) ([af9521d](https://github.com/Finbuckle/Finbuckle.MultiTenant/commit/af9521d993ce1c0369662c8db26d790c06c521f3))

### [6.4.1](https://github.com/Finbuckle/Finbuckle.MultiTenant/compare/v6.4.0...v6.4.1) (2021-10-11)


### Bug Fixes

* options not validating ([d4c6f30](https://github.com/Finbuckle/Finbuckle.MultiTenant/commit/d4c6f30d8d78b9e1c42a627f426a8ca867bc860f))

## [6.4.0](https://github.com/Finbuckle/Finbuckle.MultiTenant/compare/v6.3.1...v6.4.0) (2021-10-07)


### Bug Fixes

* ClaimStrategy bypass cookie principal validation ([#475](https://github.com/Finbuckle/Finbuckle.MultiTenant/issues/475)) ([cd38a7f](https://github.com/Finbuckle/Finbuckle.MultiTenant/commit/cd38a7f25f3eb4ccbf3fcc546cf93f2d2463df39))


### Features

* add optional parameter to specify the ClaimStrategy authentication scheme. ([#398](https://github.com/Finbuckle/Finbuckle.MultiTenant/issues/398)) thanks ! ([d74ae41](https://github.com/Finbuckle/Finbuckle.MultiTenant/commit/d74ae41a71b9df6a95a711ef3bad6d4ebc9f73f7))

### [6.3.1](https://github.com/Finbuckle/Finbuckle.MultiTenant/compare/v6.3.0...v6.3.1) (2021-09-30)


### Bug Fixes

* revert some platform targets to netstandard ([#469](https://github.com/Finbuckle/Finbuckle.MultiTenant/issues/469)) ([aceff1d](https://github.com/Finbuckle/Finbuckle.MultiTenant/commit/aceff1d73540b22ef64c6cec0fd50e43eff5387b))

## [6.3.0](https://github.com/Finbuckle/Finbuckle.MultiTenant/compare/v6.2.0...v6.3.0) (2021-09-06)

### Changes

- Removed support for .NET Core 2.1 which ended Microsoft support in August 2020.
- Retargeted specifically to .netcoreapp3.1 and .net5.0 across all packages.
- Added `AdjustKey`, `AdjustIndex`, `AdjustAlIndexes`, `AdjustUniqueIndexes` methods to be chained off `IsMultiTenant` in EFCore functionality. They add the implicit `TenantId` to the respective key/indexes.
- Reverted generic version of `IsMultiTenant` back to non-generic version for more flexibility.
- Improved tenant resolution logging functionality and performance. Thanks to ****!
- Fixed a bug with `InMemoryStore` implementation of `TryUpdate`. Thanks to ****!
- Fixed a bug where `ConfigurationStore` would throw an exception if there was no default section in the config.
- Fixed a bug where ASP.NET Core Identity security stamp validation would force user logout and raise exceptions. Thanks to **** for finding the root cause of this bug.
- Fixed a bug where `MultiTenantOptionsManager<TOptions>` was internal instead of public.
- Fixed problematic references in sample projects.
- Updated and improved documentation.
- Updated and improved tests.
- Added various project files for .NET Foundation on-boarding.

## [6.2.0](https://github.com/Finbuckle/Finbuckle.MultiTenant/compare/v6.1.0...v6.2.0) (2021-02-16)

### Changes

- Added a new events system. See PR #359 Thanks to ****!
- Some internal refactoring.
- Various documentation fixes.
- Added sourcelink to allow debugging into remote source code.
- Added a security policy.

## [6.1.0](https://github.com/Finbuckle/Finbuckle.MultiTenant/compare/v6.0.0...v6.1.0) (2020-11-10)

### Changes

- .NET 5.0 support.
- New `DistributedCacheStore` uses the ASP.NET Core distributed cache for tenant resolution.
- New `HeaderStrategy` uses HTTP headers for tenant resolution. Thanks to ****!
- Support for inheritance in multitenant Entity Framework Core entity. Thanks to ****!
- Fixed a conflict between ClaimStrategy and per-tenant authentication.
- Updated docs, samples, and unit tests.

## [6.0.0](https://github.com/Finbuckle/Finbuckle.MultiTenant/compare/v5.0.4...v6.0.0) (2020-09-08)

### Changes

- Customizable `TenantInfo`. Implement `ITenantInfo` as needed or use the basic `TenantInfo` implementation. Should work with most strategies and stores. This was a major overhaul to the library. See docs for more information.
- Changed NuGet structure: use `Finbuckle.MultiTenant.AspNetCore` for web apps and if needed add `Finbuckle.MultiTenant.EntityFrameworkCore`.
- `WithPerTenantAuthentication` - Adds support for common per-tenant authentication scenarios. See docs for full details.
- Multiple strategies and stores can be registered. They will run in the order registered and the first tenant returned by a strategy/store combination is used.
- New `ClaimStrategy` checks for a tenant claim to resolve the tenant.
- New `SessionStrategy` uses a session variable to resolve the tenant.
- Refactored `InMemoryStore`, removed deprecated configuration functionality.
- Improved Blazor support.
- Improved support for non ASP.NET Core use cases.
- Removed support for ASP.NET 3.0.
- Removed `FallbackStrategy`, `StaticStrategy` is a better alternative.
- Bug fixes, refactors, and tweaks.
- Improved unit tests.
- Updated and improved documentation.
- Updated sample. Removed some older ASP.NET Core 2.1 samples.

## [5.0.4](https://github.com/Finbuckle/Finbuckle.MultiTenant/compare/v5.0.3...v5.0.4) (2020-02-12)

### Changes

- Fixed a conflicting assembly and NuGet versions.
- Minor documentation fix.

## [5.0.3](https://github.com/Finbuckle/Finbuckle.MultiTenant/compare/v5.0.1...v5.0.3) (2020-01-21)

### Changes

- Fixed a bug where documented static methods were internal rather than public.
- Minor documentation fix.

## [5.0.1](https://github.com/Finbuckle/Finbuckle.MultiTenant/compare/v5.0.0...v5.0.1) (2020-01-15)

### Changes

- Updated for [.NET Core January 2020 Updates](https://devblogs.microsoft.com/dotnet/net-core-january-2020/) adding support for .NET Core 2.1.15, 3.0.2, and 3.1.1.
- Updated dependencies as recommended in security notices for [.NET Core January 2020 Updates](https://devblogs.microsoft.com/dotnet/net-core-january-2020/).
- *Finbuckle.MultiTenant.AspNetCore* targets `netcoreapp3.1`, `netcoreapp3.0`, and `netcoreapp2.1`.
- *Finbuckle.MultiTenant.Core* targets `netstandard2.1` and `netstandard2.0`.
- *Finbuckle.MultiTenant.EntityFrameworkCore* targets `netstandard2.1` and `netstandard2.0`.

## [5.0.0](https://github.com/Finbuckle/Finbuckle.MultiTenant/compare/v4.0.0...v5.0.0) (2020-01-12)

### Changes

- Added support for ASP.NET Core 3.1.
- Major refactor of how Entity Framework multitenant data isolation works. No longer need to derive from `MultiTenantDbContext` greatly improving flexibility. `IdentityMultiTenantDbContext` reworked under this new model and no longer requires or recommends use of multitenant support classes, e.g. `MultiTenantIdentityUser`. Attempted to minimize impact, but if using `IdentityMultiTenantDbContext` **this may be a breaking change!** Thanks ****!
- Simplified `EFCoreStore` to use `TenantInfo` directly. **This is a breaking change!**
- Fixed a bug with user id not being set correctly in legacy `IdentityMultiTenantDbContext`.
- Added `ConfigurationStore` to load tenant information from app configuration. The store is read-only in code, but changes in configuration (e.g. appsettings.json) are picked up at runtime. Updated most sample projects to use this store.
- Deprecated `InMemoryStore` functionality that reads from configuration.
- Added `HttpRemoteStore` which will make an http request to get a `TenantInfo` object. It can be extended with `DelegatingHandler`s (i.e. to add authentication headers). Added sample projects for this store. Thanks to ****!
- Fixed an exception with OpenIdConnect remote authentication if "state" is not returned from the identity provider. The new behavior will result in no tenant found for the request.
- Updated samples.
- Updated documentation.
- Updated unit tests to check against all valid project targets.
- Symbols package included for debugging.

## [4.0.0](https://github.com/Finbuckle/Finbuckle.MultiTenant/compare/v3.2.0...v4.0.0) (2019-09-24)

### Changes

- Added support for ASP.NET Core 3! Valid project targets are `netcoreapp3.0`, `netcoreapp2.0`, and `netcoreapp2.1`.
- Added a sample app for ASP.NET 3 highlighting the route strategy improvements due to the endpoint routing mechanism.
- Fixed a bug where route strategy could throw an exception when used with Razor Pages. Thanks -services!
- Support for configuring multiple multitenant strategies. Each will be tried in the order configured until a non-null tenant identifier is returned. The exception is the fallback strategy which always goes last.
- Refactored component assemblies for better dependency control. EFCore can be excluded by referencing `Finbuckle.MultiTenant.AspNetCore` instead of `Finbuckle.MultiTenant`.
- Updated documentation.
- Updated unit tests to check against all valid project targets.
- Symbols package included for debugging.

## [3.2.0](https://github.com/Finbuckle/Finbuckle.MultiTenant/compare/v3.1.0...v3.2.0) (2019-09-08)

### Changes

- Added support for any preexisting global query filters in `MultiTenantDbContext` and `MultiTenantIdentityDbContext`. Thanks !
- Exposed the inner stores and strategies as a property on the respective `StoreInfo` and `StrategyInfo` properties of `MultiTenantContext`. Previously you could only access the wrapper object for each. Thanks !
- Fixed certain methods on `MultiTenantOptionsCache` to be external as originally intended. Thanks !
- Fix a bug with `TryUpdateAsync` in the wrapper store. Thanks !
- Updated documentation and fixed typos. Thanks !

## [3.1.0](https://github.com/Finbuckle/Finbuckle.MultiTenant/compare/v3.0.1...v3.1.0) (2019-06-09)

### Changes

- Added a strategy wrapper that handles validation and logging for the active strategy. When implementing `IMultiTenantStrategy` basic validation and logging are automatically provided.
- Added the delegate strategy that accepts a lambda to return the tenant identifier. Configure by calling `WithDelegateStrategy(...)`.
- Added the fallback strategy that provides a tenant identifier if the normal strategy (or remote authentication, if applicable) fails to resolve a tenant. Configure by calling `WithFallbackStrategy(...)`.
- Added `TrySetTenantInfo` as an extension method to `HttpContext`. This will set the `TenantInfo` provided as the current tenant for the request and can optionally reset the service providers so that scoped services are regenerated under the new tenant.
- Updated and improved documentation and sample projects.
- Miscellaneous bug fixes, code improvement, and unit tests.
- Thanks to  for contributing to this release.

## [3.0.1](https://github.com/Finbuckle/Finbuckle.MultiTenant/compare/v3.0.0...v3.0.1) (2019-05-05)

### Changes

- Refactored the global query filter used in `MultiTenantDbContext` and `MultiTenantIdentityDbContext` (Thanks !) for better performance and code quality.
- Removed custom `IModelCacheKeyFactory` as it is no longer needed due to the global query filter changes.
- Updated documentation and samples.

## [3.0.0](https://github.com/Finbuckle/Finbuckle.MultiTenant/compare/v2.0.2...v3.0.0) (2019-04-01)

### Changes

- Allow resetting option cache per-tenant. This is a breaking change.
- Host strategy can match entire domain as a special case (prior it only matched a single host segment).
- Added a sample project demonstrating a common login page shared by all tenants.
- Overhauled documentation.
- Updated unit and integration tests.
- Switch to Apache 2.0 license.

## [2.0.2](https://github.com/Finbuckle/Finbuckle.MultiTenant/compare/v2.0.1...v2.0.2) (2019-02-05)

### Changes

- Fixed bug in Identity where `UserLogins` primary key was not adjusted for multitenant usage.
- Updated and Fixed the IdentityDataIsolation sample project.
- General code and test cleanup.

## [2.0.1](https://github.com/Finbuckle/Finbuckle.MultiTenant/compare/v2.0.0...v2.0.1) (2019-01-07)

### Changes

- Fixed bug where the `TenantInfo` constructor did not save the passed `Items` collection.
- Tested for compatibility with ASP.NET Core 2.2.
- Updated samples for ASP.NET Core 2.2.
- Cleaned up library dependencies to target ASP.NET Core 2.1 or greater.

## [2.0.0](https://github.com/Finbuckle/Finbuckle.MultiTenant/compare/v1.2.0...v2.0.0) (2018-12-03)

### Changes

- Changed `TenantContext` to `MultiTenantContext` which includes `TenantInfo`, `StrategyInfo`, and `StoreInfo` properties.
- Namespace changes (e.g. use of `Microsoft.Extensions.DependencyInjection` namespace for `Configure` and `ConfigureServices` methods).
- Additional and improved unit tests.
- Updated sample project dependencies.
- Various other internal improvements to code and bug fixes.
- `TryUpdate` method added to `IMultiTenantStore` interface.
- Added `EFCoreStore` which allows an Entity Framework Core database context as the tenant store.
- Added sample project demonstrating use of `EFCoreStore`.
- Custom can be configured with custom dependency injection lifetime (single, scoped, or transient) via `WithStore` method overloads.
- Custom stores automatically receive logging and error support via internal use of `MultiTenantStoreWrapper`.
- Use of async/await for strategy execution for improved performance.
- Custom strategies can be configured with custom dependency injection lifetime (single, scoped, or transient) via `WithStrategy` method overloads.
- Moved route configuration for RouteStrategy from `UseMultiTenant` to `WithRouteStrategy`.

## [1.2.0](https://github.com/Finbuckle/Finbuckle.MultiTenant/compare/v1.1.1...v1.2.0) (2018-07-22)

### Changes

- Added variants of `MultiTenantIdentityDbContext` which allows more flexible integration with Identity (Thanks !)
- Added sample project for data isolation with Identity
- Minor refactoring and more unit tests
- Various bug fixes

## [1.1.1](https://github.com/Finbuckle/Finbuckle.MultiTenant/compare/v1.1.0...v1.1.1) (2018-05-22)

### Changes

- Fixed bug affecting per-tenant data isolation using a shared database
- Added sample project for data isolation
- Added new constructors for `MultiTenantDbContext` and `MultiTenantIdentityDbContext`

## [1.1.0](https://github.com/Finbuckle/Finbuckle.MultiTenant/compare/v1.0.0...v1.1.0) (2018-04-22)

### Changes

- Remote authentication support
- Strategy improvements
- Store improvements
- Per-tenant options improvements
- Logging support
- Updated samples
- Improved unit and integration tests
- Switch to Apache 2.0 license

## [1.0.0](https://github.com/Finbuckle/Finbuckle.MultiTenant/tree/v1.0.0) (2018-01-01)

### Changes

- Initial release
<!--_history-->