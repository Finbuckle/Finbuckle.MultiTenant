using Finbuckle.MultiTenant.AzureFunctions.Bindings;

using Microsoft.AspNetCore.Http;
using Microsoft.Azure.WebJobs.Host.Bindings;

using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Finbuckle.MultiTenant.AzureFunctions
{
    /// <summary>
    /// Provides a new binding instance for the function host.
    /// </summary>
    public class TenantBindingProvider : IBindingProvider
    {
        // Name of binding data slot where we place the full HttpRequestMessage
        internal const string RequestBindingName = "$request";
        private static readonly Task<IBinding> NullBinding = Task.FromResult<IBinding>(null);

        public Task<IBinding> TryCreateAsync(BindingProviderContext context)
        {
            if(context is null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            var parameter = context.Parameter;

            if (!HasBindingAttributes(parameter))
            {
                Type genericType = typeof(TenantBinding<>).MakeGenericType(parameter.ParameterType);
                return Task.FromResult((IBinding)Activator.CreateInstance(genericType));
            }

            return NullBinding;
        }

        private static bool HasBindingAttributes(ParameterInfo parameter)
        {
            foreach (Attribute attr in parameter.GetCustomAttributes(false))
            {
                if (IsBindingAttribute(attr) && parameter.ParameterType is ITenantInfo)
                {
                    return true;
                }
            }
            return false;
        }

        private static bool IsBindingAttribute(Attribute attribute)
        {
            return attribute.GetType().GetCustomAttribute<TenantAttribute>() != null;
        }
    }
}
