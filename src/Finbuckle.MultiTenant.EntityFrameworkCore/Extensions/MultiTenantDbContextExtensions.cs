// Copyright Finbuckle LLC, Andrew White, and Contributors.
// Refer to the solution LICENSE file for more information.

using System.Runtime.CompilerServices;
using Finbuckle.MultiTenant.Abstractions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Finbuckle.MultiTenant.EntityFrameworkCore.Extensions;

/// <summary>
/// Extension methods for multi-tenant <see cref="DbContext"/> instances.
/// </summary>
public static class MultiTenantDbContextExtensions
{
    private static readonly ConditionalWeakTable<DbContext, object?> TrackingHandlerRegistry = new();
    private static readonly Lock TrackingHandlerRegistryLock = new();

    /// <summary>
    /// Ensures a TenantId property is set when an entity is attached.
    /// </summary>
    /// <typeparam name="TContext">The <see cref="DbContext"/> type.</typeparam>
    /// <typeparam name="TId">The ID implementation type.</typeparam>
    /// <param name="context">The <see cref="DbContext"/> instance.</param>
    public static void EnforceMultiTenantOnTracking<TContext, TId>(this TContext context)
        where TContext : DbContext, IMultiTenantDbContext<TId> where TId : IEquatable<TId>
    {
        // need to lock and track if the event handler has been registered already so that multiple
        // calls to EnforceMultiTenantOnTracking do not register multiple instances
        lock (TrackingHandlerRegistryLock)
        {
            if (TrackingHandlerRegistry.TryGetValue(context, out _))
                return;

            // Configure event to handle newly tracked entities.
            context.ChangeTracker.Tracking += (sender, args) =>
            {
                if (!args.Entry.Metadata.IsMultiTenant() || args.FromQuery ||
                    args.Entry.Context is not IMultiTenantDbContext<TId> multiTenantDbContext) return;

                if (multiTenantDbContext.TenantInfo is null)
                    throw new MultiTenantException("MultiTenant Entity cannot be attached if TenantInfo is null.");

                var tenantIdProperty = args.Entry.Property("TenantId");
                tenantIdProperty.CurrentValue ??= multiTenantDbContext.TenantInfo.Id;
            };

            TrackingHandlerRegistry.Add(context, null);
        }
    }

    /// <summary>
    /// Checks the TenantId on entities during SaveChanges and SaveChangesAsync taking into account <see cref="TenantNotSetMode"/> and <see cref="TenantMismatchMode"/>.
    /// </summary>
    /// <typeparam name="TContext">The <see cref="DbContext"/> type.</typeparam>
    /// <typeparam name="TId">The ID implementation type.</typeparam>
    /// <param name="context">The <see cref="DbContext"/> instance.</param>
    public static void EnforceMultiTenant<TContext, TId>(this TContext context)
        where TContext : DbContext, IMultiTenantDbContext<TId> where TId : IEquatable<TId>
    {
        var changeTracker = context.ChangeTracker;
        var tenantInfo = context.TenantInfo;
        var tenantMismatchMode = context.TenantMismatchMode;
        var tenantNotSetMode = context.TenantNotSetMode;

        var changedMultiTenantEntities = changeTracker.Entries()
            .Where(e => e.State is EntityState.Added or EntityState.Modified or EntityState.Deleted)
            .Where(e => e.Metadata.IsMultiTenant()).ToList();

        // ensure tenant context is valid
        if (changedMultiTenantEntities.Count == 0)
            return;

        if (tenantInfo is null)
            throw new MultiTenantException("MultiTenant Entity cannot be changed if TenantInfo is null.");


        // get list of all added entities with MultiTenant annotation
        var addedMultiTenantEntities = changedMultiTenantEntities.Where(e => e.State == EntityState.Added).ToList();

        // handle Tenant ID mismatches for added entities
        var mismatchedAdded = addedMultiTenantEntities.Where(e =>
            e.Property("TenantId").CurrentValue != null &&
            !e.Property("TenantId").CurrentValue!.Equals(tenantInfo.Id)).ToList();

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
            e.Property("TenantId").CurrentValue != null &&
            !e.Property("TenantId").CurrentValue!.Equals(tenantInfo.Id)).ToList();

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
            e.Property("TenantId").CurrentValue != null &&
            !e.Property("TenantId").CurrentValue!.Equals(tenantInfo.Id)).ToList();

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


    /// <summary>
    /// Creates a new instance of a <see cref="DbContext"/> bound to the given tenant.
    /// </summary>
    /// <param name="tenantInfo">The tenant information to bind to the context.</param>
    /// <typeparam name="TContext">The <see cref="DbContext"/> implementation type.</typeparam>
    /// <typeparam name="TTenantInfo">The <see cref="ITenantInfo{TId}"/> implementation type.</typeparam>
    /// <typeparam name="TId">The ID implementation type.</typeparam>
    /// <returns>The newly created <see cref="DbContext"/> instance.</returns>
    public static TContext Create<TContext, TTenantInfo, TId>(TTenantInfo tenantInfo)
        where TContext : DbContext, IMultiTenantDbContext<TId>
        where TTenantInfo : ITenantInfo<TId>
        where TId : IEquatable<TId> => Create<TContext, TTenantInfo,TId>(tenantInfo, []);

    /// <summary>
    /// Creates a new instance of a <see cref="DbContext"/> bound to the given tenant, with optional constructor dependencies.
    /// </summary>
    /// <param name="tenantInfo">The tenant information to bind to the context.</param>
    /// <param name="args">Additional dependencies for the <see cref="DbContext"/> constructor.</param>
    /// <typeparam name="TContext">The <see cref="DbContext"/> implementation type.</typeparam>
    /// <typeparam name="TTenantInfo">The <see cref="ITenantInfo{TId}"/> implementation type.</typeparam>
    /// <typeparam name="TId">The ID implementation type.</typeparam>
    /// <returns>The newly created <see cref="DbContext"/> instance.</returns>
    public static TContext Create<TContext, TTenantInfo, TId>(TTenantInfo tenantInfo, params object[] args)
        where TContext : DbContext, IMultiTenantDbContext<TId>
        where TTenantInfo : ITenantInfo<TId>
        where TId : IEquatable<TId>
    {
        try
        {
            args ??= [];
            var context = (TContext)Activator.CreateInstance(typeof(TContext), args)!;
            context.TenantInfo = tenantInfo;
            return context;
        }
        catch (MissingMethodException e)
        {
            throw new ArgumentException(
                "The provided DbContext type does not have a constructor that accepts the required parameters.", e);
        }
    }

    /// <summary>
    /// Creates a new instance of a <see cref="DbContext"/> bound to the given tenant, resolving dependencies from the provided service provider.
    /// </summary>
    /// <param name="tenantInfo">The tenant information to bind to the context.</param>
    /// <param name="serviceProvider">The <see cref="IServiceProvider"/> used to resolve <see cref="DbContext"/> constructor dependencies.</param>
    /// <param name="args">Additional dependencies for the <see cref="DbContext"/> constructor.</param>
    /// <typeparam name="TContext">The <see cref="DbContext"/> implementation type.</typeparam>
    /// <typeparam name="TTenantInfo">The <see cref="ITenantInfo{TId}"/> implementation type.</typeparam>
    /// <typeparam name="TId">The ID implementation type.</typeparam>
    /// <returns>The newly created <see cref="DbContext"/> instance.</returns>
    public static TContext Create<TContext, TTenantInfo, TId>(TTenantInfo tenantInfo, IServiceProvider serviceProvider,
        params object[] args)
        where TContext : DbContext, IMultiTenantDbContext<TId>
        where TTenantInfo : ITenantInfo<TId>
        where TId : IEquatable<TId>, ISpanParsable<TId>
    {
        try
        {
            args ??= [];
            var context = ActivatorUtilities.CreateInstance<TContext>(serviceProvider, args);
            context.TenantInfo = tenantInfo;
            return context;
        }
        catch (MissingMethodException e)
        {
            throw new ArgumentException(
                "The provided DbContext type does not have a constructor that accepts the required parameters.", e);
        }
    }
} 