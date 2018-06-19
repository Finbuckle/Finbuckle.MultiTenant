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
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Metadata;

namespace Finbuckle.MultiTenant.EntityFrameworkCore
{
    /// <summary>
    /// A database context that enforces tenant integrity on entity types
    /// marked with the <c>MultiTenant</c> attribute.
    /// </summary>
    public class MultiTenantDbContext : DbContext
    {
        private readonly TenantContext tenantContext;
        private ImmutableList<IEntityType> multiTenantEntityTypes = null;

        protected string ConnectionString => tenantContext.ConnectionString;

        public MultiTenantDbContext(TenantContext tenantContext, DbContextOptions options) : base(options)
        {
            this.tenantContext = tenantContext;
        }

        public MultiTenantDbContext(string connectionString, DbContextOptions options) : base(options)
        {
            tenantContext = new TenantContext(null, null, null, connectionString, null, null);
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
                       Where(t => t.ClrType.GetCustomAttribute<MultiTenantAttribute>() != null).
                       ToImmutableList();
                }

                return multiTenantEntityTypes;
            }
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            Shared.SetupModel(modelBuilder, tenantContext);
        }

        public override int SaveChanges(bool acceptAllChangesOnSuccess)
        {
            if (ChangeTracker.AutoDetectChangesEnabled)
                ChangeTracker.DetectChanges();

            Shared.EnforceTenantId(tenantContext, ChangeTracker, TenantNotSetMode, TenantMismatchMode);

            var origAutoDetectChange = ChangeTracker.AutoDetectChangesEnabled;
            ChangeTracker.AutoDetectChangesEnabled = false;

            var result = base.SaveChanges(acceptAllChangesOnSuccess);

            ChangeTracker.AutoDetectChangesEnabled = origAutoDetectChange;

            return result;
        }

        public override async Task<int> SaveChangesAsync(bool acceptAllChangesOnSuccess,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            if (ChangeTracker.AutoDetectChangesEnabled)
                ChangeTracker.DetectChanges();

            Shared.EnforceTenantId(tenantContext, ChangeTracker, TenantNotSetMode, TenantMismatchMode);

            var origAutoDetectChange = ChangeTracker.AutoDetectChangesEnabled;
            ChangeTracker.AutoDetectChangesEnabled = false;

            var result = await base.SaveChangesAsync(acceptAllChangesOnSuccess, cancellationToken).ConfigureAwait(false);

            ChangeTracker.AutoDetectChangesEnabled = origAutoDetectChange;

            return result;
        }
    }
}