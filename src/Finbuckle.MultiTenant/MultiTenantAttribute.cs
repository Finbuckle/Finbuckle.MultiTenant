// Copyright Finbuckle LLC, Andrew White, and Contributors.
// Refer to the solution LICENSE file for more inforation.

using System;

namespace Finbuckle.MultiTenant
{
    /// <summary>
    /// Marks a class as multitenant when used with a database context
    /// derived from MultiTenantDbContext or MultiTenantIdentityDbContext.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, Inherited = false)]
    public class MultiTenantAttribute : Attribute
    {

    }
}