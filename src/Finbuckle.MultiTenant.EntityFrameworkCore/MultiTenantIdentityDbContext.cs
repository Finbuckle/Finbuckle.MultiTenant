// Copyright Finbuckle LLC, Andrew White, and Contributors.
// Refer to the solution LICENSE file for more information.

using Finbuckle.MultiTenant.Abstractions;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Finbuckle.MultiTenant.EntityFrameworkCore;

/// <summary>
/// An Identity database context that enforces tenant integrity on multi-tenant entity types.
/// <remarks>
/// All Identity entity types are multi-tenant by default.
/// </remarks>
/// </summary>
public class MultiTenantIdentityDbContext : MultiTenantIdentityDbContext<IdentityUser>
{
    /// <inheritdoc />
    public MultiTenantIdentityDbContext(IMultiTenantContextAccessor multiTenantContextAccessor) : base(multiTenantContextAccessor)
    {
    }

    /// <inheritdoc />
    public MultiTenantIdentityDbContext(IMultiTenantContextAccessor multiTenantContextAccessor, DbContextOptions options) : base(multiTenantContextAccessor, options)
    {
    }
    
    /// <inheritdoc />
    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.Entity<IdentityUser>().IsMultiTenant().AdjustUniqueIndexes();
    }
}

/// <summary>
/// An Identity database context that enforces tenant integrity on multi-tenant entity types.
/// <remarks>
/// TUser is not multitenant by default.
/// All other Identity entity types are multitenant by default.
/// </remarks>
/// </summary>
public abstract class MultiTenantIdentityDbContext<TUser> : MultiTenantIdentityDbContext<TUser, IdentityRole, string>
    where TUser : IdentityUser
{
    /// <inheritdoc />
    protected MultiTenantIdentityDbContext(IMultiTenantContextAccessor multiTenantContextAccessor) : base(multiTenantContextAccessor)
    {
    }

    /// <inheritdoc />
    protected MultiTenantIdentityDbContext(IMultiTenantContextAccessor multiTenantContextAccessor, DbContextOptions options) : base(multiTenantContextAccessor, options)
    {
    }
    
    /// <inheritdoc />
    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.Entity<IdentityRole>().IsMultiTenant().AdjustUniqueIndexes();
    }
}

/// <summary>
/// An Identity database context that enforces tenant integrity on multi-tenant entity types.
/// <remarks>
/// TUser and TRole are not multitenant by default.
/// All other Identity entity types are multitenant by default.
/// </remarks>
/// </summary>
public abstract class MultiTenantIdentityDbContext<TUser, TRole, TKey> : MultiTenantIdentityDbContext<TUser, TRole, TKey, IdentityUserClaim<TKey>, IdentityUserRole<TKey>, IdentityUserLogin<TKey>, IdentityRoleClaim<TKey>, IdentityUserToken<TKey>>
    where TUser : IdentityUser<TKey>
    where TRole : IdentityRole<TKey>
    where TKey : IEquatable<TKey>
{
    /// <inheritdoc />
    protected MultiTenantIdentityDbContext(IMultiTenantContextAccessor multiTenantContextAccessor) : base(multiTenantContextAccessor)
    {
    }

    /// <inheritdoc />
    protected MultiTenantIdentityDbContext(IMultiTenantContextAccessor multiTenantContextAccessor, DbContextOptions options) : base(multiTenantContextAccessor, options)
    {
    }
    
    /// <inheritdoc />
    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.Entity<IdentityUserClaim<TKey>>().IsMultiTenant().AdjustUniqueIndexes();
        builder.Entity<IdentityUserRole<TKey>>().IsMultiTenant().AdjustUniqueIndexes();
        builder.Entity<IdentityUserLogin<TKey>>().IsMultiTenant().AdjustUniqueIndexes().AdjustKeys(builder);
        builder.Entity<IdentityRoleClaim<TKey>>().IsMultiTenant().AdjustUniqueIndexes();
        builder.Entity<IdentityUserToken<TKey>>().IsMultiTenant().AdjustUniqueIndexes();
    }
}

/// <summary>
/// An Identity database context that enforces tenant integrity on entity types
/// marked with the MultiTenant annotation or attribute.
/// <remarks>
/// No Identity entity types are multitenant by default.
/// </remarks>
/// </summary>
public abstract class MultiTenantIdentityDbContext<TUser, TRole, TKey, TUserClaim, TUserRole, TUserLogin, TRoleClaim, TUserToken> : IdentityDbContext<TUser, TRole, TKey, TUserClaim, TUserRole, TUserLogin, TRoleClaim, TUserToken>, IMultiTenantDbContext
    where TUser : IdentityUser<TKey>
    where TRole : IdentityRole<TKey>
    where TUserClaim : IdentityUserClaim<TKey>
    where TUserRole : IdentityUserRole<TKey>
    where TUserLogin : IdentityUserLogin<TKey>
    where TRoleClaim : IdentityRoleClaim<TKey>
    where TUserToken : IdentityUserToken<TKey>
    where TKey : IEquatable<TKey>
{
    public ITenantInfo? TenantInfo { get; }

    public TenantMismatchMode TenantMismatchMode { get; set; } = TenantMismatchMode.Throw;

    public TenantNotSetMode TenantNotSetMode { get; set; } = TenantNotSetMode.Throw;

    /// <summary>
    /// Constructs the database context instance and binds to the current tenant.
    /// </summary>
    /// <param name="multiTenantContextAccessor">The MultiTenantContextAccessor instance used to bind the context instance to a tenant.</param>
    protected MultiTenantIdentityDbContext(IMultiTenantContextAccessor multiTenantContextAccessor)
    {
        TenantInfo = multiTenantContextAccessor.MultiTenantContext.TenantInfo;
    }
    
    /// <summary>
    /// Constructs the database context instance and binds to the current tenant.
    /// </summary>
    /// <param name="multiTenantContextAccessor">The MultiTenantContextAccessor instance used to bind the context instance to a tenant.</param>
    /// <param name="options">The database options instance.</param>
    protected MultiTenantIdentityDbContext(IMultiTenantContextAccessor multiTenantContextAccessor, DbContextOptions options) : base(options)
    {
        TenantInfo = multiTenantContextAccessor.MultiTenantContext.TenantInfo;
    }

    /// <inheritdoc />
    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);
        builder.ConfigureMultiTenant();
    }

    /// <inheritdoc />
    public override int SaveChanges(bool acceptAllChangesOnSuccess)
    {
        this.EnforceMultiTenant();
        return base.SaveChanges(acceptAllChangesOnSuccess);
    }

    /// <inheritdoc />
    public override async Task<int> SaveChangesAsync(bool acceptAllChangesOnSuccess,
        CancellationToken cancellationToken = default(CancellationToken))
    {
        this.EnforceMultiTenant();
        return await base.SaveChangesAsync(acceptAllChangesOnSuccess, cancellationToken).ConfigureAwait(false);
    }
}