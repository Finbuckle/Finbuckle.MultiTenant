// Copyright Finbuckle LLC, Andrew White, and Contributors.
// Refer to the solution LICENSE file for more inforation.

using System;
using System.Threading.Tasks;

namespace Finbuckle.MultiTenant
{
    public class MultiTenantEvents
    {
        public Func<TenantResolvedContext, Task> OnTenantResolved { get; set; } = context => Task.CompletedTask;
        public Func<TenantNotResolvedContext, Task> OnTenantNotResolved { get; set; } = context => Task.CompletedTask;
    }
}