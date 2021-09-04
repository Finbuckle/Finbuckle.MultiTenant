namespace Finbuckle.MultiTenant.AspNetCore.OptionsCacheReset
{
    public interface IVersionTenantInfo : ITenantInfo
    {
        int Version { get; set; }
    }
}