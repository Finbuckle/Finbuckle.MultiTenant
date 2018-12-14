### Introduction

>{.small} As of version 1.0.0 Finbuckle.MultiTenant only supports ASP.NET Core Identity when the user entity derives from `IdentityUser`, which uses a string type as the key.

>{.small} This document assumes a shared or hybrid database model for tenant data isolation.

Finbuckle.MultiTenant has limited support for data isolation with ASP.Net Core Identity. It works similarly to [normal Finbuckle.Multitenant Entity Framework Core data isolation](/Blog/14/tenant-data-isolation-entity-framework-core-finbuckle-multitenant) except the database context derives from `MultiTenantIdentityDbContext<TUser>` instead of `MultiTenantDbContext`.

### Configuration
Add the `Finbuckle.MultiTenant.EntityFrameworkCore` and package to the project:
```{.bash}
dotnet add package Finbuckle.MultiTenant.EntityFrameworkCore
```

Derive the database context from `MultiTenantIdentityDbContext<TUser>` instead of `IdentityDbContext<TUser>`. Make sure to forward the `TenantContext` and `DbContextOptions<T>` into the base constructor:

```
public class MyIdentityDbContext : MultiTenantIdentityDbContext<appUser>
{
    public MyIdentityDbContext(TenantContext tenantContext, DbContextOptions<MyIdentityDbContext> options) :
        base(tenantContext, options)
    { }
    ...
}
```

>{.small} `TUser` must derive from `IdentityUser` which uses a string for its primary key.

Add the `MultiTenant` data annotation to the User entity classes:

```
[MultiTenant]
public class appUser : IdentityUser
{
    ...
}
```

ASP.NET Core Identity class methods on `UserManager<TUser>` or `UserStore<TUser>` that search for a specific user will be isolated to users of the current tenant, with the exception of `FindByIdAsync` which will search users of all tenants.

### Identity Options

Many identity options will be limited to the current tenant. For example, the option to require a unique email address per user will only require that an email be unique within the users for the current tenant. The exception is any option that internally relies on `UserManager<TUser>.FindByIdAsync`.

### Authentication

Internally, ASP.NET Core Identity uses regular ASP.NET Core authentication. It uses a [slightly different method for configuring cookies](https://docs.microsoft.com/en-us/aspnet/core/security/authentication/identity-configuration), but under the hood the end result is the same in that `CookieAuthenticationOptions` are being configured and consumed.

Finbuckle.Multitenant can customize these options per tenant so that user sessions are unique per tenant. See [Tenant-Scoped Authentication in ASP.NET Core 2.0+ with Finbuckle.MultiTenant](/Blog/9/tenant-scoped-authentication-asp-net-core-2-0-finbuckle-multitenant) for information on how to customize authentication options per tenant.
