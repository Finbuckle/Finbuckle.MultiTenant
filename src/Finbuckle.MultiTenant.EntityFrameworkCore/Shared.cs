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
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Finbuckle.MultiTenant.Core;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace Finbuckle.MultiTenant.EntityFrameworkCore
{
    /// <summary>
    /// Determines how entities where <c>TenantId</c> does not match the <c>TenantContext</c> are handled
    /// when <c>SaveChanges</c> or <c>SaveChangesAsync</c> is called.
    /// </summary>
    public enum TenantMismatchMode
    {
        Throw,
        Ignore,
        Overwrite
    }

    /// <summary>
    /// Determines how entities with null <c>TenantId</c> are handled
    /// when <c>SaveChanges</c> or <c>SaveChangesAsync</c> is called.
    /// </summary>
    public enum TenantNotSetMode
    {
        Throw,
        Overwrite
    }

    /// <summary>
    /// A static class containing static methods shared between
    /// MultiTenantDbContext and MultiTenantIdentityDbContext.
    /// </summary>
    internal static class Shared
    {
        /// <summary>
        /// Adds a "required" constraint to the <c>TenantId</c> string property and
        /// sets the query filter for the entity on <c>TenantId</c>.
        /// </summary>
        /// <remarks>
        ///     <list type="bullet">
        ///         <item><description>This method is called from <c>OnModelCreating</c> of the derived <c>DbContext</c>.</description></item>
        ///         <item><description>If no TenantId string property exists, a shadow property is created.</description></item>
        ///         <item><description>The query filter has no effect when calling <c>DbSet.Find</c>.</description></item>
        ///     </list>
        /// </remarks>
        /// <param name="modelBuilder"></param>
        /// <param name="tenantContext"></param>
        internal static void SetupModel(ModelBuilder modelBuilder, TenantContext tenantContext)
        {
            foreach (var t in modelBuilder.Model.GetEntityTypes().
                Where(t => HasMultiTenantAttribute(t.ClrType)))
                
            {
                var r = modelBuilder.Entity(t.ClrType);

                try
                {
                    r.Property<string>("TenantId").IsRequired().HasMaxLength(Constants.TenantIdMaxLength);
                }
                catch (Exception e)
                {
                    throw new MultiTenantException($"{t.ClrType} unable to add TenantId property", e);
                }

                // build expression tree for e => EF.Property<string>(e, "TenantId") == tenantId
                // where e is one of our entity types
                // will need this ParameterExpression for next step and for final step
                var entityParamExp = Expression.Parameter(t.ClrType, "e");

                // build up expression tree for EF.Property<string>(e, "TenantId")
                var tenantIdExp = Expression.Constant("TenantId", typeof(string));
                var efPropertyExp = Expression.Call(typeof(EF), "Property", new[] { typeof(string) }, entityParamExp, tenantIdExp);
                var leftExp = efPropertyExp;

                // build expression tree for EF.Property<string>(e, "TenantId") == tenantId
                var rightExp = Expression.Constant(tenantContext?.Id, typeof(string));
                var equalExp = Expression.Equal(leftExp, rightExp);

                // build the final expression tree
                var delegateType = Expression.GetDelegateType(t.ClrType, typeof(bool));
                var lambdaExp = Expression.Lambda(delegateType, equalExp, entityParamExp);

                // set the filter
                r.HasQueryFilter(lambdaExp);
            }
        }

        internal static bool HasMultiTenantAttribute(Type t)
        {
            return t.GetCustomAttribute<MultiTenantAttribute>() != null;
        }

        /// <summary>
        /// Checks the <c>TenantId</c> on entities taking into account
        /// the <c>tenantNotSetMode</c> and the <c>tenantMismatchMode</c>.
        /// </summary>
        /// <remarks>
        ///     <list type="bullet">
        ///         <item><description>This method is called from <c>SaveChanges</c> or <c>SaveChangesAsync</c> of the derived <c>DbContext</c>.</description></item>
        ///         <item><description>If any changes are detected in an entitiy with the <c>MultiTenant</c> attribute then the <c>TenantContext</c> must not be null.</description></item>
        ///     </list>  
        /// </remarks>
        /// /// <param name="tenantContext"></param>
        /// <param name="changeTracker"></param>
        /// <param name="tenantNotSetMode"></param>
        /// <param name="tenantMismatchMode"></param>
        internal static void EnforceTenantId(TenantContext tenantContext,
                                            ChangeTracker changeTracker,
                                            TenantNotSetMode tenantNotSetMode,
                                            TenantMismatchMode tenantMismatchMode)
        {
            var changedMultiTenantEntities = changeTracker.Entries().
                Where(e => e.State == EntityState.Added || e.State == EntityState.Modified || e.State == EntityState.Deleted).
                Where(e => Shared.HasMultiTenantAttribute(e.Entity.GetType()));

            // ensure tenant context is valid
            if (changedMultiTenantEntities.Any())
                CheckTenantContext(tenantContext);

            // get list of all added entities with TenantScope attribute
            var addedTenantScopeEntities = changedMultiTenantEntities.
                Where(e => e.State == EntityState.Added);

            // handle Tenant Id mismatches for added entities
            var mismatchedAdded = addedTenantScopeEntities.
                Where(e => (string)e.Property("TenantId").CurrentValue != null &&
                (string)e.Property("TenantId").CurrentValue != tenantContext.Id);

            if (mismatchedAdded.Any())
            {
                switch (tenantMismatchMode)
                {
                    case TenantMismatchMode.Throw:
                        throw new MultiTenantException($"{mismatchedAdded.Count()} added entities with Tenant Id mismatch."); ;

                    case TenantMismatchMode.Ignore:
                        // no action needed
                        break;

                    case TenantMismatchMode.Overwrite:
                        foreach (var e in mismatchedAdded)
                        {
                            e.Property("TenantId").CurrentValue = tenantContext.Id;
                        }
                        break;
                }
            }

            // for added entities TenantNotSetMode is always Overwrite
            var notSetAdded = addedTenantScopeEntities.
                Where(e => (string)e.Property("TenantId").CurrentValue == null);

            foreach (var e in notSetAdded)
            {
                e.Property("TenantId").CurrentValue = tenantContext.Id;
            }

            // get list of all modified entities with TenantScope attribute
            var modifiedTenantScopeEntities = changedMultiTenantEntities.
                Where(e => e.State == EntityState.Modified);

            // handle Tenant Id mismatches for modified entities
            var mismatchedModified = modifiedTenantScopeEntities.
                Where(e => (string)e.Property("TenantId").CurrentValue != null &&
                (string)e.Property("TenantId").CurrentValue != tenantContext.Id);

            if (mismatchedModified.Any())
            {
                switch (tenantMismatchMode)
                {
                    case TenantMismatchMode.Throw:
                        throw new MultiTenantException($"{mismatchedModified.Count()} modified entities with Tenant Id mismatch."); ;

                    case TenantMismatchMode.Ignore:
                        // no action needed
                        break;

                    case TenantMismatchMode.Overwrite:
                        foreach (var e in mismatchedModified)
                        {
                            e.Property("TenantId").CurrentValue = tenantContext.Id;
                        }
                        break;
                }
            }

            // handle Tenant Id not set for modified entities
            var notSetModified = modifiedTenantScopeEntities.
                Where(e => (string)e.Property("TenantId").CurrentValue == null);

            if (notSetModified.Any())
            {
                switch (tenantNotSetMode)
                {
                    case TenantNotSetMode.Throw:
                        throw new MultiTenantException($"{notSetModified.Count()} modified entities with Tenant Id not set."); ;

                    case TenantNotSetMode.Overwrite:
                        foreach (var e in notSetModified)
                        {
                            e.Property("TenantId").CurrentValue = tenantContext.Id;
                        }
                        break;
                }
            }

            // get list of all deleted  entities with TenantScope attribute
            var deletedTenantScopeEntities = changedMultiTenantEntities.
                Where(e => e.State == EntityState.Deleted);

            // handle Tenant Id mismatches for deleted entities
            var mismatchedDeleted = deletedTenantScopeEntities.
                Where(e => (string)e.Property("TenantId").CurrentValue != null &&
                (string)e.Property("TenantId").CurrentValue != tenantContext.Id);

            if (mismatchedDeleted.Any())
            {
                switch (tenantMismatchMode)
                {
                    case TenantMismatchMode.Throw:
                        throw new MultiTenantException($"{mismatchedDeleted.Count()} deleted entities with Tenant Id mismatch."); ;

                    case TenantMismatchMode.Ignore:
                        // no action needed
                        break;

                    case TenantMismatchMode.Overwrite:
                        // no action needed
                        break;
                }
            }

            // handle Tenant Id not set for deleted entities
            var notSetDeleted = deletedTenantScopeEntities.
                Where(e => (string)e.Property("TenantId").CurrentValue == null);

            if (notSetDeleted.Any())
            {
                switch (tenantNotSetMode)
                {
                    case TenantNotSetMode.Throw:
                        throw new MultiTenantException($"{notSetDeleted.Count()} deleted entities with Tenant Id not set."); ;

                    case TenantNotSetMode.Overwrite:
                        // no action needed
                        break;
                }
            }
        }

        private static void CheckTenantContext(TenantContext tenantContext)
        {
            if (tenantContext == null)
                throw new MultiTenantException("MultiTenant Entity cannot be changed if TenantContext is null.");
        }
    }
}
