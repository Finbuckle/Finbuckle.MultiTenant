// Copyright Finbuckle LLC, Andrew White, and Contributors.
// Refer to the solution LICENSE file for more inforation.

using Microsoft.EntityFrameworkCore;
using Xunit;

namespace Finbuckle.MultiTenant.EntityFrameworkCore.Test
{
    public class MultiTenantDbContextShould
    {
        [Fact]
        public void WorkWithSingleParamCtor()
        {
            var tenant1 = new TenantInfo
            {
                Id = "abc",
                Identifier = "abc",
                Name = "abc",
                ConnectionString = "DataSource=testdb.db"
            };
            var c = new TestBlogDbContext(tenant1);

            Assert.NotNull(c);
        }

        [Fact]
        public void WorkWithTwoParamCtor()
        {
            var tenant1 = new TenantInfo
            {
                Id = "abc",
                Identifier = "abc",
                Name = "abc",
                ConnectionString = "DataSource=testdb.db"
            };
            var c = new TestBlogDbContext(tenant1, new DbContextOptions<TestBlogDbContext>());

            Assert.NotNull(c);
        }
    }
}