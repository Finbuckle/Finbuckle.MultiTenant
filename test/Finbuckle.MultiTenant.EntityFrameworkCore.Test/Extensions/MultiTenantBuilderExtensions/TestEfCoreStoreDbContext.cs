// Copyright Finbuckle LLC, Andrew White, and Contributors.
// Refer to the solution LICENSE file for more inforation.

using Finbuckle.MultiTenant.Stores;
using Microsoft.EntityFrameworkCore;

namespace Finbuckle.MultiTenant.EntityFrameworkCore.Test.Extensions.MultiTenantBuilderExtensions
{
    public class TestEfCoreStoreDbContext : EFCoreStoreDbContext<TenantInfo>
    {
        public TestEfCoreStoreDbContext(DbContextOptions options) : base(options)
        {
        }
    }
}