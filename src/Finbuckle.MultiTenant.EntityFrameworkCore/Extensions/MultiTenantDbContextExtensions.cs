// Copyright Finbuckle LLC, Andrew White, and Contributors.
// Refer to the solution LICENSE file for more information.

using System.Linq;
using Finbuckle.MultiTenant.Abstractions;
using Finbuckle.MultiTenant.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

// ReSharper disable once CheckNamespace
namespace Finbuckle.MultiTenant;

public static class MultiTenantDbContextExtensions
{
    /// <summary>
    /// Checks the TenantId on entities taking into account
    /// TenantNotSetMode and TenantMismatchMode.
    /// </summary>
    public static void EnforceMultiTenant<TContext>(this TContext context) where TContext : DbContext, IMultiTenantDbContext
    {
        var changeTracker = context.ChangeTracker;
        ITenantInfo tenantInfo = context.TenantInfo!;
        var tenantMismatchMode = context.TenantMismatchMode;
        var tenantNotSetMode = context.TenantNotSetMode;

        var changedMultiTenantEntities = changeTracker.Entries().
            Where(e => e.State == EntityState.Added || e.State == EntityState.Modified || e.State == EntityState.Deleted).
            Where(e => e.Metadata.IsMultiTenant()).ToList();

        // ensure tenant context is valid
        if (changedMultiTenantEntities.Any())
        {
            if (tenantInfo == null)
                throw new MultiTenantException("MultiTenant Entity cannot be changed if TenantInfo is null.");
        }

        // get list of all added entities with MultiTenant annotation
        var addedMultiTenantEntities = changedMultiTenantEntities.
            Where(e => e.State == EntityState.Added).ToList();

        // handle Tenant ID mismatches for added entities
        var mismatchedAdded = addedMultiTenantEntities.
            Where(e => (string?)e.Property("TenantId").CurrentValue != null &&
                       (string?)e.Property("TenantId").CurrentValue != tenantInfo.Id).ToList();

        if (mismatchedAdded.Any())
        {
            switch (tenantMismatchMode)
            {
                case TenantMismatchMode.Throw:
                    throw new MultiTenantException($"{mismatchedAdded.Count} added entities with Tenant Id mismatch.");

                case TenantMismatchMode.Ignore:
                    // no action needed
                    break;

                case TenantMismatchMode.Overwrite:
                    foreach (var e in mismatchedAdded)
                    {
                        e.Property("TenantId").CurrentValue = tenantInfo.Id;
                    }
                    break;
            }
        }

        // for added entities TenantNotSetMode is always Overwrite
        var notSetAdded = addedMultiTenantEntities.
            Where(e => (string?)e.Property("TenantId").CurrentValue == null);

        foreach (var e in notSetAdded)
        {
            e.Property("TenantId").CurrentValue = tenantInfo.Id;
        }

        // get list of all modified entities with MultiTenant annotation
        var modifiedMultiTenantEntities = changedMultiTenantEntities.
            Where(e => e.State == EntityState.Modified).ToList();

        // handle Tenant ID mismatches for modified entities
        var mismatchedModified = modifiedMultiTenantEntities.
            Where(e => (string?)e.Property("TenantId").CurrentValue != null &&
                       (string?)e.Property("TenantId").CurrentValue != tenantInfo.Id).ToList();

        if (mismatchedModified.Any())
        {
            switch (tenantMismatchMode)
            {
                case TenantMismatchMode.Throw:
                    throw new MultiTenantException($"{mismatchedModified.Count} modified entities with Tenant Id mismatch.");

                case TenantMismatchMode.Ignore:
                    // no action needed
                    break;

                case TenantMismatchMode.Overwrite:
                    foreach (var e in mismatchedModified)
                    {
                        e.Property("TenantId").CurrentValue = tenantInfo.Id;
                    }
                    break;
            }
        }

        // handle Tenant ID not set for modified entities
        var notSetModified = modifiedMultiTenantEntities.
            Where(e => (string?)e.Property("TenantId").CurrentValue == null).ToList();

        if (notSetModified.Any())
        {
            switch (tenantNotSetMode)
            {
                case TenantNotSetMode.Throw:
                    throw new MultiTenantException($"{notSetModified.Count} modified entities with Tenant Id not set.");

                case TenantNotSetMode.Overwrite:
                    foreach (var e in notSetModified)
                    {
                        e.Property("TenantId").CurrentValue = tenantInfo.Id;
                    }
                    break;
            }
        }

        // get list of all deleted  entities with MultiTenant annotation
        var deletedMultiTenantEntities = changedMultiTenantEntities.
            Where(e => e.State == EntityState.Deleted).ToList();

        // handle Tenant ID mismatches for deleted entities
        var mismatchedDeleted = deletedMultiTenantEntities.
            Where(e => (string?)e.Property("TenantId").CurrentValue != null &&
                       (string?)e.Property("TenantId").CurrentValue != tenantInfo.Id).ToList();

        if (mismatchedDeleted.Any())
        {
            switch (tenantMismatchMode)
            {
                case TenantMismatchMode.Throw:
                    throw new MultiTenantException($"{mismatchedDeleted.Count} deleted entities with Tenant Id mismatch.");

                case TenantMismatchMode.Ignore:
                    // no action needed
                    break;

                case TenantMismatchMode.Overwrite:
                    // no action needed
                    break;
            }
        }

        // handle Tenant Id not set for deleted entities
        var notSetDeleted = deletedMultiTenantEntities.
            Where(e => (string?)e.Property("TenantId").CurrentValue == null).ToList();

        if (notSetDeleted.Any())
        {
            switch (tenantNotSetMode)
            {
                case TenantNotSetMode.Throw:
                    throw new MultiTenantException($"{notSetDeleted.Count} deleted entities with Tenant Id not set.");

                case TenantNotSetMode.Overwrite:
                    // no action needed
                    break;
            }
        }
    }
}