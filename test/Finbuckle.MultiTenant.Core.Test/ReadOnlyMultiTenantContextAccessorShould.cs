//    Copyright 2020 Andrew White
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
using Finbuckle.MultiTenant;
using Moq;
using Xunit;

public class ReadOnlyMultiTenantContextAccessorShould
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

    [Fact]
    public void GetReadOnlyFromMultiTenantContextAccessor()
    {
        var accessor = new TestMultiTenantContextAccessor();
        var @readonly = new ReadOnlyMultiTenantContextAccessor(accessor);
        var result = @readonly.MultiTenantContext;

        Assert.NotNull(result);
        Assert.IsType<ReadOnlyMultiTenantContext>(result);
        Assert.Same(accessor.MultiTenantContext.TenantInfo, result.TenantInfo);
        Assert.Same(accessor.MultiTenantContext.StrategyInfo, result.StrategyInfo);
        Assert.Same(accessor.MultiTenantContext.StoreInfo, result.StoreInfo);
    }

    [Fact]
    public void ThrowIfNullCtorParam()
    {
        Assert.Throws<ArgumentNullException>(() => new ReadOnlyMultiTenantContextAccessor(null));
    }
}