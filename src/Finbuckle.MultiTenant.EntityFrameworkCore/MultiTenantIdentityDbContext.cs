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
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Finbuckle.MultiTenant.Core;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;

namespace Finbuckle.MultiTenant.EntityFrameworkCore
{
    public class MultiTenantIdentityDbContext : MultiTenantIdentityDbContext<MultiTenantIdentityUser>
    {
        protected MultiTenantIdentityDbContext()
        {
        }

        protected MultiTenantIdentityDbContext(TenantContext tenantContext, DbContextOptions options) : base(tenantContext, options)
        {
        }
    }

    /// <summary>
    /// A database context compatiable with Identity that enforces tenant integrity on entity types
    /// marked with the <c>MultiTenant</c> attribute.
    /// </summary>
    public class MultiTenantIdentityDbContext<TUser> : MultiTenantIdentityDbContext<TUser, MultiTenantIdentityRole, string>
        where TUser : MultiTenantIdentityUser
    {
        protected MultiTenantIdentityDbContext()
        {
        }

        protected MultiTenantIdentityDbContext(TenantContext tenantContext, DbContextOptions options) : base(tenantContext, options)
        {
        }
    }

    public class MultiTenantIdentityDbContext<TUser, TRole, TKey> : MultiTenantIdentityDbContext<TUser, TRole, TKey, MultiTenantIdentityUserClaim<TKey>, MultiTenantIdentityUserRole<TKey>, MultiTenantIdentityUserLogin<TKey>, MultiTenantIdentityRoleClaim<TKey>, MultiTenantIdentityUserToken<TKey>>
        where TUser : MultiTenantIdentityUser<TKey>
        where TRole : MultiTenantIdentityRole<TKey>
        where TKey : IEquatable<TKey>
    {
        protected MultiTenantIdentityDbContext()
        {
        }

        protected MultiTenantIdentityDbContext(TenantContext tenantContext, DbContextOptions options) : base(tenantContext, options)
        {
        }
    }

    public class MultiTenantIdentityDbContext<TUser, TRole, TKey, TUserClaim, TUserRole, TUserLogin, TRoleClaim, TUserToken> : IdentityDbContext<TUser, TRole, TKey, TUserClaim, TUserRole, TUserLogin, TRoleClaim, TUserToken>
        where TUser : IdentityUser<TKey>
        where TRole : IdentityRole<TKey>
        where TUserClaim : IdentityUserClaim<TKey>
        where TUserRole : IdentityUserRole<TKey>
        where TUserLogin : IdentityUserLogin<TKey>
        where TRoleClaim : IdentityRoleClaim<TKey>
        where TUserToken : IdentityUserToken<TKey>
        where TKey : IEquatable<TKey>
    {
        protected internal TenantContext TenantContext { get; protected set; }

        private ImmutableList<IEntityType> tenantScopeEntityTypes = null;

        protected string ConnectionString => TenantContext.ConnectionString;

        protected MultiTenantIdentityDbContext()
        {
        }

        protected MultiTenantIdentityDbContext(TenantContext tenantContext, DbContextOptions options) : base(options)
        {
            this.TenantContext = tenantContext;
        }

        public TenantMismatchMode TenantMismatchMode { get; set; } = TenantMismatchMode.Throw;

        public TenantNotSetMode TenantNotSetMode { get; set; } = TenantNotSetMode.Throw;

        public IImmutableList<IEntityType> MultiTenantEntityTypes
        {
            get
            {
                if (tenantScopeEntityTypes == null)
                {
                    tenantScopeEntityTypes = Model.GetEntityTypes().
                       Where(t => Shared.HasMultiTenantAttribute(t.ClrType)).
                       ToImmutableList();
                }

                return tenantScopeEntityTypes;
            }
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.ReplaceService<IModelCacheKeyFactory, MultiTenantModelCacheKeyFactory>();
            base.OnConfiguring(optionsBuilder);
        }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            Shared.SetupModel(builder, TenantContext);

            // Adjust "unique" constraints on Username and Rolename.
            // Consider a more general solution in the future.
            if(Shared.HasMultiTenantAttribute(typeof(TUser)))
            {
                var props = new List<IProperty>(new[] { builder.Entity<TUser>().Metadata.FindProperty("NormalizedUserName") });
                builder.Entity<TUser>().Metadata.RemoveIndex(props);
                
                builder.Entity<TUser>(b =>
                    b.HasIndex("NormalizedUserName", "TenantId").HasName("UserNameIndex").IsUnique());
            }

            if(Shared.HasMultiTenantAttribute(typeof(TRole)))
            {
                var props = new List<IProperty>(new[] { builder.Entity<TRole>().Metadata.FindProperty("NormalizedName") });
                builder.Entity<TRole>().Metadata.RemoveIndex(props);

                builder.Entity<TRole>(b =>
                    b.HasIndex("NormalizedName", "TenantId").HasName("RoleNameIndex").IsUnique());
            }            
        }

        public override int SaveChanges(bool acceptAllChangesOnSuccess)
        {
            // Emulate AutoDetectChanges so that EnforceTenantId has complete data to work with.
            if (ChangeTracker.AutoDetectChangesEnabled)
                ChangeTracker.DetectChanges();

            Shared.EnforceTenantId(TenantContext, ChangeTracker, TenantNotSetMode, TenantMismatchMode);

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

            Shared.EnforceTenantId(TenantContext, ChangeTracker, TenantNotSetMode, TenantMismatchMode);

            var origAutoDetectChange = ChangeTracker.AutoDetectChangesEnabled;
            ChangeTracker.AutoDetectChangesEnabled = false;

            var result = await base.SaveChangesAsync(acceptAllChangesOnSuccess, cancellationToken).ConfigureAwait(false);

            ChangeTracker.AutoDetectChangesEnabled = origAutoDetectChange;

            return result;
        }
    }
}