//    Copyright 2018-2020 Andrew White
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
using Finbuckle.MultiTenant.Options;
using Microsoft.Extensions.Options;
using Moq;
using Xunit;

public class MultiTenantOptionsManagerShould
{
    [Theory]
    [InlineData("OptionName1")]
    [InlineData("OptionName2")]
    public void GetOptionByName(string optionName)
    {
        var mock = new Mock<IOptionsMonitorCache<Object>>();
        mock.Setup(c => c.GetOrAdd(It.IsAny<string>(), It.IsAny<Func<Object>>())).Returns(new Object());

        var manager = new MultiTenantOptionsManager<Object>(null, mock.Object);

        manager.Get(optionName);

        mock.Verify(c => c.GetOrAdd(It.Is<String>(p => p == optionName), It.IsAny<Func<Object>>()), Times.Once);
    }

    [Fact]
    public void GetOptionByDefaultNameIfNameNull()
    {
        var mock = new Mock<IOptionsMonitorCache<Object>>();
        mock.Setup(c => c.GetOrAdd(It.IsAny<string>(), It.IsAny<Func<Object>>())).Returns(new Object());

        var manager = new MultiTenantOptionsManager<Object>(null, mock.Object);

        manager.Get(null);

        mock.Verify(c => c.GetOrAdd(It.Is<String>(p => p == Options.DefaultName), It.IsAny<Func<Object>>()), Times.Once);
    }

    [Fact]
    public void GetOptionByDefaultNameIfGettingValueProp()
    {
        var mock = new Mock<IOptionsMonitorCache<Object>>();
        mock.Setup(c => c.GetOrAdd(It.IsAny<string>(), It.IsAny<Func<Object>>())).Returns(new Object());

        var manager = new MultiTenantOptionsManager<Object>(null, mock.Object);

        var dummy = manager.Value;

        mock.Verify(c => c.GetOrAdd(It.Is<String>(p => p == Options.DefaultName), It.IsAny<Func<Object>>()), Times.Once);
    }

    [Fact]
    public void ClearCacheOnReset()
    {
        var mock = new Mock<TestOptionsCache<Object>>();
        mock.Setup(i => i.Clear());

        var manager = new MultiTenantOptionsManager<Object>(null, mock.Object);
        manager.Reset();

        mock.Verify(i => i.Clear(), Times.Once);
    }

    public class TestOptionsCache<TOptions> : IOptionsMonitorCache<TOptions> where TOptions : class
    {
        public virtual void Clear()
        {
            throw new NotImplementedException();
        }

        public virtual TOptions GetOrAdd(string name, Func<TOptions> createOptions)
        {
            throw new NotImplementedException();
        }

        public virtual bool TryAdd(string name, TOptions options)
        {
            throw new NotImplementedException();
        }

        public virtual bool TryRemove(string name)
        {
            throw new NotImplementedException();
        }
    }
}