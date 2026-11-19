// Copyright Finbuckle LLC, Andrew White, and Contributors.
// Refer to the solution LICENSE file for more information.

namespace Finbuckle.MultiTenant.AspNetCore;

/// <summary>
/// Marker service registered by <c>AddHttpAmbientTenantScope</c> so registration is idempotent and the middleware
/// can verify at startup that a per-request ambient tenant scope is configured.
/// </summary>
internal sealed class HttpAmbientTenantScopeMarker
{
    internal const string NotConfiguredMessage =
        "No per-request ambient tenant scope is configured. Call AddHttpAmbientTenantScope() on the multi-tenant " +
        "builder, or use an ASP.NET Core helper such as WithRouteStrategy() that calls it for you.";

    internal const string NotSingletonMessage =
        "ITenantScopeProvider must be registered as a singleton that shares its ambient state with ITenantContext. " +
        "The registered ITenantScopeProvider resolves to different instances, so the scope begun for each request " +
        "would not be the one the middleware and ITenantContext use.";

    internal const string NoScopeMessage =
        "No ambient tenant scope is active for this request. AddHttpAmbientTenantScope() begins the scope at the " +
        "start of the request pipeline; ensure it is called on the multi-tenant builder and that the pipeline is " +
        "built by the ASP.NET Core host so its startup filter runs.";
}
