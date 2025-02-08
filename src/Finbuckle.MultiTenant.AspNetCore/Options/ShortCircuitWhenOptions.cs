using Finbuckle.MultiTenant.Abstractions;

namespace Finbuckle.MultiTenant.AspNetCore.Options;

public class ShortCircuitWhenOptions
{
    private Func<IMultiTenantContext, bool>? _predicate;

    /// <summary>
    /// The callback that determines if the endpoint should be short circuited.
    /// </summary>
    public Func<IMultiTenantContext, bool>? Predicate
    {
        get
        {
            return _predicate;
        }
        set
        {
            ArgumentNullException.ThrowIfNull(value);

            _predicate = value;
        }
    }
}