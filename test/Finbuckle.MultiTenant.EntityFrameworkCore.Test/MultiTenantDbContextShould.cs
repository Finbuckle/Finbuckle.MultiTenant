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
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace MultiTenantDbContextShould
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