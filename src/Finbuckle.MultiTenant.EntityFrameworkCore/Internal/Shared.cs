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
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Finbuckle.MultiTenant.Core;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace Finbuckle.MultiTenant.EntityFrameworkCore
{
    /// <summary>
    /// A static class containing static methods shared between
    /// MultiTenantDbContext and MultiTenantIdentityDbContext.
    /// </summary>
    internal static class Shared
    {
        /// <summary>
        /// Adds a "required" constraint to the TenantId string property and
        /// sets the query filter for the entity on TenantId.
        /// </summary>
        /// <remarks>
        ///     <list type="bullet">
        ///         <item><description>This method is called from OnModelCreating of the derived DbContext.</description></item>
        ///         <item><description>If no TenantId string property exists, a shadow property is created.</description></item>
        ///         <item><description>The query filter has no effect when calling DbSet.Find.</description></item>
        ///     </list>
        /// </remarks>
        /// <param name="modelBuilder"></param>
        /// <param name="tenantInfo"></param>
        public static void SetupModel(ModelBuilder modelBuilder, Expression<Func<TenantInfo>> currentTenantInfoExpression)
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

                // build expression tree for e => EF.Property<string>(e, "TenantId") == TenantInfo.Id

                // where e is one of our entity types
                // will need this ParameterExpression for next step and for final step
                var entityParamExp = Expression.Parameter(t.ClrType, "e");

                // override to match existing query paraameter if applicable
                if (GetQueryFilter(r) != null)
                {
                    entityParamExp = GetQueryFilter(r).Parameters.First();
                }

                // build up expression tree for EF.Property<string>(e, "TenantId")
                var tenantIdExp = Expression.Constant("TenantId", typeof(string));
                var efPropertyExp = Expression.Call(typeof(EF), "Property", new[] { typeof(string) }, entityParamExp, tenantIdExp);
                var leftExp = efPropertyExp;

                // build expression tree for EF.Property<string>(e, "TenantId") == TenantInfo.Id
                var rightExp = Expression.Property(currentTenantInfoExpression.Body, nameof(TenantInfo.Id));
                var predicate = Expression.Equal(leftExp, rightExp);

                // combine with existing filter
                if (GetQueryFilter(r) != null)
                {
                    var originalFilter = GetQueryFilter(r);
                    predicate = Expression.AndAlso(originalFilter.Body, predicate);
                }

                // build the final expression tree
                var delegateType = Expression.GetDelegateType(t.ClrType, typeof(bool));
                var lambdaExp = Expression.Lambda(delegateType, predicate, entityParamExp);

                // set the filter
                r.HasQueryFilter(lambdaExp);
            }
        }

        private static LambdaExpression GetQueryFilter(Microsoft.EntityFrameworkCore.Metadata.Builders.EntityTypeBuilder r)
        {
            #if NETSTANDARD2_1
                return r.Metadata.GetQueryFilter();
            #elif NETSTANDARD2_0
                return r.Metadata.QueryFilter;
            #else
                #error No valid path!
            #endif
        }

        public static bool HasMultiTenantAttribute(Type t)
        {
            return t.GetCustomAttribute<MultiTenantAttribute>() != null;
        }

        /// <summary>
        /// Checks the TenantId on entities taking into account
        /// the tenantNotSetMode and the tenantMismatchMode.
        /// </summary>
        /// <remarks>
        ///     <list type="bullet">
        ///         <item><description>This method is called from SaveChanges or SaveChangesAsync of the derived DbContext.</description></item>
        ///         <item><description>If any changes are detected in an entitiy with the MultiTenant attribute then the TenantContext must not be null.</description></item>
        ///     </list>  
        /// </remarks>
        /// /// <param name="tenantInfo"></param>
        /// <param name="changeTracker"></param>
        /// <param name="tenantNotSetMode"></param>
        /// <param name="tenantMismatchMode"></param>
        public static void EnforceTenantId(TenantInfo tenantInfo,
            ChangeTracker changeTracker,
            TenantNotSetMode tenantNotSetMode,
            TenantMismatchMode tenantMismatchMode)
        {
            var changedMultiTenantEntities = changeTracker.Entries().
                Where(e => e.State == EntityState.Added || e.State == EntityState.Modified || e.State == EntityState.Deleted).
                Where(e => Shared.HasMultiTenantAttribute(e.Entity.GetType()));

            // ensure tenant context is valid
            if (changedMultiTenantEntities.Any())
                CheckTenantInfo(tenantInfo);

            // get list of all added entities with TenantScope attribute
            var addedTenantScopeEntities = changedMultiTenantEntities.
                Where(e => e.State == EntityState.Added);

            // handle Tenant Id mismatches for added entities
            var mismatchedAdded = addedTenantScopeEntities.
                Where(e => (string)e.Property("TenantId").CurrentValue != null &&
                (string)e.Property("TenantId").CurrentValue != tenantInfo.Id);

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
                            e.Property("TenantId").CurrentValue = tenantInfo.Id;
                        }
                        break;
                }
            }

            // for added entities TenantNotSetMode is always Overwrite
            var notSetAdded = addedTenantScopeEntities.
                Where(e => (string)e.Property("TenantId").CurrentValue == null);

            foreach (var e in notSetAdded)
            {
                e.Property("TenantId").CurrentValue = tenantInfo.Id;
            }

            // get list of all modified entities with TenantScope attribute
            var modifiedTenantScopeEntities = changedMultiTenantEntities.
                Where(e => e.State == EntityState.Modified);

            // handle Tenant Id mismatches for modified entities
            var mismatchedModified = modifiedTenantScopeEntities.
                Where(e => (string)e.Property("TenantId").CurrentValue != null &&
                (string)e.Property("TenantId").CurrentValue != tenantInfo.Id);

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
                            e.Property("TenantId").CurrentValue = tenantInfo.Id;
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
                            e.Property("TenantId").CurrentValue = tenantInfo.Id;
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
                (string)e.Property("TenantId").CurrentValue != tenantInfo.Id);

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

        private static void CheckTenantInfo(TenantInfo tenantInfo)
        {
            if (tenantInfo == null)
                throw new MultiTenantException("MultiTenant Entity cannot be changed if TenantInfo is null.");
        }
    }
}
