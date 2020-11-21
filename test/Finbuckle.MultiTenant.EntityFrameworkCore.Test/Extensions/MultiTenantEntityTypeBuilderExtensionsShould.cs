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

namespace MultiTenantEntityTypeBuilderExtensionsShould
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

    public class MyMultiTenantThing
    {
        public int Id { get; set; }
        public string Prop2 { get; set; }
        public double Prop3 { get; set; }
    }

    public class DynamicModelCacheKeyFactory : IModelCacheKeyFactory
    {
        public object Create(DbContext context)
        {
            return new Object(); // Never cache!
        }
    }

    public class MultiTenantEntityTypeBuilderExtensionsShould
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
        public void AdjustUniqueIndexes()
        {
            using (var db = GetDbContext(builder =>
            {
#if NET
                builder.Entity<MyMultiTenantThing>().HasIndex(e => e.Id, nameof(MyMultiTenantThing.Id)).HasDatabaseName(nameof(MyMultiTenantThing.Id) + "DbName").IsUnique();
                builder.Entity<MyMultiTenantThing>().HasIndex(e => e.Prop2, nameof(MyMultiTenantThing.Prop2)).HasDatabaseName(nameof(MyMultiTenantThing.Prop2) + "DbName").IsUnique();
#else
                builder.Entity<MyMultiTenantThing>().HasIndex(e => e.Id).HasName(nameof(MyMultiTenantThing.Id)).IsUnique();
                builder.Entity<MyMultiTenantThing>().HasIndex(e => e.Prop2).HasName(nameof(MyMultiTenantThing.Prop2)).IsUnique();
#endif
                builder.Entity<MyMultiTenantThing>().IsMultiTenant().AdjustUniqueIndexes();
            }))
            {
                var indexes = db.Model.FindEntityType(typeof(MyMultiTenantThing)).GetIndexes().Where(i => i.IsUnique);

                foreach (var index in indexes.Where(i => i.IsUnique))
                {
                    Assert.Contains("TenantId", index.Properties.Select(p => p.Name));
                }
            }
        }

        [Fact]
        public void NotAdjustNonUniqueIndexes()
        {
            using (var db = GetDbContext(builder =>
            {
#if NET
                builder.Entity<MyMultiTenantThing>().HasIndex(e => e.Id, nameof(MyMultiTenantThing.Id)).HasDatabaseName(nameof(MyMultiTenantThing.Id) + "DbName").IsUnique();
                builder.Entity<MyMultiTenantThing>().HasIndex(e => e.Prop2, nameof(MyMultiTenantThing.Prop2)).HasDatabaseName(nameof(MyMultiTenantThing.Prop2) + "DbName");
#else
                builder.Entity<MyMultiTenantThing>().HasIndex(e => e.Id).HasName(nameof(MyMultiTenantThing.Id)).IsUnique();
                builder.Entity<MyMultiTenantThing>().HasIndex(e => e.Prop2).HasName(nameof(MyMultiTenantThing.Prop2));
#endif
                builder.Entity<MyMultiTenantThing>().IsMultiTenant().AdjustUniqueIndexes();
            }))
            {
                var indexes = db.Model.FindEntityType(typeof(MyMultiTenantThing)).GetIndexes().Where(i => i.IsUnique);

                foreach (var index in indexes.Where(i => !i.IsUnique))
                {
                    Assert.DoesNotContain("TenantId", index.Properties.Select(p => p.Name));
                }
            }
        }
    }
}