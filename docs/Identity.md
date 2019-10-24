# Data Isolation with ASP.NET Core Identity

## Introduction

Finbuckle.MultiTenant has limited support for data isolation with ASP.NET Core Identity when Entity Framework Core is used as the backing store. It works similarly to [Data Isolation with Entity Framework Core](EFCore) except the database context derives from `MultiTenantIdentityDbContext<TUser>` instead of `MultiTenantDbContext`. The same functionality can also be added without deriving from `MultiTenantIdentityDbContext<TUser>` as described in [Data Isolation with Entity Framework Core](EFCore).

See the Identity data isolation sample projects in the [GitHub repository](https://github.com/Finbuckle/Finbuckle.MultiTenant/tree/master/samples) for examples on how to use Finbuckle.MultiTenant with ASP.NET Core Identity. These samples illustrates how to isolate the tenant Identity data and integrate the Identity UI to work with a route multitenant strategy.

## Configuration
Add the `Finbuckle.MultiTenant.` and package to the project:
```{.bash}
dotnet add package Finbuckle.MultiTenant
```

Derive the database context from `MultiTenantIdentityDbContext<TUser>` instead of `IdentityDbContext<TUser>`. Make sure to forward the `TenantInfo` and `DbContextOptions<T>` into the base constructor. Also, `TUser` must derive from `IdentityUser` which uses a string for its primary key.

```
public class MyIdentityDbContext : MultiTenantIdentityDbContext<appUser>
{
    public MyIdentityDbContext(TenantInfo tenantInfo, DbContextOptions<MyIdentityDbContext> options) :
        base(tenantInfo, options)
    { }
    ...
}
```

Add the `[MultiTenant]` attribute to the User entity classes:

```
[MultiTenant]
public class appUser : IdentityUser
{
    ...
}
```

ASP.NET Core Identity class methods on `UserManager<TUser>` or `UserStore<TUser>` that search for a specific user will be isolated to users of the current tenant, with the exception of `FindByIdAsync` which will search users of all tenants.

## Identity Options

Many identity options will be limited to the current tenant. For example, the option to require a unique email address per user will only require that an email be unique within the users for the current tenant. The exception is any option that internally relies on `UserManager<TUser>.FindByIdAsync`.

## Authentication
Internally, ASP.NET Core Identity uses regular ASP.NET Core authentication. It uses a [slightly different method for configuring cookies](https://docs.microsoft.com/en-us/aspnet/core/security/authentication/identity-configuration), but under the hood the end result is the same in that `CookieAuthenticationOptions` are being configured and consumed.

Finbuckle.Multitenant can customize these options per tenant so that user sessions are unique per tenant. See [per-tenant cookie authentication options](Authentication#cookie-authentication-options) for information on how to customize authentication options per tenant.

## Support for Identity Model Types
The [ASP.NET Core Identity data model](https://docs.microsoft.com/en-us/aspnet/core/security/authentication/customize-identity-model?view=aspnetcore-2.2#the-identity-model) relies on several types which are passed to the database context as generic parameters: 
- `TUser`
- `TRole`
- `TKey`
- `TUserClaim`
- `TUserToken`
- `TUserLogin`
- `TRoleClaim`
- `TUserRole`

Default entity types exist such as the `IdentityUser`, `IdentityRole`, and `IdentityUserClaim`, which are commonly used as the generic parameters. The default for `TKey` is `string`. Apps can provide their own entity types for any of these by using alternative forms of the database context which take varying number of generic type parameters. Simple use-cases derive from `IdentityDbContext` entity types which require only a few generic parameters and plug in the default entity types for the rest.


Deriving an Identity database context from `MultiTenantIdentityDbContext` will use all of the default entity types and `string` for `TKey`. All entity types will be configured as multitenant.

Deriving from `MultiTenantIdentityDbContext<TUser>` will use the provided parameter for `TUser` and the defaults for the rest. `TUser` will not be configured as multitenant by default. All other entity types will be configured as multitenant.

Deriving from `MultiTenantIdentityDbContext<TUser, TRole, TKey>` will use the provided parameters for `<TUser>`, `TRole`, and `TKey` and the defaults for the rest. `TUser` and `TRole` will not be configured as multitenant by default. All other entity types will be configured as multitenant.

Deriving from `MultiTenantIdentityDbContext<TUser, TRole, TKey, TUserClaim, TUserRole, TUserLogin, TRoleClaim, TUserToken>` will only use provided parameters. No entity types will be configured as multitenant.

When providing non-default parameters it is recommended that provided the entity types have the `[MultiTenant]` attribute or call the `IsMultiTenant` builder extension method for each type in `OnModelCreating`.
