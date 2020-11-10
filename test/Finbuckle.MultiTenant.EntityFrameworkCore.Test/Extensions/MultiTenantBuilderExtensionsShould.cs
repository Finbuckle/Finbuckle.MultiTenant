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

using Microsoft.Extensions.DependencyInjection;
using Xunit;
using Finbuckle.MultiTenant;
using Finbuckle.MultiTenant.Stores;
using Microsoft.EntityFrameworkCore;
using Microsoft.Data.Sqlite;
using System.Data.Common;
using EFCoreStoreShould;

namespace MultiTenantBuilderExtensionsShould
{
    public class MultiTenantBuilderExtensionsShould
    {
        private DbContextOptions _options;
        private DbConnection _connection;

        public MultiTenantBuilderExtensionsShould()
        {
            _connection = new SqliteConnection("DataSource=:memory:");
            _options = new DbContextOptionsBuilder()
                    .UseSqlite(_connection)
                    .Options;

        }

        [Fact]
        public void AddEFCoreStore()
        {
            var services = new ServiceCollection();
            var builder = new FinbuckleMultiTenantBuilder<TenantInfo>(services);
            builder.WithStaticStrategy("initech").WithEFCoreStore<TestEFCoreStoreDbContext, TenantInfo>();
            var sp = services.BuildServiceProvider().CreateScope().ServiceProvider;

            var resolver = sp.GetRequiredService<IMultiTenantStore<TenantInfo>>();
            Assert.IsType<EFCoreStore<TestEFCoreStoreDbContext, TenantInfo>>(resolver);
        }

        [Fact]
        public void AddEFCoreStoreWithExistingDbContext()
        {
            var services = new ServiceCollection();
            var builder = new FinbuckleMultiTenantBuilder<TenantInfo>(services);
            services.AddDbContext<TestEFCoreStoreDbContext>(o => o.UseSqlite("DataSource=:memory:"));
            builder.WithStaticStrategy("initech").WithEFCoreStore<TestEFCoreStoreDbContext, TenantInfo>();
            var sp = services.BuildServiceProvider().CreateScope().ServiceProvider;

            var resolver = sp.GetRequiredService<IMultiTenantStore<TenantInfo>>();
            Assert.IsType<EFCoreStore<TestEFCoreStoreDbContext, TenantInfo>>(resolver);
        }
    }
}