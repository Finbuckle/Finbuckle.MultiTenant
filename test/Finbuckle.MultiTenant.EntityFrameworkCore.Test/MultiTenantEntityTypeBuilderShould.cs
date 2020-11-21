//    Copyright 2020 Finbuckle LLC, Andrew White, and Contributors
// 
//    Licensed under the Apache License, Version 2.0 (the "License");
//    you may not use this file except in compliance with the License.
//    You may obtain a copy of the License at
// 
//        http://www.apache.org/licenses/LICENSE-2.0
// 
//    Unless required by applicable law or agreed to in writing, software
//    distributed under the License is distributed on an "AS IS" BASIS,
//    WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//    See the License for the specific language governing permissions and
//    limitations under the License.

using System;
using System.Collections.Generic;
using System.Linq;
using Finbuckle.MultiTenant;
using Finbuckle.MultiTenant.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using MultiTenantIdentityDbContextShould;
using Xunit;

namespace MultiTenantEntityTypeBuilderShould
{
    public class TestDbContext : DbContext
    {
        private readonly Action<ModelBuilder> config;

        public TestDbContext(Action<ModelBuilder> config, DbContextOptions options) : base(options)
        {
            this.config = config;
        }

        DbSet<MyMultiTenantThing> MyMultiTenantThing { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            config(builder);
        }
    }

    [MultiTenant] // Note this is ignored in some tests.
    public class MyMultiTenantThing
    {
        public int Id { get; set; }
        public string Prop2 { get; set; }
    }

    public class DynamicModelCacheKeyFactory : IModelCacheKeyFactory
    {
        public object Create(DbContext context)
        {
            return new Object(); // Never cache!
        }
    }

    public class MultiTenantEntityTypeBuilderShould
    {
        private DbContext GetDbContext(Action<ModelBuilder> config)
        {
            var connection = new SqliteConnection("DataSource=:memory:");
            var options = new DbContextOptionsBuilder()
                .UseSqlite(connection)
                .ReplaceService<IModelCacheKeyFactory, DynamicModelCacheKeyFactory>() // needed for testing only
                .Options;

            var db = new TestDbContext(config, options);

            return db;
        }

        [Fact]
        public void AdjustIndex()
        {
            IMutableIndex origIndex = null;

            using (var db = GetDbContext(builder =>
            {
#if NET
                builder.Entity<MyMultiTenantThing>().HasIndex(e => e.Id, "Id").HasDatabaseName("DbName");
#else
                builder.Entity<MyMultiTenantThing>().HasIndex(e => e.Id).HasName("Id");
#endif
                origIndex = builder.Entity<MyMultiTenantThing>().Metadata.GetIndexes().First();
                builder.Entity<MyMultiTenantThing>().IsMultiTenant().AdjustIndex(origIndex);
            }))
            {

                var index = db.Model.FindEntityType(typeof(MyMultiTenantThing)).GetIndexes().First();
                Assert.NotNull(origIndex);
                Assert.NotSame(origIndex, index);
                Assert.Contains("TenantId", index.Properties.Select(p => p.Name));
#if NET
                Assert.Equal("Id", index.Name);
                Assert.Equal("DbName", index.GetDatabaseName());
#elif NETSTANDARD2_1
                Assert.Equal("Id", index.GetName());
#elif NETSTANDARD2_0
                Assert.Equal("Id", index.Relational.Name);
#endif
            }
        }

        [Fact]
        public void PreserveUniqueness()
        {
            using (var db = GetDbContext(builder =>
            {
                builder.Entity<MyMultiTenantThing>().HasIndex(e => e.Id).IsUnique();
                builder.Entity<MyMultiTenantThing>().HasIndex(e => e.Prop2);

                foreach (var index in builder.Entity<MyMultiTenantThing>().Metadata.GetIndexes().ToList())
                    builder.Entity<MyMultiTenantThing>().IsMultiTenant().AdjustIndex(index);
            }))
            {

                var index = db.Model.FindEntityType(typeof(MyMultiTenantThing)).GetIndexes().Where(i => i.Properties.Select(p => p.Name).Contains("Id")).Single();
                Assert.True(index.IsUnique);
                index = db.Model.FindEntityType(typeof(MyMultiTenantThing)).GetIndexes().Where(i => i.Properties.Select(p => p.Name).Contains("Prop2")).Single();
                Assert.False(index.IsUnique);
            }
        }

        [Fact]
        public void AdjustIndexViaMultiTenantAttribute()
        {
            IMutableIndex origIndex = null;

            using (var db = GetDbContext(builder =>
            {
#if NET
                builder.Entity<MyMultiTenantThing>().HasIndex(e => e.Id, "Id").HasDatabaseName("IdDbName");
                builder.Entity<MyMultiTenantThing>().HasIndex(e => e.Prop2, "Prop2").HasDatabaseName("Prop2DbName").IsUnique();
#else
                builder.Entity<MyMultiTenantThing>().HasIndex(e => e.Id).HasName("Id");
                builder.Entity<MyMultiTenantThing>().HasIndex(e => e.Prop2).HasName("Prop2").IsUnique();
#endif
                origIndex = builder.Entity<MyMultiTenantThing>().Metadata.GetIndexes().First();

                builder.ConfigureMultiTenant();
            }))
            {
                foreach (var index in db.Model.FindEntityType(typeof(MyMultiTenantThing)).GetIndexes())
                {
                    Assert.Contains("TenantId", index.Properties.Select(p => p.Name));
                    var otherProp = index.Properties.Where(p => p.Name != "TenantId").Single();
#if NET
                    Assert.Equal(otherProp.Name, index.Name);
                    Assert.Equal(otherProp.Name + "DbName", index.GetDatabaseName());
#elif NETSTANDARD2_1
                    Assert.Equal(otherProp.Name, index.GetName());
#elif NETSTANDARD2_0
                    Assert.Equal(otherProp.Name, index.Relational.Name);
#endif
                }
            }
        }
    }
}