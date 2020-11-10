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

using System.Collections.Generic;
using System.Data.Common;
using Finbuckle.MultiTenant;
using Finbuckle.MultiTenant.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Xunit;

public class NonDerivedIdenityDbContext : IdentityDbContext, IMultiTenantDbContext
{
    public NonDerivedIdenityDbContext(TenantInfo tenantInfo, DbContextOptions options)
        : base(options)
    {
        TenantInfo = tenantInfo;
    }

    public ITenantInfo TenantInfo { get; }
    public TenantMismatchMode TenantMismatchMode => TenantMismatchMode.Throw;
    public TenantNotSetMode TenantNotSetMode => TenantNotSetMode.Throw;

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.Entity<IdentityUser>().IsMultiTenant();
        builder.Entity<IdentityRole>().IsMultiTenant();
        builder.Entity<IdentityUserLogin<string>>().IsMultiTenant();
    }
}

public class MultiTenantNonDerivedIdentityShould
{
    private DbContextOptions _options;
    private DbConnection _connection;

    public MultiTenantNonDerivedIdentityShould()
    {
        _connection = new SqliteConnection("DataSource=:memory:");
        _options = new DbContextOptionsBuilder()
                .UseSqlite(_connection)
                .Options;
    }

    [Fact]
    public void AdjustUserIndex()
    {
        var tenant1 = new TenantInfo
        {
            Id = "abc",
            Identifier = "abc",
            Name = "abc",
            ConnectionString = "DataSource=testdb.db"
        };
        var c = new NonDerivedIdenityDbContext(tenant1, _options);

        var props = new List<IProperty>();
        props.Add(c.Model.FindEntityType(typeof(IdentityUser)).FindProperty("NormalizedUserName"));
        props.Add(c.Model.FindEntityType(typeof(IdentityUser)).FindProperty("TenantId"));

        var index = c.Model.FindEntityType(typeof(IdentityUser)).FindIndex(props);
        Assert.NotNull(index);
        Assert.True(index.IsUnique);
    }

    [Fact]
    public void AdjustRoleIndex()
    {
        var tenant1 = new TenantInfo
        {
            Id = "abc",
            Identifier = "abc",
            Name = "abc",
            ConnectionString = "DataSource=testdb.db"
        };
        var c = new NonDerivedIdenityDbContext(tenant1, _options);

        var props = new List<IProperty>();
        props.Add(c.Model.FindEntityType(typeof(IdentityRole)).FindProperty("NormalizedName"));
        props.Add(c.Model.FindEntityType(typeof(IdentityRole)).FindProperty("TenantId"));

        var index = c.Model.FindEntityType(typeof(IdentityRole)).FindIndex(props);
        Assert.NotNull(index);
        Assert.True(index.IsUnique);
    }

    [Fact]
    public void AdjustUserLoginKey()
    {
        var tenant1 = new TenantInfo
        {
            Id = "abc",
            Identifier = "abc",
            Name = "abc",
            ConnectionString = "DataSource=testdb.db"
        };
        var c = new NonDerivedIdenityDbContext(tenant1, _options);

        Assert.True(c.Model.FindEntityType(typeof(IdentityUserLogin<string>)).FindProperty("Id").IsPrimaryKey());
    }

    [Fact]
    public void AddUserLoginIndex()
    {
        var tenant1 = new TenantInfo
        {
            Id = "abc",
            Identifier = "abc",
            Name = "abc",
            ConnectionString = "DataSource=testdb.db"
        };
        var c = new NonDerivedIdenityDbContext(tenant1, _options);

        var props = new List<IProperty>();
        props.Add(c.Model.FindEntityType(typeof(IdentityUserLogin<string>)).FindProperty("LoginProvider"));
        props.Add(c.Model.FindEntityType(typeof(IdentityUserLogin<string>)).FindProperty("ProviderKey"));
        props.Add(c.Model.FindEntityType(typeof(IdentityUserLogin<string>)).FindProperty("TenantId"));

        var index = c.Model.FindEntityType(typeof(IdentityUserLogin<string>)).FindIndex(props);
        Assert.NotNull(index);
        Assert.True(index.IsUnique);
    }
}