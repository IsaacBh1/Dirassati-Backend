using System.ComponentModel.DataAnnotations;

namespace Dirassati_Backend.Features.Auth.Register.Dtos;

/// <summary>
/// Data transfer object for address information
/// </summary>
public class AddressDto
{
    /// <summary>
    /// Street address including house/building number
    /// </summary>
    /// <example>123 Main Street</example>
    [Required]
    public required string Street { get; set; }

    /// <summary>
    /// City or town name
    /// </summary>
    /// <example>New York</example>
    [Required]
    public required string City { get; set; }

    /// <summary>
    /// State, province, or region
    /// </summary>
    /// <example>NY</example>
    [Required]
    public required string State { get; set; }

    /// <summary>
    /// Postal code or ZIP code
    /// </summary>
    /// <example>10001</example>
    [Required]
    public required string PostalCode { get; set; }

    /// <summary>
    /// Country name
    /// </summary>
    /// <example>United States</example>
    [Required]
    public required string Country { get; set; }
}