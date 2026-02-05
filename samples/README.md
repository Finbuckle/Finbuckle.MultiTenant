# Samples

This folder contains simple examples of Finbuckle.MultiTenant and related packages in action.

## Available Samples

### [Web API Sample](WebApiSample/)

A minimal multi-tenant Web API demonstrating the base path strategy and in-memory tenant store. The weather forecast endpoint returns localized summaries based on each tenant's preferred language.

**Key Features:**
- Base path strategy for tenant identification
- In-memory tenant store
- Custom tenant properties
- Localized content per tenant

### [Identity Sample App](IdentitySampleApp/)

An ASP.NET Core MVC application with ASP.NET Core Identity integration showing per-tenant authentication and user management. Each tenant maintains isolated users in separate database contexts.

**Key Features:**
- Route strategy with tenant parameter
- Configuration store from appsettings.json
- Per-tenant authentication and Identity DbContext
- Multi-tenant Razor Pages
- Automatic database seeding