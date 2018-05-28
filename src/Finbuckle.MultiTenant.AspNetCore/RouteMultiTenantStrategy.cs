﻿//    Copyright 2018 Andrew White
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
using System.Threading.Tasks;
using Finbuckle.MultiTenant.Core;
using Finbuckle.MultiTenant.Core.Abstractions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Logging;

namespace Finbuckle.MultiTenant.AspNetCore
{
    public class RouteMultiTenantStrategy : IMultiTenantStrategy
    {
        private readonly string tenantParam;
        private readonly ILogger<RouteMultiTenantStrategy> logger;

        public RouteMultiTenantStrategy(string tenantParam, ILogger<RouteMultiTenantStrategy> logger = null)
        {
            if (string.IsNullOrWhiteSpace(tenantParam))
            {
                throw new MultiTenantException(null, new ArgumentException($"\"{nameof(tenantParam)}\" must not be null or whitespace", nameof(tenantParam)));
            }

            this.tenantParam = tenantParam;
            this.logger = logger;
        }

        public string GetIdentifier(object context)
        {
            if(!typeof(HttpContext).IsAssignableFrom(context.GetType()))
                throw new MultiTenantException(null,
                    new ArgumentException("\"context\" type must be of type HttpContext", nameof(context)));

            object identifier = null;
            (context as HttpContext).GetRouteData()?.Values.TryGetValue(tenantParam, out identifier);

            Utilities.TryLogInfo(logger, $"Found identifier:  \"{identifier ?? "<null>"}\"");
            
            return identifier as string;
        }
    }
}
