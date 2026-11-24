# What's New in v<span class="_version">11.0.0</span>

> This page only lists release update details specific to v<span class="_version">11.0.0</span>. [Release update details for all releases are shown in the history page.](History)

<!--_release-notes-->
## [11.0.0](https://github.com/Finbuckle/Finbuckle.MultiTenant/compare/v10.2.0...v11.0.0) (2026-11-24)

### ⚠ BREAKING CHANGES

#### abstractions

* Replace IMultiTenantContext with ITenantContext and remove IMultiTenantContextAccessor, IMultiTenantContextSetter, and the accessor implementations. Consumers must use the ambient tenant context scope APIs. ([#1212](https://github.com/Finbuckle/Finbuckle.MultiTenant/issues/1212) by [@AndrewTriesToCode](https://github.com/AndrewTriesToCode))
* ITenantResolver.ResolveAsync returns tenant information directly and exposes the primary store and cache metadata through the updated resolver contract. ([#1212](https://github.com/Finbuckle/Finbuckle.MultiTenant/issues/1212) by [@AndrewTriesToCode](https://github.com/AndrewTriesToCode))

#### aspnetcore

* UseMultiTenant<TId>() throws a MultiTenantException at startup unless AddHttpAmbientTenantScope() was called, either directly or by an ASP.NET Core builder helper. Apps that configure only general-purpose methods such as WithStaticStrategy, WithDelegateStrategy or WithStrategy must call AddHttpAmbientTenantScope() themselves. The middleware also throws for a request that has no active ambient tenant scope. ([#1238](https://github.com/Finbuckle/Finbuckle.MultiTenant/issues/1238) by [@AndrewTriesToCode](https://github.com/AndrewTriesToCode))
* A tenant set earlier in the pipeline, for example with HttpContext.SetTenantInfo in middleware before UseMultiTenant, is now kept and tenant resolution is skipped. Previously the middleware began a new scope after resolution and discarded it. ([#1238](https://github.com/Finbuckle/Finbuckle.MultiTenant/issues/1238) by [@AndrewTriesToCode](https://github.com/AndrewTriesToCode))
* ASP.NET Core integrations no longer expose or consume the removed IMultiTenantContext accessor and setter abstractions; inject ITenantContext instead. ([#1212](https://github.com/Finbuckle/Finbuckle.MultiTenant/issues/1212) by [@AndrewTriesToCode](https://github.com/AndrewTriesToCode))

#### efcore

* Querying a multi-tenant entity type through a context without a tenant set now returns no rows. It previously threw an InvalidOperationException ("An exception was thrown while attempting to evaluate a LINQ query parameter expression") wrapping a NullReferenceException. ([#1239](https://github.com/Finbuckle/Finbuckle.MultiTenant/issues/1239) by [@AndrewTriesToCode](https://github.com/AndrewTriesToCode))
* `IsMultiTenant` on entity types was a method and is now a property. Its receiver is now non-nullable, so callers that pass a nullable entity type (for example the result of `FindEntityType`) get nullable warnings and must check for null first. ([#1239](https://github.com/Finbuckle/Finbuckle.MultiTenant/issues/1239) by [@AndrewTriesToCode](https://github.com/AndrewTriesToCode))
* Multi-tenant DbContext constructors no longer accept IMultiTenantContextAccessor; resolve ITenantContext through the ambient scope. ([#1212](https://github.com/Finbuckle/Finbuckle.MultiTenant/issues/1212) by [@AndrewTriesToCode](https://github.com/AndrewTriesToCode))

#### efcoreidentity

* Multi-tenant identity DbContext constructors no longer accept IMultiTenantContextAccessor; use ITenantContext from the ambient scope. ([#1212](https://github.com/Finbuckle/Finbuckle.MultiTenant/issues/1212) by [@AndrewTriesToCode](https://github.com/AndrewTriesToCode))

#### finbuckle

* ITenantScopeProvider has two new members, EndScope() and HasScope. Custom implementations of ITenantScopeProvider must implement them. ([#1238](https://github.com/Finbuckle/Finbuckle.MultiTenant/issues/1238) by [@AndrewTriesToCode](https://github.com/AndrewTriesToCode))
* `ConfigurationStore` creates tenants with `ConfigurationBinder.Get<TTenantInfo>()` from the tenant section layered over `Defaults`, instead of binding both sections onto an uninitialized object. A type with no parameterless constructor must have a single public constructor whose parameters match configuration keys (or use the new factory overload). Collection properties present in both `Defaults` and a tenant section are overlaid by index rather than appended (for example `["a","b"]` in `Defaults` and `["c"]` in a tenant gives `["c","b"]`); dictionary properties are merged by key. Because constructors and property initializers now run, plain properties keep their initializer values when not configured, and collection initializers are added to rather than replaced. ([#1232](https://github.com/Finbuckle/Finbuckle.MultiTenant/issues/1232) by [@AndrewTriesToCode](https://github.com/AndrewTriesToCode))
* Removes the public TypeExtensions.ImplementsOrInheritsUnboundGeneric method. ([#1212](https://github.com/Finbuckle/Finbuckle.MultiTenant/issues/1212) by [@AndrewTriesToCode](https://github.com/AndrewTriesToCode))
* Remove accessor and setter registrations in favor of ITenantContext and ITenantScopeProvider. ([#1212](https://github.com/Finbuckle/Finbuckle.MultiTenant/issues/1212) by [@AndrewTriesToCode](https://github.com/AndrewTriesToCode))
* IMultiTenantStore methods accept cancellation tokens; RemoveAsync removes by tenant ID and RemoveByIdentifierAsync removes by identifier. ([#1212](https://github.com/Finbuckle/Finbuckle.MultiTenant/issues/1212) by [@AndrewTriesToCode](https://github.com/AndrewTriesToCode))
* Replace ITenantResolver.Stores with one primary Store and StoreCaches, and remove DistributedCacheStore and MultiTenantStoreWrapper. ([#1212](https://github.com/Finbuckle/Finbuckle.MultiTenant/issues/1212) by [@AndrewTriesToCode](https://github.com/AndrewTriesToCode))
* Remove InMemoryStoreOptions and its configuring overload. Tenant identifiers are case-insensitive and initial tenants are added through TenantManager. ([#1212](https://github.com/Finbuckle/Finbuckle.MultiTenant/issues/1212) by [@AndrewTriesToCode](https://github.com/AndrewTriesToCode))

#### general

* Only the net11.0 framework is targeted and runtime async is enabled. ([#1237](https://github.com/Finbuckle/Finbuckle.MultiTenant/issues/1237) by [@AndrewTriesToCode](https://github.com/AndrewTriesToCode))
* `ITenantInfo` is now `ITenantInfo<TId>` where `TId` is any `Equatable<TId>`, e.g. `int` or `GUID`. Other types and methods taking a `TTenantType` generic type parameter now also take a `TId` generic type parameter. ([#1188](https://github.com/Finbuckle/Finbuckle.MultiTenant/issues/1188) by [@mvanput](https://github.com/mvanput))
* `default(TId)` is treated as tenant not-set similar to how previously `null` was considered not-set. ([#1188](https://github.com/Finbuckle/Finbuckle.MultiTenant/issues/1188) by [@mvanput](https://github.com/mvanput))


### Features

#### aspnetcore

* require a singleton ITenantScopeProvider at startup ([#1238](https://github.com/Finbuckle/Finbuckle.MultiTenant/issues/1238) by [@AndrewTriesToCode](https://github.com/AndrewTriesToCode)) ([512544b](https://github.com/Finbuckle/Finbuckle.MultiTenant/commit/512544b38a471a1bc6d619d450573042d50337db))

#### efcore

* validate the tenant id type when the model is built ([#1239](https://github.com/Finbuckle/Finbuckle.MultiTenant/issues/1239) by [@AndrewTriesToCode](https://github.com/AndrewTriesToCode)) ([713154a](https://github.com/Finbuckle/Finbuckle.MultiTenant/commit/713154af53ef55041c648bef672adeca95519aa4))

#### finbuckle

* require OSMF EULA acceptance for official packages ([#1241](https://github.com/Finbuckle/Finbuckle.MultiTenant/issues/1241) by [@AndrewTriesToCode](https://github.com/AndrewTriesToCode)) ([b90a1ff](https://github.com/Finbuckle/Finbuckle.MultiTenant/commit/b90a1ff61c9990e5d0ef07a78f7d6dae32a73bc5))
* attest NuGet packages and assemblies before publishing ([#1242](https://github.com/Finbuckle/Finbuckle.MultiTenant/issues/1242) by [@AndrewTriesToCode](https://github.com/AndrewTriesToCode)) ([e12e081](https://github.com/Finbuckle/Finbuckle.MultiTenant/commit/e12e0810cbbf3f0141754845bf8faf6ee9b31b45))
* add RunInTenantScopeAsync to run a delegate as a tenant ([#1238](https://github.com/Finbuckle/Finbuckle.MultiTenant/issues/1238) by [@AndrewTriesToCode](https://github.com/AndrewTriesToCode)) ([8c66dd9](https://github.com/Finbuckle/Finbuckle.MultiTenant/commit/8c66dd9a1c8664c9f94132fc68942291598e441b))


### Bug Fixes

#### aspnetcore

* begin the ambient tenant scope from a startup filter ([#1240](https://github.com/Finbuckle/Finbuckle.MultiTenant/issues/1240) by [@AndrewTriesToCode](https://github.com/AndrewTriesToCode)) ([638b09e](https://github.com/Finbuckle/Finbuckle.MultiTenant/commit/638b09e25ed0629b395b74a74603cf216a71889a))

#### finbuckle

* keep decorated services owned by the container ([#1238](https://github.com/Finbuckle/Finbuckle.MultiTenant/issues/1238) by [@AndrewTriesToCode](https://github.com/AndrewTriesToCode)) ([4d4e8b5](https://github.com/Finbuckle/Finbuckle.MultiTenant/commit/4d4e8b5074763a5f4eaed3a4596a8a483c151ce5))
* key in-memory store lookups by snapshot ids ([#1232](https://github.com/Finbuckle/Finbuckle.MultiTenant/issues/1232) by [@AndrewTriesToCode](https://github.com/AndrewTriesToCode)) ([4555318](https://github.com/Finbuckle/Finbuckle.MultiTenant/commit/4555318b6770ef407558c47f86e66d1b9f7cdf81))
* propagate cancellation through remote store requests ([#1212](https://github.com/Finbuckle/Finbuckle.MultiTenant/issues/1212) by [@AndrewTriesToCode](https://github.com/AndrewTriesToCode)) ([dfd1e67](https://github.com/Finbuckle/Finbuckle.MultiTenant/commit/dfd1e67190b3758719b3b98f0f21f47d2f75212a))



<!--_release-notes-->
