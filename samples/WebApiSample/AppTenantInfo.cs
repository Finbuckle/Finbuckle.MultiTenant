using Finbuckle.MultiTenant.Abstractions;

namespace WebApiSample;

public class AppTenantInfo : ITenantInfo
{
	public string Id { get; set; } = string.Empty;
	public string Identifier { get; set; } = string.Empty;
	public string? Name { get; set; } = string.Empty;
	public string PreferredLanguage { get; set; } = "en";
}