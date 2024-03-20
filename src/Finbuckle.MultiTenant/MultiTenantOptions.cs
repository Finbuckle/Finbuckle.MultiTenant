// Copyright Finbuckle LLC, Andrew White, and Contributors.
// Refer to the solution LICENSE file for more information.

// TODO move to options folder/namespace on future major release
namespace Finbuckle.MultiTenant;

public class MultiTenantOptions
{
    public Type? TenantInfoType { get; internal set; }
    public IList<string> IgnoredIdentifiers { get; set; } = new List<string>();
    public MultiTenantEvents Events { get; set; } = new ();
}