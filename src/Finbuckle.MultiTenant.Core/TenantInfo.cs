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

using System.Collections.Generic;
using Finbuckle.MultiTenant.Core;

namespace Finbuckle.MultiTenant
{
    public class TenantInfo
    {
        private string id;

        public TenantInfo()
        {
        }

        public TenantInfo(string id, string identifier, string name, string connectionString, IDictionary<string, object> items)
        {
            Id = id;
            Identifier = identifier;
            Name = name;
            ConnectionString = connectionString;
            if (items != null)
            {
                Items = items;
            }
        }

        public string Id
        {
            get
            {
                return id;
            }
            internal set
            {
                if (value != null)
                {
                    if (value.Length > Constants.TenantIdMaxLength)
                    {
                        throw new MultiTenantException($"The tenant id cannot be null or exceed {Constants.TenantIdMaxLength} characters.");
                    }
                    id = value;
                }
            }
        }

        public string Identifier { get; internal set; }
        public string Name { get; internal set; }
        public string ConnectionString { get; internal set; }
        public IDictionary<string, object> Items { get; internal set; } = new Dictionary<string, object>();
        public IMultiTenantContext MultiTenantContext { get; internal set; }
    }
}