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
using System.Linq;
using System.Threading.Tasks;
using Finbuckle.MultiTenant.Core;
using Finbuckle.MultiTenant.Core.Abstractions;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.OAuth;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Finbuckle.MultiTenant.AspNetCore
{
    public interface IRemoteAuthenticationStrategy
    {
    }

    public class RemoteAuthenticationStrategy : IMultiTenantStrategy, IRemoteAuthenticationStrategy
    {
        public virtual string GetIdentifier(object context)
        {
            if(!(context is HttpContext))
                throw new MultiTenantException(null,
                    new ArgumentException("\"context\" type must be of type HttpContext", nameof(context)));

            var httpContext = context as HttpContext;

            var schemes = httpContext.RequestServices.GetRequiredService<IAuthenticationSchemeProvider>();
            var handlers = httpContext.RequestServices.GetRequiredService<IAuthenticationHandlerProvider>();

            foreach (var scheme in schemes.GetRequestHandlerSchemesAsync().Result)
            {
                // Check to see if this handler would apply and resolve tenant context if so.
                // Handlers have a method, ShouldHandleRequestAsync, which would be nice here, but it instantiaties
                // the handler and hence its options (which we should't instantiate without knowing the tenant...)
                // Workaround is to copy the logic from ShouldHandleRequestAsync which requires instantiating options the hard way.

                var optionType = scheme.HandlerType.GetProperty("Options").PropertyType;
                var optionsFactoryType = typeof(IOptionsFactory<>).MakeGenericType(optionType);
                var optionsFactory = httpContext.RequestServices.GetRequiredService(optionsFactoryType);
                var options = optionsFactoryType.GetMethod("Create").Invoke(optionsFactory, new[] { scheme.Name }) as RemoteAuthenticationOptions;

                if (options.CallbackPath == httpContext.Request.Path)
                {
                    // Skip if this is not a compatible type of authentication.
                    if (!(options is OAuthOptions || options is OpenIdConnectOptions))
                    {
                        continue;
                    }

                    try
                    {
                        string state = null;

                        if (string.Equals(httpContext.Request.Method, "GET", StringComparison.OrdinalIgnoreCase))
                        {
                            state = httpContext.Request.Query["state"];
                        }
                        else if (string.Equals(httpContext.Request.Method, "POST", StringComparison.OrdinalIgnoreCase)
                            && !string.IsNullOrEmpty(httpContext.Request.ContentType)
                            && httpContext.Request.ContentType.StartsWith("application/x-www-form-urlencoded", StringComparison.OrdinalIgnoreCase)
                            && httpContext.Request.Body.CanRead)
                        {
                            var formOptions = new FormOptions { BufferBody = true };
                            var form = httpContext.Request.ReadFormAsync(formOptions).Result;
                            state = form.Where(i => i.Key.ToLowerInvariant() == "state").Single().Value;
                        }

                        var oAuthOptions = options as OAuthOptions;
                        var openIdConnectOptions = options as OpenIdConnectOptions;

                        var properties = oAuthOptions?.StateDataFormat.Unprotect(state) ??
                                     openIdConnectOptions?.StateDataFormat.Unprotect(state);

                        if (properties.Items.Keys.Contains("tenantIdentifier"))
                        {
                            return properties.Items["tenantIdentifier"] as string;
                        }
                    }
                    catch (Exception e)
                    {
                        throw new MultiTenantException("Error occurred resolving tenant for remote authentication.", e);
                    }
                }
            }

            return null;
        }
    }
}