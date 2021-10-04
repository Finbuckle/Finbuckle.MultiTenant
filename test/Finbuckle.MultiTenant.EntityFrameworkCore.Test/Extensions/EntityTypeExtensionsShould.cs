// Copyright Finbuckle LLC, Andrew White, and Contributors.
// Refer to the solution LICENSE file for more inforation.

using System;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace Finbuckle.MultiTenant.EntityFrameworkCore.Test.Extensions
{
    public class EntityTypeExtensionShould : IDisposable
    {
        public class TestDbContext : DbContext
        {
            public TestDbContext(DbContextOptions options) : base(options)
            {
            }

            // ReSharper disable once MemberHidesStaticFromOuterClass
            // ReSharper disable once UnusedMember.Local
            DbSet<MyMultiTenantThing> MyMultiTenantThing { get; set; }
            // ReSharper disable once MemberHidesStaticFromOuterClass
            // ReSharper disable once UnusedMember.Local
            DbSet<MyThing> MyThing { get; set; }

            protected override void OnModelCreating(ModelBuilder builder)
            {
                builder.Entity<MyMultiTenantThing>().IsMultiTenant();
                builder.Entity<MyMultiTenantChildThing>();
            }
        }

        public class MyMultiTenantThing
        {
            public int Id { get; set; }
        }

        // ReSharper disable once MemberCanBePrivate.Global
        public class MyThing
        {
            public int Id { get; set; }
        }

        // ReSharper disable once MemberCanBePrivate.Global
        public class MyMultiTenantChildThing : MyMultiTenantThing
        {
        
        }
        
        private readonly SqliteConnection _connection = new SqliteConnection("DataSource=:memory:");

        public void Dispose()
        {
            _connection.Dispose();
        }

        private DbContext GetDbContext()
        {
            _connection.Open(); 
            var options = new DbContextOptionsBuilder()
                .UseSqlite(_connection)
                .Options;
            return new TestDbContext(options);
        }

        [Fact]
        public void ReturnTrueOnIsMultiTenantOnIfMultiTenant()
        {
            var db = GetDbContext();

            Assert.True(db.Model.FindEntityType(typeof(MyMultiTenantThing)).IsMultiTenant());
        }
        
        [Fact]
        public void ReturnTrueOnIsMultiTenantOnIfAncestorIsMultiTenant()
        {
            var db = GetDbContext();

            Assert.True(db.Model.FindEntityType(typeof(MyMultiTenantChildThing)).IsMultiTenant());
        }

        [Fact]
        public void ReturnFalseOnIsMultiTenantOnIfNotMultiTenant()
        {
            var db = GetDbContext();

            Assert.False(db.Model.FindEntityType(typeof(MyThing)).IsMultiTenant());
        }
    }
}