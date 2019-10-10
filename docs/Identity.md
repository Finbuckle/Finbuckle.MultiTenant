# Data Isolation with ASP.NET Core Identity

## Introduction

Finbuckle.MultiTenant has limited support for data isolation with ASP.NET Core Identity when Entity Framework Core is used as the backing store. It works similarly to [normal Finbuckle.Multitenant Entity Framework Core data isolation](EFCore) except the database context derives from `MultiTenantIdentityDbContext<TUser>` instead of `MultiTenantDbContext`.

See the [IdentityDataIsolationSample](https://github.com/Finbuckle/Finbuckle.MultiTenant/tree/master/samples/IdentityDataIsolationSample) project for a comprehensive example on how to use Finbuckle.MultiTenant with ASP.NET Core Identity. This sample illustrates how to isolate the tenant Identity data and integrate the Identity UI to work with a route multitenant strategy.

## Configuration
Add the `Finbuckle.MultiTenant.EntityFrameworkCore` and package to the project:
```{.bash}
dotnet add package Finbuckle.MultiTenant.EntityFrameworkCore
```

Derive the database context from `MultiTenantIdentityDbContext<TUser>` instead of `IdentityDbContext<TUser>`. Make sure to forward the `TenantInfo` and `DbContextOptions<T>` into the base constructor:

```
public class MyIdentityDbContext : MultiTenantIdentityDbContext<appUser>
{
    public MyIdentityDbContext(TenantInfo tenantInfo, DbContextOptions<MyIdentityDbContext> options) :
        base(tenantInfo, options)
    { }
    ...
}
```

>{.small} `TUser` must derive from `IdentityUser` which uses a string for its primary key.

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

Default classes exist such as the `IdentityUser`, `IdentityRole`, and `IdentityUserClaim`, which are commonly used as the generic parameters. The default for `TKey` is `string`. Apps can provide their own classes for any of these by using alternative forms of the database context which take varying number of generic type parameters. Simple use-cases derive from `IdentityDbContext` classes which require only a few generic parameters and plug in the default classes for the rest.

Finbuckle.MultiTenant supports this approach by providing classes derived from each default model class with the `[MultiTenant]` attribute applied to them. These classes are:
- `MultiTenantIdentityUser`
- `MultiTenantIdentityRole`
- `MultiTenantIdentityUserClaim`
- `MultiTenantIdentityUserToken`
- `MultiTenantIdentityUserLogin`
- `MultiTenantIdentityRoleClaim`
- `MultiTenantIDentityUserRole`

Deriving an Identity database context from `MultiTenantIdentityDbContext` will use all of the default classes and `string` for `TKey`.

Deriving from `MultiTenantIdentityDbContext<TUser>` will use the provided parameter for `TUser` and the defaults for the rest.

Deriving from `MultiTenantIdentityDbContext<TUser, TRole, TKey>` will use the provided parameters for `<TUser>`, `TRole`, and `TKey` and the defaults for the rest.

Deriving from `MultiTenantIdentityDbContext<TUser, TRole, TKey, TUserClaim, TUserRole, TUserLogin, TRoleClaim, TUserToken>` will only use provided parameters.

When providing non-default parameters it is recommended that provided the classes have the `[MultiTenant]` attribute or derive from a type with the attribute.
