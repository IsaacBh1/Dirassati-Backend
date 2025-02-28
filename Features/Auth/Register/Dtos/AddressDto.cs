using System;

namespace Dirassati_Backend.Features.Auth.Register.Dtos;

public class AddressDto
{
    public required string Street { get; set; }
    public required string City { get; set; }
    public required string State { get; set; }  // Optional for some countries
    public required string PostalCode { get; set; }
    public required string Country { get; set; }
}
