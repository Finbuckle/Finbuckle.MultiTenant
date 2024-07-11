using System;
using Microsoft.AspNetCore.Identity;

namespace Finbuckle.MultiTenant.EntityFrameworkCore;

public class MultiTenantIdentityUserRole<TKey>
    : IdentityUserRole<TKey>
    where TKey : IEquatable<TKey>
{
    public string? TenantId { get; set; }
}