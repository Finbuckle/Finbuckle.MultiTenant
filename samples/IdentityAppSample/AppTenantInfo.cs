using Finbuckle.MultiTenant.Abstractions;

public record AppTenantInfo(string Id, string Identifier, string Name) : TenantInfo(Id, Identifier, Name)
{
    public string Tier { get; set; } = "Default";
}