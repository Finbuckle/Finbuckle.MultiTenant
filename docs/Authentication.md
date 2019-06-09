# Per-Tenant Authentication

Finbuckle.MultiTenant's support for [per-tenant options](Options) is enhanced specifically to let apps customize ASP.NET Core 2.1+ authentication options. For example, `CookieAuthenticationOptions` or `OpenIdConnectOptions` can be configured separately per tenant to provide unique cookie names or OpenID Connect providers. This prevents sessions under a tenant from spilling over to another tenant if the same user-agent is used to access both. This design also prevents the need for a separate authentication schemes for every tenant.

This functionality works by giving each tenant an opportunity to customize options as they are first created, and caching the resulting options for each tenant. The options are only created once an app initiates an authentication related activity such as when `UseAuthentication` is called in the app pipeline.

Additionally, some types of authentication such as OpenID Connect and OAuth-based social providers require that the `WithRemoteAuthentication` method is called after `AddMultiTenant` during app configuration.

The sections below assume Finbuckle.MultiTenant is installed and configured. See [Getting Started](GettingStarted) for details.

## General Authentication Options

General authentication options such as `DefaultScheme` and `DefaultChallenge` schemes can be configured per-tenant. This can be useful if some tenants prefer local sign-in and other prefer to use OpenID Connect. See the [AuthenticationOptionsSamples](https://github.com/Finbuckle/Finbuckle.MultiTenant/tree/master/samples/AuthenticationOptionsSample) for a complete example.

```cs
services.AddMultiTenant().
    WithStore(...).
    WithStrategy(...).
    WithRemoteAuthentication().
    WithPerTenantOptions<AuthenticationOptions>((options, tenantInfo) =>
    {
        // Allow each tenant to have a different default challenge scheme.
        // Here the scheme is assumed to be configured in the TenantInfo's 
        // Items property.
        if (tenantInfo.Items.TryGetValue("ChallengeScheme", out object challengeScheme))
        {
            options.DefaultChallengeScheme = (string)challengeScheme;
        }
    })...
```

## Cookie Authentication Options

See the [AuthenticationOptionsSamples](https://github.com/Finbuckle/Finbuckle.MultiTenant/tree/master/samples/AuthenticationOptionsSample) in the Finbuckle.MultiTenant GitHub repository for a complete demonstration of per-tenant cookie options.

In the `Startup` class, [configure cookie authentication as usual](https://docs.microsoft.com/en-us/aspnet/core/security/authentication/cookie) for ASP.NET Core 2.1+:

```cs
public class Startup
{
    public void ConfigureServices(IServiceCollection services)
    {
        services.AddMultiTenant()...
        ...
        services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme).
            AddCookie(options =>
            {
                options.Cookie.Name = "MyAppCookie.";
            });
    }
}
 ```

Call `WithPerTenantOptions<CookieAuthenticationOptions>` after `AddMultiTenant` in the `ConfigureServices` method. The generic type parameter `CookieAuthenticationOptions` is the options type we are customizing. The method parameter is an `Action<TOptions, CookieAuthenticationOptions>` which will modify the options instance using information from the `TenantInfo`. In this case we are appending the tenant's ID to the cookie name:

```cs
services.AddMultiTenant()...
    .WithPerTenantOptions<CookieAuthenticationOptions>((o, tenantInfo) =>
    {
        o.Cookie.Name += tenantInfo.Id;
    });
```

This will cause the ASP.NET Core 2.1+ authentication middleware to only process the cookie for the specific tenant. All of the other cookie options are set normally, but each tenant's version of the options will be customized accordingly.

Depending on the [multitenant strategy](Strategies) being used, the following properties should be especially considered for per-tenant customization:

* `AccessDeniedPath`
* `Cookie.Domain`
* `Cookie.Name`
* `Cookie.Path`
* `DataProtectionProvider`
* `LoginPath` 
* `LogoutPath`

See the [CookieAuthenticationOptions documentation](https://docs.microsoft.com/en-us/dotnet/api/microsoft.aspnetcore.authentication.cookies.cookieauthenticationoptions) for details on the options that can be customized.

These customizations apply to all authentication schemes using `AuthenticationCookieOptions`, including Identity's "application" and "external" cookies.

## JWT Bearer Authentication Options

In the `Startup` class, configure JWT Bearer authentication as usual. The ASP.NET Core documentation does not cover this, but the official code repository contains a [sample project](https://github.com/aspnet/Security/tree/master/samples/JwtBearerSample).

 Call `WithPerTenantOptions<JwtBearerOptions>` after `AddMultiTenant` in the `ConfigureServices` method. The generic type parameter `JwtBearerOptions` is the options type we are customizing. The method parameter is an `Action<TOptions, JwtBearerOptions>` which will modify the options instance using information from the `TenantInfo`. In this case we are setting the authority from the `Items` collection in the `TenantInfo`:

```cs
services.AddMultiTenant()...
    .WithPerTenantOptions<JwtBearerOptions>((o, tenantInfo) =>
    {
        // Assume tenants are configured with an authority string to use here.
        o.Authority = (string)tenantInfo.Items["JwtAuthority"];
    });
```
The following properties should be especially considered for per-tenant customization:

* `Authority`
* `Audience`

See the [JwtBearerOptions documentation](https://docs.microsoft.com/en-us/dotnet/api/microsoft.aspnetcore.authentication.jwtbearer.jwtbeareroptions) for details on the options that can be customized.

These customizations apply to all authentication schemes using `JwtBearerOptions`.

## OpenID Connect Authentication Options

See the [AuthenticationOptionsSamples](https://github.com/Finbuckle/Finbuckle.MultiTenant/tree/master/samples/AuthenticationOptionsSample) in the Finbuckle.MultiTenant GitHub repository for a complete demonstration of per-tenant OpenID Connect options.

In the `Startup` class, configure OpenID Connect authentication as usual. The ASP.NET Core documentation does not cover this, but the official code repository contains a [sample project](https://github.com/aspnet/Security/tree/master/samples/OpenIdConnectSample).

 Call `WithRemoteAuthentication` and `WithPerTenantOptions<OpenIdConnectOptions>` after `AddMultiTenant` in the `ConfigureServices` method. The generic type parameter `OpenIdConnectOptions` is the options type we are customizing. The method parameter is an `Action<TOptions, OpenIdConnectOptions>` which will modify the options instance using information from the `TenantInfo`. In this case we are setting the authority from the `Items` collection in the `TenantInfo`:

```cs
services.AddMultiTenant()...
    .WithRemoteAuthentication() // Important!
    .WithPerTenantOptions<OpenIdConnectOptions>((o, tenantInfo) =>
    {
        // Assume tenants are configured with a client Id string to use here.
        o.ClientId = (string)tenantInfo.Items["ClientId"];

        // Assume tenants are configured with an authority string to use here.
        o.Authority = (string)tenantInfo.Items["Authority"];
    });
```
The following properties should be especially considered for per-tenant customization:

* `ClientId`
* `Authority`

**Do not** modify the `CallbackPath` property per-tenant&mdash;Finbuckle.MultiTenant handles this automatically via the `WithRemoteAuthentication` configuration method.

See the [OpenIdConnectOptions documentation](https://docs.microsoft.com/en-us/dotnet/api/microsoft.aspnetcore.authentication.openidconnect.openidconnectoptions) for details on the options that can be customized.

These customizations apply to all authentication schemes using `OpenIdConnectOptions`.

## Other Authentication Methods

See the [AuthenticationOptionsSamples](https://github.com/Finbuckle/Finbuckle.MultiTenant/tree/master/samples/AuthenticationOptionsSample) in the Finbuckle.MultiTenant GitHub repository for a complete demonstration of per-tenant Facebook authentication options.

Finbuckle.MultiTenant per-tenant options work with most of the [built-in ASP.NET Core 2.1+ authentication methods](https://docs.microsoft.com/en-us/aspnet/core/security/authentication/social/), with the exception of Twitter which is not derived from the internal OAuth-based authentication handler. Social login and other external providers require that `WithRemoteAuthentication` be called after `AddMultiTenant` in the `ConfigureServices` method. Each authentication method has its own options class, but the general approach is the same.