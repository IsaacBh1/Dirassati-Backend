namespace Dirassati_Backend.Common.Dtos;

using System.ComponentModel.DataAnnotations;

public class AddressDto
{
    [Required]
    [MaxLength(100)]
    public string Street { get; set; } = null!;

    [Required]
    [MaxLength(50)]
    public string City { get; set; } = null!;

    [Required]
    [MaxLength(50)]
    public string State { get; set; } = null!;

    [Required]
    [MaxLength(20)]
    public string PostalCode { get; set; } = null!;

    [Required]
    [MaxLength(50)]
    public string Country { get; set; } = null!;
}
