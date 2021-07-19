//    Copyright 2018-2020 Finbuckle LLC, Andrew White, and Contributors
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
using Microsoft.AspNetCore.Identity;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using MultiTenantEntityTypeBuilderShould;
using Xunit;

namespace Finbuckle.MultiTenant.EntityFrameworkCore.Test.Extensions
{
    public class EntityTypeBuilderExtensionsShould
    {
        public class TestIdentityDbContext : MultiTenantIdentityDbContext
        {
            public TestIdentityDbContext(TenantInfo tenantInfo, DbContextOptions options)
                : base(tenantInfo, options)
            {
            }
        }
        
        public class TestDbContext : DbContext
        {
            private readonly Action<ModelBuilder> config;

            public TestDbContext(Action<ModelBuilder> config, DbContextOptions options) : base(options)
            {
                this.config = config;
            }

            DbSet<MyMultiTenantThing> MyMultiTenantThing { get; set; }
            DbSet<MyThingWithTenantId> MyThingWithTenantId { get; set; }
            DbSet<MyThingWithIntTenantId> MyThingWithIntTenantId { get; set; }
            DbSet<MyMultiTenantThingWithAttribute> MyMultiTenantThingWithAttribute { get; set; }

            protected override void OnModelCreating(ModelBuilder builder)
            {
                // If the test passed in a custom builder use it
                if (config != null)
                    config(builder);
                // Of use the standard builder configuration
                else
                {
                    builder.Entity<MyMultiTenantThing>().IsMultiTenant();
                    builder.Entity<MyThingWithTenantId>().IsMultiTenant();
                    
                    // for MyMultiTenantThingWithAttribute
                    builder.ConfigureMultiTenant();
                }
            }
        }

        public class MyMultiTenantThing
        {
            public int Id { get; set; }
        }

        [MultiTenant]
        public class MyMultiTenantThingWithAttribute
        {
            public int Id { get; set; }
        }

        public class MyThingWithTenantId
        {
            public int Id { get; set; }
            public string TenantId { get; set; }
        }

        public class MyThingWithIntTenantId
        {
            public int Id { get; set; }
            public int TenantId { get; set; }
        }
        
        #if NETCOREAPP2_1
        public class DynamicModelCacheKeyFactory : IModelCacheKeyFactory
        {
            public object Create(DbContext context)
            {
                return new Object(); // Never cache!
            }
        }
        #endif
        
        private readonly SqliteConnection _connection = new SqliteConnection("DataSource=:memory:");

        public void Dispose()
        {
            _connection.Dispose();
        }

        private DbContext GetDbContext(Action<ModelBuilder> config = null)
        {
            _connection.Open(); 
            var options = new DbContextOptionsBuilder()
                .UseSqlite(_connection)
                #if NETCOREAPP2_1
                .ReplaceService<IModelCacheKeyFactory, DynamicModelCacheKeyFactory>() // needed for testing only
                #endif
                .Options;
            return new TestDbContext(config, options);
        }

        private TestIdentityDbContext GetTestIdentityDbContext(TenantInfo tenant)
        {
            _connection.Open();
            var options = new DbContextOptionsBuilder()
                    .UseSqlite(_connection)
                    .Options;
            return new TestIdentityDbContext(tenant, options);
        }

        [Fact]
        public void SetMultiTenantAnnotation()
        {
            using (var db = GetDbContext())
            {
                var annotation = db.Model.FindEntityType(typeof(MyMultiTenantThing)).FindAnnotation(Constants.MultiTenantAnnotationName);

                Assert.True((bool)annotation.Value);
            }
        }

        [Fact]
        public void AddTenantIdStringShadowProperty()
        {
            using (var db = GetDbContext())
            {
                var prop = db.Model.FindEntityType(typeof(MyMultiTenantThing)).FindProperty("TenantId");

                Assert.Equal(typeof(string), prop.ClrType);
                // IsShadowProperty doesn't work?
                // Assert.True(prop.IsShadowProperty());
                Assert.Null(prop.FieldInfo);
            }
        }

        [Fact]
        public void RespectExistingTenantIdStringProperty()
        {
            using (var db = GetDbContext())
            {
                var prop = db.Model.FindEntityType(typeof(MyThingWithTenantId)).FindProperty("TenantId");

                Assert.Equal(typeof(string), prop.ClrType);
                // TODO: IsShadowProperty doesn't work?
                // Assert.False(prop.IsShadowProperty());
                Assert.NotNull(prop.FieldInfo);
            }
        }

        [Fact]
        public void ThrowOnNonStringExistingTenantIdProperty()
        {
            using (var db = GetDbContext(b => b.Entity<MyThingWithIntTenantId>().IsMultiTenant()))
            {
                Assert.Throws<MultiTenantException>(() => db.Model);
            }
        }

        [Fact]
        public void SetsTenantIdStringMaxLength()
        {
            using (var db = GetDbContext())
            {
                var prop = db.Model.FindEntityType(typeof(MyMultiTenantThing)).FindProperty("TenantId");

                Assert.Equal(Finbuckle.MultiTenant.Internal.Constants.TenantIdMaxLength, prop.GetMaxLength());
            }
        }

        [Fact]
        public void SetGlobalFilterQuery()
        {
            // Doesn't appear to be a way to test this except to try it out...
            try
            {
                _connection.Open();
                var options = new DbContextOptionsBuilder()
                    .UseSqlite(_connection)
                    .Options;
                var tenant1 = new TenantInfo
                {
                    Id = "abc",
                    Identifier = "abc",
                    Name = "abc",
                    ConnectionString = "DataSource=testDb.db"
                };

                using (var db = new TestBlogDbContext(tenant1, options))
                {
                    db.Database.EnsureDeleted();
                    db.Database.EnsureCreated();

                    var blog1 = new Blog { Title = "abc" };
                    db.Blogs.Add(blog1);
                    var post1 = new Post { Title = "post in abc", Blog = blog1 };
                    db.Posts.Add(post1);
                    db.SaveChanges();
                }

                var tenant2 = new TenantInfo
                {
                    Id = "123",
                    Identifier = "123",
                    Name = "123",
                    ConnectionString = "DataSource=testDb.db"
                };
                using (var db = new TestBlogDbContext(tenant2, options))
                {
                    var blog1 = new Blog { Title = "123" };
                    db.Blogs.Add(blog1);
                    var post1 = new Post { Title = "post in 123", Blog = blog1 };
                    db.Posts.Add(post1);
                    db.SaveChanges();
                }

                int postCount1 = 0;
                int postCount2 = 0;
                using (var db = new TestBlogDbContext(tenant1, options))
                {
                    postCount1 = db.Posts.Count();
                    postCount2 = db.Posts.IgnoreQueryFilters().Count();
                }

                Assert.Equal(1, postCount1);
                Assert.Equal(2, postCount2);
            }
            finally
            {
                _connection.Close();
            }
        }

        [Fact]
        public void RespectExistingQueryFilter()
        {
            // Doesn't appear to be a way to test this except to try it out...
            var options = new DbContextOptionsBuilder()
                    .UseSqlite(_connection)
                    .Options;
            try
            {
                _connection.Open();
                var tenant1 = new TenantInfo
                {
                    Id = "abc",
                    Identifier = "abc",
                    Name = "abc",
                    ConnectionString = "DataSource=testDb.db"
                };

                using (var db = new TestDbContextWithExistingGlobalFilter(tenant1, options))
                {
                    db.Database.EnsureDeleted();
                    db.Database.EnsureCreated();

                    var blog1 = new Blog { Title = "abc" };
                    db.Blogs.Add(blog1);
                    var post1 = new Post { Title = "post in abc", Blog = blog1 };
                    db.Posts.Add(post1);
                    var post2 = new Post { Title = "Filtered Title", Blog = blog1 };
                    db.Posts.Add(post2);
                    db.SaveChanges();
                }

                int postCount1 = 0;
                int postCount2 = 0;
                using (var db = new TestDbContextWithExistingGlobalFilter(tenant1, options))
                {
                    postCount1 = db.Posts.Count();
                    postCount2 = db.Posts.IgnoreQueryFilters().Count();
                }

                Assert.Equal(1, postCount1);
                Assert.Equal(2, postCount2);
            }
            finally
            {
                _connection.Close();
            }
        }

        // The tests below are Identity specific
        [Fact]
        public void AdjustRoleIndex()
        {
            var tenant1 = new TenantInfo
            {
                Id = "abc",
                Identifier = "abc",
                Name = "abc",
                ConnectionString = "DataSource=testDb.db"
            };

            using (var c = GetTestIdentityDbContext(tenant1))
            {
                var props = new List<IProperty>();
                props.Add(c.Model.FindEntityType(typeof(IdentityRole)).FindProperty("NormalizedName"));
                props.Add(c.Model.FindEntityType(typeof(IdentityRole)).FindProperty("TenantId"));

                var index = c.Model.FindEntityType(typeof(IdentityRole)).FindIndex(props);
                Assert.NotNull(index);
                Assert.True(index.IsUnique);
            }
        }

        [Fact]
        public void AdjustUserLoginKey()
        {
            var tenant1 = new TenantInfo
            {
                Id = "abc",
                Identifier = "abc",
                Name = "abc",
                ConnectionString = "DataSource=testDb.db"
            };

            using (var c = GetTestIdentityDbContext(tenant1))
            {
                Assert.True(c.Model.FindEntityType(typeof(IdentityUserLogin<string>)).FindProperty("Id").IsPrimaryKey());
            }
        }

        [Fact]
        public void AddUserLoginIndex()
        {
            var tenant1 = new TenantInfo
            {
                Id = "abc",
                Identifier = "abc",
                Name = "abc",
                ConnectionString = "DataSource=testDb.db"
            };

            using (var c = GetTestIdentityDbContext(tenant1))
            {
                var props = new List<IProperty>();
                props.Add(c.Model.FindEntityType(typeof(IdentityUserLogin<string>)).FindProperty("LoginProvider"));
                props.Add(c.Model.FindEntityType(typeof(IdentityUserLogin<string>)).FindProperty("ProviderKey"));
                props.Add(c.Model.FindEntityType(typeof(IdentityUserLogin<string>)).FindProperty("TenantId"));

                var index = c.Model.FindEntityType(typeof(IdentityUserLogin<string>)).FindIndex(props);
                Assert.NotNull(index);
                Assert.True(index.IsUnique);
            }
        }

        [Fact]
        public void AdjustUserIndex()
        {
            var tenant1 = new TenantInfo
            {
                Id = "abc",
                Identifier = "abc",
                Name = "abc",
                ConnectionString = "DataSource=testDb.db"
            };

            using (var c = GetTestIdentityDbContext(tenant1))
            {
                var props = new List<IProperty>();
                props.Add(c.Model.FindEntityType(typeof(IdentityUser)).FindProperty("NormalizedUserName"));
                props.Add(c.Model.FindEntityType(typeof(IdentityUser)).FindProperty("TenantId"));

                var index = c.Model.FindEntityType(typeof(IdentityUser)).FindIndex(props);
                Assert.NotNull(index);
                Assert.True(index.IsUnique);
            }
        }
    }
}
