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

using Finbuckle.MultiTenant;
using Finbuckle.MultiTenant.EntityFrameworkCore;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace ModelBuiikderExtensionShould
{
    public class TestDbContext : DbContext
    {
        public TestDbContext(DbContextOptions options) : base(options)
        {
        }

        DbSet<MyMultiTenantThing> MyMultiTenantThing { get; set; }
        DbSet<MyThing> MyThing { get; set; }

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

        [Fact]
        public void DoNotSetMultiTenantOnTypeWithoutMultiTenantAttribute()
        {
            var db = GetDbContext();

            Assert.False(db.Model.FindEntityType(typeof(MyThing)).IsMultiTenant());
        }
    }
}