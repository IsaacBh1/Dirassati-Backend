using System;

namespace Dirassati_Backend.Features.Auth.Register.Dtos;

public class EmployeeDto
{
    public required string FirstName { get; set; }
    public required string LastName { get; set; }
    public required string Email { get; set; }
    public required string PhoneNumber { get; set; }
    public required string Password { get; set; }
    public int Permission { get; set; }
}
