# What's New in v<span class="_version">8.0.0</span>

> This page only lists release update details specific to v<span class="_version">8.0.0</span>. [Release update details for all releases are shown in the history page.](History)

<!--_release-notes-->
# [8.0.0](https://github.com/Finbuckle/Finbuckle.MultiTenant/compare/v7.0.2...v8.0.0) (2024-10-12)

### Bug Fixes

* BasePathStrategy no longer breaks the strategy chain ([#884](https://github.com/Finbuckle/Finbuckle.MultiTenant/issues/884)) ([3263eff](https://github.com/Finbuckle/Finbuckle.MultiTenant/commit/3263effae4638656aab827b24094a8e575ae19a0))
* prevent duplicate key annotation in AdjustKey() ([#883](https://github.com/Finbuckle/Finbuckle.MultiTenant/issues/883)) ([f75ba2c](https://github.com/Finbuckle/Finbuckle.MultiTenant/commit/f75ba2c678079d2e956bf7d32b8e5aee0159e72b))
* removed unused parameter from WithPerTenantRemoteAuthenticationConvention ([#886](https://github.com/Finbuckle/Finbuckle.MultiTenant/issues/886)) ([dd17ab5](https://github.com/Finbuckle/Finbuckle.MultiTenant/commit/dd17ab51825ec94f4ecfe704f42c6b0457562d98))


### Features

* add GetAllAsync() support for HttpRemoteStore ([#848](https://github.com/Finbuckle/Finbuckle.MultiTenant/issues/848)) ([4208149](https://github.com/Finbuckle/Finbuckle.MultiTenant/commit/4208149282eaee99e2c02a788a2653faaa24ef7a))
* added the Echo Store. ([#807](https://github.com/Finbuckle/Finbuckle.MultiTenant/issues/807)) ([a3e5eee](https://github.com/Finbuckle/Finbuckle.MultiTenant/commit/a3e5eee64f0581c5f3d6abca7bb77cc56ef1d75c))
* strategies return null on invalid context type ([#885](https://github.com/Finbuckle/Finbuckle.MultiTenant/issues/885)) ([9834575](https://github.com/Finbuckle/Finbuckle.MultiTenant/commit/9834575c957fb5bd314cf0970e54a29384026d02))
* version policy update ([#888](https://github.com/Finbuckle/Finbuckle.MultiTenant/issues/888)) ([487a3a6](https://github.com/Finbuckle/Finbuckle.MultiTenant/commit/487a3a6d9782803dc2c7a4c70f46cdecf876e991))

### BREAKING CHANGES

* This commit brings the release into alignment with the new version policy. See #887 for details.
* Included strategies for ASP.NET Core would throw an exception if the passed context was not an  type. Now they will return null indicating no identifier was found.




<!--_release-notes-->
