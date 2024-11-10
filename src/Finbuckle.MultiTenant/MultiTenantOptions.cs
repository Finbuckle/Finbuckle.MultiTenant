// Copyright Finbuckle LLC, Andrew White, and Contributors.
// Refer to the solution LICENSE file for more information.

using Finbuckle.MultiTenant.Abstractions;
using Finbuckle.MultiTenant.Events;

namespace Finbuckle.MultiTenant;

/// <summary>
/// Options for multitenant resolution.
/// </summary>
/// <typeparam name="TTenantInfo">The ITenantInfo implementation type.</typeparam>X
public class MultiTenantOptions<TTenantInfo> where TTenantInfo : class, ITenantInfo, new()
{
    public Type? TenantInfoType { get; internal set; }
    public IList<string> IgnoredIdentifiers { get; set; } = new List<string>();
    public MultiTenantEvents<TTenantInfo> Events { get; set; } = new ();
}