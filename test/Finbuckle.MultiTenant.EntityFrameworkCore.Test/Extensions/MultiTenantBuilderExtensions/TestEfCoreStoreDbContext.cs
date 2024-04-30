// Copyright Finbuckle LLC, Andrew White, and Contributors.
// Refer to the solution LICENSE file for more information.

using Finbuckle.MultiTenant.EntityFrameworkCore.Stores.EFCoreStore;
using Microsoft.EntityFrameworkCore;

namespace Finbuckle.MultiTenant.EntityFrameworkCore.Test.Extensions.MultiTenantBuilderExtensions;

public class TestEfCoreStoreDbContext : EFCoreStoreDbContext<TenantInfo>
{
    public TestEfCoreStoreDbContext(DbContextOptions options) : base(options)
    {
        }
}