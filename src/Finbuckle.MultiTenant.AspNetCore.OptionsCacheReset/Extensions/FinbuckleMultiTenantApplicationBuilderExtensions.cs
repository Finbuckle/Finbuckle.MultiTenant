using Finbuckle.MultiTenant.AspNetCore.OptionsCacheReset.Internal;
using Microsoft.AspNetCore.Builder;

namespace Finbuckle.MultiTenant.AspNetCore.OptionsCacheReset
{
    /// <summary>
    /// Extension methods for using Finbuckle.MultiTenant.AspNetCore.OptionsCacheReset.
    /// </summary>
    public static class FinbuckleMultiTenantApplicationBuilderExtensions
    {
        /// <summary>
        /// Use Finbuckle.MultiTenantOptionsResetManager middleware in processing the request.
        /// </summary>
        /// <param name="builder">The IApplicationBuilder<c/> instance the extension method applies to.</param>
        /// <returns>The same IApplicationBuilder passed into the method.</returns>
        public static IApplicationBuilder UseMultiTenantOptionsResetManager<TVersionTenantInfo>(this IApplicationBuilder builder)
            where TVersionTenantInfo : class, IVersionTenantInfo, new()
            => builder.UseMiddleware<MultiTenantOptionManagerMiddleware<TVersionTenantInfo>>();
    }
}