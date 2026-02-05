# Data Isolation with ASP.NET Core Identity

## Introduction

MultiTenant has support for data isolation with ASP.NET Core Identity when Entity Framework Core is
used as the backing store. It works similarly to [Data Isolation with Entity Framework Core](EFCore) except Identity
calls into the database instead of your own code.

See the [multi-tenant Identity sample project](https://github.com/Finbuckle/Finbuckle.MultiTenant/tree/master/samples)
for and example of how to use MultiTenant with ASP.NET Core Identity. These samples illustrate how to isolate
tenant Identity data and integrate the Identity UI to work with a route multi-tenant strategy.

## Configuration

Configuring an Identity db context to be multi-tenant is identical to that of a regular db context as described
in [Data Isolation With Entity Framework Core](EFCore) with a few extra specifics to keep in mind.

The simplest approach is to derive a db context from `MultiTenantIdentityDbContext` (which itself derives
from `IdentityDbContext`) and configure Identity to use the derived context.

If for some reason you do not want an Identity entity to be multi-tenant you can override the behavior by
calling the `IsNotMultiTenant` extension method in `OnModelCreating` after calling the base class method.

If not deriving from `MultiTenantIdentityDbContext` make sure to implement `IMultiTenantDbContext` and call the
appropriate extension methods as described in [Data Isolation with Entity Framework Core](EFCore). In this case it is
required that base class `OnModelCreating` method is called **before** any multi-tenant extension methods.

## Unique Indexes

When using a variant of `MultiTenantIdentityDbContext` any entity designated as multi-tenant will also have the 
`TenantId` property added to its unique index.

> Note: Starting in v10, all Identity entity types are configured as multi-tenant by default when you derive from one of
> the `MultiTenantIdentityDbContext` variants. You generally do not need to add `[MultiTenant]` or call `IsMultiTenant`
> yourself unless you are explicitly overriding behavior on a specific type.

## Passkeys (WebAuthn) and Identity schema versions

ASP.NET Core Identity introduced passkey support via `IdentityUserPasskey<TKey>` in Identity schema version 3.
MultiTenant respects this and only configures the passkey entity as multi-tenant when the Identity
schema version is set to 3.

- Schema version 3: `IdentityUserPasskey<TKey>` (aka `TUserPasskey`) is configured as multi-tenant and its unique
  indexes include `TenantId`.
- Schema version 2: The passkey entity is not configured for multi-tenancy and its unique indexes will not include
  `TenantId`. Depending on your Identity configuration it may not even be part of the model.

To enable passkey support with multi-tenant configuration, set the Identity stores schema version to 3:

```csharp
services.AddOptions();
services.Configure<IdentityOptions>(o =>
{
    // Use Identity schema version 3 to enable passkeys
    o.Stores.SchemaVersion = new Version(3, 0);
});
```

No additional configuration is needed in your DbContext; `MultiTenantIdentityDbContext` will detect the schema version
and configure passkey entities accordingly.

## Caveats

Internally MultiTenant's EFCore functionality relies on a global query filter. Calling the `Find` method on
an `DBSet<T>` bypasses this filter thus any place Identity uses this method internally is not filtered by multi-tenant.

Due to this limitation the Identity method `UserManager<TUser>.FindByIdAsync` will bypass the filter and search across
all tenants in the database. The `IdentityUser` class uses a GUID for the user id so there is negligible risk of data
spillover, however a different implementation of `IdentityUser<TKey>` will need to ensure global uniqueness for the user
id.

## Identity Options

Identity options can be configured for the `IdentityOptions` class as described in [Per-Tenant Options](Options). Any option that
internally relies on `UserManager<TUser>.FindByIdAsync` may be problematic as described above. If in doubt check the
Identity source code to be sure.

The Identity option to require a unique email address per user will require email addresses be unique only within the
current tenant, i.e. per-tenant options are not required for this.

## Authentication

ASP.NET Core Identity uses cookies for authentication. It uses
a [slightly different method for configuring cookies](https://docs.microsoft.com/en-us/aspnet/core/security/authentication/identity-configuration),
but under the hood standard ASP.NET Core authentication is used.

MultiTenant can isolate Identity authentication per tenant so that user sessions are unique per tenant.
See [per-tenant authentication](Authentication) for information on how to customize authentication options per tenant.

## Identity Model Customization with MultiTenantIdentityDbContext

The [ASP.NET Core Identity data model](https://docs.microsoft.com/en-us/aspnet/core/security/authentication/customize-identity-model?view=aspnetcore-2.2#the-identity-model)
relies on several types which are passed to the database context as generic parameters:

- `TUser`
- `TRole`
- `TKey`
- `TUserClaim`
- `TUserToken`
- `TUserLogin`
- `TRoleClaim`
- `TUserRole`
- `TUserPasskey` (Identity schema version 3 only)

Default entity types exist such as the `IdentityUser`, `IdentityRole`, and `IdentityUserClaim`, which are commonly used
as the generic parameters. The default for `TKey` is `string`. Your app can provide its own entity types for any of these
by using alternative forms of the database context which take varying number of generic type parameters. Simple
use-cases derive from `IdentityDbContext` types which require only a few generic parameters and plug in the default
entity types for the rest.

> In v10, the `MultiTenantIdentityDbContext` variants will configure all provided Identity entity types as
> multi-tenant by default. The passkey entity (`TUserPasskey`) is configured only when the Identity schema version
> is set to 3.

Deriving an Identity database context from `MultiTenantIdentityDbContext` will use all the default entity types
and `string` for `TKey`. All entity types will be configured as multi-tenant.

Deriving from `MultiTenantIdentityDbContext<TUser>` will use the provided parameter for `TUser` and the defaults for the
rest. All entity types will be configured as multi-tenant.

Deriving from `MultiTenantIdentityDbContext<TUser, TRole, TKey>` will use the provided parameters
for `<TUser>`, `TRole`, and `TKey` and the defaults for the rest. All entity types will be configured as multi-tenant.

Deriving from
`MultiTenantIdentityDbContext<TUser, TRole, TKey, TUserClaim, TUserRole, TUserLogin, TRoleClaim, TUserToken, TUserPasskey>`
will use all provided parameters. All entity types will be configured as multi-tenant, and `TUserPasskey` is configured
only when the Identity schema version is set to 3.

When providing non-default parameters it is recommended that the provided entity types have the `[MultiTenant]`
attribute or call the `IsMultiTenant` builder extension method for each type in `OnModelCreating` **after** calling the
base class `OnModelCreating`.
