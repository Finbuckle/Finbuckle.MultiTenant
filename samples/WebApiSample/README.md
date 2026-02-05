# Web API Sample

This sample demonstrates a simple multi-tenant Web API built with Finbuckle.MultiTenant and ASP.NET Core.

## Overview

The sample extends the default `dotnet new webapi` template to support multi-tenancy. The weather forecast endpoint returns localized summaries based on each tenant's preferred language.

## Features

- **Base Path Strategy**: Tenants are identified by the first segment of the URL path (e.g., `/acme/weatherforecast`)
- **In-Memory Store**: Tenant configuration is stored in memory for simplicity
- **Custom Tenant Info**: Extends `TenantInfo` with a `PreferredLanguage` property
- **Path Rebasing**: The `BasePathStrategy` automatically adjusts ASP.NET Core's path base, so endpoint routes don't need to include the tenant identifier

## Configuration

Three example tenants are defined in `SampleHelper.BuildTenantList()`:

- **acme**: English language
- **parisian**: French language  
- **globex**: German language

## Usage

Run the application and navigate to:

- `https://localhost:<port>/acme/weatherforecast` - Returns English summaries
- `https://localhost:<port>/parisian/weatherforecast` - Returns French summaries
- `https://localhost:<port>/globex/weatherforecast` - Returns German summaries
