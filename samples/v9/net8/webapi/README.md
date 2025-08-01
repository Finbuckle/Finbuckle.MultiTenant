# webapi Sample

This project demonstrates a simple multi-tenant webapi based on the default `dotnet new webapi` command in `.NET 8`.

The default weather endpoint is modified to provide the summary in either English, French, or German based on the 
tenant's preferred language.

Finbuckle.MultiTenant is configured to use the route strategy and an in-memory tenant store. A custom `AppTenantInfo` 
implements `ITenantInfo` and adds the `PreferredLanguage` property. Three example tenants are defined in the 
`BuildTenantList` method.