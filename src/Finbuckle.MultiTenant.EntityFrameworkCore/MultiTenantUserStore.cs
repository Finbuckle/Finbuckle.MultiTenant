using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Finbuckle.MultiTenant.EntityFrameworkCore;

public class MultiTenantUserStore<TUser, TRole, TContext, TKey>(
    TContext context,
    IdentityErrorDescriber? describer = null) : MultiTenantUserStore<TUser, TRole, TContext, TKey,
    IdentityUserClaim<TKey>, MultiTenantIdentityUserRole<TKey>, IdentityUserLogin<TKey>, IdentityUserToken<TKey>,
    IdentityRoleClaim<TKey>>(context, describer)
    where TUser : IdentityUser<TKey>
    where TRole : IdentityRole<TKey>
    where TContext : DbContext, IMultiTenantDbContext
    where TKey : IEquatable<TKey>;

public class MultiTenantUserStore<TUser, TRole, TContext,
    [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)]
    TKey, TUserClaim, TUserRole, TUserLogin,
    TUserToken, TRoleClaim>(
    TContext context,
    IdentityErrorDescriber? describer = null
) : UserStore<TUser, TRole, TContext, TKey, TUserClaim, TUserRole, TUserLogin, TUserToken, TRoleClaim>(context,
    describer)
    where TUser : IdentityUser<TKey>
    where TRole : IdentityRole<TKey>
    where TContext : DbContext, IMultiTenantDbContext
    where TKey : IEquatable<TKey>
    where TUserClaim : IdentityUserClaim<TKey>, new()
    where TUserRole : MultiTenantIdentityUserRole<TKey>, new()
    where TUserLogin : IdentityUserLogin<TKey>, new()
    where TUserToken : IdentityUserToken<TKey>, new()
    where TRoleClaim : IdentityRoleClaim<TKey>, new()
{
    protected override Task<TUserRole> FindUserRoleAsync(TKey userId, TKey roleId, CancellationToken cancellationToken)
    {
        return Context.Set<TUserRole>().FindAsync([userId, roleId, context.TenantInfo?.Id], cancellationToken).AsTask();
    }

    protected override TUserRole CreateUserRole(TUser user, TRole role)
    {
        return new TUserRole
        {
            UserId = user.Id,
            RoleId = role.Id,
            TenantId = context.TenantInfo?.Id
        };
    }
}