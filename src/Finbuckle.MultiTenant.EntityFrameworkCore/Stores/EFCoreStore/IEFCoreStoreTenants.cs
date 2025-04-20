// Copyright Finbuckle LLC, Andrew White, and Contributors.
// Refer to the solution LICENSE file for more information.

using Finbuckle.MultiTenant.Abstractions;
using Microsoft.EntityFrameworkCore;

namespace Finbuckle.MultiTenant.EntityFrameworkCore.Stores.EFCoreStore;

public interface IEFCoreStoreTenants<TTenantInfo>
    where TTenantInfo : class, ITenantInfo, new()
{
    DbSet<TTenantInfo> TenantInfo { get; }
}