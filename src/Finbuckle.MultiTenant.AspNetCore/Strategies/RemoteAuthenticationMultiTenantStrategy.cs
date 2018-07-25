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
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.OAuth;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Finbuckle.MultiTenant.Strategies
{
    public interface IRemoteAuthenticationMultiTenantStrategy
    {
    }

    public class RemoteAuthenticationMultiTenantStrategy : IMultiTenantStrategy, IRemoteAuthenticationMultiTenantStrategy
    {
        public async virtual Task<string> GetIdentifierAsync(object context)
        {
            if(!(context is HttpContext))
                throw new MultiTenantException(null,
                    new ArgumentException("\"context\" type must be of type HttpContext", nameof(context)));

            var httpContext = context as HttpContext;

            var schemes = httpContext.RequestServices.GetRequiredService<IAuthenticationSchemeProvider>();
            var handlers = httpContext.RequestServices.GetRequiredService<IAuthenticationHandlerProvider>();

            foreach (var scheme in schemes.GetRequestHandlerSchemesAsync().Result)
            {
                var optionType = scheme.HandlerType.GetProperty("Options").PropertyType;
                
                // Skip if this is not a compatible type of authentication.
                if (!typeof(OAuthOptions).IsAssignableFrom(optionType) &&
                    !typeof(OpenIdConnectOptions).IsAssignableFrom(optionType))
                {
                    continue;
                }

                // RequestHandlers have a method, ShouldHandleRequestAsync, which would be nice here,
                // but instantiating the handler internally caches an Options instance... which is bad because
                // we don't know the tenant yet. Thus we will get the Options ourselves with reflection,
                // and replicate the ShouldHandleRequestAsync logic.

                var optionsMonitorType = typeof(IOptionsMonitor<>).MakeGenericType(optionType);
                var optionsMonitor = httpContext.RequestServices.GetRequiredService(optionsMonitorType);
                var options = optionsMonitorType.GetMethod("Get").Invoke(optionsMonitor, new[] { scheme.Name }) as RemoteAuthenticationOptions;
                
                if (options.CallbackPath == httpContext.Request.Path)
                {
                    try
                    {
                        string state = null;

                        if (string.Equals(httpContext.Request.Method, "GET", StringComparison.OrdinalIgnoreCase))
                        {
                            state = httpContext.Request.Query["state"];
                        }
                        else if (string.Equals(httpContext.Request.Method, "POST", StringComparison.OrdinalIgnoreCase)
                            && httpContext.Request.HasFormContentType
                            && httpContext.Request.Body.CanRead)
                        {
                            var formOptions = new FormOptions { BufferBody = true };
                            var form = await httpContext.Request.ReadFormAsync(formOptions).ConfigureAwait(false);
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