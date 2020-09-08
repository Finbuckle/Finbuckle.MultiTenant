// Copyright 2018-2020 Andrew White
// 
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
// 
//     http://www.apache.org/licenses/LICENSE-2.0
// 
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

using System.Data.Common;
using System.Linq;
using Finbuckle.MultiTenant;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace IMultiTenantDbContextExtensionShould
{
    public class IMultiTenantDbContextExtensionShould
    {
        private DbContextOptions _options;
        private DbConnection _connection;

        public IMultiTenantDbContextExtensionShould()
        {
            _connection = new SqliteConnection("DataSource=:memory:");
            _options = new DbContextOptionsBuilder()
                    .UseSqlite(_connection)
                    .Options;
        }

        [Fact]
        public void HandleTenantNotSetWhenAdding()
        {
            try
            {
                _connection.Open();
                var tenant1 = new TenantInfo
                {
                    Id = "abc",
                    Identifier = "abc",
                    Name = "abc",
                    ConnectionString = "DataSource=testdb.db"
                };

                // TenantNotSetMode.Throw, should act as Overwrite when adding
                using (var db = new TestBlogDbContext(tenant1, _options))
                {
                    db.Database.EnsureDeleted();
                    db.Database.EnsureCreated();
                    db.TenantNotSetMode = TenantNotSetMode.Throw;

                    var blog1 = new Blog { Title = "abc" };
                    db.Blogs.Add(blog1);
                    db.SaveChanges();
                    Assert.Equal(tenant1.Identifier, db.Entry<Blog>(blog1).Property("TenantId").CurrentValue);
                }

                // TenantNotSetMode.Overwrite
                using (var db = new TestBlogDbContext(tenant1, _options))
                {
                    db.Database.EnsureDeleted();
                    db.Database.EnsureCreated();
                    db.TenantNotSetMode = TenantNotSetMode.Overwrite;

                    var blog1 = new Blog { Title = "abc2" };
                    db.Blogs.Add(blog1);
                    db.SaveChanges();
                    Assert.Equal(tenant1.Id, db.Entry<Blog>(blog1).Property("TenantId").CurrentValue);
                }
            }
            finally
            {
                _connection.Close();
            }
        }

        [Fact]
        public void HandleTenantMismatchWhenAdding()
        {
            try
            {
                _connection.Open();
                var tenant1 = new TenantInfo
                {
                    Id = "abc",
                    Identifier = "abc",
                    Name = "abc",
                    ConnectionString = "DataSource=testdb.db"
                };

                // TenantMismatchMode.Throw
                using (var db = new TestBlogDbContext(tenant1, _options))
                {
                    db.Database.EnsureDeleted();
                    db.Database.EnsureCreated();
                    db.TenantMismatchMode = TenantMismatchMode.Throw;

                    var blog1 = new Blog { Title = "abc" };
                    db.Blogs.Add(blog1);
                    db.Entry(blog1).Property("TenantId").CurrentValue = "77";

                    var e = Assert.Throws<MultiTenantException>(() => db.SaveChanges());
                }

                // TenantMismatchMode.Ignore 
                using (var db = new TestBlogDbContext(tenant1, _options))
                {
                    db.Database.EnsureDeleted();
                    db.Database.EnsureCreated();
                    db.TenantMismatchMode = TenantMismatchMode.Ignore;

                    var blog1 = new Blog { Title = "34" };
                    db.Blogs.Add(blog1);
                    db.Entry(blog1).Property("TenantId").CurrentValue = "34";
                    db.SaveChanges();
                    Assert.Equal("34", db.Entry(blog1).Property("TenantId").CurrentValue);
                }

                // TenantMismatchMode.Overwrite
                using (var db = new TestBlogDbContext(tenant1, _options))
                {
                    db.Database.EnsureDeleted();
                    db.Database.EnsureCreated();
                    db.TenantMismatchMode = TenantMismatchMode.Overwrite;

                    var blog1 = new Blog { Title = "77" };
                    db.Blogs.Add(blog1);
                    db.Entry(blog1).Property("TenantId").CurrentValue = "77";
                    db.SaveChanges();
                    Assert.Equal(tenant1.Id, db.Entry(blog1).Property("TenantId").CurrentValue);
                }
            }
            finally
            {
                _connection.Close();
            }
        }

        [Fact]
        public void HandleTenantNotSetWhenUpdating()
        {
            try
            {
                _connection.Open();
                var tenant1 = new TenantInfo
                {
                    Id = "abc",
                    Identifier = "abc",
                    Name = "abc",
                    ConnectionString = "DataSource=testdb.db"
                };

                // TenantNotSetMode.Throw
                using (var db = new TestBlogDbContext(tenant1, _options))
                {
                    db.Database.EnsureDeleted();
                    db.Database.EnsureCreated();

                    var blog1 = new Blog { Title = "abc" };
                    db.Blogs.Add(blog1);
                    db.SaveChanges();

                    db.TenantNotSetMode = TenantNotSetMode.Throw;
                    db.Entry(blog1).Property("TenantId").CurrentValue = null;

                    var e = Assert.Throws<MultiTenantException>(() => db.SaveChanges());
                }

                // TenantNotSetMode.Overwrite
                using (var db = new TestBlogDbContext(tenant1, _options))
                {
                    db.Database.EnsureDeleted();
                    db.Database.EnsureCreated();

                    var blog1 = new Blog { Title = "abc12" };
                    db.Blogs.Add(blog1);
                    db.SaveChanges();

                    db.TenantNotSetMode = TenantNotSetMode.Overwrite;
                    db.Entry(blog1).Property("TenantId").CurrentValue = null;
                    db.SaveChanges();

                    Assert.Equal(tenant1.Id, db.Entry(blog1).Property("TenantId").CurrentValue);
                }
            }
            finally
            {
                _connection.Close();
            }
        }

        [Fact]
        public void HandleTenantMismatchWhenUpdating()
        {
            try
            {
                _connection.Open();
                var tenant1 = new TenantInfo
                {
                    Id = "abc",
                    Identifier = "abc",
                    Name = "abc",
                    ConnectionString = "DataSource=testdb.db"
                };

                // TenantMismatchMode.Throw
                using (var db = new TestBlogDbContext(tenant1, _options))
                {
                    db.Database.EnsureDeleted();
                    db.Database.EnsureCreated();

                    var blog1 = new Blog { Title = "abc" };
                    db.Blogs.Add(blog1);
                    db.SaveChanges();

                    db.TenantMismatchMode = TenantMismatchMode.Throw;
                    db.Entry(blog1).Property("TenantId").CurrentValue = "11";

                    var e = Assert.Throws<MultiTenantException>(() => db.SaveChanges());
                }

                // TenantMismatchMode.Ignore
                using (var db = new TestBlogDbContext(tenant1, _options))
                {
                    db.Database.EnsureDeleted();
                    db.Database.EnsureCreated();

                    var blog1 = new Blog { Title = "abc" };
                    db.Blogs.Add(blog1);
                    db.SaveChanges();

                    db.TenantMismatchMode = TenantMismatchMode.Ignore;
                    db.Entry(blog1).Property("TenantId").CurrentValue = "11";
                    db.SaveChanges();

                    Assert.Equal("11", db.Entry(blog1).Property("TenantId").CurrentValue);
                }

                // TenantMismatchMode.Overwrite
                using (var db = new TestBlogDbContext(tenant1, _options))
                {
                    db.Database.EnsureDeleted();
                    db.Database.EnsureCreated();

                    var blog1 = new Blog { Title = "abc12" };
                    db.Blogs.Add(blog1);
                    db.SaveChanges();

                    db.TenantMismatchMode = TenantMismatchMode.Overwrite;
                    db.Entry(blog1).Property("TenantId").CurrentValue = "11";
                    db.SaveChanges();

                    Assert.Equal(tenant1.Id, db.Entry(blog1).Property("TenantId").CurrentValue);
                }
            }
            finally
            {
                _connection.Close();
            }
        }

        [Fact]
        public void HandleTenantNotSetWhenDeleting()
        {
            try
            {
                _connection.Open();
                var tenant1 = new TenantInfo
                {
                    Id = "abc",
                    Identifier = "abc",
                    Name = "abc",
                    ConnectionString = "DataSource=testdb.db"
                };

                // TenantNotSetMode.Throw
                using (var db = new TestBlogDbContext(tenant1, _options))
                {
                    db.Database.EnsureDeleted();
                    db.Database.EnsureCreated();

                    var blog1 = new Blog { Title = "abc" };
                    db.Blogs.Add(blog1);
                    db.SaveChanges();

                    db.TenantNotSetMode = TenantNotSetMode.Throw;
                    db.Entry(blog1).Property("TenantId").CurrentValue = null;
                    db.Blogs.Remove(blog1);

                    var e = Assert.Throws<MultiTenantException>(() => db.SaveChanges());
                }

                // TenantNotSetMode.Overwrite
                using (var db = new TestBlogDbContext(tenant1, _options))
                {
                    db.Database.EnsureDeleted();
                    db.Database.EnsureCreated();

                    var blog1 = new Blog { Title = "abc" };
                    db.Blogs.Add(blog1);
                    db.SaveChanges();

                    db.TenantNotSetMode = TenantNotSetMode.Overwrite;
                    db.Entry(blog1).Property("TenantId").CurrentValue = null;
                    db.Blogs.Remove(blog1);

                    Assert.Equal(1, db.SaveChanges());
                }
            }
            finally
            {
                _connection.Close();
            }
        }

        [Fact]
        public void HandleTenantMismatchWhenDeleting()
        {
            try
            {
                _connection.Open();
                var tenant1 = new TenantInfo
                {
                    Id = "abc",
                    Identifier = "abc",
                    Name = "abc",
                    ConnectionString = "DataSource=testdb.db"
                };

                // TenantMismatchMode.Throw
                using (var db = new TestBlogDbContext(tenant1, _options))
                {
                    db.Database.EnsureDeleted();
                    db.Database.EnsureCreated();

                    var blog1 = new Blog { Title = "abc" };
                    db.Blogs.Add(blog1);
                    db.SaveChanges();

                    db.TenantMismatchMode = TenantMismatchMode.Throw;
                    db.Entry(blog1).Property("TenantId").CurrentValue = "17";
                    db.Blogs.Remove(blog1);

                    var e = Assert.Throws<MultiTenantException>(() => db.SaveChanges());
                }

                // TenantMismatchMode.Ignore
                using (var db = new TestBlogDbContext(tenant1, _options))
                {
                    db.TenantMismatchMode = TenantMismatchMode.Ignore;
                    var blog1 = db.Blogs.First();
                    db.Entry(blog1).Property("TenantId").CurrentValue = "17";
                    db.Blogs.Remove(blog1);

                    Assert.Equal(1, db.SaveChanges());
                }

                // TenantMismatchMode.Overwrite
                using (var db = new TestBlogDbContext(tenant1, _options))
                {
                    db.Database.EnsureDeleted();
                    db.Database.EnsureCreated();

                    var blog1 = new Blog { Title = "abc" };
                    db.Blogs.Add(blog1);
                    db.SaveChanges();

                    db.TenantMismatchMode = TenantMismatchMode.Overwrite;
                    db.Entry(blog1).Property("TenantId").CurrentValue = "17";
                    db.Blogs.Remove(blog1);

                    Assert.Equal(1, db.SaveChanges());
                }
            }
            finally
            {
                _connection.Close();
            }
        }
    }
}