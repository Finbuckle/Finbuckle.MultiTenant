// Copyright Finbuckle LLC, Andrew White, and Contributors.
// Refer to the solution LICENSE file for more inforation.

using System;
using System.Threading;
using System.Threading.Tasks;
using Finbuckle.MultiTenant.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Finbuckle.MultiTenant
{
    /// <summary>
    /// An Identity database context that enforces tenant integrity on entity types
    /// marked with the MultiTenant annotation or attribute.
    /// <remarks>
    /// All Identity entity types are multitenant by default.
    /// </remarks>
    /// </summary>
    public abstract class MultiTenantIdentityDbContext : MultiTenantIdentityDbContext<IdentityUser>
    {
        protected MultiTenantIdentityDbContext(ITenantInfo tenantInfo) : base(tenantInfo)
        {
        }

        protected MultiTenantIdentityDbContext(ITenantInfo tenantInfo, DbContextOptions options) : base(tenantInfo, options)
        {
        }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.Entity<IdentityUser>().IsMultiTenant();
        }
    }

    /// <summary>
    /// An Identity database context that enforces tenant integrity on entity types
    /// marked with the MultiTenant annotation or attribute.
    /// <remarks>
    /// TUser is not multitenant by default.
    /// All other Identity entity types are multitenant by default.
    /// </remarks>
    /// </summary>
    public abstract class MultiTenantIdentityDbContext<TUser> : MultiTenantIdentityDbContext<TUser, IdentityRole, string>
        where TUser : IdentityUser
    {
        protected MultiTenantIdentityDbContext(ITenantInfo tenantInfo) : base(tenantInfo)
        {
        }

        protected MultiTenantIdentityDbContext(ITenantInfo tenantInfo, DbContextOptions options) : base(tenantInfo, options)
        {
        }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.Entity<IdentityRole>().IsMultiTenant();
        }
    }

    /// <summary>
    /// An Identity database context that enforces tenant integrity on entity types
    /// marked with the MultiTenant annotation or attribute.
    /// <remarks>
    /// TUser and TRole are not multitenant by default.
    /// All other Identity entity types are multitenant by default.
    /// </remarks>
    /// </summary>
    public abstract class MultiTenantIdentityDbContext<TUser, TRole, TKey> : MultiTenantIdentityDbContext<TUser, TRole, TKey, IdentityUserClaim<TKey>, IdentityUserRole<TKey>, IdentityUserLogin<TKey>, IdentityRoleClaim<TKey>, IdentityUserToken<TKey>>
        where TUser : IdentityUser<TKey>
        where TRole : IdentityRole<TKey>
        where TKey : IEquatable<TKey>
    {
        protected MultiTenantIdentityDbContext(ITenantInfo tenantInfo) : base(tenantInfo)
        {
        }

        protected MultiTenantIdentityDbContext(ITenantInfo tenantInfo, DbContextOptions options) : base(tenantInfo, options)
        {
        }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.Entity<IdentityUserClaim<TKey>>().IsMultiTenant();
            builder.Entity<IdentityUserRole<TKey>>().IsMultiTenant();
            builder.Entity<IdentityUserLogin<TKey>>().IsMultiTenant();
            builder.Entity<IdentityRoleClaim<TKey>>().IsMultiTenant();
            builder.Entity<IdentityUserToken<TKey>>().IsMultiTenant();
        }
    }

    /// <summary>
    /// An Identity database context that enforces tenant integrity on entity types
    /// marked with the MultiTenant annotation or attribute.
    /// <remarks>
    /// No Identity entity types are multitenant by default.
    /// </remarks>
    /// </summary>
    public abstract class MultiTenantIdentityDbContext<TUser, TRole, TKey, TUserClaim, TUserRole, TUserLogin, TRoleClaim, TUserToken> : IdentityDbContext<TUser, TRole, TKey, TUserClaim, TUserRole, TUserLogin, TRoleClaim, TUserToken>, IMultiTenantDbContext
        where TUser : IdentityUser<TKey>
        where TRole : IdentityRole<TKey>
        where TUserClaim : IdentityUserClaim<TKey>
        where TUserRole : IdentityUserRole<TKey>
        where TUserLogin : IdentityUserLogin<TKey>
        where TRoleClaim : IdentityRoleClaim<TKey>
        where TUserToken : IdentityUserToken<TKey>
        where TKey : IEquatable<TKey>
    {
        public ITenantInfo TenantInfo { get; }

        public TenantMismatchMode TenantMismatchMode { get; set; } = TenantMismatchMode.Throw;

        public TenantNotSetMode TenantNotSetMode { get; set; } = TenantNotSetMode.Throw;

        protected MultiTenantIdentityDbContext(ITenantInfo tenantInfo)
        {
            this.TenantInfo = tenantInfo;
        }

        protected MultiTenantIdentityDbContext(ITenantInfo tenantInfo, DbContextOptions options) : base(options)
        {
            this.TenantInfo = tenantInfo;
        }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);
            builder.ConfigureMultiTenant();
        }

        public override int SaveChanges(bool acceptAllChangesOnSuccess)
        {
            this.EnforceMultiTenant();
            return base.SaveChanges(acceptAllChangesOnSuccess);
        }

        public override async Task<int> SaveChangesAsync(bool acceptAllChangesOnSuccess,
                                                         CancellationToken cancellationToken = default(CancellationToken))
        {
            this.EnforceMultiTenant();
            return await base.SaveChangesAsync(acceptAllChangesOnSuccess, cancellationToken);
        }
    }
}