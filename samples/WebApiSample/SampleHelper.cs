namespace WebApiSample;

public abstract class SampleHelper
{
    /// <summary>
    /// Build the list of tenants for the in-memory store.
    /// </summary>
    public static List<AppTenantInfo> BuildTenantList() =>
    [
        new AppTenantInfo(Id: "tenant-001", Identifier: "acme", Name: "Acme Corporation", PreferredLanguage: "en"),
        new AppTenantInfo(Id: "tenant-002", Identifier: "globex", Name: "Globex GmbH", PreferredLanguage: "de"),
        new AppTenantInfo(Id: "tenant-003", Identifier: "parisian", Name: "Parisian Foods", PreferredLanguage: "fr")
    ];
}

