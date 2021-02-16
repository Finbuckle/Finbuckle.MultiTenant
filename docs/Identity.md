# Data Isolation with ASP.NET Core Identity

## Introduction

Finbuckle.MultiTenant has limited support for data isolation with ASP.NET Core Identity when Entity Framework Core is used as the backing store. It works similarly to [Data Isolation with Entity Framework Core](EFCore) except Identity calls into the database instead of your own code.

See the Identity data isolation sample projects in the [GitHub repository](https://github.com/Finbuckle/Finbuckle.MultiTenant/tree/master/samples) for examples on how to use Finbuckle.MultiTenant with ASP.NET Core Identity. These samples illustrates how to isolate the tenant Identity data and integrate the Identity UI to work with a route multitenant strategy.

## Configuration
Configuring an Identity db context to be multitenant is identical to that of a regular db context as described in [Data Isolation With Entity Framework Core](EFCore) with a few extra specifics to keep in mind.

The simplest approach is to derive a db context from `MultiTenantIdentityDbContext` (which itself derives from `IdentityDbContext`) and configure Identity to use the derived context.

When customizing the Identity data model, for example deriving a user entity type class from `IdentityUser`, to designate the customized entity type as multitenant either:
- Add the `[MultiTenant]` data attribute to the entity type class, or
- use the `IsMultiTenant` fluent api method in `OnModelCreating` **after** calling the base class `OnModelCreating` method (to ensure the Identity model exists).

If not deriving from `MultiTenantIdentityDbContext` make sure to implement `IMultiTenantDbContext` and call the appropriate extension methods as described in [Data Isolation with Entity Framework Core](EFCore). In this case it is required that base class `OnModelCreating` method is called **before** any multitenant extension methods.

## Caveats
Internally Finbuckle.MultiTenant's EFCore functionality relies on a global query filter. Calling the `Find` method on an `DBSet<T>` bypasses this filter thus any place Identity uses this method internally is not filtered by multitenant.

Due to this limitation the Identity method `UserManager<TUser>.FindByIdAsync` will bypass the filter and search across all tenants in the database. The `IdentityUser` class uses a GUID for the user id so there is negligible risk of data spillover, however a different implementation of `IdentityUser<TKey>` will need to ensure global uniqueness for the user id.

## Identity Options
Identity options can be configured for the `IdentityOptions` class as described in (Per-Tenant Options). Any option that internally relies on `UserManager<TUser>.FindByIdAsync` may be problematic as described above. If in doubt check the Identity source code to be sure.

The Identity option to require a unique email address per user will require email addresses be unique only within the current tenant, i.e. per-tenant options are not required for this.

## Authentication
ASP.NET Core Identity cookies for authentication. It uses a [slightly different method for configuring cookies](https://docs.microsoft.com/en-us/aspnet/core/security/authentication/identity-configuration), but under the hood standard ASP.NET Core authentication is used.

Finbuckle.MultiTenant can isolate Identity authentication per tenant so that user sessions are unique per tenant. See [per-tenant authentication](Authentication) for information on how to customize authentication options per tenant.

## Identity Model Customization with MultiTenantIdentityDbContext
The [ASP.NET Core Identity data model](https://docs.microsoft.com/en-us/aspnet/core/security/authentication/customize-identity-model?view=aspnetcore-2.2#the-identity-model) relies on several types which are passed to the database context as generic parameters: 
- `TUser`
- `TRole`
- `TKey`
- `TUserClaim`
- `TUserToken`
- `TUserLogin`
- `TRoleClaim`
- `TUserRole`

Default entity types exist such as the `IdentityUser`, `IdentityRole`, and `IdentityUserClaim`, which are commonly used as the generic parameters. The default for `TKey` is `string`. Apps can provide their own entity types for any of these by using alternative forms of the database context which take varying number of generic type parameters. Simple use-cases derive from `IdentityDbContext` types which require only a few generic parameters and plug in the default entity types for the rest.

Deriving an Identity database context from `MultiTenantIdentityDbContext` will use all of the default entity types and `string` for `TKey`. All entity types will be configured as multitenant.

Deriving from `MultiTenantIdentityDbContext<TUser>` will use the provided parameter for `TUser` and the defaults for the rest. `TUser` will not be configured as multitenant by default, and it is up to the programmer to do so as described above. All other entity types will be configured as multitenant.

Deriving from `MultiTenantIdentityDbContext<TUser, TRole, TKey>` will use the provided parameters for `<TUser>`, `TRole`, and `TKey` and the defaults for the rest. `TUser` and `TRole` will not be configured as multitenant by default, and it is up to the programmer to do so as described above if desired. All other entity types will be configured as multitenant.

Deriving from `MultiTenantIdentityDbContext<TUser, TRole, TKey, TUserClaim, TUserRole, TUserLogin, TRoleClaim, TUserToken>` will only use provided parameters. No entity types will be configured as multitenant, and it is up to the programmer to do so as described above if desired.

When providing non-default parameters it is recommended that the provided entity types have the `[MultiTenant]` attribute or call the `IsMultiTenant` builder extension method for each type in `OnModelCreating` **after** calling the base class `OnModelCreating`.
