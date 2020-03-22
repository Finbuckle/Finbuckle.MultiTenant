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
using Xunit;

public class ReadOnlyMultiTenantContextShould
{
    [Fact]
    public void CopyMultiTenantContext()
    {
        var ti = new TenantInfo("test", null, null, null, null);
        var stratInfo = new StrategyInfo();
        var storeInfo = new StoreInfo();
        var context = new MultiTenantContext();
        context.TenantInfo = ti;
        context.StrategyInfo = stratInfo;
        context.StoreInfo = storeInfo;

        var result = new ReadOnlyMultiTenantContext(context);

        Assert.NotNull(result);
        Assert.Same(ti, result.TenantInfo);
        Assert.Same(stratInfo, result.StrategyInfo);
        Assert.Same(storeInfo, result.StoreInfo);
    }

    [Fact]
    public void ThrowIfMultiTenantContextIsNull()
    {
        Assert.Throws<ArgumentNullException>(() => new ReadOnlyMultiTenantContext(null));
    }
}