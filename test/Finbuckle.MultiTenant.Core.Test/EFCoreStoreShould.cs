//    Copyright 2018 Andrew White
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
using System.Data.Common;
using Finbuckle.MultiTenant;
using Finbuckle.MultiTenant.Stores;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Xunit;

public class EFCoreStoreShould : IMultiTenantStoreTestBase<EFCoreStore<TestEFCoreDbContext, TestTenantInfoEntity>>
{
    protected override IMultiTenantStore CreateTestStore()
    {
        var connection = new SqliteConnection("DataSource=:memory:");
        connection.Open();
        var options = new DbContextOptionsBuilder()
                .UseSqlite(connection)
                .Options;
        var dbContext = new TestEFCoreDbContext(options);
        dbContext.Database.EnsureCreated();
        
        var store = new MultiTenantStoreWrapper<EFCoreStore<TestEFCoreDbContext, TestTenantInfoEntity>>
            (new EFCoreStore<TestEFCoreDbContext, TestTenantInfoEntity>(dbContext), null);

        return PopulateTestStore(store);
    }

    // Note, basic store functionality tested in MultiTenantStoreWrapperShould.cs

    // TODO Test property constraints
}