// Copyright Finbuckle LLC, Andrew White, and Contributors.
// Refer to the solution LICENSE file for more information.

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;

namespace Finbuckle.MultiTenant.Identity.EntityFrameworkCore.Test;

/// <summary>
/// Dynamic model cache key factory for tests to force model rebuilds when options vary.
/// Returns a new object instance for each call to bypass EF Core model caching.
/// </summary>
public class DynamicModelCacheKeyFactory : IModelCacheKeyFactory
{
    public object Create(DbContext context) => new object();
    public object Create(DbContext context, bool designTime) => new object();
}

