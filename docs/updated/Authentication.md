# Per-Tenant Authentication

Finbuckle.MultiTenant provides built-in support for isolating tenant
authentication. This means that the login session for a given request will only
be valid for the current tenant. Subsequent requests from the same client, but
for a different tenant (e.g. a different path when using the route strategy),
will not leak the previous login session into the tenant. This feature also
avoids the need to create separate authentication schemes for each tenant.

Common authentication options are supported per-tenant by default, but
additional authetication options can be configured per-tenant using
[per-tenant options](Options) as needed.

See the [Per-Tenant Authentication Sample](https://github.com/Finbuckle/Finbuckle.MultiTenant/tree/master/samples/ASP.NET%20Core%203/PerTenantAuthenticationSample)
for a demonstration of the features discussed in this topic.

The sections below assume Finbuckle.MultiTenant is installed and configured. See
[Getting Started](GettingStarted) for details.

## Using WithPerTenantAuthentication()

The `WithPerTenantAuthentication()` method can be called after
`AddMultiTenant<T>()` and uses conventions to configure common authentication
options based on public properties of the `ITenantInfo` type parameter.

The following happens when `WithPerTenantAuthentication()` is called:
- The default challenge scheme is set to the `ChallengeScheme` property
  of the `ITenantInfo` implementation.
- Cookie names are appended with the tenant id.
- 'Path' for cookie authentication is set to the `CookiePath` property
  of the `ITenantInfo` implementation.
- 'LoginPath' for cookie authentication is set to the `CookieLoginPath` property
  of the `ITenantInfo` implementation.
- 'LogoutPath' for cookie authentication is set to the `CookieLogoutPath`
  property of the `ITenantInfo` implementation.
- 'AccessDeniedPath' for cookie authentication is set to the
  `CookieAccessDeniedPath` property of the `ITenantInfo` implementation.
- Seveal internal services are registered to support remote authentication such
  as OAuth 2.0 and OpenID Connect.
- `Authority` for OpenID connect authentication is set to the
  `OpenIdConnectAuthority` property of the `ITenantInfo` implementation.
- `ClientId` for OpenID connect authentication is set to the
  `OpenIdConnectClientId` property of the `ITenantInfo` implementation.
- `ClientSecret` for OpenID connect authentication is set to the
  `OpenIdConnectClientSecret` property of the `ITenantInfo` implementation.

If the `ITenantInfo` implementation lacks one of the properties there is no
impact on the respective authentication property.

By changing the default challenge per-tenant, the user can be redirected to a
different scheme as needed. Combined with a per-tenant OpenID Connect authority,
this can route to shared or tenant specific authentication infrastructure.

The `CookiePath`, `CookieLoginPath`, `CookieLogoutPath`, and
`CookieAccessDeniedPath` properties can use a template format where `__tenant__`
will be replaced with the identifier for each specific tenant. For example, a
`CookieLoginPath` of "/\_\_tenant\_\_/Identity/Account/Login" will result in
"/initech/Identity/Account/Login" for the Initech tenant.

The code setup is straight-forward:

```cs
public void ConfigureServices(IServiceCollection services)
{
    services.AddControllersWithViews();
    
    services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
            .AddCookie()
            .AddOpenIdConnect();

    services.AddMultiTenant<AppTenantInfo>()
            .WithConfigurationStore()
            .WithRouteStrategy()
            .WithPerTenantAuthentication();
}

public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
{
    if (env.EnvironmentName == "Development")
    {
        app.UseDeveloperExceptionPage();
    }

    app.UseStaticFiles();
    app.UseRouting();
    app.UseMultiTenant();
    app.UseAuthentication();
    app.UseAuthorization();

    app.UseEndpoints(endpoints =>
    {
        endpoints.MapControllerRoute("default", "{__tenant__=}/{controller=Home}/{action=Index}");
    });
}
```

The code above paired with the `appSettings.json` tenant configuration below
produces the behavior described. Any tenant store configured similarly will also
work.

```json
"Finbuckle:MultiTenant:Stores:ConfigurationStore": {
    "Defaults": {
        "ConnectionString": "",
        "CookiePath": "/__tenant__",
        "CookieLoginPath": "/__tenant__/home/login",
        "CookieLogoutPath": "/__tenant__/home/logout"
    },
    "Tenants": [
        {
            "Id": "93f330717e5d4f039cd05da312d559cc",
            "Identifier": "megacorp",
            "Name": "MegaCorp",
            "ChallengeScheme": "Cookies"
        },
        {
            "Id": "505c5c97f4e2442394610c673ac91f61",
            "Identifier": "acme",
            "Name": "ACME",
            "ChallengeScheme": "OpenIdConnect",
            "OpenIdConnectAuthority": "https://finbuckle-acme.us.auth0.com",
            "OpenIdConnectClientId": "2lGONpJBwIqWuN2QDAmBbYGt0k0khwQB",
            "OpenIdConnectClientSecret": "HWxQfz6U8GvPCSsvfH5U3uv6CzAeQSt8qHrc19_qEvUQhdsaJX9Dp-t9W-5SAj0m"
        },
        {
            "Id": "4ee609d6da0342e682012232566cff0e",
            "Identifier": "initech",
            "Name": "Initech",
            "ChallengeScheme": "OpenIdConnect",
            "OpenIdConnectAuthority": "https://finbuckle-initech.us.auth0.com",
            "OpenIdConnectClientId": "nmPF6VABNmzTISvtYLPenf08ARveQifZ",
            "OpenIdConnectClientSecret": "WINWtT2WAhWYUOgGHsAPIUV-dAHs1X4qcU6Pv98HBrorlOB5OMKetnsR0Ov0LuVm"
        }
    ]
}
```

## Other Authentication Options

Internally `WithPerTenantAuthentication()` makes heavy use of
[per-tenant options](Options). For authentication options not covered by
`WithPerTenantAuthentication()`, per-tenant option can provide similar behavior.

For example, if we want to configure JWT tokens so that each tenant has a
different recognized authority for token validation we can add a field to the
`ITenantInfo` implementation and configure the option per-tenant:

```cs
services.AddMultiTenant<AppTenantInfo>()
        .WithConfigurationStore()
        .WithRouteStrategy()
        .WithPerTenantOptions<JwtBearerOptions>((o, tenantInfo) =>
        {
            // Assume tenants are configured with an authority string to use here.
            o.Authority = tenantInfo.JwtAuthority;
        });
```

The same approach can be used for cookie, OpenID Connect, or any other
authentication options type.