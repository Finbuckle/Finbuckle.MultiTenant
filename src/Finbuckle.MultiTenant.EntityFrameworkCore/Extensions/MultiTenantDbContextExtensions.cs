// Copyright Finbuckle LLC, Andrew White, and Contributors.
// Refer to the solution LICENSE file for more information.

using Finbuckle.MultiTenant.Abstractions;
using Microsoft.EntityFrameworkCore;

namespace Finbuckle.MultiTenant.EntityFrameworkCore.Extensions;

/// <summary>
/// Extension methods for multi-tenant <see cref="DbContext"/> instances.
/// </summary>
public static class MultiTenantDbContextExtensions
{
    /// <summary>
    /// Ensures a TenantId property is set when an entity is attached.
    /// </summary>
    /// <typeparam name="TContext">The <see cref="DbContext"/> type.</typeparam>
    /// <param name="context">The <see cref="DbContext"/> instance.</param>
    public static void EnforceMultiTenantOnTracking<TContext>(this TContext context)
        where TContext : DbContext, IMultiTenantDbContext
    {
        // Configure event to handle newly tracked entities.
        context.ChangeTracker.Tracking += (sender, args) =>
        {
            // Honor TenantNotSetMode on tracking from attach multi-tenant entities.
            if (!args.Entry.Metadata.IsMultiTenant() || args.FromQuery ||
                args.Entry.Context is not IMultiTenantDbContext multiTenantDbContext) return;

            if (multiTenantDbContext.TenantInfo is null)
                throw new MultiTenantException("MultiTenant Entity cannot be attached if TenantInfo is null.");

            #region Fork Sirfull : Permet de gérer '*' pour TenantId
            // Ancien code : args.Entry.Property("TenantId").CurrentValue ??= multiTenantDbContext.TenantInfo.Id;
            // Si pas de Tenant alors TenantId = '*' pour les entités globales (non multi-tenant)
            args.Entry.Property("TenantId").CurrentValue ??= (multiTenantDbContext.TenantInfo?.Id != null ? multiTenantDbContext.TenantInfo.Id : '*');
            #endregion
        };
    }

    /// <summary>
    /// Checks the TenantId on entities during SaveChanges and SaveChangesAsync taking into account <see cref="TenantNotSetMode"/> and <see cref="TenantMismatchMode"/>.
    /// </summary>
    /// <typeparam name="TContext">The <see cref="DbContext"/> type.</typeparam>
    /// <param name="context">The <see cref="DbContext"/> instance.</param>
    public static void EnforceMultiTenant<TContext>(this TContext context)
        where TContext : DbContext, IMultiTenantDbContext
    {
        var changeTracker = context.ChangeTracker;
        var tenantInfo = context.TenantInfo;
        var tenantMismatchMode = context.TenantMismatchMode;
        var tenantNotSetMode = context.TenantNotSetMode;

        #region Fork Sirfull : Permet de gérer '*' pour TenantId
        //var changedMultiTenantEntities = changeTracker.Entries()
        //    .Where(e => e.State is EntityState.Added or EntityState.Modified or EntityState.Deleted)
        //    .Where(e => e.Metadata.IsMultiTenant()).ToList();

        // On ne vérifie pas les entités avec TenantId = '*' car elles sont considérées comme des entités globales (non multi-tenant)
        var changedMultiTenantEntities = changeTracker.Entries()
            .Where(e => e.State is EntityState.Added or EntityState.Modified or EntityState.Deleted)
            .Where(e => e.Metadata.IsMultiTenant())
            .Where(e => (string?)e.Property("TenantId").CurrentValue != "*")
            .ToList();
        #endregion

        // ensure tenant context is valid
        if (changedMultiTenantEntities.Count == 0)
            return;

        if (tenantInfo is null)
            throw new MultiTenantException("MultiTenant Entity cannot be changed if TenantInfo is null.");

        // get list of all added entities with MultiTenant annotation
        var addedMultiTenantEntities = changedMultiTenantEntities.Where(e => e.State == EntityState.Added).ToList();

        // handle Tenant ID mismatches for added entities
        var mismatchedAdded = addedMultiTenantEntities.Where(e =>
            (string?)e.Property("TenantId").CurrentValue != null &&
            (string?)e.Property("TenantId").CurrentValue != tenantInfo.Id).ToList();

        if (mismatchedAdded.Count != 0)
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
        var notSetAdded = addedMultiTenantEntities.Where(e => (string?)e.Property("TenantId").CurrentValue == null);

        foreach (var e in notSetAdded)
        {
            e.Property("TenantId").CurrentValue = tenantInfo.Id;
        }

        // get list of all modified entities with MultiTenant annotation
        var modifiedMultiTenantEntities =
            changedMultiTenantEntities.Where(e => e.State == EntityState.Modified).ToList();

        // handle Tenant ID mismatches for modified entities
        var mismatchedModified = modifiedMultiTenantEntities.Where(e =>
            (string?)e.Property("TenantId").CurrentValue != null &&
            (string?)e.Property("TenantId").CurrentValue != tenantInfo.Id).ToList();

        if (mismatchedModified.Count != 0)
        {
            switch (tenantMismatchMode)
            {
                case TenantMismatchMode.Throw:
                    throw new MultiTenantException(
                        $"{mismatchedModified.Count} modified entities with Tenant Id mismatch.");

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
        var notSetModified = modifiedMultiTenantEntities
            .Where(e => (string?)e.Property("TenantId").CurrentValue == null).ToList();

        if (notSetModified.Count != 0)
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
        var deletedMultiTenantEntities = changedMultiTenantEntities.Where(e => e.State == EntityState.Deleted).ToList();

        // handle Tenant ID mismatches for deleted entities
        var mismatchedDeleted = deletedMultiTenantEntities.Where(e =>
            (string?)e.Property("TenantId").CurrentValue != null &&
            (string?)e.Property("TenantId").CurrentValue != tenantInfo.Id).ToList();

        if (mismatchedDeleted.Count != 0)
        {
            switch (tenantMismatchMode)
            {
                case TenantMismatchMode.Throw:
                    throw new MultiTenantException(
                        $"{mismatchedDeleted.Count} deleted entities with Tenant Id mismatch.");

                case TenantMismatchMode.Ignore:
                    // no action needed
                    break;

                case TenantMismatchMode.Overwrite:
                    // no action needed
                    break;
            }
        }

        // handle Tenant Id not set for deleted entities
        var notSetDeleted = deletedMultiTenantEntities.Where(e => (string?)e.Property("TenantId").CurrentValue == null)
            .ToList();

        if (notSetDeleted.Count != 0)
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