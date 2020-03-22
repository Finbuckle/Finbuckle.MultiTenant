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
using System.Threading;
using Finbuckle.MultiTenant;
using Finbuckle.MultiTenant.Options;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Moq;
using Xunit;

public class MultiTenantOptionsManagerShould
{
    public class TestMultiTenantContextAccessor : IMultiTenantContextAccessor
    {
        private readonly MultiTenantContext _context;

        public TestMultiTenantContextAccessor()
        {
            var ti = new TenantInfo("test", null, null, null, null);
            var stratInfo = new StrategyInfo();
            var storeInfo = new StoreInfo();
            _context = new MultiTenantContext();
            _context.TenantInfo = ti;
            _context.StrategyInfo = stratInfo;
            _context.StoreInfo = storeInfo;
        }

        public IMultiTenantContext MultiTenantContext => _context;
    }

    [Theory]
    [InlineData("OptionName1")]
    [InlineData("OptionName2")]
    public void GetOptionByName(string optionName)
    {
        var services = new ServiceCollection();
        services.AddTransient<IMultiTenantContextAccessor, TestMultiTenantContextAccessor>();
        services.AddMultiTenantCore().WithPerTenantOptions<TenantInfo>((o, ti) => o.Id = optionName);
        var sp = services.BuildServiceProvider();

        var manager = sp.GetService<IOptionsSnapshot<TenantInfo>>();
        var result = manager.Get(optionName);

        Assert.IsType<MultiTenantOptionsManager<TenantInfo>>(manager);
        Assert.Equal(optionName, result.Id);
    }

    [Fact]
    public void GetOptionByDefaultNameIfNameNull()
    {
        var services = new ServiceCollection();
        services.AddTransient<IMultiTenantContextAccessor, TestMultiTenantContextAccessor>();
        services.AddMultiTenantCore().WithPerTenantOptions<TenantInfo>((o, ti) => o.Id = "test");
        var sp = services.BuildServiceProvider();

        var manager = sp.GetService<IOptionsSnapshot<TenantInfo>>();
        var result = manager.Get(null);

        Assert.IsType<MultiTenantOptionsManager<TenantInfo>>(manager);
        Assert.Equal("test", result.Id);
    }

    [Fact]
    public void GetOptionByDefaultNameIfGettingValueProp()
    {
        var services = new ServiceCollection();
        services.AddTransient<IMultiTenantContextAccessor, TestMultiTenantContextAccessor>();
        services.AddMultiTenantCore().WithPerTenantOptions<TenantInfo>((o, ti) => o.Id = "test");
        var sp = services.BuildServiceProvider();

        var manager = sp.GetService<IOptionsSnapshot<TenantInfo>>();
        var result = manager.Value;

        Assert.IsType<MultiTenantOptionsManager<TenantInfo>>(manager);
        Assert.Equal("test", result.Id);
    }

    [Fact]
    public void ClearCacheOnReset()
    {
        var services = new ServiceCollection();
        services.AddTransient<IMultiTenantContextAccessor, TestMultiTenantContextAccessor>();
        services.AddMultiTenantCore().WithPerTenantOptions<TenantInfo>((o, ti) => o.Id = DateTime.Now.ToLongTimeString());
        var sp = services.BuildServiceProvider();

        var manager = sp.GetService<IOptionsSnapshot<TenantInfo>>();
        var result1 = manager.Value;

        (manager as MultiTenantOptionsManager<TenantInfo>).Reset();
        Thread.Sleep(100);
        var result2 = manager.Value;

        Assert.IsType<MultiTenantOptionsManager<TenantInfo>>(manager);
        Assert.NotEqual(result1, result2);
    }
}