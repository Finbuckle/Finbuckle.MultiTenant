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
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using Finbuckle.MultiTenant;
using Finbuckle.MultiTenant.Core;
using Finbuckle.MultiTenant.EntityFrameworkCore;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Xunit;

public class MultiTenantIdentityDbContextShould
{
    private DbContextOptions _options;
    private DbConnection _connection;

    public MultiTenantIdentityDbContextShould()
    {
        _connection = new SqliteConnection("DataSource=:memory:");
        _options = new DbContextOptionsBuilder()
                .UseSqlite(_connection)
                .Options;

    }

    [Fact]
    public void WorkWithSingleParamCtor()
    {
        var tenant1 = new TenantInfo("abc", "abc", "abc",
            "DataSource=testdb.db", null);
        var c = new TestIdentityDbContext(tenant1);

        Assert.NotNull(c);
    }

    [Fact]
    public void AdjustUserIndex()
    {
        var tenant1 = new TenantInfo("abc", "abc", "abc",
            "DataSource=testdb.db", null);
        var c = new TestIdentityDbContext(tenant1, _options);

        var props = new List<IProperty>();
        props.Add(c.Model.FindEntityType(typeof(MultiTenantIdentityUser)).FindProperty("NormalizedUserName"));
        props.Add(c.Model.FindEntityType(typeof(MultiTenantIdentityUser)).FindProperty("TenantId"));

        var index = c.Model.FindEntityType(typeof(MultiTenantIdentityUser)).FindIndex(props);
        Assert.NotNull(index);
        Assert.True(index.IsUnique);
    }

    [Fact]
    public void AdjustRoleIndex()
    {
        var tenant1 = new TenantInfo("abc", "abc", "abc",
            "DataSource=testdb.db", null);
        var c = new TestIdentityDbContext(tenant1, _options);

        var props = new List<IProperty>();
        props.Add(c.Model.FindEntityType(typeof(MultiTenantIdentityRole)).FindProperty("NormalizedName"));
        props.Add(c.Model.FindEntityType(typeof(MultiTenantIdentityRole)).FindProperty("TenantId"));

        var index = c.Model.FindEntityType(typeof(MultiTenantIdentityRole)).FindIndex(props);
        Assert.NotNull(index);
        Assert.True(index.IsUnique);
    }

    [Fact]
    public void AdjustUserLoginKey()
    {
        var tenant1 = new TenantInfo("abc", "abc", "abc",
            "DataSource=testdb.db", null);
        var c = new TestIdentityDbContext(tenant1, _options);

        Assert.True(c.Model.FindEntityType(typeof(MultiTenantIdentityUserLogin<string>)).FindProperty("Id").IsPrimaryKey());
    }

    [Fact]
    public void AddUserLoginIndex()
    {
        var tenant1 = new TenantInfo("abc", "abc", "abc",
            "DataSource=testdb.db", null);
        var c = new TestIdentityDbContext(tenant1, _options);

        var props = new List<IProperty>();
        props.Add(c.Model.FindEntityType(typeof(MultiTenantIdentityUserLogin<string>)).FindProperty("LoginProvider"));
        props.Add(c.Model.FindEntityType(typeof(MultiTenantIdentityUserLogin<string>)).FindProperty("ProviderKey"));
        props.Add(c.Model.FindEntityType(typeof(MultiTenantIdentityUserLogin<string>)).FindProperty("TenantId"));

        var index = c.Model.FindEntityType(typeof(MultiTenantIdentityUserLogin<string>)).FindIndex(props);
        Assert.NotNull(index);
        Assert.True(index.IsUnique);
    }
}