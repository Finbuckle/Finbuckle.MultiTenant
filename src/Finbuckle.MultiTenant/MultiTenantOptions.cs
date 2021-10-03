// Copyright Finbuckle LLC, Andrew White, and Contributors.
// Refer to the solution LICENSE file for more inforation.

using System.Collections.Generic;

namespace Finbuckle.MultiTenant
{
    public class MultiTenantOptions
    {
        public IList<string> IgnoredIdentifiers = new List<string>();
        public MultiTenantEvents Events { get; set; } = new MultiTenantEvents();
    }
}