# Identity App Sample

This sample demonstrates a multi-tenant ASP.NET Core MVC application with ASP.NET Core Identity integration using Finbuckle.MultiTenant.

## Overview

The sample extends the default ASP.NET Core MVC template with Identity to support multi-tenancy. Each tenant has its own isolated set of users in separate database contexts, demonstrating how to implement per-tenant authentication and user management.

## Features

- **Route Strategy**: Tenants are identified by the `__tenant__` route parameter (e.g., `/acme/Home/Index`)
- **Configuration Store**: Tenant configuration is loaded from `appsettings.json`
- **Per-Tenant Authentication**: Each tenant maintains isolated user authentication with separate database contexts
- **Custom Tenant Info**: Extends `TenantInfo` with an `Tier` property
- **Multi-Tenant Identity DbContext**: Uses Entity Framework Core with per-tenant database isolation
- **Razor Pages Multi-Tenancy**: Identity UI pages automatically include tenant routing via `MultiTenantPageRouteModelConvention`
- **Database Seeding**: Sample users are automatically created for each tenant on startup

## Configuration

Two example tenants are defined in `appsettings.json`:

- **acme**: "Acme Inc." with VIP Customer status
  - Users: `harry@acme.com`, `larry@acme.com`
- **initech**: "Initech LLC" with Standard Customer status
  - Users: `alice@initech.com`, `bob@initech.com`

All seeded users have the password: `P@ssword1`

## Database

The application uses SQLite with a shared database file (`app_identity.db`). Each tenant's data is isolated within separate Entity Framework Core contexts using the `MultiTenantDbContext` pattern.

## Usage

Run the application and navigate to:

- `https://localhost:<port>/`

You'll see a selector in the upper right corner to choose a tenant (Acme Inc. or Initech LLC). After selecting a 
tenant, you can register or log in with the seeded users for that tenant. Notice that after logging in to a tenant, 
if you switch to another tenant you will not be logged in, demonstrating the per-tenant user authentication.