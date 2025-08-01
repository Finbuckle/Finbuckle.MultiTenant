using Finbuckle.MultiTenant.Abstractions;

public class AppTenantInfo : ITenantInfo
{
    public string? Id { get; set; }
    public string? Identifier { get; set; }
    public string? Name { get; set; }
    public string? PreferredLanguage { get; set; }
}