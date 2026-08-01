namespace WebApiSample;

public abstract class SampleHelper
{
    /// <summary>
    /// Build the list of tenants for the in-memory store.
    /// </summary>
    public static List<AppTenantInfo> BuildTenantList() =>
    [
        new AppTenantInfo { Id = Guid.Parse("019f7174-49ef-7e9d-bc14-ed765ca3cce1"), Identifier = "acme", Name = "Acme Corporation", PreferredLanguage = "en" },
        new AppTenantInfo { Id = Guid.Parse("019f7174-49ef-7c04-a0f3-fd5612e8c683"), Identifier = "globex", Name = "Globex GmbH", PreferredLanguage = "de" },
        new AppTenantInfo { Id = Guid.Parse("019f7174-49ef-776f-b9d8-a9d5003117ca"), Identifier = "parisian", Name = "Parisian Foods", PreferredLanguage = "fr" }
    ];
}

