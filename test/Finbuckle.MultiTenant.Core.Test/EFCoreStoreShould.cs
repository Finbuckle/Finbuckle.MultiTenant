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
using System.Linq;
using Finbuckle.MultiTenant;
using Finbuckle.MultiTenant.Core;
using Finbuckle.MultiTenant.Stores;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Xunit;

public class EFCoreStoreShould : IMultiTenantStoreTestBase<EFCoreStore<TestEFCoreStoreDbContext, TestTenantInfoEntity>>
{
    protected override IMultiTenantStore CreateTestStore()
    {
        var connection = new SqliteConnection("DataSource=:memory:");
        connection.Open();
        var options = new DbContextOptionsBuilder()
                .UseSqlite(connection)
                .Options;
        var dbContext = new TestEFCoreStoreDbContext(options);
        dbContext.Database.EnsureCreated();

        var store = new MultiTenantStoreWrapper<EFCoreStore<TestEFCoreStoreDbContext, TestTenantInfoEntity>>
            (new EFCoreStore<TestEFCoreStoreDbContext, TestTenantInfoEntity>(dbContext), null);

        return PopulateTestStore(store);
    }

    // Note, basic store functionality tested in MultiTenantStoreWrapperShould.cs

    [Fact]
    public void AddTenantIdLengthConstraint()
    {
        var prop = GetModelProperty("Id");
        Assert.Equal(Constants.TenantIdMaxLength, prop.GetMaxLength());
    }

    private static IProperty GetModelProperty(string propName)
    {
        var connection = new SqliteConnection("DataSource=:memory:");
        var options = new DbContextOptionsBuilder()
                .UseSqlite(connection)
                .Options;
        var dbContext = new TestEFCoreStoreDbContext(options);

        var model = dbContext.Model.FindEntityType(typeof(TestTenantInfoEntity));
        var prop = model.GetProperties().Where(p => p.Name == propName).Single();
        return prop;
    }

    [Fact]
    public void AddTenantIdAsKey()
    {
        var prop = GetModelProperty("Id");
        Assert.True(prop.IsPrimaryKey());
    }

    [Fact]
    public void AddIdentifierUniqueConstraint()
    {
        var prop = GetModelProperty("Identifier");
        Assert.True(prop.IsIndex());
    }

    [Fact]
    public void AddNameRequiredConstraint()
    {
        var prop = GetModelProperty("Name");
        Assert.False(prop.IsNullable);
    }

    [Fact]
    public void AddConnectionStringRequiredConstraint()
    {
        var prop = GetModelProperty("ConnectionString");
        Assert.False(prop.IsNullable);
    }
}