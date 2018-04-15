using System;

namespace Finbuckle.MultiTenant.EntityFrameworkCore
{
    /// <summary>
    /// Marks a class as multitenant when used with a database context
    /// derived from <c>MultiTenantDbContext</c> or <c>MultiTenantIdentityDbContext</c>.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, Inherited = false)]
    public class MultiTenantAttribute : Attribute
    {

    }
}