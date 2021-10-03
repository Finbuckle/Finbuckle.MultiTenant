// Copyright Finbuckle LLC, Andrew White, and Contributors.
// Refer to the solution LICENSE file for more inforation.

using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace Finbuckle.MultiTenant.EntityFrameworkCore.Test.Extensions
{
    public class ModelExtensionsShould
    {
        [SuppressMessage("ReSharper", "UnusedMember.Local")]
        public class TestDbContext : DbContext
        {
            public TestDbContext(DbContextOptions options) : base(options)
            {
            }

            DbSet<MyMultiTenantThing> MyMultiTenantThings { get; set; }
            DbSet<MyThing> MyThings { get; set; }

            protected override void OnModelCreating(ModelBuilder builder)
            {
                builder.Entity<MyMultiTenantThing>().IsMultiTenant();
            }
        }

        public class MyMultiTenantThing
        {
            public int Id { get; set; }
        }

        public class MyThing
        {
            public int Id { get; set; }
        }
        
        private DbContext GetDbContext()
        {
            var connection = new SqliteConnection("DataSource=:memory:");
            var options = new DbContextOptionsBuilder()
                .UseSqlite(connection)
                .Options;
            return new TestDbContext(options);
        }
        
        [Fact]
        public void ReturnMultiTenantTypes()
        {
            var db = GetDbContext();

            Assert.Contains(typeof(MyMultiTenantThing), db.Model.GetMultiTenantEntityTypes().Select(et => et.ClrType));
        }

        [Fact]
        public void NotReturnNonMultiTenantTypes()
        {
            var db = GetDbContext();

            Assert.DoesNotContain(typeof(MyThing), db.Model.GetMultiTenantEntityTypes().Select(et => et.ClrType));
        }
    }
}
