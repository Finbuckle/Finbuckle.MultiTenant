// Copyright Finbuckle LLC, Andrew White, and Contributors.
// Refer to the solution LICENSE file for more inforation.

using System.Collections.Generic;

// TODO move to options folder/namespace on future major release
namespace Finbuckle.MultiTenant
{
    public class MultiTenantOptions
    {
        public IList<string> IgnoredIdentifiers = new List<string>();
        public MultiTenantEvents Events { get; set; } = new MultiTenantEvents();
    }
}