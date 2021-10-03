// Copyright Finbuckle LLC, Andrew White, and Contributors.
// Refer to the solution LICENSE file for more inforation.

using System.Diagnostics.CodeAnalysis;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace Finbuckle.MultiTenant.EntityFrameworkCore.Test.Extensions
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
            builder.ConfigureMultiTenant();
        }
    }

    [MultiTenant]
    public class MyMultiTenantThing
    {
        public int Id { get; set; }
    }

    public class MyThing
    {
        public int Id { get; set; }
    }

    public class ModelBuiikderExtensionShould
    {
        private DbContext GetDbContext()
        {
            var connection = new SqliteConnection("DataSource=:memory:");
            var options = new DbContextOptionsBuilder()
                .UseSqlite(connection)
                .Options;
            return new TestDbContext(options);
        }

        [Fact]
        public void SetMultiTenantOnTypeWithMultiTenantAttribute()
        {
            var db = GetDbContext();
        
            Assert.True(db.Model.FindEntityType(typeof(MyMultiTenantThing)).IsMultiTenant());
        }
        
        // [Fact]
        // public void AdjustKeyOnTypeWithMultiTenantAttribute()
        // {
        //     var db = GetDbContext();
        //
        //     var adjustedKey = db.Model.FindEntityType(typeof(MyMultiTenantThing)).GetKeys().Where(k => k.Properties.Select(p => p.Name).Contains("TenantId")).SingleOrDefault();
        //     Assert.NotNull(adjustedKey);
        // }

        [Fact]
        public void DoNotSetMultiTenantOnTypeWithoutMultiTenantAttribute()
        {
            var db = GetDbContext();

            Assert.False(db.Model.FindEntityType(typeof(MyThing)).IsMultiTenant());
        }
    }
}