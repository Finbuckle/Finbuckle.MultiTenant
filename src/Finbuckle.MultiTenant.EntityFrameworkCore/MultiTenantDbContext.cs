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
using System.Collections.Immutable;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Finbuckle.MultiTenant.Core;
using Finbuckle.MultiTenant.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;

namespace Finbuckle.MultiTenant
{
    /// <summary>
    /// A database context that enforces tenant integrity on entity types
    /// marked with the MultiTenant attribute.
    /// </summary>
    public abstract class MultiTenantDbContext : DbContext
    {
        protected internal TenantInfo TenantInfo { get; protected set; }
        
        private ImmutableList<IEntityType> multiTenantEntityTypes = null;

        protected string ConnectionString => TenantInfo.ConnectionString;

        protected MultiTenantDbContext(TenantInfo tenantInfo)
        {
            this.TenantInfo = tenantInfo;
        }

        protected MultiTenantDbContext(TenantInfo tenantInfo, DbContextOptions options) : base(options)
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
        
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            Shared.SetupModel(modelBuilder, () => TenantInfo);
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