using Finbuckle.MultiTenant.Abstractions;

public class AppTenantInfo : ITenantInfo<string>
{
    public string Id { get; set; } = string.Empty;
    public string Identifier { get; set; } = string.Empty;
    public string? Name { get; set; } = string.Empty;
    public string Tier { get; set; } = "Default";
}