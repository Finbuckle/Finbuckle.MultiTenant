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
using System.Threading;
using System.Threading.Tasks;
using Finbuckle.MultiTenant.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;

namespace Finbuckle.MultiTenant
{
    public abstract class MultiTenantIdentityDbContext : MultiTenantIdentityDbContext<MultiTenantIdentityUser>
    {
        protected MultiTenantIdentityDbContext(TenantInfo tenantInfo) : base(tenantInfo)
        {
        }

        protected MultiTenantIdentityDbContext(TenantInfo tenantInfo, DbContextOptions options) : base(tenantInfo, options)
        {
        }
    }

    /// <summary>
    /// A database context compatible with Identity that enforces tenant integrity on entity types
    /// marked with the MultiTenant attribute.
    /// </summary>
    public abstract class MultiTenantIdentityDbContext<TUser> : MultiTenantIdentityDbContext<TUser, MultiTenantIdentityRole, string>
        where TUser : MultiTenantIdentityUser
    {
        protected MultiTenantIdentityDbContext(TenantInfo tenantInfo) : base(tenantInfo)
        {
        }

        protected MultiTenantIdentityDbContext(TenantInfo tenantInfo, DbContextOptions options) : base(tenantInfo, options)
        {
        }
    }

    public abstract class MultiTenantIdentityDbContext<TUser, TRole, TKey> : MultiTenantIdentityDbContext<TUser, TRole, TKey, MultiTenantIdentityUserClaim<TKey>, MultiTenantIdentityUserRole<TKey>, MultiTenantIdentityUserLogin<TKey>, MultiTenantIdentityRoleClaim<TKey>, MultiTenantIdentityUserToken<TKey>>
        where TUser : MultiTenantIdentityUser<TKey>
        where TRole : MultiTenantIdentityRole<TKey>
        where TKey : IEquatable<TKey>
    {
        protected MultiTenantIdentityDbContext(TenantInfo tenantInfo) : base(tenantInfo)
        {
        }

        protected MultiTenantIdentityDbContext(TenantInfo tenantInfo, DbContextOptions options) : base(tenantInfo, options)
        {
        }
    }

    public abstract class MultiTenantIdentityDbContext<TUser, TRole, TKey, TUserClaim, TUserRole, TUserLogin, TRoleClaim, TUserToken> : IdentityDbContext<TUser, TRole, TKey, TUserClaim, TUserRole, TUserLogin, TRoleClaim, TUserToken>
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

        private ImmutableList<IEntityType> multiTenantEntityTypes = null;

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
                if (multiTenantEntityTypes == null)
                {
                    multiTenantEntityTypes = Model.GetEntityTypes().
                       Where(t => Shared.HasMultiTenantAttribute(t.ClrType)).
                       ToImmutableList();
                }

                return multiTenantEntityTypes;
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

            Shared.SetupModel(builder, TenantInfo);

            // Adjust "unique" constraints on Username and Rolename.
            if (Shared.HasMultiTenantAttribute(typeof(TUser)))
            {
                var props = new List<IProperty>(new[] { builder.Entity<TUser>().Metadata.FindProperty("NormalizedUserName") });
                builder.Entity<TUser>().Metadata.RemoveIndex(props);

                builder.Entity<TUser>(b =>
                    b.HasIndex("NormalizedUserName", "TenantId").HasName("UserNameIndex").IsUnique());
            }

            if (Shared.HasMultiTenantAttribute(typeof(TRole)))
            {
                var props = new List<IProperty>(new[] { builder.Entity<TRole>().Metadata.FindProperty("NormalizedName") });
                builder.Entity<TRole>().Metadata.RemoveIndex(props);

                builder.Entity<TRole>(b =>
                    b.HasIndex("NormalizedName", "TenantId").HasName("RoleNameIndex").IsUnique());
            }

            // Adjust private key on UserLogin.
            if (Shared.HasMultiTenantAttribute(typeof(TUserLogin)))
            {
                var pk = builder.Entity<TUserLogin>().Metadata.FindPrimaryKey();
                builder.Entity<TUserLogin>().Metadata.RemoveKey(pk.Properties);

                // Create a new ID and a unique index to replace the old pk.
                builder.Entity<TUserLogin>(b => b.Property<string>("Id").ValueGeneratedOnAdd());
                builder.Entity<TUserLogin>(b => b.HasIndex("LoginProvider", "ProviderKey", "TenantId").IsUnique());
            }
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

            var result = await base.SaveChangesAsync(acceptAllChangesOnSuccess, cancellationToken).ConfigureAwait(false);

            ChangeTracker.AutoDetectChangesEnabled = origAutoDetectChange;

            return result;
        }
    }
}