using System.ComponentModel.DataAnnotations;

namespace IdentitySample.Data;

public record Client
{
    [Key]
    public Guid Id { get; set; }
    public string Name { get; set; }
}