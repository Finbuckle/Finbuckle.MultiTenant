// Copyright Finbuckle LLC, Andrew White, and Contributors.
// Refer to the solution LICENSE file for more inforation.

namespace Finbuckle.MultiTenant
{
    public interface ITenantInfo
    {
        string? Id { get; set; }
        string? Identifier { get; set;  }
        string? Name { get; set; }
        string? ConnectionString { get; set; }
    }
}