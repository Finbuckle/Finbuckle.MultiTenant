//    Copyright 2018-2020 Finbuckle LLC, Andrew White, and Contributors
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
using Finbuckle.MultiTenant.Internal;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.OAuth;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Finbuckle.MultiTenant.Strategies
{
    public class RemoteAuthenticationCallbackStrategy : IMultiTenantStrategy
    {
        private readonly ILogger<RemoteAuthenticationCallbackStrategy> logger;
        
        public int Priority { get => -900; }

        public RemoteAuthenticationCallbackStrategy(ILogger<RemoteAuthenticationCallbackStrategy> logger)
        {
            this.logger = logger;
        }

        public async virtual Task<string> GetIdentifierAsync(object context)
        {
            if(!(context is HttpContext))
                throw new MultiTenantException(null,
                    new ArgumentException($"\"{nameof(context)}\" type must be of type HttpContext", nameof(context)));

            var httpContext = context as HttpContext;

            var schemes = httpContext.RequestServices.GetRequiredService<IAuthenticationSchemeProvider>();

            foreach (var scheme in (await schemes.GetRequestHandlerSchemesAsync()).
                Where(s => typeof(IAuthenticationRequestHandler).IsAssignableFrom(s.HandlerType)))
                // Where(s => s.HandlerType.ImplementsOrInheritsUnboundGeneric(typeof(RemoteAuthenticationHandler<>))))
            {
                // Unfortnately we can't rely on the ShouldHandleAsync method since OpenId Connect handler doesn't use it.
                // Instead we'll get the paths to check from the options.
                var optionsType = scheme.HandlerType.GetProperty("Options").PropertyType;
                var optionsMonitorType = typeof(IOptionsMonitor<>).MakeGenericType(optionsType);
                var optionsMonitor = httpContext.RequestServices.GetRequiredService(optionsMonitorType);
                var options = optionsMonitorType.GetMethod("Get").Invoke(optionsMonitor, new[] { scheme.Name }) as RemoteAuthenticationOptions;

                var callbackPath = (PathString)(optionsType.GetProperty("CallbackPath")?.GetValue(options) ?? PathString.Empty);
                var signedOutCallbackPath = (PathString)(optionsType.GetProperty("SignedOutCallbackPath")?.GetValue(options) ?? PathString.Empty);

                if (callbackPath.HasValue && callbackPath == httpContext.Request.Path ||
                    signedOutCallbackPath.HasValue && signedOutCallbackPath == httpContext.Request.Path)
                {
                    try
                    {
                        string state = null;

                        if (string.Equals(httpContext.Request.Method, "GET", StringComparison.OrdinalIgnoreCase))
                        {
                            state = httpContext.Request.Query["state"];
                        }
                        // Assumption: it is safe to read the form, limit to 1MB form size.
                        else if (string.Equals(httpContext.Request.Method, "POST", StringComparison.OrdinalIgnoreCase)
                            && httpContext.Request.HasFormContentType
                            && httpContext.Request.Body.CanRead)
                        {
                            var formOptions = new FormOptions { BufferBody = true, MemoryBufferThreshold = 1048576 };
                            
                            var form = await httpContext.Request.ReadFormAsync(formOptions);
                            state = form.Where(i => i.Key.ToLowerInvariant() == "state").Single().Value;
                        }

                        var properties = ((dynamic)options).StateDataFormat.Unprotect(state) as AuthenticationProperties;

                        if (properties == null)
                        {
                            if(logger != null)
                                logger.LogWarning("A tenant could not be determined because no state paraameter passed with the remote authentication callback.");
                            return null;
                        }

                        properties.Items.TryGetValue(Constants.TenantToken, out var identifier);

                        return identifier;
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