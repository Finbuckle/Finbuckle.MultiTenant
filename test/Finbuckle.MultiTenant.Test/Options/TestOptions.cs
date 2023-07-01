// Copyright Finbuckle LLC, Andrew White, and Contributors.
// Refer to the solution LICENSE file for more information.

using System.ComponentModel.DataAnnotations;

namespace Finbuckle.MultiTenant.Test.Options
{
    internal class TestOptions
    {
        [Required]
        public string? DefaultConnectionString { get; set; }
    }
}