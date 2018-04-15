using System;
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
using Microsoft.EntityFrameworkCore.Metadata;

namespace Finbuckle.MultiTenant.EntityFrameworkCore
{
    /// <summary>
    /// A database context compatiable with Identity that enforces tenant integrity on entity types
    /// marked with the <c>MultiTenant</c> attribute.
    /// </summary>
    public class MultiTenantIdentityDbContext<TUser> : IdentityDbContext<TUser> where TUser : IdentityUser
    {
        private readonly TenantContext _tenantContext;
        private ImmutableList<IEntityType> _tenantScopeEntityTypes = null;

        protected string ConnectionString => _tenantContext.ConnectionString;

        public MultiTenantIdentityDbContext(TenantContext tenantContext, DbContextOptions options) : base(options)
        {
            _tenantContext = tenantContext;
        }

        public MultiTenantIdentityDbContext(string connectionString, DbContextOptions options) : base(options)
        {
            _tenantContext = new TenantContext(null, null, null, connectionString, null, null);
        }

        public TenantMismatchMode TenantMismatchMode { get; set; } = TenantMismatchMode.Throw;

        public TenantNotSetMode TenantNotSetMode { get; set; } = TenantNotSetMode.Throw;

        public IImmutableList<IEntityType> MultiTenantEntityTypes
        {
            get
            {
                if (_tenantScopeEntityTypes == null)
                {
                    _tenantScopeEntityTypes = Model.GetEntityTypes().
                       Where(t => t.ClrType.GetCustomAttribute<MultiTenantAttribute>() != null).
                       ToImmutableList();
                }

                return _tenantScopeEntityTypes;
            }
        }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            Shared.SetupModel(builder, _tenantContext);
        }

        public override int SaveChanges(bool acceptAllChangesOnSuccess)
        {
            if (ChangeTracker.AutoDetectChangesEnabled)
                ChangeTracker.DetectChanges();

            Shared.EnforceTenantId(_tenantContext, ChangeTracker, TenantNotSetMode, TenantMismatchMode);

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

            Shared.EnforceTenantId(_tenantContext, ChangeTracker, TenantNotSetMode, TenantMismatchMode);

            var origAutoDetectChange = ChangeTracker.AutoDetectChangesEnabled;
            ChangeTracker.AutoDetectChangesEnabled = false;

            var result = await base.SaveChangesAsync(acceptAllChangesOnSuccess, cancellationToken).ConfigureAwait(false);

            ChangeTracker.AutoDetectChangesEnabled = origAutoDetectChange;

            return result;
        }
    }
}