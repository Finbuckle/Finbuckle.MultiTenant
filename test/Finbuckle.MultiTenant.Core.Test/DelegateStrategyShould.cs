//    Copyright 2019 Andrew White
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
using System.Collections.Concurrent;
using System.Threading.Tasks;
using Finbuckle.MultiTenant;
using Finbuckle.MultiTenant.Core;
using Finbuckle.MultiTenant.Strategies;
using Xunit;

public class DelegateStrategyShould
{
    [Fact]
    public void CallDelegate()
    {
        int i = 0;
        var strategy = new DelegateStrategy(c => Task.FromResult((i++).ToString()));
        strategy.GetIdentifierAsync(new object()).Wait();

        Assert.Equal(1, i);
    }

    [Fact]
    public async Task ReturnDelegateResult()
    {
        var strategy = new DelegateStrategy(c => Task.FromResult("test_test"));
        var result = await strategy.GetIdentifierAsync(new object());

        Assert.Equal("test_test", result);
    }
    
    [Fact]
    public void ThrowIfNullDelegate()
    {
        Assert.Throws<ArgumentNullException>(() => new DelegateStrategy(null));
    }
}