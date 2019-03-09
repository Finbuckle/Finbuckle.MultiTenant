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
using Finbuckle.MultiTenant;
using Finbuckle.MultiTenant.Core;
using Finbuckle.MultiTenant.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Xunit;

public class MultiTenantModelCacheKeyFactoryShould
{
    public class TestDbContext : DbContext{}
    public class TestMultiTenantDbContext : MultiTenantDbContext
    {
        public TestMultiTenantDbContext(TenantInfo tenantInfo, DbContextOptions options) : base(tenantInfo, options)
        {
        }
    }

    [Fact]
    public void ReturnTypeForNonMultiTenantDbContext()
    {
        var factory = new MultiTenantModelCacheKeyFactory();
        var dbContext = new TestDbContext();

        var key = factory.Create(dbContext);

        Assert.Equal(typeof(TestDbContext), key);
    }

    [Fact]
    public void ReturnTypePlusTenantIdForMultiTenantDbContext()
    {
        var factory = new MultiTenantModelCacheKeyFactory();
        var dbContext = new TestMultiTenantDbContext(
            new TenantInfo("test", null, null, null, null),
            new DbContextOptions<TestMultiTenantDbContext>());

        dynamic key = factory.Create(dbContext);

        Assert.IsType<(Type, string)>(key);
        Assert.Equal(typeof(TestMultiTenantDbContext), key.Item1);
        Assert.Equal("test", key.Item2);
    }

    [Fact]
    public void ReturnTypePlusTenantIdForMultiTenantIdentityDbContext()
    {
        var factory = new MultiTenantModelCacheKeyFactory();
        var dbContext = new TestIdentityDbContext(
            new TenantInfo("test", null, null, null, null),
            new DbContextOptions<TestIdentityDbContext>());

        dynamic key = factory.Create(dbContext);

        Assert.IsType<(Type, string)>(key);
        Assert.Equal(typeof(TestIdentityDbContext), key.Item1);
        Assert.Equal("test", key.Item2);
    }

    [Fact]
    public void ReturnTypePlusTenantIdForMultiTenantIdentityDbContext_TUser()
    {
        // Test that it works for the MultiTenantIdentityDbContext<TUser>
        var factory = new MultiTenantModelCacheKeyFactory();
        var dbContext = new TestIdentityDbContext_TUser(
            new TenantInfo("test", null, null, null, null),
            new DbContextOptions<TestIdentityDbContext_TUser>());

        dynamic key = factory.Create(dbContext);

        Assert.IsType<(Type, string)>(key);
        Assert.Equal(typeof(TestIdentityDbContext_TUser), key.Item1);
        Assert.Equal("test", key.Item2);
    }

    [Fact]
    public void ReturnTypePlusTenantIdForMultiTenantIdentityDbContext_TUser_TRole_String()
    {
        var factory = new MultiTenantModelCacheKeyFactory();
        var dbContext = new TestIdentityDbContext_TUser_TRole_String(
            new TenantInfo("test", null, null, null, null),
            new DbContextOptions<TestIdentityDbContext_TUser_TRole_String>());

        dynamic key = factory.Create(dbContext);

        Assert.IsType<(Type, string)>(key);
        Assert.Equal(typeof(TestIdentityDbContext_TUser_TRole_String), key.Item1);
        Assert.Equal("test", key.Item2);
    }

    [Fact]
    public void ReturnTypePlusTenantIdForMultiTenantIdentityDbContext_AllGenericParams()
    {
        var factory = new MultiTenantModelCacheKeyFactory();
        var dbContext = new TestIdentityDbContext_AllGenericParams(
            new TenantInfo("test", null, null, null, null),
            new DbContextOptions<TestIdentityDbContext_AllGenericParams>());

        dynamic key = factory.Create(dbContext);

        Assert.IsType<(Type, string)>(key);
        Assert.Equal(typeof(TestIdentityDbContext_AllGenericParams), key.Item1);
        Assert.Equal("test", key.Item2);
    }
}