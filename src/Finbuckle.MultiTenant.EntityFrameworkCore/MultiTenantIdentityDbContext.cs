//    Copyright 2018 Andrew White
// 
//    Licensed under the Apache License, Version 2.0 (the "License");
//    you may not use this file except in compliance with the License.
//    You may obtain a copy of the License at
// 
//        http://www.apache.org/licenses/LICENSE-2.0
// 
//    Unless required by applicable law or agreed to in writing, software
//    distributed under the License is distributed on an "AS IS" BASIS,
//    WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//    See the License for the specific language governing permissions and
//    limitations under the License.

using System;
using System.Collections.Generic; // Need for netstandard2.0 section.
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Finbuckle.MultiTenant.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Metadata;

namespace Finbuckle.MultiTenant
{
    public abstract class MultiTenantIdentityDbContext : MultiTenantIdentityDbContext<IdentityUser>
    {
        protected MultiTenantIdentityDbContext(TenantInfo tenantInfo) : base(tenantInfo)
        {
        }

        protected MultiTenantIdentityDbContext(TenantInfo tenantInfo, DbContextOptions options) : base(tenantInfo, options)
        {
        }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.Entity<IdentityUser>().IsMultiTenant();
        }
    }

    /// <summary>
    /// A database context compatible with Identity that enforces tenant integrity on entity types
    /// marked with the MultiTenant attribute.
    /// </summary>
    public abstract class MultiTenantIdentityDbContext<TUser> : MultiTenantIdentityDbContext<TUser, IdentityRole, string>
        where TUser : IdentityUser
    {
        protected MultiTenantIdentityDbContext(TenantInfo tenantInfo) : base(tenantInfo)
        {
        }

        protected MultiTenantIdentityDbContext(TenantInfo tenantInfo, DbContextOptions options) : base(tenantInfo, options)
        {
        }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.Entity<IdentityRole>().IsMultiTenant();
        }
    }

    public abstract class MultiTenantIdentityDbContext<TUser, TRole, TKey> : MultiTenantIdentityDbContext<TUser, TRole, TKey, IdentityUserClaim<TKey>, IdentityUserRole<TKey>, IdentityUserLogin<TKey>, IdentityRoleClaim<TKey>, IdentityUserToken<TKey>>
        where TUser : IdentityUser<TKey>
        where TRole : IdentityRole<TKey>
        where TKey : IEquatable<TKey>
    {
        protected MultiTenantIdentityDbContext(TenantInfo tenantInfo) : base(tenantInfo)
        {
        }

        protected MultiTenantIdentityDbContext(TenantInfo tenantInfo, DbContextOptions options) : base(tenantInfo, options)
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
        protected internal TenantInfo TenantInfo { get; protected set; }

        protected string ConnectionString => TenantInfo.ConnectionString;

        protected MultiTenantIdentityDbContext(TenantInfo tenantInfo)
        {
            this.TenantInfo = tenantInfo;
        }

        protected MultiTenantIdentityDbContext(TenantInfo tenantInfo, DbContextOptions options) : base(options)
        {
            this.TenantInfo = tenantInfo;
        }

        public TenantMismatchMode TenantMismatchMode { get; set; } = TenantMismatchMode.Throw;

        public TenantNotSetMode TenantNotSetMode { get; set; } = TenantNotSetMode.Throw;

        public IImmutableList<IEntityType> MultiTenantEntityTypes
        {
            get
            {
                return Model.GetEntityTypes().Where(et => Shared.HasMultiTenantAnnotation(et)).ToImmutableList();
            }
        }

        TenantInfo IMultiTenantDbContext.TenantInfo => TenantInfo;

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            Shared.SetupModel(builder);
        }

        public override int SaveChanges(bool acceptAllChangesOnSuccess)
        {
            // Emulate AutoDetectChanges so that EnforceTenantId has complete data to work with.
            if (ChangeTracker.AutoDetectChangesEnabled)
                ChangeTracker.DetectChanges();

            Shared.EnforceTenantId(TenantInfo, ChangeTracker, TenantNotSetMode, TenantMismatchMode);

            var origAutoDetectChange = ChangeTracker.AutoDetectChangesEnabled;
            ChangeTracker.AutoDetectChangesEnabled = false;

            var result = base.SaveChanges(acceptAllChangesOnSuccess);

            ChangeTracker.AutoDetectChangesEnabled = origAutoDetectChange;

            return result;
        }

        public override async Task<int> SaveChangesAsync(bool acceptAllChangesOnSuccess,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            // Emulate AutoDetectChanges so that EnforceTenantId has complete data to work with.
            if (ChangeTracker.AutoDetectChangesEnabled)
                ChangeTracker.DetectChanges();

            Shared.EnforceTenantId(TenantInfo, ChangeTracker, TenantNotSetMode, TenantMismatchMode);

            var origAutoDetectChange = ChangeTracker.AutoDetectChangesEnabled;
            ChangeTracker.AutoDetectChangesEnabled = false;

            var result = await base.SaveChangesAsync(acceptAllChangesOnSuccess, cancellationToken);

            ChangeTracker.AutoDetectChangesEnabled = origAutoDetectChange;

            return result;
        }
    }
}