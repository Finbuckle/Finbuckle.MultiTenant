using Finbuckle.MultiTenant.Abstractions;

namespace WebApiSample;

public record AppTenantInfo(string Id, string Identifier, string Name, string PreferredLanguage) : TenantInfo(Id, Identifier, Name)
{
}