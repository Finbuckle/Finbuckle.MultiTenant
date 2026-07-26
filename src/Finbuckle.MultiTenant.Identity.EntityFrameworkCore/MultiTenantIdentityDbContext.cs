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
/// All Identity entity types are multi-tenant by default and have the tenant ID added to the unique index.
/// </remarks>
/// </summary>
public class MultiTenantIdentityDbContext<TId> : MultiTenantIdentityDbContext<IdentityUser, TId>
    where TId : IEquatable<TId>
{
    /// <inheritdoc />
    public MultiTenantIdentityDbContext()
    {
    }

    /// <inheritdoc />
    public MultiTenantIdentityDbContext(DbContextOptions options) : base(options)
    {
    }
}

/// <summary>
/// An Identity database context that enforces tenant integrity on multi-tenant entity types.
/// <remarks>
/// All Identity entity types are multi-tenant by default and have the tenant ID added to the unique index.
/// </remarks>
/// </summary>
/// <typeparam name="TUser">The <see cref="IdentityUser"/> derived type.</typeparam>
/// <typeparam name="TId">The ID implementation type.</typeparam>
public class MultiTenantIdentityDbContext<TUser, TId> : MultiTenantIdentityDbContext<TUser, IdentityRole, string, TId>
    where TUser : IdentityUser where TId : IEquatable<TId>
{
    /// <inheritdoc />
    protected MultiTenantIdentityDbContext()
    {
    }

    /// <inheritdoc />
    protected MultiTenantIdentityDbContext(DbContextOptions options) : base(options)
    {
    }
}

/// <summary>
/// An Identity database context that enforces tenant integrity on multi-tenant entity types.
/// <remarks>
/// All Identity entity types are multi-tenant by default and have the tenant ID added to the unique index.
/// </remarks>
/// </summary>
/// <typeparam name="TUser">The <see cref="IdentityUser{TKey}"/> derived type.</typeparam>
/// <typeparam name="TRole">The <see cref="IdentityRole{TKey}"/> derived type.</typeparam>
/// <typeparam name="TKey">The key type.</typeparam>
/// <typeparam name="TId">The ID implementation type.</typeparam>
public abstract class MultiTenantIdentityDbContext<TUser, TRole, TKey, TId> : MultiTenantIdentityDbContext<TUser, TRole,
    TKey, IdentityUserClaim<TKey>, IdentityUserRole<TKey>, IdentityUserLogin<TKey>, IdentityRoleClaim<TKey>,
    IdentityUserToken<TKey>, IdentityUserPasskey<TKey>, TId>
    where TUser : IdentityUser<TKey>
    where TRole : IdentityRole<TKey>
    where TKey : IEquatable<TKey>
    where TId : IEquatable<TId>
{
    /// <inheritdoc />
    protected MultiTenantIdentityDbContext()
    {
    }

    /// <inheritdoc />
    protected MultiTenantIdentityDbContext(DbContextOptions options) : base(options)
    {
    }
}

/// <summary>
/// An Identity database context that enforces tenant integrity on multi-tenant entity types
/// <remarks>
/// All Identity entity types are multi-tenant by default and have the tenant ID added to the unique index.
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
/// <typeparam name="TUserPasskey">The <see cref="IdentityUserPasskey{TKey}"/> derived type.</typeparam>
/// <typeparam name="TId">The ID implementation type.</typeparam>
public abstract class MultiTenantIdentityDbContext<TUser, TRole, TKey, TUserClaim, TUserRole, TUserLogin, TRoleClaim,
    TUserToken, TUserPasskey, TId> :
    IdentityDbContext<TUser, TRole, TKey, TUserClaim, TUserRole, TUserLogin, TRoleClaim, TUserToken, TUserPasskey>,
    IMultiTenantDbContext<TId>
    where TUser : IdentityUser<TKey>
    where TRole : IdentityRole<TKey>
    where TUserClaim : IdentityUserClaim<TKey>
    where TUserRole : IdentityUserRole<TKey>
    where TUserLogin : IdentityUserLogin<TKey>
    where TRoleClaim : IdentityRoleClaim<TKey>
    where TUserToken : IdentityUserToken<TKey>
    where TUserPasskey : IdentityUserPasskey<TKey>
    where TKey : IEquatable<TKey>
    where TId : IEquatable<TId>
{
    /// <inheritdoc />
    public ITenantInfo<TId>? TenantInfo { get; set; }

    /// <inheritdoc />
    public TenantMismatchMode TenantMismatchMode { get; set; } = TenantMismatchMode.Throw;

    /// <inheritdoc />
    public TenantNotSetMode TenantNotSetMode { get; set; } = TenantNotSetMode.Throw;

    /// <summary>
    /// Constructs the database context instance.
    /// </summary>
    protected MultiTenantIdentityDbContext()
    {
    }

    /// <summary>
    /// Constructs the database context instance with the provided options.
    /// </summary>
    /// <param name="options">The <see cref="DbContextOptions"/> instance.</param>
    protected MultiTenantIdentityDbContext(DbContextOptions options) : base(options)
    {
    }

    /// <inheritdoc />
    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.Entity<TUser>().IsMultiTenant<TId>().AdjustUniqueIndexes();
        builder.Entity<TRole>().IsMultiTenant<TId>().AdjustUniqueIndexes();
        builder.Entity<TUserClaim>().IsMultiTenant<TId>().AdjustUniqueIndexes();
        builder.Entity<TUserRole>().IsMultiTenant<TId>().AdjustUniqueIndexes();
        builder.Entity<TUserLogin>().IsMultiTenant<TId>().AdjustUniqueIndexes();
        builder.Entity<TRoleClaim>().IsMultiTenant<TId>().AdjustUniqueIndexes();
        builder.Entity<TUserToken>().IsMultiTenant<TId>().AdjustUniqueIndexes();
        if(SchemaVersion == IdentitySchemaVersions.Version3)
            builder.Entity<TUserPasskey>().IsMultiTenant<TId>().AdjustUniqueIndexes();
        builder.ConfigureMultiTenant<TId>();
    }

    /// <inheritdoc />
    public override int SaveChanges(bool acceptAllChangesOnSuccess)
    {
        this.EnforceMultiTenant<MultiTenantIdentityDbContext<TUser, TRole, TKey, TUserClaim, TUserRole, TUserLogin, TRoleClaim,
            TUserToken, TUserPasskey, TId>, TId>();
        return base.SaveChanges(acceptAllChangesOnSuccess);
    }

    /// <inheritdoc />
    public override async Task<int> SaveChangesAsync(bool acceptAllChangesOnSuccess,
        CancellationToken cancellationToken = default(CancellationToken))
    {
        this.EnforceMultiTenant<MultiTenantIdentityDbContext<TUser, TRole, TKey, TUserClaim, TUserRole, TUserLogin, TRoleClaim,
            TUserToken, TUserPasskey, TId>, TId>();
        return await base.SaveChangesAsync(acceptAllChangesOnSuccess, cancellationToken).ConfigureAwait(false);
    }
}