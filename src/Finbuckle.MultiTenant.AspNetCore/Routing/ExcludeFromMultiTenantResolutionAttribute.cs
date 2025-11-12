using Microsoft.AspNetCore.Http;

namespace Finbuckle.MultiTenant.AspNetCore.Routing;

/// <summary>
/// Indicates that this <see cref="Endpoint"/> should be excluded from MultiTenant resolution.
/// </summary>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method | AttributeTargets.Delegate)]
public class ExcludeFromMultiTenantResolutionAttribute : Attribute, IExcludeFromMultiTenantResolutionMetadata
{
    /// <inheritdoc />
    public bool ExcludeFromResolution => true;
}