using Microsoft.AspNetCore.Http;
using Microsoft.Azure.WebJobs.Host.Bindings;
using Microsoft.Azure.WebJobs.Host.Protocols;

using System;
using System.Threading.Tasks;

namespace Finbuckle.MultiTenant.AzureFunctions.Bindings
{
    /// <summary>
    /// Runs on every request and passes the function context (e.g. Http request and host configuration) to a value provider.
    /// </summary>
    public class TenantBinding<TTenantInfo> : IBinding
        where TTenantInfo : class, ITenantInfo, new()
    {
        /// <inheritdoc />
        public bool FromAttribute { get { return false; } }

        /// <summary>
        /// Runs on every request and passes the function context (e.g. Http request and host configuration) to a value provider.
        /// </summary>
        public Task<IValueProvider> BindAsync(BindingContext context)
        {
            if (context is null)
            {
                throw new ArgumentNullException(nameof(context));
            }
            if (context.BindingData[TenantBindingProvider.RequestBindingName] is not HttpRequest request)
            {
                throw new NotSupportedException($"Argument {nameof(HttpRequest)} is null. {nameof(TenantAttribute)} must work with HttpTrigger.");
            }

            return BindAsync(request, context.ValueContext);
        }

        public Task<IValueProvider> BindAsync(object value, ValueBindingContext context)
        {
            if (value is HttpRequest request)
            {
                return Task.FromResult<IValueProvider>(new TenantValueProvider<TTenantInfo>(request));
            }
            throw new InvalidOperationException($"value must be an {nameof(HttpRequest)}");
        }

        public ParameterDescriptor ToParameterDescriptor()
        {
            return new ParameterDescriptor()
            {
                Name = "tenant",
            };
        }
    }
}