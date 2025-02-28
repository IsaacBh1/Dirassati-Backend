using System;

namespace Dirassati_Backend.Features.Auth.Register.Dtos;

public class SchoolDto
{
    public string SchoolName { get; set; } = string.Empty;
    public string SchoolType { get; set; } = string.Empty;
    public string SchoolEmail { get; set; } = string.Empty;
    public string PhoneNumber { get; set; } = string.Empty;
    public AddressDto Address { get; set; } = null!;
}
