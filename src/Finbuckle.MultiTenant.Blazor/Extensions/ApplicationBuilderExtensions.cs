using Finbuckle.MultiTenant.Blazor;

namespace Microsoft.AspNetCore.Builder
{
    /// <summary>
    /// Extension methods for using Finbuckle.MultiTenant.AspNetCore.
    /// </summary>
    public static class FinbuckleMultiTenantApplicationBuilderExtensions
    {
        /// <summary>
        /// Use Finbuckle.MultiTenant middleware in processing the request.
        /// </summary>
        /// <param name="builder">The IApplicationBuilder<c/> instance the extension method applies to.</param>
        /// <returns>The same IApplicationBuilder passed into the method.</returns>
        public static IApplicationBuilder UseBlazorMultiTenant(this IApplicationBuilder builder)
            => builder.UseMiddleware<BlazorMultiTenantMiddleware>();
        
    }
}