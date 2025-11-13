// Copyright Finbuckle LLC, Andrew White, and Contributors.
// Refer to the solution LICENSE file for more information.

using Finbuckle.MultiTenant.Abstractions;
using Finbuckle.MultiTenant.EntityFrameworkCore;
using Finbuckle.MultiTenant.EntityFrameworkCore.Extensions;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Finbuckle.MultiTenant.Identity.EntityFrameworkCore;

/// <summary>
/// An Identity database context that enforces tenant integrity on multi-tenant entity types.
/// <remarks>
/// All Identity entity types are multi-tenant by default.
/// </remarks>
/// </summary>
public class MultiTenantIdentityDbContext : MultiTenantIdentityDbContext<IdentityUser>
{
    /// <inheritdoc />
    public MultiTenantIdentityDbContext(IMultiTenantContextAccessor multiTenantContextAccessor) : base(
        multiTenantContextAccessor)
    {
    }

    /// <inheritdoc />
    public MultiTenantIdentityDbContext(IMultiTenantContextAccessor multiTenantContextAccessor,
        DbContextOptions options) : base(multiTenantContextAccessor, options)
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
/// <typeparamref name="TUser"/> is not multi-tenant by default.
/// All other Identity entity types are multi-tenant by default.
/// </remarks>
/// </summary>
/// <typeparam name="TUser">The <see cref="IdentityUser"/> derived type.</typeparam>
public abstract class MultiTenantIdentityDbContext<TUser> : MultiTenantIdentityDbContext<TUser, IdentityRole, string>
    where TUser : IdentityUser
{
    /// <inheritdoc />
    protected MultiTenantIdentityDbContext(IMultiTenantContextAccessor multiTenantContextAccessor) : base(
        multiTenantContextAccessor)
    {
    }

    /// <inheritdoc />
    protected MultiTenantIdentityDbContext(IMultiTenantContextAccessor multiTenantContextAccessor,
        DbContextOptions options) : base(multiTenantContextAccessor, options)
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
/// <typeparamref name="TUser"/> and <typeparamref name="TRole"/> are not multi-tenant by default.
/// All other Identity entity types are multi-tenant by default.
/// </remarks>
/// </summary>
/// <typeparam name="TUser">The <see cref="IdentityUser{TKey}"/> derived type.</typeparam>
/// <typeparam name="TRole">The <see cref="IdentityRole{TKey}"/> derived type.</typeparam>
/// <typeparam name="TKey">The key type.</typeparam>
public abstract class MultiTenantIdentityDbContext<TUser, TRole, TKey> : MultiTenantIdentityDbContext<TUser, TRole, TKey
    , IdentityUserClaim<TKey>, IdentityUserRole<TKey>, IdentityUserLogin<TKey>, IdentityRoleClaim<TKey>,
    IdentityUserToken<TKey>>
    where TUser : IdentityUser<TKey>
    where TRole : IdentityRole<TKey>
    where TKey : IEquatable<TKey>
{
    /// <inheritdoc />
    protected MultiTenantIdentityDbContext(IMultiTenantContextAccessor multiTenantContextAccessor) : base(
        multiTenantContextAccessor)
    {
    }

    /// <inheritdoc />
    protected MultiTenantIdentityDbContext(IMultiTenantContextAccessor multiTenantContextAccessor,
        DbContextOptions options) : base(multiTenantContextAccessor, options)
    {
    }

    /// <inheritdoc />
    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.Entity<IdentityUserClaim<TKey>>().IsMultiTenant().AdjustUniqueIndexes();
        builder.Entity<IdentityUserRole<TKey>>().IsMultiTenant().AdjustUniqueIndexes();
        builder.Entity<IdentityUserLogin<TKey>>().IsMultiTenant().AdjustUniqueIndexes();
        builder.Entity<IdentityRoleClaim<TKey>>().IsMultiTenant().AdjustUniqueIndexes();
        builder.Entity<IdentityUserToken<TKey>>().IsMultiTenant().AdjustUniqueIndexes();
    }
}

/// <summary>
/// An Identity database context that enforces tenant integrity on entity types
/// marked with the <see cref="MultiTenantAttribute"/> annotation or attribute.
/// <remarks>
/// No Identity entity types are multi-tenant by default.
/// </remarks>
/// </summary>
/// <typeparam name="TUser">The <see cref="IdentityUser{TKey}"/> derived type.</typeparam>
/// <typeparam name="TRole">The <see cref="IdentityRole{TKey}"/> derived type.</typeparam>
/// <typeparam name="TKey">The key type.</typeparam>
/// <typeparam name="TUserClaim">The <see cref="IdentityUserClaim{TKey}"/> derived type.</typeparam>
/// <typeparam name="TUserRole">The <see cref="IdentityUserRole{TKey}"/> derived type.</typeparam>
/// <typeparam name="TUserLogin">The <see cref="IdentityUserLogin{TKey}"/> derived type.</typeparam>
/// <typeparam name="TRoleClaim">The <see cref="IdentityRoleClaim{TKey}"/> derived type.</typeparam>
/// <typeparam name="TUserToken">The <see cref="IdentityUserToken{TKey}"/> derived type.</typeparam>
public abstract class MultiTenantIdentityDbContext<TUser, TRole, TKey, TUserClaim, TUserRole, TUserLogin, TRoleClaim,
    TUserToken> : IdentityDbContext<TUser, TRole, TKey, TUserClaim, TUserRole, TUserLogin, TRoleClaim, TUserToken>,
    IMultiTenantDbContext
    where TUser : IdentityUser<TKey>
    where TRole : IdentityRole<TKey>
    where TUserClaim : IdentityUserClaim<TKey>
    where TUserRole : IdentityUserRole<TKey>
    where TUserLogin : IdentityUserLogin<TKey>
    where TRoleClaim : IdentityRoleClaim<TKey>
    where TUserToken : IdentityUserToken<TKey>
    where TKey : IEquatable<TKey>
{
    /// <inheritdoc />
    public TenantInfo? TenantInfo { get; }

    /// <inheritdoc />
    public TenantMismatchMode TenantMismatchMode { get; set; } = TenantMismatchMode.Throw;

    /// <inheritdoc />
    public TenantNotSetMode TenantNotSetMode { get; set; } = TenantNotSetMode.Throw;

    /// <summary>
    /// Constructs the database context instance and binds to the current tenant.
    /// </summary>
    /// <param name="multiTenantContextAccessor">The <see cref="IMultiTenantContextAccessor"/> instance used to bind the context instance to a tenant.</param>
    protected MultiTenantIdentityDbContext(IMultiTenantContextAccessor multiTenantContextAccessor)
    {
        TenantInfo = multiTenantContextAccessor.MultiTenantContext.TenantInfo;
    }

    /// <summary>
    /// Constructs the database context instance and binds to the current tenant.
    /// </summary>
    /// <param name="multiTenantContextAccessor">The <see cref="IMultiTenantContextAccessor"/> instance used to bind the context instance to a tenant.</param>
    /// <param name="options">The <see cref="DbContextOptions"/> instance.</param>
    protected MultiTenantIdentityDbContext(IMultiTenantContextAccessor multiTenantContextAccessor,
        DbContextOptions options) : base(options)
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