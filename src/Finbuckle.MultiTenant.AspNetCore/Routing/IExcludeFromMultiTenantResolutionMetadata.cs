using Microsoft.AspNetCore.Http;

namespace Finbuckle.MultiTenant.AspNetCore.Routing;

/// <summary>
/// Indicates whether MultiTenant resolution should occur for this <see cref="Endpoint"/>.
/// </summary>
public interface IExcludeFromMultiTenantResolutionMetadata
{
    /// <summary>
    /// Gets a value indicating whether MultiTenant resolution should 
    /// occur for this <see cref="Endpoint"/>. If <see langword="true"/>,
    /// tenant resolution will not be executed.
    /// </summary>
    bool ExcludeFromResolution { get; }
}